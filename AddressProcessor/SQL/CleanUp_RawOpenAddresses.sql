declare @deletedRows int, @batchNumber int, @msg nvarchar(4000)
set @deletedRows = 1
set @batchNumber = 1

while (@deletedRows > 0) begin	
	set @msg = 'Deleting empty data, batch: ' + cast(@batchNumber as nvarchar(10))
	raiserror (@msg, 0, 1) with nowait

	begin tran
		delete top (100000) from EFRawOpenAddresses
		where City = '' or City is null or Street = '' or Street is null or PostCode = '' or PostCode is null

		set @deletedRows = @@rowcount
	commit

	set @batchNumber = @batchNumber + 1
end
go


declare @deletedRows int, @batchNumber int, @msg nvarchar(4000)
set @deletedRows = 1
set @batchNumber = 1

while (@deletedRows > 0) begin
	set @msg = 'Deleting duplicates, batch: ' + cast(@batchNumber as nvarchar(10))
	raiserror (@msg, 0, 1) with nowait

	begin tran
		;with oa as
		(
			select distinct top 1000 Number, Street, Unit, City, Region
			from EFRawOpenAddresses
			group by Number, Street, Unit, City, Region
			having count(*) > 1
		)
		delete w
		from EFRawOpenAddresses w
		inner join oa on w.Number = oa.Number and w.Street = oa.Street and w.Unit = oa.Unit and w.City = oa.City and w.Region = oa.Region

		set @deletedRows = @@rowcount
	commit

	set @batchNumber = @batchNumber + 1
end
go

