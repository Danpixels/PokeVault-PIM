USE [$(DatabaseName)];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() NOT IN (N'PokeVault', N'PokeVaultTests') OR NOT EXISTS
 (SELECT 1 FROM sys.extended_properties WHERE class=0 AND name=N'PokeVaultProjeto' AND CONVERT(nvarchar(100),value)=N'PIM-2026-Sprint1')
    THROW 51101, 'BASE_EXISTENTE_NAO_IDENTIFICADA', 1;
IF OBJECT_ID(N'dbo.SchemaVersao', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.SchemaVersao WHERE Versao=1)
        RETURN;
    THROW 51102, 'SCHEMA_INESPERADO', 1;
END;
-- Sem DROP ou reset. Falha se houver estrutura conflitante.
BEGIN TRY
BEGIN TRANSACTION;
CREATE TABLE dbo.Perfil (
    PerfilId int IDENTITY PRIMARY KEY,
    Codigo nvarchar(20) NOT NULL UNIQUE,
    CONSTRAINT CK_Perfil_Codigo CHECK (Codigo IN (N'ADMINISTRADOR',N'ATENDENTE',N'CLIENTE'))
);
CREATE TABLE dbo.Usuario (
    UsuarioId int IDENTITY PRIMARY KEY,
    PerfilId int NOT NULL REFERENCES dbo.Perfil(PerfilId),
    Nome nvarchar(120) NOT NULL,
    Email nvarchar(254) NOT NULL,
    EmailNormalizado nvarchar(254) NOT NULL UNIQUE,
    SenhaHash nvarchar(512) NOT NULL,
    Ativo bit NOT NULL CONSTRAINT DF_Usuario_Ativo DEFAULT 1,
    AdministradorFixo bit NOT NULL CONSTRAINT DF_Usuario_Fixo DEFAULT 0,
    CriadoEmUtc datetime2(3) NOT NULL CONSTRAINT DF_Usuario_Data DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Usuario_Texto CHECK (LEN(TRIM(Nome))>0 AND LEN(TRIM(Email))>0 AND LEN(TRIM(EmailNormalizado))>0 AND LEN(SenhaHash)>0),
    CONSTRAINT CK_Usuario_Fixo CHECK (AdministradorFixo=0 OR Ativo=1)
);
CREATE UNIQUE INDEX UX_Usuario_AdministradorFixo ON dbo.Usuario(AdministradorFixo) WHERE AdministradorFixo=1;
CREATE TABLE dbo.SaldoCliente (
    UsuarioId int PRIMARY KEY REFERENCES dbo.Usuario(UsuarioId),
    Saldo decimal(12,2) NOT NULL CONSTRAINT DF_Saldo_Zero DEFAULT 0,
    CONSTRAINT CK_Saldo_Valor CHECK (Saldo>=0)
);
CREATE TABLE dbo.Colecao (
    ColecaoId int IDENTITY PRIMARY KEY,
    Codigo nvarchar(30) NOT NULL UNIQUE,
    Nome nvarchar(120) NOT NULL
);
CREATE TABLE dbo.Carta (
    CartaId int IDENTITY PRIMARY KEY,
    ColecaoId int NOT NULL REFERENCES dbo.Colecao(ColecaoId),
    Numero nvarchar(20) NOT NULL,
    Nome nvarchar(120) NOT NULL,
    Raridade nvarchar(50) NOT NULL,
    ImagemUrl nvarchar(500) NULL,
    Conservacao nvarchar(30) NOT NULL,
    Preco decimal(12,2) NOT NULL,
    QuantidadeEstoque int NOT NULL CONSTRAINT DF_Carta_Estoque DEFAULT 0,
    Ativo bit NOT NULL CONSTRAINT DF_Carta_Ativo DEFAULT 1,
    CONSTRAINT UQ_Carta_Comercial UNIQUE (ColecaoId,Numero,Conservacao),
    CONSTRAINT CK_Carta_Valores CHECK (Preco>0 AND QuantidadeEstoque>=0),
    CONSTRAINT CK_Carta_Texto CHECK (LEN(TRIM(Numero))>0 AND LEN(TRIM(Nome))>0 AND LEN(TRIM(Raridade))>0),
    CONSTRAINT CK_Carta_Conservacao CHECK (Conservacao IN (N'MINT',N'NEAR_MINT',N'SLIGHTLY_PLAYED',N'MODERATELY_PLAYED',N'HEAVILY_PLAYED',N'DAMAGED'))
);
CREATE TABLE dbo.Carrinho (
    CarrinhoId int IDENTITY PRIMARY KEY,
    UsuarioId int NOT NULL UNIQUE REFERENCES dbo.Usuario(UsuarioId)
);
CREATE TABLE dbo.ItemCarrinho (
    CarrinhoId int NOT NULL REFERENCES dbo.Carrinho(CarrinhoId),
    CartaId int NOT NULL REFERENCES dbo.Carta(CartaId),
    Quantidade int NOT NULL,
    CONSTRAINT PK_ItemCarrinho PRIMARY KEY (CarrinhoId,CartaId),
    CONSTRAINT CK_ItemCarrinho_Quantidade CHECK (Quantidade>0)
);
CREATE TABLE dbo.Compra (
    CompraId int IDENTITY PRIMARY KEY,
    ClienteId int NOT NULL REFERENCES dbo.Usuario(UsuarioId),
    ChaveOperacao uniqueidentifier NOT NULL,
    ResumoRequisicao nvarchar(max) NOT NULL,
    Status nvarchar(10) NOT NULL,
    Total decimal(12,2) NOT NULL,
    CriadaEmUtc datetime2(3) NOT NULL CONSTRAINT DF_Compra_Data DEFAULT SYSUTCDATETIME(),
    CanceladaEmUtc datetime2(3) NULL,
    CanceladaPorId int NULL REFERENCES dbo.Usuario(UsuarioId),
    MotivoCancelamento nvarchar(300) NULL,
    CONSTRAINT UQ_Compra_Chave UNIQUE (ClienteId,ChaveOperacao),
    CONSTRAINT UQ_Compra_Cliente UNIQUE (CompraId,ClienteId),
    CONSTRAINT CK_Compra_Total CHECK (Total>0),
    CONSTRAINT CK_Compra_Estado CHECK (
      (Status=N'CONCLUIDO' AND CanceladaEmUtc IS NULL AND CanceladaPorId IS NULL AND MotivoCancelamento IS NULL)
      OR (Status=N'CANCELADO' AND CanceladaEmUtc IS NOT NULL AND CanceladaPorId IS NOT NULL AND MotivoCancelamento IS NOT NULL AND LEN(TRIM(MotivoCancelamento))>0))
);
CREATE TABLE dbo.ItemCompra (
    CompraId int NOT NULL REFERENCES dbo.Compra(CompraId),
    CartaId int NOT NULL REFERENCES dbo.Carta(CartaId),
    Quantidade int NOT NULL,
    PrecoUnitario decimal(12,2) NOT NULL,
    NomeCartaSnapshot nvarchar(120) NOT NULL,
    ConservacaoSnapshot nvarchar(30) NOT NULL,
    CONSTRAINT PK_ItemCompra PRIMARY KEY (CompraId,CartaId),
    CONSTRAINT CK_ItemCompra_Valores CHECK (Quantidade>0 AND PrecoUnitario>0)
);
CREATE TABLE dbo.MovimentacaoSaldo (
    MovimentacaoSaldoId int IDENTITY PRIMARY KEY,
    UsuarioId int NOT NULL REFERENCES dbo.SaldoCliente(UsuarioId),
    CompraId int NULL,
    Tipo nvarchar(20) NOT NULL,
    ValorDelta decimal(12,2) NOT NULL,
    SaldoApos decimal(12,2) NOT NULL,
    ResponsavelId int NOT NULL REFERENCES dbo.Usuario(UsuarioId),
    Motivo nvarchar(300) NULL,
    CriadaEmUtc datetime2(3) NOT NULL CONSTRAINT DF_MovSaldo_Data DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_MovSaldo_Compra FOREIGN KEY (CompraId,UsuarioId) REFERENCES dbo.Compra(CompraId,ClienteId),
    CONSTRAINT CK_MovSaldo_Valores CHECK (ValorDelta<>0 AND SaldoApos>=0),
    CONSTRAINT CK_MovSaldo_Tipo CHECK (
      (Tipo=N'COMPRA' AND CompraId IS NOT NULL AND ValorDelta<0)
      OR (Tipo=N'ESTORNO' AND CompraId IS NOT NULL AND ValorDelta>0)
      OR (Tipo=N'AJUSTE_ADMIN' AND CompraId IS NULL AND Motivo IS NOT NULL AND LEN(TRIM(Motivo))>0))
);
CREATE UNIQUE INDEX UX_MovSaldo_Compra ON dbo.MovimentacaoSaldo(CompraId,Tipo) WHERE CompraId IS NOT NULL;
CREATE TABLE dbo.MovimentacaoEstoque (
    MovimentacaoEstoqueId int IDENTITY PRIMARY KEY,
    CartaId int NOT NULL REFERENCES dbo.Carta(CartaId),
    CompraId int NULL,
    Tipo nvarchar(20) NOT NULL,
    QuantidadeDelta int NOT NULL,
    EstoqueApos int NOT NULL,
    ResponsavelId int NULL REFERENCES dbo.Usuario(UsuarioId),
    Motivo nvarchar(300) NULL,
    CriadaEmUtc datetime2(3) NOT NULL CONSTRAINT DF_MovEstoque_Data DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_MovEstoque_Item FOREIGN KEY (CompraId,CartaId) REFERENCES dbo.ItemCompra(CompraId,CartaId),
    CONSTRAINT CK_MovEstoque_Valores CHECK (QuantidadeDelta<>0 AND EstoqueApos>=0),
    CONSTRAINT CK_MovEstoque_Autor CHECK (ResponsavelId IS NOT NULL OR (Tipo=N'ENTRADA_INICIAL' AND Motivo=N'SEED_SPRINT1')),
    CONSTRAINT CK_MovEstoque_Tipo CHECK (
      (Tipo=N'COMPRA' AND CompraId IS NOT NULL AND QuantidadeDelta<0)
      OR (Tipo=N'ESTORNO' AND CompraId IS NOT NULL AND QuantidadeDelta>0)
      OR (Tipo=N'AJUSTE_ADMIN' AND CompraId IS NULL AND Motivo IS NOT NULL AND LEN(TRIM(Motivo))>0)
      OR (Tipo=N'ENTRADA_INICIAL' AND CompraId IS NULL AND QuantidadeDelta>0 AND Motivo IS NOT NULL AND LEN(TRIM(Motivo))>0))
);
CREATE UNIQUE INDEX UX_MovEstoque_Compra ON dbo.MovimentacaoEstoque(CompraId,CartaId,Tipo) WHERE CompraId IS NOT NULL;
CREATE INDEX IX_Compra_Historico ON dbo.Compra(ClienteId,CriadaEmUtc);
CREATE INDEX IX_Carta_Catalogo ON dbo.Carta(ColecaoId,Conservacao,Ativo);
CREATE INDEX IX_MovSaldo_Historico ON dbo.MovimentacaoSaldo(UsuarioId,CriadaEmUtc);
CREATE INDEX IX_MovEstoque_Historico ON dbo.MovimentacaoEstoque(CartaId,CriadaEmUtc);
CREATE TABLE dbo.SchemaVersao (
    Versao int PRIMARY KEY,
    AplicadaEmUtc datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME()
);
INSERT dbo.SchemaVersao(Versao) VALUES (1);
COMMIT;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK;
    THROW;
END CATCH;
GO
