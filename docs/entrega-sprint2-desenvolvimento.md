# Entrega sprint 2 — desenvolvimento

**Registro real:21/10/2026, America/Sao_Paulo. Estado:implementação e verificações locais desta frente entregues.** A coordenação decide o aceite da sprint com as demais evidências. Não é declaração de validação integral ou início da sprint3. Não foram editados database/, tests/ ou documentos centrais, criados chats ou enviadas mensagens a chats. PDF/trabalho final não foram reescritos.

## Resultado entregue

MVC **e** API para administrador listar/cadastrar clientes e atendentes, definir saldo fictício absoluto com motivo e versão, e desativar/reativar somente atendentes. Admin/atendente listam ativos/inativos, cadastram cartas comerciais em coleção existente e ajustam preço/estoque/ativação com motivo/versão. Navegação por perfil no layout.

Serviços compartilhados `GestaoService`/`LojaService`; SELECTs parametrizados e procedures por SqlClient; nenhum efeito financeiro duplicado em C#. Cliente nasce0/vazio. Hash de senha inicial calculado no servidor; sem segredo em retorno/listagem/evidência. Validação de campos, duas casas monetárias, duplicidade normalizada e versão Base64 de8 bytes. DTOs novos recusam campos JSON desconhecidos; não há perfil livre, desativação de cliente, exclusão física ou reutilização de identidade de carta.

Ajustes usam exclusivamente `usp_DefinirSaldoVersionado` e `usp_AjustarCartaVersionado`. Conflito51012 vira409/VERSAO_DESATUALIZADA; formulário preserva token enviado, desabilita confirmação e exige recarga explícita. Não se busca versão nova para sobrescrever automaticamente. Todas as mutações usam autorização e antifalsificação. Conta desativada é revalidada em toda requisição, incluindo sessão já emitida.

**Restore real concluído; build final0 avisos/0 erros. Últimas verificações:54/54 checks integrados administrativos e22/22 de formulários positivos.** Não são76 casos formais e não substituem Testes. Concorrência real/rollback/auditoria pós013 devem ser atribuídos somente à execução da frente responsável.

## Arquivos e contratos

| Arquivo/área própria | Entrega |
| --- | --- |
| Models/GestaoModels.cs | Entradas restritas e validadas, sem campos de papel/identidade editável indevidos |
| Domain/GestaoModels.cs | Projeções de listagem/detalhe sem senha/hash |
| Application/GestaoService.cs | Casos de uso, políticas, hash e validações compartilhados |
| Infrastructure/SqlGestaoRepository.cs | Consultas explícitas, Base64, parâmetros tipados e procedures |
| Infrastructure/SqlLojaRepository.cs | Integração parcial e mapeamento51012 |
| Controllers/GestaoController.cs e GestaoApiController.cs | Fluxos MVC e REST administrativos/comerciais |
| Views/Gestao/ | Contas, cadastro, saldo, catálogo, cadastro e ajuste de carta |
| Program.cs / layout | Políticas e isolamento por base, menu por perfil |
| dev.ps1 / README | Execução, configuração, snapshot de testes e contratos HTTP completos |
| docs/plano-desenvolvimento.md | Estado atual e continuidade após sprint2 |
| src/evidencias/sprint2/ | Roteiros locais e resultados sanitizados duráveis; não substitui evidências de Testes |

Rotas detalhadas no README: MVC `/admin/clientes`, `/admin/funcionarios`, `/gestao/ofertas` e telas subordinadas; API com prefixo `/api/v1` nas mesmas áreas. GET detalhe de cliente/carta devolve `versao`; PUT saldo e PATCH carta recebem `versaoEsperada`. Gestão lista50 por página, incluindo inativos. Coleções somente consultadas para seleção. Catálogo público/compra/histórico/estorno da sprint1 preservam contratos.

## Versão mínima e Banco

Instância de demonstração `.\SQLEXPRESS` / servidor `DESKTOP-641C55N\SQLEXPRESS`, SQL Server17.0.1000.7 Express, base marcada **PokeVault**. SQL Windows integrado com conta de preparação para esta execução.

Banco aplicou008/009/010 e o complemento **013**, sem reaplicação concorrente por Desenvolvimento. Consulta própria somente de leitura confirmou SchemaVersao1,2,3,4 e `dbo.AlteracaoCarta` existente. README exige versão4, inclusive013. Em base nova:000–006, depois008/009/010/013; não reaplicar procedimentos históricos como upgrade. Banco é responsável pelas evidências de preparação/preservação e publicação, descritas em sua entrega.

013 mantém assinatura/retorno da procedure versionada e acrescenta autor/motivo/valores anteriores e novos para qualquer alteração efetiva de carta, inclusive apenas preço/ativação. A aplicação envia o motivo; persistência/auditoria pertencem à transação SQL. Não se criou uma tela de auditoria adicional. Checks locais de dados anteriores a013 não foram reinterpretados como prova de auditoria retroativa; Testes cobre o complemento em sua rodada própria.

## Execução e identificação do binário

Comandos reais:

```powershell
./dev.ps1 Restore
./dev.ps1 Build
./dev.ps1 CopiarTestes
./dev.ps1 Run
# Verificações próprias, executadas com acesso autorizado no ambiente local:
./src/evidencias/sprint2/verificar-desenvolvimento.ps1
./src/evidencias/sprint2/verificar-mvc.ps1
```

Restore informou projetos atualizados; último build observado exit0,0 avisos/erros,2,32s. Não houve dependência nova nem instalação de ferramenta de sistema. Processo da demonstração reiniciado por esta frente em localhost:5187; processo5188 não foi interrompido por Desenvolvimento.

DLL: `src/PokeVault.Web/bin/Debug/net10.0/PokeVault.Web.dll`.

SHA256:**9B717A9B37A733A9213284C93A6DE2FC574CB66D383281AF1C5B20B7175A5EDC**.

O snapshot `.local/testapp/PokeVault.Web.dll` tem o mesmo SHA256, conferido no manifesto. Mudanças posteriores em README/relatórios/roteiros não alteram o binário. Não há alegação de commit Git.

## Evidências e resultados próprios

| Evidência durável | Conteúdo observado |
| --- | --- |
| src/evidencias/sprint2/resultado-desenvolvimento.json | UTC2026-10-21T17:54:08.2489353Z (14h54 local):54 checks aprovados |
| src/evidencias/sprint2/resultado-mvc.json | UTC2026-10-21T17:55:27.0097042Z (14h55 local):22 checks aprovados |
| src/evidencias/sprint2/resultado-desenvolvimento-tentativa1.json | Primeira tentativa preservada; interrompida por expectativa incorreta do roteiro sobre cabeçalho CSRF retido |
| src/evidencias/sprint2/verificar-desenvolvimento.ps1 | Roteiro administrativo ad hoc, base PokeVault, novas contas/oferta sintéticas |
| src/evidencias/sprint2/verificar-mvc.ps1 | Submissões MVC e validação de conflito/decimal/limpeza de senha |
| src/evidencias/sprint2/manifesto.json | Fontes principais, DLL e snapshot com SHA256 |
| src/evidencias/sprint2/estado-demo-final.txt | Consulta somente de leitura: versão4, auditoria existente e estado dos dados originais |

Nenhuma senha/cookie/hash de senha foi incluído nesses artefatos. Os roteiros recuperam o DPAPI da demonstração apenas em memória e não usam o arquivo de PokeVaultTests. São verificações controladas desta frente; **não executá-los novamente em outra base nem como reset**. A suíte formal pertence a tests/ e docs/evidencias/sprint2/ da outra frente.

Resultados efetivamente observados:

- Listagens/telas200; cadastro cliente/atendente e carta201 na API; listagem sem propriedades de segredo.
- Cliente novo com saldo0, carteira versionada e carrinho vazio. Email duplicado em caixa diferente409; tentativa de incluir perfil arbitrário400.
- Definição de saldo absoluto5 funcionou; reenvio com token antigo409/VERSAO_DESATUALIZADA; três casas decimais400. Conta sintética terminou0.
- Admin não pode usar ativação de funcionário para atingir cliente ou administrador fixo:403. Atendente/cliente não acessam administração de usuários:403.
- Cadastro comercial pelo atendente funcionou; duplicidade409. Ajuste sem mudança preservou versão; mudança de preço/estoque alterou versão; token antigo409 sem sobrescrever.
- Oferta inativa ficou fora do catálogo público e permaneceu na gestão. Cliente não acessa gestão:403.
- Sessão já emitida de atendente passou a receber401 após desativação; reativação permitiu novo login; conta sintética terminou inativa.
- Mutação administrativa autenticada sem token recebeu400 depois de remover efetivamente o cabeçalho do roteiro.
- MVC:saldo1,23 foi interpretado corretamente; formulário desatualizado retornou409 e exigiu recarga. Cadastro de atendente redirecionou à listagem; duplicidade não reexibiu senha. Ativação por formulário funcionou.
- MVC:cadastro de carta com preço1,25 e estoque0 funcionou; ajuste1,75/inativa preservou identidade. Os decimais visíveis continuam pt-BR e os campos de formulário seguem a cultura invariável da sprint1.

## Preservação e registros sintéticos

Não houve reset, troca de senha ou exclusão dos registros originais. Verificações próprias acrescentaram contas e ofertas **explicitamente sintéticas** na demonstração:clientesIDs5/7, atendentesIDs6/8/9 e cartasIDs4/5/6. Esses IDs são observações desta base, não contrato para outras instalações. Clientes novos terminaram com saldo0; atendentes/ofertas sintéticos inativos; movimentos e histórico mantidos. A primeira tentativa deixou seus registros sintéticos preservados em vez de apagá-los. Depois das checagens próprias não houve novas mutações de demonstração nesta entrega.

A consulta final leu os clientes originais com100/10, PikachuNM30/2, SP20/3 e CharmanderNM15,50/1, todas as três ofertas originais ativas; seis compras existentes estavam canceladas. Esta leitura não atribui autoria à sexta compra nem presume que a contagem anterior da sprint1 ainda fosse a linha de base desta rodada. Banco tem comparação própria de preservação do upgrade; novas inclusões sintéticas de Desenvolvimento são adicionais àquela comparação.

## Isolamento e credenciais de testes

Banco liberou PokeVaultTests; **Testes é dono da massa e do processo5188**. Desenvolvimento não executou bootstrap, fixture ou mutações nessa base e não reaplicou013 em paralelo.

README fornece:

```powershell
./dev.ps1 CopiarTestes
# Somente a frente responsável pela massa, após liberação da base:
./dev.ps1 Bootstrap -Base PokeVaultTests
./dev.ps1 Credenciais -Base PokeVaultTests
./dev.ps1 Run -Base PokeVaultTests -Porta 5188
```

Run de testes usa cópia `.local/testapp`, independente do executável de demonstração; o responsável para5188 antes de atualizar a cópia. `POKEVAULT_DATABASE` permite somente as duas bases; sem override, execução direta usa InitialCatalog da conexão. Cookies, nome DataProtection e diretório de chaves são separados por base, evitando vazamento de sessão no mesmo host entre portas.

Credenciais geradas por bootstrap ficam no DPAPI específico da origem/base. Credenciais só imprime em terminal interativo local, recusando redirecionamento. Para automação de Testes, README descreve leitura DPAPI em memória, sem imprimir/persistir segredo. Contas e arquivo protegido da demonstração foram preservados. Separação de nomes/chaves exigiu novo login da sessão web anterior; não alterou a senha.

## Problemas, correções e limites

- A primeira checagem sem CSRF usou WebRequestSession que havia retido X-CSRF-TOKEN de requisições anteriores; a aplicação recebeu token válido e respondeu200. O roteiro foi corrigido para nova sessão com mesmos cookies e sem cabeçalhos. A verificação real sem token retornou400. Não foi classificado como defeito de proteção da aplicação. A tentativa anterior foi preservada.
- Não houve erro de compilação ou defeito funcional confirmado nos fluxos administrativos executados nesta rodada. Isso não garante ausência de defeitos fora desses cenários.
- Não houve nova instalação, mudança global de segurança SQL ou configuração de conta Windows. Banco comprovou política **interna** com WITHOUT LOGIN; conexão runtime externa dedicada de menor privilégio ainda depende de identidade Windows própria configurada por operador. A demonstração segue com a conta Windows de preparação. Não declarar privilégio mínimo externo comprovado.
- Não atribuir concorrência/rollback/regressão integral/auditoria013 aos54+22 checks desta frente. A suíte formal está em execução/publicação pela frente Testes; seus resultados devem ser lidos na entrega própria, sem copiar contagens transitórias como conclusão final.
- Lista de gestão limitada a50/página; catálogo público12 e histórico100 preservados. Sem busca administrativa adicional, módulo de coleções, tela de auditoria, alteração arbitrária de identidade, recuperação/troca de senha ou desativação de clientes.
- Inspeção HTTP e formulários não equivale a ensaio amplo de responsividade/acessibilidade. Essas verificações permanecem limitadas.
- Nenhum mobile/Capacitor, buyback, relatório, logística, pagamento real, nuvem ou publicação iniciado. Não iniciar sprint3 automaticamente.

Entrega finita desta frente pronta para consolidação:implementação compilada, execução administrativa observada, evidências próprias duráveis, integração SQL exata e configuração de testes reproduzível. Aceite final da sprint cabe à coordenação após confrontar Banco/Testes e os limites remanescentes.
