# PokéVault — desenvolvimento e continuidade

Atualizado em **21/10/2026**, America/Sao_Paulo. Referências: decisões/diário centrais, contratos SQL das sprints1/2 e entregas de desenvolvimento de cada sprint. Este plano descreve o estado atual; o início de referência07/09/2026 não é data de execução.

## Arquitetura e implementação atual

Monólito ASP.NET Core10 MVC/Razor + API REST `/api/v1`, alvo net10.0, em um projeto com módulos Domain, Application e Infrastructure. `LojaService` atende os fluxos de compra; `GestaoService` atende a administração. MVC e API compartilham serviços e políticas. `SqlLojaRepository` parcial implementa os dois contratos de persistência por Microsoft.Data.SqlClient.

SQL Server é o único banco. Carta é o registro comercial por coleção/número/conservação; ofertaId corresponde a CartaId. Scripts numerados pertencem à frente de banco; sem EF/migrations. Compra, cancelamento e ajustes pertencem exclusivamente às procedures transacionais. C# não duplica movimentos de saldo/estoque. Schema atual exige versões1–4:008/009/010 e complemento013 após a base inicial;013 mantém a assinatura e acrescenta auditoria de qualquer mudança efetiva de carta.

Dinheiro decimal(12,2), UTC, compra CONCLUIDO/CANCELADO, seleção atômica de linhas do carrinho sem reserva, UUID vinculado ao conteúdo e cancelamento integral idempotente preservam os contratos da sprint1. Ajustes de saldo/Carta usam token rowversion Base64 de8 bytes e só chamam `usp_DefinirSaldoVersionado`/`usp_AjustarCartaVersionado`. Erro51012 vira409; a interface exige recarga e nova confirmação, sem reenviar automaticamente valor antigo com token novo.

## Telas e permissões entregues

| Área | Público | Cliente | Atendente | Administrador |
| --- | --- | --- | --- | --- |
| Catálogo/filtros/páginas | Sim | Sim | Sim | Sim |
| Carrinho/seleção/compra/saldo próprio | Não | Sim | Não | Não |
| Histórico/detalhe | Não | Próprio | Não | Consulta para estorno |
| Cancelamento integral | Não | Não | Não | Sim |
| Listar/criar clientes e atendentes | Não | Não | Não | Sim |
| Saldo absoluto com motivo/versão | Não | Não | Não | Sim |
| Desativar/reativar atendente | Não | Não | Não | Sim |
| Catálogo operacional ativo/inativo | Não | Não | Sim | Sim |
| Cadastro comercial e ajuste preço/estoque/ativação | Não | Não | Sim | Sim |

Menu por perfil: clientes têm carrinho/compras; admin tem Clientes/Atendentes/Compras/Gestão do catálogo; atendentes têm Gestão do catálogo. Cliente nasce com carteira0 e carrinho vazio. Cadastro exige nome/email/senha inicial, hash calculado no servidor. Não há exposição de senha/hash, cadastro público, mudança arbitrária de perfil ou desativação de clientes. Servidor protege administrador fixo e exige alvo ATENDENTE para ativação. Identidade da carta não é editável na rotina de ajuste; novas cartas usam coleções existentes.

Cookie e CSRF protegem web/API, inclusive todas as mutações novas; conta ativa/papel revalidados por requisição. Senhas iniciais têm8–128 caracteres; campos e escala monetária são validados; DTOs novos recusam propriedades JSON desconhecidas. Erro de cadastro MVC não reexibe a senha. Configuração/chaves e credenciais locais permanecem fora do versionamento, protegidas por DPAPI.

## Sequência e evidências reais

Sprints1/2 implementadas em seu recorte de desenvolvimento. Restore/build reais: zero avisos/erros no build finalS2. Nesta frente,54 checks integrados administrativos e22 de formulários passaram nas últimas execuções; resultados duráveis em `src/evidencias/sprint2/`. Incluem versão antiga409, valores decimais MVC, conta nova0/vazia, duplicidade, políticas, sessão existente bloqueada após desativação, reativação, ativo/inativo e CSRF. São checks locais desta frente, não a suíte formal nem aprovação universal de integridade.

Banco preparou PokeVault/PokeVaultTests e publicou schema4; Desenvolvimento apenas conferiu o estado por leitura, sem reaplicar migrações em paralelo nesta sprint. Demonstração continua em localhost:5187. Contas/senhas e registros originais preservados; verificações desta frente acrescentaram dados sintéticos identificados, sem reset. Clienteoriginal100/cliente2=10 e estoques originais2/3/1 foram lidos ao final.

## Execução e isolamento de testes

README e dev.ps1 documentam Restore, Build, Run, Bootstrap e Credenciais. `CopiarTestes` cria snapshot independente de binários em `.local/testapp`; `Run -Base PokeVaultTests` usa esse snapshot e porta5188 por padrão. Base escolhida tem cookies, aplicação DataProtection e chaves próprios, evitando colisões com5187.

Testes controla processo5188 e toda a massa mutante em PokeVaultTests. Desenvolvimento não executou bootstrap, fixture ou mutações naquela base. Credenciais podem ser provisionadas pela frente responsável via Bootstrap para a base selecionada e obtidas em terminal interativo ou DPAPI somente em memória, conforme README. Nunca reutilizar o arquivo DPAPI da demonstração em testes. Snapshot deve ser atualizado somente com o processo de testes parado por seu responsável.

## Contratos e próximos passos

- Banco é dono de schema/procedures/auditoria. Manter versões1–4 e instruções de upgrade; o complemento013 registra preço/ativação mesmo sem delta de estoque. Não há auditoria retroativa inventada.
- Testes publica resultados formais duráveis de autorização, compra/estorno/ajustes concorrentes, rollback e auditoria013. As54+22 verificações de Desenvolvimento não substituem essa matriz. A entrega de Testes deve fundamentar qualquer alegação de aprovação desses critérios.
- Conta SQL runtime externa dedicada ainda depende de configuração por operador. Banco comprovou política interna com usuário WITHOUT LOGIN; isso não comprova conexão externa da aplicação com menor privilégio. Bootstrap usa conexão de preparação.
- Responsividade/acessibilidade ampla e limites reais de implantação permanecem pendentes; o recorte desta sprint não inclui módulo de auditoria visual, coleção nova ou recuperação/troca de senha.
- Coordenar encerramento da sprint2 pelo diário, sem iniciar automaticamente sprint3. PDF/trabalho acadêmico final não foram reescritos nesta rodada.

## Capacitor e demais itens posteriores

Capacitor permanece após estabilização e aceite do web/API. Cliente estático com tecnologias web consome a API; Razor não é bundle estático embarcado. Antes dessa etapa verificar npm, SDK/JDK/Android Studio, plataforma escolhida, endereço HTTPS alcançável e contrato seguro de sessão/token/armazenamento/CORS. Nenhuma ferramenta mobile foi instalada nem houve publicação.

Buyback, relatórios, logística, reserva, pagamentos reais e nuvem continuam fora deste incremento. Ferramentas verificadas anteriormente: SDK10.0.400, ASP.NET runtime10.0.11, Git2.52, sqlcmd17.0.1000.7 e SQLEXPRESS17.0.1000.7; Node24.19 disponível pelo ambiente, npm/adb/Docker não encontrados no PATH inspecionado. Não afirmar ausência global nem testes de mobile.

Organização por responsabilidade, com apoio de IA. Nenhuma reunião, contribuição pessoal ou resultado anterior à execução foi presumido.
