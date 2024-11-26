namespace Softellect.AddressProcessor

open AddressProcessorRules
open CSharpInterop

/// TODO kk:20191212 - Use mapData below to correct state / zip specific erroneous words.
type AddressProjector (d : SqlConnectionData) =
    /// Projects string representation of street.
    member _.projectStreet street = cleanStreetLine street

    /// Projects string representation of city.
    member _.projectCity city = cleanCity city
