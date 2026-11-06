-- Somente leitura/checagem de permissoes. DML propositalmente sem linhas e deve ser negado.
-- Nao chama ajuste valido nem altera massa de teste/demonstracao.
USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
IF (SELECT COUNT(*) FROM dbo.SchemaVersao WHERE Versao IN (1,2,3))<>3
    THROW 51102, 'SCHEMA_INESPERADO', 1;
SELECT DB_NAME() AS Banco,CONVERT(nvarchar(80),@@SERVERNAME) AS Servidor,
    CONVERT(nvarchar(30),SERVERPROPERTY('ProductVersion')) AS VersaoMotor;
SELECT Versao,AplicadaEmUtc FROM dbo.SchemaVersao ORDER BY Versao;
SELECT OBJECT_NAME(object_id) AS Tabela,name AS Coluna,TYPE_NAME(system_type_id) AS Tipo,max_length,is_nullable
    FROM sys.columns WHERE object_id IN (OBJECT_ID(N'dbo.Carta'),OBJECT_ID(N'dbo.SaldoCliente')) AND name=N'Versao';
SELECT OBJECT_NAME(object_id) AS ProcedureNome,uses_ansi_nulls,uses_quoted_identifier
    FROM sys.sql_modules WHERE object_id IN (OBJECT_ID(N'dbo.usp_DefinirSaldoVersionado'),OBJECT_ID(N'dbo.usp_AjustarCartaVersionado'));
BEGIN TRY
    EXECUTE AS USER=N'PokeVaultRuntimeVerificacao';
    IF HAS_PERMS_BY_NAME(N'dbo.Carta',N'OBJECT',N'SELECT')<>1
       OR HAS_PERMS_BY_NAME(N'dbo.Carta',N'OBJECT',N'UPDATE')<>0
       OR HAS_PERMS_BY_NAME(N'dbo.SaldoCliente',N'OBJECT',N'INSERT')<>0
       OR HAS_PERMS_BY_NAME(N'dbo.Compra',N'OBJECT',N'DELETE')<>0
       OR HAS_PERMS_BY_NAME(N'dbo.usp_DefinirSaldoVersionado',N'OBJECT',N'EXECUTE')<>1
       OR HAS_PERMS_BY_NAME(N'dbo.usp_AjustarCartaVersionado',N'OBJECT',N'EXECUTE')<>1
       OR HAS_PERMS_BY_NAME(N'dbo.usp_DefinirSaldo',N'OBJECT',N'EXECUTE')<>0
       OR HAS_PERMS_BY_NAME(N'dbo.usp_AjustarCarta',N'OBJECT',N'EXECUTE')<>0
        THROW 51200, 'PERMISSOES_INESPERADAS', 1;
    SELECT USER_NAME() AS PrincipalVerificado,COUNT(*) AS CartasLidas FROM dbo.Carta;
    BEGIN TRY
        UPDATE dbo.Carta SET QuantidadeEstoque=QuantidadeEstoque WHERE 1=0;
        THROW 51200, 'UPDATE_NAO_FOI_NEGADO', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER()<>229 THROW;
        SELECT N'UPDATE_DIRETO_NEGADO' AS Verificacao,ERROR_NUMBER() AS ErroEsperado;
    END CATCH;
    BEGIN TRY
        INSERT dbo.SaldoCliente(UsuarioId,Saldo) SELECT 0,0 WHERE 1=0;
        THROW 51200, 'INSERT_NAO_FOI_NEGADO', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER()<>229 THROW;
        SELECT N'INSERT_DIRETO_NEGADO' AS Verificacao,ERROR_NUMBER() AS ErroEsperado;
    END CATCH;
    BEGIN TRY
        DELETE dbo.Compra WHERE 1=0;
        THROW 51200, 'DELETE_NAO_FOI_NEGADO', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER()<>229 THROW;
        SELECT N'DELETE_DIRETO_NEGADO' AS Verificacao,ERROR_NUMBER() AS ErroEsperado;
    END CATCH;
    BEGIN TRY
        EXEC dbo.usp_DefinirSaldo @AdministradorId=-1,@ClienteId=-1,@NovoSaldo=0,@Motivo=N'Verificacao de permissao';
        THROW 51200, 'AJUSTE_LEGADO_NAO_FOI_NEGADO', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER()<>229 THROW;
        SELECT N'AJUSTE_LEGADO_NEGADO' AS Verificacao,ERROR_NUMBER() AS ErroEsperado;
    END CATCH;
    BEGIN TRY
        EXEC dbo.usp_DefinirSaldoVersionado @AdministradorId=-1,@ClienteId=-1,@NovoSaldo=0,
            @Motivo=N'Verificacao sem ator valido',@VersaoEsperada=0x0000000000000000;
        THROW 51200, 'ATOR_INVALIDO_NAO_FOI_NEGADO', 1;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER()<>51000 THROW;
        SELECT N'EXECUTE_VERSIONADO_PERMITIDO_AUTORIZACAO_RECUSADA' AS Verificacao,ERROR_NUMBER() AS ErroEsperado;
    END CATCH;
    REVERT;
END TRY
BEGIN CATCH
    IF USER_NAME()=N'PokeVaultRuntimeVerificacao' REVERT;
    THROW;
END CATCH;
GO
