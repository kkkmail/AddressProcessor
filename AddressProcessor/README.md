# F# Address Processor Workflows & Main Logic

*Updated 2018-05-04*

This document describes internal logic and main interfaces of F# Address Process (AP) Service.

## AddressProcessor Internal Logic and Operation

### Problem Setup
The problem, which AP was designed to solve, is substantially different from the one, which `USAddress.AddressParser` (UAP) is solving. UAP utilizes pattern matching to split a single address into parts: full street name (including street number), city, state, zip. So, if a collection of addresses or address with some extra and/or erroneous and/or missing information is given as an input, UAP often performs incorrect splits. All subsequent calls to obtain the address key based on such input usually fail or produce something like a center of the zip address key. 

On the other side AP was designed to quickly handle bad addresses and/or collections of addresses as input. In particular, a collection of addresses is assumed as not several valid addresses separated by some token (like space, comma, semicolon) but, rather, a string where the common part of address is not repeated, for example: "660-680 N 9 ST & GARAGE BLYTHE CA 92225", which has two addresses: "660 N 9 ST BLYTHE CA 92225", "680 N 9 ST BLYTHE CA 92225" and some extra words, which should be ignored. In addition, the users might miss some parts of the address, mistype and/or use various abbreviations, etc... There are over 200 known USPS street type abbreviations, some of which might have up to 3 "flavors". For example, "ST GEORGE STREET" might be inputted by user as "ST GEORGE STREET", "ST GEORGE STR", "ST GEORGE", or even "GEORGE ST", etc...
To address such a problem, AP uses full Melissa Data (MD) address table (table `MelissaDataAll`, which is located in `SwyfftAddress` database) preprocessed and partitioned as necessary.

### Data Preparation
To address the issue of different street types and directions, AP performs data cleaning and standardization as follows.

- Remove all clear garbage rows, which match SQL garbage pattern, currently: `let GarbagePattern = @"%[^a-zA-Z0-9 - /]%"`
- Perform standardization by replacing all flavors of street types and directions with their standard abbreviations. This includes taking care of known MD glitches, like "eating" the last character of street type, for example: "HIGHWA" instead of "HIGHWAY".
- Strips the apartment number.
- Applies some general rules to check if the address is valid.
- And, finally stores the result in a cleaned address table: `EFAddresses`. This table contains about 95M rows.

This step is performed by Melissa Data Parser. See `IMelissaDataParserService` below. 


### Data Partitioning
Once the data is cleaned a partitioning is performed by removing house number column and storing obtained `StreetFullName`, `City`, `State`, and `ZipCode` in the table `EFStreetZips`. Further partitioning is performed by storing aggregate information in the tables: `EFZipCodeCities` (a map from state to all cities in each zip code) and `EFStateCities` (a map from zip code to all cities in each state). Zipped CSV `StreetZip.zip` contains the data for `EFStreetZips` and further partitioning is performed on the fly during seeding. The CSV for the file `StreetZip.zip` can be extracted from fully populated `EFAddresses` using a trivial DTS in SSMS.

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

Slightly above main AP engine there sits an address key resolution call. It takes the list of all resolved addresses, tries to get address key for all of them and returns if any is found. The first found key (if any) is returned. Table `EFAddresses` is used to get the address key.


## Components

### IAddressProcessorService
This interface has several methods. The most important one is:


`bool TryGetAddressKey(string address, out string addressKey, string expectedAddressKey = null);`

This method attempts to find an `addressKey` for a given `address`. The caller can also pass non-null `expectedAddressKey` for AP to compare. The parser logs all errors, including non-matching `addressKey` and `expectedAddressKey` (if it is not null) into the table `EFAddressErrors`.


### IMelissaDataParserService
The interface encapsulates the call to `ProcessMelissaData`. The call is used by Swyfft Console task ` ProcessMelissaDataAll`.

## Jobs and Workflows

### Seeding Primary AP Tables
The following tables are used by AP during processing of an input string:

`EFStreetZips` - contains all standardized street names (without house number and unit number) in all zip / city / state combinations. This table can be "obtained" from `EFAddresses` using the following SQL statement and `SwyfftRating` database:

```
set nocount on
truncate table EFStreetZips
go

set nocount on
;with aggrTbl as
(
	select distinct top 100000000
	StreetFullName
	,City
	,State
	,ZipCode
	, count (*) as OccurrenceCount
	from EFAddresses
	group by StreetFullName, City, State, ZipCode
)
INSERT INTO EFStreetZips
(
	StreetFullName
	,City
	,State
	,ZipCode
	,OccurrenceCount
)
select 
	StreetFullName
	,City
	,State
	,ZipCode
	,OccurrenceCount
from aggrTbl
```



`EFStateCities` - contains all US state / city pair combinations. It can be repopulated using `select distinct State, City from EFStreetZips`.

`EFStateCityWordCorrections` - contains all known word corrections for state / city pairs. This table is manually populated.

`EFZipCodeCities` - contains all US zip code / city pair combinations. It can be repopulated using `select distinct ZipCode, City from EFStreetZips`.

`EFZipCodeWordCorrections` - contains all known word corrections for zip codes. This table is manually populated.


Seeding is performed as a part of `SeedRatingCommon`. 

### SeedAddress
Seeds `EFAddresses` table. This table has about 95M rows and it needs about 30GB of free space to be created. Once the table is seeded it will be indexed. If you have a spinner, then all that will take a while. This seeding must be performed at least once and then when the underlying data is changed. Currently there is no known schedule for that. Additional workflow could be developed when we add some addresses by hands. The address seeding workflow must be revisited if / when that happens.

`Swyfft.Console.exe -t:SeedAddress`

### SeedPropertyDataAddress 
Seeds `EFAddresses` table using address keys extracted from the file `PropertyData.zip`. This seeder merges the data in contrast with `SeedAddress`, which always overwrites the data with about 95M rows. 

`SeedPropertyDataAddress` task currently uses "hard coded" value of `Data\PropertyData.zip` as a source. The only purpose of that job is to populate address related data for tests if the whole address table was not loaded (due to space constraints, for example). So, currently, there is no need to specify another file. However, shall the need arise, we can easily add a parameter to the task and pass a file name. 

To run the job call:

`Swyfft.Console.exe -t:SeedPropertyDataAddress`

### RebuildAddressIndices
Rebuilds indices on `EFAddresses` table. Rebuilding indices is slow. So if there is a concern that the indices are not there, then perform a validation first. Indices should be removed when repopulating the table. This substantially improves performance. Occasiaonally indices may become corrupted and they must be rebuilt. Use this job in all such cases.

To run the job call:

`Swyfft.Console.exe -t:RebuildAddressIndices`

### ValidateAddressIndices
Validates indices on `EFAddresses` table. Validation is quick and if the indices are there, then we do not want to requild them without the need. If indices are not validated then it perfomrs:

`
throw new InvalidOperationException($"{name}: Cannot validate index {ZipCodeIndexName} on table {AddressTbl}.");
`

To run the job call:

`Swyfft.Console.exe -t:ValidateAddressIndices`

### ProcessMelissaDataAll
Runs Melissa Data Parser. This parser is an alternative way to populate `EFAddresses` table. The table is cleared at the beginning of this task.

To use local `SwyfftAddress` DB as input use:

`Swyfft.Console.exe -t:ProcessMelissaDataAll -lmd:true`

To use Azure based `SwyfftAddress` DB as input use:

`Swyfft.Console.exe -t:ProcessMelissaDataAll -lmd:false`

To override default batch size of `500,000` add `-bs:NewBatchSize`. To override default parallel processing add `-par:false`. For example: use Azure based `SwyfftAddress` DB as input, process in `100,000` size batches, and do not run in parallel:

`Swyfft.Console.exe -t:ProcessMelissaDataAll -lmd:false -bs:100000 -par:false`

