-- Somente leitura. Nao substitui os testes transacionais/concorrentes da frente de testes.
USE [$(DatabaseName)];
GO
SET NOCOUNT ON;
IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
SELECT DB_NAME() AS Banco,@@SERVERNAME AS Servidor,
    SERVERPROPERTY('ProductVersion') AS VersaoMotor,SERVERPROPERTY('Edition') AS Edicao;
SELECT Versao,AplicadaEmUtc FROM dbo.SchemaVersao;
SELECT name FROM sys.procedures WHERE schema_id=SCHEMA_ID(N'dbo') ORDER BY name;
-- As consultas de divergencias abaixo devem retornar zero linhas numa base integra.
SELECT s.UsuarioId,s.Saldo,COALESCE(m.Total,0) AS SaldoPelosMovimentos
FROM dbo.SaldoCliente s
OUTER APPLY (SELECT SUM(ValorDelta) AS Total FROM dbo.MovimentacaoSaldo WHERE UsuarioId=s.UsuarioId) m
WHERE s.Saldo<>COALESCE(m.Total,0);
SELECT c.CartaId,c.QuantidadeEstoque,COALESCE(m.Total,0) AS EstoquePelosMovimentos
FROM dbo.Carta c
OUTER APPLY (SELECT SUM(CONVERT(bigint,QuantidadeDelta)) AS Total FROM dbo.MovimentacaoEstoque WHERE CartaId=c.CartaId) m
WHERE c.QuantidadeEstoque<>COALESCE(m.Total,0);
SELECT c.CompraId,c.Total,i.TotalItens
FROM dbo.Compra c
OUTER APPLY (SELECT SUM(CONVERT(decimal(12,2),Quantidade)*PrecoUnitario) AS TotalItens FROM dbo.ItemCompra WHERE CompraId=c.CompraId) i
WHERE i.TotalItens IS NULL OR c.Total<>i.TotalItens;
SELECT c.CompraId,c.Status,m.Tipo,m.ValorDelta
FROM dbo.Compra c
LEFT JOIN dbo.MovimentacaoSaldo m ON m.CompraId=c.CompraId AND m.Tipo=N'COMPRA'
WHERE m.CompraId IS NULL OR m.ValorDelta<>-c.Total;
SELECT c.CompraId,c.Status,m.ValorDelta
FROM dbo.Compra c
LEFT JOIN dbo.MovimentacaoSaldo m ON m.CompraId=c.CompraId AND m.Tipo=N'ESTORNO'
WHERE (c.Status=N'CANCELADO' AND (m.CompraId IS NULL OR m.ValorDelta<>c.Total))
   OR (c.Status=N'CONCLUIDO' AND m.CompraId IS NOT NULL);
SELECT i.CompraId,i.CartaId,i.Quantidade,m.QuantidadeDelta
FROM dbo.ItemCompra i
LEFT JOIN dbo.MovimentacaoEstoque m ON m.CompraId=i.CompraId AND m.CartaId=i.CartaId AND m.Tipo=N'COMPRA'
WHERE m.CompraId IS NULL OR m.QuantidadeDelta<>-i.Quantidade;
SELECT i.CompraId,i.CartaId,c.Status,m.QuantidadeDelta
FROM dbo.ItemCompra i
JOIN dbo.Compra c ON c.CompraId=i.CompraId
LEFT JOIN dbo.MovimentacaoEstoque m ON m.CompraId=i.CompraId AND m.CartaId=i.CartaId AND m.Tipo=N'ESTORNO'
WHERE (c.Status=N'CANCELADO' AND (m.CompraId IS NULL OR m.QuantidadeDelta<>i.Quantidade))
   OR (c.Status=N'CONCLUIDO' AND m.CompraId IS NOT NULL);
SELECT u.UsuarioId,p.Codigo,s.UsuarioId AS Carteira,c.CarrinhoId
FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
LEFT JOIN dbo.SaldoCliente s ON s.UsuarioId=u.UsuarioId
LEFT JOIN dbo.Carrinho c ON c.UsuarioId=u.UsuarioId
WHERE (p.Codigo=N'CLIENTE' AND (s.UsuarioId IS NULL OR c.CarrinhoId IS NULL))
   OR (p.Codigo<>N'CLIENTE' AND (s.UsuarioId IS NOT NULL OR c.CarrinhoId IS NOT NULL))
   OR (u.AdministradorFixo=1 AND p.Codigo<>N'ADMINISTRADOR');
GO
