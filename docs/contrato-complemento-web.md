# Contrato SQL — complemento da versão web

Publicado em **03/11/2026**, antes da implementação desta rodada. Complementa os contratos das sprints 1/2 e segue DECISOES.md. Não inicia mobile nem revisão acadêmica final. Estado da janela/schema: `entrega-complemento-web-banco.md`.

## Coleções: cadastro e leitura

Adicionar `dbo.Colecao.QuantidadeTotalCartas int NULL`, CHECK nulo ou positivo. Coleções existentes permanecem com **NULL/não informado**, sem inferir total pelo nome ou pelas cartas comerciais. O total não é estoque e não limita Numero de Carta; cartas secretas podem exceder a numeração oficial.

Nova procedure:

```text
dbo.usp_CadastrarColecao
  @ResponsavelId int,
  @Nome nvarchar(120),
  @QuantidadeTotalCartas int
```

Administrador ou atendente ativo. Nome após TRIM não vazio, máximo 120 caracteres; total inteiro de 1 a 2147483647. A API valida os limites antes dos parâmetros. O ator vem da sessão; formulário/API recebe apenas nome/total, sem código técnico fornecido pelo cliente.

Código gerado no SQL: prefixo `COL-` + 26 dígitos hexadecimais de UUID, até 30 caracteres; UNIQUE já existente protege colisão. Código não é chave digitada nem derivado do nome. Nomes repetidos não são bloqueados por nova regra de negócio; edições podem compartilhar nome, mas cada registro possui ID/código próprios. Cadastro não possui chave de idempotência de operação; prevenir submissões acidentais repetidas na interface não significa unicidade por nome.

Resultado único/uma linha: `ColecaoId int`, `Codigo nvarchar(30)`, `Nome nvarchar(120)`, `QuantidadeTotalCartas int`. A gestão deve atualizar sua lista e permitir selecionar o ID recém-criado. SELECT de coleções inclui a nova coluna, sem trocar as três existentes: ColecaoId, Codigo, Nome, QuantidadeTotalCartas (nullable na leitura).

Sem exclusão/edição de coleção nesta rodada. FK de Carta para Colecao continua impedindo exclusão referenciada. Contrato de cadastro de carta mantém assinatura e não compara número de carta com total da coleção.

## Raridades canônicas em novos cadastros

Lista fechada, exatamente estas grafias inglesas:

1. Common
2. Uncommon
3. Rare
4. Double Rare
5. Ace Spec Rare
6. Illustration Rare
7. Ultra Rare
8. Special Illustration Rare
9. Hyper Rare
10. Shiny Rare
11. Shiny Ultra Rare
12. Promo

`dbo.usp_CadastrarCarta` mantém nome, parâmetros e retorno da sprint 1, mas passa a validar Raridade após TRIM com comparação binária de grafia/capitalização. Guarda o valor canônico; demais valores geram **51014 / RARIDADE_INVALIDA / HTTP 400**. API valida a mesma lista antes de SQL. Não enviar símbolo, código japonês, Secret Rare ou categorias adicionais como raridade. Símbolos são UI e não mudam o valor persistido.

Preservar texto legado desconhecido para leitura/checkout/ajustes. **Não adicionar CHECK global na coluna que impeça atualizar estoque de uma carta com raridade legada.** A escrita de novos produtos é controlada pela procedure e pelo papel runtime sem DML direto. A única normalização de migração nesta rodada será **Comum → Common**, apenas texto exato, registrada com CartaId/valor anterior/novo no resultado SQL. Não inferir Raro, C/U/R ou qualquer outra tradução. Raridades desconhecidas devem aparecer identificadas como legadas pela UI, sem mapeamento silencioso.

Essa normalização pode alterar o rowversion de Carta e exigir recarga de formulário aberto; não muda preço, estoque, identidade comercial ou snapshots históricos. Reexecução da instalação não reaplica transformação já registrada em SchemaVersao.

## Adicionar ao carrinho: soma atômica

Nova operação SQL, distinta de substituir/remover:

```text
dbo.usp_AdicionarItemCarrinho
  @ClienteId int,
  @CartaId int,
  @QuantidadeAdicionar int
```

Cliente ativo, carta existente/ativa, quantidade adicionada entre **1 e 10000**. O total da linha após soma também deve ser no máximo **10000**, alinhado ao limite atual da aplicação. Usa bigint no cálculo intermediário e recusa excesso antes de UPDATE/INSERT, sem overflow ou truncamento. Quantidade de entrada inválida => 51002/DADOS_INVALIDOS/400; soma acima do limite => **51013/QUANTIDADE_CARRINHO_EXCEDIDA/409**, sem mudança parcial.

Resultado único/uma linha: `CarrinhoId int`, `CartaId int`, `Quantidade int` (quantidade total após a soma). Não confundir com QuantidadeAdicionar. Linha ausente é criada; existente é incrementada no banco. Não ler quantidade em C# para depois enviar substituição. A operação não é idempotente: cada chamada bem-sucedida soma de novo; não repetir automaticamente por timeout de resultado desconhecido. Exibir/recarregar carrinho antes de uma nova ação consciente nessa situação.

Bloquear cabeçalho do carrinho com UPDLOCK/HOLDLOCK antes de ler/alterar seus itens; a operação absoluta existente e o checkout usam o mesmo cabeçalho. Não precisa bloquear carteira: não lê/escreve saldo. Depois do cabeçalho, validar carta sob bloqueio compartilhado mantido até commit. Isso não inverte a ordem do checkout (carteira → carrinho → cartas). Compras e inclusões concorrentes são serializadas pelo carrinho, não por leitura na aplicação.

**Política de estoque ao adicionar:** carrinho não reserva e não promete disponibilidade. A soma não é limitada ao estoque corrente, inclusive se zerado; exige carta ativa e teto de quantidade. A tela exibe estoque e essa política; o checkout continua revalidando quantidade, preço, estoque e saldo e pode recusar. Não debitar carteira/estoque nem iniciar compra automaticamente ao adicionar.

`usp_DefinirItemCarrinho(@ClienteId,@CartaId,@Quantidade)` mantém assinatura e significado: quantidade absoluta, zero remove. A API PUT/edição/remover permanece distinta e compatível. O teto 10000 já validado no serviço passa a ser reforçado também no SQL dessa rotina, preservando zero e as demais recusas. Não converter PUT em soma e não alterar contrato/TVP de checkout.

## Erros, privilégios e compatibilidade

Preservados códigos anteriores: 51000 autorização, 51001 cliente inválido/inativo, 51002 dados inválidos, 51006 carta indisponível, 51010 unicidade etc. Novos: 51013 quantidade acumulada acima de 10000 (409); 51014 raridade inválida (400). Conflito de UUID/cancelamento/versionamento segue contratos anteriores.

Novas procedures têm o mesmo dono dbo e ficam sob EXECUTE do papel PokeVaultRuntime já existente; SELECT da nova coluna segue permissão de leitura. Não conceder DML direto nem remover DENY dos ajustes legados. Nenhum login/usuário global, protocolo, certificado ou autenticação de servidor será alterado. O limite de conexão runtime externa com identidade Windows própria permanece o já documentado.

## Instalação e janela para Testes

Scripts aditivos a partir de **014**: 014 metadado/normalização (versão 5), 015 publicação de coleção/raridade (versão 6), 016 carrinho aditivo/validação absoluta (versão 7). Guardas de base PokeVault/PokeVaultTests, marcador PokeVaultProjeto=PIM-2026-Sprint1 e pré-requisito versão 4; sem reset, sem reescrever 000–013 executados. `sqlcmd -E -C -I -b -f 65001`, transações e parada em erro obrigatórias.

Banco deve publicar a janela e o estado de aplicação em `entrega-complemento-web-banco.md` **antes** de Testes iniciar cenários mutantes. Testes continua dono de sua massa PokeVaultTests; Banco não provisiona usuários/fixtures nem executa cenários mutantes concorrentes. Não aplicar DDL se houver execução de testes em andamento ou ausência de liberação documentada. Após LIBERADO, não executar mais DDL nessa base sem nova janela coordenada. PokeVault e suas credenciais/histórico são preservados; eventuais alterações de raridade previstas ficam discriminadas das demais comparações de preservação.

Preparação/atualização futura pelo caminho `database/Preparar.ps1` e instruções em database/README.md. Esta publicação antecipa o contrato para desenvolvimento; execução real, versão/instância, normalizações realizadas e pendências serão informadas na entrega.
