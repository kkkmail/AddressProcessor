namespace Softellect.AddressProcessor

open Configuration
open DataUtilities
open StringParser

module Dictionaries =

    let getAllStreetAbbr (input : StreetAbbr[]) =
        let common = input |> Array.map (fun e -> e.common, e.caseValue)
        let primary = input |> Array.map (fun e -> e.primary, e.caseValue)
        let standard = input |> Array.map (fun e -> e.standard, e.caseValue)
        Array.concat [ common; primary; standard ]
        |> Map.ofArray


    let getDirectionMap (input : DirectionAbbr[]) =
        let common = input |> Array.map (fun e -> e.common, e.caseValue)
        let standard = input |> Array.map (fun e -> e.standard, e.caseValue)
        Array.concat [ common; standard ] |> Map.ofArray


    let allStreetAbbrMap = StreetAbbr.all |> getAllStreetAbbr
    let allDirectionMap = DirectionAbbr.all |> getDirectionMap
    let allStandardGroups = standardGroups |> List.map snd |> List.map concat |> Set.ofList


    let allZipStates =
        openZippedStreamFromDll ZipStateZipFileName ZipStateName
        |> readZipStates


    let allSuiteNames = SuiteName.allSuiteNames |> Set.ofList
