# Sprint 2 — testes reais e evidências

Registro real: 21/10/2026, America/Sao_Paulo. Executor desta frente: Codex com apoio de IA. Não foram atribuídas execuções a Daniel ou a integrantes humanos. Início de referência07/09/2026 não foi usado para retroagir registros. Prioridade desta rodada: sistema, testes e relatos técnicos; PDF/texto acadêmico final não foram reescritos.

## Resultado e contagem

**45 cenários executáveis distintos com último resultado aprovado:21 SQL, 18 HTTP e 6 MVC.** A aceitação é limitada às verificações codificadas; não equivale a executar integralmente os 36 casos conceituais T01–T36 nem a validar todo o sistema.

| Rodada preservada | Executados | Aprovados | Reprovados | Observação |
| --- | ---: | ---: | ---: | --- |
| rodada01-resultados.json | 32 | 32 | 0 | Primeira execução formal integrada isolada; ainda sem caso específico de auditoria sem delta |
| rodada02-resultados.json | 40 | 40 | 0 | Acrescenta valores inválidos de catálogo, formulários MVC e limpeza/ADM fixo |
| rodada03-resultados.json | 44 | 44 | 0 | Schema4 confirmado; auditoria de preço/ativação e rollback da auditoria |
| rodada04-resultados.json | 45 | 44 | 1 | HTTP18 esperava201 para campos de perfil extras; API corretamente recusou400 |
| rodada05-reteste-http18.json | 1 | 1 | 0 | Reteste focado: exigir400 e nenhuma conta criada, conforme JsonUnmappedMemberHandling.Disallow |

Último estado por ID:45 aprovados,0 reprovados,0 bloqueados **dentro deste harness**. As repetições não são cenários distintos adicionais. Foram162 execuções de cenários nas cinco rodadas,161 aprovadas e uma reprovação de expectativa do roteiro, preservada e corrigida. Não houve alteração do produto para fazer HTTP18 passar.

As referências anteriores da frente Desenvolvimento foram copiadas como `referencia-sprint1-desenvolvimento.json` (30 checks) e `referencia-sprint1-mvc.json` (7). São evidências externas históricas em PokeVault; não foram reexecutadas nem somadas aos45 cenários desta frente.

## Ambiente, versão e isolamento

- SQL Server real: `.\SQLEXPRESS`, servidor DESKTOP-641C55N\SQLEXPRESS, motor17.0.1000.7 Express. Base exclusiva PokeVaultTests, marcador PokeVaultProjeto=PIM-2026-Sprint1; versões 1/2/3/4 conferidas. Banco liberou a posse da massa em sua entrega.
- Aplicação: cópia dos binários compilados para `tests/runtime-sprint2`, com contentRoot no mesmo diretório, porta 5188, Development, `POKEVAULT_DATABASE=PokeVaultTests` e conexão Windows integrada específica. SHA256 da DLL: `9B717A9B37A733A9213284C93A6DE2FC574CB66D383281AF1C5B20B7175A5EDC`. Hash do binário de origem foi comparado e coincide com a cópia executada.
- Harness: console .NET 10 independente em tests/Sprint2; SqlClient 7.1.1 e PasswordHasher da plataforma; sem ProjectReference, sem transação externa envolvendo procedure, sem alterar solução/src/database. Hashes de fontes/harness/produto/scripts nos manifestos por rodada.
- Cada rodada criou administrador de instalação e contas/cartas próprias com identificadores aleatórios. Senhas geradas aleatoriamente apenas em memória; cadastro HTTP/MVC informa senha inicial, servidor gera hash. Credenciais da demonstração e arquivos DPAPI de demonstração não foram usados. Administrador fixo foi criado apenas se inexistente na base de testes.
- Não houve reset nem escrita desta frente em PokeVault; servidor 5187 não foi interrompido. A cópia5188 foi encerrada ao fim por PID/caminho exclusivos, com evidência em encerramento-processo-isolado.json. Massa/histórico de teste permanecem para rastreabilidade, sem fingir remoção de dados.

Conexão restrita inicialmente falhou com erro de criptografia/credenciais ODBC. Repetição autorizada fora da restrição funcionou, sem mudança global de serviço/protocolo/certificado/autenticação SQL. Não houve rejeição de revisão automática nesta rodada.

## Matriz executada e limites

A evidência de cada linha é o registro com o ID indicado em rodada04-resultados.json, exceto HTTP18, cujo resultado definitivo está em rodada05-reteste-http18.json. JSONs registram esperado separado de observado e IDs/dados úteis; fontes preservam os asserts. O manifesto correspondente identifica a versão.

| Requisito / critério | IDs executáveis | Resultado observado / limite |
| --- | --- | --- |
| Catálogo público, sessão e logout | HTTP01/HTTP13 | Catálogo200, protegido401, login próprio válido, senha errada401, logout204 e sessão posterior401 |
| ADM cria cliente e atendente; lista sem hash | HTTP02, MVC03/MVC04 | Criação201 via API e302 via formulário; cliente saldo 0/carrinho vazio; listagens sem senha/hash; hash validado em memória sem exposição |
| Definir saldo substitui e audita, sem crédito duplicado | SQL13, HTTP03/HTTP16, MVC06 | 0→100→40; delta−60; repetir40 mantém token e contagem de movimentos; autor/motivo conferidos; formulário persiste60,50 |
| Desativar/reativar atendente, proteger ADM | HTTP04/HTTP09, MVC04, SQL18 | Cookie corrente recebe401 após inativação; reativação permite novo login; alvo cliente/admin403; ADM fixo SQL51011; histórico mantido |
| Atendente cadastra/ajusta catálogo | HTTP05/HTTP15, MVC05 | Cadastro/ajuste permitido, preço35/estoque4; inativação fora do catálogo público; edição MVC15,50→20,50 e estoque1→4 |
| Autorizações e perfil imposto por payload | HTTP06/HTTP18 | Cliente/atendente403 em usuários/saldo/estorno; cliente403 no cadastro de carta; anônimo401; campos extras perfil/admin400 e nenhum cadastro |
| Antifalsificação | HTTP07 | Sem token em saldo/cadastro/ativação/catálogo e token de outra identidade recusados400; saldo preservado |
| Valores inválidos, duplicidade e token | HTTP08/HTTP09/HTTP14 | Saldo negativo/escala3/overflow, preço0/escala3, estoque negativo/fracionário, conservação inválida, token inválido/tamanho errado e motivo vazio400; duplicidades normalizadas/carta409 |
| Edição antiga não perde venda/ajuste | SQL14/SQL15/SQL16, HTTP12, MVC02/MVC05 | Venda invalida token de saldo/estoque;409/51012 preservam saldo e estoque; dois ajustes com mesmo token têm um sucesso e um51012; MVC409 pede recarga |
| Selecionados, conservações e remanescentes | SQL01, HTTP10 | Compra só NM30 deixa SP/B; compra NM+SP50 deixa B; saldos/estoques conferidos. SQL01 usa mesmo número/coleção em duas conservações |
| Quantidade corrente e recusas atômicas | SQL02/SQL10 | Estoque51008/saldo51009 sem débito/baixa/limpeza parcial; quantidade1 para linha2 recusa51005 |
| Preço alterado precisa novo aceite | SQL03, HTTP11 | Preço30→40 recusa51007/HTTP409 sem efeito; nova chave/preço confirmado40 aprova. Não houve inspeção visual do clique de confirmação |
| UUID sequencial/concorrente e conteúdo | SQL04/SQL07, HTTP10 | Reenvio retorna mesma compra; seleção em ordem invertida é equivalente; conteúdo divergente51004; concorrência gera uma compra/débito; cancelada não reativa |
| Última unidade/saldo concorrentes | SQL05/SQL06 | Duas conexões/barreira: uma compra15,50 da última unidade, outra51008; saldo30 disputado por cartas distintas30 produz um sucesso e um51009 |
| Estorno original, repetido e concorrente | SQL04/SQL08/SQL09, HTTP10 | Uma devolução; preço posterior40 não muda estorno30; saldo corrente40 recebe30 e termina70; motivo original não substituído pela repetição segundo contrato (valor do motivo não foi assertado separadamente no harness) |
| Histórico isolado | HTTP10 | Outro cliente recebe404 no detalhe da compra; não comprova paginação completa do histórico |
| Rollback após escrita | SQL11/SQL12/SQL20 | Erro51999 após INSERT de movimento/baixas ou auditoria, flag de alcance=1, valores/movimentos/pedido/carrinho revertidos conforme asserts; retry compra/cancelamento após retirar flag funciona |
| Auditoria sem delta de estoque | SQL19, HTTP17 | AlteracaoCarta registra autor/motivo/preço/estoque/ativação anteriores/novos; preço30→40 e ativo1→0 com estoque2→2; dois registros, só movimento inicial de estoque; no-op/conflito sem auditoria extra |
| Limpeza dos objetos de falha | SQL18/SQL21 e consultas finais | Zero tr_test_% remanescentes; todos removidos em finally |
| Privilégios SQL de banco | SQL17 e011 pós-teste | SELECT/EXECUTE versionado permitidos sob EXECUTE AS; DML/legados negados; não comprova login externo runtime restrito |
| MVC administrativo | MVC01–MVC06 | Sete páginas200, submissões reais de cliente/atendente/saldo/carta e conflitos409; validação HTML/HTTP, sem inspeção visual de navegador/acessibilidade |

## Rollback e coordenação do script 013

A fixture verifica nome/marcador da base e instala trigger AFTER INSERT com nome tr_test_<rodada> apenas em PokeVaultTests. A flag SESSION_CONTEXT é definida exclusivamente na conexão de teste; o trigger exige a flag e provoca51999 após uma escrita real. O alcance é registrado em outra flag de sessão, conferida depois do rollback. Nenhuma procedure de produto foi modificada e nenhum BEGIN TRAN externo foi usado. Objetos são removidos em finally e a ausência é conferida também em consulta independente final.

Banco informou lacuna de auditoria só preço/ativação e publicou 013. Esta frente registrou base livre em tests/prontidao-sprint2.md; a consulta posterior mostrou que a versão 4 já estava aplicada às 17:54:38.108UTC, antes da rodada 02. Não presumimos que aquela rodada já verificava a nova auditoria. SQL19/SQL20/HTTP17 foram acrescentados após ler013 e executados nas rodadas03/04. A lacuna anterior foi relatada por Banco; esta frente não a reproduziu antes da correção.

## Reconciliação final e problemas do roteiro

`pos-verificacoes-final.json`: scripts 007,011 e tests/Sprint2/Verificar-Integridade.sql retornaram exit0. As oito consultas de divergência de007 não retornaram linhas. Consulta final: saldos divergentes 0, estoques divergentes 0, totais divergentes 0, triggers teste 0, versões instaladas4. Base preserva60 compras, 87 usuários e 115 cartas de testes/seed; números descrevem esta execução e não são pré-condições fixas para nova rodada.

A tentativa inicial de pós-verificação usou -W junto de -y0, opções incompatíveis do sqlcmd. Falha preservada em pos-verificacoes.json; comando corrigido sem-W passou em pos-verificacoes-reteste.json e na verificação final. Não foi erro funcional do SQL/produto.

HTTP18 reprovou na rodada 04 por expectativa201 indevida do harness. A classe de cadastro usa JsonUnmappedMemberHandling.Disallow, conferida no código; API respondeu400. Expectativa corrigida para400 com verificação de ausência de conta; reteste focado passou. Nenhuma reprovação foi apagada ou convertida retrospectivamente em sucesso.

Build do harness: exit0,0 erros; warning NU1900 porque não foi possível consultar índice de vulnerabilidades do NuGet no ambiente restrito. Isso não é resultado de teste funcional nem comprova auditoria de dependências. Avisos de nulabilidade da versão intermediária do harness foram corrigidos antes das rodadas finais.

## Reprodução e pendências finitas

Harness executado diretamente: `PV_TEST_HTTP=1`, sem PV_TEST_FOCUS, então `dotnet tests/Sprint2/bin/Debug/net10.0/PokeVault.Testes.dll <novo-json>`. Só usar com processo 5188 isolado e schema 1–4 liberado. O harness recusa sobrescrever evidência e usa conexão fixa PokeVaultTests. Foco HTTP18 usa PV_TEST_FOCUS=HTTP18; não confundir reteste focado com suíte inteira.

`tests/Executar-Sprint2.ps1` encapsula cópia/servidor isolado, build do harness, evidência nova e encerramento do próprio processo. Foi validado pelo parser PowerShell (0 erros), **não executado ponta a ponta nesta rodada**; o fluxo real foi realizado pelos comandos registrados nos JSONs/relato. O wrapper falha se houver caso reprovado/bloqueado; o console isolado gera o JSON e não deve ter seu exit0 interpretado sozinho como aprovação.

Limites remanescentes: conexão externa da aplicação com identidade Windows runtime de menor privilégio não foi provisionada; as execuções usaram a conta de preparação. Não há ensaio de carga/repetições estatísticas de concorrência, teste visual de navegador, auditoria ampla de acessibilidade, mobile ou prova integral de todos osT01–T36. Não houve defeito funcional confirmado pela suíte após013; falhas de roteiro acima foram resolvidas. Coordenação decide encerramento da sprint a partir das entregas; sprint 3 não foi iniciada por esta frente.


