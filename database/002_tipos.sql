USE [$(DatabaseName)];
GO
IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
IF TYPE_ID(N'dbo.ItemCompraSelecionadoType') IS NULL
    EXEC(N'CREATE TYPE dbo.ItemCompraSelecionadoType AS TABLE (
        CartaId int NOT NULL PRIMARY KEY,
        Quantidade int NOT NULL,
        PrecoConfirmado decimal(12,2) NOT NULL
    );');
GO
