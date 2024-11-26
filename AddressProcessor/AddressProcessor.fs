namespace Softellect.AddressProcessor

open CSharpInterop
open Extensions
open StringParser
open AddressProcessorRules

type AddressProcessor (d : SqlConnectionData) =
    let mapData = getMapData d.ratingConnectionGetter


    member _.parse (r : AddressProcessorParseRequest) : AddressProcessorResult[] =
        let a =
            {
                connectionData = d
                mapData = mapData
                request = r
            }
            |> parseImpl
        a

    member _.standardize (s : string) : string = s |> standardizeString CleanStringParams.defaultValue
    member _.levenshtein (a : string) (b : string) : float = levenshtein a b
