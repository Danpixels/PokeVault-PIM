-- Complemento aditivo: motivo/autor tambem para alteracoes somente de preco/ativacao.
-- Nao modifica o script 009 ja executado, nem cria movimentos de estoque com delta zero.
USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=3)
    THROW 51102, 'SCHEMA_INESPERADO', 1;
BEGIN TRANSACTION;
DECLARE @lockResult int;
EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Schema.Sprint2',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
IF @lockResult<0 THROW 51104, 'ATUALIZACAO_OCUPADA', 1;
IF OBJECT_ID(N'dbo.AlteracaoCarta',N'U') IS NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=4) THROW 51102, 'SCHEMA_INESPERADO', 1;
    CREATE TABLE dbo.AlteracaoCarta (
        AlteracaoCartaId int IDENTITY PRIMARY KEY,
        CartaId int NOT NULL REFERENCES dbo.Carta(CartaId),
        ResponsavelId int NOT NULL REFERENCES dbo.Usuario(UsuarioId),
        Motivo nvarchar(300) NOT NULL,
        PrecoAnterior decimal(12,2) NOT NULL,
        PrecoNovo decimal(12,2) NOT NULL,
        EstoqueAnterior int NOT NULL,
        EstoqueNovo int NOT NULL,
        AtivoAnterior bit NOT NULL,
        AtivoNovo bit NOT NULL,
        CriadaEmUtc datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_AlteracaoCarta_Valores CHECK (PrecoAnterior>0 AND PrecoNovo>0 AND EstoqueAnterior>=0 AND EstoqueNovo>=0 AND LEN(TRIM(Motivo))>0),
        CONSTRAINT CK_AlteracaoCarta_Mudanca CHECK (PrecoAnterior<>PrecoNovo OR EstoqueAnterior<>EstoqueNovo OR AtivoAnterior<>AtivoNovo)
    );
    CREATE INDEX IX_AlteracaoCarta_Historico ON dbo.AlteracaoCarta(CartaId,CriadaEmUtc);
END;
ELSE IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=4)
    THROW 51102, 'SCHEMA_INESPERADO', 1;
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
            INSERT dbo.AlteracaoCarta(CartaId,ResponsavelId,Motivo,PrecoAnterior,PrecoNovo,EstoqueAnterior,EstoqueNovo,AtivoAnterior,AtivoNovo)
                VALUES (@CartaId,@ResponsavelId,TRIM(@Motivo),@precoAnterior,@Preco,@anterior,@NovoEstoque,@ativoAnterior,@Ativo);
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
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=4)
    INSERT dbo.SchemaVersao(Versao) VALUES (4);
COMMIT;
GO
