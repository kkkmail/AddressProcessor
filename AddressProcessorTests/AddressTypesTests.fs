namespace Softellect.AddressProcessorTests

open Xunit
open Xunit.Abstractions
open FluentAssertions
open Softellect.AddressProcessor.AddressTypes
open Softellect.AddressProcessor.Extensions
open Softellect.AddressProcessor.DataParsing
open Softellect.AddressProcessor.StringParser
open Softellect.AddressProcessor.QuotePropertyParser
open Softellect.AddressProcessorTests.Primitives

/// !!! Always explicitly specify types of parameters in each test with InlineData !!!
/// Otherwise test discovery may hang up if the underlying functions change signatures.
/// While this looks like a bug in xUnit, we still need to make sure that the tests work.
type AddressTypesTests(output : ITestOutputHelper) =

    let getInferenceTypes_ShouldWork o e s =
        let f l = l |> List.map (fun e -> Some e, e)
        let r = getInferenceTypes (elevate o) (f e)
        s.Should().Be(r |> List.map (fun (_, v) -> v), EmptyBecause)


    [<Fact>]
    member _.toValidSortedSubLists_ShouldWorkFor_FortGreenePl () =
        let x =
            [
                "FT"
                "GREENE"
                "PL"
            ]
            |> toValidSortedSubLists ToSublistsParams.wordsOnlyValue

        x.Count.Should().Be(4, EmptyBecause)


    [<Fact>]
    member _.toValidSortedSubLists_ShouldWorkFor_FortGreenePl2 () =
        let x =
            [
                "FT"
                "GREENE"
                "PL"
            ]
            |> toValidSortedSubLists ToSublistsParams.exactMatchValue

        x.Count.Should().Be(8, EmptyBecause)

    // TODO kk:20191122 - Does not work yet.
    //[<Theory>]
    //[<InlineData("WAREHAM ST PH2", "WAREHAM ST")>]
    //[<InlineData("7 AVE 10EF", "7 AVE")>]
    //[<InlineData("US HWY 50", "US HWY 50")>]
    //[<InlineData("S 1 ST", "S 1 ST")>]
    //[<InlineData("STATE HWY 36 W", "STATE HWY 36 W")>]
    //[<InlineData("STATE HIGHWAY 36 W", "STATE HIGHWAY 36 W")>]
    //member _.cleanUnitNumberFromStreetName_ShouldRemoveUnitNumber (s, b) =
    //    let r = cleanUnitNumberFromStreetName s
    //    r.Should().Be b


    [<Theory>]
    [<InlineData("108 ST S", "108TH ST S", "108 ST S")>]
    [<InlineData("108TH ST S", "108TH ST S", "108 ST S")>]
    [<InlineData("108 TH ST S", "108TH ST S", "108 ST S")>]
    [<InlineData("1ST ST S", "1ST ST S", "1 ST S")>]
    [<InlineData("1ST AVE S", "1ST AVE S", "1 AVE S")>]
    [<InlineData("1 ST AVE S", "1 ST AVE S", "1 ST AVE S")>] // Cannot be fixed by this function.
    member _.Street_tryCreateCleaned_ShouldWork (s : string, o : string, c : string) =
        let a = Street.tryCreateCleaned cleanStringParams (s, Some o)

        match a with
        | Some v -> v.value.Should().Be(c, EmptyBecause)
        | None -> failwith $"Unable to create street for input data: '%s{s}'."


    [<Theory>]
    [<InlineData("SAINT JOHNS", "SAINT JOHNS", "ST JOHNS")>]
    member _.City_tryCreate_ShouldWork (s : string, o : string, c : string) =
        let a = City.tryCreate cleanStringParams (s, Some o)

        match a with
        | Some v -> v.value.Should().Be(c, EmptyBecause)
        | None -> failwith $"Unable to create city for input data: '%s{s}'."


    [<Theory>]
    [<InlineData("N 1234", "N1234")>]
    [<InlineData("N 12 W 3456", "N12W3456")>]
    [<InlineData("W 1234", "W1234")>]
    [<InlineData("W 12 N 3456", "W12N3456")>]
    [<InlineData("S 1234", "S1234")>]
    [<InlineData("S 123 W 45678", "S123W45678")>]
    [<InlineData("15 W 044", "15W044")>]
    [<InlineData("3 N 063", "3N063")>]
    member _.Number_tryCreate_ShouldWork (s : string, c : string) =
        match s |> defaultSplit |> Number.tryCreate with
        | Some v -> v.value.Should().Be(c, EmptyBecause)
        | None -> failwith $"Unable to create house number for input data: '%s{s}'."


    [<Fact>]
    member _.getInferenceTypes_ShouldWork_01 () =
        let o = [ 1 ]
        let e = [ 1 ]
        let s = [ ExplicitAddress ]
        getInferenceTypes_ShouldWork o e s


    [<Fact>]
    member _.getInferenceTypes_ShouldWork_02 () =
        let o = [ 1; 25 ]
        let e = [ 1; 25 ]
        let s = [ ExplicitAddress; ExplicitAddress ]
        getInferenceTypes_ShouldWork o e s


    [<Fact>]
    member _.getInferenceTypes_ShouldWork_03 () =
        let o = [ 1; 11 ]
        let e = [ 1; 3; 5; 7; 9; 11 ]
        let s = [ ExplicitAddress; ImplicitEvenOrOddAddress; ImplicitEvenOrOddAddress; ImplicitEvenOrOddAddress; ImplicitEvenOrOddAddress; ExplicitAddress ]
        getInferenceTypes_ShouldWork o e s


    [<Fact>]
    member _.getInferenceTypes_ShouldWork_04 () =
        let o = [ 2; 12 ]
        let e = [ 2; 4; 6; 8; 10; 12 ]
        let s = [ ExplicitAddress; ImplicitEvenOrOddAddress; ImplicitEvenOrOddAddress; ImplicitEvenOrOddAddress; ImplicitEvenOrOddAddress; ExplicitAddress ]
        getInferenceTypes_ShouldWork o e s


    [<Fact>]
    member _.getInferenceTypes_ShouldWork_05 () =
        let o = [ 1; 5; 6 ]
        let e = [ 1; 3; 5; 6 ]
        let s = [ ExplicitAddress; ImplicitEvenOrOddAddress; ExplicitAddress; ExplicitAddress ]
        getInferenceTypes_ShouldWork o e s


    [<Fact>]
    member _.getInferenceTypes_ShouldWork_06 () =
        let o = [ 1; 5 ]
        let e = [ 1; 2; 3; 4; 5 ]
        let s = [ ExplicitAddress; ImplicitAllAddress; ImplicitAllAddress; ImplicitAllAddress; ExplicitAddress ]
        getInferenceTypes_ShouldWork o e s


    [<Fact>]
    member _.getInferenceTypes_ShouldWork_07 () =
        let o = [ 1; 5; 6; 9 ]
        let e = [ 1; 3; 5; 6; 7; 8; 9 ]
        let s = [ ExplicitAddress; ImplicitEvenOrOddAddress; ExplicitAddress; ExplicitAddress; ImplicitAllAddress; ImplicitAllAddress; ExplicitAddress ]
        getInferenceTypes_ShouldWork o e s


    [<Fact>]
    member _.getInferenceTypes_ShouldWork_08 () =
        let o = [ 1 ]
        let e = [ 1; 2 ]
        let s = [ ImplicitErrAddress; ImplicitErrAddress ]
        getInferenceTypes_ShouldWork o e s


    [<Fact>]
    member _.getInferenceTypes_ShouldWork_09 () =
        let o = [ 1; 2 ]
        let e = [ 1; 3 ]
        let s = [ ImplicitErrAddress; ImplicitErrAddress ]
        getInferenceTypes_ShouldWork o e s


    [<Fact>]
    member _.getInferenceTypes_ShouldWork_10 () =
        let o = [ 2; 3 ]
        let e = [ 1; 2 ]
        let s = [ ImplicitErrAddress; ImplicitErrAddress ]
        getInferenceTypes_ShouldWork o e s


    /// TODO kk:20200123 - Add ticket with address like "1518 State Route 197, Saint Marys OH 45885". The road is "OH-197".
    /// Basically allow "0" at the beginning of house number. This is rare but possible.
    ///
    /// All these street lines were taken from seeded EFQuoteProperties.
    [<Theory>]
    [<InlineData("10 1/2 South St", "SOUTH ST")>]
    [<InlineData("1003 1/2 N 1st St", "N 1ST ST")>]
    [<InlineData("1010 12th 1/2 St N", "12TH 1/2 ST N")>]
    [<InlineData("1014 1/2 1st St SE", "1ST ST SE")>]
    [<InlineData("1031 1/2 Liberty St # 1031", "LIBERTY ST")>]
    [<InlineData("1203 State St # 1/2/2015", "STATE ST")>]
    [<InlineData("1231/2 -123 E Hicks St", "E HICKS ST")>]
    [<InlineData("14320 Avenue 23 1/2", "AVENUE 23 1/2")>]
    [<InlineData("209 N Columbia St # 1/2", "N COLUMBIA ST")>]
    [<InlineData("26 Charles St # 26 1/2", "CHARLES ST")>]
    [<InlineData("3261 Us Highway 441/27", "US HIGHWAY 441")>]
    [<InlineData("4525 County Road Ff 1/2", "COUNTY ROAD FF 1/2")>]
    [<InlineData("4525 County Road Ff ½", "COUNTY ROAD FF 1/2")>]
    [<InlineData("26w341 Inwood Ln", "INWOOD LN")>]
    [<InlineData("05920 68th St # 2", "68TH ST")>]
    [<InlineData("1 Woodbury Blvd # Un102", "WOODBURY BLVD")>]
    [<InlineData("100 Engert Ave # P10", "ENGERT AVE")>]
    [<InlineData("10343 E County Highway 30a # C110", "E COUNTY HIGHWAY 30A")>]
    [<InlineData("11355 Woods Opossum Run # R", "WOODS OPOSSUM RUN")>]
    [<InlineData("2 Stonington Hill Rd Apt I", "STONINGTON HILL RD")>]
    [<InlineData("5484 Castillo De Rosas", "CASTILLO DE ROSAS")>]
    [<InlineData("8937 Sr 3001", "SR 3001")>]
    [<InlineData("4300 W Main St Ste 43", "W MAIN ST")>]
    [<InlineData("100 Sunrise Ave Ph 1", "SUNRISE AVE")>]
    [<InlineData("1 Renaissance Sq Unit 11b", "RENAISSANCE SQ")>]
    [<InlineData("130 River Landing Dr Unit 12d", "RIVER LANDING DR")>]
    [<InlineData("15 Hargrove Ln Unit 4j", "HARGROVE LN")>]
    [<InlineData("N110w16867 Ashbury Cir Unit 2", "ASHBURY CIR")>]
    [<InlineData("91 Screven St Unit E314", "SCREVEN ST")>]
    [<InlineData("4100 Ben Ficklin Rd Lot 24", "BEN FICKLIN RD")>]
    member _.parseStreetLine_ShouldWork (s : string, b : string) = (processStreetLine s).Should().Be(b, EmptyBecause)
