# Contrato SQL — sprint 1

Publicado pela frente de banco em 21/10/2026. Este contrato substitui o checkout integral proposto no plano inicial. As regras de `DECISOES.md` prevalecem. Os nomes abaixo serão usados nos scripts numerados de `database/`; resultados de execução constam em `entrega-sprint1-banco.md`.

## Conexão, tipos e acesso

- Base exclusiva `PokeVault` ou `PokeVaultTests`, schema `dbo`; preparação por scripts, sem migrations ou triggers.
- Dinheiro SQL `decimal(12,2)` e C# `decimal`; IDs `int`, quantidades `int`, datas UTC `datetime2(3)`, UUID `uniqueidentifier`. API deve recusar valores fora da faixa ou com mais de duas casas decimais antes de montar parâmetros/TVP, evitando arredondamento implícito na conversão SQL.
- `CartaId` corresponde a `ofertaId` na API. `Carta` é a oferta comercial por coleção/número/conservação.
- Códigos de perfil: ADMINISTRADOR, ATENDENTE, CLIENTE. Status: CONCLUIDO, CANCELADO. Conservações: MINT, NEAR_MINT, SLIGHTLY_PLAYED, MODERATELY_PLAYED, HEAVILY_PLAYED, DAMAGED.
- Todo parâmetro de ator autenticado deve ser extraído da sessão pela aplicação, nunca confiado ao corpo HTTP. SQL revalida perfil/ativação. Uma conexão SQL não identifica por si só o usuário web; a API constitui a fronteira de identidade.
- `EmailNormalizado` é `Email.Trim().ToUpperInvariant()` no C#. O banco exige unicidade. Hash criado em C# com `PasswordHasher<Usuario>`, recebido como texto opaco; não há senha/hash de usuário no seed SQL.
- Cada procedure de mutação controla sua própria transação; chamar sem `SqlTransaction` externa ou `TransactionScope`. Usar `CommandType.StoredProcedure`, tipos explícitos e consumir um único result set. `SET NOCOUNT ON` em todas.
- Não escrever saldo, estoque, compras, itens de compra ou movimentos diretamente em C#. Autorização da API é obrigatória mesmo quando SQL também a valida. Credencial de runtime recebe EXECUTE e SELECT, sem DML direto, conforme `006_permissoes.sql`.

## TVP e resultados de compra

Tipo `dbo.ItemCompraSelecionadoType`, enviado com `SqlDbType.Structured` e `TypeName` homônimo:

| Coluna, na ordem | Tipo SQL | Regra |
| --- | --- | --- |
| CartaId | int NOT NULL | PK do TVP, sem IDs repetidos |
| Quantidade | int NOT NULL | Positiva; deve ser exatamente a quantidade corrente da linha do carrinho |
| PrecoConfirmado | decimal(12,2) NOT NULL | Positivo; deve ser igual ao preço atual da carta |

`dbo.usp_ConcluirCompra(@ClienteId int, @ChaveOperacao uniqueidentifier, @Itens dbo.ItemCompraSelecionadoType READONLY)`.

O servidor serializa canonicamente a seleção ordenada por CartaId e compara o conteúdo completo, incluindo preços e quantidades, para idempotência. A chave é única por cliente. O reenvio idêntico consulta compra existente antes de exigir carrinho preenchido e retorna também compra já cancelada; não debita novamente. Mesma chave com outro conteúdo gera CHAVE_REUTILIZADA. Nova seleção/aceite usa nova chave. Somente linhas selecionadas são compradas e removidas; linhas não selecionadas permanecem. O conjunto é atômico, sem reserva ou compra parcial automática.

Resultado único, uma linha: `CompraId int`, `Status nvarchar(10)`, `Total decimal(12,2)`, `SaldoAtual decimal(12,2)`, `Repetida bit`. SaldoAtual é o saldo corrente no momento da chamada, não snapshot da resposta original. A aplicação deve manter a chave entre repetições por falha de transporte.

## Cancelamento e saldo

`dbo.usp_CancelarCompra(@AdministradorId int, @CompraId int, @Motivo nvarchar(300))`: administrador ativo; motivo obrigatório; cancelamento integral. Repete por CompraId, sem segunda chave, não altera autor/motivo anteriores e não devolve duas vezes. Funciona com cliente/carta desativados. Usa total e quantidades originais, sem preço corrente. Resultado com as mesmas cinco colunas da compra; Repetida indica compra já cancelada.

`dbo.usp_DefinirSaldo(@AdministradorId int, @ClienteId int, @NovoSaldo decimal(12,2), @Motivo nvarchar(300))`: substitui o saldo, registra diferença e autor. Novo saldo não negativo; cliente ativo. Sem movimento quando a diferença é zero. Resultado: `UsuarioId int`, `Saldo decimal(12,2)`, `Delta decimal(12,2)`.

## Catálogo e estoque

`dbo.usp_CadastrarCarta(@ResponsavelId int, @ColecaoId int, @Numero nvarchar(20), @Nome nvarchar(120), @Raridade nvarchar(50), @Conservacao nvarchar(30), @Preco decimal(12,2), @QuantidadeInicial int = 0, @ImagemUrl nvarchar(500) = NULL)`: administrador/atendente ativo, preço positivo, estoque inicial não negativo. Cria movimento ENTRADA_INICIAL quando maior que zero. Resultado: `CartaId int`.

`dbo.usp_AjustarCarta(@ResponsavelId int, @CartaId int, @Preco decimal(12,2), @NovoEstoque int, @Ativo bit, @Motivo nvarchar(300))`: administrador/atendente ativo; define preço, estoque absoluto e ativação. Registra AJUSTE_ADMIN de estoque pela diferença, inclusive quando feito por atendente (nome do tipo não concede privilégio). Motivo obrigatório mesmo sem mudança de estoque. Resultado: `CartaId int`, `Preco decimal(12,2)`, `QuantidadeEstoque int`, `Ativo bit`, `DeltaEstoque int`.

O nome AJUSTE_ADMIN nas movimentações significa ajuste operacional, não exclusividade de administrador no estoque. Saldo continua exclusivo do administrador.

## Usuários e carrinho

`dbo.usp_CriarUsuario(@AdministradorId int, @Nome nvarchar(120), @Email nvarchar(254), @EmailNormalizado nvarchar(254), @SenhaHash nvarchar(512), @PerfilCodigo nvarchar(20))`: administrador ativo provisiona CLIENTE ou ATENDENTE. Cliente nasce com saldo zero e carrinho vazio atomicamente. Não permite criar outro administrador por essa rotina. Resultado: `UsuarioId int`.

O administrador inicial é provisionado pelo bootstrap C# de desenvolvimento, com transação de INSERT em Usuario e `AdministradorFixo = 1`, somente quando não existe administrador fixo; é uma operação de instalação com credencial de preparação, não runtime. A aplicação deve gerar hash, não colocar senha em SQL. Perfil é resolvido por Codigo; não depender de IDs fixos de perfis.

`dbo.usp_DefinirUsuarioAtivo(@AdministradorId int, @UsuarioId int, @Ativo bit)`: administrador ativo; não permite desativar administrador fixo. Resultado: `UsuarioId int`, `Ativo bit`. Revalidar conta em todas as requisições, inclusive cookies existentes.

`dbo.usp_DefinirItemCarrinho(@ClienteId int, @CartaId int, @Quantidade int)`: cliente ativo; quantidade positiva insere/substitui linha; zero remove; negativo recusa. Não exige estoque suficiente nem reserva. Inclusão exige carta ativa; remoção funciona para carta desativada. Resultado: `CarrinhoId int`, `CartaId int`, `Quantidade int` (zero = removida). Todas as alterações do carrinho bloqueiam cabeçalho; compra bloqueia o mesmo cabeçalho.

## Leituras compartilhadas por web/API

SELECT autorizado pela aplicação, com parâmetros, sem procedures adicionais obrigatórias:

- Login: Usuario JOIN Perfil, localizar EmailNormalizado; retornar UsuarioId, Nome, SenhaHash, Ativo, AdministradorFixo e Codigo. Validar hash em C#.
- Catálogo público: Carta JOIN Colecao, apenas Carta.Ativo = 1; filtros por nome, ColecaoId e Conservacao; ordenar por CartaId, `OFFSET/FETCH`, limites de paginação na aplicação. Retornar preço e quantidade atuais. Sem imagens externas obrigatórias.
- Carrinho do cliente: Carrinho JOIN ItemCarrinho JOIN Carta, filtrar UsuarioId autenticado; retornar inclusive cartas inativas para permitir remover; preço é sempre corrente.
- Histórico: Compra WHERE ClienteId = usuário autenticado, e ItemCompra pelos IDs permitidos. Cabeçalho tem `ClienteId`, `CriadaEmUtc`, `CanceladaEmUtc`, `CanceladaPorId`, `MotivoCancelamento`; itens possuem nome/conservação snapshot e PrecoUnitario.
- Saldo: SaldoCliente WHERE UsuarioId = cliente autenticado. Administração pode consultar com autorização específica.

Schema completo em `001_schema.sql`. Subtotal = Quantidade × PrecoUnitario; não há coluna Subtotal. Total é calculado/gravado pela procedure.

## Erros estáveis para integração

Business errors são `THROW` com número fixo e mensagem igual ao código, estado 1. A API mapeia `SqlException.Number`; não expor mensagens técnicas ao usuário.

| Número | Código | HTTP sugerido |
| --- | --- | --- |
| 51000 | ACESSO_NEGADO | 403 (401 tratado antes pela API) |
| 51001 | USUARIO_INATIVO_OU_INVALIDO | 403 |
| 51002 | DADOS_INVALIDOS | 400 |
| 51003 | COMPRA_NAO_ENCONTRADA | 404 |
| 51004 | CHAVE_REUTILIZADA | 409 |
| 51005 | CARRINHO_ALTERADO | 409 |
| 51006 | CARTA_INDISPONIVEL | 409 |
| 51007 | PRECO_ALTERADO | 409, exigir nova confirmação |
| 51008 | ESTOQUE_INSUFICIENTE | 409 |
| 51009 | SALDO_INSUFICIENTE | 409 |
| 51010 | CADASTRO_DUPLICADO | 409 |
| 51011 | ADMINISTRADOR_FIXO | 409 |

TVP duplicado pode falhar com erro de PK antes do corpo da procedure; a API deve validar IDs repetidos. Erros SQL 2601/2627 são conflito de unicidade; 1205 é deadlock e permite repetição controlada com mesma chave; falhas de transporte/timeout exigem consultar/repetir a operação com mesma chave. Falhas técnicas permanecem separadas de recusas de negócio. Compra vazia é DADOS_INVALIDOS; não há compra recusada persistida.

## Massa e estado da publicação

`005_seed.sql`: perfis, coleção `MEW-151` e três cartas de demonstração: Pikachu (`025`) Near Mint R$ 30,00/estoque 2; Pikachu (`025`) Slightly Played R$ 20,00/estoque 3; Charmander (`004`) Near Mint R$ 15,50/estoque 1. CLI-01 com R$ 100,00 e CLI-02 com R$ 10,00 serão provisionados pela aplicação/testes usando hashes e a procedure de definição de saldo. IDs devem ser descobertos pelas chaves naturais, nunca presumidos. A reexecução do seed não restaura estoque nem preço de uma carta já existente.

Exceção explícita de autoria: a carga inicial SQL registra `MovimentacaoEstoque.ResponsavelId = NULL` exclusivamente em ENTRADA_INICIAL com motivo `SEED_SPRINT1`, pois antecede o bootstrap de usuários. Isso identifica a origem técnica sem inventar um autor humano. Todas as movimentações operacionais exigem responsável. São 11 tabelas de negócio e uma tabela técnica `SchemaVersao`.

Scripts: `000_preparar_base.sql`, `001_schema.sql`, `002_tipos.sql`, `003_procedures_compra.sql`, `004_procedures_operacao.sql`, `005_seed.sql`, `006_permissoes.sql`; consulta opcional `007_verificar.sql`. Rotinas usam sintaxe de SQL Server 2017 ou superior (TRIM e CREATE OR ALTER); validar no motor antes do aceite. Não criam logins nem alteram configuração de segurança. O papel runtime deve ser associado pelo operador a uma conta de aplicação sem privilégios administrativos; não usar sysadmin para avaliar o efeito de DENY.

O contrato foi publicado antes dos scripts para permitir integração paralela. Execução, instância, versão e resultados reais serão registrados na entrega; a publicação não significa SQL validado em execução.
