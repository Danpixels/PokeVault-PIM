# TCGdex151 — testes executados e resultado final

**Concluído nesta frente em 03/11/2026**, America/Sao_Paulo, com apoio de IA. A integração autorizada é somente151/sv03.5. Nenhuma melhoria anterior suspensa, mobile, infraestrutura extra ou reescrita do PDF foi iniciada. Testes não alterou src/, database/, credenciais ou dados da demonstração5187.

## Resultado da fonte final

- **25/25 cenários TCGdex aprovados:**23 em live-final (11 fixtures, 6 integração real, 4 SQL e 2 HTML) e 2 em offline-final.
- **45/45 cenários de regressão aprovados**, via wrapper executado de ponta a ponta em regressao-final, com schema 10 e rede TCGdex desabilitada.
- Versão final executada: **SHA256 D226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C**. Manifesto de Desenvolvimento confirmou essa DLL/snapshot; fonte ativa foi conferida com o mesmo hash ao encerrar. Testes compilou somente seus harnesses, nunca o produto.
- Reconciliação007/022 sem divergências; consulta final confirmou zero triggers temporárias de teste. Processos próprios5188 encerrados, incluindoPID 25284 do wrapper às 22:58:11.9036102UTC (19:58:11 São Paulo). Sem ação desta frente no processo5187.

As duas métricas25/45 são conjuntos separados; não somar repetições históricas como nova cobertura. Não equivalem a todos os subcasos conceituais do projeto ou prova de ausência universal de defeitos. HTML/HTTP não é revisão visual; a limitação anterior de computer-use foi respeitada, sem nova tentativa de contorno.

## Dependências e versão real

Banco liberou PokeVaultTests às 22:37:47UTC, com marcador PokeVaultProjeto=PIM-2026-Sprint1 e versões 1,2,3,4,8,9,10. SQL Server17.0.1000.7 Express em DESKTOP-641C55N\SQLEXPRESS. Não foram aplicados014–018; Testes não aplicou DDL. Após liberação, esta frente controlou sua massa e seus processos, sem DDL concorrente de Banco.

Primeira versão liberadaA68 foi copiada para tests/.runtime/tcgdex151-01 e realmente testada. Desenvolvimento depois corrigiu validação antes da substituição de cache e rotulagem histórica; o gate recusou a regressão quando o binário deixou de corresponder ao hashA68. A tentativa foi preservada e nenhum processo/regressão começou com hash divergente. A versão intermediária6E não foi usada em execução funcional por Testes. A liberação finalD226 foi conferida e copiada para tests/.runtime/tcgdex151-02, preservando a cópia/cache/evidênciasA68.

Na versão final, uma nova coleta com forcarRede=true foi feita para validar a correção na fronteiraHTTP, sem atribuir o resultado antigo ao novo binário. O cache válido anterior de teste foi conservado; a nova importação/repetição permaneceu idempotente. Referências e imagens da demonstração não foram usadas como massa de Testes.

## Medição real da coleção

Evidências principais: live-final/resultados.json, preservacao-importacao.json, progresso.jsonl e medicao-reteste.json, em docs/evidencias/tcgdex151/.

| Medida observada na execução final | Quantidade / interpretação |
| --- | --- |
| Cartas lógicas/detalhadas |207/207, sem CardId duplicado, somente sv03.5 |
| Traduções físicas |207, todaspt na coleta real;165 oficiais/207 totais no manifesto |
| Imagens locais disponíveis |207, ausentes0; imagemPikachu servida por URLlocal com HTTP200 |
| Fallback inglês real observado |0; fallback foi forçado e verificado nas fixtures, não inventado na coleta real |
| Referências com objetoCardmarket/TCGplayer |207/207; fontesEUR/USD mantidas como referência externa |
| Referências sem pricing principal |0 nesta coleta; ausência foi testada por fixtures e atualizaçãoSQL controlada |
| Referências com algum campo numérico no pricing principal |207 |
| Campos numéricos/nulos no pricing principal |3940/344; quantidade de campos, não de cartas/preçosBRL |
| Zeros numéricos recebidos da fonte |54; são zeros existentes no JSON, distintos de ausência/null; não representam conversão ou preço de venda |
| Pricing sem origem/data |0 |
| Referências fora de sv03.5 |0 |

As medições de campos percorrem o pricing principal armazenado noSQL, excluindo identificadores e metadados unit/updated; não contam cada variante detalhada adicional como carta. SnapshotJSON completo preserva dados adicionais. Não foram inferidos preços ausentes, conversõesEUR/USD→BRL ou equivalênciasNM/SP.

A coleta externa é uma fotografia daquela execução, não garantia de disponibilidade futura. As cópias de set/cartagravados em Fixtures vieram de GETs reais da frente Banco; isso é distinto dos GETs realizados pelo produto na importação final. Documentação primária consultada: [REST de cartas](https://tcgdex.dev/rest/card) e [preços de referência](https://tcgdex.dev/markets-prices). Exemplos da documentação não foram tratados como preços atuais.

## Matriz de cenários finais

Todos abaixo têm último resultado aprovado naD226. ID exato, data, esperado/observado e valores estão nos JSONs; fontes do harness em tests/Tcgdex151. As fixtures usam HttpMessageHandler/DI somente em tests, sem servidor adicional ou rota de falha no produto.

| Casos | Comportamento realmente exercitado |
| --- | --- |
| FX01 | Outro setId recusado antes da rede; nenhum request externo para alvo diferente |
| FX02/FX03 | Indisponibilidade/timeout sem cache retornam ausência, sem inventar referência/preço |
| FX04 | PT404/EN200 identifica idiomaen; erro de imagem não elimina metadados |
| FX05 | Erro remoto após sucesso mantém JSON e hashes dos arquivos; Cache/Offline explícitos |
| FX06 | Pricing ausente produz lista vazia, sem zero sintético |
| FX07/FX08 | Variante nova galaxy-new-holo com null/9,99 eUSD preservada; amostra real gravada contémEUR/USD, data e reverse-holofoil |
| FX09 | Importador real emDI de teste processa207 detalhes com fallbacken e manifestos corretos, sem troca de idioma; store em memória é fixture, não importaçãoSQL real |
| FX10 | Pricing malformado não envenena cache válido; sem cache não é aceito |
| FX11 | Manifesto com IDs duplicados ou total206 não substitui cache207 válido |
| LIVE01 | Importação exige perfiladmin/atendente, CSRF e corpo sem setId arbitrário; cliente403, anônimo401, CSRF400 e campo extra400, sem log de importação criado |
| LIVE02 | Importação real207 detalhes/imagens e comparação de12 hashes comerciais antes/depois iguais |
| LIVE03 | Repetição pelo atendente atualiza sem duplicar traduções/ofertas ou mudar hashes comerciais |
| LIVE04 | Nove páginas somam207 CardIds únicos; detalheHTML200 com origem/moeda/data; imagem localHTTP200 e contagem de preços/imagens |
| LIVE05 | ImagemPikachu temporariamente renomeada só no runtime de teste: detalhe continua200 com fallback; arquivos restaurados emfinally |
| DB01 | Manifesto repetido com mesma chave retorna mesma importação; outro set recusa51020 sem apagar biblioteca |
| DB02 | Snapshot novo sem pricing conservaJSON/data/origem do último preço; lote inválido é atômico; snapshot real restaurado emfinally |
| DB03 | PikachuNM/SP apontam à mesma referência; vínculo divergente51022, sem alteração comercial |
| LIVE06 | Campos comerciais faltantes400 sem oferta criada; operador informaBRL12,34/estoque2/MINT e somente uma oferta é criada |
| DB04 | Nome remoto divergente em referência já vinculada recusa51022 e preserva vínculo/comércio; nome real restaurado emfinally |
| HTML01 | Inputs específicosPreco/QuantidadeInicial realmente vazios no formulário; metadados não impõem preço/estoque |
| HTML02 | Pricing histórico preservado aparece com rótulo, fornecedores eEUR/USD, inclusive intervalo de0,028s entre coletas, sem espera artificial na versão final |
| OFF01 | Config TcgDex:RedeHabilitada=false: biblioteca207, detalhes, imagem local e reimportação do cache funcionam |
| OFF02 | ComTCG offline, oferta referenciada explicitada pela loja30BRL é comprada; remanescente preservado; UUIDrepete; estorno devolve30 e saldo100 |

O teste offline usa a opção legítima de rede do cliente no processo isolado; não desliga internet/firewall doWindows e não altera configuração global. Falhas de transporte/timeout foram também executadas com handler de teste, que registra requests. Fallback inglês real não ocorreu na coleta atual, mas foi exercitado de forma sintética e identificado como tal.

BusinessSnapshot calcula digest das 12 tabelas antigas sobre campos comerciais/identidade/histórico, sem imprimir dados individuais de usuário/hash de senha. Não inclui tabelas TCG/vínculos nem rowversion técnico; a importação e o vínculo podem escrever suas próprias tabelas sem alterar Carta/saldo/estoque/compra. Cadastro explícito de uma oferta posterior é operação separada, não conta como efeito da importação. Massa/histórico próprios permanecem preservados, sem reset.

## Regressão45 e wrapper

Comando real, após encerrar o processooffline próprio:

```powershell
$env:TcgDex__RedeHabilitada='false'
./tests/Executar-Sprint2.ps1 -Rodada regressao-final -PastaEvidencias docs/evidencias/tcgdex151 -ProdutoSha256Esperado D226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C -SchemaMaximoEsperado 10
```

Não repetir com o mesmo nome: a pasta existente é recusada. O wrapper já exercitado foi adaptado para schema esperado 4 ou 10 (padrão4), sempre recusando versões5–7 suspensas. Gate de status/base/cobertura/IDs foi preservado e não aceita exit 0 do console sozinho. Manifesto identifica hashes produto/harness/wrapper/validador; encerramento confirma fim doPID próprio. O wrapper compila somente harness e nunca aplica migrações ou inicia produto WIP.

45 cenários aprovaram gestão de usuários/saldo/catálogo, perfis/CSRF/valores inválidos, conta desativada, compra selecionada/remanescentes, mudança de preço, saldo/estoque insuficientes, UUID, última unidade/saldo/cancelamentos concorrentes, estorno original/único, conflito de versão, auditoria sem delta e rollback após escrita. Triggers de falha ficaram somente em PokeVaultTests e foram removidas emfinally; medição independente final confirmou0 remanescentes.

## Histórico, falhas e retestes preservados

| Registro | Resultado e tratamento |
| --- | --- |
| live-01/offline-01,A68 |18+2 cenários aprovados; evidência histórica, não prova da versão final |
| identity-01,A68 |2 aprovados eHTML02 reprovado por ausência do rótulo histórico em atualização muito próxima da coleta anterior |
| pricing-ui-reteste,A68 |Reteste com intervalo medido54,528s passou; não comprovava o caso inferior a1s |
| Mudança de DLL |Gate bloqueou regressãoA68 antes de iniciar processo/casos; preservado em regressao-gate-versao.json e log. Não foi defeito financeiro |
| Correção do produto,D226 |Validação antes de gravar cache e rótulo em qualquer diferençaSQL. Fonte alterada por Desenvolvimento; fixturesFX10/11 e testeHTML02 sem atraso passaram na versão final |
| Primeira mediçãoSQL |Erro240 da CTE de leitura por tipos incompatíveis. SQL original e saída preservados; query corrigida em tests/Medicao, retesteexit 0. Não houve escrita nem falha do produto |

Reconciliação007 e022 retornaram exit 0 e nenhum desvio comercial. A primeira tentativa de medição está no mesmo JSON histórico com exit 1; o resultado final correto está em medicao-reteste.json. Nenhum erro foi apagado ou convertido retroativamente em sucesso.

## Reprodução e limites

Produto e schema precisam ser liberados pelas frentes responsáveis. Não executar testes durante DDL nem importar 207 ofertas comerciais. O projeto tests/Tcgdex151 referencia a DLL da cópia de teste, sem ProjectReference/build do produto. Na execução final: PV_TCG_PHASE=online, PV_TCG_FORCE=1 e runtimeTCG02; faseoffline usa o mesmo cache com processo reiniciado e redeTCGfalse. Fasesidentity/pricing-ui são retestes focados e não equivalem à suíte inteira. O console gera resultados; aprovação depende dos registrosJSON, não só exitCode.

Limites: sem revisão visual por ferramenta, sem carga/estatística de concorrência ou falha física de disco/rede doSO; sem importação de outra coleção ou conversão de moeda. ControlesHTTP/HTML não comprovam aparência/acessibilidade visual. Dados públicos/preços podem mudar; contagens/imagens/preços desta coleta são evidência datada. Build dos harnesses concluiu com 0 erros e warningNU1900 porque auditoriaNuGet estava indisponível; não suprimir nem declarar auditoria aprovada. ContaWindows dedicada segue recomendação futura, fora do aceite acadêmico local. Não houve mudança de segurança global ou conta/serviço extra.

O relatório finito entrega resultados reais e limites para o diário central; nenhuma mensagem foi enviada a outros chats. Mobile, melhorias suspensas e trabalho acadêmico final permanecem adiados.

