# Contrato SQL — biblioteca TCGdex 151

Publicado primeiro em **03/11/2026**. Esta autorização específica não retoma 014–018, complemento de UX, mobile ou trabalho final. Estado de aplicação/janela: `entrega-tcgdex151-banco.md`.

## Modelo e limites

- Novas tabelas: `ColecaoReferenciaTCGdex`, `ReferenciaTCGdex`, `ImportacaoTCGdex151`, `ItemImportacaoTCGdex151` e `VinculoCartaTCGdex`.
- Apenas setId **sv03.5**, idiomas **pt/en**, manifesto com **165 oficiais / 207 totais** e IDs sv03.5-001 a sv03.5-207. Outro conjunto/contagem é conflito de escopo; não ampliar silenciosamente.
- Referência é biblioteca, sem preço BRL, saldo, estoque ou conservação. Manifesto cria referências breves; detalhes são enriquecidos por lotes. Não criar 207 ofertas comerciais.
- `ReferenciaTCGdexId int` é PK; UNIQUE `(Idioma,CardId)`; guardar SetId, CardId, Idioma, Numero, Nome, Raridade nullable, ImagemUrl nullable, SnapshotJson, SnapshotColetadoEmUtc, OrigemUrl, Detalhada, PrecosJson nullable, PrecosColetadosEmUtc nullable, OrigemPrecosUrl nullable, UltimaImportacaoId.
- Coleção de referência é separada de dbo.Colecao: Nome, contagens oficiais/totais, idioma, snapshot/origem/data próprios. dbo.Colecao e dbo.Carta não recebem coluna nova ou total inventado.
- Até 414 traduções podem coexistir no armazenamento, mas `vw_BibliotecaTCGdex151` retorna **uma por CardId (207 cartas lógicas)**, preferindo detalhada, depois pt, depois data mais recente. `vw` informa Idioma: fallback inglês deve ficar explícito. Consulta de uma tradução específica usa a tabela e idioma. Nunca somar traduções como cartas diferentes.
- Dados de mercado ficam em JSON bruto, aceitando fornecedores, variantes e moedas dinâmicas. Ex.: cardmarket/EUR, tcgplayer/USD e reverse-holofoil. Ausente é NULL/indisponível, não zero; não converter a BRL ou conservação NM/SP.
- Snapshot sem pricing válido não apaga o último PrecosJson válido; sua data/origem anterior permanecem e a UI o identifica como referência histórica. Raridade/imagem ausentes também não apagam informação válida anterior. Detalhe mais antigo não substitui cache mais novo.
- Não armazenar imagens binárias no SQL. Desenvolvimento baixa/cacheia somente assets TCGdex confiáveis no workspace da aplicação e usa esse cache offline; Snapshot/ImagemUrl preservam a origem. Banco não baixa mídia.

## Origem e DTO

HTTP ocorre em C# **antes** de abrir transação SQL; poucas requisições concorrentes, timeout e retry limitado. Derivar URLs no servidor, sem URL arbitrária do usuário: `https://api.tcgdex.net/v2/{pt|en}/sets/sv03.5` e `/cards/sv03.5-XXX`. Desabilitar/validar redirects para outros hosts. URLs de imagem aceitas no SQL somente `https://assets.tcgdex.net/...`, sem credenciais ou host alternativo. Snapshot é dado, nunca instrução a executar.

A aplicação envia JSON bruto TCGdex, sem traduzir variantes/preços. SQL valida id/set/localId/nome, tamanho e JSON. Número deve ser 001–207 e corresponder ao sufixo do CardId; não usar posição no array. Strings permanecem Unicode. Datas `datetime2(3)` UTC, coletadas por servidor C#; recusar data futura além de 5 minutos ou anterior a 2000. Nenhum dado financeiro depende da API no checkout.

## 1. Iniciar ou retomar manifesto

```text
dbo.usp_IniciarImportacaoTCGdex151
 @ResponsavelId int, @ChaveImportacao uniqueidentifier,
 @Idioma nvarchar(2), @SetJson nvarchar(max), @ColetadoEmUtc datetime2(3)
```

Administrador/atendente ativo, identidade da sessão. SetJson é **resposta bruta completa do endpoint set**, com id/name/cardCount e cards[207] (id, localId, name, image opcional). Limite 2 MiB de texto SQL; sem truncar parâmetros NVarChar(max).

Transação curta: validar todos os itens, serializar importações do mesmo idioma, cadastrar/atualizar metadados do set somente se não mais antigos, inserir referências breves ainda ausentes e registrar os 207 itens da importação. Não rebaixar referência detalhada a breve. Não apagar referência antiga que não venha numa tentativa inválida.

Idempotência da operação: UNIQUE ChaveImportacao; mesma chave/idioma/**mesmo texto SetJson** retorna a importação existente. Hash SHA256 do texto UTF-16 para detectar conteúdo divergente. A aplicação preserva o payload e data da primeira tentativa para reenviar; nova coleta usa nova UUID. Conteúdo divergente na mesma chave => 51023. Progresso não é apagado ao retomar.

Resultado único: `ImportacaoId int`, `Status nvarchar(20)`, `TotalReferencias int` (=207), `Pendentes int`, `Repetida bit`.

## 2. Atualizar detalhes em lotes retomáveis

```text
dbo.usp_AtualizarReferenciasTCGdex151
 @ResponsavelId int, @ImportacaoId int, @LoteJson nvarchar(max)
```

Envelope **versao 1**, de 1 a 30 cartas por chamada, até 8 MiB:

```json
{"versao":1,"cartas":[{"coletadoEmUtc":"2026-11-03T23:00:00Z","snapshot":{"id":"sv03.5-025","localId":"025","name":"Pikachu","set":{"id":"sv03.5"},"rarity":"Common","image":"https://assets.tcgdex.net/en/sv/sv03.5/025","pricing":{"tcgplayer":{"unit":"USD","reverse-holofoil":{"marketPrice":0.5}}}}}]}
```

Exemplo estrutural, **não preço atual nem massa a importar**. Snapshot efetivo deve ser a resposta real, com todos seus campos preservados. Idioma é o da importação, não inferido do nome da carta. Fallback en requer manifesto/importação en, sem rotular conteúdo inglês como pt. SQL recusa IDs duplicados no lote, cartas fora do manifesto e conflito entre id/localId/set. Imagem/raridade/pricing podem estar ausentes; pricing presente deve ser objeto JSON, sem lista fechada de variantes/moedas. Provedor/preço/data de origem continuam dentro do JSON e separados da data de coleta.

Validar lote inteiro antes de escrever. Lote inválido dá rollback integral; cache anterior permanece. Por cartão: se coleta for mais antiga que cache detalhado, não substituir e marcar item MAIS_RECENTE; caso contrário atualizar referência, conservar campos válidos ausentes e marcar APLICADO. Reenvio não duplica IDs/itens/ofertas. Estado final CONCLUIDA quando não há pendentes, senão PARCIAL. A importação inicia EM_ANDAMENTO.

Resultado único: `ImportacaoId int`, `Status nvarchar(20)`, `Recebidas int`, `Aplicadas int`, `IgnoradasMaisRecentes int`, `Pendentes int`.

Retomada: consultar ItemImportacaoTCGdex151 JOIN ReferenciaTCGdex, WHERE ImportacaoId=@id AND Estado=PENDENTE; buscar apenas esses detalhes. Atualização individual não exige esperar os demais 207 HTTPs. Uma falha pode deixar itens válidos disponíveis e outros pendentes, sem apagar biblioteca.

## 3. Registrar falha operacional sem apagar cache

```text
dbo.usp_RegistrarFalhaTCGdex151
 @ResponsavelId int, @ImportacaoId int,
 @Codigo nvarchar(80), @Mensagem nvarchar(300)
```

Após erro externo/SQL, em chamada separada fora da transação anterior, registrar código/mensagem sanitizados (sem stack, tokens, senha ou URL arbitrária). Incrementar contador de falhas; manter referências/itens concluídos. Status FALHA se zero itens tratados, PARCIAL se alguns, CONCLUIDA se todos já tratados. Falha de HTTP anterior ao manifesto é registrada no log da aplicação; não criar manifesto fictício. Resultado: ImportacaoId, Status, Pendentes. Não chamar em loop infinito; banco não executa HTTP.

## 4. Vínculo comercial opcional seguro

```text
dbo.usp_VincularOfertaTCGdex151
 @ResponsavelId int, @CartaId int, @ReferenciaTCGdexId int
```

Somente admin/atendente ativo; validar oferta na coleção existente **MEW-151**, número inteiro igual ao Numero da referência, e nome após TRIM/case-insensitive **accent-sensitive** igual. Não traduzir nomes por palpite. Divergência => 51022, sem vínculo. Referência detalhada obrigatória. Diferentes conservações podem apontar para a mesma referência.

Vínculo fica em tabela separada **VinculoCartaTCGdex**, PK CartaId; não UPDATE dbo.Carta, não muda rowversion/preço/estoque. Guardar autor/data, nome/número conferidos. Repetir vínculo idêntico retorna o existente; substituir outro vínculo é conflito, sem sobrescrita automática. Se nome remoto mudar posteriormente, mostrar inconsistência e pedir revisão; não alterar oferta/snapshot de compra.

Resultado: `CartaId int`, `ReferenciaTCGdexId int`, `Repetida bit`. Nenhum vínculo é aplicado automaticamente pela migração. Desenvolvimento pode propor/vincular explicitamente ofertas de número/nome conferidos pelas mesmas regras, sem varrer coleções genéricas.

## 5. Cadastrar oferta local a partir da biblioteca

```text
dbo.usp_CadastrarOfertaTCGdex151
 @ResponsavelId int, @ReferenciaTCGdexId int,
 @Conservacao nvarchar(30), @Preco decimal(12,2), @QuantidadeInicial int
```

Exigir referência detalhada e coleção MEW-151 já existente, preço BRL local positivo, conservação existente e estoque inteiro não negativo. Metadados de nome/número/raridade/imagem vêm da referência selecionada; Raridade nula vira rótulo explícito “Não informada (TCGdex)”, sem classificar por palpite. Nova transação insere somente a oferta pedida, movimento inicial se >0 e vínculo auditado. Não reutilizar IDs, não consultar API, não inventar preço/quantidade e não chamar rotina antiga dentro de transação externa. Duplicata de coleção/número/conservação => 51010. Resultado: `CartaId int`, `ReferenciaTCGdexId int`.

Procedures antigas de cadastro/ajustes/compra/carrinho permanecem intocadas; nenhum dos comportamentos adiados é implementado. Consultas públicas da biblioteca podem usar a view (sem importação pública). O catálogo vendável continua dbo.Carta; biblioteca não representa disponibilidade para compra.

## Integridade, implantação e resultados

Novos erros: 51020 TCGDEX_DADOS_INVALIDOS (400), 51021 TCGDEX_IMPORTACAO_NAO_ENCONTRADA (404), 51022 TCGDEX_IDENTIDADE_DIVERGENTE (409), 51023 TCGDEX_CHAVE_REUTILIZADA (409). Autorização 51000 e dados comerciais 51002/51010 continuam. Falhas técnicas são distintas. Todas as procedures recusam transação externa (51103).

Schema novo começa em **019** e versões **8,9,10** (lacuna 5–7 reservada ao WIP suspenso): 019 tabelas/view, 020 importações, 021 vínculo/cadastro comercial. 022 consulta de preparação. Não aplicar 014–018 por dependência. Guardas de bases/marcador e pré-requisito versão 4. Runtime mantém EXECUTE/SELECT e DENY DML; não criar conta/infraestrutura externa.

Antes de DDL, publicar janela no relatório desta frente e ler a confirmação de que Testes aguarda schema. Após aplicação e verificações, registrar LIBERADO e parar DDL em PokeVaultTests. Banco não concorre com fixtures/cenários mutantes e não provisiona a massa de Testes. Comparação de preservação cobre as 12 tabelas comerciais existentes e credenciais apenas por impressão agregada, sem revelar valores privados; novas tabelas são verificadas separadamente. A importação real em HTTP é responsabilidade de Desenvolvimento, com testes independentes da frente de Testes.

Fontes primárias consultadas: [set TCGdex](https://tcgdex.dev/rest/set), [objeto de carta e pricing](https://tcgdex.dev/reference/card). GETs reais desta frente em 03/11/2026 confirmaram pt/sv03.5 com 207 cartas/165 oficiais e a amostra en/sv03.5-025 com pricing cardmarket/tcgplayer; snapshots JSON em database/evidencias/tcgdex151. Nenhum preço de exemplo foi assumido como venda BRL.
