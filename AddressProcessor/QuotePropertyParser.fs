namespace Softellect.AddressProcessor

open StringParser
open AddressTypes
open Dictionaries
open Extensions

module QuotePropertyParser =

    let cleanStreet (s : string) =
        s.Replace("½", " 1/2 ").Replace("-", " ")


    /// Tries to remove suite from the end of the address line if it is there.
    /// Returns original street line if it cannot find a suite name followed by a valid suite number.
    let tryRemoveSuite s =
        let x =
            match s |> List.tryFindIndexBack (fun e -> allSuiteNames |> Set.contains e) with
            | Some i ->
                let b, c = s |> List.splitAt i

                match c with
                | [] -> b
                | _ :: [] -> b
                | h :: t :: [] ->
                    match h = "#", UnitNumber.tryCreate [t] with
                    | true, _ -> b
                    | false, Some _ -> b
                    | _ -> s
                | h :: _ ->
                    match h = "#" with
                    | true -> b
                    | false -> s
            | None -> s

        x


    let tryRemoveHouseNumber s =
        let a =
            match s with
            | [] -> s
            | h :: [] -> s
            | h :: hh :: [] ->
                match Number.tryCreateRelaxed [h] with
                | Some _ -> [hh]
                | None -> s
            | h :: hh :: t ->
                match Number.tryCreateRelaxed [h], Number.tryCreateRelaxed [ h; hh ], hh = "1/2" with
                | _, Some _, true -> t
                | Some _, _, _ -> hh :: t
                | _ -> s

        a


    let tryRemoveSlashFromFirst s =
        let a =
            match s with
            | [] -> s
            | h :: t ->
                match h = "1/2", h.Contains "/" with
                | true, _ -> s
                | false, true -> t |> tryRemoveHouseNumber
                | _ -> s

        a


    let tryRemoveSlashFromLast s =
        let a =
            match s |> List.rev with
            | [] -> s
            | h :: t ->
                match h = "1/2", h.Contains "/" with
                | true, _ -> s
                | false, true ->
                    let b =
                        match h.IndexOf('/') with
                        | 0 -> EmptyString
                        | n -> h.Substring(0, n)
                    (b :: t) |> List.rev
                | _ -> s

        a


    /// Processes street line as exists in EFQuoteProperties and extracts a street name.
    let processStreetLine (s : string) =
        let a =
            s.ToUpper()
            |> cleanStreet
            |> defaultSplit
            |> tryRemoveSuite
            |> tryRemoveHouseNumber
            |> tryRemoveSlashFromFirst
            |> tryRemoveSlashFromLast

        a |> concat
