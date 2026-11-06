# Entrega sprint 2 — Banco SQL Server

Registro real: **21/10/2026, America/Sao_Paulo**. Contrato publicado antes dos scripts. Sprint 3 não iniciada; trabalho acadêmico final não reescrito. Esta entrega não encerra a sprint 2 nem declara aprovados seus cenários funcionais.

## Liberação para Desenvolvimento e Testes

**POKEVAULTTESTS LIBERADA À FRENTE DE TESTES em 21/10/2026 às 14h49 (17h49 UTC), com complemento de schema às 14h54.** Instância `.\SQLEXPRESS`, servidor `DESKTOP-641C55N\SQLEXPRESS`, motor **17.0.1000.7 Express Edition (64-bit)**. Base criada nesta rodada, com marcador `PokeVaultProjeto=PIM-2026-Sprint1`, schema versões **1, 2, 3 e 4**, TVP e dez procedures instaladas. Não havia PokeVaultTests na leitura prévia.

Banco concluiu a preparação e não executará cenários mutantes nessa massa após a liberação. Testes passa a provisionar suas próprias contas/hashes e dados; **nenhum usuário de aplicação foi criado em PokeVaultTests por Banco**. Não reutilizar arquivo de credenciais de demonstração. Ao término da preparação havia somente os perfis/coleção/três cartas e movimentos de entrada inicial do seed; o estado posterior pertence às execuções de Testes. O usuário SQL de verificação é principal de banco, não conta de login da aplicação.

Estado temporal exato: 013 foi aplicado em PokeVaultTests às **14:54:38.108 locais / 17:54:38.108 UTC**, antes de receber a mensagem da coordenação informando que Testes já executava cenários. Houve publicação de DDL/procedure, não execução de cenário mutante por Banco. Na leitura imediatamente posterior, AlteracaoCarta tinha zero linhas; isso não prova ausência de operações simultâneas. **Nenhuma mudança adicional de schema ou massa será executada por Banco nessa base nesta entrega.** Testes deve identificar versão 4 nas suas evidências e avaliar eventual sobreposição de horário; não foi alegado que o DDL ocorreu em janela exclusiva confirmada. Assinaturas e retornos permaneceram inalterados.

**PokeVault também atualizada** por scripts aditivos 008–010 e 013; dados comerciais preservados, sem reset nem redefinição de credenciais/saldos/estoque/histórico. Novas telas usam as rotinas Versionado do `contrato-sprint2.md`. C# de compra/estorno/provisionamento continua com as assinaturas originais. O complemento 013 apenas publicou schema/procedure/auditoria, sem cenários mutantes na massa liberada.

## Artefatos entregues

| Arquivo | Entrega |
| --- | --- |
| `docs/contrato-sprint2.md` | Token Base64 de 8 bytes, parâmetros/resultados novos, conflito 51012/409, compatibilidade, política de permissões e passagem da base para Testes |
| `database/008_sprint2_versoes.sql` | ADD Versao rowversion NOT NULL a Carta e SaldoCliente; versão 2, guarda/validação, transação, sem atualização de valores comerciais |
| `database/009_sprint2_ajustes.sql` | usp_DefinirSaldoVersionado e usp_AjustarCartaVersionado; versão obrigatória, comparação bloqueada antes de escrita, predicado de versão, movimento da diferença, retorno de token; publicação como versão 3 |
| `database/010_sprint2_runtime.sql` | Principal WITHOUT LOGIN exclusivo do projeto nas bases, papel PokeVaultRuntime, permissões restritas de banco; nenhuma criação de login/alteração global |
| `database/011_sprint2_verificar.sql` | Verificações não mutantes da preparação, SETs capturados e principal/negações SQL |
| `database/012_sprint2_preservacao.sql` | Contagem/SHA256 do conteúdo comercial ordenado, sem retornar usuários/senhas/hash de senha; exclui apenas coluna Versao e tabela técnica SchemaVersao |
| `database/013_sprint2_auditoria_carta.sql` | Tabela AlteracaoCarta e nova publicação da mesma procedure versionada, sem mudar assinatura; versão 4. Preserva motivo/autor também para preço/ativação, sem inventar movimento de estoque zero |
| `database/Preparar.ps1` | Caminho único de inicialização/upgrade com sqlcmd -I/-b/UTF-8, consulta prévia, parada em erro e logs sanitizados |
| `database/README.md` | Instruções atualizadas de preparação aditiva, QUOTED_IDENTIFIER, conta runtime, integração e testes |
| `docs/plano-banco.md` | Estado técnico atualizado à sprint 2 |

Scripts históricos 000–007 e contrato/relatório da sprint 1 não foram modificados. Não foram alterados src/, tests/ ou documentos centrais, criados chats ou enviadas mensagens a outros chats.

## Comandos, acesso e resultados reais

Primeira consulta, restrita, somente leitura: `sqlcmd -S '.\SQLEXPRESS' -E -C -I -b -l 5 -t 10 -Q ...` para servidor/versão/lista de bases/principais do projeto. **Exit 1**, reproduzindo mensagens ODBC Driver 18 “A criptografia não é suportada pelo cliente”, “Provedor SSL: Credenciais não disponíveis no pacote de segurança” e “O cliente não pôde estabelecer a conexão”.

**Repetição autorizada fora da restrição: exit 0**, sem alterar serviço, protocolo, criptografia ou certificado. Confirmou PokeVault existente e PokeVaultTests ausente; nenhuma login de servidor com prefixo PokeVault encontrado. Consulta seguinte, também autorizada, confirmou marcador, versão 1 e histórico da base de demonstração antes de qualquer escrita. Isso caracteriza limitação do contexto restrito observada, não defeito comprovado do SQL ou do esquema. Não houve rejeição automática de aprovação nesta rodada.

| Execução efetiva, com acesso autorizado | Resultado observado |
| --- | --- |
| `sqlcmd ... -v DatabaseName='PokeVault' -i database/012_sprint2_preservacao.sql -o database/evidencias/sprint2/pokevault-antes.txt` | Exit 0; impressão anterior de 11 tabelas comerciais |
| `./database/Preparar.ps1 -Servidor '.\SQLEXPRESS' -Banco PokeVault -Modo Atualizar` | Exit 0; aplicou 008–010 e verificou 007/011; versões 2 e 3 às 17:49:06.450/.514 UTC |
| `./database/Preparar.ps1 -Servidor '.\SQLEXPRESS' -Banco PokeVaultTests -Modo Inicializar` | Exit 0; criou base marcada, aplicou 000–006 e 008–010 e verificou 007/011; versão 1 às 17:49:14.066, 2 às .701 e 3 às .828 UTC |
| `sqlcmd ... -v DatabaseName='<base>' -i database/013_sprint2_auditoria_carta.sql`, nas duas bases identificadas | Exit 0; complemento às 17:54:37.965 UTC em PokeVault e 17:54:38.108 em PokeVaultTests; AlteracaoCarta vazia em ambas. Preparar.ps1 atualizado para incluir 013 em execuções futuras |
| `sqlcmd ... -v DatabaseName='PokeVault' -i database/012_sprint2_preservacao.sql -o database/evidencias/sprint2/pokevault-depois.txt` + Compare-Object | Exit 0; contagens e impressões das 11 tabelas comerciais idênticas; JSON às 17:49:35 UTC registra true |
| Parser PowerShell de `database/Preparar.ps1` | Nenhum erro de sintaxe encontrado |

Todos os sqlcmd de aplicação usaram `-E -C -I -b -W -w 240 -f 65001 -l 5 -t 30`. `-I` corrige a instrução incompleta da primeira entrega: QUOTED_IDENTIFIER é necessário aos índices filtrados já presentes. A preparação usa autenticação Windows da conta autorizada de instalação e não imprime credenciais. 009 cria as duas procedures e as permissões numa transação que atravessa os batches GO; -b é obrigatório para encerrar/rollback em erro de compilação.

O preparador foi complementado depois da execução inicial para incluir 013 e pular 008/009/013 quando suas versões já constam em SchemaVersao, evitando republicar 009 sobre a evolução 013. Essa forma final passou pelo parser PowerShell; não foi novamente executada contra PokeVaultTests para respeitar a massa já em uso. Os logs comprovam a aplicação inicial do preparador e a chamada separada real de 013, não uma nova execução completa da forma final.

## Evidências persistidas nesta frente

Em `database/evidencias/sprint2/`, sem senhas, cookies ou strings de conexão com segredos:

- `preparacao-PokeVault-20261021-174906-141.txt`: aplicação aditiva e resultados 007/011.
- `preparacao-PokeVaultTests-20261021-174913-652.txt`: inicialização da base de testes e resultados 007/011.
- `pokevault-antes.txt`, `pokevault-depois.txt`, `preservacao-resultado.json`: preservação do conteúdo comercial. Tabela vazia possui contagem zero e digest NULL; os demais digests comparam o conteúdo completo ordenado, inclusive dados de usuário, sem exibi-los.
- `complemento-auditoria.txt`: publicação 013, versões finais e contagem zero na nova tabela de auditoria em ambas as bases.
- `pokevault-final-apos013.txt` e `preservacao-final-resultado.json`: comparação integral posterior às 17:55:57 UTC registrou **false**, pois já havia novos cadastros/movimentos na base; não sobrescreve o resultado inicial verdadeiro.
- `verificar-preexistentes.sql`, `pokevault-preexistentes-apos013.txt` e `preservacao-preexistentes-resultado.json`: investigação somente leitura às 17:57:04 UTC comparou os prefixos por PK limitados às contagens do snapshot anterior; os 11 recortes coincidiram por contagem/SHA256 (**true**).
- `manifesto-banco.json`: SHA256 dos scripts, preparador e documentos da versão final desta frente, para identificar o código entregue; não é resultado de testes funcionais.

Os arquivos foram conferidos no diretório após a execução. Os resultados principais também estão nas saídas reais desta conversa. Logs não substituem os testes funcionais de Testes.

Antes/depois em PokeVault: 4 usuários, 2 carteiras, 3 cartas, 6 compras, 10 itens de compra, 15 movimentos de saldo e 23 de estoque; compras/histórico não foram apagados. Números refletem a base na inspeção desta sprint e podem diferir do fechamento anterior porque houve operações legítimas entre rodadas. A adição de rowversion altera somente os novos tokens técnicos, excluídos da comparação de conteúdo.

A revisão final identificou que exigir motivo sem registrar mudanças só de preço/ativação deixaria uma lacuna de auditoria. O complemento 013 resolveu isso com uma tabela adicional de histórico e transação conjunta, sem reescrever o script já executado 009. O modelo final tem **12 tabelas de negócio e uma técnica**. 012 compara as 11 tabelas comerciais preexistentes; a nova AlteracaoCarta foi verificada separadamente e não recebe motivos retroativos inventados.

**Limite da comparação final:** às 17:55:57 UTC, a base inteira já possuía 9 usuários, 4 carteiras/carrinhos, 6 cartas, 21 movimentos de saldo e 27 de estoque. A origem dessas novas escritas não foi atribuída por Banco. Os scripts aditivos desta frente não inserem essas entidades comerciais. A tentativa de exigir igualdade integral lançou erro no comando de comparação e o JSON falso foi mantido como evidência. A investigação subsequente confirmou identidade dos prefixos preexistentes nas 11 tabelas; compras e itens históricos também mantiveram os digests originais. Isso demonstra preservação do recorte observado, não exclusividade de acesso, autoria de escritas ou imutabilidade futura da base inteira. Nenhum dado foi apagado/revertido para obter comparação positiva.

## Privilégios SQL: comprovado e limite

010 criou **PokeVaultRuntimeVerificacao WITHOUT LOGIN**, associado apenas ao papel de banco **PokeVaultRuntime**, em ambas as bases. 011 executou `EXECUTE AS USER`, leu as três cartas e comprovou:

- SELECT permitido; EXECUTE nas rotinas versionadas permitido por checagem de permissão.
- UPDATE direto de Carta, INSERT direto em SaldoCliente e DELETE direto de Compra recusados com **229**; comandos propositalmente sem linhas, sem tocar massa.
- EXECUTE de `usp_DefinirSaldo` legado recusado com **229**; verificação de permissão também confirma negação de `usp_AjustarCarta` legado.
- Chamada à versão nova sem ator válido chegou à autorização da procedure e retornou **51000**, sem DML e sem transação externa.
- Metadados das duas novas procedures: uses_ansi_nulls=1 e uses_quoted_identifier=1. Colunas Versao: timestamp/rowversion, 8 bytes, NOT NULL.

**Limite restante:** principal WITHOUT LOGIN permite comprovar permissões dentro do banco; não constitui login externo da aplicação. O servidor retorna `IsIntegratedSecurityOnly=1` (somente Windows); não há login de projeto existente identificado. Não se criou conta Windows/serviço nem mudou autenticação global para permitir SQL login. A aplicação ainda depende de identidade Windows dedicada mapeada pelo operador, ou outra implantação já autorizada, para uma conexão runtime própria. Não declarar esse critério plenamente concluído. Bootstrap continua com credencial de preparação.

## Estado dos testes e passagem de responsabilidade

Foram executadas preparação SQL, compilação/publicação real das novas procedures pelo motor, inspeção de metadados, reconciliação 007 sem linhas de divergência, verificação de permissões e preservação de dados. **Banco não executou cenários funcionais mutantes**: conflito de formulário, atualização bem-sucedida, concorrência com compra/estorno, rollback por falha após escrita ou HTTP/UI nesta rodada. Esses resultados não são presumidos pela existência das procedures.

Testes assume a massa em PokeVaultTests. Critérios: dois ajustes com mesmo token, ajuste após compra/estorno, alteração de preço/ativação invalidando token, operação sem mudança, autoridade por papel, no máximo um estorno, compra selecionada e falha após escrita. Rowversion pode consumir números mesmo se transação falhar; verificar a versão persistida da linha e os valores/movimentos, não continuidade do contador global.

As procedures preservam recusa de transação externa 51103. A estratégia de falha controlada está descrita no contrato: somente fixture temporária em PokeVaultTests marcada, instalada/removida por Testes com finally e evidência; nenhuma porta de falha na aplicação de demonstração. Não mudar PokeVault para executar cenários.

## Pendências finitas

1. Desenvolvimento integra SELECT de Versao, Base64/8 bytes, procedures Versionado e conflito 51012/409 com recarga/nova confirmação, sem reenvio automático do valor antigo.
2. Testes provisiona contas/massa própria, valida ajustes/concorrência/rollback/autorizações e registra evidência durável.
3. Operador/desenvolvimento define identidade Windows runtime externa de menor privilégio sem alterar segurança global; Banco já fornece o papel e comprova sua política interna.
4. Coordenação consolida esta entrega e os testes no diário único. A sprint 2 não está encerrada por esta entrega e a sprint 3 não foi iniciada.
