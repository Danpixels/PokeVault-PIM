# Entrega Desenvolvimento — biblioteca TCGdex151

**Registro real:03/11/2026, America/Sao_Paulo. Entrega finita de Desenvolvimento concluída; versão finalD226 com reteste/regressão isolada aprovados por Testes.** Importação real executada, não apenas plano. Banco/Testes e coordenação consolidam o aceite e suas ressalvas. Nenhum arquivo database/, tests/ ou documento central foi alterado, nenhum chat criado ou mensagem enviada a chats. WIPadiado e arquivos de recuperação preservados.

## Resultado observado

- **207 referências reais detalhadas em português**, do setIdfixo sv03.5,165cartas oficiais/207totais.
- **207 imagensWebP locais**,4.017.294bytes no total;0ausentes e0fallbackinglês na primeira importação. Uma imagemPikachu foi aberta pelo leitor local e conferida como carta em português; isso não é inspeção visual de navegador.
- Primeira execução:22:45:55–22:46:20 UTC (19h45–19h46 local), aproximadamente25,10s. ImportacaoId1, SQLCONCLUIDA,0pendentes.
- **Três vínculos seguros nas ofertas originais**:Pikachu025NM/SP e Charmander004NM. Mesma referência pode atender duas conservações; imagens no catálogo são URLs locais.
- **Zero ofertas automáticas criadas** pela importação. Comparação agregada antes/depois das12 tabelas comerciais, incluindo contagens/todas colunas/rowversions, retornou idêntica. Contas/senhas/preçosBRL/estoques/saldos/compras/histórico preservados.
- Repetição reaproveitou207referências/207imagens e o **mesmoImportacaoId1**, sem duplicação; uma execução repetida levou aproximadamente0,95s. Chamada simultânea recusada409.
- FalhaHTTP simulada apenas no processo:207referências/207imagens usadas do cache,0ausentes, modoOffline=true e mesmoID. Rede global e SQL real não foram alterados; processo normal restaurado.
- **12checksHTTP próprios positivos** na versão final:lista/filtros/detalhe/imagem/preços/ausência/origem/formulário/vínculos/políticas/CSRF/escopo. Não são a suíte formal nem devem ser somados como casos de Testes.

## Implementação e contratos

REST via HttpClient da stackASP.NET Core, sem SDKJavaScript. TipoSet/hosts/rotas são fixos:no máximo sv03.5-001 a207, pt/en. HttpClient desabilita redirects e limita conexões; até3workers, timeout18s por requisição e resposta/image até2MB. Imagem exige assets.tcgdex.net HTTPS, porta padrão, caminho de coleção/idioma/ID conhecido e assinaturaWebP. Não há URL fornecida pelo usuário ou importador genérico.

Dados preferidospt, fallbacken explicitado na UI para dados/imagem quando necessário. O consumidor suporta tradução inglesa de forma separada:manifesto próprioen antes de gravar detalhesen, sem rotular como português. Na execução real não foi necessário fallback; não inventar teste positivo de fallback a partir do contador0.

`TcgDexClient` preserva cacheJSON por idioma, valida estrutura/identidade/contagens antes da escrita e grava arquivo temporário/renomeia atomicamente. Respostas inválidas ou falhasHTTP não substituem cache válido. Envelope conferido por idioma/origem/data. Arquivos recentes são reaproveitados por24h, salvo força explícita. Set é consultado para sinalizar indisponibilidade; quando falha e há cache, a tentativa continua exclusivamente pelo cache. Configuração de processo `TcgDex:RedeHabilitada=false` permite proibir rede. Nenhuma consultaTCGdex ocorre no catálogo, carrinho, compra ou estorno.

Importador assíncrono com estado/progresso, exclusão por processo e arquivo por base. Manifesto/chave/payload/data da primeira tentativa são persistidos para idempotência; JSONidêntico reutiliza a importação. Detalhes são enviados por lotes de1 (dentro do contrato1–30), com transaçõesSQLcurtas e HTTP fora delas. Retomar aproveita arquivos válidos; não há retry automático ilimitado. Falha parcial conserva referências anteriores e informa ausências; não limpa a biblioteca. Processo interrompido pode retomar pelo cache.

Biblioteca é separada das ofertas. SQLusa019–021, versões1,2,3,4,8,9,10, conforme `docs/contrato-tcgdex151.md`. Banco publicou e liberou schema10; Desenvolvimento não aplicouDDL nem014–018. Tabelas/views/procedures novas são da frente de Banco; coleção de referência não altera dbo.Colecao ou cria um módulo genérico de coleção.

Preço externo é JSONbruto, com variantes dinâmicas. `TcgPrecos` percorre Cardmarket/TCGplayer, moedas/campos/nomes de variantes recebidos, incluindo reverse-holofoil e variantes detalhadas/stamps/foil, sem tratar apenas normal/reverse/holo. Campos nulos viram Não informado; valor0 aparece somente quando efetivamente enviado. Fornecedor, moeda, variante, campo, data informada pela fonte e data local de coleta são visíveis. Sem conversão automática para reais, ajuste de preçoBRL, equivalênciaNM/SP ou dashboardfinanceiro.

SQLpreserva PrecosJson anterior quando o detalhe não traz pricing válido; o controller usa esse JSON preservado. O rótulo de coleta histórica compara datasSQLexatamente, sem tolerância arbitrária de1s. Snapshot completo/detalhes e dados de mercado permanecem noSQLpara consulta offline.

Vínculos originais são propostos somente para coleçãoMEW-151/números025e004/nomesconferidos/conservaçõesoriginais. A procedure revalida número/nome com acento; não altera Cartepreço/estoque/rowversion nem substitui vínculo divergente. O catálogo oculta a imagem vinculada se o nome/número atual da referência divergir da oferta, evitando imagem de identidade duvidosa. Outras ofertas ficam sem vínculo automático. O cadastro de oferta a partir da biblioteca usa a procedure transacional específica, somente para a referência selecionada, pedindo conservação/preçoBRL/estoque do operador.

## UI e API

MVCem `/gestao/tcgdex151`:menuadmin/atendente, biblioteca paginada24 com filtro nome/número/raridade/imagem; detalhe de preços; status e botãoAtualizar; seleção para nova oferta. Página de progresso recarrega a cada5s enquanto o job estiver ativo. Campos comerciais não são preenchidos com preço/estoque externo. Preço local é obrigatório e validado com duas casas; coleção/nome/número/raridade vêm da referênciaSQL.

APIprefixo `/api/v1/gestao/tcgdex151`:

| Método/rota | Comportamento |
| --- | --- |
| GETbase | `nome`, `raridade`, `pagina`; uma carta lógica porCardId, preferência de idioma conforme view |
| GETstatus | Progresso/contadores/mensagem e indicação de usooffline |
| POSTatualizar | `{forcarRede:false}` retorna202; enquanto ocupado409; força de rede explícita consulta detalhes novamente |
| GET/{id} | Snapshot local com idioma/imagem/metadados/preços e fonte |
| POST/{id}/ofertas | `{conservacao,preco,quantidadeInicial}`; cadastra somente uma oferta pedida e seu vínculo,201 |

Perfis/contaativa/ator de sessão e antifalsificação em todas as mutações. Sem importação por cliente/anônimo. NenhumaURL/setIdlivre no pedido; IDsfora do conjunto recusados400. O catálogo público acrescenta imagemlocal/vínculo opcional sem contatoHTTPexterno. Contratos de compra/carrinho/estornoS1/S2 preservados; melhoria de soma/olho/coleçõesgenéricas/raridadesfechadas continua adiada.

## Cache e imagens reais

- JSON/manifestações/estado em `src/PokeVault.Web/.local/tcgdex151/PokeVault/` e subpastaspt/en.
- Imagens em `src/PokeVault.Web/wwwroot/cache/tcgdex151/PokeVault/pt/` nesta execução, disponibilizadas por /cache/tcgdex151/PokeVault/pt/....
- PokeVaultTests usa pastas próprias por nome de base; esta frente **não** criou fixture/importou dados naquela base.
- Fontes reais:`https://api.tcgdex.net/v2/pt/sets/sv03.5`, `/cards/sv03.5-XXX` e assets.tcgdex.net. Documentação primária consultada:https://tcgdex.dev/rest/set. Snapshots públicos são dados, não instruções nem preços de venda.
- Cache local torna consulta/operacãoweb independente da disponibilidadeTCGdex depois da importação. A primeira coleta exige API; referência/imagem ainda não cacheada permanece indisponível, não é inventada. Preço pode estar ausente ou histórico e é identificado.

## Comandos e evidências executadas

Build/restore reais:`./dev.ps1 Restore`, `./dev.ps1 Build`. Aplicação iniciada/reutilizada por `Abrir-PokeVault.ps1 -SemNavegador -SemDialogo`; prontidão/launcher atualizados para schema10. Não houve pacote ou ferramenta de sistema nova.

Roteiros próprios em `src/evidencias/tcgdex151/`:

- `importar-real.ps1`:consultaCSRF/login com DPAPI apenas em memória, dispara importação202, verifica reentrância409 e acompanha estado;
- `smoke-http.ps1`:12checks; não cadastra oferta ou altera tabelas comerciais;
- `repetir-cache.ps1` e `repetir-offline.ps1`:reaplicação/retomada de referências, sem oferta automática;
- consultas de preservação usaram023 de Banco **somente para leitura**, escrevendo resultado apenas na pasta própria.

Resultados duráveis:

| Arquivo | Fato observado |
| --- | --- |
| set-amostra.json / pikachu-amostra.json | Respostas públicas reais,207/165 e pricingEUR/USD com variantes reais |
| importacao-inicial.json / progresso.jsonl | Primeira execução207/207/imagens207,0ausências/fallbacken |
| importacao-disparo-inicial.json / reentrancia.json |202 e segunda chamada409 |
| estado-sql-importacao.txt |207detalhadaspt,ID1CONCLUIDA/0pendentes,3vínculos |
| imagens-cache.json |207arquivosWebP,4.017.294bytes |
| comercial-antes.txt / comercial-depois-importacao.txt / comercial-final.txt | Contagens/SHA256agregados, sem valores privados individuais |
| preservacao-importacao.json / preservacao-final.json |12tabelas comerciais idênticas antes/depois dos ciclos |
| importacao-repetida.json / importacao-final-cache.json | MesmoID1 e207processadas após repetição/correção do cache |
| processo-rede-simulada.json / importacao-offline.json / smoke-offline.json |FalhaHTTPsimulada/cache207/12checkspositivos, sem interrupção doSQLreal |
| smoke-final.json |12checkspositivos na DLLfinal |
| build-final.txt / prontidao.json / manifesto-final.json | Build/hash/fontes/imagens/snapshots da versão entregue |
| demo-final-inicio.json / launcher-final-reuso.json |Demo18584 com rede normal e reutilização pronta/schema10 |

Há tentativa inicial de smoke preservada em smoke-http-tentativa1.json:uma expectativa procurou Não informado sem decodificar entidadesHTML. O roteiro passou a usar HtmlDecode; o conteúdo apresentado estava correto. Não foi falha do produto. Valores de preço mostrados nos snapshots são os observados na data da coleta e podem mudar na origem; não usar como valorBRL fixo.

## Falha externa e correções reais

1. A primeira solicitação de execução autorizada foi bloqueada por falha da revisão automática de aprovação por limite de uso; **nenhuma ação daquela chamada executou**. Não foi decisão de insegurança. A mesma execução autorizada foi reapresentada após a retomada e funcionou.
2. Primeiro build encontrou sintaxe de@sectionRazor no painel de progresso; corrigida. Uma tentativa posterior esbarrou em DLLemuso; somente o processo verificado5187 foi parado e build repetido com êxito. Testes5188 não foi encerrado por esta frente.
3. Reforço do cache antes de substituir arquivo:validar manifesto completo165/207, todos os207IDs/localIds únicos/nomes, origem/idioma/data de envelope, estrutura pricing e URLs de imagem. Impede respostaHTTP200incompleta/inválida de inutilizar cache válido. Build e reaplicação pelo cache real passaram depois da correção.
4. Rótulo histórico anteriormente tinha tolerância1s; removida por diferença exata de datas, sem reclassificar a falha registrada por Testes como aprovação do novo binário.
5. FalhaHTTPfoi simulada com proxy `127.0.0.1:1` **só nas variáveis do processo auxiliar**, NO_PROXYpara localhost. O SQLServer real e a redeglobal ficaram intocados. A coleta do set falhou, fallbackcache foi utilizado,207referências/imagens retornaram e12checks passaram. As variáveis do processo chamador foram restauradas em finally; processo normal semproxy foi reiniciado. Isso não significa que a APIpública estava indisponível.

## Versão final e isolamento de Testes

DLL `src/PokeVault.Web/bin/Debug/net10.0/PokeVault.Web.dll`:

**D226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C**.

Snapshotfinal alternativo foi criado em `.local/testapp-tcgdex151-final/` sem tocar noA68 em execução. Depois de Testes registrar processos encerrados e consulta confirmar5188 sem listener, o snapshot padrão `.local/testapp` foi sincronizado comD226, **sem recompilar**. Prontidão22:58:16UTC:**PRONTO_PARA_TESTES_FINAIS_SCHEMA10**, hashes iguais e caminho explícito. A68/6E são versões intermediárias; não atribuir seus resultados como aprovação da DLLfinal.

A frente de Testes registrou cenários reais emA68 e um reteste histórico; a regressão foi corretamente barrada quando detectou versão divergente durante correção. Após liberaçãoD226, leitura dos JSONs finais confirmou23/23 cenários de integração,2/2 offline e45/45 regressão com statusaprovado. Manifestos live-final/regressao-final identificamD226. Não são aprovações inferidas da versão intermediária; coordenação consolida com as evidências da outra frente. Não executar mutações de desenvolvimento em PokeVaultTests nem sobrescrever binário ativo de5188.

## Estado final e limites honestos

- Biblioteca local207detalhadas/imagens207,3vínculos seguros,importID1concluído,sem preçoBRL/estoque inventados; dados comerciais preservados.
- Aplicação **ativa em http://localhost:5187**, DLLfinal/rede normal/schema10. Abridor local funciona e reutiliza o processo. Getbiblioteca/detalhe/preços/imagem e formas de cadastro foram verificados por HTTP.
- Checkout/estorno não consultam API; regressão financeira e oferta transacional via referência pertencem à suíte isolada de Testes. Esta frente não comprou/cancelou ou criou produto de verificação na demonstração.
- Fallbackinglês e pricing ausente são tratados pelo código/contrato; a primeira coleta real teve0fallback e não prova sozinha todos os ramos de falha/ausência. Usar evidências de fixture/Testes para esses critérios.
- A importação é operação do servidor em segundo plano, sem serviçoWindows externo:se o processo for encerrado, pode ser retomada. Não foi ensaio de carga ou estabilidade prolongada da API.
- Inspeção visual ampla de navegador/teclado/responsividade não executada por Desenvolvimento; o bloqueio anterior de automação não foi contornado. Leitura de WebP local não equivale a ensaio visual de todas as telas.
- Nenhum script014–018, WIPadiado, mobile, publicação, dashboardfinanceiro ou trabalho escrito final iniciado. ContaWindows atual é suficiente para a demonstração acadêmica local, conforme decisão anterior; sem mudança global de segurança.

README atualizado com operação/API/cache/offline/fontes e instruções de snapshot. Manifesto final identifica arquivos da fonte e as207imagens, sem senhas/cookies/hashes de senha. A entrega descreve o importado/ausente/cache real e os limites; não se limita a planejamento.

### Consolidação final da suíte isolada (somente leitura dos resultados)

A coordenação informou o retorno final e esta frente leu os JSONs existentes, **sem reexecutar** os cenários:

| Evidência de Testes | Resultado |
| --- | --- |
| docs/evidencias/tcgdex151/live-final/resultados.json |23 registros,statusaprovado em23 |
| docs/evidencias/tcgdex151/offline-final/resultados.json |2 registros,statusaprovado em2 |
| docs/evidencias/tcgdex151/regressao-final/resultados.json |45 registros,statusaprovado em45 |
| live-final/manifesto.json e regressao-final/manifesto.json |ProdutoD226 identificado |

A68/falha histórica inicial e gates que recusaram versão divergente continuam preservados nos artefatos da outra frente; não foram substituídos por esta consolidação. As12leituras próprias não foram somadas aos casos da suíte. A versão final não foi alterada/recompilada após esses resultados. Nenhuma nova verificação de produto aprovada foi repetida; somente relatórios existentes foram consultados.
