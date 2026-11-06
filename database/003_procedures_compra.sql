USE [$(DatabaseName)];
GO
IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
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
        INSERT dbo.ItemCompra(CompraId,CartaId,Quantidade,PrecoUnitario,NomeCartaSnapshot,ConservacaoSnapshot)
            SELECT @compraId,c.CartaId,i.Quantidade,c.Preco,c.Nome,c.Conservacao
              FROM @Itens i JOIN dbo.Carta c ON c.CartaId=i.CartaId;
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
CREATE OR ALTER PROCEDURE dbo.usp_CancelarCompra
    @AdministradorId int,
    @CompraId int,
    @Motivo nvarchar(300)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF @@TRANCOUNT<>0 THROW 51103, 'TRANSACAO_EXTERNA_NAO_SUPORTADA', 1;
    IF @CompraId IS NULL OR @Motivo IS NULL OR LEN(TRIM(@Motivo))=0 THROW 51002, 'DADOS_INVALIDOS', 1;
    DECLARE @clienteId int,@saldo decimal(12,2),@total decimal(12,2),@status nvarchar(10),@cartaId int,@estoque int;
    -- ClienteId e imutavel e compras nunca sao excluidas pela aplicacao.
    SELECT @clienteId=ClienteId FROM dbo.Compra WHERE CompraId=@CompraId;
    IF @clienteId IS NULL THROW 51003, 'COMPRA_NAO_ENCONTRADA', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@AdministradorId AND u.Ativo=1 AND p.Codigo=N'ADMINISTRADOR')
            THROW 51000, 'ACESSO_NEGADO', 1;
        SELECT @saldo=Saldo FROM dbo.SaldoCliente WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@clienteId;
        SELECT @status=Status,@total=Total FROM dbo.Compra WITH (UPDLOCK,HOLDLOCK) WHERE CompraId=@CompraId AND ClienteId=@clienteId;
        IF @saldo IS NULL OR @status IS NULL THROW 51003, 'COMPRA_NAO_ENCONTRADA', 1;
        IF @status=N'CANCELADO'
        BEGIN
            COMMIT;
            SELECT @CompraId AS CompraId,@status AS Status,@total AS Total,@saldo AS SaldoAtual,CAST(1 AS bit) AS Repetida;
            RETURN;
        END;
        IF CONVERT(decimal(13,2),@saldo)+@total>9999999999.99 THROW 51002, 'DADOS_INVALIDOS', 1;
        DECLARE cartas CURSOR LOCAL FAST_FORWARD FOR SELECT CartaId FROM dbo.ItemCompra WHERE CompraId=@CompraId ORDER BY CartaId;
        OPEN cartas;
        FETCH NEXT FROM cartas INTO @cartaId;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SELECT @estoque=QuantidadeEstoque FROM dbo.Carta WITH (UPDLOCK,HOLDLOCK) WHERE CartaId=@cartaId;
            IF EXISTS (SELECT 1 FROM dbo.ItemCompra WHERE CompraId=@CompraId AND CartaId=@cartaId
                AND CONVERT(bigint,Quantidade)+@estoque>2147483647) THROW 51002, 'DADOS_INVALIDOS', 1;
            FETCH NEXT FROM cartas INTO @cartaId;
        END;
        CLOSE cartas;
        DEALLOCATE cartas;
        UPDATE dbo.Compra SET Status=N'CANCELADO',CanceladaEmUtc=SYSUTCDATETIME(),CanceladaPorId=@AdministradorId,MotivoCancelamento=TRIM(@Motivo)
            WHERE CompraId=@CompraId AND Status=N'CONCLUIDO';
        IF @@ROWCOUNT<>1 THROW 51002, 'DADOS_INVALIDOS', 1;
        UPDATE dbo.SaldoCliente SET Saldo=Saldo+@total WHERE UsuarioId=@clienteId;
        SET @saldo=@saldo+@total;
        INSERT dbo.MovimentacaoSaldo(UsuarioId,CompraId,Tipo,ValorDelta,SaldoApos,ResponsavelId,Motivo)
            VALUES (@clienteId,@CompraId,N'ESTORNO',@total,@saldo,@AdministradorId,TRIM(@Motivo));
        UPDATE c SET QuantidadeEstoque=c.QuantidadeEstoque+i.Quantidade
            FROM dbo.Carta c JOIN dbo.ItemCompra i ON i.CartaId=c.CartaId WHERE i.CompraId=@CompraId;
        INSERT dbo.MovimentacaoEstoque(CartaId,CompraId,Tipo,QuantidadeDelta,EstoqueApos,ResponsavelId,Motivo)
            SELECT c.CartaId,@CompraId,N'ESTORNO',i.Quantidade,c.QuantidadeEstoque,@AdministradorId,TRIM(@Motivo)
              FROM dbo.Carta c JOIN dbo.ItemCompra i ON i.CartaId=c.CartaId WHERE i.CompraId=@CompraId;
        COMMIT;
        SELECT @CompraId AS CompraId,CAST(N'CANCELADO' AS nvarchar(10)) AS Status,@total AS Total,@saldo AS SaldoAtual,CAST(0 AS bit) AS Repetida;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH;
END;
GO
