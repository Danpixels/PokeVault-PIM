-- Executar com sqlcmd -b -v DatabaseName="PokeVault" (ou PokeVaultTests).
-- Nao adota bases existentes sem a marca do projeto e nunca apaga dados.
USE [master];
GO
SET NOCOUNT ON;
DECLARE @nome sysname = N'$(DatabaseName)', @sql nvarchar(max), @nova bit = 0;
IF @nome NOT IN (N'PokeVault', N'PokeVaultTests')
    THROW 51100, 'BASE_NAO_AUTORIZADA', 1;
IF DB_ID(@nome) IS NULL
BEGIN
    SET @sql = N'CREATE DATABASE ' + QUOTENAME(@nome) + N';';
    EXEC sys.sp_executesql @sql;
    SET @nova = 1;
END;
SET @sql = N'USE ' + QUOTENAME(@nome) + N';
IF @nova = 1
    EXEC sys.sp_addextendedproperty @name=N''PokeVaultProjeto'', @value=N''PIM-2026-Sprint1'';
IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N''PokeVaultProjeto'' AND CONVERT(nvarchar(100),value)=N''PIM-2026-Sprint1'')
    THROW 51101, ''BASE_EXISTENTE_NAO_IDENTIFICADA'', 1;
SELECT DB_NAME() AS Banco, SERVERPROPERTY(''ProductVersion'') AS VersaoMotor;';
EXEC sys.sp_executesql @sql, N'@nova bit', @nova;
GO
