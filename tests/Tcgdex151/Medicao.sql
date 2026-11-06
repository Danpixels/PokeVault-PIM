USE PokeVaultTests;
SET NOCOUNT ON;
IF DB_NAME()<>N'PokeVaultTests' OR (SELECT MAX(Versao) FROM dbo.SchemaVersao)<>10 THROW 51998,'BASE_SCHEMA_INVALIDOS',1;
;WITH Nodes AS(
 SELECT CardId,CONVERT(nvarchar(max),PrecosJson) COLLATE DATABASE_DEFAULT AS NodeValue,CAST(5 AS int) AS NodeType
 FROM dbo.vw_BibliotecaTCGdex151 WHERE PrecosJson IS NOT NULL
 UNION ALL
 SELECT p.CardId,CONVERT(nvarchar(max),j.value) COLLATE DATABASE_DEFAULT,CONVERT(int,j.type)
 FROM Nodes p CROSS APPLY OPENJSON(CASE WHEN p.NodeType IN(4,5) THEN p.NodeValue ELSE N'{}' END) j
 WHERE p.NodeType IN(4,5) AND j.[key] NOT IN(N'unit',N'updated',N'productId',N'idProduct') AND RIGHT(j.[key],2) COLLATE Latin1_General_100_BIN2<>N'Id'
)
SELECT
 (SELECT COUNT(*) FROM dbo.vw_BibliotecaTCGdex151) AS ReferenciasLogicas,
 (SELECT COUNT(*) FROM dbo.vw_BibliotecaTCGdex151 WHERE Detalhada=1) AS Detalhadas,
 (SELECT COUNT(*) FROM dbo.ReferenciaTCGdex) AS TraducoesFisicas,
 (SELECT COUNT(*) FROM dbo.vw_BibliotecaTCGdex151 WHERE PrecosJson IS NULL) AS SemPricing,
 COUNT(DISTINCT CASE WHEN NodeType=2 THEN CardId END) AS ReferenciasComCampoNumerico,
 SUM(CASE WHEN NodeType=2 THEN 1 ELSE 0 END) AS CamposNumericos,
 SUM(CASE WHEN NodeType=0 THEN 1 ELSE 0 END) AS CamposNulos,
 SUM(CASE WHEN NodeType=2 AND TRY_CONVERT(decimal(28,8),NodeValue)=0 THEN 1 ELSE 0 END) AS ZerosRecebidosDaFonte,
 (SELECT COUNT(*) FROM dbo.vw_BibliotecaTCGdex151 WHERE PrecosJson IS NOT NULL AND (PrecosColetadosEmUtc IS NULL OR OrigemPrecosUrl IS NULL)) AS PrecosSemOrigemData,
 (SELECT COUNT(*) FROM dbo.vw_BibliotecaTCGdex151 WHERE SetId<>N'sv03.5') AS ForaDoAlvo
FROM Nodes FOR JSON PATH,WITHOUT_ARRAY_WRAPPER OPTION(MAXRECURSION 32);
SELECT COUNT(*) AS TriggersTesteRestantes FROM sys.triggers WHERE name LIKE N'tr_test_%' FOR JSON PATH;

