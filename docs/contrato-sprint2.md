# Contrato SQL aditivo — sprint 2

Publicado em 21/10/2026, antes da implementação dos scripts desta sprint. Complementa `contrato-sprint1.md`; decisões e diário centrais prevalecem. Execução/estado de liberação da base: `entrega-sprint2-banco.md`.

## Decisão de compatibilidade

Adicionar `Versao rowversion NOT NULL` a **SaldoCliente** e **Carta**, por script 008, sem apagar linhas ou alterar preços/saldos/estoques. O SQL Server atualiza automaticamente o token quando compra, estorno ou ajuste executa UPDATE na linha. Não é data, não é valor financeiro e não deve ser incrementado pelo C#.

Criar **duas novas procedures obrigatoriamente versionadas**, mantendo as assinaturas de compra, cancelamento, criação de conta/carta e provisionamento da sprint 1. Procedures antigas `usp_DefinirSaldo` e `usp_AjustarCarta` ficam preservadas para preparação/compatibilidade com conta de instalação, mas o papel **PokeVaultRuntime recebe DENY EXECUTE nessas duas**. Novas telas/endpoints usam exclusivamente as novas rotinas. Não introduzir parâmetro opcional de versão que permita ignorar o conflito na aplicação runtime.

SchemaVersao recebe versão 2 (008), publicação operacional versão 3 (009) e complemento de auditoria versão 4 (013); a entrega final desta sprint requer as três, além da inicial 1. Não reaplicar 003/004/006 em upgrade, pois são a publicação histórica da sprint 1; aplicar os scripts novos por último também numa base nova. Compra, estorno, seed e bootstrap com credencial de preparação continuam compatíveis. Scripts 000–007 não serão modificados nesta sprint.

## Token entre SQL, C# e formulário/API

- SELECT de gestão: incluir `s.Versao` ou `c.Versao`; SQL retorna `byte[8]`/`binary(8)`.
- DTO de gestão: `versao` em **Base64** por `Convert.ToBase64String(bytes)`. Na submissão, `versaoEsperada` Base64; formulário oculto carrega exatamente esse texto. Decodificar e exigir exatamente 8 bytes; vazio, Base64 inválido ou tamanho diferente => 400, sem SQL mutante.
- Parâmetro: `SqlDbType.Binary`, Size=8; enviar bytes, nunca número, string Base64 ou token hex diretamente ao banco.
- Um erro SQL 51012 significa **VERSAO_DESATUALIZADA**, HTTP 409. Exibir mensagem para recarregar os dados e confirmar novamente. Não fazer leitura e reenvio automáticos do mesmo valor absoluto com token novo: isso anularia a proteção.
- Outro ajuste, compra ou estorno pode invalidar a versão mesmo quando o saldo/estoque final coincide com o anterior. Isso é correto. O token é opaco e vale apenas para aquela linha/base.
- Resultados de atualização bem-sucedida devolvem `Versao binary(8)` capturada dentro da transação. Nova leitura pode ter token diferente por uma operação posterior legítima.
- Se versão coincidir mas valor de saldo já for igual, retornar delta zero e a mesma Versao sem movimento/UPDATE. Para carta, nenhum UPDATE se preço, estoque e ativação forem todos iguais; motivo ainda obrigatório. Não testar divergência por igualdade de contador financeiro.

## Definir saldo com proteção

```text
dbo.usp_DefinirSaldoVersionado
  @AdministradorId int,
  @ClienteId int,
  @NovoSaldo decimal(12,2),
  @Motivo nvarchar(300),
  @VersaoEsperada binary(8)
```

Administrador ativo; cliente ativo; saldo absoluto não negativo; motivo obrigatório. Bloqueia carteira com UPDLOCK/HOLDLOCK, compara versão **antes de qualquer escrita**, registra diferença/autor/motivo e retorna nova versão na mesma transação. Predicado de UPDATE inclui VersaoEsperada. Concorrência com compra/estorno é serializada pela mesma carteira. Uma versão antiga nunca sobrescreve o saldo calculado por outra operação.

Resultado único/uma linha, na ordem: `UsuarioId int`, `Saldo decimal(12,2)`, `Delta decimal(12,2)`, `Versao binary(8)`.

## Ajustar carta com proteção

```text
dbo.usp_AjustarCartaVersionado
  @ResponsavelId int,
  @CartaId int,
  @Preco decimal(12,2),
  @NovoEstoque int,
  @Ativo bit,
  @Motivo nvarchar(300),
  @VersaoEsperada binary(8)
```

Atendente ou administrador ativo; preço positivo; estoque absoluto não negativo; motivo obrigatório. Bloqueia a carta, compara versão antes de escrita e atualiza preço/estoque/ativação com predicado de versão. Registra delta de estoque quando diferente de zero. Mudança só de preço/ativação também invalida a versão da linha; não inventa movimento de estoque zero.

Resultado único/uma linha, na ordem: `CartaId int`, `Preco decimal(12,2)`, `QuantidadeEstoque int`, `Ativo bit`, `DeltaEstoque int`, `Versao binary(8)`.

Identidade comercial (coleção/número/conservação/nome/raridade) não é editada por essa rotina. Não reutilizar produto já vendido. Cadastro continua em `usp_CadastrarCarta` sem versão esperada por não existir linha anterior; após retorno do ID, SELECT lê a versão para edição futura.

Complemento publicado/aplicado às 14h54 de 21/10/2026: script `013_sprint2_auditoria_carta.sql` acrescenta **AlteracaoCarta**, com CartaId, ResponsavelId, Motivo, PrecoAnterior/PrecoNovo, EstoqueAnterior/EstoqueNovo, AtivoAnterior/AtivoNovo e CriadaEmUtc. A mesma `usp_AjustarCartaVersionado`, com assinatura/retorno inalterados, registra uma linha por alteração efetiva, inclusive só preço ou ativação. Não grava auditoria nem movimento quando não há mudança; versão antiga continua sendo conflito antes dessa avaliação. Movimento de estoque continua somente para delta diferente de zero. Auditoria e atualização pertencem à mesma transação; falha ao gravar auditoria desfaz valores/movimentos. A tabela é de acréscimo, DML direto negado ao runtime pelo papel. Não há carga retroativa de motivos desconhecidos. Essa evolução é aditiva em 013; 009 já executado permanece histórico.

## Listagens e gestão de usuários

Não criar SELECT * nem expor SenhaHash em telas/listagens. Login continua com a consulta específica da sprint 1. Consultas de gestão parametrizadas sugeridas:

- Clientes: Usuario JOIN Perfil por Codigo=CLIENTE, LEFT/JOIN SaldoCliente; projetar UsuarioId, Nome, Email, Ativo, Saldo e s.Versao. Apenas administrador acessa.
- Funcionários: Usuario JOIN Perfil por Codigo=ATENDENTE; projetar UsuarioId, Nome, Email, Ativo. Não incluir administrador fixo na lista de funcionários; protegê-lo também no servidor.
- Catálogo operacional: Carta JOIN Colecao, incluir ativos e inativos e c.Versao; administrador/atendente. Catálogo público continua somente ativos e não precisa expor token.
- Coleções existentes: ColecaoId, Codigo, Nome para seleção no cadastro. Sem novo módulo de coleção.

`usp_CriarUsuario` já cria cliente com carteira zero e carrinho vazio, ou atendente sem carteira. Hash é produzido pelo servidor C# a partir da senha inicial do formulário; senha/hash nunca retornam na listagem ou nas evidências. Desativação/reativação de atendente usa `usp_DefinirUsuarioAtivo`; o serviço deve exigir que o alvo seja ATENDENTE. A sprint mantém a proteção do administrador fixo e não permite edição arbitrária de perfil. Revalidar conta ativa em cada requisição autenticada.

## Permissões e identidade SQL

Scripts do projeto não alteram autenticação, protocolos, criptografia ou certificados do servidor. Será preparado um principal **de banco** exclusivo para comprovar SELECT/EXECUTE e negação de DML, quando possível. Um usuário WITHOUT LOGIN validado por EXECUTE AS USER comprova as permissões no banco; não comprova uma conexão externa da aplicação com conta própria. A entrega distinguirá esses níveis e qualquer limite de implantação da conta runtime.

O papel runtime não pode executar os ajustes legados sem versão; bootstrap/fixture usa credencial de preparação, nunca a conexão runtime para inserir administrador ou definir saldo inicial por rotina legada. A identidade de usuário web continua extraída da sessão pela API; o SQL valida o papel do ID informado mas não conhece o cookie.

## Testes, rollback e posse da base

Banco prepara `PokeVaultTests` apenas se ausente ou com marcador de projeto válido; aplica schema/atualizações/seed, faz verificações de preparação e informa liberação no relatório desta frente. **Depois da liberação, a frente de testes é dona da massa e dos cenários mutantes; Banco não dispara smoke mutante em paralelo.** PokeVault e suas credenciais/compras/histórico são preservados.

Não adicionar modo de falha, parâmetro de erro, trigger de teste ou backdoor às procedures da demonstração. As procedures continuam recusando transação externa (51103). Para comprovar rollback após escrita, a frente de testes pode instalar **somente em PokeVaultTests marcada**, com credencial de preparação, uma restrição/trigger de teste temporária que provoque erro em uma escrita posterior (por exemplo INSERT de movimentação após UPDATE). A própria fixture deve verificar DB_NAME/marcador, remover o objeto em finally e registrar a limpeza. Não colocar esse objeto em PokeVault nem como parte da atualização publicada do produto.

Alternativa: ambiente SQL de testes isolado com mecanismo externo de falha controlada. Não usar BEGIN TRAN externo para simular rollback das procedures atuais; elas o recusam por contrato. Cada cenário deve provisionar dados próprios e preservar evidência, sem reset da base de demonstração.

## Erros e ordem de aplicação

Preservados 51000–51011 do contrato anterior. Adicionados `51012 / VERSAO_DESATUALIZADA / HTTP 409`; versão NULL chega como `51002 / DADOS_INVALIDOS / HTTP 400`. Demais falhas técnicas não viram conflito de versão por conveniência. Rejeição por versão não deixa movimento nem altera token ou valores.

Upgrade: consulta prévia somente leitura → `008_sprint2_versoes.sql` → `009_sprint2_ajustes.sql` → `010_sprint2_runtime.sql` → `013_sprint2_auditoria_carta.sql` → verificações somente de leitura. Base nova: primeiro 000–006, depois 008–010 e 013; 007/011/012 são consultas de verificação, não migrações. Executar sqlcmd **com `-I -b -f 65001`**: -I ativa QUOTED_IDENTIFIER necessário aos índices filtrados já existentes, -b interrompe erros. Guarda de propriedade e validação de versões em todos os scripts novos. Instruções/caminho de preparação atualizados em `database/README.md` e `database/Preparar.ps1`.

Este contrato é publicado cedo para integração; não declara que as atualizações já foram aplicadas ou que testes foram aprovados. Estado real e comandos estarão em `entrega-sprint2-banco.md`.
