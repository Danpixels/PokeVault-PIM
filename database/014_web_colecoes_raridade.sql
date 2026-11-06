USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=4) THROW 51102,'SCHEMA_INESPERADO',1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @lockResult int;
    EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Schema.ComplementoWeb',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
    IF @lockResult<0 THROW 51104,'ATUALIZACAO_OCUPADA',1;
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=5)
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Colecao') AND name=N'QuantidadeTotalCartas' AND system_type_id=56 AND is_nullable=1)
            OR OBJECT_ID(N'dbo.CK_Colecao_TotalCartas',N'C') IS NULL THROW 51102,'SCHEMA_INESPERADO',1;
        COMMIT;
        RETURN;
    END;
    IF COL_LENGTH(N'dbo.Colecao',N'QuantidadeTotalCartas') IS NOT NULL THROW 51102,'SCHEMA_INESPERADO',1;
    ALTER TABLE dbo.Colecao ADD QuantidadeTotalCartas int NULL;
    ALTER TABLE dbo.Colecao ADD CONSTRAINT CK_Colecao_TotalCartas CHECK (QuantidadeTotalCartas IS NULL OR QuantidadeTotalCartas>0);
    DECLARE @normalizadas TABLE (CartaId int,RaridadeAnterior nvarchar(50),RaridadeNova nvarchar(50));
    UPDATE dbo.Carta SET Raridade=N'Common'
        OUTPUT inserted.CartaId,deleted.Raridade,inserted.Raridade INTO @normalizadas
        WHERE Raridade COLLATE Latin1_General_100_BIN2=N'Comum' AND DATALENGTH(Raridade)=DATALENGTH(N'Comum');
    INSERT dbo.SchemaVersao(Versao) VALUES (5);
    COMMIT;
    SELECT CartaId,RaridadeAnterior,RaridadeNova FROM @normalizadas ORDER BY CartaId;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK;
    THROW;
END CATCH;
GO
