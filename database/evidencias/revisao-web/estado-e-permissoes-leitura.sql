-- Revisao autorizada em 03/11/2026: SELECT e contexto de sessao, sem DDL/DML/fixture.
USE [$(DatabaseName)];
GO
SET NOCOUNT ON;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
SELECT SYSUTCDATETIME() AS ObservadoEmUtc,DB_NAME() AS Banco,
    CONVERT(nvarchar(80),@@SERVERNAME) AS Servidor,CONVERT(nvarchar(30),SERVERPROPERTY('ProductVersion')) AS Motor;
SELECT Versao,AplicadaEmUtc FROM dbo.SchemaVersao ORDER BY Versao;
SELECT CONVERT(nvarchar(60),value) AS Marcador FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto';
SELECT (SELECT COUNT(*) FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Colecao') AND name=N'QuantidadeTotalCartas') AS ColunaComplemento,
       (SELECT COUNT(*) FROM sys.procedures WHERE name IN (N'usp_CadastrarColecao',N'usp_AdicionarItemCarrinho')) AS ProceduresComplemento,
       (SELECT COUNT(*) FROM sys.check_constraints WHERE name=N'CK_Colecao_TotalCartas') AS ConstraintComplemento,
       (SELECT COUNT(*) FROM dbo.SchemaVersao WHERE Versao>4) AS VersoesPosteriores;
-- Metadados correspondentes a parte de leitura de 011; nao executar seus ensaios de DML negado.
SELECT OBJECT_NAME(object_id) AS Tabela,name AS Coluna,TYPE_NAME(system_type_id) AS Tipo,max_length,is_nullable
 FROM sys.columns WHERE object_id IN (OBJECT_ID(N'dbo.Carta'),OBJECT_ID(N'dbo.SaldoCliente')) AND name=N'Versao';
SELECT OBJECT_NAME(object_id) AS ProcedureNome,uses_ansi_nulls,uses_quoted_identifier
 FROM sys.sql_modules WHERE object_id IN (OBJECT_ID(N'dbo.usp_DefinirSaldoVersionado'),OBJECT_ID(N'dbo.usp_AjustarCartaVersionado'));
SELECT name AS ProcedureNome,modify_date FROM sys.procedures WHERE name IN
 (N'usp_ConcluirCompra',N'usp_CancelarCompra',N'usp_CadastrarCarta',N'usp_DefinirItemCarrinho') ORDER BY name;
SELECT p.Codigo,COUNT(*) AS Usuarios,SUM(CASE WHEN u.Ativo=1 THEN 1 ELSE 0 END) AS Ativos,
 SUM(CASE WHEN u.SenhaHash IS NOT NULL AND LEN(u.SenhaHash)>0 THEN 1 ELSE 0 END) AS ComHashArmazenado
 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId GROUP BY p.Codigo ORDER BY p.Codigo;
SELECT COUNT(*) AS AdministradoresFixos,SUM(CASE WHEN u.Ativo=1 AND p.Codigo=N'ADMINISTRADOR' THEN 1 ELSE 0 END) AS FixosValidos
 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId WHERE u.AdministradorFixo=1;
SELECT COUNT(*) AS Compras,SUM(CASE WHEN Status=N'CONCLUIDO' THEN 1 ELSE 0 END) AS Concluidas,
 SUM(CASE WHEN Status=N'CANCELADO' THEN 1 ELSE 0 END) AS Canceladas FROM dbo.Compra;
SELECT COUNT(*) AS MovimentosSaldo FROM dbo.MovimentacaoSaldo;
SELECT COUNT(*) AS MovimentosEstoque FROM dbo.MovimentacaoEstoque;
SELECT COUNT(*) AS AlteracoesCarta FROM dbo.AlteracaoCarta;
BEGIN TRY
 EXECUTE AS USER=N'PokeVaultRuntimeVerificacao';
 SELECT USER_NAME() AS PrincipalVerificado,
  HAS_PERMS_BY_NAME(N'dbo.Carta',N'OBJECT',N'SELECT') AS PodeLerCarta,
  HAS_PERMS_BY_NAME(N'dbo.Carta',N'OBJECT',N'UPDATE') AS PodeAtualizarCartaDireto,
  HAS_PERMS_BY_NAME(N'dbo.SaldoCliente',N'OBJECT',N'INSERT') AS PodeInserirSaldoDireto,
  HAS_PERMS_BY_NAME(N'dbo.Compra',N'OBJECT',N'DELETE') AS PodeExcluirCompraDireto,
  HAS_PERMS_BY_NAME(N'dbo.usp_DefinirSaldoVersionado',N'OBJECT',N'EXECUTE') AS PodeExecutarSaldoVersionado,
  HAS_PERMS_BY_NAME(N'dbo.usp_AjustarCartaVersionado',N'OBJECT',N'EXECUTE') AS PodeExecutarCartaVersionada,
  HAS_PERMS_BY_NAME(N'dbo.usp_DefinirSaldo',N'OBJECT',N'EXECUTE') AS PodeExecutarSaldoLegado,
  HAS_PERMS_BY_NAME(N'dbo.usp_AjustarCarta',N'OBJECT',N'EXECUTE') AS PodeExecutarCartaLegada;
 REVERT;
END TRY
BEGIN CATCH
 IF USER_NAME()=N'PokeVaultRuntimeVerificacao' REVERT;
 THROW;
END CATCH;
GO
