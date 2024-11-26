namespace Softellect.AddressProcessor

open AddressTypes
open MatchTypes
open MatchingRules

module NewAddressRules =

    let addressFailed m = { m with result = Failed; matchError = Some InvalidAddress }


    let addAddress m =
        {
            m with
                resolved = { resolvedAddress = m.address; resolvedResult = m.result; resolvedStepResults = m.stepResults } :: m.resolved
                address = Address.defaultValue
                matchError = None
                allSkipped = m.currentSkipped @ m.allSkipped
                currentSkipped = []
        }


    let addEmptyAddress m =
        {
            m with
                address = Address.defaultValue
                matchError = None
                allSkipped = m.currentSkipped @ m.allSkipped
                currentSkipped = []
        }


    let newAddressRule (ri, m) =
        let retVal =
            match m.result with
            | Failed -> failed (ri, m)
            | _ ->
                match m.address.isEmpty with
                | true -> ri, addEmptyAddress m
                | false ->
                    match m.address.isValid with
                    | true ->
                        match m.resolved with
                        | [] -> ri, addAddress m
                        | h :: _ ->
                            match h.resolvedAddress = m.address with
                            | false -> ri, addAddress m
                            | true -> ri, m
                    | false -> ri, addressFailed m

        retVal
