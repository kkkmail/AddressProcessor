namespace Softellect.AddressProcessorTests

open Xunit
open Xunit.Abstractions
open FluentAssertions

open Softellect.AddressProcessor
open Softellect.AddressProcessor.CSharpInterop
open Softellect.AddressProcessor.StringParser
open Softellect.AddressProcessor.MatchParams
open Softellect.AddressProcessor.AddressProcessorRules
open Softellect.AddressProcessor.MatchTypes
open Softellect.AddressProcessorTests.Primitives

open Microsoft.EntityFrameworkCore
open Microsoft.Data.SqlClient

/// !!! Always explicitly specify types of parameters in each test with InlineData !!!
/// Otherwise test discovery may hang up if the underlying functions change signatures.
/// While this looks like a bug in xUnit, we still need to make sure that the tests work.
type AddressProcessorTests(output : ITestOutputHelper) =


    member private _.getConn() =
        let conn = failwith "getConn is not implemented"
        new SqlConnection(conn)


    member private t.getSqlConnectionData() =
        {
            getCoreConn = t.getConn
            getRatingConn = t.getConn
        }


    member private t.getAddressProcessor() = t.getSqlConnectionData() |> AddressProcessor


    /// If you update InputParams here make sure that you match them in:
    ///     Softellect.AddressProcessor.AddressProcessorService.ParseUnvalidatedAddressOrDefault.
    member private t.runParseAddress address =
        let d = t.getSqlConnectionData()
        let mapData = getMapData d.ratingConnectionGetter

        let r =
            {
                AddressString = address
                ParseParams =
                    {
                        InputParams =
                            {
                                RemoveUnitNumber = true
                                UseAbbreviatedNames = false
                                IgnoreStreetNotFound = true
                                DoSimpleParse = true
                                ExpandHyphen = true
                                DoFuzzySearch = false
                            }
                        OutputParams =
                            {
                                GetAddressKey = false
                                GetAddressInferenceType = false
                            }
                    }
            }

        let p = CleanStringParams.defaultValue
        let input = toInput p r.AddressString
        let removeUnitNumber = r.ParseParams.InputParams.RemoveUnitNumber

        // kk:20220609 - Do not delete commented code.
        let rules =
            [
//                getRulesSimple mapData MatchParams.mediumValue
//                getStreetNotFoundRules mapData removeUnitNumber MatchParams.strictValue
//                getRulesNoCity mapData removeUnitNumber MatchParams.strictSkippedCityValue
                getRulesNoStreetNoCity mapData removeUnitNumber MatchParams.strictSkippedCityValue
            ]

        let result = parseAddressImpl rules input
        result.matchError.Should().Be(None, EmptyBecause)


    [<Fact>]
    member t.parseScottRoad_ShouldSucceed() = t.runParseAddress "14632 Scott Rd, WINTER GARDEN FL 32830"


    [<Fact>]
    member t.parseLakeScene_ShouldSucceed() = t.runParseAddress "4831 LAKE SCENE PL, SARASOTA FL 34243"
