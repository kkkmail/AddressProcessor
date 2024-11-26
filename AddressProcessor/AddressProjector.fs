namespace Softellect.AddressProcessor

open Swyfft.Services.AddressProcessor
open AddressProcessorRules
open CSharpInterop

/// TODO kk:20191212 - Use mapData below to correct state / zip specific erroneous words.
type AddressProjector (d : SqlConnectionData) =
    let mapData = getMapData d.ratingConnectionGetter

    /// Projects Street1 of the entity.
    member __.getProjectedStreetLine entity = getProjectedStreetLineImpl entity

    /// Projects City of the entity.
    member __.getProjectedCity entity = getProjectedCityImpl entity

    /// Projects string representation of street.
    member __.projectStreet street = cleanStreetLine street

    /// Projects string representation of city.
    member __.projectCity city = cleanCity city
