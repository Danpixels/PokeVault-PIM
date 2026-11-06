# PokéVault — demonstração acadêmica local

Aplicação web ASP.NET Core10 MVC/Razor + API REST `/api/v1`, SQL Server, **schema12**. A fonte ativa corresponde aos fluxos das sprints1/2 e à bibliotecaTCGdex151; não depende de binário antigo para funcionar. O fluxo de compra e a gestão da coleção151 foram simplificados a pedido de Daniel. Coleções genéricas e raridades fechadas continuam adiadas. Login e cadastro de usuários têm botão acessível para mostrar/ocultar senha, inicialmente oculta. Mobile e trabalho escrito final ficam para depois.

## Abrir sem digitar comandos

Dê duplo clique em **Abrir-PokeVault.cmd**, na raiz desta pasta. Ele inicia um helper oculto, reutiliza a demonstração se já estiver ativa e abre [PokéVault local](http://localhost:5187). O processo local permanece ativo enquanto o sistema é usado; fechar a aba não encerra o processo.

O computador precisa ter o .NET10 e o SQL Server já instalados, com a base PokeVault preparada no schema12. O abridor verifica o banco antes de abrir a página. Se o SQL estiver parado, conexão incorreta ou schema incompatível, mostra uma orientação. Se5187 estiver ocupada por outro programa/versão, informa o conflito e não encerra processos alheios. Não instala ferramentas, inicia serviços de sistema, cria banco, faz migração, executa bootstrap ou reinicializa dados.

Se o DLL não existir, o helper chama Restore/Build do projeto, sem preparar o banco; falhas ficam em `.local/launcher/`. Com o DLL existente usa esse build normal da fonte ativa. Fontes alteradas por um desenvolvedor exigem novo Build. A abertura usa Windows PowerShell em modo oculto, com política aplicada somente ao processo; não altera política global. O navegador abre normalmente. Há registros técnicos sem credenciais em `.local/launcher/aberturas.jsonl` e logs locais do processo.

## Contas e fluxo da demonstração

Use as credenciais já criadas, inclusive as utilizadas por Daniel; contas/senhas/histórico não foram alterados nesta revisão. Não há cadastro público ou recuperação/troca de senha. O provisionamento existente é uma ação explícita de preparação, nunca uma consequência de abrir a aplicação.

| Perfil | Fluxo disponível |
| --- | --- |
| Público | Catálogo, nome/coleção/conservação e páginas |
| Cliente | Saldo fictício, carrinho próprio, compra selecionada, histórico e detalhe próprios |
| Atendente | Gestão de cartas ativas/inativas; cadastro e ajuste de preço/estoque/ativação |
| Administrador | Gestão comercial, clientes/saldo, atendentes e cancelamento integral de compras |

No catálogo, **Definir no carrinho substitui a quantidade dessa linha**. Dentro do carrinho é possível atualizar/remover, selecionar as linhas e revisar antes de comprar. Somente as selecionadas são compradas; as demais permanecem. A alteração desse fluxo para inclusão aditiva continua adiada. Carrinho não reserva estoque; compra revalida quantidade/preço/saldo. Saldo é fictício, sem banco financeiro, entrega, reserva ou logística.

Administrador informa motivo para definir saldo absoluto ou cancelar compra; o estorno é integral pelo valor/quantidades originais, aplicado uma única vez. Ajustes de saldo/cartas carregam versão esperada; conflito409 exige recarga e nova confirmação, sem sobrescrever compra/ajuste concorrente. Novos clientes começam com saldo0 e carrinho vazio. Só atendentes podem ser ativados/desativados; administrador fixo e clientes não participam desse fluxo de desativação.

Coleções continuam as existentes; raridade permanece texto no cadastro comercial. Não há botão de olho ou novos símbolos. Identidade comercial de carta não é reaproveitada no ajuste. Essas melhorias não devem ser confundidas com o arquivo WIP preservado.

## Configuração e fonte reproduzível

SDK alvo10.0.400 (global.json), runtime ASP.NET Core10. SQL local padrão `.\SQLEXPRESS`, autenticação Windows integrada, base PokeVault, conexão criptografada com confiança no certificado do servidor local escolhido. A conta Windows atual de desenvolvimento é aceita por decisão de Daniel para a demonstração acadêmica. Não são necessários IIS, hospedagem externa, serviço Windows ou conta de sistema dedicada. Isso não configura implantação produtiva.

Configuração privada opcional:`src/PokeVault.Web/appsettings.Local.json` ou `ConnectionStrings__PokeVault`/`ConnectionStrings__Preparacao`. Nenhum segredo deve ser versionado. Exemplo sem senha:

```json
{
  "ConnectionStrings": {
    "PokeVault": "Server=.\\SQLEXPRESS;Database=PokeVault;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;Connect Timeout=5"
  }
}
```

Para desenvolvedores, a partir da raiz:

```powershell
./dev.ps1 Restore
./dev.ps1 Build
./dev.ps1 Run
```

O processo precisa ser interrompido antes de substituir DLLs em uso. `dev.ps1` mantém caches de .NET/NuGet em `.local/`, preserva a configuração global e não usa mais `.local/demo-estavel-sprint2` nem o marcador de suspensão como fallback. Fonte/Build/Run normais entregam o produto atual. Pacotes permanecem Microsoft.Data.SqlClient7.1.1 e Pkcs/ProtectedData10.0.13; não há ORM/migrations/banco em memória.

Schema é exclusivamente dos scripts SQL versionados. **Não aplicar014–018**: rascunhos antigos suspensos. A integraçãoTCGdex usa019–021, com versões1,2,3,4,8,9,10; A atualização024 acrescenta schema12, idioma/acabamento por oferta, snapshots de compra e adição atômica ao carrinho; ver docs/entrega-fluxo151.md. Instalação nova explícita segue database/README e os contratosS1/S2 até013, depois019–021,024 e026, com `sqlcmd -E -C -I -b -f 65001` e guardas de propriedade. Nunca apagar/resetar/adotar uma base desconhecida.

Bootstrap/Credenciais continuam disponíveis somente como ações explícitas de preparação. Senhas geradas ficam protegidas por DPAPI da conta Windows em `src/PokeVault.Web/.local/`; hash no SQL. `./dev.ps1 Credenciais` só mostra em terminal interativo e recusa redirecionamento. Não executar bootstrap para tentar corrigir login de uma conta existente nem publicar esse arquivo. A revisão não reexecutou bootstrap.

## Testes isolados e versão liberada

`src/evidencias/revisao-web/prontidao.json` identifica DLL/snapshot para regressão em PokeVaultTests. Testes controla seu processo5188 e a massa; Desenvolvimento controla5187. Não copiar/recompilar um snapshot que esteja em uso. Após Build, com processo de testes parado pelo responsável:

```powershell
./dev.ps1 CopiarTestes
./dev.ps1 Run -Base PokeVaultTests -Porta 5188
```

O snapshot em `.local/testapp` é independente do executável da demonstração. `POKEVAULT_DATABASE` seleciona a base; execução direta sem override usa InitialCatalog da conexão. Cookies, chaves e aplicação DataProtection são separados por base. Testes prepara suas próprias contas; esta revisão não alterou dados de PokeVaultTests. `/health` confirma somente processo; `/health/ready` verifica acesso ao banco, marcador do projeto e schema12, devolvendo200/503 sem segredo.

Contrato API anterior preservado:token por GET `/api/v1/antiforgery`, cookie de sessão por POST `/api/v1/sessao`, token novo após login e cabeçalho `X-CSRF-TOKEN` nas mutações. GET `/api/v1/ofertas`; GET `/api/v1/carrinho` e PUT/DELETE de `/api/v1/carrinho/itens/{id}` conforme contrato; compra POST `/api/v1/compras` com UUID e seleção; GET histórico/detalhe; POST cancelamento com motivo. PUT no item é absoluto e zero remove. Gestão usa `/api/v1/admin/clientes`, `/admin/funcionarios` e `/gestao/ofertas`, conforme contratosS1/S2. Não há endpoints novos de coleção/adição ativos.

Resultados desta frente:restore/build0 erros/avisos, abertura/início/reuso, erro de banco temporariamente simulado sem parar SQL real e leituras autenticadas dos três perfis. Execução do .cmd solicitou abertura do navegador ao sistema operacional. Isso não é inspeção visual de tela. Captura/navegação automatizada encontraram bloqueio no ambiente pela frente de Testes; não se tentou contorná-lo. Resultados formais de regressão e limites estão na entrega de Testes; não presumir aceite pelo build.

## WIP preservado e recuperação futura

Pasta:`src/arquivo-wip/complemento-web-20261103-190746/`; ZIP adjacente com inventário SHA256 e117 arquivos,109 fontes conferidas. ZIP SHA256:**67CEBE7AB8230560CB18C2EC5638B6470BE627115D41D764F320EE91375554EB**. Foram excluídos segredos locais/.keys/.local e caches; foi preservado o DLL WIP separadamente. README/dev.ps1 e documentos próprios anteriores também estão no arquivo.

Para recuperar futuramente, copiar o ZIP para uma **pasta separada**, verificar inventario-sha256.json e comparar seletivamente com a fonte ativa, somente após nova autorização. Não descompactar por cima do projeto ativo ou aplicar os scriptsWIP agora. O histórico de suspensão permanece em docs/entrega-complemento-web-desenvolvimento.md e src/evidencias/complemento-web/; não é mecanismo de execução atual.

Entrega desta revisão:docs/entrega-revisao-web-desenvolvimento.md. Evidências próprias:src/evidencias/revisao-web/. Documento acadêmico final e mobile não foram iniciados.



## Biblioteca TCGdex — somente151

**Importação real concluída:**207 referências detalhadas em português,207 imagens locais (4.017.294bytes),0ausentes e0fallbackinglês nesta primeira execução. As três ofertas originais tiveram vínculo de identidade conferida para exibir a imagem local; NM/SP continuam ofertas distintas e podem compartilhar a mesma referência. Comparação agregada antes/depois das12 tabelas comerciais ficou idêntica, inclusive preçosBRL/estoques/contas/histórico. A importação não criou207 ofertas e não fez conversão de moeda.

Entre como administrador ou atendente e escolha **BibliotecaTCGdex151** no menu (`/gestao/tcgdex151`). Há busca por nome/número, filtro de raridade, páginas de24, detalhe da carta e preços externos. Em cada detalhe, **Usar referência no cadastro de oferta** pede conservação, preçoBRL e quantidade local, inicialmente sem preço/estoque inventado. Somente a oferta explicitamente cadastrada é inserida. Os dados de nome/número/raridade vêm da referência validada no SQL; duplicidade comercial gera conflito. Cadastro manual anterior continua disponível.

**Atualizar coleção151** inicia trabalho assíncrono, com no máximo3 requisições simultâneas e timeout18s por requisição. Acompanhe o link de status; a página de progresso se atualiza a cada5s enquanto estiver em execução. A repetição simultânea recebe409. JSONs recentes e imagens existentes são reaproveitados; o checkbox de consultar novamente detalhes já em cache força a atualização. Retomar sem forçar reaproveita o progresso/cache. O manifesto idêntico mantém chave/payload/data para idempotência, sem duplicar referências. Falhas não limpam dados anteriores nem fazem retry infinito.

Cartas/pricing são snapshots de referência, não anúncios com disponibilidade. Os dados são preferencialmente pt; fallbacken fica identificado para dados/imagem. Raridade ausente é exibida como não informada; a biblioteca não ativa o select fechado/símbolos adiados. Cardmarket/EUR e TCGplayer/USD aparecem com variantes dinâmicas (inclusive reverse-holofoil), campos e data informada pelo fornecedor. Ausência aparece como **Não informado**, não como zero. Preço zero só aparece se a própria fonte o informou. `PrecosJson` anterior é conservado se uma nova coleta não trouxer pricing válido; o detalhe rotula a coleta anterior por comparação exata das datas. Não interpretar preços externos como preçoBRL ou conservaçãoNM/SP.

Origens oficiais consultadas:[TCGdex REST](https://tcgdex.dev/rest/set) e [objeto da carta](https://tcgdex.dev/reference/card). O detalhe traz fonte/API e datas reais de coleta/fornecedor. As respostas públicas reais usadas na entrega estão nas evidências; não são valores fixos de venda. Ativos da TCGdex são guardados somente no caminho local e continuam sujeitos à origem/créditos do conteúdo.

### Offline, cache e segurança

- JSONs/manifestações/estado:`src/PokeVault.Web/.local/tcgdex151/<base>/`; arquivos são separados por PokeVault/PokeVaultTests.
- ImagensWebP:`src/PokeVault.Web/wwwroot/cache/tcgdex151/<base>/<pt|en>/`; servidas localmente por /cache. Não exigem API no catálogo.
- SnapshotSQL:coleção/referências/logs/itens/vínculos em tabelas novas, separadas do estoque e das finanças.
- Rede:apenas HTTPS `api.tcgdex.net` e `assets.tcgdex.net`,setIdfixosv03.5,IDs001–207,redirectdesabilitado,semURLarbitrária no pedido. Resposta/image limitada a2MB; tipo/assinaturaWebP e estruturaJSON conferidos antes de substituir cache. Arquivo válido não é apagado por resposta inválida.
- `TcgDex__RedeHabilitada=false` pode ser definido **no processo** para impedir consultas remotas e usar apenas cache. Sem cache inicial/API, status informa indisponibilidade e mantém SQL anterior. A primeira importação precisa de acessoAPI.
- Catálogo, carrinho, compra e estorno não consultam APIexterna. A importação é ação explícita protegida por perfil e antifalsificação.

Foi verificada uma falhaHTTP real simulada com proxyinacessível **só no processo auxiliar da demonstração**, sem mudar redeglobal ou SQL:207 referências e207 imagens vieram do cache, mesmoImportacaoId1,0ausentes e12checksHTTP positivos. O processo normal, sem o proxy, foi restaurado e está ativo. Não houve alteração de dados comerciais nesse ensaio. Isso não é afirmar que a TCGdex pública estava fora do ar.

### API e versão de testes

Prefixo `/api/v1/gestao/tcgdex151`, cookie e token `X-CSRF-TOKEN` como nos demais endpoints:

| Método/rota | Uso |
| --- | --- |
| GET base | `?nome=025&raridade=Comum&pagina=1`, lista24/total lógico207 |
| GET `/status` | Progresso/contadores/estado da última tentativa |
| POST `/atualizar` | JSON `{"forcarRede":false}`,202; em execução409 |
| GET `/{id}` | Referência local, ex.sv03.5-025 |
| POST `/{id}/ofertas` | JSON `{ "conservacao":"DAMAGED", "preco":12.50, "quantidadeInicial":0 }`; cria apenas a oferta pedida,201 |

Importador não aceita outrosetId nemURL. API de catálogo público preserva o contrato e acrescenta imagemlocal/vínculo opcional, sem consultarTCGdex. Biblioteca/atualização/cadastro de referência são restritos a gestão. Consulta à API não é disparada na inicialização do sistema.

Versão final da integração:**D226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C**. Prontidão em `src/evidencias/tcgdex151/prontidao.json`. Foi criado snapshot **novo** `.local/testapp-tcgdex151-final` sem sobrescrever o snapshotA68 em uso por Testes. O responsável para5188 antes de trocar a versão. Para executar o snapshotfinal diretamente, após configurar a conexão de testes:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:POKEVAULT_DATABASE='PokeVaultTests'
dotnet .local/testapp-tcgdex151-final/PokeVault.Web.dll --contentRoot (Resolve-Path src/PokeVault.Web).Path --urls http://localhost:5188
```

Depois que Testes parar seu processo, também pode executar CopiarTestes e usar dev.ps1 Run -Base PokeVaultTests normalmente. Não iniciar fixture em PokeVault. Os resultados da suíte formal pertencem à frente de Testes e à sua versão identificada; não recontar smoke próprio como casos formais.

Entrega/evidências:docs/entrega-tcgdex151-desenvolvimento.md e src/evidencias/tcgdex151/. WIPadiado, mobile e trabalho final continuam intocados. A nova migração é específica de referência151; não é retomada de cadastro genérico de coleções, olho, raridades fechadas ou soma de carrinho.

### Gestão e compra da coleção151

Admin/atendente acessam **Gestão de Catálogo** (`/gestao/tcgdex151`): as207 referências aparecem em uma única página, com controles recolhidos por padrão em Colocar em estoque. Cada carta permite criar versões por idioma (pt/en/ja), conservação e acabamento (Normal/Holo/Reverse holo), com preçoBRL e unidades. Cada versão existente tem seu formulário de atualização com proteção de versão e auditoria automática, sem justificativa manual. Zero unidades tira a versão da vitrine. A média, quando disponível, aparece apenas com valor e moeda original, sem data/texto adicional; não preenche/converte o preçoBRL.

A página inicial mostra somente cartas com estoque, agrupadas por número da coleção151, estoque somado e menor preço. O detalhe público lista as versões; **Comprar** soma a quantidade ao carrinho. O ícone de sacola junto ao nome abre o carrinho com coleção/número/estado/idioma/acabamento, preço unitário, subtotal e total selecionado. A compra ainda exige confirmação, valida saldo/estoque/preço e mantém itens não selecionados. Registros antigos não recebem idioma/acabamento presumidos: mostram “não informado”. Novas compras guardam snapshots desses campos; compras antigas conservam os dados históricos disponíveis.


### Criar conta e alterar senha

Use Criar conta no cabeçalho ou no login: somente email/senha, perfil cliente fixo, saldo inicial zero. Senhas de6 a128 caracteres; armazenamento com hash. Clique no seu nome para abrir Minha conta e trocar a senha informando a atual, a nova e a confirmação. Disponível nos três perfis. Outras sessões precisam entrar novamente após a troca. As credenciais DPAPI do bootstrap são somente iniciais: não são atualizadas com a senha escolhida pelo titular. Migração026/schema12, após024. Ver docs/entrega-contas.md.
