-- Leitura de preservacao dos prefixos de PK do snapshot anterior desta rodada.
-- Os limites abaixo foram obtidos de pokevault-antes.txt, nao sao IDs de contrato.
USE [PokeVault];
GO
SET NOCOUNT ON;
IF DB_NAME()<>N'PokeVault' OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
 THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
DECLARE @originais TABLE(Tabela sysname,Limite int);
INSERT @originais VALUES (N'Carrinho',2),(N'Carta',3),(N'Colecao',1),(N'Compra',6),(N'ItemCarrinho',0),
 (N'ItemCompra',10),(N'MovimentacaoEstoque',23),(N'MovimentacaoSaldo',15),(N'Perfil',3),(N'SaldoCliente',2),(N'Usuario',4);
DECLARE @tabela sysname,@limite int,@objeto int,@colunas nvarchar(max),@ordem nvarchar(max),@sql nvarchar(max),@json nvarchar(max),@linhas bigint;
DECLARE @resultado TABLE(Tabela sysname,Linhas bigint,Sha256 varchar(64));
DECLARE tabelas CURSOR LOCAL FAST_FORWARD FOR SELECT Tabela,Limite FROM @originais ORDER BY Tabela;
OPEN tabelas;
FETCH NEXT FROM tabelas INTO @tabela,@limite;
WHILE @@FETCH_STATUS=0
BEGIN
 SET @objeto=OBJECT_ID(N'dbo.'+QUOTENAME(@tabela));
 SELECT @colunas=STRING_AGG(CONVERT(nvarchar(max),QUOTENAME(name)),N',') WITHIN GROUP(ORDER BY column_id)
  FROM sys.columns WHERE object_id=@objeto AND name<>N'Versao';
 SELECT @ordem=STRING_AGG(CONVERT(nvarchar(max),QUOTENAME(c.name)),N',') WITHIN GROUP(ORDER BY ic.key_ordinal)
  FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
  JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
  WHERE i.object_id=@objeto AND i.is_primary_key=1 AND ic.key_ordinal>0;
 SET @sql=N'SELECT @j=(SELECT TOP (@limite) '+@colunas+N' FROM dbo.'+QUOTENAME(@tabela)+N' ORDER BY '+@ordem+N' FOR JSON PATH,INCLUDE_NULL_VALUES); SELECT @n=CASE WHEN COUNT_BIG(*)<@limite THEN COUNT_BIG(*) ELSE @limite END FROM dbo.'+QUOTENAME(@tabela)+N';';
 EXEC sys.sp_executesql @sql,N'@j nvarchar(max) OUTPUT,@n bigint OUTPUT,@limite int',@j=@json OUTPUT,@n=@linhas OUTPUT,@limite=@limite;
 INSERT @resultado VALUES (@tabela,@linhas,CONVERT(varchar(64),HASHBYTES('SHA2_256',CONVERT(varbinary(max),@json)),2));
 FETCH NEXT FROM tabelas INTO @tabela,@limite;
END;
CLOSE tabelas; DEALLOCATE tabelas;
SELECT Tabela,Linhas,Sha256 FROM @resultado ORDER BY Tabela;
GO
