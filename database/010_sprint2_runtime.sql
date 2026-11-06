-- Somente permissoes/principal dentro da base identificada. Nao cria login de servidor.
USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=3) OR DATABASE_PRINCIPAL_ID(N'PokeVaultRuntime') IS NULL
    THROW 51102, 'SCHEMA_INESPERADO', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    IF DATABASE_PRINCIPAL_ID(N'PokeVaultRuntimeVerificacao') IS NULL
        CREATE USER PokeVaultRuntimeVerificacao WITHOUT LOGIN;
    ELSE IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name=N'PokeVaultRuntimeVerificacao' AND type=N'S' AND authentication_type=0)
        THROW 51105, 'PRINCIPAL_EXISTENTE_INESPERADO', 1;
    IF NOT EXISTS (SELECT 1 FROM sys.database_role_members
        WHERE role_principal_id=DATABASE_PRINCIPAL_ID(N'PokeVaultRuntime') AND member_principal_id=DATABASE_PRINCIPAL_ID(N'PokeVaultRuntimeVerificacao'))
        ALTER ROLE PokeVaultRuntime ADD MEMBER PokeVaultRuntimeVerificacao;
    GRANT CONNECT TO PokeVaultRuntimeVerificacao;
    GRANT SELECT ON SCHEMA::dbo TO PokeVaultRuntime;
    GRANT EXECUTE ON SCHEMA::dbo TO PokeVaultRuntime;
    GRANT REFERENCES ON TYPE::dbo.ItemCompraSelecionadoType TO PokeVaultRuntime;
    DENY INSERT,UPDATE,DELETE ON SCHEMA::dbo TO PokeVaultRuntime;
    DENY EXECUTE ON OBJECT::dbo.usp_DefinirSaldo TO PokeVaultRuntime;
    DENY EXECUTE ON OBJECT::dbo.usp_AjustarCarta TO PokeVaultRuntime;
    COMMIT;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK;
    THROW;
END CATCH;
GO
