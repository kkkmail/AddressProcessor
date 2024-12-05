namespace Softellect.AddressProcessorTests

open System
open System.IO
open Xunit
open Xunit.Abstractions
open FluentAssertions
open MBrace.FsPickler

open Softellect.AddressProcessor.DynamicData
open Softellect.AddressProcessor.AddressTypes
open Softellect.AddressProcessor.StringParser
open Softellect.AddressProcessor.Extensions
open Softellect.AddressProcessor.MatchParams
open Softellect.AddressProcessor.MetricTreeInterop
open Softellect.AddressProcessor
open Softellect.AddressProcessor.StreetNameRules
open Softellect.AddressProcessor.MatchingRules
open Softellect.AddressProcessor.MatchTypes
open Softellect.AddressProcessor.Configuration

open Softellect.AddressProcessorTests.TreeTestData
open Softellect.AddressProcessorTests.Primitives

open Microsoft.EntityFrameworkCore
open Microsoft.Data.SqlClient

/// !!! Always explicitly specify types of parameters in each test with InlineData !!!
/// Otherwise test discovery may hang up if the underlying functions change signatures.
/// While this looks like a bug in xUnit, we still need to make sure that the tests work.
type TreeTests(output : ITestOutputHelper) =

    let binSerializer = FsPickler.CreateBinarySerializer()
    let binSerialize t = binSerializer.Pickle t


    /// http://www.fssnip.net/iW/title/Oneliner-generic-timing-function
    let time f a = System.Diagnostics.Stopwatch.StartNew() |> (fun sw -> (f a, sw.Elapsed.TotalMilliseconds))


    /// kk:20200211 - Do not delete commented line. It is used to switch between MetricListMap and ProjectedMap.
    let mapStreet t (g : #seq<Street>) =
        let toValidSortedSubLists (x : Street) = x.toValidSortedSubLists
        let toValidOriginalSortedSubLists (x : Street) = x.toValidOriginalSortedSubLists
        let w f = g |> Seq.map (fun x -> (f x t |> Set.toSeq |> Seq.map (fun e -> e, x)))

        let create e =
            {
                street = e
                city = City { projectedName = "A"; originalName = Some "A" }
                state = State.AL
            }

        match t.projectionUsage with
        | ProjectedOnly -> [ toValidSortedSubLists ]
        | OriginalOnly -> [ toValidOriginalSortedSubLists ]
        | ProjectedAndOriginal -> [ toValidSortedSubLists; toValidOriginalSortedSubLists ]
        |> List.map w
        |> List.fold Seq.append Seq.empty
        |> Seq.fold (fun acc r -> acc |> Seq.append r) Seq.empty
        |> Seq.groupBy fst
        |> Seq.map (fun (e, v) -> e, v |> List.ofSeq |> List.map (fun (_, s) -> s) |> List.distinct |> List.map create)
        |> Map.ofSeq
        |> Some
        //|> toStreetMetricListMap
        |> toStreetProjectedMap


    let getObjectSize o = (binSerialize o).Length


    let metricTreeTest n =
        let m = n / 100
        let k = n / 10

        let list =
            ([1..n] |> List.map (sprintf "BIRCH%i"))
            @
            ([1..n] |> List.map (fun i -> $"%i{i / m}CHWO%i{i % m}"))
            @
            [
                "SOME STUFF"
                "BIRCHWOOD"
                "BIRCHWOOD 123"
                "12 BIRCHWOOF 123"
                "SOME BIRCHWOOD"
                "SOME MORE STUFF"
            ]
            @
            ([1..n] |> List.map (fun i -> $"%i{i / k}HWOO%i{i % k}"))
            @
            ([1..n] |> List.map (sprintf "%iWOOD"))
            |> List.distinct
            |> List.map toByteString

        let d = 3
        let n = "BIRCHWOOF" |> toByteString

        let t1 = DateTime.Now

        let tree = toMetricTree list
        let t2 = DateTime.Now

        let a = toListDistance tree d n
        let t3 = DateTime.Now

        let b = existsDistance tree d n
        let t4 = DateTime.Now

        output.WriteLine $"Size of tree = %i{getObjectSize tree}"
        output.WriteLine $"a = %A{a |> List.map fromByteString}"
        output.WriteLine $"b = %A{b}"

        a.Length.Should().Be(1, EmptyBecause) |> ignore
        ((a |> List.head) |> fromByteString).Should().Be("BIRCHWOOD", EmptyBecause) |> ignore

        (t2 - t1, t3 - t2, t4 - t3)


    let runFuzzyStreetMatch c e w =
        let create = Street.tryCreateCleaned CleanStringParams.defaultValue
        let t = ToSublistsParams.fuzzyMatchValue
        let f = streetDefaultDistance
        let comparand = c |> defaultSplit |> List.rev

        let m =
            w
            |> List.map (fun (a, b) -> a, Some b)
            |> List.map create
            |> List.choose id
            |> mapStreet t

        let n = f c

        let s =
            c
            |> m.tryFind f
            |> List.distinct
            |> List.map (fun e -> e.street, e.street.value.normalizedDistance c, distanceValidator id comparand e.street)
            |> List.sortBy (fun (_, _, e) -> e)

        output.WriteLine $"n = %i{n}, s = %A{s}"
        let a, _, _ = s.Head
        a.value.Should().Be(e, EmptyBecause)


    let toPassFail v =
        match v with
        | Perfect -> "PASS"
        | Partial p when p < MatchThreshold -> "PASS"
        | _ -> "FAIL"


    let getDistanceMatch i p c a =
        let comparand = c |> defaultSplit |> List.rev
        let d = distanceValidator p comparand a.street
        output.WriteLine $"i = %s{i}, a = %s{a.street.value}, c = %s{c}, d = %A{d} - %s{toPassFail d}"
        d


    let compareListsMatch i p c a =
        let comparand = c |> defaultSplit |> List.rev

        let b =
            {
                comparand = comparand
                fullComparand = comparand
                fullComparandType = NoSkippedTail
                skipped = []
            }

        let d = compareListsValidator p b a
        output.WriteLine $"i = %s{i}, a = %s{a.street.value}, c = %s{c}, d = %A{d} - %s{toPassFail d}"
        d


    let comparers =
        [
            "id                   ", getDistanceMatch, id
            "removeSpaceProjector ", getDistanceMatch, removeSpaceProjector
            "halfPhoneticProjector", getDistanceMatch, halfPhoneticProjector
            "phoneticProjector    ", getDistanceMatch, phoneticProjector
            "removeVowelsProjector", getDistanceMatch, removeVowelsProjector
            "compareListsMatch    ", compareListsMatch, id
        ]


    let runComparers s o c =
        let a =
            {
                street = Street.tryCreateCleaned CleanStringParams.defaultValue (s, Some o) |> Option.defaultWith (fun () -> failwith "Unable to create street!")
                city = City { projectedName = "A"; originalName = Some "A" }
                state = State.AL
            }

        comparers |> List.map (fun (i, f, p) -> f i p c a)


    let loadData sqlConn x =
        x
        |> List.map ZipCode
        |> List.map (loadZipMapData sqlConn ToSublistsParams.exactMatchValue)
        |> List.choose id
        |> List.map (fun e -> e |> Map.toList)
        |> List.concat
        |> List.map (fun (_, s) -> s |> List.map (fun e -> e.street.value |> defaultSplit, [e]))
        |> List.concat
        |> Map.ofList


    /// kk:20200106 - This test is loosely based on: https://gist.github.com/pocketberserker/3975704
    ///
    /// The purpose of this test is [mostly] to measure performance.
    /// There are around 300K distinct words in all street names in the US according to MD.
    /// This test with n = 100_000 has around 400K words.
    ///
    /// As of 20200106 and using i9-9980XE the results are as follows (time is in ms):
    ///     For n = 100_000 the [not stellar] results are:
    ///         Size of tree = 34_117_337
    ///         |BKTree | 175070.645400 |  1686.489700 |   422.866700 |
    ///
    ///     For n = 10_000 the [mediocre] results are:
    ///         Size of tree = 3_347_955
    ///         |BKTree |  12548.430000 |   533.573300 |   421.871500 |
    ///
    /// which means that we want to keep the number of words substantially below 10K to ensure that it will not take forever to get the results.
    [<Theory>]
    [<InlineData(5_000)>]
    [<InlineData(10_000)>]
    [<InlineData(20_000)>]
    member _.runMetricTreeTest (n : int) =
        [| 1..1 |]
        |> Array.map (fun _ -> metricTreeTest n)
        |> Array.map (fun (ofL, toLD, exD) -> (ofL.TotalMilliseconds, toLD.TotalMilliseconds, exD.TotalMilliseconds))
        |> Array.unzip3
        |> fun (ofL, toLD, exD) -> (Array.average ofL, Array.average toLD, Array.average exD)
        |||> sprintf "|BKTree | %12f | %12f | %12f |"
        |> output.WriteLine


    /// The purpose of this test is to measure various performance times related to exact and fuzzy searches.
    [<Fact>]
    member _.runMetricMapTest() =
        let conn = failwith "conn is not implemented."
        let sqlConn = RatingConnectionGetter (fun () -> new SqlConnection(conn))

        let z = ZipCode "71909"
        let s = "EMANUEL DR"
        let s1 = "EMMANUEL DR"

        let distance _ = 2

        let t1 = DateTime.Now
        let a = loadZipMapData sqlConn ToSublistsParams.fuzzyMatchValue z
        let t2 = DateTime.Now

        let outputStreetCityStates n x =
            output.WriteLine $"%s{n} =\n  ["

            x
            |> List.sort
            |> List.map (fun v -> $"    %s{v.street.value}, %s{v.city.value} %s{v.state.value}")
            |> List.map output.WriteLine
            |> ignore

            output.WriteLine "  ]\n"

        match a with
        | Some m ->
            //let t = toStreetMetricListMap (Some m)
            let t = toStreetProjectedMap (Some m)
            let t3 = DateTime.Now
            let c = t.tryFind distance s
            let t4 = DateTime.Now
            let c1 = t.tryFind distance s1
            let t5 = DateTime.Now
            let c2 = m |> Map.tryFind (s |> defaultSplit |> List.sort) |> Option.defaultValue []
            let t6 = DateTime.Now
            let c3 = t.tryFind distance s1
            let t7 = DateTime.Now

            [
                "Load: %12f", (t2 - t1)
                "Tree: %12f", (t3 - t2)
                "Correct: %12f", (t4 - t3)
                "Fuzzy: %12f", (t5 - t4)
                "Map: %12f", (t6 - t5)
                "Fuzzy (again): %12f", (t7 - t6)
            ]
            |> List.map(fun (a, b) -> sprintf (Printf.StringFormat<float->string> a) b.TotalMilliseconds)
            |> String.concat ", "
            |> output.WriteLine

            let compare a b = (a |> List.sort) = (b |> List.sort)

            $"c = c1 = %A{compare c c1}" |> output.WriteLine
            $"c = c2 = %A{compare c c2}" |> output.WriteLine
            $"c1 = c2 = %A{compare c1 c2}" |> output.WriteLine
            $"c1 = c3 = %A{compare c1 c3}" |> output.WriteLine
            $"Map length:  %i{m.Count}" |> output.WriteLine
            //sprintf "Tree length:  %A" (t.tree.ToList().Length) |> output.WriteLine
            $"Projected tree length:  %A{t.projectedTree.ToList().Length}" |> output.WriteLine
            $"Tree map length:  %A{t.treeMap.Count}" |> output.WriteLine

            outputStreetCityStates "c" c
            outputStreetCityStates "c1" c1
            outputStreetCityStates "c2" c2
        | None -> failwith "Unable to load data..."


    /// The purpose of this test is to measure various performance times related to exact and fuzzy searches.
    [<Fact>]
    member _.runMetricMapTest1() =
        let conn = failwith "conn is not implemented."
        let sqlConn = RatingConnectionGetter (fun () -> new SqlConnection(conn))
        let distance _ = 2
        let m = loadData sqlConn zipCodes
        //let t = toStreetMetricListMap (Some m)
        let t = toStreetProjectedMap (Some m)
        let g (s : StreetCityState) = s.street.value
        let w = m |> Map.toList |> List.map snd |> List.concat |> List.distinct

        let q1 =
            {
                normalizeString = id
                projectString = halfPhoneticProjector >> toByteString
            }

        let p1 = ProjectedMap.ofList q1 g w

        let q2 =
            {
                normalizeString = id
                projectString = phoneticProjector >> toByteString
            }

        let p2 = ProjectedMap.ofList q2 g w

        let q3 =
            {
                normalizeString = id
                projectString = removeVowelsProjector >> toByteString
            }

        let p3 = ProjectedMap.ofList q3 g w

        let t = [ ("MetricListMap", t.tryFind); ("halfPhoneticProjector", p1.tryFind); ("phoneticProjector", p2.tryFind); ("removeVowelsProjector", p3.tryFind) ]

        let run (a, b, c) =
            //let n s = s |> defaultSplit |> List.sort |> concat
            let n = id

            let x =
                List.allPairs t [("Correct", n a); ("RemovedSpace", n b); ("ChangedOneChar", n c)]
                |> List.map (fun ((n, f), (s, e)) -> n, s, time (f distance) e)
                |> List.map (fun (n, s, (r, t)) -> n, s, t, r.Length)
                |> List.map (fun (n, s, t, r) -> (a, n, s, t, r))
                |> List.sortBy (fun (a, n, s, _, _) -> (a, n, s))

            x

        let results =
            streets100
            |> List.map run
            |> List.concat
            |> List.map (fun (a, n, s, t, r) -> $"%s{a},%s{n},%s{s},%12f{t},%4i{r}")
            |> String.concat "\n"

        let f = @"C:\Temp\TreeTest.csv"

        try
            File.WriteAllText(f, "StreetName,MapType,ErrType,RunTime,FoundCount\n" + results)
            $"Saved results into file '%s{f}'." |> output.WriteLine
        with
        | e -> $"Exception occurred trying to save file '%s{f}': '%A{e}'. The exception will be ignored." |> output.WriteLine


    [<Fact>]
    member _.runStreetRiverTest() =
        [
            "RIVER ST", "RIVER ST"
            "SILVER ST", "SILVER ST"
            "DOVER ST", "DOVER ST"
        ]
        |> runFuzzyStreetMatch "RIVERR ST" "RIV ST"


    [<Fact>]
    member _.runStreetRiverTest2() =
        [
            "RIVER STREET", "RIVER STREET"
            "SILVER ST", "SILVER ST"
            "DOVER ST", "DOVER ST"
        ]
        |> runFuzzyStreetMatch "RIVERR ST" "RIV ST"


    [<Fact>]
    member _.runStreetRidgebrookTest() =
        [
            "RIDGEBROOK RD", "RIDGEBROOK RD"
            "ELLARD RD", "ELLARD RD"
        ]
        |> runFuzzyStreetMatch "RDG BROOK RD" "RIDGEBROOK RD"


    [<Fact>]
    member _.runStreetRidgebrookTest2() =
        [
            "RIDGEBROOK RD", "RIDGEBROOK RD"
            "ELLARD RD", "ELLARD RD"
        ]
        |> runFuzzyStreetMatch "RDG BRK RD" "RIDGEBROOK RD"


    [<Fact>]
    member _.runStreetParkMeadowTest() =
        [
            "PARK MDW DR", "PARK MEADOW DR"
        ]
        |> runFuzzyStreetMatch "PARKMEADOW" "PARK MDW DR"


    [<Fact>]
    member _.runComparerParkMeadowTest() =
        let _ = runComparers "PARK MDW DR" "PARK MEADOW DR" "PARKMEADOW"
        0


    [<Fact>]
    member _.runComparerRidgebrookTest() =
        let _ = runComparers "RIDGEBROOK RD" "RIDGEBROOK RD" "RDG BROOK RD"
        0


    [<Fact>]
    member _.runComparerRidgebrookTest2() =
        let _ = runComparers "RIDGEBROOK RD" "RIDGEBROOK RD" "RDG BRK RD"
        0


    [<Fact>]
    member _.runComparerWesternAveTest() =
        let _ = runComparers "WESTERN AVE" "WESTERN AVE" "S WESTERN AVE"
        0


    [<Fact>]
    member _.runComparerRiverStreetTest() =
        let _ = runComparers "RIV ST" "RIVER ST" "RIVERR ST"
        0


    [<Fact>]
    member _.runComparerRiverStreetTest2() =
        let _ = runComparers "RIV RD" "RIVER RD" "RIVERR ST"
        0
