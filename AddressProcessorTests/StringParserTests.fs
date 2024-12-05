namespace Softellect.AddressProcessorTests

open Xunit
open Xunit.Abstractions
open FluentAssertions

open Softellect.AddressProcessor.StringParser
open Softellect.AddressProcessorTests.Primitives

/// !!! Always explicitly specify types of parameters in each test with InlineData !!!
/// Otherwise test discovery may hang up if the underlying functions change signatures.
/// While this looks like a bug in xUnit, we still need to make sure that the tests work.
type StringParserTests(output : ITestOutputHelper) =

    [<Theory>]
    [<InlineData("12-003", "12-003")>]
    [<InlineData("12-03", "12-03")>]
    [<InlineData("129-33", "129 131 133")>]
    [<InlineData("128-32", "128 130 132")>]
    [<InlineData("123-26", "123 126")>]
    [<InlineData("122-27", "122 127")>]
    [<InlineData("1234-6-9", "1234 1236 1239")>]

    // TODO kk:20191121 - This is a tough call. Current view is that this looks like a mistype and we just fill in the assumed missing digits.
    [<InlineData("1234-6-5", "1234 1236 1235")>]

    [<InlineData("1234-6-9-45", "1234 1236 1239 1245")>]
    [<InlineData("12-A", "12 A")>]
    [<InlineData("F-1", "F 1")>]
    [<InlineData("1901-1901-1915", "1901 1901 1915")>]
    [<InlineData("4321-1234", "4321 1234")>]
    member _.expandHyphen_ShouldWork (s : string, b : string) =
        let r = expandHyphen ExpandHyphenParams.defaultValue s
        r.Should().Be(b, EmptyBecause)


    [<Fact>]
    member _.expandHyphen_ShouldExpandLargeRange () =
        let s = "1234-5678"
        let r = expandHyphen ExpandHyphenParams.defaultValue s
        let c = String.concat " " [ for i in 1234..2..5678 -> i.ToString() ]
        r.Should().Be(c, EmptyBecause)


    [<Theory>]
    [<InlineData("1234", true)>]
    [<InlineData("ABC", false)>]
    [<InlineData("123456789", false)>]
    [<InlineData("1234 1/2", false)>]
    [<InlineData("1234 1 /2", false)>]
    [<InlineData("1234 1/ 2", false)>]
    [<InlineData("1234 1 / 2", false)>]
    [<InlineData("123456789 1/2", false)>]
    [<InlineData("N1W10", false)>]
    [<InlineData("N1W123456789", false)>]
    [<InlineData("1234A", true)>]
    [<InlineData("1234 A", true)>]
    [<InlineData("123456789A", false)>]
    [<InlineData("1234AB", false)>]
    member _.isRegularHouseNumber_ShouldWork (s : string, b : bool) = (isRegularHouseNumber s).Should().Be(b, EmptyBecause)


    [<Theory>]
    [<InlineData("1234", false)>]
    [<InlineData("ABC", false)>]
    [<InlineData("123456789", false)>]
    [<InlineData("1234 1/2", false)>]
    [<InlineData("1234 1 /2", false)>]
    [<InlineData("1234 1/ 2", false)>]
    [<InlineData("1234 1 / 2", false)>]
    [<InlineData("123456789 1/2", false)>]
    [<InlineData("N1W10", true)>]
    [<InlineData("N1W123456789", false)>]
    [<InlineData("1234A", false)>]
    [<InlineData("1234 A", false)>]
    [<InlineData("123456789A", false)>]
    [<InlineData("1234AB", false)>]
    member _.isGridHouseNumber_ShouldWork (s : string, b : bool) = (isGridHouseNumber s).Should().Be(b, EmptyBecause)


    [<Theory>]
    [<InlineData("1234", false)>]
    [<InlineData("ABC", false)>]
    [<InlineData("123456789", false)>]
    [<InlineData("1234 1/2", true)>]
    [<InlineData("1234 1 /2", true)>]
    [<InlineData("1234 1/ 2", true)>]
    [<InlineData("1234 1 / 2", true)>]
    [<InlineData("123456789 1/2", false)>]
    [<InlineData("N1W10", false)>]
    [<InlineData("N1W123456789", false)>]
    [<InlineData("1234A", false)>]
    [<InlineData("1234 A", false)>]
    [<InlineData("123456789A", false)>]
    [<InlineData("1234AB", false)>]
    member _.isHalfIntegerHouseNumber_ShouldWork (s : string, b : bool) = (isHalfIntegerHouseNumber s).Should().Be(b, EmptyBecause)


    [<Theory>]
    [<InlineData("1234", "1234")>]
    [<InlineData("12.34", "12.34")>]
    [<InlineData("1234 1/2", "1234 1/2")>]
    [<InlineData("1234A", "1234 A")>]
    [<InlineData("F1", "F 1")>]
    [<InlineData("F-1", "F-1")>]
    member _.splitAlphaNum_ShouldWork (s : string, b : string) =
        let r = splitAlphaNum s
        r.Should().Be(b, EmptyBecause)


    [<Theory>]
    [<InlineData("123 COUNTY RD", "123 CR")>]
    [<InlineData("123 COUNTY RD NEW YORK", "123 CR NEW YORK")>]
    [<InlineData("123 COUNTY DR", "123 COUNTY DR")>]
    [<InlineData("123 COUNTY DR NEW YORK", "123 COUNTY DR NEW YORK")>]
    [<InlineData("123 COUNTY ROAD", "123 CR")>]
    [<InlineData("123 FARM TO MARKET", "123 FM")>]
    [<InlineData("123 FARM TO MARKET RD", "123 FM")>]
    [<InlineData("123 FARM TO MARKET ROAD", "123 FM")>]
    [<InlineData("123 MAIN ST", "123 MAIN ST")>]
    member _.replaceGroups_ShouldWork (s : string, d : string) =
        let a = defaultSplit s
        let b = replaceGroups standardGroups a
        let c = seqToString b
        c.Should().Be(d, EmptyBecause)
