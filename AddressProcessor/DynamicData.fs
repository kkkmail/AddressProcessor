namespace Softellect.AddressProcessor

open DataUpdater
open AddressTypes
open DataParsing
open Extensions
open StringParser
open Swyfft.Services.AddressProcessor
open MetricTreeInterop

/// kk:20200211 - Do not remove commented out code here. It is used to swith between MetricListMap and ProjectedMap.
module DynamicData =

    type ZipCodeCityMap = Map<ZipCode, list<State * City>>
    type ZipCodeCityMetricMap = Map<ZipCode, State * MetricStringMap<City>>

    type ZipMap = Map<ZipCode, Map<list<string>, list<StreetCityState>>>
    type ZipMetricMap = Map<ZipCode, MetricListMap<StreetCityState>>
    //type ZipMetricMap = Map<ZipCode, ProjectedMap<StreetCityState>>

    type StateCityMap = Map<State * City, Map<list<string>, list<StreetCityState>>>
    type StateCityMetricMap = Map<State * City, MetricListMap<StreetCityState>>
    //type StateCityMetricMap = Map<State * City, ProjectedMap<StreetCityState>>

    type ZipToCityMap = Map<ZipCode, Map<list<string>, list<City>>>
    type ZipToCityMetricMap = Map<ZipCode, MetricListMap<City>>
    //type ZipToCityMetricMap = Map<ZipCode, ProjectedMap<City>>

    type StateToCityMap = Map<State, Map<list<string>, list<City>>>
    type StreetZipDetailedMap = Map<StreetZipKey, StreetZipInfo>
    type CountyMap = Map<int, County>
    type CountyNameMap = Map<string * State, County>
    type ZipCodeSelectMap = Map<StreetCityState, list<ZipCode>>
    type CitySelectMap = Map<StreetCityState * ZipCode, list<City>>


    let logLoadingErr s e = printfn $"%A{s}::%A{e}"
    let cleanStringParams = CleanStringParams.defaultValue
    let groupBy x = x |> List.groupBy fst|> List.map (fun (a, b) -> a, b |> List.map snd)
    let toStreetValue (x : Street) = x.value
    let toStreetOriginalValue (x : Street) = x.originalValue
    let toCityValue (x : City) = x.value
    let toCityOriginalValue (x : City) = x.originalValue
    let toStreet (x : StreetCityState) = x.street
    let toCity (x : City) = x

    let chooser x =
        x
        |> List.groupBy (fun (s, _) -> s.state, s.city.value, s.street.value)
        |> List.map (fun (_, v) -> v |> List.sortByDescending snd |> List.tryHead)
        |> List.choose id
        |> List.map fst


    let toStateCityMap t a =
        a
        |> List.map (fun (_, (s, i)) -> (s.state, s.city), (s, i))
        |> groupBy
        |> List.map (fun (k, v) -> k, v |> chooser |> mapStreetCityState t)
        |> Map.ofList


    let loadZipCodeCityMapData conn =
        let zipCodeCities = loadZipCodeCities conn

        zipCodeCities
        |> List.map (fun r ->
                let zo = r.ZipCode |> ZipCode.tryCreate
                let so = r.StateCode |> State.tryCreate
                let co = (r.City, Some r.CityOriginalName) |> City.tryCreate CleanStringParams.defaultValue
                match zo, so, co with | Some z, Some s, Some c -> Some (z, (s, c)) | _ -> None )
        |> List.choose id
        |> groupBy
        |> Map.ofList


    type ZipCodeCityMapUpdater (getConn) =
        interface IUpdater<unit, ZipCodeCityMap> with
            member __.init () = Map.empty
            member __.remove _ _ = Map.empty

            member __.update _ m =
                try
                    match m.IsEmpty with
                    | true -> loadZipCodeCityMapData getConn
                    | false -> m
                with
                | ex ->
                    logLoadingErr "ZipCodeCityMapUpdater.update" ex
                    m


    type ZipCodeCityMetricMapUpdater (getConn) =
        interface IUpdater<unit, ZipCodeCityMetricMap> with
            member __.init () = Map.empty
            member __.remove _ _ = Map.empty

            member __.update _ m =
                try
                    match m.IsEmpty with
                    | true ->
                        let map =
                            loadZipCodeCityMapData getConn
                            |> Map.toList
                            |> List.map (fun (z, e) -> z, e |> List.unzip)
                            |> List.map (fun (z, (s, c)) -> z, s |> List.countBy id |> List.sortByDescending (fun (_, i) -> i) |> List.map fst |> List.tryHead, c)
                            |> List.map (fun (z, s, c) -> s |> Option.bind (fun st -> Some (z, st, c)))
                            |> List.choose id
                            |> List.map (fun (z, s, c) -> (z, (s, c |> List.map (fun e -> e.value, e) |> Map.ofList |> MetricStringMap.ofStringMap)))
                            |> Map.ofList

                        map
                    | false -> m
                with
                | ex ->
                    logLoadingErr "ZipCodeCityMetricMapUpdater.update" ex
                    m


    let loadZipMapData conn t p =
        loadStreetZipTblForZip conn p
        |> processStreetZipTbl
        |> groupBy
        |> List.tryPick (fun (z, e) -> match z = p with | true -> Some (e |> chooser |> mapStreetCityState t) | false -> None)


    type ZipMapUpdater (getConn) =
        interface IUpdater<ZipCode, ZipMap> with
            member __.init () = Map.empty
            member __.remove p m = m.Remove p

            member __.update p m =
                try
                    match m.ContainsKey p with
                    | true -> m
                    | false ->
                        match loadZipMapData getConn ToSublistsParams.exactMatchValue p with
                        | Some x -> m.Add (p, x)
                        | None -> m.Add (p, Map.empty)
                with
                | ex ->
                    logLoadingErr "ZipMapUpdater.update" ex
                    m

    let private toMap g h w =
        w
        |> Option.defaultValue Map.empty
        |> Map.toList
        |> List.map (fun (_, v) -> v)
        |> List.concat
        |> List.distinct
        |> List.allPairs g
        |> List.map (fun (f, e) -> (h >> f) e, e)
        |> List.distinct
        |> List.groupBy (fun (a, _) -> a)
        |> List.map (fun (a, b) -> a, b |> List.map (fun (_, e) -> e) |> List.distinct)
        |> Map.ofList


    let toMetricListMap g h p w = toMap g h w |> MetricListMap.ofStringMap p

    let toStreetMetricListMap : Map<list<string>, list<StreetCityState>> option -> MetricListMap<StreetCityState> =
        toMetricListMap [ toStreetValue; toStreetOriginalValue ] toStreet removeSpaceProjector

    let toCityMetricListMap : Map<list<string>, list<City>> option -> MetricListMap<City> =
        toMetricListMap [ toCityValue; toCityOriginalValue ] toCity removeSpaceProjector


    let toProjectedMap g h p w = toMap g h w |> withPhoneticProjector p

    let toStreetProjectedMap : Map<list<string>, list<StreetCityState>> option -> ProjectedMap<StreetCityState> =
        toProjectedMap [ toStreetValue; toStreetOriginalValue ] toStreet removeSpaceProjector

    let toCityProjectedMap : Map<list<string>, list<City>> option -> ProjectedMap<City> =
        toProjectedMap [ toCityValue; toCityOriginalValue ] toCity removeSpaceProjector


    type ZipMetricMapUpdater (getConn) =
        interface IUpdater<ZipCode, ZipMetricMap> with
            member __.init () = Map.empty
            member __.remove p m = m.Remove p

            member __.update p m =
                try
                    match m.ContainsKey p with
                    | true -> m
                    | false ->
                        let t =
                            loadZipMapData getConn ToSublistsParams.fuzzyMatchValue p
                            |> toStreetMetricListMap
                            //|> toStreetProjectedMap

                        m.Add (p, t)
                with
                | ex ->
                    logLoadingErr "ZipMetricMapUpdater.update" ex
                    m


    let loadStateCityData conn t p =
        let s, c = p

        loadStreetZipTblForStateCity conn s c
        |> processStreetZipTbl
        |> toStateCityMap t
        |> Map.tryFind p


    type StateCityMapUpdater (getConn) =
        interface IUpdater<State * City, StateCityMap> with
            member __.init () = Map.empty
            member __.remove p m = m.Remove p

            member __.update p m =
                try
                    match m.ContainsKey p with
                    | true -> m
                    | false ->
                        match loadStateCityData getConn ToSublistsParams.exactMatchValue p with
                        | Some x -> m.Add (p, x)
                        | None -> m.Add (p, Map.empty)
                with
                | ex ->
                    logLoadingErr "StateCityMapUpdater.update" ex
                    m


    type StateCityMetricMapUpdater (getConn) =
        interface IUpdater<State * City, StateCityMetricMap> with
            member __.init () = Map.empty
            member __.remove p m = m.Remove p

            member __.update p m =
                try
                    match m.ContainsKey p with
                    | true -> m
                    | false ->

                        let t =
                            loadStateCityData getConn ToSublistsParams.fuzzyMatchValue p
                            |> toStreetMetricListMap
                            //|> toStreetProjectedMap

                        m.Add (p, t)
                with
                | ex ->
                    logLoadingErr "StateCityMapUpdater.update" ex
                    m


    let loadZipToCityData conn t =
        let zipCodeCities = loadZipCodeCities conn

        zipCodeCities
        |> List.map (fun r ->
                let zo = r.ZipCode |> ZipCode.tryCreate
                let co = (r.City, Some r.CityOriginalName) |> City.tryCreate CleanStringParams.defaultValue
                match zo, co with | Some z, Some c -> Some (z, c) | _ -> None )
        |> List.choose id
        |> groupBy
        |> List.map (fun (z, e) -> z, mapCity t e)
        |> Map.ofList


    type ZipToCityMapUpdater (getConn) =
        interface IUpdater<unit, ZipToCityMap> with
            member __.init () = Map.empty
            member __.remove _ _ = Map.empty

            member __.update _ m =
                try
                    match m.IsEmpty with
                    | true -> loadZipToCityData getConn ToSublistsParams.exactMatchValue
                    | false -> m
                with
                | ex ->
                    logLoadingErr "ZipToCityMapUpdater.update" ex
                    m


    type ZipToCityMetricMapUpdater (getConn) =
        interface IUpdater<unit, ZipToCityMetricMap> with
            member __.init () = Map.empty
            member __.remove _ _ = Map.empty

            member __.update _ m =
                try
                    match m.IsEmpty with
                    | true -> loadZipToCityData getConn ToSublistsParams.fuzzyMatchValue |> Map.map (fun _ v -> toCityMetricListMap (Some v))
                    | false -> m
                with
                | ex ->
                    logLoadingErr "ZipToCityMetricMapUpdater.update" ex
                    m


    type StateToCityCityMapUpdater (getConn) =
        interface IUpdater<unit, StateToCityMap> with
            member __.init () = Map.empty
            member __.remove _ _ = Map.empty

            member __.update _ m =
                try
                    match m.IsEmpty with
                    | true ->
                        let zipCodeCities = loadZipCodeCities getConn

                        zipCodeCities
                        |> List.map (fun r ->
                                let so = r.StateCode |> State.tryCreate
                                let co = (r.City, Some r.CityOriginalName) |> City.tryCreate CleanStringParams.defaultValue
                                match so, co with | Some s, Some c -> Some (s, c) | _ -> None )
                        |> List.choose id
                        |> groupBy
                        |> List.map (fun (s, e) -> s, mapCity ToSublistsParams.exactMatchValue e)
                        |> Map.ofList
                    | false -> m

                with
                | ex ->
                    logLoadingErr "StateToCityCityMapUpdater.update" ex
                    m


    type StreetZipDetailedMapUpdater() =
        interface IUpdater<StreetZipInfo, StreetZipDetailedMap> with
            member __.init () = Map.empty
            member __.remove p m = m.Remove p.key

            member __.update p m =
                try
                    match m.TryFind p.key with
                    | None -> m.Add (p.key, p)
                    | Some v ->
                        let q =
                            { v with
                                OccurrenceCount = v.OccurrenceCount + p.OccurrenceCount
                                StreetOriginalName =
                                    if p.StreetOriginalName.Length > v.StreetOriginalName.Length
                                    then p.StreetOriginalName
                                    else v.StreetOriginalName
                            }

                        m.Add (p.key, q)
                with
                | ex ->
                    logLoadingErr "StreetZipDetailedMapUpdater.update" ex
                    m


    type CountyMapUpdater (getConn) =
        interface IUpdater<unit, CountyMap> with
            member __.init () = Map.empty
            member __.remove _ _ = Map.empty
            member __.update _ m =
                try
                    match m.IsEmpty with
                    | true ->
                        let counties = loadCounties getConn
                        counties
                        |> Seq.map(fun r -> County.tryCreate cleanStringParams (r.Id, r.CountyName, r.StateCode) |> Option.bind (fun c -> Some (r.Id, c)))
                        |> Seq.choose id
                        |> Map.ofSeq
                    | false -> m
                with
                | ex ->
                    logLoadingErr "CountySetUpdater.update" ex
                    m


    type CountyNameMapUpdater (getConn) =
        interface IUpdater<unit, CountyNameMap> with
            member __.init () = Map.empty
            member __.remove _ _ = Map.empty
            member __.update _ m =
                try
                    match m.IsEmpty with
                    | true ->
                        let counties = loadCounties getConn
                        counties
                        |> Seq.map(fun r -> County.tryCreate cleanStringParams (r.Id, r.CountyName, r.StateCode) |> Option.bind (fun c -> Some ((c.countyName, c.state), c)))
                        |> Seq.choose id
                        |> Map.ofSeq
                    | false -> m
                with
                | ex ->
                    logLoadingErr "CountyNameMapUpdater.update" ex
                    m


    type ZipCodeSelectMapUpdater (getConn) =
        interface IUpdater<StreetCityState, ZipCodeSelectMap> with
            member __.init () = Map.empty
            member __.remove p m = m.Remove p

            member __.update p m =
                try
                    match m.ContainsKey p with
                    | true -> m
                    | false -> m.Add (p, loadZipSelectTbl getConn p)
                with
                | ex ->
                    logLoadingErr "ZipCodeSelectMapUpdater.update" ex
                    m


    type CitySelectMapUpdater (getConn) =
        interface IUpdater<StreetCityState * ZipCode, CitySelectMap> with
            member __.init () = Map.empty
            member __.remove p m = m.Remove p

            member __.update p m =
                try
                    match m.ContainsKey p with
                    | true -> m
                    | false -> m.Add (p, loadCitySelectTbl getConn p)
                with
                | ex ->
                    logLoadingErr "CitySelectMapUpdater.update" ex
                    m


    let zipCodeCityMapUpdater getConn = new AsyncUpdater<unit, ZipCodeCityMap>(ZipCodeCityMapUpdater getConn)
    let zipCodeCityMetricMapUpdater getConn = new AsyncUpdater<unit, ZipCodeCityMetricMap>(ZipCodeCityMetricMapUpdater getConn)

    let zipMapUpdater getConn = new AsyncUpdater<ZipCode, ZipMap>(ZipMapUpdater getConn)
    let zipMetricMapUpdater getConn = new AsyncUpdater<ZipCode, ZipMetricMap>(ZipMetricMapUpdater getConn)

    let stateCityMapUpdater getConn = new AsyncUpdater<State * City, StateCityMap>(StateCityMapUpdater getConn)
    let stateCityMetricMapUpdater getConn = new AsyncUpdater<State * City, StateCityMetricMap>(StateCityMetricMapUpdater getConn)

    let zipToCityMapUpdater getConn = new AsyncUpdater<unit, ZipToCityMap>(ZipToCityMapUpdater getConn)
    let zipToCityMetricMapUpdater getConn = new AsyncUpdater<unit, ZipToCityMetricMap>(ZipToCityMetricMapUpdater getConn)

    let stateToCityMapUpdater getConn = new AsyncUpdater<unit, StateToCityMap>(StateToCityCityMapUpdater getConn)
    let streetZipDetailedMapUpdater () = new AsyncUpdater<StreetZipInfo, StreetZipDetailedMap>(StreetZipDetailedMapUpdater ())
    let countyMapUpdater getConn = new AsyncUpdater<unit, CountyMap>(CountyMapUpdater getConn)
    let countyNameMapUpdater getConn = new AsyncUpdater<unit, CountyNameMap>(CountyNameMapUpdater getConn)
    let zipCodeSelectMapUpdater getConn = new AsyncUpdater<StreetCityState, ZipCodeSelectMap>(ZipCodeSelectMapUpdater getConn)
    let citySelectMapUpdater getConn = new AsyncUpdater<StreetCityState * ZipCode, CitySelectMap>(CitySelectMapUpdater getConn)


    type AddressPartUpdater =
        {
            zipCodeCityUpdater : AsyncUpdater<unit, ZipCodeCityMap>
            zipCodeCityMetricUpdater : AsyncUpdater<unit, ZipCodeCityMetricMap>
            zipToCityMapUpdater : AsyncUpdater<unit, ZipToCityMap>
            countyUpdater : AsyncUpdater<unit, CountyMap>
        }

        member this.updateContent() =
            do this.zipCodeCityUpdater.updateContent()
            do this.zipCodeCityMetricUpdater.updateContent()
            do this.zipToCityMapUpdater.updateContent()
            do this.countyUpdater.updateContent()
