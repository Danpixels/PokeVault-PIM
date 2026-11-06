-- Atualizacao aditiva: nao reaplicar scripts historicos para fazer upgrade.
USE [$(DatabaseName)];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=1)
    THROW 51102, 'SCHEMA_INESPERADO', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @lockResult int;
    EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Schema.Sprint2',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
    IF @lockResult<0 THROW 51104, 'ATUALIZACAO_OCUPADA', 1;
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=2)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Carta') AND name=N'Versao' AND system_type_id=189 AND is_nullable=0)
            OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.SaldoCliente') AND name=N'Versao' AND system_type_id=189 AND is_nullable=0)
            THROW 51102, 'SCHEMA_INESPERADO', 1;
        COMMIT;
        RETURN;
    END;
    IF COL_LENGTH(N'dbo.Carta',N'Versao') IS NOT NULL OR COL_LENGTH(N'dbo.SaldoCliente',N'Versao') IS NOT NULL
        THROW 51102, 'SCHEMA_INESPERADO', 1;
    ALTER TABLE dbo.Carta ADD Versao rowversion NOT NULL;
    ALTER TABLE dbo.SaldoCliente ADD Versao rowversion NOT NULL;
    INSERT dbo.SchemaVersao(Versao) VALUES (2);
    COMMIT;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK;
    THROW;
END CATCH;
GO
