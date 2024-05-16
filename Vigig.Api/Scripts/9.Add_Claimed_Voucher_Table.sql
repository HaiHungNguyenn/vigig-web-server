IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClaimedVoucher')
BEGIN
    CREATE TABLE [ClaimedVoucher](
        Id UNIQUEIDENTIFIER PRIMARY KEY NOT NULL,
        EventTitle NVARCHAR(MAX) NOT NULL,
        Field NVARCHAR(MAX) NOT NULL,
        StartDate DATETIME NOT NULL,
        EndDate DATETIME NOT NULL,
        VoucherId UNIQUEIDENTIFIER NOT NULL,
        CustomerId UNIQUEIDENTIFIER NOT NULL,
    )
END

ALTER TABLE [ClaimedVoucher]
ADD FOREIGN KEY (VoucherId) REFERENCES [Voucher](Id)

ALTER TABLE [ClaimedVoucher]
ADD FOREIGN KEY (CustomerId) REFERENCES [VigigUser](Id)