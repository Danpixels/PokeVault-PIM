# Sprint 1 — preparação, roteiro e registro de testes

Registro real: 21/10/2026, America/Sao_Paulo. Preparação por Codex com apoio de IA; nenhuma execução atribuída a Daniel ou a outro integrante. 07/09/2026 é início de referência informado, não data de execução destes testes.

## Resultado desta rodada

**Execução funcional bloqueada; 0 cenários funcionais executados, 0 aprovados, 0 reprovados.** Os 30 casos S1 da matriz T01–T36 estão bloqueados para execução integrada; os seis casos S2 ficam planejados para a rodada correspondente:

- S1: T01, T02, T08–T35 = 30 casos, todos bloqueados nesta preparação.
- S2: T03–T07 e T36 = 6 casos, não executados / previstos para sprint 2.

Foi executada somente inspeção estática de prontidão por `tests/Inspecionar-Prontidao.ps1`. Resultado em `evidencias/sprint1-preparacao-inventario.json`, com horário UTC e hashes: decisões, diário, contrato e projeto MVC presentes; zero scripts SQL encontrados naquele instante. O Program.cs lido ainda era a estrutura MVC inicial sem autenticação/catálogo do domínio. Esses são instantâneos de trabalho simultâneo, não defeitos atribuídos às outras frentes nem prova de estado posterior.

Contrato SQL lido após sua publicação. Especifica nomes, TVP, resultados e erros, mas declara publicação anterior aos scripts e não comprova execução. A diferença transitória entre os preços descritos no contrato inicial e a massa abaixo está sob conciliação pela frente de banco; não foi registrada como defeito funcional.

## Dependências para desbloquear

| Bloqueio | Evidência/limite atual | Condição de liberação |
| --- | --- | --- |
| B01 — SQL controlado | Inventário registrou zero .sql; não houve conexão SQL nesta frente | Scripts reais, ordem de execução e schema entregues; instância/base PokeVaultTests identificadas; permissão e preparação de massa documentadas |
| B02 — app/contrato HTTP | MVC inicial inspecionado; autenticação/antifalsificação em desenvolvimento | Versão identificada com login, catálogo, cookie/token e rotas/DTOs reais; URL local e procedimento de início |
| B03 — massa | Valores são preparação esperada, não saldo/estoque carregados | ADM/atendente/C1/C2 provisionados com hashes por mecanismo da aplicação; IDs descobertos; preços/saldos conferidos |
| B04 — falha controlada | Não há mecanismo de falha após escrita identificado | Método restrito à base de testes, ponto após primeira escrita demonstrado e verificação de rollback |

Não criar/resetar base desconhecida. Não mudar src/, database/, arquivos centrais ou configuração de outras frentes para forçar testes. Não manter transação externa ao chamar procedures: contrato diz que elas controlam suas próprias transações.

## Preparação verificável

1. Ler a entrega real do banco/desenvolvimento, schema em database/ e contrato-sprint1.md; registrar hash dos arquivos e versão da aplicação. Não usar plano como substituto do DDL.
2. Confirmar servidor/instância e `DB_NAME()` antes de qualquer preparação. Usar PokeVaultTests exclusiva, com autorização/propriedade documentadas. Não reaproveitar PokeVault de demonstração para testes destrutivos.
3. Aplicar scripts somente pelo procedimento entregue pelo banco, com credencial de preparação. Runtime deve usar credencial restrita. Registrar erros e códigos de saída sem strings de conexão/segredos.
4. Provisionar contas fictícias via bootstrap da aplicação e rotinas autorizadas. Descobrir perfis por Codigo e contas por EmailNormalizado; nunca presumir IDs nem gravar senha em SQL.
5. Descobrir cartas por coleção+número+conservação, conferindo nomes: Pikachu NEAR_MINT a 30/estoque2; Pikachu SLIGHTLY_PLAYED a 20/estoque3; Charmander NEAR_MINT a 15,50/estoque1. C1=100; C2=10. Ajustar via rotinas autorizadas, com motivo de teste, preservando auditoria. Não presumir carga já conciliada.
6. Registrar estado anterior, carrinho e movimentos por IDs. Rodar uma sequência por vez; concorrência usa apenas o caso explicitamente preparado, duas conexões e barreira de início. Nunca presumir que abrir duas janelas prova simultaneidade.
7. Usar `tests/Conferir-Estado.sql` somente após conferir seus nomes contra o DDL real. O arquivo é leitura, sem teste funcional embutido; preencher IDs positivos e usar a base correta. Guardar resultado antes/depois em arquivos separados.
8. Preparar quatro sessões HTTP independentes (anônima, ADM, atendente e cliente), cookies isolados e antifalsificação obtida pelos mecanismos reais. Não inventar token/URL. Capturas sanitizadas; nenhum cookie ou senha nos arquivos de evidência.

## Contrato real disponível e oráculos

| Casos | Chamadas SQL publicadas / observação | Resultado esperado a conferir |
| --- | --- | --- |
| T08/T10–T16/T21/T22/T27–T33 | dbo.usp_ConcluirCompra; @ClienteId int, @ChaveOperacao uniqueidentifier, @Itens dbo.ItemCompraSelecionadoType READONLY | TVP na ordem CartaId int, Quantidade int, PrecoConfirmado decimal(12,2); retorno CompraId/Status/Total/SaldoAtual/Repetida |
| T17–T20/T25/T34/T35 | dbo.usp_CancelarCompra(@AdministradorId, @CompraId, @Motivo) | Mesmo formato de retorno; Repetida=1 na repetição; SaldoAtual é corrente, não resposta original congelada |
| T06/T07/T33/T35 | dbo.usp_DefinirSaldo(@AdministradorId,@ClienteId,@NovoSaldo,@Motivo) | UsuarioId, Saldo, Delta; repetição da definição sem mudança tem Delta=0 e não cria movimento |
| Preparação de carrinho | dbo.usp_DefinirItemCarrinho(@ClienteId,@CartaId,@Quantidade) | Quantidade positiva substitui linha; zero remove; negativo recusa. Quantidade zero no carrinho é remoção; zero no checkout é inválido |
| Preparação de preço/estoque | dbo.usp_AjustarCarta(@ResponsavelId,@CartaId,@Preco,@NovoEstoque,@Ativo,@Motivo) | NovoEstoque absoluto, movimento pela diferença; permitido ao atendente, saldo não |
| Conta inativa / provisão | dbo.usp_DefinirUsuarioAtivo; dbo.usp_CriarUsuario | Usar assinaturas do contrato, não duplicar bootstrap em tests/ |

Erros de negócio SQL a conferir, sem alegação de observação: 51000 ACESSO_NEGADO; 51001 USUARIO_INATIVO_OU_INVALIDO; 51002 DADOS_INVALIDOS; 51003 COMPRA_NAO_ENCONTRADA; 51004 CHAVE_REUTILIZADA; 51005 CARRINHO_ALTERADO; 51006 CARTA_INDISPONIVEL; 51007 PRECO_ALTERADO; 51008 ESTOQUE_INSUFICIENTE; 51009 SALDO_INSUFICIENTE; 51010 CADASTRO_DUPLICADO; 51011 ADMINISTRADOR_FIXO. Mapeamentos HTTP do contrato são sugeridos, pendentes de confirmação na implementação: 400 para dados inválidos, 403 para autorização/inativação, 404 para compra inexistente, 409 para conflitos. API anônima protegida deve responder 401 sem redirecionar para HTML de login. Antifalsificação: registrar código real e ausência de efeitos; não confundir sua rejeição com validação de regra financeira.

IDs duplicados podem falhar na PK do TVP antes da procedure; testar também validação HTTP. Deadlock SQL 1205 não pode ser registrado como sucesso; exigir rollback e repetição controlada com a mesma chave, preservando respostas reais. Erros 2601/2627 de unicidade e timeout devem ser documentados separadamente.

## Sequências com valores de conferência

| Sequência independente | Esperado, nunca resultado observado nesta rodada |
| --- | --- |
| Seleção simples T10→T17→T19 | Carrinho NM1/SP1/B1; comprar só NM: saldo70/NM1/SP3/B1, carrinho SP1/B1. Cancelar: saldo100/NM2; carrinho continua SP1/B1. Repetir: nenhum efeito adicional |
| Seleção múltipla T11→T18 | Comprar NM1/SP1: total50/saldo50/NM1/SP2, B1 no carrinho. Cancelar: saldo100/NM2/SP3, B1 permanece |
| Preço T27 | Preço NM 30→40 após confirmação: 51007 e estado intacto. Após ação explícita de novo aceite/chave, total40/saldo60/NM1 |
| Reenvio T28/T29 | Repetir UUID/conteúdo ordenado de outra forma: mesma CompraId/Repetida=1; alterar conteúdo: 51004, nenhum efeito extra |
| Concorrência T21 | Dois clientes saldo100; B estoque1: uma compra15,50, um saldo84,50 e outro100, estoque0 |
| Saldo T22 | C1 saldo30/NM2: no máximo uma aprovação por UUIDs distintos; saldo0/NM1, outra recusa sem efeitos |
| Cancelamento T20 | Duas chamadas sobre mesma compra: um ESTORNO de saldo e um por carta; saldo/estoque repostos uma vez |
| Saldo corrente T35 | Comprar30 partindo100, definir saldo40 (delta−30), cancelar30: saldo70 |

Antes de T23, definir com banco método de induzir falha após escrita sem alterar procedures de produção e sem usar transação externa. Uma entrada inválida rejeitada antes de iniciar escritas não demonstra rollback. Se indisponível, manter T23 bloqueado.

## Demonstração proposta, ainda não ensaiada

1. Mostrar catálogo sem login, filtros e conservações, versão e ambiente local com saldo fictício.
2. Autenticar C1; montar NM1/SP1/B1 e selecionar NM+SP; mostrar total50 e B não selecionada.
3. Confirmar compra, mostrar saldo50/NM1/SP2 e carrinho remanescente B1; abrir histórico.
4. Fazer tentativas independentes de estoque/saldo insuficientes com estados antes/depois identificados.
5. Como ADM, cancelar a compra com motivo: saldo100/NM2/SP3; histórico cancelado e B1 preservada.
6. Repetir cancelamento e UUID original; mostrar ausência de novos efeitos/movimentos.
7. Exibir resultados reais de antifalsificação, concorrência, preço e rollback quando disponíveis, junto às limitações. Não apresentar esse roteiro como execução.

## Registro para execução futura

Uma linha por tentativa: ID do caso, versão/hash e ambiente, data/hora/fuso, executor real, base exclusiva, pré-condições/IDs/UUIDs, passos efetivos, esperado, observado, status (aprovado/reprovado/bloqueado), evidências existentes, defeito e reteste. Guardar evidências em pasta de rodada distinta; preservar falhas anteriores. Resultado observado não pode ser preenchido copiando o esperado.

Nesta rodada: observação funcional **não disponível** para todos os casos. O único artefato de observação é o inventário estático citado acima. Próximo passo: conferir DDL/entregas e executar primeiro T01/T02/T09/T26 quando as dependências forem liberadas.


## Atualização de inspeção no encerramento da preparação

Ainda em 21/10/2026, a segunda captura encontrou oito scripts SQL, incluindo 000_preparar_base.sql, 001_schema.sql a 006_permissoes.sql e 007_verificar.sql, além de LojaApiController.cs, LojaController.cs e FluxoModels.cs. Nova evidência estática: evidencias/sprint1-preparacao-inventario-02.json. Isso supera a ausência inicial de arquivos, mas não comprova SQL implantado nem HTTP executável. Program.cs relido continuava inicial, e ainda não havia entrega-sprint1-banco.md/entrega-sprint1-desenvolvimento.md na listagem consultada. B01 passa a significar base/implantação/massa pendentes, e B02 integração/configuração/runtime pendentes; não ausência de arquivos fonte.

O DDL 001_schema.sql foi lido e os nomes das tabelas/colunas usados em tests/Conferir-Estado.sql conferem textualmente com ele. Isso é revisão estática, não compilação nem execução SQL. 005_seed.sql já descreve Pikachu NM30/2, SP20/3 e Charmander NM15,50/1; a divergência transitória inicial de massa foi conciliada no código. IDs e carga real seguem não verificados.

Rotas observadas no código, **não testadas**:

| Operação | Rota real lida | Preparação HTTP |
| --- | --- | --- |
| Token | GET /api/v1/antiforgery | Resposta prevista token/header; header X-CSRF-TOKEN; usar sessão correta, sem persistir segredo |
| Login | POST /api/v1/sessao | LoginModel com Email/Senha; não armazenar payload com senha em evidência |
| Identidade/logout | GET /api/v1/sessao; DELETE /api/v1/sessao | Cookie isolado por perfil; testar logout e requisição protegida posterior |
| Catálogo | GET /api/v1/ofertas | Parâmetros nome, colecao, conservacao, pagina; anônimo permitido no atributo |
| Carrinho/saldo | GET /api/v1/carrinho; GET /api/v1/me/saldo | Perfil CLIENTE |
| Linha | PUT /api/v1/carrinho/itens/{ofertaId}; DELETE mesma rota | QuantidadeModel possui Quantidade; zero remove pelo contrato SQL |
| Compra/histórico | POST /api/v1/compras; GET /api/v1/compras | CompraModel possui ChaveOperacao e Itens; conferir ItemSelecionado/serialização antes de automatizar payload |
| Detalhe/cancelar | GET /api/v1/compras/{compraId}; POST /api/v1/compras/{compraId}/cancelamento | CancelamentoModel possui Motivo; cancelamento requer ADMINISTRADOR |
| MVC | GET /; GET/POST /entrar; POST /sair; GET /carrinho; POST /carrinho/confirmar; POST /comprar | Captura real necessária para provar navegação/seleção/novo aceite |

Não inferir antifalsificação efetiva apenas do endpoint de token nem autenticação efetiva apenas dos atributos Authorize: registro de serviços, filtros e middleware ainda devem ser entregues/verificados. Status funcional continua: 0 executados, dependências abertas.

