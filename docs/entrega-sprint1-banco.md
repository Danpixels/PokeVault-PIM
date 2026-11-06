# Entrega da sprint 1 — banco de dados

Registro efetivo: **21/10/2026**, frente SQL Server, trabalho local com apoio de IA. A data 07/09/2026 é referência informada de início do projeto, não data de implementação ou teste desta entrega. A sprint não está declarada concluída.

## Resultado disponível para revisão

- `docs/contrato-sprint1.md` publicado antes dos scripts: nomes, parâmetros/TVP, campos de retorno, códigos de erro, seleção do carrinho, idempotência e integração com hash C#.
- `database/000_preparar_base.sql`: cria somente PokeVault/PokeVaultTests ausente; recusa base existente sem identificação do projeto; não apaga/reinicializa base.
- `database/001_schema.sql`: 11 tabelas de negócio + SchemaVersao; FKs, CHECKs, índices, restrições de domínio, chave de operação e movimentos únicos.
- `database/002_tipos.sql`: TVP ItemCompraSelecionadoType, com CartaId, Quantidade e PrecoConfirmado.
- `database/003_procedures_compra.sql`: compra selecionada atômica e cancelamento integral idempotente.
- `database/004_procedures_operacao.sql`: seis procedures de saldo, cadastro/ajuste de carta, cadastro/ativação de usuário e carrinho. Total: oito procedures no pacote.
- `database/005_seed.sql`: perfis, coleção MEW-151, Pikachu NM R$30/2, SP R$20/3, Charmander NM R$15,50/1; sem usuários, senhas ou hashes. Reexecução não reseta cartas existentes.
- `database/006_permissoes.sql`: papel PokeVaultRuntime com SELECT/EXECUTE e DML direto negado; não cria login nem associa contas existentes.
- `database/007_verificar.sql`: consultas somente de leitura para metadados e divergências de saldo, estoque, totais, movimentos e carteiras. Preparadas, não executadas.
- `database/README.md`: ordem, comandos de aplicação com parada em erro, proteção de base, bootstrap e verificação posterior.
- `docs/plano-banco.md`: atualizado para as decisões aprovadas; checkout selecionado substitui a proposta anterior de carrinho inteiro.

Somente os arquivos autorizados desta frente foram escritos. Não foram alterados src/, tests/ ou documentos centrais; não houve criação de chats nem envio de mensagens a outros chats.

## Verificações efetivamente realizadas

| Verificação | Resultado observado | Limite |
| --- | --- | --- |
| Leitura de DECISOES.md/diario-sprints.md e material anterior | Contratos conciliados com seleção parcial de linhas, autorizações e massa solicitada | Leitura e revisão não comprovam operação |
| `Get-Command sqlcmd,dotnet` | sqlcmd existente em `C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\SQLCMD.EXE`; dotnet no PATH | Não houve instalação |
| `Get-Service -Name 'MSSQL*'` | SQLEXPRESS, SQLEXPRESS01 e SQLEXPRESS02 Running | Serviço ativo não comprova conexão |
| `sqlcmd -?` | Ferramenta informa versão 17.0.1000.7 e opções `-N[s\|m\|o]`, `-C`, `-b`, `-v` | Versão do cliente, não resposta do servidor |
| Leitura de `HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL` e `...\MSSQL17.<instância>\Setup` | Três instâncias registradas; Version/PatchLevel 17.0.1000.7, Edition Express Edition em todas | Versão nominal obtida do registro; `SERVERPROPERTY` do motor não foi obtido |
| Duas consultas de leitura por sqlcmd à candidata SQLEXPRESS | Ambas terminaram com exit code 1 e erro ODBC/SSL, detalhado abaixo | Nem a lista de bases nem versão do motor foi retornada |
| Inspeção estática PowerShell dos arquivos SQL | 8 nomes de procedures encontrados; todos presentes no contrato; 12 CREATE TABLE no DDL | Conferência textual; não é parser SQL, compilação ou teste transacional |
| Leitura do código de integração disponível em src/, sem alterações | Nomes/parâmetros de compra, cancelamento, carrinho, cadastro de usuário e TVP correspondem ao contrato; leitura de resultado usa CompraId/Status/Total/SaldoAtual/Repetida | Inspeção de código em desenvolvimento, sem build ou execução pela frente de banco |
| Busca textual por DROP/TRUNCATE/CREATE TRIGGER e nomes/valores antigos | Apenas comentário “Sem DROP ou reset” no schema; não há comandos de exclusão de base/tabelas ou triggers no pacote | Não substitui revisão/execução dos scripts |

Para os metadados de versão, a instância candidata é **`.\SQLEXPRESS`**, identificador de registro **MSSQL17.SQLEXPRESS**. Não foi selecionada uma instância operacional comprovada. As demais instâncias não foram usadas como tentativa de contornar a falha.

## Comandos de conexão e erros reais

Primeira tentativa, somente leitura:

```powershell
sqlcmd -S '.\SQLEXPRESS' -E -C -l 5 -t 10 -Q "SET NOCOUNT ON; SELECT @@SERVERNAME AS Servidor, SERVERPROPERTY('ProductVersion') AS Versao, SERVERPROPERTY('Edition') AS Edicao; SELECT name FROM sys.databases WHERE name IN ('PokeVault','PokeVaultTests');"
```

Segunda tentativa, somente leitura, usando transporte local e opção de criptografia opcional anunciada pelo próprio help; não houve alteração de configuração:

```powershell
sqlcmd -S 'lpc:.\SQLEXPRESS' -E -No -l 5 -t 10 -Q "SET NOCOUNT ON; SELECT @@SERVERNAME AS Servidor, SERVERPROPERTY('ProductVersion') AS Versao, SERVERPROPERTY('Edition') AS Edicao; SELECT name FROM sys.databases WHERE name IN ('PokeVault','PokeVaultTests');"
```

Em ambas, foram observadas estas mensagens do cliente, seguidas de mensagem de servidor não encontrado/acessível:

```text
Sqlcmd: Erro: Microsoft ODBC Driver 18 for SQL Server : A criptografia não é suportada pelo cliente..
Sqlcmd: Erro: Microsoft ODBC Driver 18 for SQL Server : Provedor SSL: Credenciais não disponíveis no pacote de segurança
.
Sqlcmd: Erro: Microsoft ODBC Driver 18 for SQL Server : O cliente não pôde estabelecer a conexão.
```

Essas mensagens não comprovam senha inválida, defeito nas procedures ou inexistência do banco. A causa de ambiente não foi isolada. Não houve pedido/rejeição de aprovação automática nesta rodada; não é bloqueio comprovado por auto-review. Não foram alterados serviços, protocolos, certificados ou configurações de segurança, nem instaladas ferramentas.

Uma busca por assemblies locais de parser SQL não encontrou ScriptDom nos resultados acessíveis; rg informou “Acesso negado” em subpastas DATA/Log/Backup/JOBS/Xtp do SQL Server. Não foi feita elevação ou tentativa de acesso aos dados. Nenhum parser SQL foi executado.

## Estado de execução e problemas

**Scripts escritos, ainda não aplicados nem compilados no SQL Server. Nenhuma base criada/modificada por esta frente. Nenhum teste funcional, de rollback, concorrência, idempotência, autorização ou permissão do papel runtime foi executado.** A verificação estática confirma organização textual, não aprovação do sistema.

| ID | Problema/limitação real | Próximo passo |
| --- | --- | --- |
| B01 | Conexão sqlcmd falha no cliente ODBC/SSL | Operador/desenvolvimento estabelece acesso autorizado ao ambiente; repetir consulta somente de leitura e confirmar versão/base antes de aplicar scripts |
| B02 | Impossível consultar se PokeVault/PokeVaultTests existem ou pertencem ao projeto | Usar guarda do script 000; não marcar/adotar/resetar base desconhecida |
| B03 | DDL/TVP/procedures não validados pelo motor | Aplicar em base exclusiva e registrar erros/resultados reais; corrigir contrato/scripts de forma coordenada se necessário |
| B04 | Credenciais/hash do administrador ainda dependem de bootstrap C# | Desenvolvimento usa PasswordHasher<Usuario> com credencial de preparação; provisiona clientes e saldo via procedures, sem senha em SQL/logs |
| B05 | Papel runtime não associado/verificado | Operador cria/mapeia conta exclusiva sem sysadmin/db_owner e valida acesso em sessão própria |
| B06 | Integridade sob concorrência/rollback ainda sem evidência | Frente de testes executa cenários em PokeVaultTests depois de aplicação e integração |
| B07 | Na leitura do Bootstrap.cs disponível, o saldo inicial estava em 100m para ambos os clientes de demonstração | Se esses clientes forem a massa CLI-01/CLI-02, desenvolvimento/testes deve definir explicitamente CLI-02 em R$10 pela procedure; a massa SQL não cria contas nem presume que essa preparação ocorreu |

Decisão técnica explícita do seed: `MovimentacaoEstoque.ResponsavelId` aceita NULL exclusivamente em ENTRADA_INICIAL com motivo SEED_SPRINT1; evita inventar autor humano antes de existir usuário. Operações normais exigem autor. A FK/rotina protege vínculos comerciais; conferir a exceção na revisão de schema.

## Dependências para desenvolvimento e testes

Consumir nomes/tipos/resultados exatamente como publicados. `SaldoAtual` é corrente; testar idempotência por CompraId, estado e ausência de efeitos duplicados, sem exigir saldo igual após outras operações legítimas. Compra verifica chave antes de exigir linhas ainda presentes no carrinho. Cancelamento repetido usa somente CompraId e motivo, sem segunda chave. Atendente pode cadastrar/ajustar catálogo e estoque, mas não definir saldo ou cancelar.

Após resolver acesso: aplicar 000–006 em ordem, bootstrap do administrador, clientes CLI-01/CLI-02 e seus saldos (R$100/R$10), integrar web/API, executar 007 e os testes reais. Contas e dados de teste não foram inventados como já cadastrados. As alegações acadêmicas devem permanecer “scripts preparados, execução pendente” até haver evidências de SQL e integração.
