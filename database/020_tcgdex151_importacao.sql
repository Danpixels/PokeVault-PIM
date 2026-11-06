USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON; SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON; SET ARITHABORT ON; SET CONCAT_NULL_YIELDS_NULL ON; SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT IN(N'PokeVault',N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
 THROW 51101,'BASE_EXISTENTE_NAO_IDENTIFICADA',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersao WHERE Versao=8) THROW 51102,'SCHEMA_INESPERADO',1;
BEGIN TRANSACTION;
DECLARE @lockResult int;
EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Schema.TCGdex151',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
IF @lockResult<0 THROW 51104,'ATUALIZACAO_OCUPADA',1;
GO
CREATE OR ALTER PROCEDURE dbo.usp_IniciarImportacaoTCGdex151
 @ResponsavelId int,@ChaveImportacao uniqueidentifier,@Idioma nvarchar(2),@SetJson nvarchar(max),@ColetadoEmUtc datetime2(3)
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
 IF @ChaveImportacao IS NULL OR @Idioma IS NULL OR @Idioma COLLATE Latin1_General_100_BIN2 NOT IN(N'pt',N'en')
  OR @SetJson IS NULL OR ISJSON(@SetJson)<>1 OR DATALENGTH(@SetJson)>2097152 OR @ColetadoEmUtc IS NULL
  OR @ColetadoEmUtc<'20000101' OR @ColetadoEmUtc>DATEADD(minute,5,SYSUTCDATETIME())
  THROW 51020,'TCGDEX_DADOS_INVALIDOS',1;
 DECLARE @nome nvarchar(4000)=JSON_VALUE(@SetJson,'$.name');
 IF JSON_VALUE(@SetJson,'$.id') IS NULL OR JSON_VALUE(@SetJson,'$.id') COLLATE Latin1_General_100_BIN2<>N'sv03.5'
  OR COALESCE(TRY_CONVERT(int,JSON_VALUE(@SetJson,'$.cardCount.official')),-1)<>165
  OR COALESCE(TRY_CONVERT(int,JSON_VALUE(@SetJson,'$.cardCount.total')),-1)<>207
  OR @nome IS NULL OR LEN(TRIM(@nome))=0 OR LEN(@nome)>120
  OR COALESCE(LEFT(LTRIM(JSON_QUERY(@SetJson,'$.cards')),1),N'')<>N'['
  THROW 51020,'TCGDEX_DADOS_INVALIDOS',1;
 DECLARE @itens TABLE(CardId nvarchar(4000),Numero nvarchar(4000),Nome nvarchar(4000),Imagem nvarchar(4000),SnapshotJson nvarchar(max),Tipo int);
 INSERT @itens SELECT JSON_VALUE(value,'$.id'),JSON_VALUE(value,'$.localId'),JSON_VALUE(value,'$.name'),JSON_VALUE(value,'$.image'),value,type
  FROM OPENJSON(@SetJson,'$.cards');
 IF (SELECT COUNT(*) FROM @itens)<>207 OR EXISTS(SELECT CardId FROM @itens GROUP BY CardId HAVING COUNT(*)>1)
  OR EXISTS(SELECT 1 FROM @itens WHERE Tipo<>5 OR CardId IS NULL OR Numero IS NULL OR Nome IS NULL OR LEN(TRIM(Nome))=0 OR LEN(Nome)>120
   OR TRY_CONVERT(int,Numero) IS NULL OR TRY_CONVERT(int,Numero) NOT BETWEEN 1 AND 207
   OR CardId COLLATE Latin1_General_100_BIN2<>N'sv03.5-'+RIGHT(N'000'+CONVERT(nvarchar(3),TRY_CONVERT(int,Numero)),3)
   OR DATALENGTH(CardId)<>20 OR (Imagem IS NOT NULL AND (LEN(Imagem)>500 OR Imagem COLLATE Latin1_General_100_BIN2 NOT LIKE N'https://assets.tcgdex.net/%')))
  THROW 51020,'TCGDEX_DADOS_INVALIDOS',1;
 DECLARE @id int,@status nvarchar(20),@pendentes int,@hash binary(32)=HASHBYTES('SHA2_256',CONVERT(varbinary(max),@SetJson)),
  @hashAnterior binary(32),@idiomaAnterior nvarchar(2),@origem nvarchar(300)=N'https://api.tcgdex.net/v2/'+@Idioma+N'/sets/sv03.5',@lockResult int;
 BEGIN TRY
  BEGIN TRANSACTION;
  IF NOT EXISTS(SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId WHERE u.UsuarioId=@ResponsavelId AND u.Ativo=1 AND p.Codigo IN(N'ADMINISTRADOR',N'ATENDENTE'))
   THROW 51000,'ACESSO_NEGADO',1;
  EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.TCGdex151.Importacao',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
  IF @lockResult<0 THROW 51104,'IMPORTACAO_OCUPADA',1;
  SELECT @id=ImportacaoId,@status=Status,@hashAnterior=SetHash,@idiomaAnterior=Idioma FROM dbo.ImportacaoTCGdex151 WHERE ChaveImportacao=@ChaveImportacao;
  IF @id IS NOT NULL
  BEGIN
   IF @hashAnterior<>@hash OR @idiomaAnterior COLLATE Latin1_General_100_BIN2<>@Idioma COLLATE Latin1_General_100_BIN2 THROW 51023,'TCGDEX_CHAVE_REUTILIZADA',1;
   SELECT @pendentes=COUNT(*) FROM dbo.ItemImportacaoTCGdex151 WHERE ImportacaoId=@id AND Estado=N'PENDENTE';
   COMMIT;
   SELECT @id AS ImportacaoId,@status AS Status,207 AS TotalReferencias,@pendentes AS Pendentes,CAST(1 AS bit) AS Repetida;
   RETURN;
  END;
  IF NOT EXISTS(SELECT 1 FROM dbo.ColecaoReferenciaTCGdex WHERE Idioma=@Idioma AND SetId=N'sv03.5')
   INSERT dbo.ColecaoReferenciaTCGdex(Idioma,SetId,Nome,QuantidadeOficial,QuantidadeTotal,SnapshotJson,ColetadoEmUtc,OrigemUrl)
    VALUES(@Idioma,N'sv03.5',@nome,165,207,@SetJson,@ColetadoEmUtc,@origem);
  ELSE
   UPDATE dbo.ColecaoReferenciaTCGdex SET Nome=@nome,SnapshotJson=@SetJson,ColetadoEmUtc=@ColetadoEmUtc,OrigemUrl=@origem
    WHERE Idioma=@Idioma AND SetId=N'sv03.5' AND ColetadoEmUtc<=@ColetadoEmUtc;
  INSERT dbo.ImportacaoTCGdex151(ChaveImportacao,Idioma,SetHash,SetSnapshotJson,ColetadoEmUtc,ResponsavelId,UltimoResponsavelId,Status)
   VALUES(@ChaveImportacao,@Idioma,@hash,@SetJson,@ColetadoEmUtc,@ResponsavelId,@ResponsavelId,N'EM_ANDAMENTO');
  SET @id=CONVERT(int,SCOPE_IDENTITY());
  INSERT dbo.ReferenciaTCGdex(Idioma,SetId,CardId,Numero,Nome,ImagemUrl,Detalhada,SnapshotJson,SnapshotColetadoEmUtc,OrigemUrl,UltimaImportacaoId)
   SELECT @Idioma,N'sv03.5',i.CardId,RIGHT(N'000'+CONVERT(nvarchar(3),TRY_CONVERT(int,i.Numero)),3),i.Nome,i.Imagem,0,i.SnapshotJson,@ColetadoEmUtc,@origem,@id
   FROM @itens i WHERE NOT EXISTS(SELECT 1 FROM dbo.ReferenciaTCGdex r WHERE r.Idioma=@Idioma AND r.CardId=i.CardId);
  INSERT dbo.ItemImportacaoTCGdex151(ImportacaoId,ReferenciaTCGdexId,Estado)
   SELECT @id,r.ReferenciaTCGdexId,N'PENDENTE' FROM dbo.ReferenciaTCGdex r JOIN @itens i ON i.CardId=r.CardId WHERE r.Idioma=@Idioma;
  COMMIT;
  SELECT @id AS ImportacaoId,CAST(N'EM_ANDAMENTO' AS nvarchar(20)) AS Status,207 AS TotalReferencias,207 AS Pendentes,CAST(0 AS bit) AS Repetida;
 END TRY
 BEGIN CATCH
  IF XACT_STATE()<>0 ROLLBACK;
  THROW;
 END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_AtualizarReferenciasTCGdex151
 @ResponsavelId int,@ImportacaoId int,@LoteJson nvarchar(max)
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
 IF @LoteJson IS NULL OR ISJSON(@LoteJson)<>1 OR DATALENGTH(@LoteJson)>8388608 THROW 51020,'TCGDEX_DADOS_INVALIDOS',1;
 IF COALESCE(TRY_CONVERT(int,JSON_VALUE(@LoteJson,'$.versao')),-1)<>1 OR COALESCE(LEFT(LTRIM(JSON_QUERY(@LoteJson,'$.cartas')),1),N'')<>N'['
  THROW 51020,'TCGDEX_DADOS_INVALIDOS',1;
 DECLARE @itens TABLE(CardId nvarchar(4000),Numero nvarchar(4000),Nome nvarchar(4000),Imagem nvarchar(4000),Raridade nvarchar(4000),SetId nvarchar(4000),
  DataTexto nvarchar(4000),ColetadoEmUtc datetime2(3),SnapshotJson nvarchar(max),PrecosJson nvarchar(max),Tipo int);
 INSERT @itens
  SELECT JSON_VALUE(value,'$.snapshot.id'),JSON_VALUE(value,'$.snapshot.localId'),JSON_VALUE(value,'$.snapshot.name'),JSON_VALUE(value,'$.snapshot.image'),
   JSON_VALUE(value,'$.snapshot.rarity'),JSON_VALUE(value,'$.snapshot.set.id'),JSON_VALUE(value,'$.coletadoEmUtc'),
   TRY_CONVERT(datetime2(3),JSON_VALUE(value,'$.coletadoEmUtc')),JSON_QUERY(value,'$.snapshot'),JSON_QUERY(value,'$.snapshot.pricing'),type
  FROM OPENJSON(@LoteJson,'$.cartas');
 IF (SELECT COUNT(*) FROM @itens) NOT BETWEEN 1 AND 30 OR EXISTS(SELECT CardId FROM @itens GROUP BY CardId HAVING COUNT(*)>1)
  OR EXISTS(SELECT 1 FROM @itens WHERE Tipo<>5 OR CardId IS NULL OR Numero IS NULL OR Nome IS NULL OR LEN(TRIM(Nome))=0 OR LEN(Nome)>120
   OR TRY_CONVERT(int,Numero) IS NULL OR TRY_CONVERT(int,Numero) NOT BETWEEN 1 AND 207
   OR CardId COLLATE Latin1_General_100_BIN2<>N'sv03.5-'+RIGHT(N'000'+CONVERT(nvarchar(3),TRY_CONVERT(int,Numero)),3) OR DATALENGTH(CardId)<>20
   OR SetId IS NULL OR SetId COLLATE Latin1_General_100_BIN2<>N'sv03.5'
   OR ColetadoEmUtc IS NULL OR ColetadoEmUtc<'20000101' OR ColetadoEmUtc>DATEADD(minute,5,SYSUTCDATETIME()) OR RIGHT(DataTexto,1) COLLATE Latin1_General_100_BIN2<>N'Z'
   OR (Imagem IS NOT NULL AND (LEN(Imagem)>500 OR Imagem COLLATE Latin1_General_100_BIN2 NOT LIKE N'https://assets.tcgdex.net/%'))
   OR LEN(Raridade)>100 OR COALESCE(LEFT(LTRIM(SnapshotJson),1),N'')<>N'{')
  OR EXISTS(SELECT 1 FROM @itens i CROSS APPLY OPENJSON(i.SnapshotJson) p WHERE p.[key]=N'pricing' AND p.type NOT IN(0,5))
  THROW 51020,'TCGDEX_DADOS_INVALIDOS',1;
 UPDATE @itens SET PrecosJson=NULL WHERE PrecosJson IS NOT NULL AND NOT EXISTS(SELECT 1 FROM OPENJSON(PrecosJson));
 DECLARE @idioma nvarchar(2),@status nvarchar(20),@pendentes int,@aplicadas int=0,@ignoradas int=0,@lockResult int;
 BEGIN TRY
  BEGIN TRANSACTION;
  IF NOT EXISTS(SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId WHERE u.UsuarioId=@ResponsavelId AND u.Ativo=1 AND p.Codigo IN(N'ADMINISTRADOR',N'ATENDENTE'))
   THROW 51000,'ACESSO_NEGADO',1;
  EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.TCGdex151.Importacao',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
  IF @lockResult<0 THROW 51104,'IMPORTACAO_OCUPADA',1;
  SELECT @idioma=Idioma FROM dbo.ImportacaoTCGdex151 WITH(UPDLOCK,HOLDLOCK) WHERE ImportacaoId=@ImportacaoId;
  IF @idioma IS NULL THROW 51021,'TCGDEX_IMPORTACAO_NAO_ENCONTRADA',1;
  IF EXISTS(SELECT 1 FROM @itens i WHERE NOT EXISTS(SELECT 1 FROM dbo.ReferenciaTCGdex r JOIN dbo.ItemImportacaoTCGdex151 it ON it.ReferenciaTCGdexId=r.ReferenciaTCGdexId
    WHERE r.CardId=i.CardId AND r.Idioma=@idioma AND it.ImportacaoId=@ImportacaoId AND r.Numero=RIGHT(N'000'+CONVERT(nvarchar(3),TRY_CONVERT(int,i.Numero)),3)))
   THROW 51022,'TCGDEX_IDENTIDADE_DIVERGENTE',1;
  DECLARE @aplicacao TABLE(ReferenciaTCGdexId int PRIMARY KEY,Estado nvarchar(20));
  INSERT @aplicacao SELECT r.ReferenciaTCGdexId,CASE WHEN r.Detalhada=1 AND r.SnapshotColetadoEmUtc>i.ColetadoEmUtc THEN N'MAIS_RECENTE' ELSE N'APLICADO' END
   FROM @itens i JOIN dbo.ReferenciaTCGdex r WITH(UPDLOCK,HOLDLOCK) ON r.CardId=i.CardId AND r.Idioma=@idioma;
  UPDATE r SET Nome=i.Nome,Raridade=COALESCE(NULLIF(TRIM(i.Raridade),N''),r.Raridade),ImagemUrl=COALESCE(i.Imagem,r.ImagemUrl),Detalhada=1,
   SnapshotJson=i.SnapshotJson,SnapshotColetadoEmUtc=i.ColetadoEmUtc,OrigemUrl=N'https://api.tcgdex.net/v2/'+@idioma+N'/cards/'+r.CardId,
   PrecosJson=CASE WHEN i.PrecosJson IS NOT NULL AND (r.PrecosColetadosEmUtc IS NULL OR r.PrecosColetadosEmUtc<=i.ColetadoEmUtc) THEN i.PrecosJson ELSE r.PrecosJson END,
   PrecosColetadosEmUtc=CASE WHEN i.PrecosJson IS NOT NULL AND (r.PrecosColetadosEmUtc IS NULL OR r.PrecosColetadosEmUtc<=i.ColetadoEmUtc) THEN i.ColetadoEmUtc ELSE r.PrecosColetadosEmUtc END,
   OrigemPrecosUrl=CASE WHEN i.PrecosJson IS NOT NULL AND (r.PrecosColetadosEmUtc IS NULL OR r.PrecosColetadosEmUtc<=i.ColetadoEmUtc) THEN N'https://api.tcgdex.net/v2/'+@idioma+N'/cards/'+r.CardId ELSE r.OrigemPrecosUrl END,
   UltimaImportacaoId=@ImportacaoId
   FROM dbo.ReferenciaTCGdex r JOIN @itens i ON i.CardId=r.CardId AND r.Idioma=@idioma JOIN @aplicacao a ON a.ReferenciaTCGdexId=r.ReferenciaTCGdexId WHERE a.Estado=N'APLICADO';
  UPDATE it SET Estado=a.Estado,AtualizadoEmUtc=SYSUTCDATETIME() FROM dbo.ItemImportacaoTCGdex151 it JOIN @aplicacao a ON a.ReferenciaTCGdexId=it.ReferenciaTCGdexId WHERE it.ImportacaoId=@ImportacaoId;
  SELECT @aplicadas=COUNT(*) FROM @aplicacao WHERE Estado=N'APLICADO';
  SELECT @ignoradas=COUNT(*) FROM @aplicacao WHERE Estado=N'MAIS_RECENTE';
  SELECT @pendentes=COUNT(*) FROM dbo.ItemImportacaoTCGdex151 WHERE ImportacaoId=@ImportacaoId AND Estado=N'PENDENTE';
  SET @status=CASE WHEN @pendentes=0 THEN N'CONCLUIDA' ELSE N'PARCIAL' END;
  UPDATE dbo.ImportacaoTCGdex151 SET Status=@status,UltimoResponsavelId=@ResponsavelId,AtualizadaEmUtc=SYSUTCDATETIME() WHERE ImportacaoId=@ImportacaoId;
  COMMIT;
  SELECT @ImportacaoId AS ImportacaoId,@status AS Status,(SELECT COUNT(*) FROM @itens) AS Recebidas,@aplicadas AS Aplicadas,@ignoradas AS IgnoradasMaisRecentes,@pendentes AS Pendentes;
 END TRY
 BEGIN CATCH
  IF XACT_STATE()<>0 ROLLBACK;
  THROW;
 END CATCH;
END;
GO
CREATE OR ALTER PROCEDURE dbo.usp_RegistrarFalhaTCGdex151
 @ResponsavelId int,@ImportacaoId int,@Codigo nvarchar(80),@Mensagem nvarchar(300)
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF @@TRANCOUNT<>0 THROW 51103,'TRANSACAO_EXTERNA_NAO_SUPORTADA',1;
 IF @Codigo IS NULL OR LEN(TRIM(@Codigo))=0 OR @Mensagem IS NULL OR LEN(TRIM(@Mensagem))=0 THROW 51020,'TCGDEX_DADOS_INVALIDOS',1;
 DECLARE @id int,@pendentes int,@status nvarchar(20),@lockResult int;
 BEGIN TRY
  BEGIN TRANSACTION;
  IF NOT EXISTS(SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId WHERE u.UsuarioId=@ResponsavelId AND u.Ativo=1 AND p.Codigo IN(N'ADMINISTRADOR',N'ATENDENTE')) THROW 51000,'ACESSO_NEGADO',1;
  EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.TCGdex151.Importacao',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
  IF @lockResult<0 THROW 51104,'IMPORTACAO_OCUPADA',1;
  SELECT @id=ImportacaoId FROM dbo.ImportacaoTCGdex151 WITH(UPDLOCK,HOLDLOCK) WHERE ImportacaoId=@ImportacaoId;
  IF @id IS NULL THROW 51021,'TCGDEX_IMPORTACAO_NAO_ENCONTRADA',1;
  SELECT @pendentes=COUNT(*) FROM dbo.ItemImportacaoTCGdex151 WHERE ImportacaoId=@id AND Estado=N'PENDENTE';
  SET @status=CASE WHEN @pendentes=0 THEN N'CONCLUIDA' WHEN @pendentes=207 THEN N'FALHA' ELSE N'PARCIAL' END;
  UPDATE dbo.ImportacaoTCGdex151 SET Falhas=Falhas+1,UltimoCodigoFalha=TRIM(@Codigo),UltimaMensagemFalha=TRIM(@Mensagem),Status=@status,
   UltimoResponsavelId=@ResponsavelId,AtualizadaEmUtc=SYSUTCDATETIME() WHERE ImportacaoId=@id;
  COMMIT;
  SELECT @id AS ImportacaoId,@status AS Status,@pendentes AS Pendentes;
 END TRY
 BEGIN CATCH
  IF XACT_STATE()<>0 ROLLBACK;
  THROW;
 END CATCH;
END;
GO
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersao WHERE Versao=9) INSERT dbo.SchemaVersao(Versao) VALUES(9);
COMMIT;
GO
