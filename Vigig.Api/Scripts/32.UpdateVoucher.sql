﻿ALTER TABLE [Voucher]
ADD EventId UNIQUEIDENTIFIER REFERENCES [Event](Id)