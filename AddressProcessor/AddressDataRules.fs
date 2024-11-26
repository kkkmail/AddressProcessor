namespace Softellect.AddressProcessor

open StringParser
open AddressTypes

module AddressDataRules =

    let cleanStringParams = CleanStringParams.defaultValue


    let suiteNames =
        SuiteName.allSuiteNames
        |> List.map (fun e -> " " + e)


    type AddressSource =
        | MD3
        | MD4
        | OA
        | Unknown


    type AddressData =
        {
            rowId : int
            streetCityState : StreetCityState
            zipCode : ZipCode
            occurrencCount : int
            source : AddressSource
        }
