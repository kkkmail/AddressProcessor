namespace Softellect.AddressProcessor

module OpenAddressDataTypes =

    type CountryState =
        {
            country : string
            state : string
        }


    type SourceType =
        | StateWide of CountryState
        | City of CountryState * string
        | Town of CountryState * string
        | County of CountryState * string
        | Unknown of CountryState * string


    type RowData =
        {
            latitude : decimal option
            longitude : decimal option
            number : string
            street : string
            unit : string
            city : string
            district : string
            region : string
            postCode : string
            id : string
            hash : string
            source : string
        }


    let truncateOpenAddressSql = "truncate table dbo.EFOpenAddresses"
    let truncateRawOpenAddressSql = "truncate table dbo.EFRawOpenAddresses"
