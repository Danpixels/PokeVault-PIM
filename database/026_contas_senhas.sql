USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET XACT_ABORT ON; SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;
SET ANSI_PADDING ON; SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN(N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
 THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersao WHERE Versao=11) THROW 51102,'SCHEMA_INESPERADO',1;
BEGIN TRANSACTION;
DECLARE @lockResult int;
EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Schema.Contas',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
IF @lockResult<0 THROW 51104,'ATUALIZACAO_OCUPADA',1;
GO
CREATE OR ALTER PROCEDURE dbo.usp_RegistrarCliente
 @Nome nvarchar(120),@Email nvarchar(254),@EmailNormalizado nvarchar(254),@SenhaHash nvarchar(512)
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
 IF @Nome IS NULL OR LEN(TRIM(@Nome))=0 OR @Email IS NULL OR LEN(TRIM(@Email))=0
  OR @EmailNormalizado IS NULL OR LEN(TRIM(@EmailNormalizado))=0 OR @SenhaHash IS NULL OR LEN(@SenhaHash)<20
  THROW 51002,'DADOS_INVALIDOS',1;
 DECLARE @perfilId int,@usuarioId int;
 BEGIN TRY
  BEGIN TRANSACTION;
  SELECT @perfilId=PerfilId FROM dbo.Perfil WHERE Codigo=N'CLIENTE';
  IF @perfilId IS NULL THROW 51002,'DADOS_INVALIDOS',1;
  INSERT dbo.Usuario(PerfilId,Nome,Email,EmailNormalizado,SenhaHash)
   VALUES(@perfilId,TRIM(@Nome),TRIM(@Email),@EmailNormalizado,@SenhaHash);
  SET @usuarioId=CONVERT(int,SCOPE_IDENTITY());
  INSERT dbo.SaldoCliente(UsuarioId,Saldo) VALUES(@usuarioId,0);
  INSERT dbo.Carrinho(UsuarioId) VALUES(@usuarioId);
  COMMIT;
  SELECT @usuarioId AS UsuarioId;
 END TRY
 BEGIN CATCH
  DECLARE @erro int=ERROR_NUMBER();
  IF XACT_STATE()<>0 ROLLBACK;
  IF @erro IN(2601,2627) THROW 51010,'CADASTRO_DUPLICADO',1;
  THROW;
 END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_AlterarSenhaConta
 @UsuarioId int,@HashEsperado nvarchar(512),@NovoHash nvarchar(512)
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
 IF @NovoHash IS NULL OR LEN(@NovoHash)<20 OR @HashEsperado IS NULL OR LEN(@HashEsperado)<20
  THROW 51002,'DADOS_INVALIDOS',1;
 UPDATE dbo.Usuario SET SenhaHash=@NovoHash
 WHERE UsuarioId=@UsuarioId AND Ativo=1
  AND SenhaHash COLLATE Latin1_General_100_BIN2=@HashEsperado COLLATE Latin1_General_100_BIN2;
 IF @@ROWCOUNT<>1 THROW 51012,'VERSAO_DESATUALIZADA',1;
END;
GO
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersao WHERE Versao=12) INSERT dbo.SchemaVersao(Versao) VALUES(12);
COMMIT;
GO
