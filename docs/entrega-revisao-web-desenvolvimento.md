# Entrega — revisão da web acadêmica local

**03/11/2026, America/Sao_Paulo. Entrega finita de Desenvolvimento concluída.** Fonte ativa reproduzível em schema4, WIP preservado, abertura por duplo clique e demonstração ativa. Não retoma melhorias adiadas, mobile ou trabalho acadêmico final. Aceite da revisão é consolidado pela coordenação com Banco/Testes; esta entrega separa resultados próprios e das outras frentes.

## Fonte ativa e WIP preservado

Antes de qualquer isolamento, o complemento foi copiado integralmente no escopo de fontes/configuração própria para:

- Pasta `src/arquivo-wip/complemento-web-20261103-190746/`;
- ZIP adjacente `complemento-web-20261103-190746.zip`;
- Inventário `inventario-sha256.json`,117 arquivos;109 arquivos da fonte original conferidos contra suas cópias;
- ZIP SHA256:**67CEBE7AB8230560CB18C2EC5638B6470BE627115D41D764F320EE91375554EB**;
- Registro de preservação em `src/evidencias/revisao-web/arquivo-wip.json`.

O arquivo contém o projeto de fonte completo do WIP, assets, README/dev.ps1/configuração pública/solução, relatório anterior próprio e DLL do rascunho. Exclui appsettings.Local.json, .local, .keys, caches e demais bin/obj; a DLL WIP foi copiada expressamente para binario-wip. Não há cópia de credenciais ou chaves de teste. Os arquivos ativos novos só foram retirados após conferência de hash da cópia correspondente; não houve git reset, limpeza de base ou apagamento de trabalho sem preservação.

Recuperação futura:copiar/extrair o ZIP **para outra pasta**, conferir inventário e comparar seletivamente com o produto atual, após nova autorização. Não extrair por cima do projeto ativo nem aplicar014–016. O código adiado continua no arquivo e o histórico da suspensão permanece intacto.

O isolamento da fonte ativa restaurou exclusivamente o comportamento S1/S2:

| Arquivos ajustados | O que foi isolado/restaurado |
| --- | --- |
| Application/LojaService.cs e GestaoService.cs | Retiradas adição e criação de coleção/validação fechada de raridade; preservados casos de uso S1/S2 |
| Domain/Models.cs e GestaoModels.cs | DTOs públicos sem novas propriedades de raridade/total de coleção |
| Infrastructure/SqlLojaRepository.cs e SqlGestaoRepository.cs | Coleções consultam somente3 colunas do schema4; sem procedures novas de adição/coleção; erros0–12 preservados |
| Controllers Loja/Gestao MVC/API | Removidas somente rotas do WIP; PUT absoluto, seleção, checkout, histórico, estorno e gestõesS2 mantidos |
| Models/GestaoModels.cs | Retirado CadastroColecaoModel; validaçõesS2 preservadas |
| Views Login/UsuarioNovo | Senha oculta, sem controle de olho adiado |
| Views NovaCarta/Cartas/Ajustar | Coleções existentes e raridade textual, sem símbolos/select fechado |
| Views Catalogo e layout | Quantidade absoluta e botão Definir no carrinho; menuS2, sem novas coleções/scriptWIP |
| wwwroot/css/site.css | CSS anterior sem regras exclusivas das melhorias |
| dev.ps1 | Retirada dependência operacional de SUSPENSO.json e do binário antigo; Run usa build normal |

Arquivos exclusivos de WIP retirados **somente da fonte ativa**, já arquivados:Domain/Raridades.cs, partials _PasswordToggle/_Raridade, views Colecoes/NovaColecao, controls.js e rarities.svg. O projeto ativo fica dentro de src/PokeVault.Web; a pasta de arquivo é irmã e não entra na compilação. Não foram alteradas fontes alheias em database/, tests/ ou documentos centrais.

## Compilação e versão pronta

Comandos efetivamente executados:

```powershell
./dev.ps1 Restore
./dev.ps1 Build
./dev.ps1 CopiarTestes
```

Restore real informou dependências atualizadas. Build final:**0 avisos,0 erros**, exit0, última duração2,13s. Nenhum pacote novo, instalação de sistema ou migração. Dois erros transitórios do isolamento foram corrigidos antes da liberação:remoção parcial de uma string de rota com `{ofertaId}` e vírgula de tupla no mapeamento de erros. As tentativas anteriores foram preservadas em build-isolamento-tentativa1/2.txt; não são falhas presentes na versão final.

DLL ativa:`src/PokeVault.Web/bin/Debug/net10.0/PokeVault.Web.dll`.

**SHA256:177F24F52C71190AC3420CD62E34F741B73DD1B7DD4ABF5FA64A392547E33CE4**.

Prontidão publicada às22:11:53 UTC/19h11 local em `src/evidencias/revisao-web/prontidao.json`:schema4, fonte semWIP, snapshot .local/testapp com mesmo hash. Testes foi liberado por esse artefato e controla5188; esta frente não reescreveu o snapshot nem recompilou o binário após liberação. Mudanças posteriores limitaram-se ao helper/README/evidências/relatórios. `manifesto-final.json` identifica as fontes e os scripts finais.

A aplicação agora compila e funciona pelas fontes normais. **Não depende mais de .local/demo-estavel-sprint2, tests/runtime-sprint2 ou outro binário antigo para executar.** Essas cópias e os documentos de suspensão continuam como histórico, sem serem mecanismo de Run.

## Abertura por duplo clique

Entregues `Abrir-PokeVault.cmd` e `Abrir-PokeVault.ps1`. O .cmd chama o helper Windows PowerShell oculto e abre localhost:5187 no navegador padrão. O helper:

- consulta `/health/ready`, que verifica conexão SQL, marcador de projeto e versão4;
- reutiliza um processo PokéVault pronto, sem iniciar outro;
- inicia o DLL atual da fonte ativa se não houver processo;
- se não existir DLL, executa Restore/Build, nunca bootstrap/seed/migração;
- não instala ferramentas nem inicia/para SQL Server, IIS ou serviço Windows;
- informa porta ocupada, indisponibilidade de SQL/schema ou falha de início, sem encerrar processo alheio;
- em falha de início, encerra apenas o processo auxiliar que ele próprio criou;
- deixa logs locais sem credenciais em .local/launcher, sem necessidade de Daniel digitar PowerShell.

O novo endpoint de prontidão é somente leitura e devolve JSON200/503 sem conexão/segredo. `/health` anterior continua informando apenas processo. A alteração de Program/LocalReadiness necessária ao launcher está incluída no DLL liberado. Não há porta de falha de teste, mudança de esquema ou escrita de dado nesse endpoint.

Resultados reais:

| Evidência | Resultado |
| --- | --- |
| launcher-inicio.json | Iniciou o novo build, PID26072, reutilizado=false, pronto |
| launcher-reuso.json | Segunda chamada pronto/reutilizado=true, sem novo PID |
| launcher-banco-indisponivel.txt/json | Override temporário apontou apenas um processo auxiliar a portaSQL inválida; exit1 esperado; servidorSQL real intocado e porta5199 liberada ao final |
| launcher-banco-indisponivel-corrigido.txt | Repetição após corrigir codificação do helper:mensagem em português legível e recusa esperada |
| launcher-cmd.json | Execução da entrada .cmd equivalente ao duplo clique:reutilizado=true, navegadorSolicitado=true às22:14:30 UTC |
| prontidao-http.json | Aplicação pronta/schema4 por HTTP200 |

O primeiro ensaio em Windows PowerShell5.1 revelou acentos incorretos em script UTF-8 semBOM. Abrir-PokeVault.ps1 e dev.ps1 foram salvos com BOM e o ensaio de erro foi repetido com mensagem correta. Não houve mudança no binário liberado a Testes. Nenhuma conexão real foi interrompida para simular o erro; somente variável de ambiente de um processo auxiliar, restaurada em finally.

**Distinção visual:**navegadorSolicitado=true significa que o sistema operacional aceitou o pedido de abertura. Não é comprovação de captura/navegação/inspeção visual automatizada da página.

## Revisão funcional própria e preservação

`src/evidencias/revisao-web/leituras-http.ps1` executou21 checagens positivas de navegação/login, registradas em leituras-http-resultado.json:

- processo/SQL prontos, catálogo, login, API pública e catálogo sem resultado200;
- carrinho anônimo401 e página inválida da API400;
- login das contas existentes de administrador, atendente e cliente200;
- telas administrativas de clientes/atendentes/cadastros, catálogo operacional e compras200;
- telas do atendente e carrinho/histórico de cliente200.

Senhas DPAPI foram lidas apenas em memória; não aparecem nos resultados. **Nenhuma conta, senha, saldo, estoque, carrinho ou compra foi alterada por essas verificações.** Não foi executado bootstrap/fixture em PokeVault ou PokeVaultTests, nem alterado dado produzido por Daniel. Só cookies locais de autenticação foram utilizados. Gestões/checkout mutantes e concorrência são cobertos pela regressão isolada de Testes, não reexecutados na demonstração.

Revisão da fonte/HTML/CSS manteve menu por perfil, labels, foco visível, link de pular conteúdo, mensagens/validações, estados vazios, confirmação selecionada e tabelas com rolagem em tela estreita. Não houve redesign amplo nem conclusão de melhorias adiadas. A tentativa de automatizar navegador da frente de Testes foi bloqueada pelo ambiente ao identificar a URL do Chrome; respeitou-se o bloqueio e não se tentou contorná-lo. Captura visual, teclado real e comparação desktop/estreito não foram executados por esta frente e não são inferidos das respostas HTTP.

Coordenação informou a conclusão da regressão formal do wrapper com45/45 e seis verificaçõesHTML/HTTP pela frente Testes. Esses resultados devem ser citados a partir de `docs/entrega-revisao-web-testes.md` e suas evidências, **não somados às21 leituras próprias** nem tratados como inspeção visual. Não foram repetidas verificações aprovadas após esse retorno.

## Estado final e limites

- Demonstração **ativa em http://localhost:5187**, novo build da fonte atual, SQL/schema4 pronto. O processo permanece em segundo plano; fechar o navegador não o encerra.
- Fonte compatível com schema4; nenhum014–016 aplicado por esta frente. Banco confirmou schema4, ausência de vestígios do complemento e reconciliação, em sua entrega própria.
- Acesso Windows de desenvolvimento atual aceito para demonstração acadêmica. ContaWindows dedicada, IIS, hospedagem e menor privilégio externo **não são requisitos nem impedimentos** desta entrega local, conforme decisão de Daniel. A limitação de contexto de desenvolvimento está documentada, sem mudança global de segurança.
- Não foram adicionadas funcionalidades de olho, coleções, raridades ou adição. Fonte e DLL WIP preservados para futura autorização/discussão final.
- Logs do helper, caches e chaves continuam locais/ignorados pelo versionamento; não há segredo em evidência pública. Não foi criada infraestrutura de sistema.
- README foi reescrito para instruções atuais, preservando sua versãoWIP no arquivo. Documentos históricos de suspensão não foram alterados para fingir continuidade; o mecanismo de execução foi retirado do dev.ps1.
- Ensaio visual automatizado amplo permanece não executado por bloqueio explícito. Relato manual anterior de Daniel é contexto, não prova desta versão nem lista de casos percorridos.
- Mobile, nova sprint e reescrita do trabalho final não foram iniciados. Encerramento/ressalvas são consolidados pela coordenação, sem pedir novas ações de implementação aqui.

Evidências sanitizadas próprias em `src/evidencias/revisao-web/`:arquivoWIP, restore/build e tentativas, prontidão/binário, launcher, leiturasHTTP e manifesto final. A entrega é reproduzível pelas fontes normais, com abertura simples local e WIP recuperável em separado.
