namespace Softellect.AddressProcessor

open System
open FSharpx.Collections

/// Collection of type aliases / functions to simplify dealing with metric trees in a unified and implementation independent manner.
/// Don't use any of implementation dependent code.
/// If anything else is needed then put functions / alias types here and use them.
module MetricTreeInterop =

    /// An abstraction over some specific metric tree implementation.
    type MetricTree = BKTree<ByteString>


    /// Converts string into a ByteString representation.
    let toByteString = ByteString.ofString


    /// Converts ByteString back to string representation.
    let fromByteString = ByteString.toString


    /// Converts a sequence of ByteString into a MetricTree.
    /// Replaces BKTree.ByteString.ofList, BKTree.ByteString.ofArray, BKTree.ByteString.ofSeq by a single function.
    let toMetricTree (s : #seq<ByteString>) = BKTree.ByteString.ofSeq s


    /// Gets all elements of MetricTree, which can be obtained from a given input (n) using no more than (d) edits.
    /// Note that the tree parameter was moved upfront to allow partial application.
    let toListDistance t d n = BKTree.ByteString.toListDistance d n t


    /// Checks that there exists at least one element in MetricTree, which is not further than (d) edits from a given input (n).
    /// Note that the tree parameter was moved upfront to allow partial application.
    let existsDistance t d n = BKTree.ByteString.existsDistance d n t


    /// Calculates distance between two byte strings.
    let distance = BKTree.ByteString.distance


    /// Calculates normalized distance between two byte strings.
    let normalizedDistance a b =
        let d = distance a b
        let m = a.Count
        let n = b.Count
        let norm = max (sqrt (((float m) * (float m) + (float n) * (float n)) / 2.0)) 1.0
        let r = (float d) / norm
        r


    type String
        with

        /// Calculates distance between two strings.
        /// DO NOT USE if distance from the same string is calculated many times.
        /// Convert string, which will be used may times into ByteString and calculate the distance from it.
        member a.distance (b : string) = (toByteString a, toByteString b) ||> distance

        member a.normalizedDistance (b : string) = (toByteString a, toByteString b) ||> normalizedDistance


    type ByteString
        with

        /// Calculates the distance from ByteString to a given string.
        member a.distance (b : string) = (a, toByteString b) ||> distance

        member a.normalizedDistance (b : string) = (a, toByteString b) ||> normalizedDistance

    //======================================
    // End of interop.
    //======================================

    [<Literal>]
    let SingleSpace = " "


    let projectionMap1 =
        [
            '0', '0'
            '1', '0'
            '2', '0'
            '3', '0'
            '4', '0'
            '5', '0'
            '6', '0'
            '7', '0'
            '8', '0'
            '9', '0'
            'A', 'A'
            'B', 'B'
            'C', 'C'
            'D', 'D'
            'E', 'A'
            'F', 'B'
            'G', 'C'
            'H', 'A'
            'I', 'A'
            'J', 'C'
            'K', 'C'
            'L', 'L'
            'M', 'M'
            'N', 'M'
            'O', 'A'
            'P', 'B'
            'Q', 'C'
            'R', 'R'
            'S', 'C'
            'T', 'D'
            'U', 'A'
            'V', 'B'
            'W', 'A'
            'X', 'C'
            'Y', 'A'
            'Z', 'C'
        ]
        |> Map.ofList


    let projectionMap2 = projectionMap1 |> Map.filter (fun _ v -> v <> 'A')


    let projectionMap3 =
        [
            '0', '0'
            '1', '1'
            '2', '2'
            '3', '3'
            '4', '4'
            '5', '5'
            '6', '6'
            '7', '7'
            '8', '8'
            '9', '9'
            'A', 'A'
            'B', 'B'
            'C', 'C'
            'D', 'D'
            'E', 'A'
            'F', 'F'
            'G', 'G'
            'H', 'A'
            'I', 'A'
            'J', 'G'
            'K', 'C'
            'L', 'L'
            'M', 'M'
            'N', 'M'
            'O', 'A'
            'P', 'B'
            'Q', 'C'
            'R', 'R'
            'S', 'C'
            'T', 'D'
            'U', 'A'
            'V', 'F'
            'W', 'A'
            'X', 'X'
            'Y', 'A'
            'Z', 'X'
        ]
        |> List.filter (fun (_, v) -> v <> 'A')
        |> Map.ofList


    let vowels = [ 'A'; 'E'; 'I'; 'O'; 'U'; 'Y' ] |> Set.ofList
    let isVowel c = vowels |> Set.contains c
    let isConsolant c = c |> isVowel |> not


    let toSpace c =
        match c with
        | None -> ' '
        | Some ' ' -> ' '
        | Some v -> v


    let fixW p c n nn =
        match c with
        | 'W' ->
            match (toSpace p), (toSpace n), (toSpace nn) with
            | ' ', ' ', _ -> 'V' // as in " W "
            | ' ', 'R', _ -> c // as in " WRENCH", " WRONG"
            | ' ', 'H', 'O' -> c // as in " WHO", " WHOLE"
            | ' ', 'H', _ -> 'V' // as in " WHAT", " WHERE"
            | ' ', _, _ -> 'V'
            | _, ' ', _ -> c // as in "KNOW "
            | _, e, _ when isVowel e -> 'V' // as in "KNOWING"
            | _, e, _ when isConsolant e -> c // as in "KNOWHOW"
            | _ -> 'V'
        | _ -> c


    let fixAllW (s : list<char>) =
        let rec inner acc rem p c n nn =
            let c1 = fixW p c (Some n) (Some nn)

            match rem with
            | [] -> (c1 :: acc) |> List.rev
            | h :: t -> inner (c1 :: acc) t (Some c1) n nn h

        match s with
        | [] -> s
        | c::[] -> [ fixW None c None None ]
        | c :: n :: [] -> [ fixW None c (Some n) None; fixW (Some c) n None None ]
        | c :: n :: nn :: t -> inner [] t None c n nn


    let private projectStringBase fixer (map : Map<char, char>) (s : string) =
        let b = s.ToCharArray() |> List.ofArray |> fixer

        let folder (prev, acc) r =
            match prev, map |> Map.tryFind r with
            | Some p, Some c ->
                match p = c with
                | true -> Some c, acc
                | false -> Some c, c :: acc
            | None, Some c -> Some c, c :: acc
            | Some p, None -> Some p, acc
            | None, None -> None, acc

        let x = b |> List.fold folder (None, []) |> snd |> List.rev |> Array.ofList
        x


    let projectString fixer map s = projectStringBase fixer map s |> String
    let projectToByteString fixer map s = projectStringBase fixer map s |> Array.map byte |> ByteString.create


    /// Removes all white space from the string.
    let removeSpaceProjector (s : string) = s.Replace(" ", "")


    /// Performs about half of phonetic projection.
    let halfPhoneticProjector = projectString id projectionMap1


    /// Performs phonetic projection. It is based on Soundex ideas. However, it is still fairly different.
    let phoneticProjector = projectString id projectionMap2


    /// Removes vowels, merges some close consonants, and then removes the duplicates.
    let removeVowelsProjector = projectString fixAllW projectionMap3


    /// A blend of a map and metric tree with string as key and 'T as value.
    /// Exact match from map is tried first to improve performance and then
    /// if it fails, then approximate match from MetricTree is performed.
    type MetricStringMap<'T> =
        {
            tree : MetricTree
            map : Map<string, 'T>
        }

        /// Tries to find the key in the map and returns the value if successful.
        /// Otherwise uses metric tree to perform fuzzy matching, then looks up the keys in the map.
        /// If exact match is found, then the distance parameter is ignored.
        member m.tryFind f n o : list<'T> =
            match m.map |> Map.tryFind n with
            | Some v -> [ v ]
            | None ->
                match m.map |> Map.tryFind o with
                | Some v -> [ v ]
                | None ->
                    let d = f n
                    let d1 = f o
                    let a = n |> toByteString
                    let a1 = o |> toByteString
                    let b = toListDistance m.tree d a |> List.map fromByteString
                    let b1 = toListDistance m.tree d1 a1 |> List.map fromByteString
                    let c = b |> List.map (fun e -> m.map |> Map.tryFind e) |> List.choose id
                    let c1 = b1 |> List.map (fun e -> m.map |> Map.tryFind e) |> List.choose id
                    c @ c1

        /// Creates MetricMap from a given map with string key.
        static member ofStringMap (m : Map<string, 'T>) =
            let a = m |> Map.toList
            let t, v = a |> List.unzip

            {
                tree = t |> List.map toByteString |> toMetricTree
                map = List.zip t v |> Map.ofList
            }


    /// A blend of metric tree and a map. It utlizes projector to project input into some standardized representation.
    type MetricListMap<'T when 'T : equality and 'T : comparison> =
        {
            tree : MetricTree
            map : Map<string, list<'T>>
            projector : string -> string
        }

        /// Tries to find the key in the map and returns the value if successful.
        /// Otherwise uses metric tree to perform fuzzy matching, then looks up the keys in the map.
        /// If exact match is found, then the distance parameter is ignored.
        member m.tryFind f n : list<'T> =
            match m.map |> Map.tryFind n with
            | Some v -> v
            | None ->
                let d = f n
                let a = n |> (m.projector >> toByteString)
                let b = toListDistance m.tree d a |> List.map fromByteString
                let c = b |> List.map (fun e -> m.map |> Map.tryFind e) |> List.choose id |> List.concat |> List.distinct
                c

        /// Creates MetricMap from a given map with string key.
        static member ofStringMap projector (m : Map<string, list<'T>>) =
            let a = m |> Map.toList
            let t0, v0 = a |> List.unzip
            let t = t0 |> List.map projector
            let map = List.zip t v0 |> List.groupBy (fun (e, _) -> e) |> List.map (fun (k, v) -> k, v |> List.map (fun (_, e) -> e) |> List.concat |> List.distinct) |> Map.ofList

            {
                tree = t |> List.distinct |> List.map toByteString |> toMetricTree
                map = map
                projector = projector
            }


    type ProjectionParams =
        {
            normalizeString : string -> string
            projectString : string -> ByteString
        }


    let getProjectedDistance (p : ProjectionParams) (pr : ByteString) d =
        match d with
        | 0 | 1 | 2 | 3 | 4 -> d
        | 5 |6 | 7 | 8 -> d - 1
        | 9 | 10 | 11 -> d - 2
        | _ -> (d / 2) + 2


    type ProjectedMap<'T when 'T : equality and 'T : comparison> =
        {
            projectedTree : MetricTree
            treeMap : Map<string, MetricTree>
            map : Map<string, list<'T>>
            projectionParams : ProjectionParams
        }


        member m.tryFind f (i : string) =
            let n = m.projectionParams.normalizeString i
            match m.map |> Map.tryFind n with
            | Some v -> v
            | None ->
                let p = m.projectionParams.projectString n
                let d = f i
                let d1 = getProjectedDistance m.projectionParams p d
                let t = toListDistance m.projectedTree d1 p |> List.map (fun e -> m.treeMap |> Map.tryFind (fromByteString e)) |> List.choose id
                let a = n |> toByteString

                // TODO kk:20200211 - Do in 90 days - replace 3 lines below with commented out line once everyithing is confirmed to work as expected.
                //let c = t |> List.map (fun e -> toListDistance e d a |> List.map (fun e -> m.map |> Map.tryFind (fromByteString e)) |> List.choose id) |> List.concat |> List.concat
                let c0 =  t |> List.map (fun e -> toListDistance e d a)
                let c1 = c0 |> List.map (fun x -> x |> List.map (fun e -> m.map |> Map.tryFind (fromByteString e)))
                let c = c1 |> List.map (fun e -> e |> List.choose id) |> List.concat |> List.concat
                c


        static member private create x p =
            let t, _ = x |> List.unzip

            let q =
                t
                |> List.map (fun e -> p.projectString e, e)
                |> List.groupBy (fun (e, _) -> e)
                |> List.map (fun (a, b) -> a, b |> List.map (fun (_, d) -> d))
                |> List.map (fun (a, b) -> a, b |> List.distinct)

            {
                projectedTree = q |> List.map fst |> toMetricTree
                treeMap = q |> List.map (fun (a, b) -> fromByteString a, b |> List.map toByteString |> toMetricTree)|> Map.ofList
                map = x |> Map.ofList
                projectionParams = p
            }


        static member ofList p k (i : list<'T>) =
            let x =
                i
                |> List.map (fun e -> (k >> p.normalizeString) e, e)
                |> List.groupBy (fun (a, _) -> a)
                |> List.map (fun (a, b) -> a, b |> List.map snd)

            ProjectedMap<'T>.create x p


        static member ofMap p (m : Map<string, list<'T>>) =
            let x =
                m
                |> Map.toList
                |> List.map (fun (k, v) -> p.normalizeString k, v)
                |> List.groupBy (fun (a, _) -> a)
                |> List.map (fun (a, b) -> a, b |> List.map snd |> List.concat)

            ProjectedMap<'T>.create x p


    /// Creates ProjectedMap with phoneticProjector.
    let withPhoneticProjector<'T when 'T : equality and 'T : comparison> projector (m : Map<string, list<'T>>) =
        let p =
            {
                normalizeString = projector
                projectString = phoneticProjector >> toByteString
            }

        ProjectedMap<'T>.ofMap p m
