select top 1000
	Id, 
	count (*)  as Cnt
from EFRawOpenAddresses
group by Id
having count(*) > 1
order by 2 desc

select top 1000
	[Hash], 
	count (*)  as Cnt
from EFRawOpenAddresses
group by [Hash]
having count(*) > 1
order by 2 desc

----------------------------------
;with oa as
(
select distinct Number, Street, Unit, City, Region
from EFRawOpenAddresses
--where City <> '' and City is not null and Street <> '' and Street is not null and PostCode <> '' and PostCode is not null
group by Number, Street, Unit, City, Region
having count(*) > 1
)
select top 1000
*
from oa

select top 1000
	Number, Street, Unit, City, Region,
	count (*)  as Cnt
from EFRawOpenAddresses
where City <> '' and City is not null and Street <> '' and Street is not null and PostCode <> '' and PostCode is not null
group by Number, Street, Unit, City, Region
having count(*) > 1
order by 6 desc

select count(*) from EFRawOpenAddresses where City = '' or City is null or Street = '' or Street is null or PostCode = '' or PostCode is null
--17,643,233
--64,311,691

select count(*) from EFRawOpenAddresses where PostCode = '' or PostCode is null

select count(*) from EFRawOpenAddresses where Region = '' or Region is null

select count(*) from EFRawOpenAddresses where District = '' or District is null

select top 1000
	*,
	count (*)  as Cnt
from EFRawOpenAddresses
having count(*) > 1
order by 2 desc


select top 1000 *
from EFRawOpenAddresses

