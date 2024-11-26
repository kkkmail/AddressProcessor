namespace Softellect.AddressProcessor

open System

module DatabaseTypes =

    let settingSelectStr = "select * from EFAddressProcessorSettings where AddressProcessorSettingId = @AddressProcessorSettingId"


    type StreetZipDetailed =
        {
            Id : int
            City : string
            State : string
            ZipCode : string
            OccurrenceCount : int
            StreetFullName : string
            StreetOriginalName : string option
            CityOriginalName : string option
        }


    type QuotePropertyDataDetailed =
        {
            QuotePropertyId : int
            StreetLine : string
            City : string
            State : string
            ZipCode : string
        }


    let streetZipTblName = "EFStreetZips"


    let garbagePattern =
        @"
                and (len(replace(replace(replace(replace(trim([StreetOriginalName]), '/', ' '),' ','<>'),'><',''),'<>',' ')) - len(replace(replace(replace(replace(replace(trim([StreetOriginalName]), '/', ' '),' ','<>'),'><',''),'<>',' '), ' ', ''))) <= 7
                and (len(replace(replace(replace(trim([StreetFullName]),' ','<>'),'><',''),'<>',' ')) - len(replace(replace(replace(replace(trim([StreetFullName]),' ','<>'),'><',''),'<>',' '), ' ', ''))) <= 7
                and (len(StreetFullName) - len(replace(StreetFullName, '/', ''))) <= 1
            "


    let streetZipAllSelectStr =
        @"
            select
                z.*,
                c.CityOriginalName
            from dbo.EFStreetZips z
            inner join dbo.EFZipCodeCities c on z.Zip = c.Zip and z.City = c.City and z.StateCode = c.StateCode
            where z.OccurrenceCount > 0"


    let streetZipSelectStr = streetZipAllSelectStr + @" and z.Zip = @Zip" + garbagePattern
    let streetZipStateCitySelectStr = streetZipAllSelectStr + @" and z.StateCode = @StateCode and z.City = @City" + garbagePattern
    let streetZipAddOnTblName = "EFStreetZipAddOns"


    let streetZipAddOnAllSelectStr =
        @"
            select
                z.StreetZipAddOnId as StreetZipId,
                z.StreetFullName,
                z.City,
                z.StateCode,
                z.Zip,
                z.OccurrenceCount,
                z.StreetOriginalName,
                c.CityOriginalName
            from dbo.EFStreetZipAddOns z
            inner join dbo.EFZipCodeCities c on z.Zip = c.Zip and z.City = c.City and z.StateCode = c.StateCode
            where z.OccurrenceCount > 0"


    let streetZipAddOnSelectStr = streetZipAddOnAllSelectStr + @" and z.Zip = @Zip"
    let streetZipAddOnStateCitySelectStr = streetZipAddOnAllSelectStr + @" and z.StateCode = @StateCode and z.City = @City"


    let zipCodeSelectStr =
        @"
        declare @s nvarchar(100) = @Street, @c nvarchar(50) = @City, @st nvarchar(2) = @StateCode

        ;with w as
        (
            select z.Zip
            from dbo.EFStreetZips z
            inner join dbo.EFZipCodeCities c on z.Zip = c.Zip and z.City = c.City and z.StateCode = c.StateCode
            where z.StreetFullName = @s and z.City = @c and z.StateCode = @st

            union

            select z.Zip
            from dbo.EFStreetZipAddOns z
            inner join dbo.EFZipCodeCities c on z.Zip = c.Zip and z.City = c.City and z.StateCode = c.StateCode
            where z.StreetFullName = @s and z.City = @c and z.StateCode = @st
         )
         select distinct Zip from w
        "


    let citySelectStr =
        @"
        declare @s nvarchar(100) = @Street, @st nvarchar(2) = @StateCode, @z nvarchar(50) = @Zip

        ;with w as
        (
            select z.City, c.CityOriginalName
            from dbo.EFStreetZips z
            inner join dbo.EFZipCodeCities c on z.Zip = c.Zip and z.City = c.City and z.StateCode = c.StateCode
            where z.StreetFullName = @s and z.StateCode = @st and z.Zip = @z

            union

            select z.City, c.CityOriginalName
            from dbo.EFStreetZipAddOns z
            inner join dbo.EFZipCodeCities c on z.Zip = c.Zip and z.City = c.City and z.StateCode = c.StateCode
            where z.StreetFullName = @s and z.StateCode = @st and z.Zip = @z
         )
         select distinct City, CityOriginalName from w
        "


    let zipCodeCitySelectStr ="select * from dbo.EFZipCodeCities"
    let countySelectStr ="select * from dbo.EFCounties"
    let wordCorrectionSelectStr = @"select * from dbo.EFZipCodeWordCorrections"
    let quotePropertyBatchSizeStr = "1000"
    let quotePropertyBatchSize = quotePropertyBatchSizeStr |> Int32.Parse
    let selectMaxQuotePropertyIdSql = @"select isnull(max(isnull(QuotePropertyId, 0)), 0) as MaxQuotePropertyId from dbo.EFQuoteProperties"


    let addressKeySelectStr =
        @"
            select
                AddressKey
            from dbo.EFQuoteProperties
            where
                Zip = @Zip
                and StateCode = @StateCode
                and ProjectedCity = @City
                and ProjectedStreetLine = @StreetLine"


    let addressKeyNoZipSelectStr =
        @"
            select
                AddressKey
            from dbo.EFQuoteProperties
            where
                StateCode = @StateCode
                and ProjectedCity = @City
                and ProjectedStreetLine = @StreetLine"


    let quotePropertyDataSelectStr =
        @"
            select top " + quotePropertyBatchSizeStr + @"
                QuotePropertyId,
                isnull(Street1, '') as Street1,
                isnull(City, '') as City,
                StateCode,
                isnull(Zip, '') as Zip
            from dbo.EFQuoteProperties
            where
                QuotePropertyId > @startId and QuotePropertyId <= @endId
                and Street1 is not null and City is not null and Zip is not null
            order by QuotePropertyId"
