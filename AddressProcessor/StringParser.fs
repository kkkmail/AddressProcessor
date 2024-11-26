namespace Softellect.AddressProcessor

open System
open System.Text.RegularExpressions
open System.Reflection
open Microsoft.FSharp.Reflection
open Configuration

module StringParser =

    [<Literal>]
    let EmptyString = ""


    [<Literal>]
    let Hyphen = "-"


    [<Literal>]
    let DefaultSplitCharacter = " "


    type List<'T>
        with

        /// Replaces a by b.
        /// If none found then returns the original list.
        /// If a is empty then nothing is replaced.
        static member replace comparer a b lst =
            let r = b |> List.rev

            let rec tryMatch acc rem matched x =
                match rem with
                | [] ->
                    match x with
                    | [] -> r @ acc
                    | _ -> matched @ acc
                    |> List.rev
                | h :: t ->
                    match x with
                    | [] -> tryMatch (r @ acc) rem [] a
                    | xh :: xt ->
                        match comparer h xh with
                        | true -> tryMatch acc t (h :: matched) xt
                        | false -> tryMatch (h :: matched @ acc) t [] a

            match a with
            | [] -> lst
            | _ -> tryMatch [] lst [] a


    let defaultComparer a b = a = b


    /// Super standard splitter.
    let defaultSplit (s : string) =
        s.Split([| DefaultSplitCharacter |], StringSplitOptions.RemoveEmptyEntries)
        |> List.ofArray


    /// Splitters for address parsing.
    /// Keeping "-" and "/" as parts of the string.
    let addressSeparators =
        [
            ","
            "."
            ";"
            "\""
            "'"
            "“"
            "”"
            "‘"
            "’"
            "["
            "]"
            "`"
            "@"
            "#"
            "$"
            "%"
            "^"
            "&"
            "*"
            "("
            ")"
            "_"
            "+"
            "="
            "<"
            ">"
            "\r"
            "\n"
            "\t"
        ]


    /// Words, which have a very low weight.
    /// Note that there are streets like "DAN AND MARY ST".
    let optionalWords =
        [
            "AND", 0.0
            "/", 0.0
            "-", 0.0

        ]
        |> Map.ofList


    /// Standard abbreviations or words, which should be replaced in ORIGINAL strings.
    let originalAbbr : Map<string, string> =
        [
            //"TWENTYNINE", "29"
        ]
        |> Map.ofList


    /// Standard abbreviations or words, which should be replaced in PROJECTED strings.
    let standardAbbr =
        [
            "SAINT", "ST"
            "PETERSBURG", "PETE"
            "TWENTYNINE", "29"
        ]
        |> Map.ofList


    /// Standard groups of words, which should be replaced by abbeviations in PROJECTED strings.
    let standardGroups =
        [
            "FARM TO MARKET ROAD", "FM"
            "FARM TO MARKET RD", "FM"
            "FARM TO MARKET", "FM"

            "COUNTY ROAD", "CR"
            "COUNTY RD", "CR"
        ]
        |> List.map (fun (a, b) -> (defaultSplit a, defaultSplit b))


    let replaceGroups g s = g |> List.fold (fun acc (a, b) -> acc |> List.replace defaultComparer a b ) s


    /// Parameter to control how to expand ranges like "10-20", "11-21"
    type ExpandMatchingParityRange =
        | DoNotExpandParity
        | ExpandByTwo
        | ExpandByOne

        static member defaultValue = ExpandByTwo


    /// Parameter to control how to expand ranges like "10-21" and "11-20".
    type ExpandNonMatchingParityRange =
        | DoNotExpand
        | ExpandAll

        static member defaultValue = DoNotExpand


    type ExpandHyphenParams =
        {
            maxHouseRange : int // Maximum number of house numbers allowed in expanding ranges like "1234-36"
            expandMatchingParityRange : ExpandMatchingParityRange
            expandNonMatchingParityRange : ExpandNonMatchingParityRange
        }

        static member defaultMaxHouseRange = 9000

        static member defaultValue =
            {
                maxHouseRange = ExpandHyphenParams.defaultMaxHouseRange
                expandMatchingParityRange = ExpandMatchingParityRange.defaultValue
                expandNonMatchingParityRange = ExpandNonMatchingParityRange.defaultValue
            }

        static member expandAllNonMatchingParityValue =
            {
                maxHouseRange = ExpandHyphenParams.defaultMaxHouseRange
                expandMatchingParityRange = ExpandMatchingParityRange.defaultValue
                expandNonMatchingParityRange = ExpandAll
            }

        static member doNotExpandValue =
            {
                maxHouseRange = ExpandHyphenParams.defaultMaxHouseRange
                expandMatchingParityRange = DoNotExpandParity
                expandNonMatchingParityRange = DoNotExpand
            }


    /// Describes what to do with a string with hyphens.
    /// All other strings are unaffected.
    type HyphenTreatmentType =
        | ExpandHyphen of ExpandHyphenParams // "123-4" -> "123 124", but "2-1" -> "2-1" and "A-1" -> "A-1"
        | RemoveHyphen // "1-F" -> "1F", "F-1" -> "F1", and "1-2" -> "12" !
        | RemoveUnitNumberHyphen // "1-F" -> "1F", "F-1" -> "F1", but "1-2" -> "1-2" and "A-B" -> "A-B" and "1-2A" -> "1-2A" and "A1-2" -> "A1-2"
        | ReplaceHyphenWithSpace // "1-F" -> "1 F", "1-2" -> "1 2"
        | DoNotChangeHyphen // Same as id: "1-F" -> "1-F"


    type HyphenTreatmentParams =
        {
            hyphenTreatmentType : HyphenTreatmentType
        }

        static member defaultValue =
            {
                hyphenTreatmentType = ExpandHyphen ExpandHyphenParams.defaultValue
            }

        static member unitNumberValue =
            {
                hyphenTreatmentType = RemoveUnitNumberHyphen
            }

        static member expandAllNonMatchingParityValue =
            {
                hyphenTreatmentType = ExpandHyphen ExpandHyphenParams.expandAllNonMatchingParityValue
            }

        static member doNotExpandValue =
            {
                hyphenTreatmentType = ExpandHyphen ExpandHyphenParams.doNotExpandValue
            }


    let getValueOrDefault d x =
        match x with
        | Some v -> v
        | None -> d


    /// Same as getValueOrDefault but takes a lazy func f() so that not to perform unneded eveluation if we have a value.
    let getValueOrDefault2 f x =
        match x with
        | Some v -> v
        | None -> f()


    /// Splits alpha-numerical strings at the boundary of letters and digits.
    /// Other characters are unafected.
    /// For example, "1A" -> "1 A", "1-A" -> "1-A".
    let splitAlphaNum (s : string) =
        let x = s.ToUpper().ToCharArray() |> List.ofArray

        let rec split p (r : list<char>) acc =
            match r with
            | [] -> acc
            | h :: t ->
                match p with
                | None -> h :: acc
                | Some c ->
                    match (Char.IsDigit h = Char.IsDigit c) || (Char.IsLetter h = Char.IsLetter c) with
                    | true -> h :: acc
                    | false -> h :: ' ' :: acc
                |> split (Some h) t

        let result = split None x [] |> List.rev |> Array.ofList
        let s1 = String result
        s1


    ///Gets the weight of the word when it is skipped.
    let getSkipWeight s = optionalWords.TryFind s |> getValueOrDefault 1.0


    let (|Int|_|) (str : string) =
       match System.Int32.TryParse(str) with
       | (true, x) -> Some (x)
       | _ -> None


    let (|Int64|_|) (str : string) =
       match System.Int64.TryParse(str) with
       | (true, x) -> Some (x)
       | _ -> None


    let (|Bool|_|) (str : string) =
       match System.Boolean.TryParse(str) with
       | (true, x) -> Some (x)
       | _ -> None


    let (|Float|_|) (str : string) =
       match System.Double.TryParse(str) with
       | (true, x) -> Some (x)
       | _ -> None


    /// https://stackoverflow.com/questions/13011811/generic-type-constraints-and-duck-typing/13012065
    let inline floatFromString<'T when 'T : (static member create : float -> 'T) and 'T : (static member defaultValue : 'T)> (s : string) : 'T =
        match s with
        | Float value -> (^T : (static member create : float -> 'T) (value))
        | _ -> (^T : (static member defaultValue : 'T) ())


    let inline intFromString<'T when 'T : (static member create : int -> 'T) and 'T : (static member defaultValue : 'T)> (s : string) : 'T =
        match s with
        | Int value -> (^T : (static member create : int -> 'T) (value))
        | _ -> (^T : (static member defaultValue : 'T) ())


    let inline int64FromString<'T when 'T : (static member create : int64 -> 'T) and 'T : (static member defaultValue : 'T)> (s : string) : 'T =
        match s with
        | Int64 value -> (^T : (static member create : int64 -> 'T) (value))
        | _ -> (^T : (static member defaultValue : 'T) ())


    /// https://stackoverflow.com/questions/36782413/arithmetic-casting-to-generic-type-in-f
    let convert<'T> (s : string) : 'T =
        match box Unchecked.defaultof<'T> with
        | :? int64 -> int64 s |> unbox<'T>
        | :? int -> int s |> unbox<'T>
        | :? bool -> float s |> unbox<'T>
        | :? float -> float s |> unbox<'T>
        | :? string -> string s |> unbox<'T>
        | _ -> failwith ("I give up for (box s) = " + (box s).ToString())


    let toValidWebAddress (address : string option) : string option =
        match address with
        | Some a ->
            if a.ToLower().StartsWith("http://") || a.ToLower().StartsWith("https://") then a |> Some
            else "http://" + a |> Some
        | None -> None


    /// https://stackoverflow.com/questions/2484919/how-do-i-split-a-string-by-strings-and-include-the-delimiters-using-net
    let tryGetRegexPattern (delimeters : list<string>) (input : string) =
        match delimeters with
        | [] -> None
        | h::t -> "(" + (t |> List.fold (fun acc e -> (Regex.Escape e) + "|" + acc) h) + ")" |> Some


    // TODO:: Add handling of cases like "1-ST"
    // If testing for single string, for example, "1ST" pass (first all except last two) and (last two) letters: "1" "ST"
    let tryGetNumeral2 (s : string) (n : string) : int option =
        //printfn "tryGetNumeral2::s = %A, n = %A" s n
        let tryOneDigitNumeral (s : string) (n : string) =
            match s.Length with
            | 1 ->
                match s, n with
                | "1", "ST" -> Some 1
                | "2", "ND" -> Some 2
                | "3", "RD" -> Some 3
                | _, "TH" ->
                    match Int32.TryParse s with
                    | true, v when v >= 4 || v = 0 -> Some v
                    | _ -> None
                | _ -> None
            | _ -> None

        let tryGetStandardNumberal (s : string) (n : string) =
            match Int32.TryParse s with
            | true, i ->
                match s.Substring (s.Length - 2) |> int, n with
                | num, "TH" when num >= 10 && num <= 20 -> Some i
                | _ ->
                    match tryOneDigitNumeral (s.Substring (s.Length - 1)) n with
                    | Some _ -> Some i
                    | None -> None
            | false, _ -> None

        match s.Length with
        | 0 -> None
        | 1 -> tryOneDigitNumeral s n
        | 3 ->
            match s, n with
            | "FIF", "TH" -> Some 5
            | "SIX", "TH" -> Some 6
            | _ -> tryGetStandardNumberal s n
        | _ -> tryGetStandardNumberal s n


    let tryGetNumeral (s : string) : int option =
        match s.Length with
        | 0 | 1 | 2 -> None
        | _ -> tryGetNumeral2 (s.Substring (0, s.Length - 2)) (s.Substring (s.Length - 2))


    /// Cleans numerals from the list of strings, e.g.:
    /// [ "1ST"; "AVE" ] -> [ "1"; "AVE" ]
    /// [ "10TH"; "AVE" ] -> [ "10"; "AVE" ]
    /// [ "10"; "TH"; "AVE" ] -> [ "10"; "AVE" ]
    /// Note that [ "1"; "ST"; "AVE" ], [ "3"; "RD"; "AVE" ] cannot be fixed here because the code at this level does not know anything about street types.
    let cleanNumerals (sl : list<string>) =
        let rec fixNumeral acc prev rem =
            match prev with
            | Some p ->
                match rem with
                | [] -> p :: acc
                | h :: t ->
                    match tryGetNumeral2 p h with
                    | Some i ->
                        match i % 10, h with
                        | 1, "ST" -> fixNumeral (p :: acc) (Some h) t // Cannot replace e.g. [ ...; "1"; "ST"; ...] by [ ...; "1"; ...] because "ST" could be just "street".
                        | 3, "RD" -> fixNumeral (p :: acc) (Some h) t // Cannot replace e.g. [ ...; "3"; "RD"; ...] by [ ...; "3"; ...] because "RD" could be just "road".
                        | _ -> fixNumeral (i.ToString() :: acc) None t
                    | None -> fixNumeral (p :: acc) (Some h) t
            | None ->
                match rem with
                | [] -> acc
                | h :: t -> fixNumeral acc (Some h) t

        sl
        |> List.map (fun s -> match tryGetNumeral s with | Some i -> i.ToString() | None -> s)
        |> fixNumeral [] None
        |> List.rev


    let seqToString (s : #seq<string>) = s |> Seq.fold (fun acc r -> acc + (if acc = EmptyString then EmptyString else " ") + r) EmptyString


    let concat r = r |> List.map (fun e -> e.ToString()) |> String.concat DefaultSplitCharacter


    let expandRange p i j =
        match (i + j) % 2 = 0 with
        | true ->
            match p.expandMatchingParityRange with
            | DoNotExpandParity -> [ i; j ]
            | ExpandByOne -> [ for e in i..j -> e ]
            | ExpandByTwo ->  [ for e in i..2..j -> e ]
        | false ->
            match p.expandNonMatchingParityRange with
            | DoNotExpand -> [ i; j ]
            | ExpandAll -> [ for e in i..j -> e ]
        |> concat


    let expandOneHyphen p (a : string) (b : string) =
        match a.StartsWith "0" || b.StartsWith "0" with
        | false ->
            match Int32.TryParse a, Int32.TryParse b with
            | (true, i), (true, j) ->
                match j > i with
                | true ->
                    match (j - i) <= p.maxHouseRange with
                    | true -> expandRange p i j |> Some
                    | false -> None
                | false ->
                    match a.Length > b.Length with
                    | true ->
                        // Like "12-3"
                        let a1, b1 = a.Trim(), b.Trim()

                        match Int32.TryParse (a1.Substring(0, a1.Length - b1.Length) + b1) with
                        | (true, j1) when j1 > i && (j1 - i) <= p.maxHouseRange -> expandRange p i j1 |> Some
                        | _ -> None
                    | false -> [a; b] |> concat |> Some // Like "123-112"
            | _ -> a + DefaultSplitCharacter + b |> Some // Like "123N-1" - remove hyphen as we can't do anything with it.
        | true -> None // House numbers don't have leading zeroes.


    /// Adjusts the second parameter, j to account for skipped first digits, e.g. (1234, 5) -> (1234, 1235)
    let adjustNumber i j =
        match i > 0 && j > 0 with
        | true ->
            match i <= j with
            | true -> Some j
            | false ->
                let b = j.ToString().Length
                let m = pown 10 b
                let j1 = (i / m) * m + j
                Some j1

                // TODO kk:20191121 - Either add a parameter to control if we require increasing numbers OR remove commented lines.
                // Delete after 60 days if no decision is made.
                //match j1 > i with
                //| true -> Some j1
                //| false -> None
        | false -> None


    let expandManyHyphens p (a : string) g =
        let (v, n) = g |> List.map (fun (e : string) -> Int32.TryParse e) |> List.unzip

        let rec inner acc i e =
            match acc with
            | Some a ->
                match e with
                | [] -> acc
                | h :: t ->
                    match adjustNumber i h with
                    | Some j -> inner (Some (j :: a)) j t
                    | None -> None
            | None -> None

        match v |> List.fold (fun acc e -> acc && e) true, Int32.TryParse a with
        | true, (true, i) ->
            match inner (Some [ i ]) i n with
            | Some r -> r |> List.rev |> concat |> Some
            | None ->  None
        | _ -> None // Some strings are not numbers.


    let tryExpand p (x : string) =
        match x.Split([| Hyphen |], StringSplitOptions.RemoveEmptyEntries) |> List.ofArray with
        | [] -> None
        | _ :: [] -> None
        | a :: b :: [] -> expandOneHyphen p a b
        | a :: g -> expandManyHyphens p a g
        |> getValueOrDefault x


    /// Replaces "even-even" and "odd-odd" parts like "1234-38" and "1234-1238" by "1234 1236 1238" (expands the range).
    /// Replaces "even-odd" and "odd-even" parts like "1234-37" by "1234 1237" (does not expand the range).
    /// Replaces parts like "1234-56-78" by "1234 1256 1278".
    /// Parts like "12-003" or even "12-03" WILL currently be ignored as it is unclear if we should interpret them as ranges.
    /// Very large ranges are not replaced.
    let expandHyphen p s =
        s
        |> defaultSplit
        |> List.map (tryExpand p)
        |> String.concat DefaultSplitCharacter


    let removeHyphen (s : String) = s.Replace(Hyphen, EmptyString)
    let isAllAlpha (s : String) = Regex.IsMatch(s, @"^[A-Z]+$")
    let isAllNumeric (s : String) = Regex.IsMatch(s, @"^[0-9]+$")


    /// Normalized unit numbers are like:
    ///    "1F", "A1", "12", "AA"
    /// Add more patterns if needed.
    /// Weird cases like below are currently excluded:
    ///     "14-2A", "45-394"
    let isUnitNumber (s : String) =
        [
            fun () -> Regex.IsMatch(s, @"^[A-Z]{1}$") // from "A" to "Z"
            fun () -> Regex.IsMatch(s, @"^[0-9]{1,4}$") // from "1" to "9999"
            fun () -> Regex.IsMatch(s, @"^[A-Z]{1}[ ]{0,1}[0-9]{1,4}$") // from "A1" to "Z9999"
            fun () -> Regex.IsMatch(s, @"^[1-9]{1}[0-9]{0,3}[ ]{0,1}[A-Z]{1}$") // from "1A" to "9999Z"
        ]
        |> List.tryFind (fun e -> e())
        |> Option.isSome


    let gridHouseNumberPatterns =
        [
            @"^[N]{1}[ ]{0,1}[1-9]{1}[0-9]{0,2}[ ]{0,1}[W]{1}[ ]{0,1}[1-9]{1}[0-9]{1,4}$" // from "N1W10" to "N999W99999"
            @"^[W]{1}[ ]{0,1}[1-9]{1}[0-9]{0,2}[ ]{0,1}[N]{1}[ ]{0,1}[1-9]{1}[0-9]{1,4}$" // from "W1N10" to "W999N99999"
            @"^[S]{1}[ ]{0,1}[1-9]{1}[0-9]{0,2}[ ]{0,1}[W]{1}[ ]{0,1}[1-9]{1}[0-9]{1,4}$" // from "S1W10" to "S999W99999"
            @"^[W]{1}[ ]{0,1}[1-9]{1}[0-9]{0,2}[ ]{0,1}[S]{1}[ ]{0,1}[1-9]{1}[0-9]{1,4}$" // from "W1S10" to "W999S99999"
            @"^[N]{1}[ ]{0,1}[1-9]{1}[0-9]{0,4}$" // from "N1" to "N99999"
            @"^[W]{1}[ ]{0,1}[1-9]{1}[0-9]{0,4}$" // from "W1" to "W99999"
            @"^[E]{1}[ ]{0,1}[1-9]{1}[0-9]{0,4}$" // from "E1" to "E99999"
            @"^[S]{1}[ ]{0,1}[1-9]{1}[0-9]{0,4}$" // from "S1" to "S99999"
            @"^[B]{1}[ ]{0,1}[1-9]{1}[0-9]{0,4}$" // from "B1" to "S99999"
            @"^[ ]{0,1}[1-9]{1}[0-9]{0,2}[ ]{0,1}[N]{1}[ ]{0,1}[1-9]{1}[0-9]{1,4}$" // from "1N10" to "999N99999"
            @"^[ ]{0,1}[1-9]{1}[0-9]{0,2}[ ]{0,1}[S]{1}[ ]{0,1}[1-9]{1}[0-9]{1,4}$" // from "1S10" to "999S99999"
            @"^[ ]{0,1}[1-9]{1}[0-9]{0,2}[ ]{0,1}[W]{1}[ ]{0,1}[1-9]{1}[0-9]{1,4}$" // from "1W10" to "999W99999"
            @"^[ ]{0,1}[1-9]{1}[0-9]{0,2}[ ]{0,1}[N]{1}[ ]{0,1}[0]{1}[1-9]{1}[0-9]{1,3}$" // from "1N01" to "999N09999"
            @"^[ ]{0,1}[1-9]{1}[0-9]{0,2}[ ]{0,1}[W]{1}[ ]{0,1}[0]{1}[1-9]{1}[0-9]{1,3}$" // from "1W01" to "999W09999"
        ]


    let halfIntegerHouseNumberPatterns =
        [
            @"^[1-9]{1}[0-9]{0,4}[ ]{1}[1]{1}[ ]{0,1}[\/]{1}[ ]{0,1}[2]{1}$" // from "1 1/2" to "99999 1/2"
        ]


    let regularHouseNumberPatterns =
        [
            @"^[1-9]{1}[0-9]{0,6}$" // from "1" to "9999999"
            @"^[1-9]{1}[0-9]{0,4}[ ]{0,1}[A-J]{1}$" // from "1A" to "99999J"
        ]


    /// Allows "0" at the beginning. Usually this is a bug but some are valid!
    let regularRelaxedHouseNumberPatterns =
        [
            @"^[0-9]{0,9}$" // from "0" to "9999999"
            @"^[0-9]{0,5}[ ]{0,1}[A-J]{1}$" // from "0A" to "99999J"
        ]


    let allHouseNumberPatterns =
        regularHouseNumberPatterns @ gridHouseNumberPatterns @ halfIntegerHouseNumberPatterns


    let private isHouseNumber patterns (s : String) =
        patterns
        |> List.map (fun (e : String) -> fun () -> Regex.IsMatch(s, e))
        |> List.tryFind (fun e -> e())
        |> Option.isSome


    let isRegularHouseNumber = isHouseNumber regularHouseNumberPatterns
    let isRegularRelaxedHouseNumber = isHouseNumber regularRelaxedHouseNumberPatterns
    let isGridHouseNumber = isHouseNumber gridHouseNumberPatterns
    let isHalfIntegerHouseNumber = isHouseNumber halfIntegerHouseNumberPatterns


    /// Removes hyphen between Alpha-Num and Num-Alpha:
    ///     "1-F" -> "1F"
    ///     "F-1" -> "F1"
    /// But:
    ///     "1-2" -> "1-2"
    ///     "A-B" -> "A-B"
    ///     "1-2A" -> "1-2A"
    ///     "A1-2" -> "A1-2"
    let removeUnitNumberHyphen (s : String) =
        let tryUnitNumber a b =
            let w() =
                match a + b with
                | c when isUnitNumber c -> c
                | _ -> s

            match (a, b) with
            | _ when (isAllAlpha a) && (isAllNumeric b) -> w() // "F-1" -> "F1"
            | _ when (isAllNumeric a) && (isAllAlpha b) -> w() // "2-A" -> "2A"
            | _ -> s // Anything else.

        match s.Split([| Hyphen |], StringSplitOptions.RemoveEmptyEntries) |> List.ofArray with
        | a :: b :: [] -> tryUnitNumber a b
        | _ -> s // Not exactly one hyphen.


    let replaceHyphenWithSpace (s : String) = s.Replace(Hyphen, DefaultSplitCharacter)


    let split (splitters : string[]) (s : string) =
        s.Split(splitters, StringSplitOptions.RemoveEmptyEntries)


    type HyphenTreatmentType
        with

        member this.processHyphen =
            match this with
            | ExpandHyphen p -> expandHyphen p
            | RemoveHyphen -> removeHyphen
            | RemoveUnitNumberHyphen -> removeUnitNumberHyphen
            | ReplaceHyphenWithSpace -> replaceHyphenWithSpace
            | DoNotChangeHyphen -> id


    type CleanStringParams =
        {
            ignored : List<string>
            replacementGroups : List<List<string> * List<string>>
            hyphenTreatmentParams : HyphenTreatmentParams
            maxNumberOfWords : int option
        }

        static member defaultValue =
            {
                ignored = addressSeparators
                replacementGroups = standardGroups
                hyphenTreatmentParams = HyphenTreatmentParams.defaultValue
                maxNumberOfWords = None
            }

        static member unitNumberValue =
            {
                ignored = addressSeparators
                replacementGroups = standardGroups
                hyphenTreatmentParams = HyphenTreatmentParams.unitNumberValue
                maxNumberOfWords = None
            }

        static member expandAllNonMatchingParityValue =
            {
                ignored = addressSeparators
                replacementGroups = standardGroups
                hyphenTreatmentParams = HyphenTreatmentParams.expandAllNonMatchingParityValue
                maxNumberOfWords = None
            }

        /// Does not expand any hyphen separated ranges.
        static member doNotExpandValue =
            {
                ignored = addressSeparators
                replacementGroups = standardGroups
                hyphenTreatmentParams = HyphenTreatmentParams.doNotExpandValue
                maxNumberOfWords = None
            }


    let combineIfNotTooLong m (sl : List<string>) =
        let maxNumberOfWords = m |> Option.defaultValue MaxNumberOfWords
        if sl.Length <= maxNumberOfWords
        then sl |> seqToString
        else EmptyString // If there are too many words, then we will get factorial+ time growth.


    let cleanString i (s : string) =
        let fixPartsSeparator (s : string) =
            s.Replace("–", Hyphen).Replace("—", Hyphen).Replace(" -", Hyphen).Replace("- ", Hyphen).Replace("½", "1/2").Replace("/", " / ").Replace("  ", DefaultSplitCharacter)
            |> i.hyphenTreatmentParams.hyphenTreatmentType.processHyphen
            |> defaultSplit
            |> cleanNumerals
            |> replaceGroups i.replacementGroups
            |> combineIfNotTooLong i.maxNumberOfWords
            |> splitAlphaNum

        let a = i.ignored |> List.fold (fun (acc : string) r -> acc.Replace(r, DefaultSplitCharacter)) (s.ToUpper())

        let b =
            [| 1..5 |] |> Array.fold (fun (acc : string) _ -> acc.Replace("  ", DefaultSplitCharacter)) a
            |> fixPartsSeparator

        b


    let tryAdd (eo : 'A option) (l : list<'A>) =
        match eo with
        | Some e -> e :: l
        | None -> l


    /// Splits the numbers from the beginning of the list.
    /// Returns list of numbers (as strings) and remaining list of strings, which starts from not a number.
    let getNumbers (s : list<string>) =
        let rec inner (n: list<string>) (s : list<string>) =
            match s with
            | [] -> n, s
            | h :: t ->
                match h |> Int32.TryParse |> fst with
                | true -> inner (h :: n) t
                | false -> n, s

        inner [] s


    let allCases<'T when 'T : comparison>() : list<'T> =
        let flags = BindingFlags.Public ||| BindingFlags.NonPublic
        let cases = FSharpType.GetUnionCases(typeof<'T>, flags)
        [ for c in cases -> FSharpValue.PreComputeUnionConstructor (c, flags) [||] :?> 'T ]
        |> List.sortBy (fun f -> f)


    /// http://www.fssnip.net/5S/title/All-subsets-of-a-set
    let rec subsets s =
        set [ // Add current set to the set of subsets.
            yield s
            // Remove each element and generate subset of that smaller set.
            for e in s do
                yield! subsets (Set.remove e s) ]


    let private toSubsets s =
        s
        |> List.mapi (fun i e -> (i, e))
        |> Set.ofList
        |> subsets


    /// Returns set of all sorted sublists, which can be made out of input list.
    let setOfSortedSublists (s : list<'A>) =
        s
        |> toSubsets
        |> Set.map (fun e -> e |> Set.toList |> List.map (fun (_, e) -> e) |> List.sort)


    /// Returns sets of all subsets, which can be made out of input list, and keeping the order of words.
    let setOfUnsortedSublists (s : list<'A>) =
        let r =
            s
            |> toSubsets
            |> Set.map (fun e -> e |> Set.toList |> List.sortBy (fun (i, _) -> i) |> List.map (fun (_, e) -> e))
        r


    let correctWords (m : Map<string, string>) (u : list<string>) =
        u |> List.map (fun w -> match m.TryFind w with | Some c -> c | None -> w)


    /// Returns ToString() representation of underlying object if it is Some and EmptyString if it is None.
    let toStr (so : 'A option) =
        match so with
        | Some s -> s.ToString()
        | None -> EmptyString


    /// Computes normalized Levenshtein distance.
    /// http://www.fssnip.net/bj/title/Levenshtein-distance
    let levenshtein word1 word2 =
        let preprocess = fun (str : string) -> str.ToLower().ToCharArray()
        let chars1, chars2 = preprocess word1, preprocess word2
        let m, n = chars1.Length, chars2.Length
        let table : int[,] = Array2D.zeroCreate (m + 1) (n + 1)
        for i in 0..m do
            for j in 0..n do
                match i, j with
                | i, 0 -> table.[i, j] <- i
                | 0, j -> table.[i, j] <- j
                | _, _ ->
                    let delete = table.[i-1, j] + 1
                    let insert = table.[i, j-1] + 1
                    //cost of substitution is 2
                    let substitute =
                        if chars1.[i - 1] = chars2.[j - 1]
                            then table.[i-1, j-1] //same character
                            else table.[i-1, j-1] + 2
                    table.[i, j] <- List.min [delete; insert; substitute]
        //table.[m, n], table //return tuple of the table and distance
        let norm = max (sqrt (((float m) * (float m) + (float n) * (float n)) / 2.0)) 1.0
        (float table.[m, n]) / norm
