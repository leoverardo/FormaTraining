SET NOCOUNT ON;

IF COL_LENGTH('dbo.TrainerSubscriptions', 'BillingCycle') IS NULL
BEGIN
    ALTER TABLE dbo.TrainerSubscriptions
    ADD BillingCycle int NOT NULL
        CONSTRAINT DF_TrainerSubscriptions_BillingCycle DEFAULT (0);
END;

IF COL_LENGTH('dbo.TrainerSubscriptions', 'PlatformPlanPriceId') IS NULL
BEGIN
    ALTER TABLE dbo.TrainerSubscriptions
    ADD PlatformPlanPriceId uniqueidentifier NULL;
END;

IF COL_LENGTH('dbo.TrainerSubscriptions', 'InitPoint') IS NULL
BEGIN
    ALTER TABLE dbo.TrainerSubscriptions
    ADD InitPoint nvarchar(max) NULL;
END;

IF COL_LENGTH('dbo.TrainerSubscriptions', 'MercadoPagoPayerId') IS NULL
BEGIN
    ALTER TABLE dbo.TrainerSubscriptions
    ADD MercadoPagoPayerId nvarchar(max) NULL;
END;

IF COL_LENGTH('dbo.TrainerPayments', 'ProviderSubscriptionId') IS NULL
BEGIN
    ALTER TABLE dbo.TrainerPayments
    ADD ProviderSubscriptionId nvarchar(max) NULL;
END;

IF COL_LENGTH('dbo.TrainerPayments', 'RawPayload') IS NULL
BEGIN
    ALTER TABLE dbo.TrainerPayments
    ADD RawPayload nvarchar(max) NULL;
END;

IF OBJECT_ID('dbo.PaymentWebhookLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PaymentWebhookLogs
    (
        Id uniqueidentifier NOT NULL,
        Provider nvarchar(450) NOT NULL,
        EventId nvarchar(450) NOT NULL,
        Type nvarchar(max) NOT NULL,
        ResourceId nvarchar(450) NULL,
        RawPayload nvarchar(max) NOT NULL,
        ProcessedAt datetime2 NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT PK_PaymentWebhookLogs PRIMARY KEY (Id)
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PaymentWebhookLogs_Provider_EventId'
      AND object_id = OBJECT_ID('dbo.PaymentWebhookLogs')
)
BEGIN
    CREATE UNIQUE INDEX IX_PaymentWebhookLogs_Provider_EventId
    ON dbo.PaymentWebhookLogs (Provider, EventId);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PaymentWebhookLogs_ResourceId'
      AND object_id = OBJECT_ID('dbo.PaymentWebhookLogs')
)
BEGIN
    CREATE INDEX IX_PaymentWebhookLogs_ResourceId
    ON dbo.PaymentWebhookLogs (ResourceId);
END;

IF OBJECT_ID('dbo.PlatformPlanPrices', 'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = 'IX_TrainerSubscriptions_PlatformPlanPriceId'
         AND object_id = OBJECT_ID('dbo.TrainerSubscriptions')
   )
BEGIN
    CREATE INDEX IX_TrainerSubscriptions_PlatformPlanPriceId
    ON dbo.TrainerSubscriptions (PlatformPlanPriceId);
END;

IF OBJECT_ID('dbo.PlatformPlanPrices', 'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.foreign_keys
       WHERE name = 'FK_TrainerSubscriptions_PlatformPlanPrices_PlatformPlanPriceId'
   )
BEGIN
    ALTER TABLE dbo.TrainerSubscriptions
    ADD CONSTRAINT FK_TrainerSubscriptions_PlatformPlanPrices_PlatformPlanPriceId
        FOREIGN KEY (PlatformPlanPriceId)
        REFERENCES dbo.PlatformPlanPrices (Id);
END;
