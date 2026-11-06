# PokéVault — planejamento de coordenação

> Prioridade atual, confirmada por Daniel em03/11/2026: fechar a WEB para demonstracao academica LOCAL, sem hospedagem externa nem conta Windows dedicada obrigatoria. Mobile por ultimo e sem inicio automatico; melhorias olho/colecoes/raridades/fluxo aditivo adiadas. O backlog e as sprints abaixo preservam o planejamento original; situacao efetiva e criterios atuais estao em DECISOES.md e diario-sprints.md, secao Fechamento web. As entregas das sprints1/2 ja estao registradas, com limites reais de cobertura.

> Atualização de 21/10/2026: os quatro planos foram lidos e conciliados. A situação inicial de planos pendentes registrada abaixo é histórica. Para a execução, prevalecem a rodada de implementação de `DECISOES.md` e `diario-sprints.md`. Daniel confirmou catálogo público, gestão de catálogo/estoque por atendente, saldo por substituição e checkout de itens selecionados, mantendo os demais no carrinho. As propostas antigas de checkout integral e de negar essas ações enquanto pendentes foram substituídas. A primeira sprint foi autorizada; seus resultados ainda precisam de comprovação. PV-01 está concluído quanto à conciliação; contratos detalhados das procedures são tarefa de banco antes da integração.

## Base e situação deste plano

Este documento propõe a organização do trabalho com base em `docs/DECISOES.md`, fonte do escopo aprovado por Daniel. O texto extraído em `tmp/pdfs/pokevault.txt` foi consultado como referência acadêmica; suas descrições de funcionalidades prontas, reuniões e testes não comprovam execução.

Esta rodada produz somente planejamento, com apoio de IA. Nenhuma implementação, execução de testes, reunião ou conclusão de funcionalidade é registrada aqui. Não foi criado quadro Trello externo. Os chats representam frentes de responsabilidade, sem atribuição a pessoas fictícias.

Na inspeção desta frente, apenas `docs/DECISOES.md` estava disponível em `docs/`. Os planos de banco, desenvolvimento e testes/documentação permanecem pendentes de leitura e conciliação. O backlog abaixo é uma proposta, não um registro de progresso. Datas, duração das sprints, capacidade e responsáveis humanos estão a definir.

## Resultado pretendido e limites

A demonstração deve permitir autenticar usuários, consultar cartas e suas conservações, montar um carrinho, concluir uma compra com saldo fictício e cancelar essa compra como administrador, devolvendo saldo e estoque uma única vez e preservando o histórico.

Tecnologias previstas: ASP.NET Core MVC, API REST, SQL Server e aplicativo com Capacitor. O primeiro incremento será demonstrável localmente pela web; a integração mobile será proposta para uma sprint posterior. Isso não retira API e Capacitor do planejamento.

Regras aprovadas que orientam todas as entregas:

- Perfis de administrador, atendente e cliente; credenciais fixas do administrador ainda serão escolhidas.
- Mesma carta em conservações diferentes tem registros comerciais, preços e quantidades independentes.
- Compra simulada desconta saldo e estoque; compra aprovada fica concluída. O cancelamento administrativo altera o estado para cancelado e estorna uma única vez.
- Administrador pode criar clientes, definir saldo fictício, criar e excluir/desativar funcionários e reverter compras.
- Catálogo, filtros, carrinho e histórico integram a demonstração proposta.
- Reserva por três dias, retirada/envio e pagamento bancário real estão fora do escopo. Não entram como estados ou tarefas das sprints.
- Buyback e relatórios ficam depois do fluxo principal, sujeitos a refinamento. Nuvem, CI/CD, HandTalk e precificação externa citados no PDF não se tornam compromissos desta rodada sem alinhamento de escopo.

## Product backlog priorizado

P0: necessário ao primeiro fluxo completo. P1: complementa a demonstração aprovada após o núcleo. P2: evolução a avaliar depois do núcleo. A ordem dentro de cada prioridade considera dependências, não prazo. Todos os itens estão **propostos, sem início de implementação comprovado**.

| ID | Prioridade | Entrega proposta | Critério de conclusão verificável | Dependências |
| --- | --- | --- | --- | --- |
| PV-01 | P0 | Conciliar os quatro planos e registrar os contratos do núcleo | Modelo, permissões, operações de compra/estorno e roteiro de aceite sem conflitos; decisões pendentes identificadas | Planos das outras frentes |
| PV-02 | P0 | Preparar solução e execução local | Aplicação web e API iniciam, conectam ao SQL Server e possuem instruções reproduzíveis de configuração, sem segredos no repositório | PV-01; ambiente a verificar |
| PV-03 | P0 | Persistência e massa mínima de demonstração | Base criada de forma reproduzível; contém perfis, cliente de demonstração e mesma carta em duas conservações com preço/estoque distintos; permite compra, itens e histórico de cancelamento | PV-01; plano de banco |
| PV-04 | P0 | Login, logout e autorização | Login válido e inválido verificados; logout encerra acesso; rotas web e API protegem ações por perfil e dados do cliente; administrador usa credenciais fixas escolhidas para demonstração | PV-02, PV-03; matriz de permissões |
| PV-05 | P0 | Catálogo e filtro inicial | Lista apresenta nome, conservação, preço e quantidade; filtro mínimo proposto por nome e conservação funciona; itens sem estoque não são compráveis | PV-03; contrato de consulta |
| PV-06 | P0 | Carrinho mínimo | Adicionar, alterar quantidade e remover itens funciona; conserva a identidade do registro comercial; quantidades inválidas são recusadas; total exibido é recalculado pelo servidor na compra | PV-04, PV-05 |
| PV-07 | P0 | Compra simulada consistente | Compra válida gera pedido concluído, itens e descontos de saldo/estoque em uma operação atômica; saldo ou estoque insuficiente e falha intermediária não deixam alterações parciais | PV-03, PV-04, PV-06; estratégia transacional |
| PV-08 | P0 | Histórico mínimo do cliente e consulta administrativa | Cliente vê apenas suas compras, itens, valores e estado; administrador localiza compra para cancelamento; preço da compra permanece preservado após alteração do catálogo | PV-04, PV-07 |
| PV-09 | P0 | Cancelamento e estorno administrativo | Somente administrador cancela compra concluída; saldo e quantidades originais retornam uma vez; repetição ou concorrência de cancelamentos não duplica devolução; pedido e itens continuam consultáveis | PV-07, PV-08; estratégia transacional |
| PV-10 | P0 | Validar e demonstrar o fluxo da primeira sprint | Roteiro de ponta a ponta executado com evidências reais; falhas bloqueadoras resolvidas; instruções locais permitem repetir a demonstração | PV-04 a PV-09; plano de testes |
| PV-11 | P1 | Administração de clientes, funcionários e saldo | Administrador cria cliente, define saldo fictício e cria/desativa ou exclui funcionário conforme política conciliada; demais perfis não acessam essas operações; histórico referenciado permanece íntegro | PV-04, PV-09; decisões sobre saldo e exclusão |
| PV-12 | P1 | Preparação de catálogo/estoque e filtros complementares | Forma acordada de cadastrar/atualizar registros comerciais funciona; estoque e preço independentes por conservação; filtros adicionais acordados, como coleção e raridade, retornam resultados coerentes | PV-03, PV-05; permissões e campos a confirmar |
| PV-13 | P1 | Completar a operação do perfil atendente | Somente as ações expressamente acordadas na matriz de permissões estão disponíveis; bloqueios administrativos são verificados | PV-01, PV-04; definição do papel do atendente |
| PV-14 | P1 | Aplicativo Capacitor integrado à API | No dispositivo ou emulador escolhido, login, catálogo, compra e consulta de histórico usam a mesma base; cancelamento feito na web aparece no aplicativo; regras do servidor não são duplicadas de forma divergente | PV-10; contrato da API e ambiente mobile |
| PV-15 | P1 | Consolidar evidências e material acadêmico | Modelos, diagrama, instruções e texto descrevem a versão efetivamente demonstrada; cada alegação de teste remete a execução registrada; limitações ficam explícitas | Incrementos efetivamente validados; frente de testes/documentação |
| PV-16 | P2 | Refinar buyback | Proposta separada define entrada de estoque, eventual efeito no saldo, permissões e critérios; implementação só entra em sprint após alinhamento | Núcleo concluído; confirmação de escopo |
| PV-17 | P2 | Refinar relatórios | Perguntas e cálculos necessários à demonstração definidos; se selecionado, relatório reconciliado com compras e cancelamentos, sem contar venda cancelada como receita realizada | Núcleo concluído; confirmação de escopo |

Propostas como filtro mínimo, forma de carga e gestão de catálogo precisam ser conciliadas com as demais frentes. Não representam novas decisões de produto já aprovadas.

## Critérios comuns para iniciar e concluir

Um cartão está pronto para iniciar quando tem objetivo, escopo, dependências, regra de permissão e critério de aceite claros. Pendência que altere sua regra de negócio precisa ser resolvida antes da implementação dependente. As demais atividades podem seguir quando independentes.

Para considerar uma entrega concluída:

1. O comportamento atende ao critério do cartão e às decisões aprovadas, com integração à base e ao fluxo pertinente.
2. Validações ocorrem no servidor; restrições de acesso não dependem apenas de ocultar botões.
3. Verificações pertinentes foram realmente executadas e registradas com ambiente, passos/dados, resultado esperado, observado e referência à versão. Teste planejado não conta como teste aprovado.
4. Não há falha aberta que comprometa autenticação, autorização, integridade de saldo/estoque ou estorno único naquele incremento.
5. Instruções e documentação afetadas refletem o comportamento entregue e suas limitações. Há evidência suficiente para repetir a demonstração.

Conclusão de planejamento significa que existe uma proposta revisável. Não significa conclusão dos cartões de implementação. Nesta rodada nenhum cartão de sistema deve ser marcado como concluído por causa deste arquivo.

## Primeira sprint proposta — login, catálogo, compra e estorno

**Meta:** demonstrar localmente o ciclo cliente → compra concluída → cancelamento pelo administrador, com dados persistidos e consistentes. Duração e datas ficam em aberto até verificar ambiente, planos e capacidade; o compromisso deve ser revisado antes de iniciar.

**Entrada:** PV-01 conciliado; permissões mínimas e credenciais de demonstração definidas; estratégia para transações e repetição de requisições acordada; SQL Server e SDK disponíveis ou impedimento registrado. Planejar não comprova que o ambiente está pronto.

| Ordem | Recorte selecionado | Saída verificável esperada |
| --- | --- | --- |
| 1 | PV-02 e PV-03: base técnica e dados mínimos | Web/API conectadas à base local; preparação reproduzível com administrador, cliente e cartas em duas conservações; perfil atendente representado para verificar bloqueios |
| 2 | PV-04: acesso | Cliente e administrador autenticam; credencial inválida falha; sessão encerrada perde acesso; operações administrativas recusadas para outros perfis |
| 3 | PV-05 e PV-06: selecionar produtos | Cliente filtra catálogo e monta carrinho com quantidades válidas e identificação inequívoca da conservação |
| 4 | PV-07: confirmar compra | Pedido concluído com valores calculados no servidor; saldo e estoque reduzidos juntos; rejeições não geram débito ou baixa parcial |
| 5 | PV-08 e PV-09: consultar e reverter | Compra visível no histórico; administrador cancela; valores e quantidades originais retornam uma única vez, mantendo itens e registro do cancelamento |
| 6 | PV-10: integrar e verificar | Demonstração repetível, evidências reais e defeitos bloqueadores resolvidos |

Para conter o recorte, propõe-se massa inicial controlada para usuários, saldo e catálogo. Telas completas de administração ficam na sprint seguinte; isso depende de concordância técnica na conciliação dos planos. O histórico mínimo já entra nesta sprint porque sustenta a compra e seu estorno. A API deve compartilhar as regras do núcleo, com o desenho de endpoints definido pela frente de desenvolvimento.

### Roteiro de aceite proposto para a sprint

Estes cenários estão **planejados, não executados**. A frente de testes deve detalhar os dados e a forma de coleta de evidências.

- Autenticar cliente e administrador; rejeitar senha inválida e acesso sem autenticação às operações protegidas.
- Consultar a mesma carta em duas conservações; comprar uma delas e confirmar que a outra mantém preço e estoque independentes.
- Concluir carrinho com mais de um item e verificar pedido, itens, total, saldo e estoque antes/depois na base persistida.
- Recusar saldo insuficiente, estoque insuficiente, quantidade inválida e falha durante a transação sem efeitos parciais. Revalidar preço e disponibilidade no servidor ao confirmar.
- Disputar a última unidade com compras concorrentes: no máximo a quantidade disponível é vendida, sem saldo/estoque negativos. Reenvio da confirmação não deve cobrar duas vezes a mesma operação; mecanismo a definir pelas frentes técnicas.
- Impedir cliente de consultar compra de outro cliente ou cancelar compra; impedir atendente de executar estorno administrativo.
- Cancelar como administrador e conferir devolução exata dos valores e itens originais. Repetir e concorrer pedidos de cancelamento: nenhuma devolução adicional.
- Consultar compra cancelada após nova sessão: estado, itens e valores permanecem no histórico. Logout invalida o acesso protegido.

**Saída da sprint:** PV-02 a PV-10 atendem aos critérios do recorte e aos critérios comuns de conclusão, com roteiro local e evidências. Se houver item incompleto, registrá-lo como pendente e replanejar; não declarar o fluxo concluído. Se a capacidade exigir redução, renegociar o compromisso preservando compra/estorno e integridade como unidade de demonstração.

## Próximas sprints propostas

Sequência condicionada à validação da anterior; sem datas ou progresso presumido.

| Sprint | Objetivo e seleção proposta | Condição de saída |
| --- | --- | --- |
| 2 — Administração e operação | PV-11 a PV-13: cadastros, saldo fictício, funcionários, manutenção comercial e filtros complementares; fechar ações do atendente | Demonstração pode preparar e operar seus dados pela forma acordada; permissões e preservação do histórico verificadas; fluxo da sprint 1 continua funcionando |
| 3 — Integração mobile | PV-14: Capacitor consumindo a API do núcleo; PV-15 parcialmente, com instruções de execução web/mobile e diagrama atualizado | Mesmo saldo, estoque e pedidos observados pelos dois clientes; evidências no alvo mobile escolhido e regressão do núcleo |
| 4 — Consolidação acadêmica | PV-15: revisar texto, modelos e roteiro final a partir da implementação e dos testes reais; corrigir defeitos restantes | Demonstração reproduzível e documentação coerente com o entregue; pendências e limites explícitos |
| Evolução posterior, a selecionar | Refinamento de PV-16 e PV-17; avaliar outros itens do PDF somente se necessários e alinhados | Escopo, prioridade e aceite definidos antes de assumir implementação; não comprometer a entrega principal |

A documentação e as evidências devem acompanhar cada sprint. A sprint 4 consolida esse material, sem adiar o registro de testes ou decisões até o final. Nenhuma sprint pressupõe publicação em nuvem.

## Dependências entre frentes

| Frente / artefato esperado | Insumo necessário para coordenação | Situação e encaminhamento proposto |
| --- | --- | --- |
| Banco — `docs/plano-banco.md` | Modelo de usuários/perfis, carta e registro comercial por conservação, saldo, compra/itens e cancelamento; histórico; restrições; atomicidade e concorrência; carga inicial | Pendente na inspeção. Conciliar com PV-03, PV-07 e PV-09 antes de implementar persistência |
| Desenvolvimento — `docs/plano-desenvolvimento.md` | Estrutura MVC/API, autenticação, matriz de autorização, carrinho, contratos e tratamento de falhas/reenvios; configuração local e caminho para Capacitor | Pendente na inspeção. Conciliar com PV-02 e PV-04 a PV-14 |
| Testes/documentação — `docs/plano-testes.md` | Casos de aceite, dados controlados, verificações de concorrência e estorno único, formato de evidências e atualização do trabalho escrito | Pendente na inspeção. Vincular cenários aos IDs do backlog antes de declarar incrementos concluídos |
| Coordenação — este arquivo | Ordem das entregas, dependências, critérios de passagem e decisões em aberto | Proposta inicial para revisão; não altera o escopo central |

Compra e estorno precisam de uma estratégia única para atualizar saldo e estoque. Se houver procedures ou triggers, seu papel deve ser conciliado com a aplicação para evitar baixa ou devolução duplicada. A escolha técnica pertence aos planos de banco e desenvolvimento; o critério de integridade permanece obrigatório.

## Pendências e impedimentos a verificar

São pontos em aberto, sem afirmar que impedimentos técnicos já ocorreram.

| Pendência | Impacto | Encaminhamento proposto |
| --- | --- | --- |
| Receber e conciliar os três planos | Contratos e recorte da sprint ainda não fechados | Revisar divergências antes de iniciar cartões dependentes; manter decisões de produto em `DECISOES.md` pela frente autorizada |
| Escolher login e senha fixos do administrador | Preparação do acesso demonstrativo | Daniel define credenciais; desenvolvimento define armazenamento/configuração sem senha exposta no código |
| Definir ações do atendente e acesso ao catálogo sem login | Autorização e navegação ainda parcialmente abertas | Propor matriz por operação; confirmar o que não estiver definido, mantendo estorno exclusivo do administrador |
| Escolher exclusão ou desativação de funcionário | Risco de apagar referências históricas | Preferência proposta: desativar quando houver vínculos; conciliar a regra antes de implementar |
| Detalhar “definir saldo fictício” | Forma de ajuste e rastreabilidade | Especificar se é valor final ou crédito incremental e como registrar ajuste, evitando confusão com estorno |
| Definir carga e manutenção do catálogo, filtros e carrinho | Esforço e contratos entre camadas | Confirmar campos, lista de conservações, persistência do carrinho e política de preço na confirmação; preservar valor efetivamente comprado |
| Verificar SDK, SQL Server e alvo Capacitor | Execução local e mobile ainda não comprovadas | Fazer verificação na fase de implementação; registrar erro real, alternativa e dependência, sem assumir instalação disponível |
| Definir duração/capacidade e distribuição real do trabalho | Não há cronograma ou atribuição humana confirmados | Preencher apenas após organização real por Daniel; manter sequência por dependência até lá |
| Confirmar exigências acadêmicas adicionais do PDF | Possível pressão de escopo | Separar exigência acadêmica a esclarecer de funcionalidade aprovada; alinhar antes de adicionar entregas |

## Modelo de quadro Trello

Modelo documental, sem criação externa ou movimentações reais. As quatro listas propostas são:

| Lista | Regra de entrada | Regra de saída |
| --- | --- | --- |
| A fazer | Cartão priorizado, com dependências e aceite registrados; bloqueios explícitos na descrição | Trabalho efetivamente iniciado após resolver dependências necessárias |
| Em desenvolvimento | Implementação iniciada e escopo assumido | Entrega integrada, pronta para executar verificações e com instruções/dados de teste |
| Em teste | Validação realmente iniciada ou aguardando execução com artefato verificável | Aceite comprovado leva a Concluído; defeito exige correção e retorno a Em desenvolvimento |
| Concluído | Critérios específicos e comuns atendidos, com evidências vinculadas | Reabertura se for constatado defeito que invalide o aceite |

Não preencher listas de progresso por inferência. Ao montar o quadro futuramente, cartões ainda não iniciados entram em **A fazer**; esta proposta não prova movimentação. Sugere-se limitar trabalho simultâneo a um cartão por executor efetivamente disponível e priorizar desbloquear o fluxo principal. O limite será ajustado à capacidade real.

Cada cartão deve conter:

- **Título:** `[PV-xx] verbo + resultado`; desdobramentos podem usar `PV-07.1`, mantendo vínculo com o item pai.
- **Prioridade e sprint proposta:** P0/P1/P2 e seleção ainda não comprometida ou compromisso confirmado.
- **Frente:** coordenação, banco, desenvolvimento ou testes/documentação. Responsável humano somente quando realmente definido; até lá, “a definir”.
- **Objetivo e limites:** comportamento esperado e recorte do cartão.
- **Dependências e bloqueios:** IDs relacionados, decisão necessária e condição para desbloquear.
- **Checklist de execução:** passos técnicos acordados, sem marcar atividades não realizadas.
- **Aceite e evidência:** cenários, referência da versão, dados, resultado esperado/observado, arquivos ou capturas e defeitos vinculados.
- **Datas:** preencher somente com planejamento confirmado ou eventos efetivos; sem datas fictícias.

Exemplo de cartão ainda não iniciado:

> **[PV-09] Cancelar compra com estorno único**  
> Prioridade: P0. Sprint proposta: 1. Frente principal: desenvolvimento; dependência de banco e testes/documentação. Responsável humano: a definir.  
> Dependências: PV-07, PV-08 e estratégia transacional conciliada.  
> Aceite: administrador cancela compra concluída; saldo e estoque retornam exatamente uma vez, inclusive com requisições concorrentes; histórico permanece; cliente e atendente recebem recusa.  
> Evidências: pendentes de implementação e execução. Situação proposta no futuro quadro: A fazer.

## Uso deste plano na próxima rodada

O próximo passo é conciliar os planos pendentes, resolver decisões que bloqueiam o núcleo e transformar o recorte da sprint 1 em tarefas executáveis. Depois de cada incremento real, atualizar situação, dependências e evidências correspondentes. O planejamento pode apoiar o capítulo de gerenciamento do PIM, mas só resultados efetivamente registrados devem ser descritos como realizados.
