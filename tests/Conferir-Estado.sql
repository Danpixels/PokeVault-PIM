-- PREPARACAO: consultas de leitura, NAO EXECUTADAS em SQL Server nesta rodada.
-- Conferir nomes contra database/001_schema.sql real antes de executar.
-- Contrato: docs/contrato-sprint1.md. Nao conectar base desconhecida.
-- Substituir NULL por IDs positivos da massa propria. Nao contem reset/DML.
SET NOCOUNT ON;
IF DB_NAME() <> N'PokeVaultTests'
    THROW 51100, 'Consulta restrita a PokeVaultTests; confirmar base exclusiva.', 1;
DECLARE @ClienteId int = NULL;
DECLARE @CompraId int = NULL;
IF @ClienteId IS NULL OR @ClienteId <= 0
    THROW 51101, 'Preencher ClienteId positivo da massa controlada.', 1;
SELECT DB_NAME() AS Banco, SYSUTCDATETIME() AS CapturadoEmUtc;
SELECT UsuarioId, Saldo FROM dbo.SaldoCliente WHERE UsuarioId = @ClienteId;
SELECT c.CarrinhoId, i.CartaId, i.Quantidade, a.Conservacao, a.Preco, a.QuantidadeEstoque
FROM dbo.Carrinho c
JOIN dbo.ItemCarrinho i ON i.CarrinhoId = c.CarrinhoId
JOIN dbo.Carta a ON a.CartaId = i.CartaId
WHERE c.UsuarioId = @ClienteId ORDER BY i.CartaId;
SELECT CartaId, Nome, Conservacao, Preco, QuantidadeEstoque, Ativo
FROM dbo.Carta ORDER BY CartaId;
SELECT CompraId, ClienteId, ChaveOperacao, Status, Total, CriadaEmUtc,
       CanceladaEmUtc, CanceladaPorId, MotivoCancelamento
FROM dbo.Compra WHERE ClienteId = @ClienteId ORDER BY CompraId;
IF @CompraId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Compra WHERE CompraId = @CompraId AND ClienteId = @ClienteId)
        THROW 51102, 'Compra nao pertence ao cliente informado.', 1;
    SELECT CompraId, CartaId, Quantidade, PrecoUnitario,
           NomeCartaSnapshot, ConservacaoSnapshot
    FROM dbo.ItemCompra WHERE CompraId = @CompraId ORDER BY CartaId;
    SELECT CompraId, Tipo, COUNT_BIG(*) AS QuantidadeMovimentos, SUM(ValorDelta) AS Delta
    FROM dbo.MovimentacaoSaldo WHERE CompraId = @CompraId GROUP BY CompraId, Tipo;
    SELECT CompraId, CartaId, Tipo, COUNT_BIG(*) AS QuantidadeMovimentos, SUM(QuantidadeDelta) AS Delta
    FROM dbo.MovimentacaoEstoque WHERE CompraId = @CompraId GROUP BY CompraId, CartaId, Tipo;
END;
-- Capturas devem ocorrer sem operacoes concorrentes alheias ao caso.
-- Estes SELECTs nao formam snapshot atomico; nao usar leitura intermediaria como estado final.
-- Contagens/deltas precisam ser comparados a ItemCompra/Total e ao antes/depois.
