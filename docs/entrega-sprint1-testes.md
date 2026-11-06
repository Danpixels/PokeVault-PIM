# Entrega sprint 1 — testes e documentação

Registro real: **21/10/2026**, America/Sao_Paulo. Frente executada por Codex com apoio de IA. Insumo para a coordenação consolidar em docs/diario-sprints.md; esta frente não editou o diário nem enviou mensagens a outros chats. Início de referência 07/09/2026 não foi usado para retroagir atividades.

## Resultado entregue e limites

**Preparação documental e inspeção estática entregues. Execução funcional bloqueada: 0 casos executados, 0 aprovados, 0 reprovados.** Trinta casos S1 bloqueados (T01/T02/T08–T35); seis S2 planejados e não executados (T03–T07/T36). Nenhuma aprovação de integridade SQL, HTTP, interface, autenticação ou mobile foi declarada. Não houve build/restore nem conexão SQL nesta frente.

| Artefato exclusivo desta frente | Entrega real |
| --- | --- |
| docs/plano-testes.md | Matriz conciliada com seleção atômica, carrinho remanescente, quantidade corrente, SP, catálogo público, saldo por substituição, UUID, preço com nova confirmação, antifalsificação e concorrência |
| docs/testes-sprint1.md | Dependências, contrato SQL lido, nomes de procedures/TVP/erros, roteiro por valores, preparação segura e modelo de execução |
| docs/rastreabilidade-academica.md | Requisito → artefato real ou pendente → caso → evidência → seções 3/4/5/6; alegações a revisar e parágrafos factuais utilizáveis |
| tests/Inspecionar-Prontidao.ps1 | Script somente de leitura executado para inventariar existência e hashes, sem avaliar funcionamento |
| tests/Conferir-Estado.sql | Consultas antes/depois preparadas; não executadas e ainda sem validação contra DDL real; guarda exige PokeVaultTests e ID controlado |
| tests/README.md | Limites e uso da preparação; sem infraestrutura de teste que force alterações de outras frentes |
| docs/evidencias/sprint1-preparacao-inventario.json | Única evidência observada: inventário estático, UTC 2026-10-21T17:09:05Z, equivalente a 14:09:05 em São Paulo |

## Constatações, dependências e encaminhamentos

- Na inspeção estática havia contrato SQL e projeto MVC; nenhum script SQL foi encontrado. O Program.cs lido ainda era o inicial MVC. Arquivos estão sendo produzidos por outras frentes; essas constatações valem para o instante observado e não são defeitos funcionais.
- Contrato-sprint1.md foi lido. Ele publica usp_ConcluirCompra, usp_CancelarCompra, rotinas de saldo/carrinho/catálogo/usuários, TVP e erros 51000–51011, mas declara que sua publicação antecede os scripts. Implementação/execução não foram presumidas.
- Massa esperada: Pikachu NM30/2, SP20/3; Charmander NM15,50/1; CLI-01=100, CLI-02=10. A descrição inicial divergente do seed no contrato está sendo conciliada por Banco conforme orientação recebida. Não foi registrada como defeito funcional.
- B01: aguardam-se DDL/scripts e base PokeVaultTests exclusiva identificada/controlada, preparação/credenciais por mecanismo seguro e evidência da instância. Não houve reset de base desconhecida.
- B02: autenticação/API/antifalsificação seguem em implementação; aguardam-se versão executável, URL e contratos HTTP reais. Não criar endpoints ou DTOs fictícios em testes.
- B03: provisionamento de usuários/saldos e IDs reais ainda precisa ser conferido. Testes não gravam hashes nem duplicam bootstrap C#.
- B04: rollback após escrita exige método de falha controlada seguro. Sem ele, manter T23 bloqueado mesmo que recusas por validação funcionem.

A próxima execução deve conferir as entregas recentes e os hashes, validar a base exclusiva, preparar massa controlada e começar por T01/T02/T09/T26. Em seguida, compra selecionada/carrinho remanescente/histórico, recusas/preço/UUID e concorrência/estorno/rollback. Só registrar aprovação após observar as respostas e a persistência real. Nenhum defeito funcional foi confirmado nesta rodada porque nenhum cenário funcional foi executado.

## Insumo factual para diário e escrito

Em 21/10/2026, a frente de testes/documentação revisou critérios conforme as decisões conciliadas e o contrato SQL publicado, preparou consultas de leitura, roteiro de demonstração e matriz de rastreabilidade acadêmica. Executou um inventário estático de arquivos com hashes, sem conectar ao SQL Server ou testar a aplicação. A execução integrada permaneceu bloqueada pelas dependências de ambiente, aplicação e massa. O trabalho escrito deve manter procedimentos/tecnologias como planejados até haver evidências; não afirmar triggers financeiras, buyback, reserva de três dias, nuvem, CI/CD, HandTalk, integração Capacitor ou cumprimento integral de requisitos sem comprovação. O diário central continua sob responsabilidade da coordenação.

## Atualização final da preparação — mesma data

A produção paralela avançou: nova inspeção registrou oito scripts SQL em database/ e foram lidos os controllers LojaApiController/LojaController e FluxoModels. A evidência adicional é docs/evidencias/sprint1-preparacao-inventario-02.json. O seed 005_seed.sql já contém os valores acordados, resolvendo textualmente a divergência transitória. Os nomes de tabelas/colunas das consultas de conferência foram comparados ao DDL entregue, sem execução SQL. Rotas HTTP reais foram acrescentadas ao roteiro, ainda sem chamadas executadas.

A constatação inicial de ausência de scripts é histórica e foi superada; o bloqueio atual é disponibilidade comprovada da base exclusiva/massa e aplicação integrada/configurada. Program.cs ainda estava inicial na releitura e não havia entrega de banco/desenvolvimento disponível na listagem final consultada. Não iniciar aplicativo em produção parcial para fabricar resultado funcional. Contagens de casos conferidas no arquivo: 36 total, 30 S1 e 6 S2. Nenhum caso funcional executado/aprovado/reprovado.

