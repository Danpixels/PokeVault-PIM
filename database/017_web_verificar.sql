-- Consultas de preparacao; nao executa cenarios mutantes.
USE [$(DatabaseName)];
GO
SET NOCOUNT ON;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
IF (SELECT COUNT(*) FROM dbo.SchemaVersao WHERE Versao IN (4,5,6,7))<>4 THROW 51102,'SCHEMA_INESPERADO',1;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Colecao') AND name=N'QuantidadeTotalCartas' AND system_type_id=56 AND is_nullable=1)
    THROW 51102,'SCHEMA_INESPERADO',1;
SELECT DB_NAME() AS Banco,CONVERT(nvarchar(80),@@SERVERNAME) AS Servidor,CONVERT(nvarchar(30),SERVERPROPERTY('ProductVersion')) AS Motor;
SELECT Versao,AplicadaEmUtc FROM dbo.SchemaVersao ORDER BY Versao;
SELECT COUNT(*) AS Colecoes,COUNT(QuantidadeTotalCartas) AS TotaisInformados FROM dbo.Colecao;
SELECT Raridade,COUNT(*) AS Cartas FROM dbo.Carta GROUP BY Raridade ORDER BY Raridade;
SELECT OBJECT_NAME(object_id) AS ProcedureNome,uses_ansi_nulls,uses_quoted_identifier
    FROM sys.sql_modules WHERE object_id IN (OBJECT_ID(N'dbo.usp_CadastrarColecao'),OBJECT_ID(N'dbo.usp_CadastrarCarta'),OBJECT_ID(N'dbo.usp_AdicionarItemCarrinho'),OBJECT_ID(N'dbo.usp_DefinirItemCarrinho'));
BEGIN TRY
    EXECUTE AS USER=N'PokeVaultRuntimeVerificacao';
    IF COALESCE(HAS_PERMS_BY_NAME(N'dbo.usp_CadastrarColecao',N'OBJECT',N'EXECUTE'),0)<>1
       OR COALESCE(HAS_PERMS_BY_NAME(N'dbo.usp_AdicionarItemCarrinho',N'OBJECT',N'EXECUTE'),0)<>1
       OR COALESCE(HAS_PERMS_BY_NAME(N'dbo.Colecao',N'OBJECT',N'SELECT'),0)<>1
       OR COALESCE(HAS_PERMS_BY_NAME(N'dbo.Colecao',N'OBJECT',N'INSERT'),1)<>0
       OR COALESCE(HAS_PERMS_BY_NAME(N'dbo.ItemCarrinho',N'OBJECT',N'UPDATE'),1)<>0
       OR COALESCE(HAS_PERMS_BY_NAME(N'dbo.usp_DefinirSaldo',N'OBJECT',N'EXECUTE'),1)<>0
        THROW 51200,'PERMISSOES_INESPERADAS',1;
    SELECT USER_NAME() AS Principal,N'EXECUTE_SELECT_PERMITIDOS_DML_E_LEGADO_NEGADOS' AS Verificacao;
    REVERT;
END TRY
BEGIN CATCH
    IF USER_NAME()=N'PokeVaultRuntimeVerificacao' REVERT;
    THROW;
END CATCH;
GO
