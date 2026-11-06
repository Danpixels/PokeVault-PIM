USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET XACT_ABORT ON; SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;
SET ANSI_PADDING ON; SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN(N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
 THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersao WHERE Versao=10) THROW 51102,'SCHEMA_INESPERADO',1;
BEGIN TRANSACTION;
DECLARE @lockResult int;
EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Schema.Fluxo151',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
IF @lockResult<0 THROW 51104,'ATUALIZACAO_OCUPADA',1;
GO
IF COL_LENGTH('dbo.Carta','Idioma') IS NULL
 ALTER TABLE dbo.Carta ADD Idioma nvarchar(20) NOT NULL CONSTRAINT DF_Carta_Idioma DEFAULT N'NAO_INFORMADO',
 Acabamento nvarchar(20) NOT NULL CONSTRAINT DF_Carta_Acabamento DEFAULT N'NAO_INFORMADO';
IF COL_LENGTH('dbo.ItemCompra','IdiomaSnapshot') IS NULL
 ALTER TABLE dbo.ItemCompra ADD IdiomaSnapshot nvarchar(20) NULL,AcabamentoSnapshot nvarchar(20) NULL,
 ColecaoSnapshot nvarchar(120) NULL,NumeroSnapshot nvarchar(20) NULL;
GO
IF EXISTS(SELECT 1 FROM sys.key_constraints WHERE name=N'UQ_Carta_Comercial')
 ALTER TABLE dbo.Carta DROP CONSTRAINT UQ_Carta_Comercial;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Carta') AND name=N'UQ_Carta_Variante')
 ALTER TABLE dbo.Carta ADD CONSTRAINT UQ_Carta_Variante UNIQUE(ColecaoId,Numero,Conservacao,Idioma,Acabamento);
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_Carta_Variante')
 ALTER TABLE dbo.Carta ADD CONSTRAINT CK_Carta_Variante CHECK
 (Idioma IN(N'NAO_INFORMADO',N'pt',N'en',N'ja') AND Acabamento IN(N'NAO_INFORMADO',N'NORMAL',N'HOLO',N'REVERSE_HOLO'));
GO
CREATE OR ALTER PROCEDURE dbo.usp_CadastrarOfertaTCGdex151
 @ResponsavelId int,@ReferenciaTCGdexId int,@Conservacao nvarchar(30),@Preco decimal(12,2),@QuantidadeInicial int,@Idioma nvarchar(20)=N'NAO_INFORMADO',@Acabamento nvarchar(20)=N'NAO_INFORMADO'
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
 IF @Preco IS NULL OR @Preco<=0 OR @QuantidadeInicial IS NULL OR @QuantidadeInicial<0 OR @Conservacao IS NULL
  OR @Conservacao NOT IN(N'MINT',N'NEAR_MINT',N'SLIGHTLY_PLAYED',N'MODERATELY_PLAYED',N'HEAVILY_PLAYED',N'DAMAGED') THROW 51002,'DADOS_INVALIDOS',1;
 IF @Idioma IS NULL OR @Idioma NOT IN(N'NAO_INFORMADO',N'pt',N'en',N'ja') OR @Acabamento IS NULL OR @Acabamento NOT IN(N'NAO_INFORMADO',N'NORMAL',N'HOLO',N'REVERSE_HOLO') THROW 51002,'DADOS_INVALIDOS',1;
 DECLARE @colecaoId int,@cartaId int,@nome nvarchar(120),@numero nvarchar(3),@raridade nvarchar(100),@imagem nvarchar(500);
 BEGIN TRY
  BEGIN TRANSACTION;
  IF NOT EXISTS(SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId WHERE u.UsuarioId=@ResponsavelId AND u.Ativo=1 AND p.Codigo IN(N'ADMINISTRADOR',N'ATENDENTE')) THROW 51000,'ACESSO_NEGADO',1;
  SELECT @colecaoId=ColecaoId FROM dbo.Colecao WITH(HOLDLOCK) WHERE Codigo COLLATE Latin1_General_100_BIN2=N'MEW-151';
  SELECT @nome=Nome,@numero=Numero,@raridade=COALESCE(NULLIF(TRIM(Raridade),N''),N'Não informada (TCGdex)'),@imagem=ImagemUrl
   FROM dbo.ReferenciaTCGdex WITH(HOLDLOCK) WHERE ReferenciaTCGdexId=@ReferenciaTCGdexId AND Detalhada=1 AND SetId=N'sv03.5';
  IF @colecaoId IS NULL OR @nome IS NULL THROW 51022,'TCGDEX_IDENTIDADE_DIVERGENTE',1;
  IF LEN(@raridade)>50 THROW 51020,'TCGDEX_DADOS_INVALIDOS',1;
  IF EXISTS(SELECT 1 FROM dbo.Carta WITH(UPDLOCK,HOLDLOCK) WHERE ColecaoId=@colecaoId AND Numero=@numero AND Conservacao=@Conservacao AND Idioma=@Idioma AND Acabamento=@Acabamento)
   THROW 51010,'CADASTRO_DUPLICADO',1;
  INSERT dbo.Carta(ColecaoId,Numero,Nome,Raridade,ImagemUrl,Conservacao,Preco,QuantidadeEstoque,Idioma,Acabamento)
   VALUES(@colecaoId,@numero,@nome,@raridade,@imagem,@Conservacao,@Preco,@QuantidadeInicial,@Idioma,@Acabamento);
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
CREATE OR ALTER PROCEDURE dbo.usp_ConcluirCompra
    @ClienteId int,
    @ChaveOperacao uniqueidentifier,
    @Itens dbo.ItemCompraSelecionadoType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103, 'TRANSACAO_EXTERNA_NAO_SUPORTADA', 1;
    IF @ClienteId IS NULL OR @ChaveOperacao IS NULL OR NOT EXISTS (SELECT 1 FROM @Itens)
       OR EXISTS (SELECT 1 FROM @Itens WHERE Quantidade<=0 OR PrecoConfirmado<=0)
        THROW 51002, 'DADOS_INVALIDOS', 1;
    DECLARE @saldo decimal(12,2), @total decimal(12,2), @totalAmplo decimal(38,2),
        @compraId int, @status nvarchar(10), @resumo nvarchar(max), @anterior nvarchar(max),
        @carrinhoId int, @cartaId int, @quantidade int, @precoConfirmado decimal(12,2),
        @preco decimal(12,2), @estoque int, @ativo bit;
    SELECT @resumo = (SELECT CartaId,Quantidade,PrecoConfirmado FROM @Itens ORDER BY CartaId FOR JSON PATH);
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ClienteId AND u.Ativo=1 AND p.Codigo=N'CLIENTE')
            THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        SELECT @saldo=Saldo FROM dbo.SaldoCliente WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@ClienteId;
        IF @saldo IS NULL THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        -- Serializacao por carteira protege tambem chaves ainda inexistentes.
        SELECT @compraId=CompraId, @status=Status, @total=Total, @anterior=ResumoRequisicao
          FROM dbo.Compra WHERE ClienteId=@ClienteId AND ChaveOperacao=@ChaveOperacao;
        IF @compraId IS NOT NULL
        BEGIN
            IF @anterior COLLATE Latin1_General_100_BIN2 <> @resumo COLLATE Latin1_General_100_BIN2
                THROW 51004, 'CHAVE_REUTILIZADA', 1;
            COMMIT;
            SELECT @compraId AS CompraId,@status AS Status,@total AS Total,@saldo AS SaldoAtual,CAST(1 AS bit) AS Repetida;
            RETURN;
        END;
        SELECT @carrinhoId=CarrinhoId FROM dbo.Carrinho WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@ClienteId;
        IF @carrinhoId IS NULL OR EXISTS (
            SELECT 1 FROM @Itens i LEFT JOIN dbo.ItemCarrinho c ON c.CarrinhoId=@carrinhoId AND c.CartaId=i.CartaId
            WHERE c.CartaId IS NULL OR c.Quantidade<>i.Quantidade)
            THROW 51005, 'CARRINHO_ALTERADO', 1;
        -- Um seek bloqueado por vez em ordem de ID; ORDER BY de um UPDATE nao garante ordem de locks.
        DECLARE cartas CURSOR LOCAL FAST_FORWARD FOR SELECT CartaId,Quantidade,PrecoConfirmado FROM @Itens ORDER BY CartaId;
        OPEN cartas;
        FETCH NEXT FROM cartas INTO @cartaId,@quantidade,@precoConfirmado;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @preco=NULL;
            SELECT @preco=Preco,@estoque=QuantidadeEstoque,@ativo=Ativo
              FROM dbo.Carta WITH (UPDLOCK,HOLDLOCK) WHERE CartaId=@cartaId;
            IF @preco IS NULL OR @ativo=0 THROW 51006, 'CARTA_INDISPONIVEL', 1;
            IF @preco<>@precoConfirmado THROW 51007, 'PRECO_ALTERADO', 1;
            IF @estoque<@quantidade THROW 51008, 'ESTOQUE_INSUFICIENTE', 1;
            FETCH NEXT FROM cartas INTO @cartaId,@quantidade,@precoConfirmado;
        END;
        CLOSE cartas;
        DEALLOCATE cartas;
        SELECT @totalAmplo=SUM(CONVERT(decimal(12,2),Quantidade)*PrecoConfirmado) FROM @Itens;
        IF @totalAmplo<=0 OR @totalAmplo>9999999999.99 THROW 51002, 'DADOS_INVALIDOS', 1;
        SET @total=CONVERT(decimal(12,2),@totalAmplo);
        IF @saldo<@total THROW 51009, 'SALDO_INSUFICIENTE', 1;
        INSERT dbo.Compra(ClienteId,ChaveOperacao,ResumoRequisicao,Status,Total)
            VALUES (@ClienteId,@ChaveOperacao,@resumo,N'CONCLUIDO',@total);
        SET @compraId=CONVERT(int,SCOPE_IDENTITY());
        INSERT dbo.ItemCompra(CompraId,CartaId,Quantidade,PrecoUnitario,NomeCartaSnapshot,ConservacaoSnapshot,IdiomaSnapshot,AcabamentoSnapshot,ColecaoSnapshot,NumeroSnapshot)
            SELECT @compraId,c.CartaId,i.Quantidade,c.Preco,c.Nome,c.Conservacao,c.Idioma,c.Acabamento,co.Nome,c.Numero
              FROM @Itens i JOIN dbo.Carta c ON c.CartaId=i.CartaId JOIN dbo.Colecao co ON co.ColecaoId=c.ColecaoId;
        UPDATE dbo.SaldoCliente SET Saldo=Saldo-@total WHERE UsuarioId=@ClienteId AND Saldo>=@total;
        IF @@ROWCOUNT<>1 THROW 51009, 'SALDO_INSUFICIENTE', 1;
        SET @saldo=@saldo-@total;
        INSERT dbo.MovimentacaoSaldo(UsuarioId,CompraId,Tipo,ValorDelta,SaldoApos,ResponsavelId)
            VALUES (@ClienteId,@compraId,N'COMPRA',-@total,@saldo,@ClienteId);
        UPDATE c SET QuantidadeEstoque=c.QuantidadeEstoque-i.Quantidade
          FROM dbo.Carta c JOIN @Itens i ON i.CartaId=c.CartaId WHERE c.QuantidadeEstoque>=i.Quantidade;
        IF @@ROWCOUNT<>(SELECT COUNT(*) FROM @Itens) THROW 51008, 'ESTOQUE_INSUFICIENTE', 1;
        INSERT dbo.MovimentacaoEstoque(CartaId,CompraId,Tipo,QuantidadeDelta,EstoqueApos,ResponsavelId)
            SELECT c.CartaId,@compraId,N'COMPRA',-i.Quantidade,c.QuantidadeEstoque,@ClienteId
              FROM dbo.Carta c JOIN @Itens i ON i.CartaId=c.CartaId;
        DELETE c FROM dbo.ItemCarrinho c JOIN @Itens i ON i.CartaId=c.CartaId WHERE c.CarrinhoId=@carrinhoId;
        COMMIT;
        SELECT @compraId AS CompraId,CAST(N'CONCLUIDO' AS nvarchar(10)) AS Status,@total AS Total,@saldo AS SaldoAtual,CAST(0 AS bit) AS Repetida;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_AdicionarItemCarrinho
 @ClienteId int,@CartaId int,@Quantidade int
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
 IF @Quantidade IS NULL OR @Quantidade<1 OR @Quantidade>10000 THROW 51002,'DADOS_INVALIDOS',1;
 DECLARE @carrinhoId int,@atual int=0,@estoque int;
 BEGIN TRY
  BEGIN TRANSACTION;
  IF NOT EXISTS(SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId WHERE u.UsuarioId=@ClienteId AND u.Ativo=1 AND p.Codigo=N'CLIENTE') THROW 51001,'USUARIO_INATIVO_OU_INVALIDO',1;
  -- Mesmo inicio/ordem de bloqueio da compra: carteira, carrinho, carta.
  DECLARE @saldo decimal(12,2);
  SELECT @saldo=Saldo FROM dbo.SaldoCliente WITH(UPDLOCK,HOLDLOCK) WHERE UsuarioId=@ClienteId;
  IF @saldo IS NULL THROW 51001,'USUARIO_INATIVO_OU_INVALIDO',1;
  SELECT @carrinhoId=CarrinhoId FROM dbo.Carrinho WITH(UPDLOCK,HOLDLOCK) WHERE UsuarioId=@ClienteId;
  IF @carrinhoId IS NULL BEGIN INSERT dbo.Carrinho(UsuarioId) VALUES(@ClienteId); SET @carrinhoId=CONVERT(int,SCOPE_IDENTITY()); END;
  SELECT @estoque=QuantidadeEstoque FROM dbo.Carta WITH(HOLDLOCK) WHERE CartaId=@CartaId AND Ativo=1;
  IF @estoque IS NULL THROW 51006,'CARTA_INDISPONIVEL',1;
  SELECT @atual=Quantidade FROM dbo.ItemCarrinho WHERE CarrinhoId=@carrinhoId AND CartaId=@CartaId;
  IF @atual+@Quantidade>10000 THROW 51002,'DADOS_INVALIDOS',1;
  IF @atual+@Quantidade>@estoque THROW 51008,'ESTOQUE_INSUFICIENTE',1;
  IF @atual=0 INSERT dbo.ItemCarrinho(CarrinhoId,CartaId,Quantidade) VALUES(@carrinhoId,@CartaId,@Quantidade);
  ELSE UPDATE dbo.ItemCarrinho SET Quantidade=Quantidade+@Quantidade WHERE CarrinhoId=@carrinhoId AND CartaId=@CartaId;
  COMMIT;
 END TRY BEGIN CATCH IF XACT_STATE()<>0 ROLLBACK; THROW; END CATCH;
END;
GO
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersao WHERE Versao=11) INSERT dbo.SchemaVersao(Versao) VALUES(11);
COMMIT;
GO
