# F# Address Processor Workflows & Main Logic

*Updated 204-11-26*

This document describes internal logic and main interfaces of F# Address Process (AP) Service.

## AddressProcessor Internal Logic and Operation

### Problem Setup
The problem, which AP was designed to solve, is substantially different from the one, which `USAddress.AddressParser` (UAP) is solving. UAP utilizes pattern matching to split a single address into parts: full street name (including street number), city, state, zip. So, if a collection of addresses or address with some extra and/or erroneous and/or missing information is given as an input, UAP often performs incorrect splits. 

On the other side AP was designed to quickly handle bad addresses and/or collections of addresses as input. In particular, a collection of addresses is assumed as not several valid addresses separated by some token (like space, comma, semicolon) but, rather, a string where the common part of address is not repeated, for example: "660-680 N 9 ST & GARAGE BLYTHE CA 92225", which has two addresses: "660 N 9 ST BLYTHE CA 92225", "680 N 9 ST BLYTHE CA 92225" and some extra words, which should be ignored. In addition, the users might miss some parts of the address, mistype and/or use various abbreviations, etc... There are over 200 known USPS street type abbreviations, some of which might have up to 3 "flavors". For example, "ST GEORGE STREET" might be inputted by user as "ST GEORGE STREET", "ST GEORGE STR", "ST GEORGE", or even "GEORGE ST", etc...
To address such a problem, AP uses full address table preprocessed and partitioned as necessary.

### Data Preparation
To address the issue of different street types and directions, AP performs data cleaning and standardization as follows.

- Remove all clear garbage rows, which match SQL garbage pattern, currently: `let GarbagePattern = @"%[^a-zA-Z0-9 - /]%"`
- Perform standardization by replacing all flavors of street types and directions with their standard abbreviations. This includes taking care of known glitches, like "eating" the last character of street type, for example: "HIGHWA" instead of "HIGHWAY".
- Strips the apartment number.
- Applies some general rules to check if the address is valid.
- And, finally stores the result in a cleaned address table. This table may contain over 100M rows.

### Data Partitioning
Once the data is cleaned a partitioning is performed by removing house number column and storing obtained `StreetFullName`, `City`, `State`, and `ZipCode` in the table `StreetZips`. Further partitioning is performed by storing aggregate information in the tables: `ZipCodeCities` (a map from state to all cities in each zip code) and `StateCities` (a map from zip code to all cities in each state).

### Simple Description of AP operation
AP parses the address string backwards. Note that "-" is not a token for AP and all parts separated by "-" are glued together to form a single word. AP deals with "-" internally because the logic is very different depending on the location of hyphen. 

AP uses two standardized circular rule collections (`RuleInfo`). Each rule collection contains the following rules (and some supporting data, which is not described here):

- `zipRule : Rule` - attempts to extract the zip from the last word of the string. 
- `stateRule : Rule` - attempts to extract state.
- `cityRule : Rule` - attempts to extract city.
- `streetRule : Rule` - attempts to extract street.
- `numberRule : bool -> Rule` - attempts to extract house number
- `newAddress : Rule` - applies new address and restarts processing.

If any of the rules succeeds, then it removes the word(s) that it processed and passes the remaining string further. newAddress rule creates address and restarts processing if there are any words left.

The rules utilize look ahead and look backward checks. It is now easier to explain how AP works using an example: "660-680 N 9 ST & GARAGE BLYTHE CA 92225":

- Processing starts. At this point there are no already resolved addresses.
- Input string is standardized. That removes extra symbols, like "&", "glues" parts separated by "-", so that, for example "660 - 680" becomes "660-680", replaces all multiple spaces by single spaces, and finally replaces all matching known flavors of street types and direction by their standard representations.
- zipRule is applied and zip code "92225" is extracted and validated using the data.
- a map data for resolved zip is dynamically loaded:
```
            zipUpdater : AsyncUpdater<ZipCode, ZipMap>
            stateCityUpdater : AsyncUpdater<State * City, StateCityMap>
            zipToCityUpdater : AsyncUpdater<ZipCode, ZipToCityMap>
            stateToCityUpdater : AsyncUpdater<State, StateToCityMap>
            wordMap : Map<ZipCode, Map<string, string>>
```
Full description of these maps is beyond the scope of this document, so only the primary map is described in details: ` zipUpdater : AsyncUpdater<ZipCode, ZipMap>`. `AsyncUpdater` dynamically loads a part of map, called `ZipMap` for a given zip code. This map is internally a type abbreviation:
`type ZipMap = Map<ZipCode, Map<list<string>, list<StreetCityState>>>`.

So, it is a map from `ZipCode` (key is a zip code) to a map of `list<string>` (key is a list of all possible sorted valid word combinations in a street name) to a `list<StreetCityState>>` (list of Street, City, State triples) of all `StreetCityState`s, where any valid sorted sublist from the list of words from which full street name consists, matches the key. A set of valid sublists is obtained from the full set of all sublist of words, from which the full street name consists, by applying certain weighting function. See `toValidSubLists` for details. The main idea is that if we have, let's say "Massachusetts Ave" then we want to match it with input "Massachusetts Ave", "Massachusetts", but not with "Ave". Perfect matches are always returned and partial matches are sorted by some rank to return the best match.

- City is resolved and matched using `ZipMap`.
- Street name is resolved and matched using `ZipMap`.
- At this point "660-680" is not yet processed and "GARAGE" was discarded.
- Number rule kicks in, strips "680" out of "660-680" and produces a house number.
- newAddress rule extracts full address, adds it to the list of resolved addresses, and restarts processing.
- zipRule now tries to match the zip code but there is only "660" left, which could've been "00660" zip code (AP assumes that front zeros could be eaten)! So, the rule performs look ahead, finds out that future rules (city, street, number) will fail, and then takes the zip from the last resolved address ("92225"), then returns that the zip code was inferred rather than resolved.
- state, city, and street rules proceed in a similar way, infer their data from last resolved address and advance to number rule.
- number rule consumes "660" and produces a house number.
- finally, newAddress rule adds address to the list of resolved addresses.
- Since there is nothing left, the processing ends.
