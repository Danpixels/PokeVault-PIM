USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN (N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
 THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=4) OR EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao BETWEEN 5 AND 7)
 THROW 51102,'SCHEMA_INESPERADO',1;
BEGIN TRANSACTION;
DECLARE @lockResult int;
EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Schema.TCGdex151',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
IF @lockResult<0 THROW 51104,'ATUALIZACAO_OCUPADA',1;
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=8)
BEGIN
 IF EXISTS (SELECT 1 FROM sys.tables WHERE schema_id=SCHEMA_ID(N'dbo') AND name IN
  (N'ColecaoReferenciaTCGdex',N'ReferenciaTCGdex',N'ImportacaoTCGdex151',N'ItemImportacaoTCGdex151',N'VinculoCartaTCGdex'))
  THROW 51102,'SCHEMA_INESPERADO',1;
 CREATE TABLE dbo.ColecaoReferenciaTCGdex (
  Idioma nvarchar(2) NOT NULL,
  SetId nvarchar(20) NOT NULL,
  Nome nvarchar(120) NOT NULL,
  QuantidadeOficial int NOT NULL,
  QuantidadeTotal int NOT NULL,
  SnapshotJson nvarchar(max) NOT NULL,
  ColetadoEmUtc datetime2(3) NOT NULL,
  OrigemUrl nvarchar(300) NOT NULL,
  CONSTRAINT PK_ColecaoRefTCG PRIMARY KEY(Idioma,SetId),
  CONSTRAINT CK_ColecaoRefTCG_Escopo CHECK (Idioma COLLATE Latin1_General_100_BIN2 IN (N'pt',N'en') AND SetId COLLATE Latin1_General_100_BIN2=N'sv03.5' AND QuantidadeOficial=165 AND QuantidadeTotal=207),
  CONSTRAINT CK_ColecaoRefTCG_Json CHECK (ISJSON(SnapshotJson)=1)
 );
 CREATE TABLE dbo.ImportacaoTCGdex151 (
  ImportacaoId int IDENTITY PRIMARY KEY,
  ChaveImportacao uniqueidentifier NOT NULL UNIQUE,
  Idioma nvarchar(2) NOT NULL,
  SetId nvarchar(20) NOT NULL CONSTRAINT DF_ImportacaoTCG_Set DEFAULT N'sv03.5',
  SetHash binary(32) NOT NULL,
  SetSnapshotJson nvarchar(max) NOT NULL,
  ColetadoEmUtc datetime2(3) NOT NULL,
  ResponsavelId int NOT NULL REFERENCES dbo.Usuario(UsuarioId),
  UltimoResponsavelId int NOT NULL REFERENCES dbo.Usuario(UsuarioId),
  Status nvarchar(20) NOT NULL,
  Falhas int NOT NULL CONSTRAINT DF_ImportacaoTCG_Falhas DEFAULT 0,
  UltimoCodigoFalha nvarchar(80) NULL,
  UltimaMensagemFalha nvarchar(300) NULL,
  CriadaEmUtc datetime2(3) NOT NULL CONSTRAINT DF_ImportacaoTCG_Criada DEFAULT SYSUTCDATETIME(),
  AtualizadaEmUtc datetime2(3) NOT NULL CONSTRAINT DF_ImportacaoTCG_Atualizada DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_ImportacaoTCG_Colecao FOREIGN KEY(Idioma,SetId) REFERENCES dbo.ColecaoReferenciaTCGdex(Idioma,SetId),
  CONSTRAINT CK_ImportacaoTCG_Status CHECK(Status IN(N'EM_ANDAMENTO',N'PARCIAL',N'CONCLUIDA',N'FALHA') AND Falhas>=0),
  CONSTRAINT CK_ImportacaoTCG_Json CHECK(ISJSON(SetSnapshotJson)=1)
 );
 CREATE TABLE dbo.ReferenciaTCGdex (
  ReferenciaTCGdexId int IDENTITY PRIMARY KEY,
  Idioma nvarchar(2) NOT NULL,
  SetId nvarchar(20) NOT NULL,
  CardId nvarchar(30) NOT NULL,
  Numero nvarchar(3) NOT NULL,
  Nome nvarchar(120) NOT NULL,
  Raridade nvarchar(100) NULL,
  ImagemUrl nvarchar(500) NULL,
  Detalhada bit NOT NULL CONSTRAINT DF_ReferenciaTCG_Detalhada DEFAULT 0,
  SnapshotJson nvarchar(max) NOT NULL,
  SnapshotColetadoEmUtc datetime2(3) NOT NULL,
  OrigemUrl nvarchar(300) NOT NULL,
  PrecosJson nvarchar(max) NULL,
  PrecosColetadosEmUtc datetime2(3) NULL,
  OrigemPrecosUrl nvarchar(300) NULL,
  UltimaImportacaoId int NOT NULL REFERENCES dbo.ImportacaoTCGdex151(ImportacaoId),
  CONSTRAINT UQ_ReferenciaTCG_IdiomaCard UNIQUE(Idioma,CardId),
  CONSTRAINT FK_ReferenciaTCG_Colecao FOREIGN KEY(Idioma,SetId) REFERENCES dbo.ColecaoReferenciaTCGdex(Idioma,SetId),
  CONSTRAINT CK_ReferenciaTCG_Numero CHECK(TRY_CONVERT(int,Numero) IS NOT NULL AND TRY_CONVERT(int,Numero) BETWEEN 1 AND 207
   AND DATALENGTH(Numero)=6 AND Numero COLLATE Latin1_General_100_BIN2=RIGHT(N'000'+CONVERT(nvarchar(3),TRY_CONVERT(int,Numero)),3)
   AND CardId COLLATE Latin1_General_100_BIN2=N'sv03.5-'+Numero),
  CONSTRAINT CK_ReferenciaTCG_Json CHECK(ISJSON(SnapshotJson)=1 AND (PrecosJson IS NULL OR ISJSON(PrecosJson)=1)),
  CONSTRAINT CK_ReferenciaTCG_PrecoOrigem CHECK(
   (PrecosJson IS NULL AND PrecosColetadosEmUtc IS NULL AND OrigemPrecosUrl IS NULL)
   OR (PrecosJson IS NOT NULL AND PrecosColetadosEmUtc IS NOT NULL AND OrigemPrecosUrl IS NOT NULL)),
  CONSTRAINT CK_ReferenciaTCG_Nome CHECK(LEN(TRIM(Nome))>0)
 );
 CREATE TABLE dbo.ItemImportacaoTCGdex151 (
  ImportacaoId int NOT NULL REFERENCES dbo.ImportacaoTCGdex151(ImportacaoId),
  ReferenciaTCGdexId int NOT NULL REFERENCES dbo.ReferenciaTCGdex(ReferenciaTCGdexId),
  Estado nvarchar(20) NOT NULL,
  AtualizadoEmUtc datetime2(3) NOT NULL CONSTRAINT DF_ItemImportacaoTCG_Data DEFAULT SYSUTCDATETIME(),
  CONSTRAINT PK_ItemImportacaoTCG PRIMARY KEY(ImportacaoId,ReferenciaTCGdexId),
  CONSTRAINT CK_ItemImportacaoTCG_Estado CHECK(Estado IN(N'PENDENTE',N'APLICADO',N'MAIS_RECENTE'))
 );
 CREATE TABLE dbo.VinculoCartaTCGdex (
  CartaId int PRIMARY KEY REFERENCES dbo.Carta(CartaId),
  ReferenciaTCGdexId int NOT NULL REFERENCES dbo.ReferenciaTCGdex(ReferenciaTCGdexId),
  ResponsavelId int NOT NULL REFERENCES dbo.Usuario(UsuarioId),
  NomeConferido nvarchar(120) NOT NULL,
  NumeroConferido nvarchar(20) NOT NULL,
  ConferidoEmUtc datetime2(3) NOT NULL CONSTRAINT DF_VinculoTCG_Data DEFAULT SYSUTCDATETIME()
 );
 CREATE INDEX IX_ItemImportacaoTCG_Pendentes ON dbo.ItemImportacaoTCGdex151(ImportacaoId,Estado);
 CREATE INDEX IX_ReferenciaTCG_Numero ON dbo.ReferenciaTCGdex(Numero,Idioma);
END;
ELSE IF (SELECT COUNT(*) FROM sys.tables WHERE schema_id=SCHEMA_ID(N'dbo') AND name IN
 (N'ColecaoReferenciaTCGdex',N'ReferenciaTCGdex',N'ImportacaoTCGdex151',N'ItemImportacaoTCGdex151',N'VinculoCartaTCGdex'))<>5
 THROW 51102,'SCHEMA_INESPERADO',1;
GO
CREATE OR ALTER VIEW dbo.vw_BibliotecaTCGdex151
AS
SELECT ReferenciaTCGdexId,Idioma,SetId,CardId,Numero,Nome,Raridade,ImagemUrl,Detalhada,SnapshotJson,
 SnapshotColetadoEmUtc,OrigemUrl,PrecosJson,PrecosColetadosEmUtc,OrigemPrecosUrl,UltimaImportacaoId
FROM (
 SELECT r.*,ROW_NUMBER() OVER(PARTITION BY CardId ORDER BY Detalhada DESC,
  CASE WHEN Idioma=N'pt' THEN 0 ELSE 1 END,SnapshotColetadoEmUtc DESC,ReferenciaTCGdexId) AS Preferencia
 FROM dbo.ReferenciaTCGdex r
) fonte WHERE Preferencia=1;
GO
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=8) INSERT dbo.SchemaVersao(Versao) VALUES(8);
COMMIT;
GO
