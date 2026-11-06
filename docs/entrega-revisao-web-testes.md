# Entrega — revisão web / testes

**Concluída nesta frente em 03/11/2026**, America/Sao_Paulo. Execução com apoio de IA. Relatório finito para coordenação consolidar no diário; não inicia complemento, mobile ou revisão do PDF.

## Resultado

- **45/45 cenários de regressão aprovados** na fonte final, via `tests/Executar-Sprint2.ps1` realmente executado de ponta a ponta.
- **6/6 conferências suplementares HTML/HTTP aprovadas**, separadas da regressão: estados vazios, rótulos/alertas, navegação por perfil, formulário inválido e filtros/paginação.
- **Revisão visual desktop/estreita não executada.** Computer-use encerrou a vinculação do Chrome porque não conseguiu determinar a URL no Windows com confiança para aplicar sua política. Nenhuma tentativa de contorno. Não equiparar HTML/CSS à inspeção visual.
- Reconciliação final: **zero divergências de saldo/estoque/totais e zero triggers temporárias**; schema 4. Processos próprios5188 encerrados. Demonstração5187 não foi alterada/interrompida por esta frente.

São 51 cenários distintos aprovados nos dois conjuntos 45+6, sem incluir resultados históricos ou o relato manual de Daniel. O wrapper teve duas execuções45/45 nesta revisão: a primeira foi preservada e a final repetiu os 45 após ajustes de isolamento de variáveis e manifesto do próprio wrapper; não somar repetições como nova cobertura. Nenhum defeito funcional aberto foi confirmado neste conjunto final.

## Versão e evidências finais

Desenvolvimento publicou prontidao.json às 22:11:53.9857932UTC, estado PRONTO_PARA_REGRESSAO_ISOLADA, schema 4 e fonteAtivaSemWip=true. Foi lido antes da regressão. Produto executado: **SHA256 177F24F52C71190AC3420CD62E34F741B73DD1B7DD4ABF5FA64A392547E33CE4**. Não compilei o produto nem usei binário WIP/antigo. Compilei somente o harness.

Evidências em `docs/evidencias/revisao-web/`:

| Artefato | Comprovação |
| --- | --- |
| prontidao-produto.json | Liberação e hash da fonte final fornecidos por Desenvolvimento |
| wrapper-final/resultados.json, manifesto.json e encerramento.json | 45 cenários, esperado/observado/IDs, hashes produto/harness/wrapper/validador e término do processo |
| wrapper-final-execucao.txt | Execução real de restore/build do harness, suíte, gate e saída aprovada |
| wrapper-01/ e wrapper-01-execucao.txt | Primeira execução completa preservada |
| html-01/ | Seis verificações complementares, hash do mesmo produto e encerramento |
| validacao-status-historicos.json | Gate usado no wrapper rejeitou um resultado histórico reprovado e um reteste parcial de um caso |
| reconciliacao-final.json | Consultas007/Verificar-Integridade exit 0, sem divergências/objetos de falha remanescentes |
| limite-computer-use.json | Motivo exato da interrupção UI, sem interação/captura/credenciais |

`docs/testes-revisao-web.md` detalha a matriz, reprodução e limites. Contas/cartas/saldos próprios em PokeVaultTests; senhas aleatórias apenas em memória. Nenhuma conta de demonstração ou arquivo de senhas foi usado. Não houve fixture UI nem arquivo temporário de credenciais para navegador.

## Trabalho concreto no wrapper/harness

Wrapper agora resolve a raiz, aceita pasta de evidência e hash esperado, isola variáveis/caches, inicia cópia oculta 5188, espera health, executa suíte, valida base/quantidade/status/IDs e encerra seu PID em finally. Manifesto grava hashes do produto, harness, wrapper e validador. Recusa pasta já existente e não compila produto/aplica DDL/reset.

`tests/Validar-Resultado.ps1` é o gate efetivamente chamado; throw impede aprovação se qualquer cenário reprovar/bloquear ou se a execução tiver menos de 45 casos/IDs repetidos/base errada. Gate foi exercitado com arquivos históricos reais. O exit 0 do console não é aceito sozinho. Execução final normal do wrapper: exit 0, 45 aprovados.

A regressão cobriu compras selecionadas/remanescentes, preços, recusas de saldo/estoque, UUID/última unidade/saldo/cancelamentos concorrentes, estorno original/único, gestão/perfis/desativação/CSRF/validações, versões esperadas, auditoria sem delta e rollback após escrita. Conferências HTML suplementam estados/semântica, sem afirmar execução visual. Histórico anterior e rascunhos adiados foram preservados; fonte ativa das outras frentes não foi alterada por Testes.

## Limites e próximo ponto

Computer-use indisponível nesta execução: não comprovados layout desktop/estreito, teclado/foco visível, contraste/leituras assistivas ou roteiro visual inteiro. Presença de markup semântico é evidência diferente. Relato de Daniel de testes manuais funcionarem permanece relato separado, sem casos especificados.

Build do harness: 0 erros, warning NU1900 por consulta de vulnerabilidades NuGet indisponível; não é auditoria de pacotes aprovada. Produto final tem build 0 avisos/erros nas evidências de Desenvolvimento. Conexão Windows atual foi usada, sem criar identidade/serviço/infraestrutura extra; conta dedicada é recomendação futura, **fora dos critérios obrigatórios**, conforme Daniel.

Último wrapper encerrouPID 11096 às 19:15:28 São Paulo. Consulta final mantém PokeVaultTests em schema 4, com 90 compras, 134 usuários e 189 cartas de teste/seed e histórico preservado. Nenhuma 014–016 aplicada. Não há teste mutante ou processo5188 desta frente ativo ao fim. Próximo ponto é coordenação consolidar o balanço e apresentar a versão local com `Abrir-PokeVault.cmd` e o roteiro proposto, considerando o limite visual. Nenhum avanço de melhorias/mobile autorizado por este relatório.

