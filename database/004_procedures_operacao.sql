USE [$(DatabaseName)];
GO
IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
GO
CREATE OR ALTER PROCEDURE dbo.usp_DefinirSaldo
    @AdministradorId int, @ClienteId int, @NovoSaldo decimal(12,2), @Motivo nvarchar(300)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103, 'TRANSACAO_EXTERNA_NAO_SUPORTADA', 1;
    IF @NovoSaldo IS NULL OR @NovoSaldo<0 OR @Motivo IS NULL OR LEN(TRIM(@Motivo))=0 THROW 51002, 'DADOS_INVALIDOS', 1;
    DECLARE @anterior decimal(12,2),@delta decimal(12,2);
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@AdministradorId AND u.Ativo=1 AND p.Codigo=N'ADMINISTRADOR') THROW 51000, 'ACESSO_NEGADO', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ClienteId AND u.Ativo=1 AND p.Codigo=N'CLIENTE') THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        SELECT @anterior=Saldo FROM dbo.SaldoCliente WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@ClienteId;
        IF @anterior IS NULL THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        SET @delta=@NovoSaldo-@anterior;
        IF @delta<>0
        BEGIN
            UPDATE dbo.SaldoCliente SET Saldo=@NovoSaldo WHERE UsuarioId=@ClienteId;
            INSERT dbo.MovimentacaoSaldo(UsuarioId,Tipo,ValorDelta,SaldoApos,ResponsavelId,Motivo)
                VALUES (@ClienteId,N'AJUSTE_ADMIN',@delta,@NovoSaldo,@AdministradorId,TRIM(@Motivo));
        END;
        COMMIT;
        SELECT @ClienteId AS UsuarioId,@NovoSaldo AS Saldo,@delta AS Delta;
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
    IF @@TRANCOUNT<>0 THROW 51103, 'TRANSACAO_EXTERNA_NAO_SUPORTADA', 1;
    IF @Numero IS NULL OR LEN(TRIM(@Numero))=0 OR @Nome IS NULL OR LEN(TRIM(@Nome))=0
        OR @Raridade IS NULL OR LEN(TRIM(@Raridade))=0 OR @Preco IS NULL OR @Preco<=0
        OR @QuantidadeInicial IS NULL OR @QuantidadeInicial<0 OR @Conservacao IS NULL
        OR @Conservacao NOT IN (N'MINT',N'NEAR_MINT',N'SLIGHTLY_PLAYED',N'MODERATELY_PLAYED',N'HEAVILY_PLAYED',N'DAMAGED')
        THROW 51002, 'DADOS_INVALIDOS', 1;
    DECLARE @cartaId int;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ResponsavelId AND u.Ativo=1 AND p.Codigo IN (N'ADMINISTRADOR',N'ATENDENTE')) THROW 51000, 'ACESSO_NEGADO', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Colecao WHERE ColecaoId=@ColecaoId) THROW 51002, 'DADOS_INVALIDOS', 1;
        IF EXISTS (SELECT 1 FROM dbo.Carta WHERE ColecaoId=@ColecaoId AND Numero=TRIM(@Numero) AND Conservacao=@Conservacao)
            THROW 51010, 'CADASTRO_DUPLICADO', 1;
        INSERT dbo.Carta(ColecaoId,Numero,Nome,Raridade,ImagemUrl,Conservacao,Preco,QuantidadeEstoque)
            VALUES (@ColecaoId,TRIM(@Numero),TRIM(@Nome),TRIM(@Raridade),@ImagemUrl,@Conservacao,@Preco,@QuantidadeInicial);
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
CREATE OR ALTER PROCEDURE dbo.usp_AjustarCarta
    @ResponsavelId int,@CartaId int,@Preco decimal(12,2),@NovoEstoque int,@Ativo bit,@Motivo nvarchar(300)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103, 'TRANSACAO_EXTERNA_NAO_SUPORTADA', 1;
    IF @Preco IS NULL OR @Preco<=0 OR @NovoEstoque IS NULL OR @NovoEstoque<0 OR @Ativo IS NULL
        OR @Motivo IS NULL OR LEN(TRIM(@Motivo))=0 THROW 51002, 'DADOS_INVALIDOS', 1;
    DECLARE @anterior int,@delta int;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ResponsavelId AND u.Ativo=1 AND p.Codigo IN (N'ADMINISTRADOR',N'ATENDENTE')) THROW 51000, 'ACESSO_NEGADO', 1;
        SELECT @anterior=QuantidadeEstoque FROM dbo.Carta WITH (UPDLOCK,HOLDLOCK) WHERE CartaId=@CartaId;
        IF @anterior IS NULL THROW 51006, 'CARTA_INDISPONIVEL', 1;
        SET @delta=@NovoEstoque-@anterior;
        UPDATE dbo.Carta SET Preco=@Preco,QuantidadeEstoque=@NovoEstoque,Ativo=@Ativo WHERE CartaId=@CartaId;
        IF @delta<>0
            INSERT dbo.MovimentacaoEstoque(CartaId,Tipo,QuantidadeDelta,EstoqueApos,ResponsavelId,Motivo)
                VALUES (@CartaId,N'AJUSTE_ADMIN',@delta,@NovoEstoque,@ResponsavelId,TRIM(@Motivo));
        COMMIT;
        SELECT @CartaId AS CartaId,@Preco AS Preco,@NovoEstoque AS QuantidadeEstoque,@Ativo AS Ativo,@delta AS DeltaEstoque;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_CriarUsuario
    @AdministradorId int,@Nome nvarchar(120),@Email nvarchar(254),@EmailNormalizado nvarchar(254),@SenhaHash nvarchar(512),@PerfilCodigo nvarchar(20)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103, 'TRANSACAO_EXTERNA_NAO_SUPORTADA', 1;
    IF @Nome IS NULL OR LEN(TRIM(@Nome))=0 OR @Email IS NULL OR LEN(TRIM(@Email))=0
        OR @EmailNormalizado IS NULL OR LEN(TRIM(@EmailNormalizado))=0 OR @SenhaHash IS NULL OR LEN(@SenhaHash)=0
        OR @PerfilCodigo IS NULL OR @PerfilCodigo NOT IN (N'CLIENTE',N'ATENDENTE') THROW 51002, 'DADOS_INVALIDOS', 1;
    DECLARE @usuarioId int,@perfilId int;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@AdministradorId AND u.Ativo=1 AND p.Codigo=N'ADMINISTRADOR') THROW 51000, 'ACESSO_NEGADO', 1;
        IF EXISTS (SELECT 1 FROM dbo.Usuario WHERE EmailNormalizado=@EmailNormalizado) THROW 51010, 'CADASTRO_DUPLICADO', 1;
        SELECT @perfilId=PerfilId FROM dbo.Perfil WHERE Codigo=@PerfilCodigo;
        IF @perfilId IS NULL THROW 51002, 'DADOS_INVALIDOS', 1;
        INSERT dbo.Usuario(PerfilId,Nome,Email,EmailNormalizado,SenhaHash)
            VALUES (@perfilId,TRIM(@Nome),TRIM(@Email),@EmailNormalizado,@SenhaHash);
        SET @usuarioId=CONVERT(int,SCOPE_IDENTITY());
        IF @PerfilCodigo=N'CLIENTE'
        BEGIN
            INSERT dbo.SaldoCliente(UsuarioId,Saldo) VALUES (@usuarioId,0);
            INSERT dbo.Carrinho(UsuarioId) VALUES (@usuarioId);
        END;
        COMMIT;
        SELECT @usuarioId AS UsuarioId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_DefinirUsuarioAtivo
    @AdministradorId int,@UsuarioId int,@Ativo bit
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103, 'TRANSACAO_EXTERNA_NAO_SUPORTADA', 1;
    IF @Ativo IS NULL THROW 51002, 'DADOS_INVALIDOS', 1;
    DECLARE @fixo bit;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@AdministradorId AND u.Ativo=1 AND p.Codigo=N'ADMINISTRADOR') THROW 51000, 'ACESSO_NEGADO', 1;
        SELECT @fixo=AdministradorFixo FROM dbo.Usuario WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@UsuarioId;
        IF @fixo IS NULL THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        IF @fixo=1 AND @Ativo=0 THROW 51011, 'ADMINISTRADOR_FIXO', 1;
        UPDATE dbo.Usuario SET Ativo=@Ativo WHERE UsuarioId=@UsuarioId;
        COMMIT;
        SELECT @UsuarioId AS UsuarioId,@Ativo AS Ativo;
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
    IF @@TRANCOUNT<>0 THROW 51103, 'TRANSACAO_EXTERNA_NAO_SUPORTADA', 1;
    IF @CartaId IS NULL OR @Quantidade IS NULL OR @Quantidade<0 THROW 51002, 'DADOS_INVALIDOS', 1;
    DECLARE @carrinhoId int;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ClienteId AND u.Ativo=1 AND p.Codigo=N'CLIENTE') THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        SELECT @carrinhoId=CarrinhoId FROM dbo.Carrinho WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@ClienteId;
        IF @carrinhoId IS NULL THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        IF @Quantidade=0
            DELETE dbo.ItemCarrinho WHERE CarrinhoId=@carrinhoId AND CartaId=@CartaId;
        ELSE
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM dbo.Carta WHERE CartaId=@CartaId AND Ativo=1) THROW 51006, 'CARTA_INDISPONIVEL', 1;
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
