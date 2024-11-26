namespace Softellect.AddressProcessor

module AddressDataDataTypes =

    let addressDataSql =
        @"
select
    *
from dbo.AddressData
where
    AddressDataId >= @StartID
    and AddressDataId < @EndID
    and (len(replace(replace(replace(replace(trim([StreetFullName]), '/', ' / '),' ','<>'),'><',''),'<>',' ')) - len(replace(replace(replace(replace(replace(trim([StreetFullName]), '/', ' / '),' ','<>'),'><',''),'<>',' '), ' ', ''))) <= 7
    and (len(StreetFullName) - len(replace(StreetFullName, '/', ''))) <= 1"


    let addressDataMaxIdSql = "select isnull(max(isnull(AddressDataId, 0)), 0) as MaxId from dbo.AddressData"
    let truncateStreetZipSql = "truncate table dbo.EFStreetZips"
