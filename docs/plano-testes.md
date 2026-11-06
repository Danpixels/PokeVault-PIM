# PokéVault — plano de testes

Revisão: 21/10/2026, frente de testes e documentação, com apoio de IA. Execução formal da sprint 2 registrada em testes-sprint2.md e entrega-sprint2-testes.md. Fontes: DECISOES.md e diario-sprints.md. Esta revisão substitui as propostas anteriores conflitantes: compra apenas das linhas selecionadas, conjunto selecionado atômico, demais linhas preservadas; conservação SP/SLIGHTLY_PLAYED; catálogo público; saldo definido por substituição.

## Estado e escopo

A matriz abaixo define critérios conceituais. Na preparação da sprint 1, todos estavam não executados; esse registro histórico permanece em testes-sprint1.md. A execução formal posterior, em PokeVaultTests, está em testes-sprint2.md, com mapeamento e limites por requisito. Não interpretar a aprovação de um cenário executável como execução integral de todos os subcasos T01–T36. Um plano, leitura de código, restore ou build não comprova funcionamento. SQL Server real e base exclusiva controlada são obrigatórios para aceite de integridade. Não usar banco em memória como evidência de bloqueios/rollback SQL.

Sprint 1: login/logout, catálogo público, autorização HTTP/antifalsificação, carrinho persistente selecionável, compra, histórico e cancelamento administrativo. Telas completas de funcionários/clientes/saldo e manutenção comercial ficam na sprint 2; massa controlada permite testar o núcleo antes dessas telas. Capacitor é posterior; testes de navegador não comprovam aplicativo. Fora do escopo: reserva, retirada/envio e pagamentos reais; buyback/relatórios permanecem posteriores.

## Massa e invariantes

Massa proposta, não carregada: ADM ativo fixo, ATENDENTE ativo, C1 com R$ 100,00, C2 com R$ 10,00; A-NM/NEAR_MINT por R$ 30,00 com 2 unidades; mesma carta A-SP/SLIGHTLY_PLAYED por R$ 20,00 com 3 unidades; B-NM por R$ 15,50 com 1 unidade. IDs reais virão da preparação controlada, nunca serão presumidos. Senhas não entram em código, relatório ou evidência. Cada caso restaura seu próprio estado, salvo sequência expressamente indicada.

- Compra: saldo diminui pelo total calculado no servidor; estoque diminui pelas quantidades selecionadas; somente linhas compradas saem do carrinho; snapshots, compra e movimentos são persistidos juntos.
- Recusa: não há compra concluída, débito, baixa ou limpeza parcial; valores e linhas devem ser iguais aos anteriores.
- Quantidade selecionada deve coincidir com a quantidade atual da linha. Não é seleção de parte da quantidade de uma linha.
- Preço alterado exige nova confirmação antes de cobrar; não aceitar total/preço arbitrário do cliente.
- Mesmo UUID + mesmo cliente + mesmo conteúdo canônico retorna a compra existente, inclusive após limpeza do carrinho ou cancelamento; não reativar compra cancelada. Mesmo UUID com conteúdo divergente gera conflito.
- Cancelamento integral: crédito pelo total original sobre saldo corrente, reposição das quantidades originais nas conservações corretas e preservação do histórico. Repetição/concorrência não gera segundo estorno, não altera motivo original nem recoloca linhas no carrinho.
- Conta desativada perde acesso; autorização ocorre no servidor. API usa 401/403 conforme autenticação/permissão; mutações por cookie exigem antifalsificação.

## Matriz de casos

Preservados os IDs T01–T25 para referências históricas; detalhes foram conciliados. Todos tiveram estado inicial não executado; consultar o relatório da sprint 2 para evidência efetiva de cobertura. Casos S2 não bloqueiam a entrega das telas S1; regras de autorização do núcleo continuam obrigatórias.

| Caso | Sprint | Preparação/ação | Critério de aceite |
| --- | --- | --- | --- |
| T01 | S1 | Login de cada perfil, senha inválida, logout e reutilização da sessão anterior | Válido autentica no perfil correto; inválido não autentica; logout impede nova operação protegida. Verificar cookie e resposta HTTP sem registrar segredo. |
| T02 | S1 | Anônimo/cliente/atendente tenta saldo e estorno por HTTP direto; testar acesso a carrinho/compra | Anônimo recebe 401 na API protegida; perfil proibido recebe 403. Cliente e atendente nunca definem saldo/estornam; só cliente compra. Nenhum efeito persistido. |
| T03 | S2 | ADM cria atendente e testa acesso | Conta persiste com perfil ATENDENTE sem privilégio administrativo. Campos inválidos/duplicidade recusados conforme contrato real. |
| T04 | S2 | ADM desativa atendente; testar login e sessão existente; tentar desativar ADM fixo | Acesso desativado é recusado em requisições posteriores; histórico preservado e ADM fixo protegido. |
| T05 | S2 | ADM provisiona cliente; tentar cadastro público/elevação de perfil | Cliente ganha carteira zero/carrinho na mesma transação. Sem cadastro público nem perfil imposto por cliente. |
| T06 | S2 | Definir saldo 100 a partir de 40 com autor/motivo; repetir 100 | Saldo 100, delta +60 uma vez; repetição sem mudança não cria movimento financeiro; autor/motivo persistidos. |
| T07 | S2 | Saldo negativo/não numérico/acima de decimal(12,2); cliente/atendente tenta definição | Recusa sem efeito; apenas ADM ativo autorizado. Confirmar validação HTTP e SQL. |
| T08 | S1 | Comprar 1 A-NM com A-SP também disponível | Saldo 70, NM 1, SP 3 a preço 20; mesma carta em conservações independentes, snapshots corretos. |
| T09 | S1 | Catálogo sem sessão; filtros nome/coleção/conservação e paginação; carrinho autenticado | Consulta pública funciona; filtros/páginas coerentes, variantes distinguíveis; alteração de carrinho não reserva nem debita. |
| T10 | S1 | Carrinho NM 1, SP 1, B 1; selecionar somente NM | Total 30, saldo 70, NM 1; SP 3/B 1 intactos. Carrinho conserva SP 1 e B 1 e remove NM. Histórico contém só NM. |
| T11 | S1 | Mesmo carrinho, selecionar NM e SP | Total 50, saldo 50, NM 1/SP 2, B intacto e única linha remanescente. Um pedido com os dois selecionados. |
| T12 | S1 | Execuções separadas: B com saldo 15,50; NM quantidade 2 com saldo 60 | Saldo e estoque selecionado chegam exatamente a zero sem arredondamento/recusa por igualdade. |
| T13 | S1 | Selecionar NM quantidade 3 com estoque 2; repetir com estoque zero | Estoque insuficiente; nenhum efeito em saldo, estoque, compra, movimentos ou carrinho. |
| T14 | S1 | C2 seleciona NM 1 por 30 com saldo 10 | Saldo insuficiente, saldo 10/estoque 2 e carrinho intactos; sem compra. |
| T15 | S1 | Selecionar NM 1 + B 2, com B estoque 1; SP não selecionada | Recusar todos os selecionados; nenhuma baixa/débito; todas as linhas preservadas. |
| T16 | S1 | Seleção vazia, IDs repetidos/ausentes do carrinho, quantidade 0/negativa/fracionária; quantidade 1 quando linha atual é 2; preço/identidade adulterados | Payload inválido é recusado sem efeito. Servidor usa identidade da sessão e exige quantidade corrente; nunca permite compra parcial da quantidade da linha. |
| T17 | S1 | Após T10, ADM cancela com motivo | Saldo 100, NM 2; estado CANCELADO com autor/data/motivo, histórico mantido; SP/B remanescentes não mudam; NM não retorna ao carrinho. |
| T18 | S1 | Cancelar compra de T11 | Devolve 50 e NM 1/SP 1; saldo 100, estoques 2/3; B continua no carrinho. |
| T19 | S1 | Após T17, repetir tela/API com outro motivo; reenviar UUID original da compra | Nenhum novo efeito, compra continua cancelada, motivo original preservado; UUID retorna compra existente sem reativação. |
| T20 | S1 | Duas chamadas sincronizadas de cancelamento da mesma compra concluída | Um crédito de saldo e uma reposição por variante; índices/movimentos sem duplicação, estado cancelado. |
| T21 | S1 | B estoque 1; dois clientes com 100 compram B simultaneamente, UUIDs distintos | Uma compra e um débito 15,50; outro saldo intacto; estoque 0. Registrar sincronização e ambas respostas. |
| T22 | S1 | Cliente saldo 30, NM estoque 2; duas compras simultâneas com UUIDs distintos | Só uma aprovada; saldo 0, estoque 1. Rejeição pode decorrer do carrinho já alterado, sem permitir segundo gasto. |
| T23 | S1 | Induzir falha após primeira escrita de compra; repetir para estorno em base descartável | Rollback total de compra/itens/carrinho/saldo/estoque/movimentos. Sem mecanismo seguro de injeção após escrita, marcar bloqueado; validação prévia não comprova rollback. |
| T24 | S1 | C1 consulta histórico próprio; C2 tenta ID da compra de C1; adulterar clienteId | Sem vazamento de compra alheia; histórico próprio preserva valores e estados. Contrato determinará 403/404 do recurso alheio. |
| T25 | S1 | Comprar NM por 30; preço muda a 40; cancelar | Estorno usa 30 original, não 40, e repõe 1 NM. Histórico mantém preço praticado. Mudança via preparação controlada/procedure autorizada. |
| T26 | S1 | Cookie válido sem token antifalsificação; token inválido/de outra sessão; depois token correto | Mutação sem token válido é recusada sem efeito; token correto permite apenas operação autorizada. Incluir login/logout conforme filtros reais; registrar código real de rejeição. |
| T27 | S1 | Carrinho confirma NM por 30; preço passa a 40 antes do checkout | Conflito de preço, nenhum efeito/limpeza. Exibir 40 e exigir nova ação explícita; após aceite e nova chave, cobrar 40. Não efetuar repetição automática cobrando novo preço. |
| T28 | S1 | Repetir compra aprovada com mesmo UUID e conteúdo; inverter ordem dos itens; repetir após limpeza | Mesma compra/total e apenas um débito/baixa; ordem não altera conteúdo canônico; carrinho remanescente intacto. |
| T29 | S1 | Reusar UUID aprovado mudando preço, quantidade ou conjunto selecionado | Conflito, sem alterar compra original/carrinho/saldo/estoque/movimentos. |
| T30 | S1 | Enviar simultaneamente o mesmo UUID/conteúdo | Uma compra e um conjunto de movimentos; respostas referem mesma compra ou repetição controlada documentada, sem duplicação. |
| T31 | S1 | Linha B indisponível ou preço alterado, mas não selecionada; comprar só NM | Compra NM por 30 aprovada; B preservada no carrinho e não validada como selecionada; sem baixa de B. |
| T32 | S1 | C1 e C2 usam o mesmo UUID para conteúdos válidos próprios | Escopo da chave é cliente; resultados independentes sem expor compra alheia; se houver estoque/saldo, ambas aprovam. |
| T33 | S1 | Recusa por saldo com UUID novo; corrigir saldo por rotina controlada; repetir mesmo conteúdo/chave | Ausência de compra após rollback permite nova tentativa; uma aprovação e um conjunto de movimentos ao final. |
| T34 | S1 | ADM cancela sem motivo/com espaços; compra inexistente | Recusa compreensível conforme contrato; sem crédito/reposição/movimento; motivo obrigatório. |
| T35 | S1 | Após compra de 30, definir saldo corrente para 40; cancelar | Saldo final 70, crédito 30; não sobrescrever saldo para 100. Guardar movimento de ajuste e estorno distintos. |
| T36 | S2 | Atendente cadastra carta/ajusta preço e estoque com motivo; tenta saldo/estorno | Manutenção comercial permitida e auditada; saldo/estorno proibidos. Regressão T02 já exige negação no núcleo S1. |

## Ordem, evidências e aceite

Executar primeiro T01/T02/T09/T26, depois fluxo T08/T10/T11/T17/T18/T24, recusas/UUID/preço e, por fim, concorrência e rollback. Verificar a interface e os endpoints reais, sem inventar URLs/DTOs. Contrato SQL fonte: database/ e docs/contrato-sprint1.md quando publicados; planos antigos não substituem esse contrato.

Cada execução registra versão/hash, data real e fuso America/Sao_Paulo, executor real, base/instância, pré-condições, IDs/UUIDs, requisições/respostas sanitizadas e consultas antes/depois, esperado versus observado, status e defeito/reteste. Concorrência exige duas conexões e barreira de início; cliques sequenciais não são prova. Preservar reprovações e não substituir evidência por resultado esperado.

Evidências futuras: docs/evidencias/<rodada>/Txx-<etapa>.<ext>. Nenhum arquivo antecipado deve simular resultado. Prints mostram interface; HTTP comprova resposta/permissão; SQL comprova persistência/movimentos. Redigir limites de cada um, sem publicar senhas/tokens/conexões sensíveis.

Aceite S1 exige fluxo local reproduzível, integridade e autorização comprovadas sem defeito bloqueador aberto. Casos bloqueados não são aprovados. Capacitor, telas S2 e funcionalidades posteriores permanecem explicitamente pendentes. Preparação/roteiro: testes-sprint1.md; rastreabilidade: rastreabilidade-academica.md; resumo ao diário: entrega-sprint1-testes.md.


## Adendo de execução — sprint 2 em 21/10/2026

45 cenários executáveis distintos têm último resultado aprovado: 21 SQL, 18 HTTP e 6 MVC. A rodada 04 teve 44 aprovações e uma reprovação de expectativa do roteiro HTTP18; reteste isolado na rodada 05 aprovou a recusa 400 de campos extras de perfil, com ausência de cadastro. Rodadas 01–05 e manifestos foram preservados em docs/evidencias/sprint2/. Não contabilizar os 30+7 checks históricos de Desenvolvimento como casos desta frente.

O contrato-sprint2.md adiciona rowversion de8 bytes/Base64, procedures obrigatoriamente versionadas e51012/VERSAO_DESATUALIZADA; o script013 adiciona AlteracaoCarta para auditoria inclusive de alterações só de preço/ativação. CasosSQL13–SQL21, HTTP03–HTTP09/HTTP12/HTTP14–HTTP18 e MVC01–MVC06 complementam a matriz, cobrindo gestão, conflitos, auditoria e validação. Regressão do núcleo usaSQL01–SQL12 eHTTP10–HTTP11.

Detalhes e correspondências estão em testes-sprint2.md; não inferir cobertura completa dos36 casos conceituais. Exemplos ainda sem execução integral por esta frente: todos os filtros/páginas deT09, limites exatos deT12, linha não selecionada inviável deT31, mesmoUUID entre clientes deT32, repetição após recusa deT33 e todos os erros de motivo/inexistência deT34. A lista não substitui a leitura dos asserts. Critérios de concorrência/rollback agora possuem execução SQL real delimitada; não há ensaio de carga nem prova estatística de ausência de falhas.

SQL Server real/base exclusiva, schema1–4; último estado reconciliado sem divergências e sem triggers temporárias. Identidade SQL runtime externa de menor privilégio permanece pendente; execução com credencial de preparação não a comprova. MVC foi verificado por HTML/HTTP e submissão de formulários, sem inspeção visual ampla. Não há resultado de Capacitor ou início de sprint3 nesta frente. Texto acadêmico final/PDF não foi reescrito.

