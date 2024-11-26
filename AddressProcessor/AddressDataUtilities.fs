namespace Softellect.AddressProcessor

open DataUpdater

module AddressDataUtilities =

    type ErrUpdater() =
        interface IUpdater<int, list<int>> with
            member __.init () = []
            member __.remove p m = m // cannot remove
            member __.update p m = p :: m


    let allErr = new AsyncUpdater<int, list<int>>(ErrUpdater ())
    let printErr logger = allErr.getContent() |> List.rev |> List.map (fun e -> logger (e.ToString())) |> ignore
