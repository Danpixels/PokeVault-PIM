USE PokeVaultTests;
SET NOCOUNT ON;
IF NOT EXISTS(SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1') THROW 51998,'BASE_TESTE_INVALIDA',1;
SELECT DB_NAME() AS Banco,MAX(Versao) AS SchemaMaximo,SYSUTCDATETIME() AS InstanteUtc FROM dbo.SchemaVersao FOR JSON PATH;
SELECT CartaId,Raridade FROM dbo.Carta WHERE Raridade COLLATE Latin1_General_100_BIN2 NOT IN(N'Common',N'Uncommon',N'Rare',N'Double Rare',N'Ace Spec Rare',N'Illustration Rare',N'Ultra Rare',N'Special Illustration Rare',N'Hyper Rare',N'Shiny Rare',N'Shiny Ultra Rare',N'Promo',N'Comum') ORDER BY CartaId FOR JSON PATH;
