USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN(N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
 THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersao WHERE Versao=9) THROW 51102,'SCHEMA_INESPERADO',1;
BEGIN TRANSACTION;
DECLARE @lockResult int;
EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Schema.TCGdex151',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
IF @lockResult<0 THROW 51104,'ATUALIZACAO_OCUPADA',1;
GO
CREATE OR ALTER PROCEDURE dbo.usp_VincularOfertaTCGdex151
 @ResponsavelId int,@CartaId int,@ReferenciaTCGdexId int
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
 DECLARE @nomeLocal nvarchar(120),@numeroLocal nvarchar(20),@nomeRef nvarchar(120),@numeroRef nvarchar(3),@refAnterior int;
 BEGIN TRY
  BEGIN TRANSACTION;
  IF NOT EXISTS(SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId WHERE u.UsuarioId=@ResponsavelId AND u.Ativo=1 AND p.Codigo IN(N'ADMINISTRADOR',N'ATENDENTE')) THROW 51000,'ACESSO_NEGADO',1;
  SELECT @nomeLocal=c.Nome,@numeroLocal=c.Numero FROM dbo.Carta c WITH(HOLDLOCK) JOIN dbo.Colecao co ON co.ColecaoId=c.ColecaoId
   WHERE c.CartaId=@CartaId AND co.Codigo COLLATE Latin1_General_100_BIN2=N'MEW-151';
  SELECT @nomeRef=Nome,@numeroRef=Numero FROM dbo.ReferenciaTCGdex WITH(HOLDLOCK) WHERE ReferenciaTCGdexId=@ReferenciaTCGdexId AND Detalhada=1 AND SetId=N'sv03.5';
  IF @nomeLocal IS NULL OR @nomeRef IS NULL OR TRY_CONVERT(int,@numeroLocal) IS NULL OR TRY_CONVERT(int,@numeroLocal)<>TRY_CONVERT(int,@numeroRef)
   OR TRIM(@nomeLocal) COLLATE Latin1_General_100_CI_AS<>TRIM(@nomeRef) COLLATE Latin1_General_100_CI_AS THROW 51022,'TCGDEX_IDENTIDADE_DIVERGENTE',1;
  SELECT @refAnterior=ReferenciaTCGdexId FROM dbo.VinculoCartaTCGdex WITH(UPDLOCK,HOLDLOCK) WHERE CartaId=@CartaId;
  IF @refAnterior IS NOT NULL
  BEGIN
   IF @refAnterior<>@ReferenciaTCGdexId THROW 51022,'TCGDEX_IDENTIDADE_DIVERGENTE',1;
   COMMIT;
   SELECT @CartaId AS CartaId,@ReferenciaTCGdexId AS ReferenciaTCGdexId,CAST(1 AS bit) AS Repetida;
   RETURN;
  END;
  INSERT dbo.VinculoCartaTCGdex(CartaId,ReferenciaTCGdexId,ResponsavelId,NomeConferido,NumeroConferido)
   VALUES(@CartaId,@ReferenciaTCGdexId,@ResponsavelId,@nomeLocal,@numeroLocal);
  COMMIT;
  SELECT @CartaId AS CartaId,@ReferenciaTCGdexId AS ReferenciaTCGdexId,CAST(0 AS bit) AS Repetida;
 END TRY
 BEGIN CATCH
  IF XACT_STATE()<>0 ROLLBACK;
  THROW;
 END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_CadastrarOfertaTCGdex151
 @ResponsavelId int,@ReferenciaTCGdexId int,@Conservacao nvarchar(30),@Preco decimal(12,2),@QuantidadeInicial int
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
 IF @Preco IS NULL OR @Preco<=0 OR @QuantidadeInicial IS NULL OR @QuantidadeInicial<0 OR @Conservacao IS NULL
  OR @Conservacao NOT IN(N'MINT',N'NEAR_MINT',N'SLIGHTLY_PLAYED',N'MODERATELY_PLAYED',N'HEAVILY_PLAYED',N'DAMAGED') THROW 51002,'DADOS_INVALIDOS',1;
 DECLARE @colecaoId int,@cartaId int,@nome nvarchar(120),@numero nvarchar(3),@raridade nvarchar(100),@imagem nvarchar(500);
 BEGIN TRY
  BEGIN TRANSACTION;
  IF NOT EXISTS(SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId WHERE u.UsuarioId=@ResponsavelId AND u.Ativo=1 AND p.Codigo IN(N'ADMINISTRADOR',N'ATENDENTE')) THROW 51000,'ACESSO_NEGADO',1;
  SELECT @colecaoId=ColecaoId FROM dbo.Colecao WITH(HOLDLOCK) WHERE Codigo COLLATE Latin1_General_100_BIN2=N'MEW-151';
  SELECT @nome=Nome,@numero=Numero,@raridade=COALESCE(NULLIF(TRIM(Raridade),N''),N'Não informada (TCGdex)'),@imagem=ImagemUrl
   FROM dbo.ReferenciaTCGdex WITH(HOLDLOCK) WHERE ReferenciaTCGdexId=@ReferenciaTCGdexId AND Detalhada=1 AND SetId=N'sv03.5';
  IF @colecaoId IS NULL OR @nome IS NULL THROW 51022,'TCGDEX_IDENTIDADE_DIVERGENTE',1;
  IF LEN(@raridade)>50 THROW 51020,'TCGDEX_DADOS_INVALIDOS',1;
  IF EXISTS(SELECT 1 FROM dbo.Carta WITH(UPDLOCK,HOLDLOCK) WHERE ColecaoId=@colecaoId AND Numero=@numero AND Conservacao=@Conservacao)
   THROW 51010,'CADASTRO_DUPLICADO',1;
  INSERT dbo.Carta(ColecaoId,Numero,Nome,Raridade,ImagemUrl,Conservacao,Preco,QuantidadeEstoque)
   VALUES(@colecaoId,@numero,@nome,@raridade,@imagem,@Conservacao,@Preco,@QuantidadeInicial);
  SET @cartaId=CONVERT(int,SCOPE_IDENTITY());
  IF @QuantidadeInicial>0 INSERT dbo.MovimentacaoEstoque(CartaId,Tipo,QuantidadeDelta,EstoqueApos,ResponsavelId,Motivo)
   VALUES(@cartaId,N'ENTRADA_INICIAL',@QuantidadeInicial,@QuantidadeInicial,@ResponsavelId,N'Cadastro local por referência TCGdex 151');
  INSERT dbo.VinculoCartaTCGdex(CartaId,ReferenciaTCGdexId,ResponsavelId,NomeConferido,NumeroConferido)
   VALUES(@cartaId,@ReferenciaTCGdexId,@ResponsavelId,@nome,@numero);
  COMMIT;
  SELECT @cartaId AS CartaId,@ReferenciaTCGdexId AS ReferenciaTCGdexId;
 END TRY
 BEGIN CATCH
  DECLARE @erro int=ERROR_NUMBER();
  IF XACT_STATE()<>0 ROLLBACK;
  IF @erro IN(2601,2627) THROW 51010,'CADASTRO_DUPLICADO',1;
  THROW;
 END CATCH;
END;
GO
-- Novos objetos seguem o mesmo proprietario e papel; preservar DENY de DML/ajustes legados.
GRANT SELECT ON OBJECT::dbo.vw_BibliotecaTCGdex151 TO PokeVaultRuntime;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersao WHERE Versao=10) INSERT dbo.SchemaVersao(Versao) VALUES(10);
COMMIT;
GO
