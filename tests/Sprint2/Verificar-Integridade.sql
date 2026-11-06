USE PokeVaultTests;
SET NOCOUNT ON;
IF DB_NAME()<>N'PokeVaultTests' OR NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51998,'BASE_TESTE_INVALIDA',1;
SELECT
 DB_NAME() AS Banco,CONVERT(nvarchar(100),@@SERVERNAME) AS Servidor,
 (SELECT COUNT(*) FROM dbo.SaldoCliente s OUTER APPLY(SELECT SUM(ValorDelta) AS Total FROM dbo.MovimentacaoSaldo m WHERE m.UsuarioId=s.UsuarioId)m WHERE s.Saldo<>COALESCE(m.Total,0)) AS SaldosDivergentes,
 (SELECT COUNT(*) FROM dbo.Carta c OUTER APPLY(SELECT SUM(CONVERT(bigint,QuantidadeDelta)) AS Total FROM dbo.MovimentacaoEstoque m WHERE m.CartaId=c.CartaId)m WHERE c.QuantidadeEstoque<>COALESCE(m.Total,0)) AS EstoquesDivergentes,
 (SELECT COUNT(*) FROM dbo.Compra c OUTER APPLY(SELECT SUM(Quantidade*PrecoUnitario) AS Total FROM dbo.ItemCompra i WHERE i.CompraId=c.CompraId)i WHERE i.Total IS NULL OR c.Total<>i.Total) AS TotaisDivergentes,
 (SELECT COUNT(*) FROM sys.triggers WHERE name LIKE N'tr_test_%') AS TriggersTesteRestantes,
 (SELECT COUNT(*) FROM dbo.SchemaVersao WHERE Versao IN(1,2,3,4)) AS VersoesInstaladas,
 (SELECT COUNT(*) FROM dbo.Compra) AS ComprasPreservadas,
 (SELECT COUNT(*) FROM dbo.Usuario) AS UsuariosTeste,
 (SELECT COUNT(*) FROM dbo.Carta) AS CartasTeste
FOR JSON PATH,WITHOUT_ARRAY_WRAPPER;
