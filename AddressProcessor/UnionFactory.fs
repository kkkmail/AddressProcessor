namespace Softellect.AddressProcessor
    open System.Collections.Generic
    open StringParser

    type CaseFactory<'C, 'L, 'K when 'C : comparison and 'L : comparison and 'K : comparison> =
        {
            caseValue : 'C
            label : 'L
            key : 'K
        }


    type CaseFactory<'C, 'L when 'C : comparison and 'L : comparison> = CaseFactory< 'C, 'L, 'L>


    type UnionFactory< 'C, 'L, 'K when 'C : comparison and 'L : comparison and 'K : comparison> (all : array<'C * 'L * 'K> ) =
        let map = all |> Array.map (fun (c, l, _) -> (c, l)) |> Map.ofArray
        let mapRev = all |> Array.map (fun (c, l, _) -> (l, c)) |> Map.ofArray
        let mapVal = all |> Array.map (fun (c, _, k) -> (c, k)) |> Map.ofArray
        let mapValRev = all |> Array.map (fun (c, _, k) -> (k, c)) |> Map.ofArray

        let allListValue : System.Collections.Generic.List<CaseFactory< 'C, 'L, 'K>> =
            let x =
                all
                |> List.ofArray
                |> List.sortBy (fun (c, _, _) -> c)
                |> List.map (fun (c, l, k) -> { caseValue = c; label = l; key = k } : CaseFactory< 'C, 'L, 'K>)
            new System.Collections.Generic.List<CaseFactory<'C, 'L, 'K>> (x)

        let allSortedListValue : System.Collections.Generic.List<CaseFactory<'C, 'L, 'K>> =
            let x =
                all
                |> List.ofArray
                |> List.sortBy (fun (_, l, _) -> l)
                |> List.map (fun (c, l, k) -> { caseValue = c; label = l; key = k } : CaseFactory<'C, 'L, 'K>)
            new System.Collections.Generic.List<CaseFactory<'C, 'L, 'K>> (x)

        let allCasesValue = all |> Array.map (fun (c, _, _) -> c)

        [<CompiledName("All")>]
        member this.all : array<'C * 'L * 'K> = all

        [<CompiledName("AllCases")>]
        member this.allCases : array<'C> = allCasesValue

        //Use key, like cip code "54.0105", NOT descriptions like cip description "Public/Applied History" to create.
        [<CompiledName("FromKey")>]
        member this.fromKey (k : 'K) : 'C = mapValRev.Item (k)

        // For integer keys you can pass "1" instead of 1
        [<CompiledName("FromKeyString")>]
        member this.fromKeyString (s : string) : 'C = this.fromKey (convert s)

         //Use key, like cip code "54.0105", NOT descriptions like cip description "Public/Applied History" to create.
        [<CompiledName("TryFromKey")>]
        member this.tryFromKey (k : 'K) : 'C option = mapValRev.TryFind k

        [<CompiledName("TryFromKeyString")>]
        member this.tryFromKeyString (s : string) : 'C option = mapValRev.TryFind (convert s)

         //Use key, like cip code "54.0105", NOT descriptions like cip description "Public/Applied History" to create.
        [<CompiledName("TryFromKey")>]
        member this.tryFromKey (k : 'K option) : 'C option =
            match k with
            | Some x -> this.tryFromKey x
            | None -> None

        [<CompiledName("AllList")>]
        member this.allList : System.Collections.Generic.List< CaseFactory<'C, 'L, 'K>> = allListValue

        [<CompiledName("AllSortedList")>]
        member this.allSortedList : System.Collections.Generic.List< CaseFactory<'C, 'L, 'K>> = allSortedListValue

        [<CompiledName("FromLabel")>]
        member this.fromLabel (label : 'L) : 'C = mapRev.[label]

        [<CompiledName("TryFromLabel")>]
        member this.tryFromLabel (label : 'L) : 'C option = mapRev.TryFind label

        [<CompiledName("TryFromLabel")>]
        member this.tryFromLabelOpt (label : 'L option) : 'C option =
            match label with
            | Some x -> this.tryFromLabel x
            | None -> None

        [<CompiledName("GetLabel")>]
        member this.getLabel (c : 'C) : 'L = map.[c]

        [<CompiledName("TryGetLabel")>]
        member this.tryGetLabel (co : 'C option) : 'L option =
            match co with
            | Some c -> this.getLabel c |> Some
            | None -> None

        [<CompiledName("GetKey")>]
        member this.getKey (c : 'C) : 'K = mapVal.[c]

        [<CompiledName("TryGetKey")>]
        member this.tryGetKey (co : 'C option) : 'K option =
            match co with
            | Some c -> this.getKey c |> Some
            | None -> None


    // DU factory: 'C - case, 'L - string label, and there is no separate key
    type UnionFactory<'C, 'L when 'C : comparison and 'L : comparison> (all : array<'C * 'L> ) =
        inherit UnionFactory<'C, 'L, 'L> (all |> Array.map (fun (c, l) -> (c, l, l)))


    // DU factory: 'C - case, 'L - usually string label, 'K - key, 'S - short label
    type UnionFactory< 'C, 'L, 'K, 'S when 'C : comparison and 'L : comparison and 'K : comparison and 'S : comparison> (all : array<'C * 'L * 'K * 'S> ) =
        inherit UnionFactory<'C, 'L, 'K> (all |> Array.map (fun (c, l, k, _) -> (c, l, k)))

        let mapShort = all |> Array.map (fun (c, _, _, s) -> (c, s)) |> Map.ofArray
        let mapShortRev = all |> Array.map (fun (c, _, _, s) -> (s, c)) |> Map.ofArray

        let shortListValue : System.Collections.Generic.List<'S> =
            let x =
                all
                |> List.ofArray
                |> List.sortBy (fun (c, _, _, _) -> c)
                |> List.map (fun (_, _, _, s) -> s)
            new System.Collections.Generic.List<'S> (x)

        [<CompiledName("All")>]
        member this.allWithShort : array<'C * 'L * 'K * 'S> = all

        [<CompiledName("FromShort")>]
        member this.fromShort (short : 'S) : 'C = mapShortRev.[short]

        [<CompiledName("TryFromShort")>]
        member this.tryFromShort (short : 'S) : 'C option = mapShortRev.TryFind short

        [<CompiledName("TryFromShort")>]
        member this.tryFromShort (short : 'S option) : 'C option =
            match short with
            | Some x -> this.tryFromShort x
            | None -> None

        [<CompiledName("GetShort")>]
        member this.getShort (c : 'C) : 'S = mapShort.[c]

        [<CompiledName("ShortList")>]
        member this.shortList : System.Collections.Generic.List<'S> = shortListValue
