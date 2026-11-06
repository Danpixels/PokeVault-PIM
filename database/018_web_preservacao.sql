-- Snapshot de preservacao: somente leitura, sem dados/credenciais individuais na saida.
-- Exclui tokens Versao e nova coluna QuantidadeTotalCartas, conferida separadamente em017.
-- Projeta Comum exato como Common ANTES e DEPOIS para comparar a mudanca intencional.
USE [$(DatabaseName)];
GO
SET NOCOUNT ON;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
DECLARE @tabela sysname,@objeto int,@colunas nvarchar(max),@ordem nvarchar(max),@sql nvarchar(max),@json nvarchar(max),@linhas bigint;
DECLARE @resultado TABLE(Tabela sysname,Linhas bigint,Sha256 varchar(64));
DECLARE tabelas CURSOR LOCAL FAST_FORWARD FOR SELECT name FROM sys.tables WHERE schema_id=SCHEMA_ID(N'dbo') AND name IN
 (N'Perfil',N'Usuario',N'SaldoCliente',N'Colecao',N'Carta',N'Carrinho',N'ItemCarrinho',N'Compra',N'ItemCompra',N'MovimentacaoSaldo',N'MovimentacaoEstoque',N'AlteracaoCarta') ORDER BY name;
OPEN tabelas;
FETCH NEXT FROM tabelas INTO @tabela;
WHILE @@FETCH_STATUS=0
BEGIN
    SET @objeto=OBJECT_ID(N'dbo.'+QUOTENAME(@tabela));
    SELECT @colunas=STRING_AGG(CONVERT(nvarchar(max),CASE WHEN @tabela=N'Carta' AND name=N'Raridade'
        THEN N'CASE WHEN Raridade COLLATE Latin1_General_100_BIN2=N''Comum'' AND DATALENGTH(Raridade)=DATALENGTH(N''Comum'') THEN N''Common'' ELSE Raridade END AS Raridade'
        ELSE QUOTENAME(name) END),N',') WITHIN GROUP(ORDER BY column_id)
        FROM sys.columns WHERE object_id=@objeto AND name NOT IN (N'Versao',N'QuantidadeTotalCartas');
    SELECT @ordem=STRING_AGG(CONVERT(nvarchar(max),QUOTENAME(c.name)),N',') WITHIN GROUP(ORDER BY ic.key_ordinal)
        FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
        JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
        WHERE i.object_id=@objeto AND i.is_primary_key=1 AND ic.key_ordinal>0;
    SET @sql=N'SELECT @j=(SELECT '+@colunas+N' FROM dbo.'+QUOTENAME(@tabela)+N' ORDER BY '+@ordem+N' FOR JSON PATH,INCLUDE_NULL_VALUES); SELECT @n=COUNT_BIG(*) FROM dbo.'+QUOTENAME(@tabela)+N';';
    EXEC sys.sp_executesql @sql,N'@j nvarchar(max) OUTPUT,@n bigint OUTPUT',@j=@json OUTPUT,@n=@linhas OUTPUT;
    INSERT @resultado VALUES (@tabela,@linhas,CONVERT(varchar(64),HASHBYTES('SHA2_256',CONVERT(varbinary(max),COALESCE(@json,N'[]'))),2));
    FETCH NEXT FROM tabelas INTO @tabela;
END;
CLOSE tabelas; DEALLOCATE tabelas;
SELECT Tabela,Linhas,Sha256 FROM @resultado ORDER BY Tabela;
GO
