USE [$(DatabaseName)];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=2) OR DATABASE_PRINCIPAL_ID(N'PokeVaultRuntime') IS NULL
    THROW 51102, 'SCHEMA_INESPERADO', 1;
BEGIN TRANSACTION;
DECLARE @lockResult int;
EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Schema.Sprint2',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
IF @lockResult<0 THROW 51104, 'ATUALIZACAO_OCUPADA', 1;
-- sqlcmd -b e obrigatorio; erro de compilacao em batch posterior encerra a conexao e reverte esta transacao.
GO
CREATE OR ALTER PROCEDURE dbo.usp_DefinirSaldoVersionado
    @AdministradorId int, @ClienteId int, @NovoSaldo decimal(12,2), @Motivo nvarchar(300), @VersaoEsperada binary(8)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103, 'TRANSACAO_EXTERNA_NAO_SUPORTADA', 1;
    IF @NovoSaldo IS NULL OR @NovoSaldo<0 OR @Motivo IS NULL OR LEN(TRIM(@Motivo))=0 OR @VersaoEsperada IS NULL
        THROW 51002, 'DADOS_INVALIDOS', 1;
    DECLARE @anterior decimal(12,2),@delta decimal(12,2),@versao binary(8);
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@AdministradorId AND u.Ativo=1 AND p.Codigo=N'ADMINISTRADOR') THROW 51000, 'ACESSO_NEGADO', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ClienteId AND u.Ativo=1 AND p.Codigo=N'CLIENTE') THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        SELECT @anterior=Saldo,@versao=Versao FROM dbo.SaldoCliente WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@ClienteId;
        IF @anterior IS NULL THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        IF @versao<>@VersaoEsperada THROW 51012, 'VERSAO_DESATUALIZADA', 1;
        SET @delta=@NovoSaldo-@anterior;
        IF @delta<>0
        BEGIN
            UPDATE dbo.SaldoCliente SET Saldo=@NovoSaldo WHERE UsuarioId=@ClienteId AND Versao=@VersaoEsperada;
            IF @@ROWCOUNT<>1 THROW 51012, 'VERSAO_DESATUALIZADA', 1;
            INSERT dbo.MovimentacaoSaldo(UsuarioId,Tipo,ValorDelta,SaldoApos,ResponsavelId,Motivo)
                VALUES (@ClienteId,N'AJUSTE_ADMIN',@delta,@NovoSaldo,@AdministradorId,TRIM(@Motivo));
            SELECT @versao=Versao FROM dbo.SaldoCliente WHERE UsuarioId=@ClienteId;
        END;
        COMMIT;
        SELECT @ClienteId AS UsuarioId,@NovoSaldo AS Saldo,@delta AS Delta,@versao AS Versao;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_AjustarCartaVersionado
    @ResponsavelId int,@CartaId int,@Preco decimal(12,2),@NovoEstoque int,@Ativo bit,@Motivo nvarchar(300),@VersaoEsperada binary(8)
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103, 'TRANSACAO_EXTERNA_NAO_SUPORTADA', 1;
    IF @Preco IS NULL OR @Preco<=0 OR @NovoEstoque IS NULL OR @NovoEstoque<0 OR @Ativo IS NULL
        OR @Motivo IS NULL OR LEN(TRIM(@Motivo))=0 OR @VersaoEsperada IS NULL THROW 51002, 'DADOS_INVALIDOS', 1;
    DECLARE @anterior int,@delta int,@precoAnterior decimal(12,2),@ativoAnterior bit,@versao binary(8);
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ResponsavelId AND u.Ativo=1 AND p.Codigo IN (N'ADMINISTRADOR',N'ATENDENTE')) THROW 51000, 'ACESSO_NEGADO', 1;
        SELECT @anterior=QuantidadeEstoque,@precoAnterior=Preco,@ativoAnterior=Ativo,@versao=Versao
            FROM dbo.Carta WITH (UPDLOCK,HOLDLOCK) WHERE CartaId=@CartaId;
        IF @anterior IS NULL THROW 51006, 'CARTA_INDISPONIVEL', 1;
        IF @versao<>@VersaoEsperada THROW 51012, 'VERSAO_DESATUALIZADA', 1;
        SET @delta=@NovoEstoque-@anterior;
        IF @delta<>0 OR @precoAnterior<>@Preco OR @ativoAnterior<>@Ativo
        BEGIN
            UPDATE dbo.Carta SET Preco=@Preco,QuantidadeEstoque=@NovoEstoque,Ativo=@Ativo WHERE CartaId=@CartaId AND Versao=@VersaoEsperada;
            IF @@ROWCOUNT<>1 THROW 51012, 'VERSAO_DESATUALIZADA', 1;
            IF @delta<>0
                INSERT dbo.MovimentacaoEstoque(CartaId,Tipo,QuantidadeDelta,EstoqueApos,ResponsavelId,Motivo)
                    VALUES (@CartaId,N'AJUSTE_ADMIN',@delta,@NovoEstoque,@ResponsavelId,TRIM(@Motivo));
            SELECT @versao=Versao FROM dbo.Carta WHERE CartaId=@CartaId;
        END;
        COMMIT;
        SELECT @CartaId AS CartaId,@Preco AS Preco,@NovoEstoque AS QuantidadeEstoque,@Ativo AS Ativo,@delta AS DeltaEstoque,@versao AS Versao;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
-- Preservar chamadas de instalacao legadas sem permitir bypass nas telas runtime.
DENY EXECUTE ON OBJECT::dbo.usp_DefinirSaldo TO PokeVaultRuntime;
DENY EXECUTE ON OBJECT::dbo.usp_AjustarCarta TO PokeVaultRuntime;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=3)
    INSERT dbo.SchemaVersao(Versao) VALUES (3);
COMMIT;
GO
