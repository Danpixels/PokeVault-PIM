# Entrega sprint 1 — desenvolvimento

**Registro real: 21/10/2026, America/Sao_Paulo.** Frente executada por Codex com apoio de IA. Insumo para a coordenação consolidar o diário; não foram editados documentos centrais, arquivos de Banco ou de Testes, criados chats ou enviadas mensagens a chats. 07/09/2026 permanece somente início de referência informado por Daniel.

## Resultado concreto

Aplicação local implementada e compilada em .NET 10, com MVC/Razor e REST `/api/v1` compartilhando os mesmos serviços. Entregues login/logout, revalidação de conta ativa, catálogo público com três filtros e paginação, carrinho persistente com seleção, confirmação explícita de preço, compra por UUID/TVP, histórico próprio e consulta administrativa para cancelamento integral com motivo.

Compra/estorno/ajustes financeiros pertencem às procedures de Banco; C# não altera saldo, estoque, compra ou movimentos diretamente. Única escrita direta de instalação: INSERT transacional do administrador fixo pelo bootstrap, como previsto no contrato. Demais contas e saldos passam pelas procedures. Sem EF/migrations, reserva, logística, pagamento real, publicação, administração completa ou Capacitor.

**Build final aprovado com 0 avisos e 0 erros. Verificações locais: 30/30 checks HTTP/integrados e 7/7 checks do roteiro MVC aprovados na última execução. Isso não significa 37 casos formais nem aprovação da suíte completa da sprint.** A reconciliação de Banco retornou zero linhas de divergência. Concorrência/rollback sob falha permanecem sem comprovação.

## Ambiente e base efetivamente usados

| Item | Observação real |
| --- | --- |
| SDK/runtime | SDK 10.0.400; ASP.NET Core runtime 10.0.11 |
| SQL | `DESKTOP-641C55N\SQLEXPRESS`, motor 17.0.1000.7, Express Edition 64-bit |
| Instância de conexão | `.\SQLEXPRESS`, autenticação Windows integrada, criptografia e confiança no certificado local escolhido |
| Base | `PokeVault`, criada nesta rodada pelo script 000 após leitura confirmar ausência; marcador PokeVaultProjeto=PIM-2026-Sprint1 |
| Schema | Versão 1; consulta retornou AplicadaEmUtc=2026-10-21 17:17:13.599 |
| Scripts | 000–006 publicados por Banco executados; 007 de reconciliação executado somente para leitura |
| Runtime HTTP | `http://localhost:5187`, ASPNETCORE_ENVIRONMENT=Development; processo iniciado pelo comando dev.ps1 Run |
| Credencial de SQL usada | Conta Windows de preparação; conta runtime restrita **não** provisionada/verificada |
| Base de testes formal | `PokeVaultTests` **não** preparada/usada por esta frente |

O erro ODBC/SSL relatado por Banco e reproduzido inicialmente nesta frente ocorreu no ambiente restrito. A consulta de leitura executada com acesso autorizado fora dessa restrição funcionou e identificou servidor/versão. Não foram alterados protocolos, serviços, criptografia ou certificados do SQL Server para obter acesso. Não houve rejeição de aprovação automática nesta rodada. Esta evidência supera o bloqueio histórico de acesso somente para esta execução e não altera o relato anterior de Banco.

## Arquivos entregues

- `PokeVault.sln`, `global.json`, `NuGet.Config`, `.gitignore`, `dev.ps1` e `README.md`.
- `src/PokeVault.Web/PokeVault.Web.csproj`, `Program.cs`, configuração e perfis de execução locais.
- `Domain/Models.cs`: entidades/DTOs de domínio, resultado idempotente e códigos de conservação.
- `Application/LojaService.cs`: autenticação, autorização e validação compartilhadas; interface de persistência.
- `Infrastructure/SqlLojaRepository.cs`: SELECTs parametrizados, TVP e chamadas às procedures; tradução de recusas SQL/concorrência/falhas técnicas.
- `Infrastructure/Sessao.cs`, `AntiforgeryProblemFilter.cs`, `Bootstrap.cs`: sessão, ProblemDetails de antifalsificação, hash e provisionamento seguro.
- `Controllers/LojaController.cs`, `LojaApiController.cs`: fluxos web/API.
- `Views/Loja/`: catálogo, login, carrinho, confirmação, histórico e compra/cancelamento; layout e CSS próprios.
- `docs/plano-desenvolvimento.md`: atualizado às decisões e à implementação real; este relatório.

Um único projeto, com camadas por módulos/diretórios; não foram criadas bibliotecas separadas de Domain/Application/Infrastructure. Solução não adiciona nem altera projetos de teste. Arquivos de template MVC sem uso principal foram removidos (HomeController e suas views Index/Privacy); dependências estáticas do template permanecem disponíveis, embora o layout use apenas o CSS próprio.

## Comandos e resultados observados

| Ação | Comando/forma efetivamente usada | Resultado |
| --- | --- | --- |
| Criar estrutura | `dotnet new mvc -n PokeVault.Web -o src/PokeVault.Web --framework net10.0 --no-restore`; solução sln e add | Primeira tentativa bloqueada no cache global; repetição com DOTNET_CLI_HOME no workspace criou projeto/solução |
| Restore inicial | `dotnet restore PokeVault.sln --configfile NuGet.Config`, com caches/APPDATA locais | Config global inicialmente bloqueada; depois rede bloqueada (NU1301). Execução autorizada com rede restaurou |
| Resolver alerta de pacote | Referência explícita Pkcs 10.0.13 | Restore inicial de SqlClient trouxe Pkcs 9.0.18 com NU1902/NU1903; restore final não exibiu alertas |
| DPAPI de bootstrap | Adicionar ProtectedData 10.0.13 e restaurar | Corrigiu quatro erros CS0103 do primeiro build |
| Build final | `./dev.ps1 Build` → `dotnet build PokeVault.sln --no-restore` | Exit 0, 0 avisos, 0 erros; última duração observada 1,89 s |
| Consulta prévia SQL | `sqlcmd -S '.\SQLEXPRESS' -E -C -b -l 5 -t 10 ...` fora da restrição | Exit 0; versão/instância identificadas; bases do projeto ausentes antes da criação |
| Preparar SQL | Scripts 000–006 via sqlcmd, `-E -C -I -b -f 65001 -v DatabaseName='PokeVault' -i ...` | Exit 0 após ajuste -I; oito procedures publicadas compiladas pelo motor |
| Bootstrap | `dotnet run --project src/PokeVault.Web --no-build --no-launch-profile -- --bootstrap` | Exit 0 após schema pronto; quatro contas criadas; senha nunca impressa |
| Iniciar aplicação | `./dev.ps1 Run` | Kestrel confirmou localhost:5187 e ambiente Development |
| Checks HTTP integrados | `.local/smoke-desenvolvimento.ps1`, última execução 17:23:59 UTC / 14:23:59 local | 30 checks aprovados; JSON abaixo |
| Checks de formulário | `.local/smoke-mvc.ps1`, última execução 17:23:33 UTC / 14:23:33 local | 7 checks aprovados; JSON abaixo |
| Reconciliação | `sqlcmd -S '.\SQLEXPRESS' -E -C -I -b -W -f 65001 -v DatabaseName='PokeVault' -i database/007_verificar.sql` | Exit 0; nenhuma linha nas consultas de divergência |
| Alinhar saldo CLI-02 | `usp_DefinirSaldo` por IDs consultados por AdministradorFixo/EmailNormalizado, NovoSaldo=10, motivo de alinhamento da massa | Retornou saldo10 e delta-90; consulta final confirmou cliente100/cliente2=10 |

Os comandos completos de preparação, configuração e execução estão no README e em dev.ps1. Caminhos de cache e dados locais não foram versionados. Credenciais `--credentials` só são exibidas em terminal interativo local, com saída redirecionada recusada; esse comando não foi usado para expor senhas nos resultados desta entrega.

## Resultados verificados e evidências

| Verificação real | Resultado observado |
| --- | --- |
| Processo e consultas públicas | /health, /, /entrar, /api/v1/ofertas e /api/v1/antiforgery retornaram 200; catálogo público mostrou os três registros de SQL |
| Filtros | Nome Pikachu + coleção MEW-151 + SLIGHTLY_PLAYED retornaram somente a oferta correspondente |
| Autenticação | Logins API dos três papéis e login web por formulário funcionaram; logout web passou a retornar 401 no carrinho |
| Antifalsificação | Mutação de carrinho autenticada sem token foi recusada com 400 |
| Compra selecionada | NM30 + SP20, uma unidade de cada: Total50, saldo100→50, status CONCLUIDO |
| Carrinho remanescente | Charmander não selecionado permaneceu com uma unidade; foi removido pela API depois da verificação |
| Idempotência | Mesmo UUID/conteúdo retornou mesmo CompraId com Repetida=true, sem novo débito; conteúdo diferente retornou409/CHAVE_REUTILIZADA |
| Isolamento/autorização | Segundo cliente recebeu404 ao consultar compra alheia; cliente recebeu403 ao estornar; atendente recebeu403 ao comprar |
| Cancelamento | Administrador cancelou e repetiu o cancelamento; retorno CANCELADO, segundo Repetida=true, saldo voltou100, estoques NM2/SP3 |
| MVC | Formulários de login, revisão com UUID/preço, compra, cancelamento e logout retornaram as telas/estados esperados após correção decimal |
| Interface visual | Catálogo aberto no navegador e screenshot inspecionado: cabeçalho, filtros, três cards, conservação/preço/estoque legíveis. Não houve ensaio completo de responsividade/acessibilidade |
| Reconciliação SQL | Zero divergências retornadas para saldo/movimentos, estoque/movimentos, total/itens, movimentos de compra/estorno e cliente/carteira/carrinho |

Evidências locais efetivamente existentes, sem senhas/cookies:

- `.local/smoke-desenvolvimento-resultado.json`: UTC `2026-10-21T17:23:59.6017076Z`, 30 resultados aprovados.
- `.local/smoke-mvc-resultado.json`: UTC `2026-10-21T17:23:33.3826706Z`, 7 resultados aprovados.
- `.local/smoke-desenvolvimento.ps1` e `.local/smoke-mvc.ps1`: roteiros ad hoc usados nesta frente, leem credenciais protegidas somente em memória; não são a suíte de tests/.
- `.local/manifesto-desenvolvimento.json`: SHA256 de fontes principais e DLL da versão executada.

Esses artefatos ficam ignorados pelo Git e existem neste workspace. Os roteiros pressupõem a base de demonstração conhecida, contas do bootstrap e estoque/saldo inicial; não são um reset nem devem ser executados indiscriminadamente em outra base. O JSON contém somente a última execução, não todo o histórico de tentativas. Resultados de comandos anteriores também constam das saídas desta conversa. Para uma suíte durável e isolada, a frente de testes precisa trabalhar em PokeVaultTests.

DLL executada: `src/PokeVault.Web/bin/Debug/net10.0/PokeVault.Web.dll`.

SHA256: `39F9651502784DDC3955CAF0E15573B343E1D825E474ECB4076E09876993D9B3`.

Estado final observado na base: compras1,2,3,5 CANCELADO por50 cada; compra4 CANCELADO por30. Compras1/2 vieram de roteiros interrompidos e foram canceladas pela API administrativa, sem apagar histórico. Saldos finais100/10; estoques do catálogo voltaram2/3/1; carrinho usado pela verificação foi limpo pelas operações normais. IDs foram descobertos nas respostas/consultas, não assumidos como contrato do bootstrap.

## Problemas encontrados e tratamento

1. **QUOTED_IDENTIFIER:** primeira execução de001 falhou em CREATE INDEX com erro1934. A transação do schema foi revertida;000 já havia criado a base marcada. Reexecutar001–006 com sqlcmd `-I` resolveu, sem modificar scripts de Banco. Seu README deve incorporar essa opção futuramente pela frente responsável.
2. **Bootstrap antecipado:** primeira chamada ocorreu após a tentativa de schema recusada e falhou com objeto dbo.Usuario inexistente (208). Repetição após001–006 completos passou. O arquivo de credenciais já protegido permitiu retomar sem perder senhas. Não houve recuperação por DML financeiro direto.
3. **DPAPI/EventLog na restrição:** execução inicial restrita não conseguiu proteger chaves DPAPI e o log EventLog também não tinha acesso. Execução autorizada fora da restrição funcionou; provedores foram limitados a Console. Chaves não foram gravadas sem proteção para contornar.
4. **Arquivo em uso:** uma recompilação enquanto o servidor estava ativo falhou por bloqueio do executável (MSB3021/3027). Servidor interrompido e build repetido com êxito. README orienta parar antes de compilar.
5. **Defeito MVC decimal real:** preço oculto30.00 foi interpretado pela cultura do host como outro valor, gerando PRECO_ALTERADO. Corrigido com cultura invariável explícita na leitura dos formulários; exibição monetária continua pt-BR. Fluxo MVC completo e regressão API passaram depois.
6. **Erros dos roteiros locais:** codificação inicial da seleção em array de formulário foi corrigida para pares repetidos; ProblemDetails em bytes passou a ser decodificado como UTF-8. Uma composição de comandos conferiu indevidamente LASTEXITCODE após script PowerShell, interrompendo antes de007 apesar dos30 checks aprovados;007 foi executado separadamente com exit0. Esses problemas de roteiro não foram classificados como defeitos da aplicação.
7. **Saldo CLI-02:** bootstrap inicialmente dava100 a ambos. Código corrigido para segunda conta10 sem reset em reexecução; a conta existente foi ajustada por usp_DefinirSaldo com motivo e delta auditado-90. Valor final10 verificado em SQL. Assim há evidência tanto textual quanto executada da correção.

## Limites e pendências finitas

- Não executar sprint2 nesta rodada: cadastro/gestão completa de clientes, saldo, funcionários e catálogo/estoque continuam pendentes de telas/endpoints; procedures já publicadas não equivalem a UI entregue.
- Conta runtime exclusiva com papel PokeVaultRuntime ainda precisa ser configurada por operador e ter SELECT/EXECUTE e negação de DML comprovados sem sysadmin/db_owner. O padrão Windows integrado usado nesta verificação tem privilégios de preparação.
- A frente de testes ainda deve executar sua suíte em PokeVaultTests: concorrência por última unidade/saldo, UUID/cancelamento concorrentes, rollback após escrita/falha controlada, alterações reais de preço durante confirmação e invalidação por desativação com cookie existente. Nenhum desses critérios foi declarado aprovado aqui.
- Login usa hash, antifalsificação e limite de tentativas, mas não há fluxo de recuperação/troca de senha. Arquivo DPAPI depende da conta Windows; não deve ser publicado.
- Bootstrap não redefine saldo em reexecução. Uma interrupção entre criação de cliente e saldo inicial pode exigir completar o provisionamento pela procedure com conferência do estado; não reinicializar contas existentes às cegas.
- Catálogo pagina12 e histórico limita100; sem imagens obrigatórias, detalhe de oferta dedicado, relatórios, buyback ou filtros adicionais.
- UI foi inspecionada visualmente no desktop; testes amplos de acessibilidade/responsividade permanecem pendentes.
- Mobile Capacitor, autenticação entre origens, ferramentas Android, rede de dispositivo e implantação/publicação não foram iniciados.
- README e plano atualizados; documento acadêmico/diagrama e diário central devem ser atualizados pelas frentes responsáveis a partir dessas evidências, sem presumir encerramento da sprint.

A entrega é funcional no fluxo básico local observado e está pronta para a revisão integrada da coordenação e para a próxima execução formal de testes. Não há alegação de reunião, contribuição de integrante humano ou teste anterior à data real desta rodada.
