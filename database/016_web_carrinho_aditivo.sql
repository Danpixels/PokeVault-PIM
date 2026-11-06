USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=6) THROW 51102,'SCHEMA_INESPERADO',1;
BEGIN TRANSACTION;
DECLARE @lockResult int;
EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Schema.ComplementoWeb',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
IF @lockResult<0 THROW 51104,'ATUALIZACAO_OCUPADA',1;
GO
CREATE OR ALTER PROCEDURE dbo.usp_AdicionarItemCarrinho
    @ClienteId int,@CartaId int,@QuantidadeAdicionar int
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
    IF @CartaId IS NULL OR @QuantidadeAdicionar IS NULL OR @QuantidadeAdicionar<1 OR @QuantidadeAdicionar>10000
        THROW 51002,'DADOS_INVALIDOS',1;
    DECLARE @carrinhoId int,@quantidadeAnterior int=0,@novaQuantidade bigint;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ClienteId AND u.Ativo=1 AND p.Codigo=N'CLIENTE') THROW 51001,'USUARIO_INATIVO_OU_INVALIDO',1;
        SELECT @carrinhoId=CarrinhoId FROM dbo.Carrinho WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@ClienteId;
        IF @carrinhoId IS NULL THROW 51001,'USUARIO_INATIVO_OU_INVALIDO',1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Carta WITH (HOLDLOCK) WHERE CartaId=@CartaId AND Ativo=1) THROW 51006,'CARTA_INDISPONIVEL',1;
        SELECT @quantidadeAnterior=Quantidade FROM dbo.ItemCarrinho WHERE CarrinhoId=@carrinhoId AND CartaId=@CartaId;
        SET @novaQuantidade=CONVERT(bigint,@quantidadeAnterior)+CONVERT(bigint,@QuantidadeAdicionar);
        IF @novaQuantidade>10000 THROW 51013,'QUANTIDADE_CARRINHO_EXCEDIDA',1;
        UPDATE dbo.ItemCarrinho SET Quantidade=CONVERT(int,@novaQuantidade) WHERE CarrinhoId=@carrinhoId AND CartaId=@CartaId;
        IF @@ROWCOUNT=0 INSERT dbo.ItemCarrinho(CarrinhoId,CartaId,Quantidade) VALUES (@carrinhoId,@CartaId,CONVERT(int,@novaQuantidade));
        COMMIT;
        SELECT @carrinhoId AS CarrinhoId,@CartaId AS CartaId,CONVERT(int,@novaQuantidade) AS Quantidade;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_DefinirItemCarrinho
    @ClienteId int,@CartaId int,@Quantidade int
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
    IF @CartaId IS NULL OR @Quantidade IS NULL OR @Quantidade<0 OR @Quantidade>10000 THROW 51002,'DADOS_INVALIDOS',1;
    DECLARE @carrinhoId int;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ClienteId AND u.Ativo=1 AND p.Codigo=N'CLIENTE') THROW 51001,'USUARIO_INATIVO_OU_INVALIDO',1;
        SELECT @carrinhoId=CarrinhoId FROM dbo.Carrinho WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@ClienteId;
        IF @carrinhoId IS NULL THROW 51001,'USUARIO_INATIVO_OU_INVALIDO',1;
        IF @Quantidade=0
            DELETE dbo.ItemCarrinho WHERE CarrinhoId=@carrinhoId AND CartaId=@CartaId;
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.Carta WHERE CartaId=@CartaId AND Ativo=1) THROW 51006,'CARTA_INDISPONIVEL',1;
            UPDATE dbo.ItemCarrinho SET Quantidade=@Quantidade WHERE CarrinhoId=@carrinhoId AND CartaId=@CartaId;
            IF @@ROWCOUNT=0 INSERT dbo.ItemCarrinho(CarrinhoId,CartaId,Quantidade) VALUES (@carrinhoId,@CartaId,@Quantidade);
        END;
        COMMIT;
        SELECT @carrinhoId AS CarrinhoId,@CartaId AS CartaId,@Quantidade AS Quantidade;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=7) INSERT dbo.SchemaVersao(Versao) VALUES (7);
COMMIT;
GO
