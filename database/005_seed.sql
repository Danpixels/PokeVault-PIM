USE [$(DatabaseName)];
GO
SET NOCOUNT ON; SET XACT_ABORT ON;
IF NOT EXISTS (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
-- Reexecucao somente completa registros ausentes; nao reseta precos, estoque ou historico.
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @lockResult int;
    EXEC @lockResult=sys.sp_getapplock @Resource=N'PokeVault.Seed.Sprint1',@LockMode=N'Exclusive',@LockOwner=N'Transaction',@LockTimeout=10000;
    IF @lockResult<0 THROW 51104, 'SEED_OCUPADO', 1;
    INSERT dbo.Perfil(Codigo)
        SELECT v.Codigo FROM (VALUES (N'ADMINISTRADOR'),(N'ATENDENTE'),(N'CLIENTE')) v(Codigo)
        WHERE NOT EXISTS (SELECT 1 FROM dbo.Perfil p WHERE p.Codigo=v.Codigo);
    IF NOT EXISTS (SELECT 1 FROM dbo.Colecao WHERE Codigo=N'MEW-151')
        INSERT dbo.Colecao(Codigo,Nome) VALUES (N'MEW-151',N'Pokémon 151 — demonstração');
    DECLARE @colecaoId int;
    SELECT @colecaoId=ColecaoId FROM dbo.Colecao WHERE Codigo=N'MEW-151';
    DECLARE @novas TABLE (CartaId int,Quantidade int);
    INSERT dbo.Carta(ColecaoId,Numero,Nome,Raridade,Conservacao,Preco,QuantidadeEstoque)
        OUTPUT inserted.CartaId,inserted.QuantidadeEstoque INTO @novas(CartaId,Quantidade)
        SELECT @colecaoId,v.Numero,v.Nome,v.Raridade,v.Conservacao,v.Preco,v.Quantidade
        FROM (VALUES
            (N'025',N'Pikachu',N'Comum',N'NEAR_MINT',CONVERT(decimal(12,2),30.00),2),
            (N'025',N'Pikachu',N'Comum',N'SLIGHTLY_PLAYED',CONVERT(decimal(12,2),20.00),3),
            (N'004',N'Charmander',N'Comum',N'NEAR_MINT',CONVERT(decimal(12,2),15.50),1)
        ) v(Numero,Nome,Raridade,Conservacao,Preco,Quantidade)
        WHERE NOT EXISTS (SELECT 1 FROM dbo.Carta c WHERE c.ColecaoId=@colecaoId AND c.Numero=v.Numero AND c.Conservacao=v.Conservacao);
    -- Seed de instalacao, sem humano ficticio nem usuario/senha inventado.
    INSERT dbo.MovimentacaoEstoque(CartaId,Tipo,QuantidadeDelta,EstoqueApos,ResponsavelId,Motivo)
        SELECT CartaId,N'ENTRADA_INICIAL',Quantidade,Quantidade,NULL,N'SEED_SPRINT1' FROM @novas;
    COMMIT;
    SELECT c.CartaId,c.Numero,c.Nome,c.Conservacao,c.Preco,c.QuantidadeEstoque
        FROM dbo.Carta c WHERE c.ColecaoId=@colecaoId ORDER BY c.CartaId;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK;
    THROW;
END CATCH;
GO
