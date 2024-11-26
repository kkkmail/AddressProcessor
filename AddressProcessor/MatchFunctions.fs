namespace Softellect.AddressProcessor

open MatchTypes

module MatchFunctions =

    /// Matches m with beginning of i.
    /// Returns tail of i remaining after matching (if matches).
    let rec matchLists (i : list<string>) (m : list<string>) =
        match i, m with
        | _, [] -> Some i
        | [], _ :: _ -> None
        | hi :: ti, hm :: tm ->
            match hi = hm with
            | true -> matchLists ti tm
            | false -> None


    /// This function is precise. Imprecise functions should be built either on top of it or similarly if fuzzy string comparision is needed.
    /// Finds match score of i and m at the end.
    /// We are interested in matching by m.
    /// Returns (elements before match, elements after match) option.
    let tryMatchList (i : list<string>) (m : list<string>) =
        let rec inner (b : list<string>) (i : list<string>) (m : list<string>) =
            match matchLists i m with
            | Some a -> Some (b |> List.rev, a) // Found a match - return what's before and what's after.
            | None ->
                match i with
                | [] -> None // Did not find any match
                | h :: t -> inner (h :: b) t m // Retry by shifting one element in i.

        inner [] i m


    /// Tries to match the beginning of the list with the set (the order is ignored).
    /// Returns remaining list (after removing all that matched)
    /// and remaining set (after removing all that DID NOT match).
    let matchSet (m : Set<string>) (i : list<string>) =
        let rec inner  (m : Set<string>) (i : list<string>) =
            //printfn "matchSet::inner::m = %A, i = %A" m i
            match i with
            | [] -> m, i
            | h :: t ->
                match m.Contains h with
                | true -> inner (m.Remove h) t
                | false -> m, i

        inner m i


    /// Tries to match the beginning of list with the set.
    /// Returns remaining list and match score.
    let getMatchScore (s : Set<string>) (u : list<string>) =
        match s.IsEmpty with
        | true -> u, MatchResult.Perfect
        | false ->
            let sr, ur = matchSet s u

            match sr = s with
            | true -> u, MatchResult.Failed
            | false ->
                let r =
                    match sr.IsEmpty with
                    | true -> MatchResult.Perfect
                    | false -> (float sr.Count) / (float s.Count) |> MatchResult.Partial
                ur, r


    /// Tries to match set with the sliding window.
    /// Stops at perfect match. If there are no perfect matches, then return best match along with skipped and a tail.
    let tryFindMatch (s : Set<string>) (u : list<string>) =
        //printfn "tryFindMatch::s = %A, u = %A" s u
        let rec inner (s : Set<string>) (u : list<string>) (skip : list<string>) (acc) =
            //printfn "tryFindMatch::inner::s = %A, u = %A, skip = %A, acc = %A" s u skip acc
            match getMatchScore s u with
            | ur, MatchResult.Perfect -> [ MatchResult.Perfect, skip, ur ]
            | ur, MatchResult.Failed ->
                match ur with
                | [] -> acc
                | h :: t -> inner s t (h :: skip) acc
            | ur, r ->
                match ur with
                | [] -> acc
                | h :: t -> (r, skip, ur) :: (inner s t (h :: skip) acc)

        match inner s u [] [] |> List.sortBy (fun (r, _, _) -> r) |> List.tryHead with
        | Some e -> e
        | None -> MatchResult.Failed, [], u
