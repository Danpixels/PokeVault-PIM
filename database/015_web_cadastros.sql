USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=5) OR DATABASE_PRINCIPAL_ID(N'PokeVaultRuntime') IS NULL
    THROW 51102,'SCHEMA_INESPERADO',1;
BEGIN TRANSACTION;
DECLARE @lockResult int;
EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Schema.ComplementoWeb',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
IF @lockResult<0 THROW 51104,'ATUALIZACAO_OCUPADA',1;
GO
CREATE OR ALTER PROCEDURE dbo.usp_CadastrarColecao
    @ResponsavelId int,@Nome nvarchar(120),@QuantidadeTotalCartas int
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
    IF @Nome IS NULL OR LEN(TRIM(@Nome))=0 OR @QuantidadeTotalCartas IS NULL OR @QuantidadeTotalCartas<=0
        THROW 51002,'DADOS_INVALIDOS',1;
    DECLARE @codigo nvarchar(30),@colecaoId int;
    SET @Nome=TRIM(@Nome);
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ResponsavelId AND u.Ativo=1 AND p.Codigo IN (N'ADMINISTRADOR',N'ATENDENTE'))
            THROW 51000,'ACESSO_NEGADO',1;
        SET @codigo=N'COL-'+LEFT(REPLACE(CONVERT(nvarchar(36),NEWID()),N'-',N''),26);
        INSERT dbo.Colecao(Codigo,Nome,QuantidadeTotalCartas) VALUES (@codigo,@Nome,@QuantidadeTotalCartas);
        SET @colecaoId=CONVERT(int,SCOPE_IDENTITY());
        COMMIT;
        SELECT @colecaoId AS ColecaoId,@codigo AS Codigo,@Nome AS Nome,@QuantidadeTotalCartas AS QuantidadeTotalCartas;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_CadastrarCarta
    @ResponsavelId int,@ColecaoId int,@Numero nvarchar(20),@Nome nvarchar(120),@Raridade nvarchar(50),
    @Conservacao nvarchar(30),@Preco decimal(12,2),@QuantidadeInicial int=0,@ImagemUrl nvarchar(500)=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
    IF @Numero IS NULL OR LEN(TRIM(@Numero))=0 OR @Nome IS NULL OR LEN(TRIM(@Nome))=0
        OR @Preco IS NULL OR @Preco<=0 OR @QuantidadeInicial IS NULL OR @QuantidadeInicial<0 OR @Conservacao IS NULL
        OR @Conservacao NOT IN (N'MINT',N'NEAR_MINT',N'SLIGHTLY_PLAYED',N'MODERATELY_PLAYED',N'HEAVILY_PLAYED',N'DAMAGED')
        THROW 51002,'DADOS_INVALIDOS',1;
    SET @Raridade=TRIM(@Raridade);
    IF @Raridade IS NULL OR @Raridade COLLATE Latin1_General_100_BIN2 NOT IN
        (N'Common',N'Uncommon',N'Rare',N'Double Rare',N'Ace Spec Rare',N'Illustration Rare',
         N'Ultra Rare',N'Special Illustration Rare',N'Hyper Rare',N'Shiny Rare',N'Shiny Ultra Rare',N'Promo')
        THROW 51014,'RARIDADE_INVALIDA',1;
    DECLARE @cartaId int;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ResponsavelId AND u.Ativo=1 AND p.Codigo IN (N'ADMINISTRADOR',N'ATENDENTE')) THROW 51000,'ACESSO_NEGADO',1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Colecao WHERE ColecaoId=@ColecaoId) THROW 51002,'DADOS_INVALIDOS',1;
        IF EXISTS (SELECT 1 FROM dbo.Carta WHERE ColecaoId=@ColecaoId AND Numero=TRIM(@Numero) AND Conservacao=@Conservacao)
            THROW 51010,'CADASTRO_DUPLICADO',1;
        INSERT dbo.Carta(ColecaoId,Numero,Nome,Raridade,ImagemUrl,Conservacao,Preco,QuantidadeEstoque)
            VALUES (@ColecaoId,TRIM(@Numero),TRIM(@Nome),@Raridade,@ImagemUrl,@Conservacao,@Preco,@QuantidadeInicial);
        SET @cartaId=CONVERT(int,SCOPE_IDENTITY());
        IF @QuantidadeInicial>0
            INSERT dbo.MovimentacaoEstoque(CartaId,Tipo,QuantidadeDelta,EstoqueApos,ResponsavelId,Motivo)
                VALUES (@cartaId,N'ENTRADA_INICIAL',@QuantidadeInicial,@QuantidadeInicial,@ResponsavelId,N'Cadastro de carta');
        COMMIT;
        SELECT @cartaId AS CartaId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=6) INSERT dbo.SchemaVersao(Versao) VALUES (6);
COMMIT;
GO
