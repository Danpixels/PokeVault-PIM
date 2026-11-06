# PokéVault — plano e implementação proposta do banco SQL Server

Atualização: 21/10/2026, sprint 2 autorizada. Este arquivo incorpora as decisões e a execução técnica registradas em `docs/DECISOES.md`, `docs/diario-sprints.md` e `docs/entrega-sprint2-banco.md`. O PDF é referência acadêmica; suas afirmações não comprovam implementação ou execução. A reescrita acadêmica final fica para depois do sistema.

## Escopo consolidado

Uma base compartilhada pela aplicação ASP.NET Core e API, acessível pelo aplicativo somente via API. Catálogo público, perfis ADMINISTRADOR/ATENDENTE/CLIENTE, carteira fictícia por cliente, carrinho persistente sem reserva, compra selecionada atômica e cancelamento integral administrativo. Estados de compra: CONCLUIDO e CANCELADO. Buyback, relatórios, logística, endereços, pagamentos reais e reserva por prazo continuam fora da sprint.

Carta é o produto comercial por coleção/número/conservação, com preço e estoque próprios. `ofertaId` da API corresponde a CartaId. Demonstração sem variantes adicionais de idioma/acabamento. Dinheiro `decimal(12,2)`, quantidades `int`, textos `nvarchar`, UTC em `datetime2(3)`. Hash criado por `PasswordHasher<Usuario>` no C#; scripts SQL não armazenam credenciais demonstrativas.

## Modelo atual dos scripts

| Tabela | Relacionamentos e regras principais |
| --- | --- |
| Perfil | Código único de um dos três perfis; 1:N Usuario. |
| Usuario | FK PerfilId; email normalizado único; Nome, Email, SenhaHash, Ativo, AdministradorFixo e data UTC. No máximo um administrador fixo, sempre ativo. |
| SaldoCliente | UsuarioId é PK/FK; saldo não negativo e Versao rowversion. Rotina de criação garante uma carteira para cliente, nenhuma para funcionário. |
| Colecao | Código único, nome; 1:N Carta. |
| Carta | FK ColecaoId; UNIQUE coleção/número/conservação; preço positivo, estoque não negativo, Ativo e Versao rowversion. Cada conservação é outra linha comercial. |
| Carrinho | FK UsuarioId com UNIQUE; criado junto com cliente. Cabeçalho bloqueado nas alterações de itens e na compra. |
| ItemCarrinho | PK CarrinhoId/CartaId; FKs e quantidade positiva; sem preço definitivo ou reserva. |
| Compra | FK ClienteId; UNIQUE ClienteId/ChaveOperacao; resumo canônico da seleção, total e status; data, autor e motivo no cancelamento. |
| ItemCompra | PK CompraId/CartaId; FKs; quantidade/preço originais; snapshots de nome e conservação. |
| MovimentacaoSaldo | FKs para carteira, compra/cliente e autor; delta, saldo após, tipo, motivo e UTC. Uma baixa e um estorno no máximo por compra. |
| MovimentacaoEstoque | FKs para carta, item da compra e autor; delta, estoque após, tipo, motivo e UTC. Uma baixa e um estorno no máximo por item. |
| AlteracaoCarta | Histórico de ajustes versionados: FK CartaId/ResponsavelId, motivo, preço/estoque/ativação anterior e novo, data UTC. Inclui alterações só de preço/ativação, sem movimento de estoque zero. |
| SchemaVersao | Tabela técnica: versão do schema e data da aplicação. Não é uma entidade comercial. |

São 12 tabelas de negócio e uma técnica. Campos iniciais em `database/001_schema.sql`; atualizações aditivas em 008–010 e 013. Contrato original em `docs/contrato-sprint1.md`, novas rotinas de gestão em `docs/contrato-sprint2.md`. Versões de publicação: 1 inicial, 2 rowversions, 3 ajustes versionados, 4 auditoria de carta. Scripts históricos executados são preservados; 013 complementa 009 sem mudar sua assinatura.

Conservações: MINT, NEAR_MINT, SLIGHTLY_PLAYED, MODERATELY_PLAYED, HEAVILY_PLAYED, DAMAGED. CHECK limita o domínio; FK protege referências; índices únicos protegem identidade comercial, carrinho, chaves e movimentos repetidos. Não excluir compras, itens e movimentos; usuários e cartas são desativados. Não alterar a identidade de uma carta já vendida para representar outro produto.

A igualdade entre total e itens, a existência de itens, a associação de carteira/carrinho a cliente, os papéis autorizados e os valores exatos dos movimentos são garantidos pelas rotinas, não por CHECKs entre tabelas. O papel runtime nega DML direto e permite executar procedures; precisa ser associado a uma conta sem privilégios administrativos e validado em execução.

## Compra selecionada e idempotência

A API recebe UUID e lista selecionada de CartaId, quantidade e preço confirmado; identidade do cliente vem da sessão. Cada quantidade selecionada deve corresponder à linha completa atual do carrinho. O conjunto inteiro é comprado ou nada é alterado. Linhas não selecionadas permanecem.

`usp_ConcluirCompra` valida cliente ativo, bloqueia carteira, procura compra existente pela chave e compara representação canônica ordenada da seleção. Repetição idêntica retorna a compra e seu estado atual mesmo após remoção dos itens; chave com conteúdo divergente gera conflito. SaldoAtual da resposta é corrente, não o saldo da resposta original. Nova tentativa depois de alterar seleção/preço deve usar nova chave.

Para compra nova, a rotina bloqueia cabeçalho do carrinho e cartas por ID crescente, confere linhas, ativação, preço e estoque; calcula total no servidor; debita carteira/estoque com atualização condicionada; grava compra, itens/snapshots e movimentos; remove apenas linhas selecionadas e confirma tudo. Mudança de preço gera conflito que exige novo aceite. Falha provoca rollback integral; não grava compra recusada.

## Cancelamento e ajustes

`usp_CancelarCompra` exige administrador ativo, CompraId e motivo. Bloqueia carteira, compra e cartas; usa total/quantidades originais; credita saldo, devolve estoque e grava movimentos e estado CANCELADO na mesma transação. Se já cancelada, retorna a operação existente sem repetir efeitos ou substituir motivo/autor. Não exige segunda chave. Funciona para cliente/carta desativados; não repõe carrinho e não reativa produto.

`usp_DefinirSaldoVersionado` substitui saldo com token obrigatório da linha e registra diferença, autor e motivo. Repetir a definição com mesmo valor e token válido não cria movimento nem UPDATE. Administrador é o único autorizado. A rotina original `usp_DefinirSaldo` permanece para provisionamento com credencial de instalação, mas recebe DENY EXECUTE no papel runtime.

Atendente e administrador podem usar `usp_CadastrarCarta` e `usp_AjustarCartaVersionado` para catálogo, preço e estoque; ajuste absoluto exige versão esperada e gera movimento da diferença. A rotina legada `usp_AjustarCarta` também é negada ao runtime. `AJUSTE_ADMIN` é o nome do tipo operacional de movimento, inclusive quando estoque é ajustado por atendente; não concede acesso a saldo.

SELECT de gestão inclui Versao, convertido em Base64 para tela/API e de volta a byte[8] em parâmetro Binary(8). Compra/estorno atualizam tokens automaticamente sem mudar assinatura. Token desatualizado provoca 51012/VERSAO_DESATUALIZADA/HTTP409 antes de qualquer escrita. A interface exige recarga e nova confirmação; não reenviar automaticamente o valor absoluto com um token recém-lido. As rotinas comparam sob UPDLOCK/HOLDLOCK e atualizam com predicado de versão. Se carta não tiver mudanças em preço/estoque/ativação, não atualiza a linha; motivo segue obrigatório.

AlteracaoCarta preserva autor/motivo e valores antes/depois de todo ajuste versionado efetivo, inclusive só de preço/ativação. A auditoria é gravada na mesma transação; sem mudança não cria linha e motivo antigo não é inventado retroativamente. MovimentacaoEstoque mantém apenas variações reais de quantidade.

`usp_CriarUsuario` provisiona cliente ou atendente por administrador; cria carteira zero e carrinho atomicamente para cliente. `usp_DefinirUsuarioAtivo` preserva administrador fixo. `usp_DefinirItemCarrinho` insere/substitui quantidade ou remove com zero, sem reserva. Todas são transacionais e rejeitam transação externa para evitar desfazer transação do chamador.

Não há triggers nem segundo mecanismo de baixa/devolução. Ordem principal dos bloqueios: carteira, carrinho quando usado, compra existente quando usada, cartas crescentes. A rotina de carrinho usa seu cabeçalho; ajustes usam os mesmos registros operacionais. Aplicação deve tratar deadlocks/falhas técnicas sem trocar a chave de uma compra com resultado desconhecido. Concorrência precisa de teste real antes de aceite.

## Entregas e integração

Scripts numerados em `database/`, sem migrations: preparação com proteção contra base desconhecida; schema; TVP; procedures de compra/estorno; procedures operacionais; seed; papel runtime. `007_verificar.sql` contém consultas somente de leitura para reconciliação. `database/README.md` descreve aplicação e bootstrap. Reexecução não reseta estoque, preços ou usuários existentes.

Massa conciliada: Pikachu 025 NM R$30/2 unidades e SP R$20/3 unidades; Charmander 004 NM R$15,50/1 unidade, coleção MEW-151. Desenvolvimento/testes provisionam CLI-01 R$100 e CLI-02 R$10 usando hash e procedure de saldo. IDs são descobertos, não fixados.

Exceção técnica explícita: entrada inicial pelo seed tem ResponsavelId nulo e motivo SEED_SPRINT1, pois ocorre antes do bootstrap de usuários; CHECK permite essa única origem sem autor humano. Operações normais exigem responsável. Credenciais existentes da demonstração são preservadas; o bootstrap C# usa hash e credencial de preparação, separado do papel runtime. Banco não acessa nem expõe o arquivo protegido de credenciais.

## Estado real e pendências

Sprint 1: a falha ODBC/SSL inicial desta frente foi superada pela execução autorizada de Desenvolvimento, que aplicou 000–006 em PokeVault. Sprint 2: a consulta restrita repetiu o erro, mas a execução autorizada desta frente confirmou `.\SQLEXPRESS`, servidor DESKTOP-641C55N\SQLEXPRESS, motor 17.0.1000.7 Express Edition (64-bit). 008–010 foram aplicados em PokeVault; PokeVaultTests estava ausente e foi criada/marcada, recebeu 000–006 e 008–010 e foi liberada a Testes às 14h49 locais de 21/10/2026. O complemento 013 foi aplicado às 14h54min37s em PokeVault e 14h54min38s em PokeVaultTests; nenhuma publicação adicional será feita por esta frente na massa de Testes nesta entrega. O aviso de que Testes já executava cenários chegou depois dessa aplicação; não afirmar janela exclusiva confirmada. Nenhuma ferramenta instalada ou configuração global de segurança alterada.

007 retornou zero divergências nas duas bases. 011 confirmou rowversions e SETs das novas procedures e verificou SELECT/EXECUTE e recusa de DML/ajustes legados sob EXECUTE AS USER do principal PokeVaultRuntimeVerificacao WITHOUT LOGIN. Comparação das 11 tabelas comerciais antes/depois em PokeVault retornou conteúdo idêntico por contagem/SHA256 ordenado, excluindo novos tokens técnicos. São evidências de preparação/permissões/preservação; não são testes funcionais de conflito, concorrência ou rollback.

A comparação integral posterior ao 013 observou novos cadastros/movimentos e retornou false; a origem dessas escritas não foi atribuída por esta frente. A checagem subsequente dos prefixos preexistentes, limitados às contagens iniciais e ordenados por PK, confirmou os mesmos digests nas 11 tabelas. As duas evidências, positiva e negativa, foram preservadas no relatório, sem reset ou reversão de dados. AlteracaoCarta nasceu vazia, sem carga retroativa de motivos desconhecidos.

Limite runtime: servidor somente Windows, sem login dedicado do projeto identificado. O principal WITHOUT LOGIN é apenas de banco; conexão externa própria da aplicação depende de identidade Windows dedicada/mapeamento pelo operador. Não se mudou autenticação global ou criou conta Windows. O uso real da aplicação com identidade SQL mínima permanece pendente.

Pendências para desenvolvimento/coordenação:

1. Desenvolvimento integra token de versão nos ajustes e trata 51012 com recarga/novo aceite. Preservar TVP, ofertas e fluxo de compra/cancelamento existentes.
2. Operador define identidade Windows dedicada da aplicação e associa ao papel; validar conexão externa sem sysadmin/db_owner. O papel interno já foi verificado nesta rodada.
3. Testes assume a massa de PokeVaultTests, cria suas contas/hashes e executa conflitos, casos funcionais e regressão; Banco não executa cenários mutantes após liberação.
4. API deve extrair autor da sessão, aplicar autorização e revalidar ativação em requisições; SQL não conhece a identidade web sem esse vínculo.
5. Executar testes em PokeVaultTests: ajustes com token antigo, compra/subconjunto, preço alterado, falhas/rollback, idempotência antes/depois de cancelamento, disputa por última unidade/saldo e estorno simultâneo. Esta frente não executou esses cenários mutantes; não presumir aprovação. Falha controlada somente em fixture isolada de Testes, sem backdoor nas procedures e sem transação externa (recusada por contrato).
6. Evoluir schema apenas por novos scripts explícitos e alinhar texto acadêmico ao que realmente for aplicado e testado. A publicação não encerra a sprint.

Comandos, resultados, liberação da base e evidências reais desta rodada estão em `docs/entrega-sprint2-banco.md` e `database/evidencias/sprint2/`. O relatório da sprint 1 permanece histórico. A referência 07/09/2026 não é data de execução destas atividades; seus registros são de 21/10/2026. Não iniciar sprint 3 por este plano.
