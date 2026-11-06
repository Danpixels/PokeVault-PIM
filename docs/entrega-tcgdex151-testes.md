# Entrega de Testes — TCGdex 151

**Concluída nesta frente em 03/11/2026**, America/Sao_Paulo, com apoio de IA. Integração específica sv03.5, sem retomar melhorias suspensas, mobile ou trabalho acadêmico final.

## Resultado da versão final

**25/25 cenários TCGdex aprovados e 45/45 cenários de regressão aprovados**, separados por conjunto e executados na mesma versão final:

**D226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C**.

Foram 23 cenários online/SQL/fixtures e dois offline. A regressão de 45 passou pelo wrapper completo, com rede TCGdex desabilitada. Não somar rodadas históricas como nova cobertura nem atribuir os resultados A68 ao binário final. Nenhuma reprovação funcional permanece aberta no conjunto final executado.

Produto copiado da DLL liberada por Desenvolvimento, sem compilação por Testes; harnesses próprios compilados. Base real PokeVaultTests, servidor DESKTOP-641C55N\SQLEXPRESS, motor 17.0.1000.7 Express, marcador PIM-2026-Sprint1 e versões **1,2,3,4,8,9,10**. Banco liberou schema antes dos cenários mutantes. Não houve DDL por Testes, aplicação de 014–018 ou alteração de dados/senhas/processo da demonstração 5187.

## Contagens reais

| Medida na coleta final | Resultado |
| --- | --- |
| Referências lógicas/detalhadas | 207/207, somente sv03.5; 165 oficiais/207 totais no manifesto |
| Traduções físicas/idioma real | 207, todas pt |
| Imagens locais/ausentes | 207/0; imagem local servida com HTTP 200 |
| Fallback inglês na coleta real | 0; fallback en foi verificado por fixtures identificadas como sintéticas |
| Objetos Cardmarket/TCGplayer | 207/207, referências EUR/USD separadas de venda BRL |
| Referências sem pricing principal | 0; ausência e indisponibilidade exercitadas por fixtures/SQL |
| Campos numéricos/nulos no pricing principal | 3940/344, não quantidade de cartas |
| Zeros efetivamente recebidos da fonte | 54, distintos de ausência/null; não foram fabricados ou aplicados como venda |
| Pricing sem origem/data ou referência fora do alvo | 0/0 |

A importação e a repetição mantiveram iguais os **12 hashes comerciais** antes/depois: contas, saldo, cartas/preços/estoques, carrinhos, compras/itens e movimentações/auditoria. Importar não criou 207 ofertas. Um caso posterior criou **somente a oferta explicitamente solicitada**, com preço BRL 12,34, estoque 2 e conservação MINT; isso é teste de operação local separado da importação.

## Cobertura substantiva

- Perfis/CSRF/alvo fixo; importação real, nove páginas com 207 IDs únicos, repetição pelo atendente sem duplicação e manifesto idempotente.
- Falha HTTP/timeout sem cache, fallback pt→en, imagem ausente e arquivo temporariamente indisponível, cache preservado, resposta malformada sem envenenar arquivo válido, manifesto duplicado/fora da contagem recusado.
- Preço ausente não vira zero; variantes dinâmicas e null preservados; EUR/USD, fornecedor e data apresentados; último pricing válido/data/origem preservados e mostrado como histórico.
- NM/SP compartilham referência; identidade divergente ou nome remoto alterado recusa vínculo e não muda dados locais. Snapshots sintéticos temporários foram restaurados em finally.
- Formulário usa metadados, deixando preço/estoque realmente vazios; exige campos comerciais do operador. Nenhuma conversão para BRL ou conservação inferida.
- Com `TcgDex:RedeHabilitada=false`, biblioteca/detalhes/imagens e reimportação por cache continuam; compra selecionada de oferta referenciada, remanescente, replay e estorno funcionam sem API externa.
- Regressão 45: gestão/perfis/CSRF/validações, compras/recusas/preço/UUID, última unidade/saldo/cancelamentos concorrentes, estorno original, versão esperada, auditoria e rollback após escrita.

Fixtures são apenas HttpMessageHandler/DI em tests; nenhum servidor adicional, rota de falha ou mudança de produto para simular erro. O modo offline é configuração legítima do processo, sem alterar rede/firewall/segurança do Windows. SQL comercial/histórico de teste foi preservado, sem reset.

## Evidências e versões preservadas

Em `docs/evidencias/tcgdex151/`:

| Arquivos/pastas | Conteúdo |
| --- | --- |
| live-final/ | 23 aprovados D226; manifesto de produto/harness/fontes, progresso real e hashes comerciais antes/depois |
| offline-final/ | Dois aprovados na mesma D226, rede TCGdex false e processo/cache próprios |
| regressao-final/ e regressao-final-execucao.txt | 45 aprovados, manifesto e encerramento do processo do wrapper |
| reconciliacao-final.json | Scripts 007/022 exit 0; primeira tentativa de medição com erro de CTE preservada |
| medicao-reteste.json | Consulta de medição corrigida, exit 0, contagens reais e zero triggers temporárias |
| live-01/, offline-01/, identity-01/, pricing-ui-reteste/ | Tentativas A68, incluindo reprovação de rótulo e reteste de maior intervalo; históricas |
| regressao-gate-versao.json e log da tentativa | Gate recusou hash divergente antes de iniciar regressão/processo |

O relatório detalhado está em `docs/testes-tcgdex151.md`; códigos e fixtures em `tests/Tcgdex151/`. Nenhuma senha/token foi gravada nas evidências; contas sintéticas próprias e senhas aleatórias somente em memória. Snapshot/cache anterior foi preservado em runtime separado, não substituído enquanto ativo.

## Problemas e tratamento real

1. **Rótulo histórico A68:** identity-01 registrou ausência do rótulo em coletas próximas. Reteste A68 com intervalo 54,528 s passou, mas não provava o caso inferior a um segundo. Desenvolvimento mudou a comparação na D226; a execução final sem espera artificial passou com intervalo medido de **0,028 s**. Falha e resultados anteriores permanecem.
2. **Cache HTTP 200 inválido:** Desenvolvimento corrigiu validação antes de substituir arquivo. FX10/FX11 exercitaram pricing malformado, IDs duplicados e contagem errada na D226, preservando o cache válido. Não declarar que essa correção foi testada na A68.
3. **Mudança de hash:** o gate bloqueou a tentativa antiga; nenhuma regressão foi iniciada com binário diferente do liberado. Após liberação D226, a cópia isolada nova foi usada.
4. **Medição SQL:** primeira CTE de leitura falhou com erro 240 de tipos; script/saída preservados, casts corrigidos e reteste passou. Não foi defeito do produto e não houve escrita. Casos já aprovados não foram repetidos por essa medição.
5. **Build do harness:** 0 erros, warning NU1900 por consulta NuGet indisponível; não equivale a auditoria de dependências aprovada. Produto teve seu build pela frente responsável.

## Reconciliação, encerramento e limites

007/022 não apontaram divergência comercial. A medição final confirmou zero triggers `tr_test_%`; falhas de rollback da regressão foram temporárias apenas em PokeVaultTests. Processos de teste online/offline foram encerrados em sequência; último processo 5188 foi **PID 25284**, encerrado pelo wrapper às **22:58:11.9036102 UTC / 19:58:11 São Paulo**. Nenhum listener 5188 desta frente permanece ativo.

Não houve revisão visual: limitação anterior de computer-use respeitada sem contorno; HTML/HTTP não comprova aparência, teclado ou acessibilidade visual. Sem teste de carga, falha física de disco/rede, garantia estatística ou cobertura universal. Fallback inglês foi simulado, não observado na coleta real atual. Contagens e preços são fotografia datada, não compromisso de disponibilidade futura. Conta Windows dedicada é recomendação futura, não bloqueio acadêmico local; nenhuma conta/serviço/infraestrutura foi criada.

Entrega pronta para coordenação consolidar o diário: fonte final executada, importação/cache/offline/vínculos e regressão comprovados nos limites acima. Não inicia outra rodada, melhorias suspensas, mobile ou reescrita acadêmica.
