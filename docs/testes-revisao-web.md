# Revisão final da web — testes executados

**Concluída nesta frente em 03/11/2026**, America/Sao_Paulo, com apoio de IA. Escopo: demonstração acadêmica local da fonte ativa estável, schema 4. Complemento suspenso e mobile não foram executados. O PDF e o texto acadêmico final não foram reescritos.

## Resultado finito

- **45/45 cenários de regressão aprovados** em uma execução completa da versão final do wrapper (`wrapper-final`). São21 SQL, 18 HTTP e 6 MVC, com contas e massa próprias em PokeVaultTests.
- **6/6 verificações suplementares HTML/HTTP aprovadas**, em execução separada (`html-01`). Não são inspeção visual ou execução de teclado em navegador.
- Uma execução anterior do wrapper nesta mesma revisão (`wrapper-01`) também aprovou45/45. Foi preservada. A execução final foi repetida após ajustes do próprio wrapper para limpar variáveis de seleção de suíte e registrar hashes do wrapper/validador no manifesto; não houve alteração/recompilação do produto por esta frente.
- **Revisão visual desktop/largura estreita não executada:** computer-use interrompeu a vinculação do Chrome por não conseguir determinar sua URL no Windows com confiança para aplicar a política. Nenhuma tentativa de contorno, captura, navegação, teclado ou entrada de senha foi realizada.

Há 51 cenários distintos aprovados nos dois conjuntos (45+6). As duas execuções da regressão não duplicam cobertura; totalizam90 execuções dos mesmos 45 cenários. Os resultados anteriores de21/10 e o relato manual de Daniel são referências históricas separadas, sem soma às métricas desta revisão.

## Versão, ambiente e isolamento

Desenvolvimento publicou `src/evidencias/revisao-web/prontidao.json`, estado `PRONTO_PARA_REGRESSAO_ISOLADA`, fonteAtivaSemWip=true, schema 4, em 03/11/2026 às 22:11:53.9857932UTC. Esta liberação foi lida antes de iniciar o wrapper. Cópia preservada em `docs/evidencias/revisao-web/prontidao-produto.json`.

**Produto executado: SHA256 177F24F52C71190AC3420CD62E34F741B73DD1B7DD4ABF5FA64A392547E33CE4.** Binário final fornecido por Desenvolvimento, sem build do produto por Testes. Não se usou binário WIP nem snapshot antigo da sprint2. As cópias de execução ficaram em `tests/.runtime/wrapper-01` e `tests/.runtime/wrapper-final`, com contentRoot próprio, assets locais e porta 5188. O produto era o mesmo nas execuções HTML/HTTP e de regressão.

SQL Server real: `.\SQLEXPRESS`, DESKTOP-641C55N\SQLEXPRESS, motor17.0.1000.7 Express. Base `PokeVaultTests` marcada como PIM-2026-Sprint1, schema 1–4. O harness ganhou guarda que recusa schema máximo diferente de4 antes da fixture mutante. Nenhuma 014–016 aplicada. Dados/credenciais/demo 5187 não foram alterados ou interrompidos por esta frente.

Cada execução criou dados próprios; senhas aleatórias somente em memória durante os testes automatizados. Nenhuma conta/senha da demonstração foi reutilizada. O auxiliar de preparação de credenciais para UI foi compilado, mas não executado, porque o acesso UI foi encerrado: não foi criado arquivo temporário com senhas e não houve teste de navegador autenticado. Relatórios e JSONs não contêm senhas/tokens.

## Wrapper realmente exercitado

Comando da execução final, na raiz do projeto:

```powershell
./tests/Executar-Sprint2.ps1 -Rodada wrapper-final -PastaEvidencias docs/evidencias/revisao-web -ProdutoSha256Esperado 177F24F52C71190AC3420CD62E34F741B73DD1B7DD4ABF5FA64A392547E33CE4
```

Usar outro nome de rodada em uma nova execução: o script recusa sobrescrever pasta de evidências. Seu ciclo real cobriu: resolução da raiz; conferência do hash liberado; recusa de porta 5188 ocupada; cópia isolada; restore/build apenas do harness; início de processo oculto próprio; espera de health; execução da suíte; validação do JSON; encerramento em finally; restauração de variáveis. O script não compila a aplicação, não executa DDL e não usa a base PokeVault.

Alterações do wrapper nesta frente: pasta de evidências parametrizada; hash esperado do produto; APPDATA/LOCALAPPDATA e caches locais; limpeza/restauração dos seletores de execução; validação de base/quantidade/status/IDs; manifesto com hashes produto/harness/wrapper/validador; arquivo de encerramento com PID/estado. O exit 0 do console isolado não é usado sozinho como aprovação.

Gate efetivamente verificado em `validacao-status-historicos.json`: o mesmo `tests/Validar-Resultado.ps1` chamado pelo wrapper rejeitou o JSON histórico com um cenário reprovado e o reteste histórico parcial de apenas um caso, ambos conforme esperado. Esses dois ensaios são verificações do gate, não cenários do produto final. Qualquer reprovação/bloqueio/status ausente, cobertura menor que45, base errada ou IDs repetidos interrompe a aprovação. A execução final normal retornou exit 0 e45 aprovados.

Restore/build do harness concluídos:0 erros, warning NU1900 porque a consulta de vulnerabilidades do NuGet estava indisponível. Não foi suprimido nem tratado como auditoria de dependências aprovada. O build do produto com 0 avisos/erros pertence à evidência de Desenvolvimento; não foi reexecutado por Testes.

## Cobertura real da regressão

IDs, esperado e observado estão em `wrapper-final/resultados.json`; implementações do harness em `tests/Sprint2/`. As45 aprovações não significam execução integral de todos os subcasos conceituais T01–T36.

| Grupo | Casos executados | Evidência efetiva |
| --- | --- | --- |
| Login/sessão/catálogo público | HTTP01/HTTP13 | Consulta pública200, protegido401, autenticação e logout; senha incorreta401 |
| Gestão de pessoas/saldo | SQL13/SQL18, HTTP02–HTTP04/HTTP09/HTTP16/HTTP18, MVC03/MVC04/MVC06 | ADM cadastra cliente/atendente; saldo zero/carrinho vazio; hash validado sem exposição; saldo absoluto/delta/no-op; cookie inativo; proteção ADM; sem elevação por payload |
| Catálogo/perfis/CSRF/entradas | HTTP05–HTTP08/HTTP14/HTTP15, MVC01/MVC05 | Atendente gerencia carta/estoque, sem gestão de saldo/usuários/estorno; cliente/anônimo negados; antifalsificação; duplicidade; escalas/limites inválidos |
| Compra selecionada e remanescentes | SQL01/SQL02/SQL10, HTTP10 | Compra das linhas selecionadas, conservações independentes, demais linhas preservadas; quantidade corrente; saldo/estoque insuficientes sem efeito parcial |
| Preço/UUID/histórico/estorno | SQL03/SQL04/SQL09, HTTP10/HTTP11 | Mudança de preço409 até nova confirmação; reenvio idempotente e conteúdo divergente; isolamento de compra; estorno original sobre saldo corrente e repetição |
| Concorrência | SQL05–SQL08/SQL16 | Última unidade/saldo disputados em conexões independentes; mesmo UUID/cancelamentos simultâneos; versão esperada concorrente sem sobrescrita |
| Versão/auditoria | SQL14/SQL15/SQL19, HTTP12/HTTP17, MVC02/MVC05 | Venda invalida formulário antigo; erro409/51012; autor/motivo/valores antigos/novos em alterações só preço/ativação; no-op sem duplicar movimento |
| Rollback/limpeza/privilégios de banco | SQL11/SQL12/SQL17/SQL18/SQL20/SQL21 | Falha após escrita de compra/estorno/auditoria, estado revertido; triggers temporárias removidas; SELECT/EXECUTE/negação de DML sob principal de banco |

Falhas controladas foram induzidas somente em PokeVaultTests por triggers temporárias com flag SESSION_CONTEXT, removidas em finally e conferidas por consulta independente. Nenhuma procedure/demo recebeu porta de falha e nenhuma transação externa foi usada para simular rollback.

## Conferências suplementares HTML/HTTP

| ID | Comportamento efetivamente conferido | Resultado |
| --- | --- | --- |
| REV-H01 | Catálogo vazio com orientação; langpt-BR, viewport, link pular conteúdo/main/nav; CSS local com media/focus | Aprovado; presença no markup/CSS, sem prova visual |
| REV-H02 | Login rotulado/senha oculta/autocomplete; autenticação inválida renderiza alerta sem eco de senha | Aprovado por POST/HTML |
| REV-H03 | Cliente novo vê carrinho e histórico vazios; linha posterior tem nomes acessíveis na seleção/quantidade | Aprovado por HTML |
| REV-H04 | Links de navegação diferenciados para administrador, atendente, cliente e anônimo | Aprovado por HTML; autorização efetiva também nos 45 casos |
| REV-H05 | Cadastro MVC inválido apresenta alerta e não cria conta | Aprovado por POST e SQL |
| REV-H06 | Filtros combinados nome/coleção/conservação e duas páginas de massa própria, sem IDs repetidos | Aprovado por API real |

Não se verificou comportamento visual em largura estreita, foco visível, ordem real de tabulação, contraste ou ausência de sobreposição. Ter viewport/media/focus no código não comprova esses aspectos. O bloqueio de computer-use está em `limite-computer-use.json`; foi respeitado. Daniel relatou manualmente funcionamento anterior, com casos não especificados; não é aprovação desta suíte.

## Reconciliação e término dos processos

`reconciliacao-final.json`: scripts007 e `tests/Sprint2/Verificar-Integridade.sql` retornaram exit 0. O007 não retornou linhas nas consultas de divergências. Resultado agregado final: saldos divergentes 0, estoques divergentes 0, totais divergentes 0, triggers temporárias 0 e versões instaladas 4. Massa/histórico mantidos:90 compras, 134 usuários e 189 cartas de seed/testes; não são valores iniciais obrigatórios para novas execuções.

`wrapper-01/encerramento.json`, `html-01/encerramento.json` e `wrapper-final/encerramento.json` confirmam encerramento dos processos próprios 5188. Último wrapper encerrouPID 11096 às 22:15:28.4517198UTC (19:15:28 São Paulo). Nenhuma intervenção no processo5187 por Testes.

## Entrega e limites

Não houve reprovação funcional no produto final no conjunto executado. Houve limitação real de computer-use e warning de auditoria NuGet; ambos registrados sem contorno ou falsa aprovação. Não há ensaio de carga, prova estatística de ausência de conflitos, teste mobile ou cobertura universal. A identidade Windows dedicada não é exigência de aceite acadêmico desta rodada, conforme Daniel; segue apenas recomendação para eventual implantação futura. O acesso Windows de desenvolvimento atual foi preservado, sem conta/serviço/infraestrutura adicional.

Para apresentação local, consultar `docs/ROTEIRO-DEMONSTRACAO-WEB.md`, roteiro proposto pela coordenação, e as instruções de Desenvolvimento para `Abrir-PokeVault.cmd`. Este roteiro não foi ensaiado visualmente por esta frente. Os45 fluxos/validações e as6 conferências suplementares têm evidência automatizada; a decisão de aceite local considera o limite visual explicitado. Não iniciar complemento/mobile ou reescrever PDF automaticamente.

