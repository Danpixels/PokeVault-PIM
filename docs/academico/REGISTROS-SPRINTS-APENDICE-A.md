# A.8 REGISTROS DOCUMENTAIS DAS SPRINTS E DOS INCREMENTOS

Esta seção reúne registros efetivamente produzidos durante o desenvolvimento, preservados em docs/diario-sprints.md. São transcrições selecionadas do diário técnico solicitado por Daniel, com apoio de IA, e não atas de reuniões presenciais com participantes ou assinaturas presumidos. O documento integral acompanha o repositório; os relatórios de cada frente complementam esses registros.

O pedido original foi localizado no chat “PokéVault — Coordenação”, no turno de 21/10/2026 que solicitou um diário único para cada sprint, com acordos, tarefas por departamento e problemas. O início de referência de 07/09/2026 foi informado naquele pedido; as atividades documentadas abaixo mantêm suas datas reais.

Forma de leitura: ID e data identificam a entrada original; “Registro” e “Resultado/limite” transcrevem as respectivas células do diário. A seleção não altera o estado histórico das entregas, e os limites de uma versão podem ter sido superados por incrementos posteriores.

## A.8.1 Sprint técnica 1 — abertura, acompanhamento e encerramento

Objetivo registrado: demonstrar o fluxo de autenticação, catálogo público, compra selecionada com saldo fictício, histórico e cancelamento integral administrativo. Frentes: Coordenação, Banco de dados, Desenvolvimento e Testes/documentação. O ciclo foi encerrado com o fluxo básico funcional e aceite completo ainda parcial.

### R01 — 21/10/2026

Registro: Leitura e conciliação dos quatro planos pela coordenação com apoio de IA

Resultado/limite: Documentos de planejamento disponíveis; escolhas técnicas e divergências registradas; nenhum teste funcional alegado

### R02 — 21/10/2026

Registro: Daniel responde às dúvidas de funcionamento

Resultado/limite: Catálogo público, atribuições do atendente, saldo por substituição, checkout selecionado e reconfirmação de preço registrados

### R06 — 21/10/2026

Registro: Contrato SQL publicado por Banco, lido pela coordenação e encaminhado a Desenvolvimento e Testes

Resultado/limite: docs/contrato-sprint1.md contém TVP, procedures, resultados, erros e regras de seleção/idempotência; execução SQL ainda não comprovada neste acompanhamento

### R13 — 21/10/2026, acompanhamento iniciado às 14h23 (São Paulo)

Registro: Entrega inicial de Banco concluída e relatório lido

Resultado/limite: docs/entrega-sprint1-banco.md e database/000–007 disponíveis: 11 tabelas de negócio + SchemaVersao, TVP, oito procedures, seed conciliado e instruções/permissões. Banco fez inspeções estáticas, mas não aplicou os scripts nem criou base: suas duas consultas falharam em ODBC/SSL. Não confundir entrega dos scripts com aceite de integridade

### R17 — 21/10/2026

Registro: Entrega final de Desenvolvimento recebida; coordenação leu relatório e JSONs de verificação

Resultado/limite: docs/entrega-sprint1-desenvolvimento.md registra scripts 000–006 aplicados em PokeVault, motor SQLEXPRESS 17.0.1000.7; reconciliação 007 com saída 0 e zero divergências. .local/smoke-desenvolvimento-resultado.json contém 30 verificações positivas e .local/smoke-mvc-resultado.json contém sete; datas UTC 17:23:59 e 17:23:33. São checks de desenvolvimento, não os 30 casos formais da matriz. Relatório esclarece que a saída agregada 1 de R15 veio do roteiro e a reconciliação foi executada separadamente com sucesso

### R18 — 21/10/2026

Registro: Daniel informa o fim da primeira sprint; coordenação registra balanço do ciclo

Resultado/limite: Fluxo básico funcional entregue; aceite completo não comprovado, com pendências explícitas abaixo. Os três chats terminaram suas entregas iniciais; automação de acompanhamento consultada com status PAUSED. Nenhuma sprint seguinte iniciada

Fonte: docs/diario-sprints.md, entradas identificadas acima; registros originais de 2026.

## A.8.2 Sprint técnica 2 — administração, testes e pendências

Meta registrada: permitir administrar contas, saldo e catálogo pelas telas, mantendo autorização, integridade e auditoria, e incorporar as verificações críticas pendentes de S1. A permissão SQL interna foi verificada; a conexão externa por identidade dedicada permaneceu como ressalva histórica.

### S2-R01 — 21/10/2026

Registro: Daniel solicita “Então vamos pra Sprint 2! Comece”; coordenação confere decisões, fechamento da sprint 1 e código existente

Resultado/limite: Sprint autorizada; capítulos acadêmicos aguardam sistema final. Implementação e testes desta sprint ainda não executados no momento da abertura

### S2-R05 — 21/10/2026, acompanhamento às 14h54 (São Paulo)

Registro: Entrega parcial de Banco e contrato aditivo lidos

Resultado/limite: Contrato usa rowversion em Carta/SaldoCliente, Base64 de oito bytes e procedures Versionado com conflito 51012/409; scripts 008–010 aplicados em PokeVault e PokeVaultTests. Relatório registra liberação da base de testes às 14h49 e evidências em database/evidencias/sprint2/; comparação antes/depois preservou o conteúdo das 11 tabelas comerciais da demonstração

### S2-R06 — 21/10/2026

Registro: Privilégios internos verificados por Banco; limite de conexão externa registrado

Resultado/limite: Principal WITHOUT LOGIN exclusivo do banco comprovou SELECT/EXECUTE e recusas 229 de DML e de ajustes legados sem versão. Isso não comprova conexão runtime independente: servidor somente Windows e identidade dedicada externa ainda não provisionada. Nenhuma mudança global de segurança foi feita ou autorizada

### S2-R08 — 21/10/2026

Registro: Lacuna de auditoria identificada por Banco e correção encaminhada

Resultado/limite: Ajuste somente de preço/ativação exigia motivo sem persistir todo o registro. Script aditivo 013_sprint2_auditoria_carta.sql propõe tabela AlteracaoCarta com autor/motivo/valores anteriores e novos, sem movimento estoque zero e sem mudar assinaturas. Coordenação avisou as três frentes e pediu janela livre de cenários mutantes antes de aplicar em PokeVaultTests, caso ainda não aplicado; Testes deve conferir versão e adicionar caso/regressão de auditoria. Aplicação do 013 ainda não confirmada neste acompanhamento

### S2-R10 — 21/10/2026

Registro: Resultados finais e versão do produto confrontados pela coordenação

Resultado/limite: Produto com build 0 avisos/erros; Desenvolvimento registra 54 checks integrados e 22 MVC positivos. Testes consolida 45 cenários distintos com último estado aprovado (21 SQL, 18 HTTP, 6 MVC). Rodada04 teve 44 aprovações e uma expectativa de roteiro inadequada; reteste HTTP18 passou exigindo 400 e nenhuma conta criada. Não há rodada única 45/45 nem soma desses cenários com checks de Desenvolvimento

### S2-R12 — 21/10/2026

Registro: Balanço final e encerramento do acompanhamento da rodada

Resultado/limite: Administração funcional, regressão e cenários críticos comprovados nos limites dos relatórios. Conta Windows externa runtime de menor privilégio continua pendente; permissões de banco comprovadas por WITHOUT LOGIN não substituem essa implantação. Acompanhamento deve ser pausado após consolidar este balanço; sprint3 não iniciada

### S2-R13 — 21/10/2026

Registro: Automação de acompanhamento pausada após consolidação

Resultado/limite: Ferramenta confirmou status PAUSED da automação existente. Três entregas concluídas, evidências conferidas e ressalvas registradas; não haverá início automático de outra sprint

Fonte: docs/diario-sprints.md, entradas identificadas acima; registros originais de 2026.

## A.8.3 Incremento TCGdex 151 — execução e retorno manual

O diário registra este trabalho em 03/11/2026 como integração específica da coleção sv03.5, distinta das sprints coletivas de setembro. Entradas com horário UTC foram mantidas como no documento de origem; 22h UTC corresponde a 19h em São Paulo.

### TCG-R01 — 03/11/2026

Registro: Link/documentacao/exemplos recebidos e autorizacao para tentar apenas151; coordenacao consultou fonteoficial e endpoints reais e registrou recorte. Integracao ainda nao implementada nesta abertura

### TCG-R06 — 03/11/2026, acompanhamento22:47UTC

Registro: Importacao real da demonstracao concluiu22:46:20UTC: importacao-resultado.json registra207processados/importados,207imagenslocais,0ausentes e0fallbackingles. ConsultaSQL registra207referenciasdetalhadaspt/importacaoCONCLUIDAsem pendentes e3vinculos. imagens-cache.json registra207arquivos(4017294bytes). preservacao-importacao.json compara12tabelascomerciais comtodascolunas/rowversions/hash/contagens e registraiguais=true. Fonte das evidencias: src/evidencias/tcgdex151/. Confirmadoimportreal,nãoaceitefinaldeoffline/repetição/regressão; Desenvolvimentoverificaessescriterios eTestes segueativoemPokeVaultTests

### TCG-R07 — 03/11/2026, acompanhamento22:58UTC

Registro: Versao finalD226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C liberada schema10/snapshot. Devcorrigiu validacaorespostaAPI antesde substituircache (165/207/IDs/precos/URLs) e rotulohistorico compara datas exatamente. Testes preservouidentity01 com1reprovacao de rotulohistorico naA68 e reteste, depoisexecutou finalD226. Coordenacao leu live-final23aprovados, offline-final2aprovados e regressao-final45aprovados; sao conjuntos separados,naosomar repeticoes. Relatoriosfinais/reconciliacao em consolidacao,semnovo bloqueio paraDaniel

### TCG-R09 — 03/11/2026, acompanhamento23:08UTC

Registro: Coordenacao leu os tres relatorios finais e snapshots individuais confirmaram chats concluidos. Na DLLD226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C: live-final23aprovados, offline-final2aprovados, regressao-final45aprovados; hashes e versao do produto conferidos. Nenhum resultadoA68 atribuido como aceite daD226

### TCG-R12 — 03/11/2026

Registro: Balanco consolidado e automacao de acompanhamento removida apos concluir a entrega; ferramenta confirmoudeleteStatus=deleted. Nao ha nova rodada/mobile iniciada nem verificacoesperiodicas pendentes desta integracao

### TCG-R13 — 03/11/2026

Registro: Daniel relata manualmente: “Consegui comprar a carta!”, apos orientacao para cadastrar oferta da bibliotecaTCGdex e comprar como cliente. Compra bem-sucedida relatada pelo usuario; carta/CompraId/quantidade/valores e capturas nao informados, sem atribuir numeros do exemplo como observados ou alegar conferencia independente de saldo/estoque/estorno

### TCG-R14 — 03/11/2026

Registro: Apos orientacao de cancelamento administrativo e conferencia de saldo/estoque/historico, Daniel responde “Perfeito! Funcionou! E agora?”. Registrar sucesso do cancelamento conforme relato manual, sem inferir IDs/valores ou nova verificacao independente. Proxima conversa proposta: definir fluxo de compras/UX e revisar backlog das melhorias adiadas; nenhum novo desenvolvimento/mobile autorizado apenas por esta pergunta

Fonte: docs/diario-sprints.md, entradas identificadas acima; registros originais de 2026.

## A.8.4 Toques finais da web e atualização acadêmica

Após a integração, o diário conserva as entradas WEB151-01 a WEB151-06 e ACA-R01. A seguir apresenta-se uma síntese editorial dessas entradas, identificada como síntese, sem atribuir a elas novas sprints ou reuniões.

WEB151-01 e WEB151-02 — 03/11/2026: gestão unificada da coleção 151, variantes por idioma, conservação e acabamento, preço em reais definido localmente, carrinho cumulativo e snapshots de compra; script 024/schema 11, com preservação dos registros anteriores.

WEB151-03 e WEB151-04 — 03/11/2026: conflito inicial de DLL em uso e expectativa de teste ainda voltada à tela antiga; correção do roteiro, regressão final 45/45 e 10/10 do fluxo no produto 1F34…; demonstração local disponibilizada com limites de verificação HTML/HTTP. Essa é uma versão anterior a FDCE e 64F1.

WEB151-05 — 03/11/2026, aproximadamente 21h19: retorno positivo de Daniel e solicitação de olho de senha, controles recolhidos, média simplificada e 207 cartas em página única. O diário registra implementação e 10/10 cenários de fluxo na pasta acabamento-visual151, correspondente a E4. A inspeção visual manual continuou separada dos checks HTML/HTTP.

WEB151-06 — 03/11/2026, aproximadamente 21h32: cadastro público e alteração da própria senha nos três perfis; migração 026/schema 12, 12 tabelas comerciais preservadas, 10/10 contas e 45/45 regressão no binário 64F1. As credenciais da demonstração não foram alteradas automaticamente.

ACA-R01 — 04/11/2026: início autorizado da reescrita acadêmica, com primeira cópia editável separada, diagramas e referências vinculadas ao sistema real. O registro mantém como pendentes as contribuições individuais não fornecidas, capturas finais, conferência do manual e mobile. Trata-se de revisão documental, sem novo teste funcional.

Fonte: docs/diario-sprints.md, seções “Toques finais da web — fluxo e Gestão de Catálogo151” e “Reescrita acadêmica iniciada em04/11/2026”; docs/entrega-fluxo151.md e docs/entrega-contas.md.

## A.8.5 Arquivos completos e relação com o corpo do PIM

Diário integral: docs/diario-sprints.md. Planejamento e decisões: docs/planejamento.md e docs/DECISOES.md. Entregas por frente: docs/entrega-sprint1-{banco,desenvolvimento,testes}.md e docs/entrega-sprint2-{banco,desenvolvimento,testes}.md. Os contratos e resultados referidos nesses documentos permanecem nos caminhos apresentados em A.1 e A.6.

Uso no PIM: seções 5.1–5.5 para acordos, distribuição e acompanhamento; 5.6–5.8 para problemas, correções e critérios de aceite. O planejamento mobile das sprints propostas 3 e 4 não constitui evidência de execução. Diário integral no commit de referência: https://github.com/Danpixels/PokeVault-PIM/blob/30bf512/docs/diario-sprints.md. As atas da equipe e exportações do Trello, quando existentes fora deste acervo, continuam sendo materiais complementares a conferir.


