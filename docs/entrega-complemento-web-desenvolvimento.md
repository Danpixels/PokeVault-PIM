# Entrega de Desenvolvimento — complemento web suspenso

**03/11/2026, America/Sao_Paulo. SUSPENSO por mudança de prioridade de Daniel.** As quatro melhorias serão discutidas por último, junto do fluxo de compras. Este documento registra somente o estado real de interrupção; não declara conclusão/aceite das melhorias e não autoriza novos avanços.

## O que já havia sido produzido antes da suspensão

- Olho de senha em login e cadastro de senha inicial:partial SVG local, type=button, aria-controls/aria-pressed/rótulo de ação, JS de alternância e CSS. Não houve ensaio de teclado, foco, retenção do valor ou submissão pelo navegador.
- Coleções:modelo/serviço/repository/controller API/MVC e views de listagem/cadastro, formulário Nome/QuantidadeTotalCartas, código retornado pela procedure, total legado nullable e seleção do ID recém-criado no cadastro de carta. Integração depende da coluna/procedure ainda ausentes.
- Raridades:lista canônica de12 nomes ingleses, validação de novo cadastro, select nativo, preview/legenda e sprite SVG local, exibição no catálogo/gestão e fallback textual legado. Imagem de referência fornecida foi inspecionada. Nenhuma conclusão de fidelidade visual/acessibilidade por navegador.
- Carrinho:nova ação web POST `/carrinho/adicionar` e API POST `/api/v1/carrinho/itens/{ofertaId}/adicionar` com `{quantidade}`, serviço e chamada de `usp_AdicionarItemCarrinho`. Catálogo do rascunho usa rótulo aditivo; PUT/substituição e remoção anteriores permanecem separados. Não houve inclusão real nem teste de soma/concorrência/checkout com essa versão.
- Mapeamentos de erros51013/51014 e leitura de raridade pública; código de prevenção de duplo submit nos formulários novos. Não se implementou leitura-modificação-escrita da quantidade em C#.
- Contrato de Banco lido em `docs/contrato-complemento-web.md`. Nenhum arquivo database/, tests/ ou documento central foi alterado nesta frente.

Arquivos principais preservados:Domain/Raridades.cs; Models/GestaoModels.cs; serviços Loja/Gestao; SqlLojaRepository/SqlGestaoRepository; controllers Loja/Gestao MVC/API; views de login/cadastro/catálogo/gestão, novas Colecoes/NovaColecao, partials _PasswordToggle/_Raridade; layout; wwwroot/js/controls.js, images/rarities.svg e CSS. Fonte continua como rascunho, sem apagar ou desfazer trabalho.

## O que efetivamente executou

| Ação | Resultado real |
| --- | --- |
| Leitura de decisões/diário/contrato e imagem | Realizada |
| Restore e build do rascunho, antes da suspensão | Concluídos; último build0 avisos/0 erros,1,10s |
| Cópia de binários para .local/testapp | Realizada antes da suspensão, sem processo5188 identificado na consulta; esse snapshot é rascunho e não deve ser iniciado |
| Início temporário do processo novo em5187 | Kestrel iniciou; nenhum cenário funcional/mutante desta frente foi executado antes da suspensão |
| DDL/seed/ajustes/fixtures de banco por Desenvolvimento | Nenhum nesta rodada |
| Testes funcionais/UI, soma, coleções, raridades, regressão por Desenvolvimento | **Não executados** |
| Estabilização após instrução | Processo do rascunho interrompido fora de qualquer operação SQL desta frente; artefato estável da sprint2 copiado e iniciado |
| Conferência mínima após estabilização | /health, / e /entrar retornaram200; login servido sem controle novo de olho |

Não houve criação/alteração de usuários, senhas, saldos, estoque, carrinho, compras ou histórico por esta frente nesta rodada. Não foram iniciados testes ou novas melhorias após a suspensão. A conferência HTTP final é exclusivamente de estabilidade, sem mutação nem aceite funcional do complemento.

Comandos anteriores à suspensão:`./dev.ps1 Restore`, `./dev.ps1 Build`, `./dev.ps1 CopiarTestes`, `./dev.ps1 Run`. Cache NuGet já estava restaurado; não foram adicionadas dependências ou instaladas ferramentas de sistema. O processo antigo da demonstração foi parado por ID/caminho verificado, preservando testes. A sessão de execução de outubro já não era controlável por write_stdin; foi usado o processo efetivamente identificado.

## Estado do banco e dependências

A entrega atual de Banco informa que a tentativa de014 falhou com erro207; SchemaVersao permanece4 e coluna/procedures novas não foram publicadas. Desenvolvimento não tentou corrigir/reaplicar esses scripts e não tocou em PokeVaultTests. O relatório de Banco é a fonte dos detalhes da operação que estava em andamento quando a prioridade mudou; o início de janela anteriormente registrado não equivale a migração concluída.

Assim, executar o rascunho como versão operacional exporia chamadas a integração ausente. Em vez de completar recurso adiado ou reverter fontes/schema, foi mantido um artefato previamente validado.

## Demonstração estabilizada

- URL:[http://localhost:5187](http://localhost:5187), modo Development, base PokeVault.
- Artefato executado:`.local/demo-estavel-sprint2/PokeVault.Web.dll`.
- SHA256 da sprint2:**9B717A9B37A733A9213284C93A6DE2FC574CB66D383281AF1C5B20B7175A5EDC**.
- Origem:`tests/runtime-sprint2/PokeVault.Web.dll`, apenas lido/copiadο para diretório próprio. Hash coincide com o relatório da sprint2. Nenhum arquivo de Testes foi alterado. Copiados somente assemblies/deps/runtimeconfig e dependências nativas; sem copiar chaves/contas/credenciais de testes.
- `dev.ps1 Run` para PokeVault detecta `src/evidencias/complemento-web/SUSPENSO.json` e executa essa cópia estável, conferindo hash. Ausência/divergência interrompe a inicialização, sem usar o rascunho como fallback.
- ContentRoot continua src/PokeVault.Web para a configuração/credenciais existentes. As Razor Views da versão estável estão compiladas no artefato; as melhorias de fonte não foram implantadas por esse caminho.
- Processo estável deixado em execução ao finalizar. Não foi interrompido o processo5188, nem iniciada fixture/DDL/teste mutante naquela base.

Este uso do artefato estável é estabilização da implantação, não exclusão ou reversão do código produzido. O binário do rascunho permanece em bin/ e no snapshot .local/testapp, identificado separadamente; não reutilizar esse snapshot enquanto suspenso. Build posterior do rascunho não muda automaticamente qual artefato o Run de demonstração utiliza sob o marcador.

## Evidências próprias duráveis

Em `src/evidencias/complemento-web/`:

- `restore.txt` e `build.txt`:saídas reais anteriores à suspensão;
- `SUSPENSO.json`:decisão de não executar o rascunho e referência do artefato estável;
- `artefatos-suspensos.json`:hash do rascunho separado do hash da demonstração;
- `estabilidade-apos-suspensao.json`:UTC2026-11-03T18:16:17.8991402Z (15h16 local), três rotas200, controle novo ausente.

Não há resultado de teste funcional inventado nem senhas/cookies/hashes de senha nesses arquivos. README registra a suspensão e o caminho seguro de execução. Nenhum relatório acadêmico final foi reescrito.

## Próximo ponto seguro

**Parar aqui.** Qualquer retomada exige nova autorização de Daniel/coordenação, revisão do contrato/rascunho e correção/publicação de Banco com janela coordenada antes de integrar/testar. Até lá, manter schema4, fontes preservadas e demonstração no artefato estável. Não remover marcador para publicar rascunho, iniciar novos testes/migrações ou avançar mobile/nova rodada.
