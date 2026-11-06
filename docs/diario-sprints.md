# PokéVault — diário de sprints

Documento único para acompanhar acordos, atividades por frente, problemas, decisões e evidências ao longo de todas as sprints. Mantido pela coordenação com apoio de IA, para subsidiar o trabalho acadêmico em desenvolvimento. Não atribui atividades a integrantes humanos sem registro real.

## Convenção de datas e estados

Daniel informou **07/09/2026** como início de referência do projeto/sprint 1. Essa referência não comprova quais atividades ocorreram naquele dia. Este diário foi aberto em **21/10/2026**, data da conciliação e autorização de implementação. Não foram fornecidos registros de execução entre essas datas; não há reuniões, testes ou entregas retroativos a relatar.

Cada entrada usa a data efetiva de registro e separa proposta, instrução enviada, artefato entregue e resultado verificado. Uma entrega só vai para Concluído com evidência adequada. Datas de término e duração não são inventadas; a sprint encerra quando seu aceite for comprovado ou houver replanejamento explícito.

## Sprint 1 — login, catálogo, compra e estorno

**Início de referência:** 07/09/2026, informado por Daniel. **Mobilização registrada:** 21/10/2026. **Encerramento do ciclo:** registrado em 21/10/2026 após Daniel informar que a primeira sprint acabou. **Balanço:** incremento básico funcional entregue, objetivo parcialmente atendido quanto ao aceite completo; validações formais pendentes. A abertura e os acompanhamentos históricos abaixo foram preservados.

### Objetivo e acordos

Demonstrar localmente o cliente autenticando, consultando catálogo público, escolhendo itens do carrinho para comprar com saldo fictício e consultando histórico; administrador cancela integralmente a compra, com devolução única de saldo e estoque. Outros itens do carrinho permanecem disponíveis para compra futura.

Fontes lidas: `DECISOES.md`, `planejamento.md`, `plano-banco.md`, `plano-desenvolvimento.md`, `plano-testes.md` e referência acadêmica extraída em `../tmp/pdfs/pokevault.txt`. O PDF é referência, não evidência de implementação.

| Assunto | Conciliação / acordo | Origem |
| --- | --- | --- |
| Catálogo | Público, inclusive API de consulta; compra só com login de cliente | Resposta de Daniel nesta rodada |
| Atendente | Cadastra cartas e ajusta preço/estoque; não altera saldo nem estorna | Resposta de Daniel nesta rodada |
| Saldo | Definir substitui valor corrente, com movimento da diferença e autor/motivo | Resposta de Daniel + rastreabilidade técnica |
| Seleção no carrinho | Cliente seleciona linhas; compra atômica do conjunto selecionado; não selecionados ficam no carrinho | Resposta de Daniel; substitui proposta de carrinho inteiro |
| Mudança de preço | Conflito exige nova confirmação | Resposta de Daniel nesta rodada |
| Modelo de carta | Uma tabela comercial Carta com conservação, preço e estoque; ofertaId da API mapeia CartaId | Escolha técnica da coordenação entre planos de banco e desenvolvimento |
| Escritas críticas | Procedures SQL únicas para débito/baixa/estorno/ajustes; sem duplicação em C# ou triggers | Escolha técnica da coordenação |
| Evolução do schema | Scripts numerados em database/, banco responsável; sem migrations paralelas | Escolha técnica da coordenação |
| Web e API | Monólito modular .NET 10, serviços compartilhados, SqlClient para persistência; cookie e antifalsificação nas mutações por cookie | Escolha técnica baseada no plano de desenvolvimento |
| Conservação | Domínio de seis códigos do plano de banco; exemplo LP nos testes passa a SP, preservando massa numérica | Conciliação de nomenclatura |
| Funcionários e histórico | Desativação, bloqueio de sessões existentes e preservação de vínculos | Escolha permitida no escopo aprovado |
| Credenciais | Preparar provisionamento seguro com hash; credencial inicial gerada localmente, sem inserir segredo no diário | Escolha de configuração técnica |
| Escopo administrativo | Cancelamento e consulta necessária entram agora; telas completas de clientes/saldo/funcionários e manutenção comercial ficam na sprint 2; massa controlada sustenta sprint 1 | Recorte do planejamento de coordenação |

Os contratos detalhados de SQL, parâmetros e retornos serão publicados por Banco em `docs/contrato-sprint1.md` antes da integração. A autorização e os limites ficam no servidor, inclusive para chamadas diretas à API. Nenhum recurso de logística, reserva ou pagamento real será criado.

### Trabalho por departamento / frente

| Frente | Tarefas definidas | Artefatos e condição de aceite | Dependências |
| --- | --- | --- | --- |
| Coordenação | Conciliar planos, registrar respostas, distribuir escopo exclusivo, acompanhar resultados e manter este diário | Decisões centrais e diário atualizados; quadro local com fatos/evidências | Relatos e artefatos das demais frentes |
| Banco de dados | Verificar acesso SQL somente de leitura primeiro; escolher primeira instância acessível; criar base exclusiva do projeto se ausente; scripts DDL, massa e procedures de compra selecionada/estorno/ajustes/carrinho | database/ e contrato-sprint1.md; instruções reproduzíveis; execução SQL documentada se possível; não alterar bases desconhecidas | Schema decidido; parâmetros publicados antes da integração |
| Desenvolvimento | Criar solução MVC/API e camadas, configuração segura, login e catálogo; carrinho selecionável; compra e histórico; cancelamento administrativo; consumir procedures | src/, solução, instruções locais; restore/build reais; fluxo conectado ao SQL após contrato disponível | Pode iniciar estrutura/login/catálogo; integra escritas após scripts/contrato de banco |
| Testes e documentação | Conciliar matriz com regras confirmadas, preparar roteiro/consultas/automação e rastreabilidade acadêmica; executar somente quando houver versão e base de teste próprias | tests/ e docs de teste/evidências; resultados esperados separados de observados; matriz requisito → implementação → caso → evidência → seção acadêmica | Preparação independente; execução funcional depende de aplicação e SQL |

**Divisão de arquivos:** coordenação escreve DECISOES.md, planejamento.md e este diário; banco escreve database/, plano-banco.md e contrato-sprint1.md; desenvolvimento escreve src/, solução, configuração e README; testes escreve tests/, plano-testes.md e documentação de testes/evidências. Cada frente registra seu resumo real em `docs/entrega-sprint1-<banco|desenvolvimento|testes>.md`; coordenação consolida no diário. Não sobrescrever arquivos de outra frente.

### Quadro local da abertura

Este quadro documenta a organização; não representa criação de Trello externo.

| Cartão | Frente | Estado na abertura | Evidência / condição de avanço |
| --- | --- | --- | --- |
| PV-01 — conciliar planos e funcionamento | Coordenação | Concluído quanto a decisões e recorte | Quatro planos lidos; respostas de Daniel registradas em DECISOES.md; contrato SQL detalhado ainda será entregue por Banco |
| S1-B01 — schema, contrato e massa | Banco | Em desenvolvimento | contrato-sprint1.md publicado e lido; schema/massa ainda em preparação no primeiro acompanhamento |
| S1-B02 — procedures e integridade | Banco | Em desenvolvimento | 003_procedures_compra.sql presente; execução e integridade ainda não verificadas |
| S1-D01 — estrutura, configuração e login | Desenvolvimento | Em desenvolvimento | Estrutura MVC presente em src/; build/restore e autenticação ainda precisam de evidência |
| S1-D02 — catálogo, carrinho, compra e histórico | Desenvolvimento | Em desenvolvimento | Desenvolvimento relata integração ao contrato/TVP; funcionamento ainda não verificado |
| S1-D03 — cancelamento administrativo | Desenvolvimento | A fazer | Depende de compra e procedure de estorno |
| S1-T01 — matriz, rastreabilidade e preparação | Testes/documentação | Concluído quanto à preparação documental | entrega-sprint1-testes.md, testes-sprint1.md e rastreabilidade-academica.md lidos; inspeção estática registrada. Não significa aprovação funcional |
| S1-T02 — execução integrada e defeitos | Testes/documentação | A fazer | Depende de versão executável e SQL Server de teste |

### Problemas, riscos e encaminhamentos

| ID | Constatação | Encaminhamento | Situação inicial |
| --- | --- | --- | --- |
| I01 | Planos divergem entre Carta única e Carta/Oferta separadas | Modelo simples de Banco adotado; mapear ofertaId para CartaId | Resolvido no planejamento, implementação a conferir |
| I02 | Planos divergem sobre quem atualiza saldo/estoque e como versionar schema | Procedures e scripts sob responsabilidade de Banco; aplicação não duplica efeitos | Resolvido no planejamento, integração a conferir |
| I03 | Checkout integral proposto conflita com seleção pedida por Daniel | DTO e procedures recebem conjunto selecionado; limpar só linhas compradas; adaptar testes | Regra decidida; execução pendente |
| I04 | Testes usam LP e banco propõe SLIGHTLY_PLAYED | Unificar SP/SLIGHTLY_PLAYED nos dados e cenários | Correção encaminhada |
| I05 | SQL Server tem instâncias listadas no plano de desenvolvimento, mas conexão ainda não comprovada | Banco verifica versão/acesso e usa base exclusiva, sem reset destrutivo | A verificar, não é falha de conexão comprovada |
| I06 | Restore de pacotes, build e fluxo funcional não executados nos planos | Desenvolvimento registra comandos e erros reais; testes não presumem aprovação | A verificar |
| I07 | PDF contém alegações de implementação e práticas ainda sem evidência | Preparar matriz de revisão; atualizar escrito conforme entrega real e evidências | Em acompanhamento acadêmico |
| I08 | Data de referência é anterior aos registros disponíveis | Distinguir cronologia informada e execução comprovada; não fabricar reuniões/testes | Tratamento documental definido |

### Verificação e critério de encerramento

Verificar login/logout, catálogo público, isolamento de cliente, conservações independentes, seleção/subconjunto e preservação dos itens restantes, compra multi-item, preço alterado, saldo/estoque insuficientes, reenvio de chave, concorrência pela última unidade e saldo, rollback e cancelamento repetido/concorrente pelo valor original. Testes de transação exigem SQL Server real. Cadastro administrativo completo pode permanecer na sprint 2; dados controlados devem permitir a demonstração mínima desta sprint.

Para encerrar: aplicação local reproduzível e identificada, compra/estorno demonstrados, nenhuma falha bloqueadora de autorização/integridade aberta, evidências vinculadas, limitações registradas e texto/diagrama pertinente atualizado. Build aprovado sozinho não comprova funcionamento nem encerra a sprint.

### Diário de registros efetivos

| Registro | Data real | Atividade / decisão | Resultado comprovado e limites |
| --- | --- | --- | --- |
| R01 | 21/10/2026 | Leitura e conciliação dos quatro planos pela coordenação com apoio de IA | Documentos de planejamento disponíveis; escolhas técnicas e divergências registradas; nenhum teste funcional alegado |
| R02 | 21/10/2026 | Daniel responde às dúvidas de funcionamento | Catálogo público, atribuições do atendente, saldo por substituição, checkout selecionado e reconfirmação de preço registrados |
| R03 | 21/10/2026 | Abertura deste diário único e recorte da sprint 1 | Início de referência informado como 07/09/2026; datas de atividades anteriores não comprovadas; implementação encaminhada em seguida |
| R04 | 21/10/2026 | Instruções enviadas aos chats PokéVault — Banco de dados, PokéVault — Desenvolvimento e PokéVault — Testes e documentação | Ferramenta confirmou envio aos três chats existentes; trabalho dividido por arquivos; execução/entrega ainda não comprovadas no momento do envio |
| R05 | 21/10/2026 | Primeiro acompanhamento das três frentes | Chats ativos: Banco iniciou inspeção e contrato; Desenvolvimento relata estrutura MVC criada e bloqueio do cache/configuração NuGet do perfil, trabalhando com configuração no workspace; Testes prepara matriz e relata ausência inicial de contrato/base. São relatos de progresso, não aceite funcional |
| R06 | 21/10/2026 | Contrato SQL publicado por Banco, lido pela coordenação e encaminhado a Desenvolvimento e Testes | docs/contrato-sprint1.md contém TVP, procedures, resultados, erros e regras de seleção/idempotência; execução SQL ainda não comprovada neste acompanhamento |
| R07 | 21/10/2026 | Conciliação da massa numérica | Banco orientado a manter NM R$30,00/2, SP R$20,00/3 e segunda carta NM R$15,50/1; clientes de teste com R$100,00 e R$10,00. Instrução enviada às três frentes, sem presumir carga realizada |
| R08 | 21/10/2026 | Banco relata duas tentativas locais de conexão recusadas pelo cliente ODBC com erro de criptografia/SSL | Scripts 000_preparar_base.sql, 001_schema.sql e 002_tipos.sql já presentes na inspeção; criação/aplicação da base não comprovada. Solicitado relatório dos erros e entrega dos scripts com execução pendente, sem alteração de segurança |
| R09 | 21/10/2026 | Revisão automática rejeitou uma mensagem de acompanhamento a Banco por interpretar que sugeria alteração global de segurança | Mensagem não enviada; substituída por instrução limitada a documentar erros e entregar scripts, proibindo alteração de segurança. A entrega documental prossegue; não houve mudança de configuração |
| R10 | 21/10/2026 | Acompanhamento de Desenvolvimento e leitura do roteiro entregue por Testes | Desenvolvimento relata restore obtido e correção de dependência apontada como vulnerável, build ainda pendente. docs/testes-sprint1.md prepara 30 casos da sprint 1 e seis da sprint 2; registra zero cenários funcionais executados e consultas somente preparadas. Isso não aprova a implementação |
| R11 | 21/10/2026 | Entrega de preparação de Testes/documentação recebida e lida pela coordenação | docs/entrega-sprint1-testes.md, docs/rastreabilidade-academica.md, docs/testes-sprint1.md e scripts de inspeção/consulta entregues. Matriz liga requisitos aos capítulos 3/4/5/6. Zero casos funcionais executados; 30 casos S1 bloqueados e seis S2 planejados. Evidência estática: docs/evidencias/sprint1-preparacao-inventario.json |
| R12 | 21/10/2026 | Acompanhamento posterior das entregas configurado neste chat | Automação acompanhar-entregas-da-sprint-1-pok-vault verifica mudanças a cada 10 minutos, consolida fatos novos neste diário e fica silenciosa sem mudança acionável. Deve pausar quando as três entregas iniciais forem consolidadas ou tiverem bloqueio final documentado; não inicia novas sprints nem declara conclusão sem aceite |
| R13 | 21/10/2026, acompanhamento iniciado às 14h23 (São Paulo) | Entrega inicial de Banco concluída e relatório lido | docs/entrega-sprint1-banco.md e database/000–007 disponíveis: 11 tabelas de negócio + SchemaVersao, TVP, oito procedures, seed conciliado e instruções/permissões. Banco fez inspeções estáticas, mas não aplicou os scripts nem criou base: suas duas consultas falharam em ODBC/SSL. Não confundir entrega dos scripts com aceite de integridade |
| R14 | 21/10/2026 | Segunda evidência e fechamento da preparação de Testes lidos | docs/evidencias/sprint1-preparacao-inventario-02.json identifica oito scripts SQL; consultas comparadas textualmente ao DDL e rotas reais acrescentadas ao roteiro. A ausência de scripts no inventário inicial foi superada. Permanecem zero casos funcionais executados pela frente de Testes nesta rodada |
| R15 | 21/10/2026 | Desenvolvimento relata avanço funcional; coordenação consulta execução registrada no chat | Build ./dev.ps1 Build registrado com saída 0, zero avisos/erros. .local/smoke-mvc.ps1 teve saída 0 e sete verificações positivas de login/formulário, carrinho, UUID, compra, login administrativo, cancelamento e logout. Verificações API subsequentes mostram resultados positivos, mas o comando agregado terminou com saída 1; não considerar a rodada agregada aprovada. Relatório final/evidências e delimitação da conexão SQL ainda aguardados |
| R16 | 21/10/2026 | Divergência transitória do saldo do segundo cliente verificada em código | Banco havia apontado 100 para ambos no bootstrap inspecionado anteriormente; leitura atual de src/PokeVault.Web/Infrastructure/Bootstrap.cs usa 10m para cliente2 e 100m para cliente1. Correção textual observada; não comprova por si só os valores persistidos. Desenvolvimento solicitado a documentar preparação/verificação real e não iniciar sprint 2 |
| R17 | 21/10/2026 | Entrega final de Desenvolvimento recebida; coordenação leu relatório e JSONs de verificação | docs/entrega-sprint1-desenvolvimento.md registra scripts 000–006 aplicados em PokeVault, motor SQLEXPRESS 17.0.1000.7; reconciliação 007 com saída 0 e zero divergências. .local/smoke-desenvolvimento-resultado.json contém 30 verificações positivas e .local/smoke-mvc-resultado.json contém sete; datas UTC 17:23:59 e 17:23:33. São checks de desenvolvimento, não os 30 casos formais da matriz. Relatório esclarece que a saída agregada 1 de R15 veio do roteiro e a reconciliação foi executada separadamente com sucesso |
| R18 | 21/10/2026 | Daniel informa o fim da primeira sprint; coordenação registra balanço do ciclo | Fluxo básico funcional entregue; aceite completo não comprovado, com pendências explícitas abaixo. Os três chats terminaram suas entregas iniciais; automação de acompanhamento consultada com status PAUSED. Nenhuma sprint seguinte iniciada |

### Balanço do encerramento — 21/10/2026

**Resultado:** primeiro incremento funcional local de ASP.NET Core MVC/Razor + API REST e SQL Server. O ciclo está encerrado por informação de Daniel, com objetivo parcialmente atendido: o percurso básico funciona nas verificações realizadas, mas os critérios completos de concorrência, rollback e isolamento de testes ainda não foram comprovados. Encerrar o ciclo não significa aprovar requisitos não testados.

| Frente | Entrega consolidada | Limite de aceite |
| --- | --- | --- |
| Banco | 11 tabelas de negócio mais SchemaVersao, TVP, oito procedures, seed, permissões e reconciliação. Desenvolvimento aplicou os scripts na base PokeVault e usou as procedures | O erro ODBC/SSL da inspeção inicial foi superado para a execução autorizada de Desenvolvimento, sem alterar configuração de segurança. Conta runtime restrita ainda não configurada/verificada |
| Desenvolvimento | Login/logout dos três papéis, catálogo público com nome/coleção/conservação e paginação, carrinho persistente, compra dos itens selecionados, histórico e cancelamento administrativo integral com motivo | Build sem avisos/erros; verificações locais positivas. Telas completas de gestão e aplicativo mobile permanecem posteriores |
| Testes/documentação | Matriz de 30 casos S1 e seis S2; roteiros, consultas e inventários; rastreabilidade dos requisitos para capítulos 3–6 | Frente executou inspeções estáticas; sua suíte funcional formal não foi executada. Não somar esses casos aos checks de Desenvolvimento |
| Coordenação | Planos conciliados, decisões de funcionamento registradas, backlog/sprints, distribuição por frente e diário único | Trabalho escrito final e diagrama ainda precisam incorporar implementação e evidências atuais; planejamento não substitui essa revisão |

**Verificações efetivamente registradas:** build com zero avisos/erros; 30/30 checks HTTP/integrados e 7/7 checks MVC positivos; reconciliação SQL sem divergências segundo o relatório de Desenvolvimento. Coordenação leu os dois JSONs de resultados e o manifesto, sem repetir as operações na base.

Exemplo observado: cliente com R$100 compra uma carta NM de R$30 e uma SP de R$20, fica com R$50 e mantém no carrinho o item não selecionado. O administrador cancela a compra, saldo volta a R$100 e estoques retornam às quantidades anteriores. Repetição da compra com mesmo UUID não duplica débito; repetição de cancelamento não duplica estorno. Cliente não consulta compra alheia nem cancela; atendente não compra. Esses resultados valem para os cenários efetivamente executados, sem afirmar garantia universal sob concorrência.

**Problemas resolvidos nesta rodada:** configuração/cache/restauração de pacotes e dependência apontada por auditoria; compilação da proteção DPAPI; conexão SQL no contexto autorizado; QUOTED_IDENTIFIER na aplicação do schema usando sqlcmd -I; preço decimal recebido pelo formulário MVC; saldo inicial da segunda conta. O relatório de Desenvolvimento documenta erros, correções e verificações posteriores. As verificações criaram compras que foram canceladas pelas rotinas normais, preservando histórico; não foi feito apagamento para simular estado limpo.

**Pendências transferidas para refinamento do próximo ciclo, sem início automático:**

1. Preparar PokeVaultTests exclusiva e executar a matriz formal; provar disputa por última unidade/saldo, compras/cancelamentos concorrentes e rollback após escrita com falha controlada.
2. Validar conta SQL runtime com privilégios restritos; a execução atual usou conta Windows de preparação.
3. Conferir alteração de preço entre confirmação e compra e perda de acesso da conta desativada com sessão existente; critérios ainda não comprovados nesta entrega.
4. Tornar evidências duráveis: os JSONs/roteiros de Desenvolvimento estão em .local/, ignorada pelo Git, e guardam a última execução. Preservar cópias sanitizadas e mapear casos formais às evidências antes do escrito final.
5. Implementar gestão de clientes/saldo/funcionários e catálogo/estoque conforme recorte proposto da sprint 2. Procedures disponíveis não significam telas/endpoints completos entregues.
6. Atualizar texto acadêmico e diagrama ao modelo real; Capacitor segue para sprint posterior. Responsividade/acessibilidade ampla também não foi validada.

Para o trabalho escrito: “A primeira sprint entregou um incremento local do PokéVault com autenticação, catálogo público, carrinho persistente, compra dos itens selecionados, histórico e cancelamento administrativo. As verificações de desenvolvimento registraram 30 checks HTTP/integrados e sete do fluxo MVC positivos, além de compilação sem avisos ou erros e reconciliação sem divergências reportadas. A suíte formal de testes e as verificações de concorrência e rollback permaneceram pendentes, impedindo afirmar validação integral do sistema.”

### Subsídios para o trabalho escrito

O capítulo 5 pode descrever a priorização e os acordos desta rodada a partir deste diário, distinguindo planejamento de execução. Capítulos 3 e 4 devem receber arquitetura, contratos e modelo reais quando entregues. Resultados e conclusão devem citar testes efetivamente executados e suas limitações. A frente de documentação preparará uma matriz de revisão do PDF, sem afirmar retrospectivamente que as funcionalidades ou reuniões descritas já ocorreram.

Formulação utilizável agora: “O planejamento do PokéVault priorizou uma demonstração local do fluxo de autenticação, catálogo, compra simulada e estorno. A conciliação dos planos definiu responsabilidades por frente e critérios de integridade de saldo e estoque. O início de referência informado para a primeira sprint foi 7 de setembro de 2026; os registros de conciliação e mobilização apresentados neste diário foram produzidos em 21 de outubro de 2026.”

## Sprint 2 — administração e operação

**Situação:** rodada de entregas integrada e encerrada pela coordenação em 21/10/2026, com meta funcional entregue e ressalva de infraestrutura explicitada no balanço. Iniciada por instrução de Daniel na mesma data real, sem duração inventada. Acordos herdados: atendente e administrador gerem catálogo/estoque; apenas administrador cria clientes/funcionários, define saldo e estorna. O trabalho escrito será reescrito após terminar o sistema; este diário e as evidências continuam sendo mantidos.

| Frente | Trabalho previsto | Dependências / problemas a tratar |
| --- | --- | --- |
| Banco | Completar rotinas de cadastros/ajustes e preservar auditoria | Contratos e defeitos reais da sprint 1 |
| Desenvolvimento | Telas de clientes, saldo, funcionários e manutenção comercial; filtros complementares | Autorização confirmada; não ampliar para logística |
| Testes/documentação | T03–T07 e permissões do atendente, conta desativada e ajustes concorrentes; atualizar escrito | Versão executável e massa controlada |
| Coordenação | Refinar seleção e atualizar este diário com problemas/entregas reais | Capacidade e aceite da sprint 1 |

Critério proposto de conclusão: administração funcional, auditada e autorizada, com regressão do núcleo e documentação correspondente. Problemas ocorridos e execuções: ainda não há registros desta sprint.

### Compromisso de abertura e divisão executável

Meta: Daniel consegue criar contas, preparar saldo fictício e manter catálogo/estoque pelas telas, com autorização no servidor, histórico preservado e verificações reais. A sprint incorpora os testes críticos pendentes da primeira.

| Cartão | Frente | Entrega / aceite | Estado inicial |
| --- | --- | --- | --- |
| S2-C01 | Coordenação | Abertura, instruções e acompanhamento no diário único; decisões existentes respeitadas | Concluído quanto ao recorte; envio e retornos serão registrados |
| S2-B01 | Banco | Publicar contrato-sprint2.md, scripts aditivos numerados, ajustes com versão esperada e preparação controlada de PokeVaultTests | A fazer |
| S2-B02 | Banco | Validar procedures e privilégios runtime do projeto; registrar configuração realizada ou limite concreto | A fazer |
| S2-D01 | Desenvolvimento | MVC/API de clientes e atendentes: listagem/cadastro, saldo, desativação/reativação de atendente | A fazer |
| S2-D02 | Desenvolvimento | MVC/API de catálogo operacional: listar ativos/inativos, cadastrar carta por conservação, ajustar preço/estoque/ativação com motivo e controle de conflito | A fazer |
| S2-D03 | Desenvolvimento | Integrar contratos, navegar por perfil, preservar núcleo e credenciais existentes; build, execução e README atualizados | A fazer |
| S2-T01 | Testes | Testes executáveis e evidências duráveis em base exclusiva; validar cadastros, saldo, permissões, sessões desativadas e preço alterado | A fazer |
| S2-T02 | Testes | Última unidade/saldo e estorno concorrentes, idempotência, rollback após escrita e regressão de compra selecionada | A fazer |

**Arquivos:** Banco é dono de database/, docs/contrato-sprint2.md, docs/plano-banco.md e docs/entrega-sprint2-banco.md. Desenvolvimento é dono de src/, solução/configuração/README/dev.ps1, docs/plano-desenvolvimento.md e docs/entrega-sprint2-desenvolvimento.md. Testes é dono de tests/, docs/testes-sprint2.md, docs/evidencias/sprint2/ e docs/entrega-sprint2-testes.md, podendo atualizar seu plano. Coordenação edita os documentos centrais e este diário. Não iniciar testes mutantes concorrentes na mesma massa; Testes coordena a execução funcional em PokeVaultTests depois da preparação por Banco e da versão disponibilizada por Desenvolvimento.

Contratos HTTP devem seguir a base /api/v1: /admin/clientes e /admin/funcionarios para gestão administrativa; /gestao/ofertas para catálogo operacional incluindo inativos. Rotas específicas podem ser detalhadas por Desenvolvimento mantendo o catálogo público e o checkout existentes. Ajuste de saldo e estoque exige versão esperada; Banco publica os parâmetros e Desenvolvimento integra sem alterações financeiras diretas. Listagens nunca devolvem hash/senha.

### Dependências e riscos na abertura

- Existe aplicação funcional em PokeVault; não resetar estoque, saldo, contas ou histórico para produzir evidência. Reinício do processo necessário a build fica sob Desenvolvimento; todos os demais usam processo/base de testes separados.
- Banco entrega o contrato aditivo primeiro; Desenvolvimento pode iniciar telas e políticas com as procedures atuais enquanto aguarda a versão esperada. Testes prepara cenários independentemente e executa após base e aplicação de testes identificadas.
- PokeVaultTests ainda não estava preparada na entrega da sprint 1; criar com guarda de propriedade e scripts versionados. Testes prepara suas contas/massa sem reutilizar o arquivo protegido de credenciais de demonstração.
- Privilégios runtime e falha controlada permanecem critérios em aberto. Nenhuma alteração global de segurança é autorizada; registrar limites reais sem declarar aprovação.
- Não há novas dúvidas de funcionamento bloqueadoras identificadas na abertura. Escolhas técnicas rotineiras podem ser resolvidas pelas frentes conforme contratos; mudanças de funcionamento devem retornar a Daniel pela coordenação.

### Registros efetivos da sprint 2

| Registro | Data real | Ação / acordo | Evidência e limite |
| --- | --- | --- | --- |
| S2-R01 | 21/10/2026 | Daniel solicita “Então vamos pra Sprint 2! Comece”; coordenação confere decisões, fechamento da sprint 1 e código existente | Sprint autorizada; capítulos acadêmicos aguardam sistema final. Implementação e testes desta sprint ainda não executados no momento da abertura |
| S2-R02 | 21/10/2026 | Instruções enviadas aos três chats existentes de Banco, Desenvolvimento e Testes/documentação | Envio confirmado pela ferramenta; divisão de arquivos, critérios e dependências repassados. Banco deve publicar contrato primeiro; Testes tem posse da massa de teste após preparação da base |
| S2-R03 | 21/10/2026 | Automação existente reaproveitada para acompanhar a sprint 2 | Nome atualizado para Acompanhar entregas da sprint 2 — PokéVault, a cada 10 minutos, com aviso somente sobre mudanças relevantes. Deve consolidar entregas, promover continuação de tarefas desbloqueadas e pausar no balanço final da rodada; não iniciar sprint 3 |
| S2-R04 | 21/10/2026 | Primeiro snapshot confirma os três chats ativos | Banco iniciou inspeção/contrato, Desenvolvimento iniciou leitura/implementação e Testes iniciou preparação isolada. S2-B01, S2-D01 e S2-T01 passam a Em desenvolvimento; demais cartões preservam dependências. Nenhuma entrega ou teste S2 aprovado nesta abertura |
| S2-R05 | 21/10/2026, acompanhamento às 14h54 (São Paulo) | Entrega parcial de Banco e contrato aditivo lidos | Contrato usa rowversion em Carta/SaldoCliente, Base64 de oito bytes e procedures Versionado com conflito 51012/409; scripts 008–010 aplicados em PokeVault e PokeVaultTests. Relatório registra liberação da base de testes às 14h49 e evidências em database/evidencias/sprint2/; comparação antes/depois preservou o conteúdo das 11 tabelas comerciais da demonstração |
| S2-R06 | 21/10/2026 | Privilégios internos verificados por Banco; limite de conexão externa registrado | Principal WITHOUT LOGIN exclusivo do banco comprovou SELECT/EXECUTE e recusas 229 de DML e de ajustes legados sem versão. Isso não comprova conexão runtime independente: servidor somente Windows e identidade dedicada externa ainda não provisionada. Nenhuma mudança global de segurança foi feita ou autorizada |
| S2-R07 | 21/10/2026, evidência de 14h53min19s | Primeira rodada de Testes lida e contada pela coordenação | docs/evidencias/sprint2/rodada01-resultados.json registra 32 cenários aprovados em PokeVaultTests, instância SQLEXPRESS. Inclui compra selecionada, recusas, concorrência e rollback conforme casos registrados. Desenvolvimento relata telas MVC/API compiladas sem avisos/erros, mas seu relatório ainda está em verificação. Os três chats continuam ativos; não é aceite final da sprint |
| S2-R08 | 21/10/2026 | Lacuna de auditoria identificada por Banco e correção encaminhada | Ajuste somente de preço/ativação exigia motivo sem persistir todo o registro. Script aditivo 013_sprint2_auditoria_carta.sql propõe tabela AlteracaoCarta com autor/motivo/valores anteriores e novos, sem movimento estoque zero e sem mudar assinaturas. Coordenação avisou as três frentes e pediu janela livre de cenários mutantes antes de aplicar em PokeVaultTests, caso ainda não aplicado; Testes deve conferir versão e adicionar caso/regressão de auditoria. Aplicação do 013 ainda não confirmada neste acompanhamento |
| S2-R09 | 21/10/2026, acompanhamento iniciado às 15h04 (São Paulo) | Três entregas finais lidas e snapshots confirmam chats concluídos | docs/entrega-sprint2-banco.md, entrega-sprint2-desenvolvimento.md e entrega-sprint2-testes.md recebidos. Banco registra 013 aplicado às 14:54:38.108 em PokeVaultTests, antes de receber o pedido de janela exclusiva; sem alegar exclusividade retroativa. Testes confirmou versão 4 e executou casos de auditoria posteriores ao complemento |
| S2-R10 | 21/10/2026 | Resultados finais e versão do produto confrontados pela coordenação | Produto com build 0 avisos/erros; Desenvolvimento registra 54 checks integrados e 22 MVC positivos. Testes consolida 45 cenários distintos com último estado aprovado (21 SQL, 18 HTTP, 6 MVC). Rodada04 teve 44 aprovações e uma expectativa de roteiro inadequada; reteste HTTP18 passou exigindo 400 e nenhuma conta criada. Não há rodada única 45/45 nem soma desses cenários com checks de Desenvolvimento |
| S2-R11 | 21/10/2026 | Reconciliação e limpeza finais conferidas em JSON | docs/evidencias/sprint2/pos-verificacoes-final.json registra scripts 007/011/Verificar-Integridade com exit0, zero divergências de saldo/estoque/totais e zero triggers temporárias. encerramento-processo-isolado.json registra término somente do processo5188 às 15h00min47s. PokeVaultTests permanece com históricos e massa própria; demonstração5187 não foi interrompida por Testes |
| S2-R12 | 21/10/2026 | Balanço final e encerramento do acompanhamento da rodada | Administração funcional, regressão e cenários críticos comprovados nos limites dos relatórios. Conta Windows externa runtime de menor privilégio continua pendente; permissões de banco comprovadas por WITHOUT LOGIN não substituem essa implantação. Acompanhamento deve ser pausado após consolidar este balanço; sprint3 não iniciada |
| S2-R13 | 21/10/2026 | Automação de acompanhamento pausada após consolidação | Ferramenta confirmou status PAUSED da automação existente. Três entregas concluídas, evidências conferidas e ressalvas registradas; não haverá início automático de outra sprint |

### Estado parcial após o primeiro acompanhamento

- **S2-B01:** contrato e atualizações iniciais entregues/aplicadas; complemento de auditoria em desenvolvimento.
- **S2-B02:** permissões internas verificadas; conexão runtime por identidade externa continua pendente.
- **S2-D01, S2-D02 e S2-D03:** implementação compilada e verificações locais em andamento, conforme entrega parcial; aceite final pendente.
- **S2-T01 e S2-T02:** em teste; primeira rodada com 32 aprovações registradas, complementação de catálogo/MVC e regressão da auditoria pendentes. Conservar rodada01 como evidência histórica.
- **Impedimento de integração a coordenar:** atualização de schema 013 não deve coincidir com fixtures ou cenários concorrentes de Testes. O contrato pode ser consumido sem alteração de assinatura; comunicar estado real antes da rodada final.

### Condição de encerramento

Gestões de usuários/saldo/catálogo disponíveis por perfil, validação e antifalsificação também por HTTP direto, contas desativadas sem acesso, ajustes auditados e conflito de edição tratado; compra selecionada/histórico/estorno continuam funcionando. Casos críticos de concorrência/rollback e limites de permissão SQL precisam de resultados reais ou bloqueios explícitos no balanço; build sozinho não comprova esses aceites. Cada frente entrega resumo de execução, arquivos, comandos, problemas e evidências reais; a coordenação consolida o encerramento, sem iniciar a sprint 3 automaticamente.

### Balanço final da rodada — 21/10/2026

**Meta funcional atingida no recorte local verificado:** administrador cadastra/lista clientes e atendentes, define saldo absoluto com motivo e desativa/reativa atendentes; administrador/atendente cadastram cartas e ajustam preço/estoque/ativação. MVC e API compartilham serviços, validam autorização e antifalsificação, recusam dados inválidos/campos indevidos e não expõem senha/hash nas listagens. Novas contas começam com saldo zero e carrinho vazio. Ajustes versionados recusam formulário desatualizado e exigem recarga, preservando operações concorrentes.

| Cartão | Balanço | Evidência / ressalva |
| --- | --- | --- |
| S2-C01 | Concluído | Decisões, distribuição, integração dos três relatórios e este diário |
| S2-B01 | Concluído | Scripts aditivos 008–010/013 aplicados, schema4, contrato e base de testes liberados; auditoria AlteracaoCarta corrigida e testada |
| S2-B02 | Parcial; pendência externa registrada | SELECT/EXECUTE e negação de DML/ajustes legados comprovados sob principal de banco; falta conexão runtime por identidade Windows dedicada |
| S2-D01/S2-D02/S2-D03 | Concluídos no recorte entregue | Telas/API, navegação por perfil, integração versionada, build e README; evidências de Desenvolvimento próprias e duráveis |
| S2-T01/S2-T02 | Concluídos no conjunto executado | 45 cenários distintos aprovados após reteste; matriz e relatórios delimitam subcasos não cobertos, sem declarar cobertura universal |

**Comprovações novas em relação à sprint 1:** compras disputando última unidade/saldo; reenvio UUID e cancelamentos simultâneos; rollback após escrita real; mudança de preço com novo aceite; invalidação da sessão de atendente desativado; conflito de versão sem perda de venda/ajuste; auditoria de preço/ativação mesmo sem delta de estoque. Falhas foram induzidas exclusivamente em PokeVaultTests com triggers temporárias removidas em finally; verificação independente confirmou zero remanescentes. Nenhum mecanismo de falha foi incorporado à demonstração.

**Versão conferida:** DLL do produto e snapshot de testes SHA256 `9B717A9B37A733A9213284C93A6DE2FC574CB66D383281AF1C5B20B7175A5EDC`. Evidências da suíte: docs/evidencias/sprint2/rodada01–05, consolidacao-final.json e pos-verificacoes-final.json. Checks próprios de Desenvolvimento: src/evidencias/sprint2/resultado-desenvolvimento.json e resultado-mvc.json; não somar suas contagens à suíte. Manifestos e falhas/retestes estão preservados.

**Problemas tratados:** motivo/autor de alteração somente preço/ativação passou a constar de AlteracaoCarta; teste HTTP18 passou a esperar a recusa400 correta para campos de perfil proibidos, sem mudar o produto para satisfazer expectativa errada; opção sqlcmd incompatível foi corrigida e retestada; verificação de CSRF de Desenvolvimento removeu cabeçalho que seu roteiro retinha inadvertidamente. Todos constam nos relatos, sem converter tentativas reprovadas em aprovações retroativas.

**Dados e acessos:** scripts de upgrade preservaram registros preexistentes por comparação de conteúdo. Desenvolvimento acrescentou contas/ofertas explicitamente sintéticas em PokeVault para suas verificações e as deixou inativas/com saldo zero quando pertinente, preservando histórico. Dados originais foram conferidos em seu relatório; credenciais originais não foram redefinidas. Testes utilizou massa própria em PokeVaultTests, sem alterações na demonstração.

**Ressalvas e pendências para próximos refinamentos, sem início automático:**

1. Configurar uma identidade Windows dedicada para a conexão runtime e comprovar seus privilégios externamente. O servidor aceita apenas autenticação Windows; não se criou conta Windows nem alterou autenticação global. Demonstração ainda usa identidade de preparação. Esse critério fica explicitamente incompleto.
2. Cobertura não equivale a todos os subcasos conceituais T01–T36; testes não demonstram ausência universal de defeitos. Wrapper tests/Executar-Sprint2.ps1 teve parser validado, mas não foi executado inteiro; harness/fluxos foram executados por comandos diretos.
3. Build do harness apresentou aviso NU1900 por indisponibilidade da consulta NuGet no contexto restrito; isso não é resultado de teste nem auditoria de dependências concluída. Produto teve build sem avisos/erros segundo Desenvolvimento.
4. Não houve ensaio visual amplo, acessibilidade, carga ou repetição estatística de concorrência. Paginação e demais limites constam nos relatórios. Recuperação de senha, gestão de coleções e tela de auditoria não foram adicionadas.
5. Capacitor e revisão do trabalho acadêmico continuam posteriores; nenhuma sprint3, publicação, buyback ou relatório foi iniciada.

Encerramento da rodada com ressalva não transforma o critério de identidade externa em aprovado. Os relatórios e evidências ficam prontos para revisão por Daniel; a próxima sprint depende de sua instrução.

### Complemento web da sprint 2 — iniciado em 03/11/2026

**Atualização de prioridade em 03/11/2026:** Daniel esclareceu que as melhorias e o fluxo de compras serão discutidos por último. Complemento suspenso e observações mantidas no backlog; nenhum novo avanço está autorizado até retomada. Instruções iniciais e propostas abaixo são históricas. Frentes orientadas a concluir somente operações em andamento em ponto seguro, registrar trabalho parcial e preservar a demonstração, sem desfazer/apagar artefatos por iniciativa própria.

Daniel relatou que os testes manuais que realizou funcionaram e trouxe quatro melhorias: visibilidade de senha, cadastro de coleções, raridades predefinidas com símbolos ingleses e clareza da inclusão no carrinho. O relato não substitui execuções formais nem identifica todos os casos percorridos. A rodada anterior permanece encerrada com seu histórico; este complemento precede o mobile e não inicia a sprint3.

| ID | Frente | Tarefa e aceite | Estado na abertura |
| --- | --- | --- | --- |
| WEB-C01 | Coordenação | Registrar feedback, recorte e contratos; acompanhar entregas sem reescrever trabalho final | Recorte registrado |
| WEB-B01 | Banco | Contrato/migração aditiva para coleções com total, lista de raridades e inclusão aditiva atômica no carrinho; preservar dados legados e permissões | A fazer |
| WEB-D01 | Desenvolvimento | Olho de senha acessível no login/cadastro; cadastro de coleção e seleção imediata no cadastro de carta | A fazer |
| WEB-D02 | Desenvolvimento | Select de 12 raridades, nomes e símbolos ingleses locais na gestão/catálogo; novos valores inválidos recusados | A fazer |
| WEB-D03 | Desenvolvimento | Catálogo com estoque, quantidade a adicionar e botão Adicionar ao carrinho; somar linhas existentes sem perder inclusões concorrentes; carrinho mantém edição absoluta | A fazer |
| WEB-T01 | Testes | Coleções, raridades, inclusão aditiva/repetida/concorrente e regressão do checkout/edição; olho por navegador se disponível; evidências sanitizadas | A fazer |

Banco publica primeiro docs/contrato-complemento-web.md e escreve database/ e docs/entrega-complemento-web-banco.md. Desenvolvimento escreve src/, README/dev.ps1 e docs/entrega-complemento-web-desenvolvimento.md. Testes escreve tests/, docs/testes-complemento-web.md, docs/evidencias/complemento-web/ e docs/entrega-complemento-web-testes.md. Coordenação edita decisões e diário único. Sem DDL concorrente com cenários mutantes: Banco indica liberação do schema antes da execução de Testes. Demonstração5187 preservada; testes usam PokeVaultTests e processo isolado.

Pontos de integração: código técnico de coleção gerado pelo servidor; total legado não confirmado permanece não informado; não limitar número da carta por total da coleção. Raridades desconhecidas legadas ficam preservadas/identificadas; somente novos cadastros exigem lista fechada. Adicionar deve incrementar atomicamente, não ler quantidade na aplicação e sobrescrever; a atualização absoluta permanece operação distinta. Ícones devem acompanhar nomes para acessibilidade, usando a referência visual fornecida por Daniel.

| Registro | Data real | Fato / limite |
| --- | --- | --- |
| WEB-R01 | 03/11/2026 | Feedback recebido de Daniel com imagem de referência de raridades; coordenação leu views de login/catálogo/carrinho, modelos e schema. Constatação: senha sem alternância, raridade texto livre, coleção apenas selecionada e ação de catálogo substitui quantidade existente. Melhorias ainda não implementadas no momento da abertura |
| WEB-R02 | 03/11/2026 | Instruções enviadas aos três chats existentes, com divisão exclusiva de arquivos e contrato primeiro por Banco; envio confirmado. Acompanhamento existente atualizado para complemento web, ativo a cada10minutos, silencioso sem mudanças relevantes e finito até balanço das entregas. Não é evidência de implementação/teste |
| WEB-R03 | 03/11/2026 | Daniel revê a prioridade: por enquanto são observações; melhorias e fluxo de compras ficam por último. Coordenação enviou orientação de suspensão aos três chats, sem novas migrações/recursos/testes do complemento, preservando operações em andamento e registrando estado parcial. A rotina de acompanhamento deve ser pausada; não reabrir tarefas do complemento automaticamente |
| WEB-R04 | 03/11/2026 | Automação confirmada PAUSED. Banco e Desenvolvimento acusaram recebimento e registram ponto seguro. Banco relata tentativa014 com erro de compilação e nenhum script posterior chamado; confirmação de estado somente por leitura em andamento. Desenvolvimento relata código parcial compilado, sem aceite/liberação do complemento. Coordenação conferiu HTTP200 em /, /api/v1/ofertas e /entrar da demonstração5187; isso prova disponibilidade dessas rotas, não aprovação das melhorias adiadas. Artefatos parciais devem ser preservados para eventual retomada |

Critério de conclusão: os quatro comportamentos entregues na web, novas coleções selecionáveis, taxonomia respeitada, inclusão aditiva correta e regressão aprovada com evidências; limites de testes/UI explicitados e acessos originais preservados. Não interpretar a rodada como entrega mobile ou revisão acadêmica.

### Fechamento da web academica local — aberto em 03/11/2026

**Balanço atualizado em03/11/2026: revisão funcional concluída para a demonstração acadêmica local, com limite explícito de inspeção visual automatizada.** Abertura e registros intermediários abaixo são históricos; conclusão detalhada ao final desta seção. ContaWindows dedicada e hospedagem não são pendências obrigatórias, mobile e melhorias continuam adiados.

Daniel determinou prioridade exclusiva a demonstracao local apresentavel. Conta Windows dedicada deixa de ser pendencia obrigatoria; sua ausencia permanece limitacao de implantacao documentada. Mobile fica por ultimo e melhorias do complemento seguem adiadas.

| Cartao | Frente | Entrega / aceite | Estado inicial |
| --- | --- | --- | --- |
| REV-C01 | Coordenacao | Rever criterios e consolidar resultados no diario, sem nova infraestrutura | Recorte registrado |
| REV-D01 | Desenvolvimento | Arquivar WIP adiado com inventario e tornar fonte ativa compativel com schema4, build reproduzivel sem fallback oculto | A fazer |
| REV-D02 | Desenvolvimento | Revisar navegacao/validacoes/telas, corrigir defeitos necessarios e oferecer abertura simples por duplo clique; deixar demo funcionando | A fazer |
| REV-B01 | Banco | Conferir schema4 e reconciliacao/dados com leituras, sem novas migracoes ou mudancas globais | A fazer |
| REV-T01 | Testes | Executar roteiro automatizado completo com fonte final + verificacao navegador dos fluxos/perfis e tamanhos de tela, evidencias duraveis | A fazer |

Risco concreto identificado na abertura: fontes/binarios de complemento suspenso dependem de coluna/procedures nao implantadas; demo usa artefato estavel separado. Para fechar web reproduzivel, Desenvolvimento preservara o WIP em arquivo local inventariado e ajustara apenas a fonte ativa necessaria ao schema4. Nao concluir melhorias adiadas sob pretexto de estabilizacao.

Posse: Desenvolvimento src/configuracao/README/dev.ps1/abertura local + docs/entrega-revisao-web-desenvolvimento.md; Banco database/evidencias/revisao-web/ + docs/entrega-revisao-web-banco.md, sem novos DDL; Testes tests/ + docs/evidencias/revisao-web/ + docs/testes-revisao-web.md + docs/entrega-revisao-web-testes.md; Coordenacao documentos centrais. Arquivo WIP fica sob posse de Desenvolvimento em pasta separada, sem alterar conteudo historico das entregas. Banco nao modifica massa e Testes nao toca em PokeVault/demo5187.

| Registro | Data real | Fato / limite |
| --- | --- | --- |
| REV-R01 | 03/11/2026 | Daniel pede fechamento web local e mobile por ultimo. Coordenacao le entregas da interrupcao e fontes: demo estavel preservada, fonte ativa ainda contem WIP; necessidade real de isolamento e reproducibilidade antes do fechamento |
| REV-R02 | 03/11/2026 | Instrucoes de revisao enviadas aos tres chats existentes; confirmacao de envio recebida. Desenvolvimento deve preservar WIP e fonte estavel reproduzivel; Banco apenas leitura; Testes wrapper completo e navegador na fonte final. Nenhuma contaWindows dedicada/hospedagem exigida. Ainda sem resultados desta revisao |
| REV-R03 | 03/11/2026 | Acompanhamento existente reativado e limitado ao fechamento web local, a cada10minutos, silencioso sem mudancas relevantes. Deve integrar entregas/desbloquear execucao de Testes e pausar no balanco final, sem iniciar mobile ou melhorias adiadas |
| REV-R04 | 03/11/2026 | Coordenacao le arquivo-wip.json de Desenvolvimento: arquivo de117 itens,109 fontes conferidas e ZIP com SHA256 registrado; segredos/.local/.keys/bin/obj excluidos salvo DLL WIP identificada. Isso comprova preparacao do arquivo, nao conclusao da fonte final |
| REV-R05 | 03/11/2026 | Leituras de Banco em execucoes.json registram quatro comandos exit0 nas duas bases. Metadados PokeVault mostram schema1–4 e zero coluna/procedures/constraint/versoes do complemento adiado. Nenhuma migracao adicional foi executada nesta revisao; reconciliacoes e relatorio final em consolidacao |
| REV-R06 | 03/11/2026 | Testes relata ferramenta computer-use encerrada por nao conseguir identificar URL do Chrome com confianca para aplicar politica. Nao ha revisao visual automatizada aprovada nem tentativa autorizada de contorno. Coordenacao orientou prosseguir wrapper/regressao e delimitar HTML/HTTP versus visual; feedback manual de Daniel continua relato separado |
| REV-R07 | 03/11/2026 | Coordenacao produziu docs/ROTEIRO-DEMONSTRACAO-WEB.md com preparacao e percurso propostos para apresentar web local; nao registra teste executado nem exige hospedagem/identidade dedicada |
| REV-R08 | 03/11/2026 | Fonte final liberada em src/evidencias/revisao-web/prontidao.json, 22:11:53UTC, estado PRONTO_PARA_REGRESSAO_ISOLADA/schema4. HashDLL/snapshot177F24F52C71190AC3420CD62E34F741B73DD1B7DD4ABF5FA64A392547E33CE4; build.txt registra0avisos/0erros. Coordenacao avisou Testes para executar wrapper inteiro na versao liberada; orientou Desenvolvimento a preservar snapshot durante execucao. Nao e resultado de regressao aprovada |
| REV-R09 | 03/11/2026 | Wrapper executado pontaaponta pela frente de Testes na fonte final. Coordenacao le docs/evidencias/revisao-web/wrapper-01/resultados.json e confirma45 registros com statusaprovado. Evidencias de execucao, manifesto e encerramento do processo isolado preservadas; verificacoes finais de HTML/HTTP/reconciliacao em andamento. Nenhuma inspecao visual automatizada alegada |
| REV-R10 | 03/11/2026 | Entrega parcial de Desenvolvimento registra demo5187 no build ativo novo (nao fallback antigo), ready200 com SQL/schema4, launcher iniciado e reutilizado sem segundo processo. Abrir-PokeVault.cmd e helper criados. Teste de falha e abertura do navegador ainda em andamento; nao e aceite final do launcher |
| REV-R11 | 03/11/2026 | Banco concluiu revisao somente leitura: ambas bases schema4 sem complemento parcial, reconciliacao007 sem divergencias e quatro contasdemo ativas/senhascompativeis verificadas em memoria. Nao houve reset ou mudanca de identidade/configuracao. Relatorio preserva limites de comparacao historica |
| REV-R12 | 03/11/2026 | Coordenacao le html-01/resultados.json: seis verificacoesHTTP/HTML aprovadas para estadosvazios/semantica, rotulos/login sem ecoarsenha, navegacao/perfis, validacao sem cadastro indevido e filtros/paginacao. Sao separadas dos45cenarios wrapper; nao comprovam layout visual/foco/teclado |
| REV-R13 | 03/11/2026 | Launcher-cmd.json registra pedido de abertura do navegador e reutilizacao; cenario processolocal com bancoindisponivel encerrou auxiliar5199 e retornou falha compreensivel sem alterar SQL real. Acentos helperPowerShell corrigidos e evidencia reteste preservada. Coordenacao conferiuHTTP200 em ready/catalogo/login/APIcatalogo no5187; relatorios finais em consolidacao |
| REV-R14 | 03/11/2026 | Três relatórios finais recebidos e lidos pela coordenação. Desenvolvimento entregou fonte ativa/build/launcher e21checagens próprias de leitura/login; Testes entregou wrapper completo45/45 e HTML/HTTP6/6. Rodada wrapper-final repetiu a suíte após ajustes do próprio wrapper, não é nova cobertura. SHA256produto177F24F52C71190AC3420CD62E34F741B73DD1B7DD4ABF5FA64A392547E33CE4 coincide entre DLL/prontidão/manifesto de execução |
| REV-R15 | 03/11/2026 | Coordenacao confronta reconciliacao-final.json: schema4 e zero divergencias de saldo/estoque/totais ou triggers temporarias; processo5188 encerrado e historico de teste preservado. Demo5187 permanece no build ativo, com ready/catalogo/login/API respondendo200 nas leituras de fechamento. Nenhum reset ou migracao do complemento foi executado |
| REV-R16 | 03/11/2026 | Tres chats com entregas concluidas; coordenacao fecha revisao funcional local e pausa automacao, statusPAUSED confirmado pela ferramenta. Nenhuma nova etapa/mobile/melhoria adiada iniciada; inspecao visual automatizada permanece limite explicito |

### Balanço final da revisão web local — 03/11/2026

**Entrega aceita no recorte funcional acadêmico local verificado:** fonte ativa reproduzível e compatível com banco atual, fluxos S1/S2 preservados, execução simples por duplo clique e regressão completa aprovada. A inspeção visual automatizada desktop/estreita permaneceu não executada por limitação explícita da ferramenta; isso não foi convertido em aprovação visual nem substituído por inferência de HTML. Não há defeito funcional confirmado aberto nas execuções finais, sem alegar ausência universal de defeitos.

| Cartão | Resultado real | Referência |
| --- | --- | --- |
| REV-C01 | Concluído: escopo proporcional à demonstração, integração das três entregas e roteiro proposto | Este diário, DECISOES.md e ROTEIRO-DEMONSTRACAO-WEB.md |
| REV-D01 | Concluído: WIP adiado arquivado com inventário antes do isolamento; fonte ativa compila e não depende de fallback antigo | entrega-revisao-web-desenvolvimento.md; src/evidencias/revisao-web/arquivo-wip.json, build.txt, prontidao.json |
| REV-D02 | Concluído no recorte verificado: abridor por duplo clique, início/reuso/erro de banco, mensagens corretas e demo ativa;21checagens próprias sem mudar dados | Abrir-PokeVault.cmd/.ps1, README e evidências próprias de launcher/leituras HTTP |
| REV-B01 | Concluído: leituras de schema4/ausência de complemento/reconciliação e contas atuais, sem escrita | entrega-revisao-web-banco.md e database/evidencias/revisao-web/ |
| REV-T01 | Concluído para regressão e HTML/HTTP; visual automatizado não executado | entrega-revisao-web-testes.md; docs/evidencias/revisao-web/wrapper-final/, html-01/ e limite-computer-use.json |

O wrapper completo registrou45/45cenários; o conjunto complementar registrou6/6. As21checagens de Desenvolvimento são outra métrica e não devem ser somadas como cobertura formal adicional. Último wrapper conferiu hash da versão liberada, executou restauração/build do harness e suíte, recusou resultados históricos reprovados/parciais no teste do gate e encerrou seu processo em finally. A reconciliação final preserva compras/usuários/cartas de teste, sem apagar dados para produzir sucesso. ContaWindows de desenvolvimento segue como contexto local documentado, sem infraestrutura nova exigida.

**Estabilização realizada:** arquivos do complemento ficaram preservados em src/arquivo-wip/complemento-web-20261103-190746/ e ZIP adjacente com inventário; segredos/chaves/caches excluídos. Foram isoladas da fonte ativa apenas rotas/DTOs/consultas/assets das melhorias adiadas e mantidos os contratos S1/S2. Build intermediário com erro de rota residual foi corrigido; build final0avisos/0erros. Helper local teve acentos corrigidos para WindowsPowerShell5.1 e foi retestado. Essas correções não implantam olho/coleções/raridades/adição.

**Forma de apresentar:** duplo clique em Abrir-PokeVault.cmd na pasta PIM ou acessar http://localhost:5187 quando já ativo. O abridor inicia/reutiliza a aplicação, verifica SQL/schema e solicita abertura do navegador; não executa bootstrap, migração ou reset. A aplicação precisa permanecer em execução local e SQL disponível. Nenhuma hospedagem externa é exigida. Usuários/senhas atuais foram preservados; quatro contas originais conferidas ativas e compatíveis em memória, sem expor segredos.

**Limites e próximos passos, sem execução automática:** inspeção visual real de teclado/foco/layout em diferentes tamanhos não comprovada por ferramenta; relato manual anterior de Daniel é contexto separado, sem lista de casos. Sem prova de carga produtiva ou cobertura universal de todos os subcasos. WarningNU1900 do harness por indisponibilidade da consultaNuGet não equivale a auditoria de dependências aprovada. Melhorias de senha/coleções/raridade/fluxo de carrinho ficam para a conversa posterior de UX e compras; mobile por último e trabalho escrito final também aguardam instrução. Nenhuma dessas frentes foi retomada por este fechamento.

Critérios locais obrigatórios estão encerrados nos limites documentados, sem exigir contaWindows dedicada ou servidor externo. Relatórios, roteiro e evidências ficam disponíveis para a apresentação e para a futura reescrita acadêmica. O acompanhamento desta revisão deve ser pausado; não iniciar nova etapa automaticamente.

Condicao de conclusao adotada na abertura e aplicada no balanco acima: fonte final compila; site local abre de forma simples e responde com SQL; fluxos aprovados dos tres perfis e regressao da fonte final verificados, problemas encontrados resolvidos/retestados ou limites explicitos; dados/acessos preservados; README/diario atualizados. Nao exigir identidade Windows dedicada, hospedagem, teste de carga produtiva ou mobile para aceite academico local.

## Extensao web TCGdex151 — abertura em03/11/2026

**Situação atual: integração concluída e consolidada pela coordenação em03/11/2026, nos limites verificados.** Importação real207referências/imagens,25cenáriosTCGdex e45regressões aprovados na mesma versão final. Abertura e registros intermediários abaixo são históricos; balanço ao final desta seção. Não inicia mobile ou outras melhorias adiadas.

Daniel pediu tentar API TCGdex com apenas colecao151. Extensao autorizada especifica, sem iniciar mobile e sem retomar automaticamente olho/raridadesfechadas/colecoesgenericas/novo fluxocarrinho. Registros anteriores de fechamento web preservados como baseline.

Consultas reais da coordenacao emapi.tcgdex.net: listaen/sets confirmou sv03.5; pt/en/sets/sv03.5 retornaram207cartas, official165/total207. pt/en/cards/sv03.5-025 retornaramPikachu, imagem, raridadee pricingCardmarketEUR e TCGplayerUSD com variantesnormal/reverse-holofoil. Restricaode rede inicial foi superada por execucao autorizada, sem alterar segurancaglobal. Consulta/documentacao nao comprovam integracao implantada.

| Cartao | Frente | Entrega / criterio | Estado inicial |
| --- | --- | --- | --- |
| TCG-C01 | Coordenacao | Recorte, contratos, acompanhamento e evidenciasno diario | Recorte registrado |
| TCG-B01 | Banco | Contrato/migracaoaditiva para referenciascolecao/cartas/precos e vinculosseguros, sem tocar estoque/preco/historico | A fazer |
| TCG-D01 | Desenvolvimento | Consumidor REST limitado151, importacao controlada/cache/retomada; biblioteca local/imagens, fornecedores/moedas/datas | A fazer |
| TCG-D02 | Desenvolvimento | Vincular ofertas seguras a imagem/referencia e pre-preencher cadastro local, mantendo precoBRL/estoque/conservacao locais | A fazer |
| TCG-T01 | Testes | Importacao real/fixture, repeticao, ausencia/falhaAPI/cache, autorizacao e regressao sem dependerinternet no checkout | A fazer |

Banco: database/ e docs/contrato-tcgdex151.md, docs/entrega-tcgdex151-banco.md. Desenvolvimento: src/config/README e docs/entrega-tcgdex151-desenvolvimento.md. Testes: tests/, docs/evidencias/tcgdex151/, docs/testes-tcgdex151.md e docs/entrega-tcgdex151-testes.md. Coordenacao: docscentrais. Banco publica contrato primeiro e libera schema antes dos testesmutantes; desenvolvimento controla demo5187 e testesprocesso5188isolado.

Importar referencia nao cria estoque vendavel ficticio. Biblioteca local207cartas e distinta de ofertascomerciais; referencias faltantes devem ser explicitadas e imagem faltante tem fallback. MesmoPikachuNM/SP pode compartilharidentidadeTCGdex preservando valores/estoques separados. Verificarcodigo/numero/nome antes de vincular, sem substituircartas historicas por dadosestrangeiros. Nenhum trabalho finalacademico reescrito agora.

| Registro | Data real | Fato / limite |
| --- | --- | --- |
| TCG-R01 | 03/11/2026 | Link/documentacao/exemplos recebidos e autorizacao para tentar apenas151; coordenacao consultou fonteoficial e endpoints reais e registrou recorte. Integracao ainda nao implementada nesta abertura |
| TCG-R02 | 03/11/2026 | Instrucoes enviadas aos tres chats existentes, envio confirmado: Banco publica contrato/migracoesaditivas posteriores aoWIP; Desenvolvimento integra importacao/cache/UI; Testes executa em baseisolada aposliberacao. Acompanhamento reativado a cada10minutos, finito e silencioso semmudancas relevantes. Nenhum teste de integracao alegado neste envio |
| TCG-R03 | 03/11/2026, acompanhamento iniciado22:37UTC | ContratoSQL publicado e lido pela coordenacao: biblioteca separada,207cartaslogicas com traducoespt/en, precosJSONdinamicos e preservacao de cachevalido, importacoesporlotesretomaveis e vinculosporcolecao/numero/nome. Scripts019–021 escritos, janelaDDLdeclarada livre porTestes; Banco ainda verifica/aplica. Desenvolvimento eTestes ativos aguardam liberacao tecnica; nenhuma importacao completa ou teste aprovado alegado neste acompanhamento. Estado em progresso,semacao deDaniel necessaria |
| TCG-R04 | 03/11/2026 | Daniel pede retomar chats interrompidos. Snapshots confirmam Banco concluido e Desenvolvimento/Testes com turno falhado por limite de uso. Nao foi erro de negocio comprovado. Banco liberou019–021/schema10 nasduasbases22:37:47UTC, preservacao12tabelas iguais na janela; prontidaoDev22:39:58 publica build/hashA68A5FA77DA7D8F0E529F7DB7F9D95052AE29D2DD2C339B051EF0908AF65C805. Integracao real/testes ainda pendentes |
| TCG-R05 | 03/11/2026 | Coordenacao enviou retomada a Desenvolvimento eTestes, com dependencias liberadas/artefatos exatos. Dev continua importacao real/cache/demo; Testes pode iniciar casosisolados imediatamente no snapshotcompilado. Banco nao reaberto semdefeitoSQLconcreto, evitando repetir migraçõesconcluidas. Nenhum novoresultadoaprovado foi inferido da retomada |
| TCG-R06 | 03/11/2026, acompanhamento22:47UTC | Importacao real da demonstracao concluiu22:46:20UTC: importacao-resultado.json registra207processados/importados,207imagenslocais,0ausentes e0fallbackingles. ConsultaSQL registra207referenciasdetalhadaspt/importacaoCONCLUIDAsem pendentes e3vinculos. imagens-cache.json registra207arquivos(4017294bytes). preservacao-importacao.json compara12tabelascomerciais comtodascolunas/rowversions/hash/contagens e registraiguais=true. Fonte das evidencias: src/evidencias/tcgdex151/. Confirmadoimportreal,nãoaceitefinaldeoffline/repetição/regressão; Desenvolvimentoverificaessescriterios eTestes segueativoemPokeVaultTests |
| TCG-R07 | 03/11/2026, acompanhamento22:58UTC | Versao finalD226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C liberada schema10/snapshot. Devcorrigiu validacaorespostaAPI antesde substituircache (165/207/IDs/precos/URLs) e rotulohistorico compara datas exatamente. Testes preservouidentity01 com1reprovacao de rotulohistorico naA68 e reteste, depoisexecutou finalD226. Coordenacao leu live-final23aprovados, offline-final2aprovados e regressao-final45aprovados; sao conjuntos separados,naosomar repeticoes. Relatoriosfinais/reconciliacao em consolidacao,semnovo bloqueio paraDaniel |
| TCG-R08 | 03/11/2026 | Coordenacao avisou Desenvolvimento/Testes que resultadosfinais foram lidos e pediu fechamento finito dos relatorios/limites/manifestos sem repetir cenarios aprovados semmudanca. Nenhuma implementacaoextra ou retoma de melhoriasadiadas autorizada |
| TCG-R09 | 03/11/2026, acompanhamento23:08UTC | Coordenacao leu os tres relatorios finais e snapshots individuais confirmaram chats concluidos. Na DLLD226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C: live-final23aprovados, offline-final2aprovados, regressao-final45aprovados; hashes e versao do produto conferidos. Nenhum resultadoA68 atribuido como aceite daD226 |
| TCG-R10 | 03/11/2026 | Medicao-reteste.json observado com exit0:207referenciaslogicas/detalhadas/pt,0sempricingprincipal,3940camposnumericos/344nulos/54zeros reais,0precossemorigemdata/0foradoalvo e0triggerstemporarias. Zeros/nulos sao campos recebidos, nao cartasnem precoBRL. Fonte intermediaria e erroCTEleitura preservados |
| TCG-R11 | 03/11/2026 | Conferencia final da coordenacao: /health/ready retornou200, aplicacaoPokeVault pronta/schema10; DLLno disco confereD226. Biblioteca disponibilizada aDaniel por link de teste na conversa. Operacao emPokeVaultTests encerrada pelas frentes; demo5187 ativa, sem nova contaWindows/infraestrutura |
| TCG-R12 | 03/11/2026 | Balanco consolidado e automacao de acompanhamento removida apos concluir a entrega; ferramenta confirmoudeleteStatus=deleted. Nao ha nova rodada/mobile iniciada nem verificacoesperiodicas pendentes desta integracao |
| TCG-R13 | 03/11/2026 | Daniel relata manualmente: “Consegui comprar a carta!”, apos orientacao para cadastrar oferta da bibliotecaTCGdex e comprar como cliente. Compra bem-sucedida relatada pelo usuario; carta/CompraId/quantidade/valores e capturas nao informados, sem atribuir numeros do exemplo como observados ou alegar conferencia independente de saldo/estoque/estorno |
| TCG-R14 | 03/11/2026 | Apos orientacao de cancelamento administrativo e conferencia de saldo/estoque/historico, Daniel responde “Perfeito! Funcionou! E agora?”. Registrar sucesso do cancelamento conforme relato manual, sem inferir IDs/valores ou nova verificacao independente. Proxima conversa proposta: definir fluxo de compras/UX e revisar backlog das melhorias adiadas; nenhum novo desenvolvimento/mobile autorizado apenas por esta pergunta |

### Balanço final da integração TCGdex151 — 03/11/2026

**Entregue e verificado:** biblioteca local da coleção151 (sv03.5),165cartas oficiais/207totais, com207referênciasdetalhadas e207imagensWebP em português. Referências e fornecedores/preços são separados das ofertasvendáveis do PokéVault. Vinculadas com identidade conferida as três ofertasoriginais(PikachuNM/SP eCharmanderNM), sem mudar preçoBRL, estoque, contas, saldos ou histórico. Comparação das12tabelascomerciais durante a importação confirmou igualdade; isso não impede operações legítimas posteriores de Daniel.

| Cartão | Balanço | Evidência / limite |
| --- | --- | --- |
| TCG-C01 | Concluído | Recorte, fontes primárias, integração dos relatórios e este diário |
| TCG-B01 | Concluído | Contrato e migrações019–021/schema8–10 aplicados após4; cinco tabelas e cinco procedures; preservação12tabelas e liberação antes da massa deTestes. WIP014–018 não aplicado |
| TCG-D01 | Concluído | Importador REST restrito151, progresso/retomada/cache, biblioteca207, imagenslocais, preçosorigem/moeda/variante/data e tratamento deausência/histórico. README/launcher prontos para schema10 |
| TCG-D02 | Concluído | Vínculosseguros das ofertasoriginais e cadastro de uma oferta selecionada com metadados, exigindo preçoBRL/conservação/estoque locais |
| TCG-T01 | Concluído nos conjuntos executados |25/25TCGdex(23online/SQL/fixtures+2offline) e45/45regressão na DLLfinalD226; evidências em docs/evidencias/tcgdex151/ |

A primeira importação da demonstração terminou em aproximadamente25,10s; repetição com cache concluiu em aproximadamente0,95s, mantendo mesmoID1 e207referências/imagens. Reentrância durante execução recusou409. Imagens somam4.017.294bytes nesta coleta;0ausências/fallbackinglês observados. O fallback inglês e preçosausentes foram exercitados por fixtures, não inferidos do contadorzero. Biblioteca oferece busca/paginação/detalhe com fornecedores, variantesdinâmicas como reverse-holofoil e dadosoriginais emEUR/USD. Ausência/null é Não informado; um zero só é exibido quando efetivamente recebido da fonte.

**Fluxo para demonstração:** entrar como administrador ou atendente em http://localhost:5187/gestao/tcgdex151, consultar as207cartas e abrir detalhes. Ao cadastrar oferta pela referência, metadados vêm da biblioteca; o operador informa conservação, preço de venda emreais e quantidade. Só essa oferta solicitada entra no estoque local. Importação isolada não cria207produtosvendáveis. Catálogo/compra/estorno consultam dadoslocais e não chamamTCGdex no fechamento. Cliente/anônimo não podem disparar importação.

**Verificações substantivas:** importação real, manifestação idempotente e repetição semduplicar; integridadecomercial antes/depois; autorização/CSRF/alvofixo; campos/variantespreço dinâmicos; cache preservado com resposta inválida/timeout/HTTPfalho; ausência depreço sem virarzero; imagemindisponível comfallback; histórico depreço anterior identificado; vínculo por identidade e recusa dedivergência; cadastro só por solicitação; funcionamento de biblioteca/imagens e compra/estorno semredeTCGdex. Regressão45 inclui concorrência, rollback e gestõesexistentes. Suíte executada emPokeVaultTests comprocesso/caches próprios5188; não alterou demonstração5187. Fixtures usam substituiçãoHTTP emTestes, sem backdoor de falha no produto ou alteraçõesglobais.

**Correções e tentativas preservadas:** erroRazor inicial corrigido antes de liberarbuild; rótulohistóricoA68 falhava em intervaloscurtos e naD226 passou a comparar datas exatamente (casofinalcom0,028s); validaçãoHTTP200 antes de substituir cache foi reforçada e testada commanifesto/pricinginválidos. Gate recusou hashdivergente, sem iniciarregressão na versãodesatualizada. MediçãoSQLCTE240 tevecastcorrigido e reteste somenteleitura comexit0. Tentativas/versõesintermediárias e reprovações não foram apagadas ou reclassificadas retroativamente como aprovações.

**Versão final conferida:** D226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C, build deproduto semavisos/erros conforme Desenvolvimento. Testes não recompilou produto; manifests live-final/offline-final/regressao-final identificam a mesma DLL. Os12checksHTTP próprios de Desenvolvimento e repetições históricas não são somados aos25/45 conjuntos. Relatórios: entrega-tcgdex151-banco.md, entrega-tcgdex151-desenvolvimento.md e entrega-tcgdex151-testes.md. Origem/cache reais e manifestos preservados nas pastas de evidência de cada frente.

**Limites mantidos:** nenhuma inspeçãovisual automatizada abrangente de navegador/teclado/responsividade foi executada; bloqueioCUAanterior respeitado. Sem ensaio decarga/falhafísica/garantiauniversal. Fallbackinglês foi simulado, não necessário na primeira coleta real. Preços/datasecontagens são fotografia da coleta, não garantia de disponibilidade futura. WarningNU1900 do harness indica consultaNuGet indisponível, não auditoria de dependências aprovada. CredencialWindows atual aceita para usoacadêmicolocal; sem hospedagem/contadedicada obrigatórias.

As melhorias olho/coleçõesgenéricas/raridadesfechadas/novo fluxo de carrinho continuam adiadas; mobile e trabalhoacadêmicofinal não foram iniciados. Integração específica encerrada; não iniciar nova etapa automaticamente. O acompanhamento periódico desta entrega deixou de ser necessário após a consolidação.

## Sprint 3 — integração Capacitor

**Situação:** proposta, não iniciada. Datas em aberto. Objetivo: mesmo núcleo e base usados por web e aplicativo.

Prioridade revista em03/11/2026: mobile por ultimo, apos fechamento da web e posteriores discussoes de fluxo/melhorias; Daniel pretende deixar parte para os colegas. Nao iniciar automaticamente nem presumir participacao/contribuicoes ja realizadas.

Banco valida consultas/transações sem criar base paralela; Desenvolvimento prepara alvo mobile e contrato seguro de autenticação; Testes/documentação verifica compra no aplicativo e cancelamento refletido entre canais; Coordenação registra ambiente, dependências e decisões. Problemas a verificar: ferramentas Android, endereço da API e sessão entre origens. Não são falhas já observadas. Conclusão exige execução real no alvo escolhido e evidências separadas de navegador.

## Sprint 4 — consolidação acadêmica e demonstração final

**Situação:** proposta, não iniciada. Datas em aberto. Banco consolida modelo/scripts reais; Desenvolvimento corrige defeitos e finaliza instruções; Testes/documentação consolida evidências, limitações e revisão do texto/diagramas; Coordenação confronta backlog e critérios e registra o encerramento real.

Buyback e relatórios continuam candidatos posteriores, sem compromisso de implementação nesta sprint. Conclusão exige demonstração reproduzível, referências entre evidências e alegações do escrito e pendências explícitas. Nenhum problema ocorrido ou resultado de encerramento existe nesta abertura.

## Modelo de nova entrada e encerramento

Para cada nova entrada: ID; data real/fuso; sprint; frente; ação ou acordo; arquivos/versão; resultado observado; problema e encaminhamento; evidências; próximo passo. Referências são preenchidas somente com artefatos existentes. Coordenação mantém o diário; as frentes fornecem resumos verificáveis.

Ao encerrar cada sprint, registrar: objetivo atingido/parcial/não atingido; cartões concluídos e respectivos aceites; pendências transferidas com motivo; defeitos conhecidos; testes aprovados/reprovados/bloqueados/não executados; mudanças de escopo; efeitos no trabalho escrito; data real do encerramento. Não preencher esses resultados antecipadamente.


## Toques finais da web — fluxo e Gestão de Catálogo151

**Registro real:03/11/2026, America/Sao_Paulo, aproximadamente21h.** Rodada no chat de coordenação, conforme nova determinação de Daniel; sem delegação, automação nova ou abertura de sprint. Trabalho escrito permanece adiado.

- **WEB151-01 — Acordo/implementação:** Gestão de Catálogo passa a reunir a biblioteca207/165, com formulários de versões abaixo das cartas: idioma, estado, Normal/Holo/Reverse holo, unidades e preçoBRL. Edição de versão existente usa valores absolutos e versão esperada; sem justificativa manual, mantendo auditoria automática. Zero unidades tira a versão da vitrine. Média Cardmarket compacta com moeda/data quando disponível, sem conversão para BRL. Link técnico de detalhes/preços saiu da navegação. Vitrine pública de cartas em estoque, detalhe por carta/versões, Comprar cumulativo, sacola junto ao nome e carrinho com identificação/variantes/preços/totais. Seleção parcial, confirmação, saldo fictício e estorno preservados.
- **WEB151-02 — Banco/preservação:** script024/schema11 aplicado primeiro em PokeVaultTests, depois PokeVault. IDs e atributos comerciais preexistentes preservados; idioma/acabamento antigos não foram inventados. Novas compras guardam snapshots. Script025 confirmou hashes idênticos de todas as colunas anteriores das12 tabelas comerciais, incluindo versões. Migrações014–018 continuam suspensas.
- **WEB151-03 — Problema/reteste:** build inicial encontrou DLL em uso pelo processo demo; processo do projeto foi identificado, encerrado e reiniciado após validação. Primeiro ensaio da regressão44/45: teste ainda exigia200 na gestão antiga, que passou a redirecionar302. Expectativa atualizada para conferir destino e tela unificada200; resultado inicial preservado. Final **45/45 regressão +10/10 novo fluxo**, mesmo SHA256 `1F34AC2895889603D276A39C971AD2ADBE7847A27B8663C02966D03389C6C089`. Testes reais em SQL/HTTP/MVC incluem soma concorrente, versões independentes, snapshots, seleção/remanescente, replay/estorno e recusa de sobrescrita. Harness avisou indisponibilidade da consulta NuGet de vulnerabilidades (NU1900); build do produto0 erros/avisos. Não houve mudança global para contornar o aviso.
- **WEB151-04 — Disponibilização/limites:** demonstração reiniciada em5187, readiness200/schema11 e home200 com2 cartas agrupadas em estoque naquele instante. Nenhum dado demonstrativo foi resetado. Telas verificadas por HTML/HTTP; inspeção visual e execução visual do total JavaScript ainda dependem de teste manual. Respeitado o bloqueio anterior de automação do navegador. Não declarar aceite de Daniel antes de sua revisão. Permanecem adiados olho, coleções genéricas, raridades fechadas, mobile e reescrita final.

Evidências e instruções: `docs/entrega-fluxo151.md`, `docs/evidencias/fluxo151/fluxo151-final/`, `docs/evidencias/fluxo151/regressao-fluxo151-final/`, `docs/evidencias/fluxo151/demonstracao/`. Relatórios anteriores preservados; horários UTC nos JSON correspondem ao dia03/11 local mesmo quando indicam04/11 UTC.


- **WEB151-05 —03/11/2026, aproximadamente21h19, America/Sao_Paulo:** Daniel informou que o fluxo funcionou muito bem e solicitou quatro ajustes de apresentação. Implementados olho para senha no login/cadastro de usuários,207 cartas em página única, todos os controles de estoque recolhidos em Colocar em estoque e média apenas com valor/moeda. Estado inicial oculto, controles type=button com aria-label/aria-pressed e ícones locais; menus usam details/summary nativos. Não houve migração ou alteração de contas/estoque/histórico na demonstração. Build0 erros/avisos; sintaxe JavaScript validada por node --check;10/10 cenários do fluxo em PokeVaultTests aprovados, incluindo HTML de207 menus fechados e três telas de senha. Mesmo aviso NU1900 do harness, sem bloquear execução. Evidências em `docs/evidencias/fluxo151/acabamento-visual151/`, com manifesto/hash e verificação HTTP da demonstração reiniciada. As referências anteriores a9 páginas/olho adiado descrevem o estado anterior e foram superadas por esta autorização. A interação visual do olho e a revisão de aparência continuam para conferência manual; não foi contornado o limite anterior do navegador. Sem nova sprint ou delegação.


- **WEB151-06 —03/11/2026, aproximadamente21h32, America/Sao_Paulo:** Daniel solicitou cadastro apenas email/senha e troca de senha nos três perfis. Implementadas rotas/criar-conta e/conta, cadastro CLIENTE fixo com saldo0/carrinho vazio, hash e email normalizado único; troca própria com senha atual/confirmada, hash esperado contra concorrência e renovação da sessão/invalidação das outras. Olho de senha nos novos campos. Migração026/schema12 aplicada em testes antes da demonstração;12 tabelas comerciais integralmente idênticas antes/depois. Nenhuma senha da demonstração foi trocada pelo agente; Daniel fará suas escolhas. Build produto0 erros/avisos; primeiro build do harness bloqueou por campos html/text nos helpers de formulário, corrigido sem execução de cenários naquele ensaio. Final10/10 contas e45/45 regressão no mesmo produto SHA25664F1DECDB77CBC6F5C0126C18864A8EC50E2986C6A702E6B06F7FBD0BC01514A. Aviso NU1900 do harness sem impedir testes. Demonstração5187 pronta/schema12; telas validadas via HTML/HTTP, sem alegação visual. Evidências em docs/evidencias/contas e entrega-contas.md. Avaliação do escrito feita pela extração existente tmp/pdfs/pokevault.txt: há material para relato web completo, mas texto antigo contém alegações a corrigir (triggers/prazo3 dias/mobile/HandTalk/nuvem/Docker). Mapa em docs/revisao-trabalho-apos-web.md; não reescrito o PDF nem inventadas participações/conclusões individuais. Mesma coordenação, sem nova sprint ou delegação.


## Reescrita acadêmica iniciada em04/11/2026

**ACA-R01 —04/11/2026, America/Sao_Paulo:** Daniel enviou PIM IV Em Desenvolvimento.pdf e autorizou começar a reescrita com base no sistema entregue. PDF de36 páginas extraído e lido, confrontado com código/contratos/entregas e registros existentes. Documento PIM IV Pokévault localizado e consultado no Google Drive somente para leitura; original compartilhado preservado. Produzida primeira versão DOCX separada,36 páginas, com resumo/abstract, capítulos1–6,8 quadros,4 diagramas novos, referências oficiais consultadas e2 apêndices. Modelo de dados e fluxo reescritos conforme schema12, variantes, cadastro/troca de senha, TCGdex e compra/estorno; mobile descrito como pendente, sem alegar nuvem/HandTalk/triggers de produção. Participações e conclusões individuais não inventadas. Texto-fonte em docs/academico/PokeVault_reescrita_2026-11-04.md; entrega em output/documents/PokeVault_trabalho_reescrito_2026-11-04.docx. Renderizador padrão não encontrou LibreOffice; usado Microsoft Word COM oculto para atualizar sumário e renderizar PDF interno, Poppler para PNG e inspeção de todas36 páginas. Bordas e quebras foram revistas; após retoques finais, imagens inalteradas comparadas pixel a pixel e páginas alteradas reinspecionadas. Manifesto/hash em docs/evidencias/reescrita-academica-20261104/manifesto.json. Nenhum código do sistema ou banco alterado; nenhum novo teste funcional executado nesta revisão. Pendências: relatos/conclusões dos colegas, capturas finais da interface, validação com manual/grade da instituição e mobile posterior. Não declarar documento final de submissão nem atendimento integral ao enunciado ainda não fornecido.
