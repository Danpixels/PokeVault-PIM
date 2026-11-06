USE [$(DatabaseName)];
GO
IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
IF DATABASE_PRINCIPAL_ID(N'PokeVaultRuntime') IS NULL
    CREATE ROLE PokeVaultRuntime AUTHORIZATION dbo;
GRANT SELECT ON SCHEMA::dbo TO PokeVaultRuntime;
GRANT EXECUTE ON SCHEMA::dbo TO PokeVaultRuntime;
GRANT REFERENCES ON TYPE::dbo.ItemCompraSelecionadoType TO PokeVaultRuntime;
DENY INSERT, UPDATE, DELETE ON SCHEMA::dbo TO PokeVaultRuntime;
-- Nao cria logins/usuarios, nao associa contas existentes automaticamente.
-- Administrador local mapeia o login do processo a um usuario nesta base e adiciona ao papel.
-- Bootstrap de hash/admin usa credencial de instalacao, fora desse papel.
GO
