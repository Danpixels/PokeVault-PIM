USE [$(DatabaseName)];
GO
SET NOCOUNT ON;
IF DB_NAME() NOT IN(N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
 THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
IF (SELECT COUNT(*) FROM dbo.SchemaVersao WHERE Versao IN(4,8,9,10))<>4 OR EXISTS(SELECT 1 FROM dbo.SchemaVersao WHERE Versao BETWEEN 5 AND 7)
 THROW 51102,'SCHEMA_INESPERADO',1;
SELECT SYSUTCDATETIME() AS ObservadoEmUtc,DB_NAME() AS Banco,CONVERT(nvarchar(80),@@SERVERNAME) AS Servidor,CONVERT(nvarchar(30),SERVERPROPERTY('ProductVersion')) AS Motor;
SELECT Versao,AplicadaEmUtc FROM dbo.SchemaVersao ORDER BY Versao;
SELECT (SELECT COUNT(*) FROM dbo.ReferenciaTCGdex) AS Traducoes,
 (SELECT COUNT(*) FROM dbo.vw_BibliotecaTCGdex151) AS CartasLogicas,
 (SELECT COUNT(*) FROM dbo.ImportacaoTCGdex151) AS Importacoes,
 (SELECT COUNT(*) FROM dbo.VinculoCartaTCGdex) AS Vinculos;
SELECT OBJECT_NAME(object_id) AS ProcedureNome,uses_ansi_nulls,uses_quoted_identifier FROM sys.sql_modules
 WHERE object_id IN(OBJECT_ID(N'dbo.usp_IniciarImportacaoTCGdex151'),OBJECT_ID(N'dbo.usp_AtualizarReferenciasTCGdex151'),
 OBJECT_ID(N'dbo.usp_RegistrarFalhaTCGdex151'),OBJECT_ID(N'dbo.usp_VincularOfertaTCGdex151'),OBJECT_ID(N'dbo.usp_CadastrarOfertaTCGdex151'));
SELECT (SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Colecao') AND name=N'QuantidadeTotalCartas') AS ColunaWip,
 (SELECT COUNT(*) FROM sys.procedures WHERE name IN(N'usp_CadastrarColecao',N'usp_AdicionarItemCarrinho')) AS ProceduresWip;
BEGIN TRY
 EXECUTE AS USER=N'PokeVaultRuntimeVerificacao';
 IF COALESCE(HAS_PERMS_BY_NAME(N'dbo.vw_BibliotecaTCGdex151',N'OBJECT',N'SELECT'),0)<>1
  OR COALESCE(HAS_PERMS_BY_NAME(N'dbo.usp_IniciarImportacaoTCGdex151',N'OBJECT',N'EXECUTE'),0)<>1
  OR COALESCE(HAS_PERMS_BY_NAME(N'dbo.usp_AtualizarReferenciasTCGdex151',N'OBJECT',N'EXECUTE'),0)<>1
  OR COALESCE(HAS_PERMS_BY_NAME(N'dbo.usp_VincularOfertaTCGdex151',N'OBJECT',N'EXECUTE'),0)<>1
  OR COALESCE(HAS_PERMS_BY_NAME(N'dbo.ReferenciaTCGdex',N'OBJECT',N'UPDATE'),1)<>0
  OR COALESCE(HAS_PERMS_BY_NAME(N'dbo.usp_DefinirSaldo',N'OBJECT',N'EXECUTE'),1)<>0
  THROW 51200,'PERMISSOES_INESPERADAS',1;
 SELECT USER_NAME() AS Principal,N'SELECT_EXECUTE_PERMITIDOS_DML_LEGADO_NEGADOS' AS Resultado;
 REVERT;
END TRY
BEGIN CATCH
 IF USER_NAME()=N'PokeVaultRuntimeVerificacao' REVERT;
 THROW;
END CATCH;
GO
