print 'Creating INDEX IX_RawOpenAddress_Zip...'
set nocount on
CREATE NONCLUSTERED INDEX IX_RawOpenAddress_Zip ON dbo.EFRawOpenAddresses
(
	PostCode ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

print '...done.'
GO

print 'Creating INDEX IX_RawOpenAddress_Number...'
set nocount on
CREATE NONCLUSTERED INDEX IX_RawOpenAddress_Number ON dbo.EFRawOpenAddresses
(
	Number ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

print '...done.'
GO

print 'Creating INDEX IX_RawOpenAddress_Street...'
set nocount on
CREATE NONCLUSTERED INDEX IX_RawOpenAddress_Street ON dbo.EFRawOpenAddresses
(
	Street ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

print '...done.'
GO

print 'Creating INDEX IX_RawOpenAddress_City...'
set nocount on
CREATE NONCLUSTERED INDEX IX_RawOpenAddress_City ON dbo.EFRawOpenAddresses
(
	City ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

print '...done.'
GO

print 'Creating INDEX IX_RawOpenAddress_District...'
set nocount on
CREATE NONCLUSTERED INDEX IX_RawOpenAddress_District ON dbo.EFRawOpenAddresses
(
	District ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

print '...done.'
GO

print 'Creating INDEX IX_RawOpenAddress_Region...'
set nocount on
CREATE NONCLUSTERED INDEX IX_RawOpenAddress_Region ON dbo.EFRawOpenAddresses
(
	Region ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

print '...done.'
GO

print 'Creating INDEX IX_RawOpenAddress_Id...'
set nocount on
CREATE NONCLUSTERED INDEX IX_RawOpenAddress_Id ON dbo.EFRawOpenAddresses
(
	Id ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

print '...done.'
GO

print 'Creating INDEX IX_RawOpenAddress_Hash...'
set nocount on
CREATE NONCLUSTERED INDEX IX_RawOpenAddress_Hash ON dbo.EFRawOpenAddresses
(
	[Hash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

print '...done.'
GO

