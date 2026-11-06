# APÊNDICE A – DOCUMENTAÇÃO TÉCNICA, CÓDIGOS E EVIDÊNCIAS DO PROJETO

RASCUNHO PARA REVISÃO — PIM PokéVault — 6 de novembro de 2026

Este apêndice reúne registros de gestão, excertos do código-fonte e resultados preservados do desenvolvimento da aplicação web PokéVault. Cada material é associado a uma finalidade e às seções correspondentes do PIM. Os excertos foram transcritos dos arquivos disponíveis; não constituem programas completos para execução isolada.

Os identificadores E1–E8 seguem o índice da versão reescrita do trabalho. Os números das listagens e dos quadros usam o prefixo A para facilitar a revisão deste documento separado; sua numeração e a paginação deverão ser compatibilizadas quando o apêndice for incorporado ao PIM.

As lacunas permanecem identificadas como PENDENTE. Resultados de 2026 aqui apresentados são registros históricos: os testes não foram reexecutados durante a organização deste rascunho. A cópia atual do código e o binário de uma rodada passada são identificações diferentes.

# A.1 MAPA DE EVIDÊNCIAS

O Quadro A.1 relaciona os identificadores já utilizados no trabalho com materiais efetivamente localizados. Os caminhos são relativos à raiz do projeto PIM; o vínculo com os capítulos é uma organização editorial deste apêndice.

Quadro A.1 – Evidências e vínculo com o PIM

| ID / item | Material e localização | Relação com o PIM |
| --- | --- | --- |
| E1 / A.2 | docs/DECISOES.md; docs/diario-sprints.md; contratos e relatórios de entrega. | 5.1–5.5: decisões, planejamento e evolução; registros técnicos, sem substituir atas da equipe. |
| E2 / A.6 | docs/entrega-tcgdex151-testes.md; docs/evidencias/tcgdex151/live-final/ e offline-final/. | 3.6 e 5.6–5.8: integração TCGdex e 25 cenários. |
| E3 / A.6 | docs/evidencias/tcgdex151/regressao-final/{manifesto,resultados}.json. | 5.7 / Quadro 7: regressão 45/45 da versão D226. |
| E4 / A.6 | docs/evidencias/fluxo151/acabamento-visual151/{manifesto,resultados}.json. | 3.3 e 5.7: 10/10 sobre fluxo e apresentação HTML/HTTP; não são capturas visuais. |
| E5 / A.6 | docs/evidencias/contas/contas-validacao/{manifesto,resultados}.json. | 3.4 e 5.7: 10/10 de cadastro e alteração de senha. |
| E6 / A.6 | docs/evidencias/contas/regressao-contas/{manifesto,resultados}.json. | 5.7 / Quadro 7: regressão 45/45 da versão 64F1. |
| E7 / A.6 | docs/evidencias/contas/demonstracao/{antes.txt,depois.txt,resultado.json}. | 4.7–4.9: preservação reportada de 12 tabelas e prontidão schema 12. |
| E8 / A.3–A.5 e A.7 | src/PokeVault.Web/; database/; tests/; dev.ps1 e global.json. | 3 e 4: implementação, scripts e roteiros; versões dos arquivos identificadas por SHA256. |

Fonte: elaborado a partir dos arquivos do projeto PokéVault (2026).

# A.2 REQUISITOS, BACKLOGS E SPRINTS

O Product Backlog a seguir é uma síntese documental dos requisitos, contratos e entregas disponíveis. Não é exportação do Trello nem reprodução de prioridades, estimativas ou responsáveis individuais não registrados. A coluna de aceite resume regras existentes; não indica uma aprovação nova.

Quadro A.2 – Síntese do Product Backlog e critérios documentados

| Funcionalidade | Aceite documentado | Situação sustentada | Origem |
| --- | --- | --- | --- |
| Acesso e perfis | Login válido; conta ativa; operações limitadas a CLIENTE, ATENDENTE e ADMINISTRADOR. | Implementado na web; verificação histórica em E6. | contrato-sprint1.md; LojaService.cs; E6. |
| Variantes e catálogo | Identidade por coleção, número, conservação, idioma e acabamento; preço e estoque próprios. | Implementado; evolução do esquema no script 024. | 024_fluxo_catalogo151.sql; entrega-fluxo151.md; E4. |
| Carrinho e compra selecionada | Comprar apenas linhas selecionadas, preservar remanescentes e revalidar preço, saldo e estoque. | Implementado; compra transacional e chave UUID. | contrato-sprint1.md; script 024; E6, SQL-01 e HTTP-10. |
| Repetição e estorno | Mesma chave/conteúdo não debita novamente; cancelamento integral devolve valor e quantidade originais uma única vez. | Implementado; cenários de idempotência e concorrência registrados. | scripts 003/024; E6, SQL-04 e SQL-08/09. |
| Administração | Saldo absoluto e ajustes com motivo/versão; conflito exige recarga; auditoria de alterações. | Implementado; privilégio externo de runtime permanece limitado. | contrato-sprint2.md; scripts 009/013; E6, SQL-14/17/19. |
| Referências TCGdex 151 | 207 referências; biblioteca separada da oferta; cache e moeda externa explícitos; sem preço BRL automático. | Implementado; 25/25 em E2. | contrato-tcgdex151.md; entrega-tcgdex151-testes.md. |
| Cadastro e própria senha | Cadastro fixa CLIENTE, saldo zero e carrinho vazio; alteração exige senha atual e invalida outras sessões. | Implementado na evolução de 03/11; substitui restrição histórica de cadastro só administrativo. | entrega-contas.md; 026_contas_senhas.sql; E5/E6. |
| Mobile, buyback, relatórios e nuvem | Não há aceite funcional comprovado nas fontes consultadas para esse recorte. | Planejado ou fora da implementação web documentada. | PIM, Quadro 4; diário; pendências de revisão. |

Fonte: elaborado a partir dos arquivos do projeto PokéVault (2026).

## A.2.1 Planejamento acadêmico e cronologia técnica

O PIM registra uma primeira sprint de organização do banco, perfis e coleção 151 e uma segunda sprint em 16/09/2026, voltada ao planejamento mobile. São relatos do documento acadêmico: não foram localizadas atas, cartões ou uma exportação do Trello que comprovem individualmente as atividades. O diário técnico foi aberto em 21/10/2026 e informa 07/09/2026 apenas como início de referência comunicado por Daniel.

As segundas-feiras de 21/09/2026, 28/09/2026 e 05/10/2026 e os demais pontos semanais do Quadro 6 devem continuar identificados como acompanhamento previsto quando não houver registro correspondente. Não se atribuem entregas a essas datas por inferência.

Quadro A.3 – Marcos encontrados e natureza do registro

| Data local | Registro | Leitura documental |
| --- | --- | --- |
| 07/09/2026 | Início de referência informado no diário. | Não comprova execução ou reunião naquela data. |
| 16/09/2026 | Segunda sprint descrita no PIM. | Planejamento mobile; ata/cartões pendentes. |
| 21/10/2026 | Diário, contratos e entregas técnicas S1/S2. | Datas reais dos registros técnicos; não reconstruir setembro a partir delas. |
| 03/11/2026 | Reorientação da demonstração, TCGdex, fluxo e contas. | Incrementos da frente web; não numerar como sprints coletivas sem ata. |
| 04/11/2026 | Reescrita e consolidação acadêmica. | Documentação posterior à execução; não é nova campanha de testes. |

Fonte: elaborado a partir dos arquivos do projeto PokéVault (2026).

## A.2.2 Sprint Backlog técnico 1 — fluxo de compra e estorno

A seleção abaixo preserva os IDs do quadro local do diário. O estado inicial e o fechamento são momentos diferentes: a preparação documental não foi tratada como aprovação funcional. A frente de testes preparou a matriz formal, enquanto os checks executados inicialmente foram de desenvolvimento.

Quadro A.4 – Sprint Backlog técnico 1: recorte e encerramento

| Cartão / frente | Trabalho selecionado | Registro do encerramento em 21/10/2026 |
| --- | --- | --- |
| S1-B01 / Banco | Schema, contrato SQL e massa. | Scripts 000–007 entregues; execução posterior por Desenvolvimento reportada; entrega inicial de Banco tinha limite ODBC/SSL. |
| S1-B02 / Banco | Compra, estorno e integridade. | Procedures entregues; concorrência e rollback ainda pendentes no aceite S1. |
| S1-D01 / Desenvolvimento | Estrutura, configuração e login. | Incremento local compilado; autenticação nos três papéis e catálogo público reportados. |
| S1-D02 e S1-D03 / Desenvolvimento | Carrinho, compra, histórico e estorno administrativo. | Fluxo básico com 30 checks HTTP/integrados e 7 MVC positivos reportados; não equivalem aos 30 casos formais. |
| S1-T01 / Testes e documentação | Matriz, rastreabilidade e preparação. | Preparação documental concluída; inventários e roteiro disponíveis. |
| S1-T02 / Testes e documentação | Execução integrada e defeitos. | Suíte formal não executada nessa entrega; pendências transferidas ao ciclo seguinte. |

Fonte: elaborado a partir dos arquivos do projeto PokéVault (2026).

Origem: docs/diario-sprints.md, seção Sprint 1, quadro local da abertura e balanço de encerramento; docs/entrega-sprint1-desenvolvimento.md; docs/entrega-sprint1-testes.md. Os checks de S1 permanecem vinculados ao relato histórico; este rascunho não os soma aos conjuntos E2–E6.

## A.2.3 Sprint Backlog técnico 2 — administração e integridade

Quadro A.5 – Sprint Backlog técnico 2: seleção e evidência

| Cartão / frente | Trabalho e critério registrado | Situação documental |
| --- | --- | --- |
| S2-C01 / Coordenação | Recorte, decisões e acompanhamento no diário. | Abertura e encerramento técnico registrados em 21/10/2026. |
| S2-B01 / Banco | Contrato e scripts aditivos; ajustes com versão esperada; base de testes própria. | 008–010 e complemento 013 reportados nas entregas; contrato preservado. |
| S2-B02 / Banco | Verificar privilégios e registrar limites. | Permissões internas via WITHOUT LOGIN / EXECUTE AS USER verificadas; conta Windows externa exclusiva permanece pendente. |
| S2-D01 / Desenvolvimento | Contas, saldo e gestão de atendentes. | MVC/API entregues e testados nos relatórios do ciclo. |
| S2-D02 e S2-D03 / Desenvolvimento | Catálogo operacional, conflitos e integração sem perder o núcleo. | Ajustes versionados e auditoria; fechamento e limites no diário. |
| S2-T01 e S2-T02 / Testes | Evidências duráveis, autorização, concorrência, rollback e regressão. | Rodadas históricas em docs/evidencias/sprint2/; versões posteriores reaplicaram a regressão em E3/E6. |

Fonte: elaborado a partir dos arquivos do projeto PokéVault (2026).

Origem: docs/diario-sprints.md, seção Sprint 2, compromisso de abertura e balanço; docs/contrato-sprint2.md e docs/entrega-sprint2-{banco,desenvolvimento,testes}.md. Os IDs correspondem a frentes técnicas, sem atribuição automática a integrantes humanos.

## A.2.4 Incrementos posteriores e evolução dos requisitos

Os contratos e entregas de TCGdex 151, fluxo 151 e contas mostram refinamentos posteriores: biblioteca externa separada do comércio, variantes por idioma/acabamento, adição cumulativa ao carrinho, cadastro público e alteração da própria senha. As restrições antigas de cadastro exclusivamente administrativo e de carrinho por substituição descrevem versões anteriores; não devem ser reaplicadas à versão final sem considerar docs/entrega-fluxo151.md e docs/entrega-contas.md.

# A.3 ARQUITETURA E TELAS

A aplicação está organizada como um projeto web ASP.NET Core, com controladores MVC/API, serviços de aplicação, modelos e componentes de infraestrutura. A consulta às pastas sustenta essa separação; não demonstra implantação de microsserviços.

```
src/PokeVault.Web/
  Controllers/ — LojaController, LojaApiController, Gestao, Conta e TcgDex
  Application/ — LojaService, GestaoService, ContaService e TcgDexService
  Domain/ e Models/ — entidades, contratos e modelos de entrada/saída
  Infrastructure/ — repositórios SQL, sessão, cliente TCGdex e imagens locais
  Views/ — páginas Razor e componentes compartilhados
  Program.cs — configuração da aplicação
```

Fluxo de compra localizado: LojaApiController.Comprar → LojaService.Comprar → SqlLojaRepository.Comprar → dbo.usp_ConcluirCompra. Os trechos em A.5 mostram essa cadeia; A.4 apresenta as regras SQL; A.6 liga os comportamentos aos cenários registrados.

PENDENTE — Capturas reais e selecionadas do catálogo, detalhe da carta, carrinho, login e administração. Não foram localizadas imagens finais de interface em docs/ ou output/. Os arquivos Razor comprovam a implementação textual e os testes HTML/HTTP comprovam suas verificações específicas; nenhum dos dois substitui captura de tela ou auditoria visual.

PENDENTE — Selecionar o diagrama final já utilizado no PIM, conferir nomes e inserir sua cópia com legenda. Este rascunho fornece o mapa textual das camadas; não apresenta um diagrama novo como artefato histórico.

# A.4 BANCO DE DADOS: INTEGRIDADE E TRANSAÇÕES

A sequência de scripts demonstra a evolução da base. O script 001 registra o esquema inicial, 008/009 acrescentam proteção por versão, 013 complementa auditoria, 019–021 introduzem a biblioteca TCGdex, 024 atualiza variantes e republica a compra com snapshots ampliados, e 026 acrescenta cadastro e senha. Portanto, o texto não deve tratar o script 003 como única definição final de compra.

Listagem A.1 – Chaves e valores positivos dos itens de compra

```
CREATE TABLE dbo.ItemCompra (
    CompraId int NOT NULL REFERENCES dbo.Compra(CompraId),
    CartaId int NOT NULL REFERENCES dbo.Carta(CartaId),
    Quantidade int NOT NULL,
    PrecoUnitario decimal(12,2) NOT NULL,
    NomeCartaSnapshot nvarchar(120) NOT NULL,
    ConservacaoSnapshot nvarchar(30) NOT NULL,
    CONSTRAINT PK_ItemCompra PRIMARY KEY (CompraId,CartaId),
    CONSTRAINT CK_ItemCompra_Valores CHECK (Quantidade>0 AND PrecoUnitario>0)
);
```

Fonte: database/001_schema.sql, linhas 91–100; cópia local conferida em 6 nov. 2026.

As FKs ligam o item à compra e à oferta comercial Carta; a chave composta evita repetição da mesma carta na compra e o CHECK exige quantidade e preço positivos. Uso no PIM: 4.1–4.4. Este é o DDL inicial; o script 024 acrescenta snapshots de idioma, acabamento, coleção e número.

Listagem A.2 – Identidade da variante comercial

```
IF EXISTS(SELECT 1 FROM sys.key_constraints WHERE name=N'UQ_Carta_Comercial')
 ALTER TABLE dbo.Carta DROP CONSTRAINT UQ_Carta_Comercial;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Carta') AND name=N'UQ_Carta_Variante')
 ALTER TABLE dbo.Carta ADD CONSTRAINT UQ_Carta_Variante UNIQUE(ColecaoId,Numero,Conservacao,Idioma,Acabamento);
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_Carta_Variante')
 ALTER TABLE dbo.Carta ADD CONSTRAINT CK_Carta_Variante CHECK
 (Idioma IN(N'NAO_INFORMADO',N'pt',N'en',N'ja') AND Acabamento IN(N'NAO_INFORMADO',N'NORMAL',N'HOLO',N'REVERSE_HOLO'));
```

Fonte: database/024_fluxo_catalogo151.sql, linhas 21–27; cópia local conferida em 6 nov. 2026.

O script substitui a unicidade antiga por coleção, número, conservação, idioma e acabamento e restringe os domínios. Isso permite separar preços e estoques das versões comerciais da mesma carta. Uso no PIM: 4.2–4.4; evidência comportamental complementar E4.

Listagem A.3 – Serialização da carteira e idempotência de compra

```
    SELECT @resumo = (SELECT CartaId,Quantidade,PrecoConfirmado FROM @Itens ORDER BY CartaId FOR JSON PATH);
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.Usuario u JOIN dbo.Perfil p ON p.PerfilId=u.PerfilId
            WHERE u.UsuarioId=@ClienteId AND u.Ativo=1 AND p.Codigo=N'CLIENTE')
            THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        SELECT @saldo=Saldo FROM dbo.SaldoCliente WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@ClienteId;
        IF @saldo IS NULL THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        -- Serializacao por carteira protege tambem chaves ainda inexistentes.
        SELECT @compraId=CompraId, @status=Status, @total=Total, @anterior=ResumoRequisicao
          FROM dbo.Compra WHERE ClienteId=@ClienteId AND ChaveOperacao=@ChaveOperacao;
        IF @compraId IS NOT NULL
        BEGIN
            IF @anterior COLLATE Latin1_General_100_BIN2 <> @resumo COLLATE Latin1_General_100_BIN2
                THROW 51004, 'CHAVE_REUTILIZADA', 1;
            COMMIT;
            SELECT @compraId AS CompraId,@status AS Status,@total AS Total,@saldo AS SaldoAtual,CAST(1 AS bit) AS Repetida;
            RETURN;
        END;
```

Fonte: database/024_fluxo_catalogo151.sql, linhas 83–101; cópia local conferida em 6 nov. 2026.

O resumo canônico ordena os itens; a carteira é bloqueada e uma chave já usada retorna a compra existente apenas se o conteúdo coincide. Reutilização divergente gera CHAVE_REUTILIZADA. Uso no PIM: 4.5; correspondência E6, SQL-04 e SQL-07.

Listagem A.4 – Baixa, remoção seletiva e reversão em caso de erro

```
        UPDATE c SET QuantidadeEstoque=c.QuantidadeEstoque-i.Quantidade
          FROM dbo.Carta c JOIN @Itens i ON i.CartaId=c.CartaId WHERE c.QuantidadeEstoque>=i.Quantidade;
        IF @@ROWCOUNT<>(SELECT COUNT(*) FROM @Itens) THROW 51008, 'ESTOQUE_INSUFICIENTE', 1;
        INSERT dbo.MovimentacaoEstoque(CartaId,CompraId,Tipo,QuantidadeDelta,EstoqueApos,ResponsavelId)
            SELECT c.CartaId,@compraId,N'COMPRA',-i.Quantidade,c.QuantidadeEstoque,@ClienteId
              FROM dbo.Carta c JOIN @Itens i ON i.CartaId=c.CartaId;
        DELETE c FROM dbo.ItemCarrinho c JOIN @Itens i ON i.CartaId=c.CartaId WHERE c.CarrinhoId=@carrinhoId;
        COMMIT;
        SELECT @compraId AS CompraId,CAST(N'CONCLUIDO' AS nvarchar(10)) AS Status,@total AS Total,@saldo AS SaldoAtual,CAST(0 AS bit) AS Repetida;
    END TRY
    BEGIN CATCH
        IF XACT_STATE()<>0 ROLLBACK;
        THROW;
    END CATCH;
```

Fonte: database/024_fluxo_catalogo151.sql, linhas 138–151; cópia local conferida em 6 nov. 2026.

O trecho integra a mesma procedure iniciada na Listagem A.3. Remove apenas itens selecionados e executa rollback no tratamento de erro. As conferências de preço/saldo e o débito estão no intervalo intermediário do arquivo completo; este excerto não é a transação inteira. Uso no PIM: 4.5; E6, SQL-01/02 e SQL-11.

Listagem A.5 – Cancelamento repetido sem novo estorno

```
        SELECT @saldo=Saldo FROM dbo.SaldoCliente WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@clienteId;
        SELECT @status=Status,@total=Total FROM dbo.Compra WITH (UPDLOCK,HOLDLOCK) WHERE CompraId=@CompraId AND ClienteId=@clienteId;
        IF @saldo IS NULL OR @status IS NULL THROW 51003, 'COMPRA_NAO_ENCONTRADA', 1;
        IF @status=N'CANCELADO'
        BEGIN
            COMMIT;
            SELECT @CompraId AS CompraId,@status AS Status,@total AS Total,@saldo AS SaldoAtual,CAST(1 AS bit) AS Repetida;
            RETURN;
        END;
```

Fonte: database/003_procedures_compra.sql, linhas 112–120; cópia local conferida em 6 nov. 2026.

O cancelamento lê o total original e encerra sem novo crédito quando a compra já está cancelada. A reposição e o crédito completos constam no mesmo arquivo, linhas 134–145; não utilizam o preço corrente da carta. Uso no PIM: 4.6; E6, SQL-08/09 e SQL-12.

Listagem A.6 – Recusa de saldo administrativo desatualizado

```
        SELECT @anterior=Saldo,@versao=Versao FROM dbo.SaldoCliente WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@ClienteId;
        IF @anterior IS NULL THROW 51001, 'USUARIO_INATIVO_OU_INVALIDO', 1;
        IF @versao<>@VersaoEsperada THROW 51012, 'VERSAO_DESATUALIZADA', 1;
        SET @delta=@NovoSaldo-@anterior;
        IF @delta<>0
        BEGIN
            UPDATE dbo.SaldoCliente SET Saldo=@NovoSaldo WHERE UsuarioId=@ClienteId AND Versao=@VersaoEsperada;
            IF @@ROWCOUNT<>1 THROW 51012, 'VERSAO_DESATUALIZADA', 1;
            INSERT dbo.MovimentacaoSaldo(UsuarioId,Tipo,ValorDelta,SaldoApos,ResponsavelId,Motivo)
                VALUES (@ClienteId,N'AJUSTE_ADMIN',@delta,@NovoSaldo,@AdministradorId,TRIM(@Motivo));
```

Fonte: database/009_sprint2_ajustes.sql, linhas 38–47; cópia local conferida em 6 nov. 2026.

A versão esperada é comparada antes do ajuste; o UPDATE também exige a mesma versão. O trecho é parcial e pertence a usp_DefinirSaldoVersionado. Uso no PIM: 4.5–4.7; E6, SQL-14 e HTTP-12. A evolução de auditoria de Carta está em 013_sprint2_auditoria_carta.sql.

# A.5 SERVIÇOS, API E INTEGRAÇÃO TCGDEX

Listagem A.7 – Endpoint de compra e identidade da sessão

```
    [HttpPost("compras"), Authorize(Roles = "CLIENTE")]
    public async Task<IActionResult> Comprar(CompraModel model, CancellationToken ct)
    {
        var resultado = await loja.Comprar(User.UsuarioId(), model.ChaveOperacao, model.Itens, ct);
        return Ok(resultado); // Pode ser reenvio idempotente; não presumir criação nova.
    }
```

Fonte: src/PokeVault.Web/Controllers/LojaApiController.cs, linhas 59–64; cópia local conferida em 6 nov. 2026.

A rota completa é POST /api/v1/compras. O cliente autenticado vem de User.UsuarioId(), e o corpo fornece chave e itens. O perfil é exigido no endpoint e revalidado no serviço. Uso no PIM: 3.2, 3.4–3.5; E6, HTTP-06/10.

Listagem A.8 – Validação da seleção no serviço

```
    public async Task<ResultadoCompra> Comprar(int clienteId, Guid chave, IReadOnlyList<ItemSelecionado>? itens, CancellationToken ct)
    {
        await Exigir(clienteId, "CLIENTE", ct);
        if (chave == Guid.Empty || itens is null || itens.Count is < 1 or > 100 || itens.Any(i => i is null) || itens.Select(i => i.OfertaId).Distinct().Count() != itens.Count
            || itens.Any(i => i.OfertaId <= 0 || i.Quantidade is < 1 or > 10000 || i.PrecoConfirmado <= 0 || i.PrecoConfirmado > 9999999999.99m || decimal.Round(i.PrecoConfirmado, 2) != i.PrecoConfirmado))
            throw new RegraException("COMPRA_INVALIDA", "Selecione até 100 linhas distintas com quantidade e preço válidos e uma chave UUID.", 400);
        return await repository.Comprar(clienteId, chave, itens.OrderBy(i => i.OfertaId).ToArray(), ct);
    }
```

Fonte: src/PokeVault.Web/Application/LojaService.cs, linhas 60–67; cópia local conferida em 6 nov. 2026.

O serviço exige cliente ativo, chave UUID válida, seleção limitada, IDs distintos e valores dentro das regras antes de delegar ao repositório. Uso no PIM: 3.2 e 3.5; E6 complementa a leitura estática com recusas e compra selecionada.

Listagem A.9 – Repositório e parâmetros SQL tipados

```
    public Task<ResultadoCompra> Comprar(int clienteId, Guid chave, IReadOnlyList<ItemSelecionado> itens, CancellationToken ct) =>
        Executar("dbo.usp_ConcluirCompra", CommandType.StoredProcedure, p =>
        {
            p.Add("@ClienteId", SqlDbType.Int).Value = clienteId;
            p.Add("@ChaveOperacao", SqlDbType.UniqueIdentifier).Value = chave;
            var tabela = new DataTable();
            tabela.Columns.Add("CartaId", typeof(int));
            tabela.Columns.Add("Quantidade", typeof(int));
            tabela.Columns.Add("PrecoConfirmado", typeof(decimal));
            foreach (var item in itens) tabela.Rows.Add(item.OfertaId, item.Quantidade, item.PrecoConfirmado);
            var tvp = p.Add("@Itens", SqlDbType.Structured);
            tvp.TypeName = "dbo.ItemCompraSelecionadoType"; tvp.Value = tabela;
        }, r => LerResultado(r, ct), ct);

```

Fonte: src/PokeVault.Web/Infrastructure/SqlLojaRepository.cs, linhas 134–147; cópia local conferida em 6 nov. 2026.

A camada de persistência chama a procedure e envia os itens por TVP dbo.ItemCompraSelecionadoType. OfertaId da API é mapeado a CartaId no banco. O C# não duplica o débito/estoque realizado pela procedure. Uso no PIM: 3.2 e 4.5.

Listagem A.10 – Documento externo e persistência de referência

```
                    var doc = await client.Carta(id, forcar, set.Offline, token);
                    if (doc is null) { Atualizar(s => s with { Processados = s.Processados + 1, Ausentes = s.Ausentes + 1 }); return; }
                    using var json = JsonDocument.Parse(doc.Dados.Json);
                    var root = json.RootElement;
                    var carta = new TcgReferencia(id, root.GetProperty("localId").GetString()!, root.GetProperty("name").GetString()!, TcgPrecos.Texto(root, "rarity"), doc.Dados.Idioma,
                        doc.ImagemLocal, doc.ImagemRemota, doc.IdiomaImagem, doc.Dados.Json, root.TryGetProperty("pricing", out var precos) ? precos.GetRawText() : "{}", doc.Dados.ColetadoEmUtc, doc.Dados.Url);
                    using var cardScope = scopes.CreateScope();
                    var idiomaLog = await LogIdioma(doc.Dados.Idioma);
                    await cardScope.ServiceProvider.GetRequiredService<ITcgDexStore>().SalvarCarta(idiomaLog, ator, carta, token);
```

Fonte: src/PokeVault.Web/Application/TcgDexService.cs, linhas 121–129; cópia local conferida em 6 nov. 2026.

O importador lê o documento validado pelo cliente TCGdex, conserva idioma/origem/snapshot e usa ITcgDexStore.SalvarCarta. O restante do importador contabiliza resultados e limita concorrência a três requisições. Criar referência não cria estoque nem define preço BRL. Uso no PIM: 3.6 e 4.4; E2, LIVE-02/03 e OFF-01.

Os endpoints também incluem GET /api/v1/ofertas, GET /api/v1/carrinho, GET /api/v1/compras, GET /api/v1/compras/{compraId} e POST /api/v1/compras/{compraId}/cancelamento, conforme LojaApiController.cs. A autenticação é por cookie; a proteção antifalsificação e as políticas completas devem ser conferidas junto a Program.cs e aos filtros, sem inferi-las apenas dos excertos.

# A.6 TESTES, RESULTADOS E LIMITES DAS EVIDÊNCIAS

Os resultados abaixo foram conferidos nos arquivos JSON preservados. O horário é apresentado em America/Sao_Paulo (UTC−3), com o carimbo UTC mantido nos arquivos originais. E3 e E6 são execuções de regressão em versões diferentes; as duas contagens de 45 não representam 90 cenários independentes.

Quadro A.6 – Conjuntos do Quadro 7 e seus registros

| Evidência / conjunto | Data/hora local e resultado | Versão do produto | Arquivo-base |
| --- | --- | --- | --- |
| E2 – TCGdex | 2026-11-03 19:57:16 e 2026-11-03 19:57:56<br>23 + 2 = 25/25 aprovados. | D226…<br>PokeVaultTests | docs/evidencias/tcgdex151/live-final/resultados.json; offline-final/resultados.json. |
| E3 – Regressão TCGdex | 2026-11-03 19:58:11<br>45/45 aprovados. | D226…<br>PokeVaultTests | docs/evidencias/tcgdex151/regressao-final/{manifesto,resultados}.json. |
| E4 – Fluxo/apresentação | 2026-11-03 21:19:13<br>10/10 aprovados. | FDCE…<br>PokeVaultTests | docs/evidencias/fluxo151/acabamento-visual151/{manifesto,resultados}.json. |
| E5 – Cadastro/senha | 2026-11-03 21:30:46<br>10/10 aprovados. | 64F1…<br>PokeVaultTests | docs/evidencias/contas/contas-validacao/{manifesto,resultados}.json. |
| E6 – Regressão de contas | 2026-11-03 21:31:05<br>45/45 aprovados. | 64F1…<br>PokeVaultTests | docs/evidencias/contas/regressao-contas/{manifesto,resultados}.json. |

Fonte: elaborado a partir dos arquivos do projeto PokéVault (2026).

## A.6.1 Identificação integral dos binários testados

```
D226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C
```

```
FDCE50AF5C3FE42F82A394C2980A300FA8E6886EB9B316E375E6F66706D165EF
```

```
64F1DECDB77CBC6F5C0126C18864A8EC50E2986C6A702E6B06F7FBD0BC01514A
```

D226 corresponde a E2/E3; FDCE a E4; 64F1 a E5/E6. São SHA256 do produto registrados nos manifestos, não hashes de commit Git. O manifesto de E2 live-final e o relatório de entrega associam também os dois testes offline à mesma versão; offline-final não contém manifesto próprio.

## A.6.2 Amostras verificáveis de cenário, entrada e resultado

## E6 — SQL-04/T28,T29,T19

Esperado (transcrição): UUID retorna mesma compra apos limpeza/cancelamento; divergente51004

```
Observado (transcrição JSON):
{
  "u": 229,
  "a": 298,
  "key": "5a27c0b4-b54d-4e0b-a543-23642cee0960",
  "compra": 152,
  "reenvio": {
    "CompraId": 152,
    "Status": "CANCELADO",
    "Total": 30,
    "SaldoAtual": 100,
    "Repetida": true
  }
}
```

Resultado registrado: aprovado. Origem: docs/evidencias/contas/regressao-contas/resultados.json. Os IDs numéricos pertencem à massa de testes.

## E6 — SQL-05/T21

Esperado (transcrição): Ultima unidade duas conexoes; uma aprova outra51008

```
Observado (transcrição JSON):
{
  "u": 230,
  "v": 231,
  "a": 299,
  "codes": [
    0,
    51008
  ],
  "saldos": [
    84.5,
    100
  ]
}
```

Resultado registrado: aprovado. Origem: docs/evidencias/contas/regressao-contas/resultados.json. Os IDs numéricos pertencem à massa de testes.

## E6 — SQL-11/T23-compra

Esperado (transcrição): Falha51999 apos movimentos/baixas; rollback integral sem transacao externa

```
Observado (transcrição JSON):
{
  "u": 237,
  "a": 306,
  "compra": 0,
  "falha": 51999,
  "pontoAposEscrita": 1,
  "transacaoExterna": false,
  "recuperacao": "sucesso apos retirar flag"
}
```

Resultado registrado: aprovado. Origem: docs/evidencias/contas/regressao-contas/resultados.json. Os IDs numéricos pertencem à massa de testes.

## E5 — CONTA-03

Esperado (transcrição): Cadastro sempre CLIENTE com saldo0/carrinho vazio; injeção de perfil/ID/saldo ignorada

```
Observado (transcrição JSON):
{
  "id": 222,
  "perfil": "CLIENTE",
  "saldo": 0,
  "carrinho": 0,
  "hashVerificado": true
}
```

Resultado registrado: aprovado. Origem: docs/evidencias/contas/contas-validacao/resultados.json. Os IDs numéricos pertencem à massa de testes.

## E5 — CONTA-07

Esperado (transcrição): Troca própria ignora ID de outro usuário e preserva papel/saldo; outras sessões perdem acesso

```
Observado (transcrição JSON):
{
  "troca": 302,
  "outraSessao": 401,
  "contaAlheiaPreservada": true
}
```

Resultado registrado: aprovado. Origem: docs/evidencias/contas/contas-validacao/resultados.json. Os IDs numéricos pertencem à massa de testes.

## E2 — LIVE-02/importacao-real-preservacao

Esperado (transcrição): Importacao real207 detalhes; zero mudanca nos12 hashes comerciais

```
Observado (transcrição JSON):
{
  "status": {
    "emExecucao": false,
    "importacaoId": 1,
    "processados": 207,
    "importados": 207,
    "ausentes": 0,
    "imagensLocais": 207,
    "fallbackIngles": 0,
    "modoOffline": false,
    "mensagem": "207 referências processadas. Preços externos são informativos; estoque e preçoBRL preservados.",
    "inicioUtc": "2026-11-03T22:56:47.5531983Z",
    "fimUtc": "2026-11-03T22:57:12.900731Z"
  },
  "referenciasLogicas": 207,
  "detalhadas": 207,
  "traducoes": 207,
  "tabelasComerciaisIguais": 12
}
```

Resultado registrado: aprovado. Origem: docs/evidencias/tcgdex151/live-final/resultados.json. Os IDs numéricos pertencem à massa de testes.

As entradas completas e a preparação dos casos estão nos harnesses tests/Sprint2/ e tests/Tcgdex151/ e nos roteiros docs/testes-sprint2.md e docs/testes-tcgdex151.md. As amostras acima não reproduzem senhas, cookies ou tokens; o UUID de uma operação histórica não é necessário para o argumento acadêmico.

## A.6.3 Preservação e limitações

E7: docs/evidencias/contas/demonstracao/resultado.json registra, em 03/11/2026 às 21:31:39 no horário local (04/11/2026 00:31:39 UTC), prontidão da base PokeVault com schema requerido 12, hash 64F1 e 12 tabelas preservadas. Os arquivos antes.txt e depois.txt permanecem como comparação de origem; não foi feita nova consulta ao banco nesta montagem.

O sucesso registrado vale para os cenários e versões indicados. A validação de privilégios SQL por EXECUTE AS USER não comprova conexão externa com conta runtime independente. Testes HTML/HTTP não certificam usabilidade visual, segurança integral, carga ou conformidade WCAG. Os casos de fallback sintético da TCGdex devem permanecer separados da importação real, na qual foram observadas 207 referências, 207 imagens e zero fallback inglês.

# A.7 REPOSITÓRIO, REPRODUÇÃO E PENDÊNCIAS

## A.7.1 Localização e identificação dos arquivos

Raiz local consultada: C:\Users\Intel\Documents\ChatGPT\PIM. Código em src/PokeVault.Web/, SQL em database/, testes em tests/ e relatórios em docs/. Os caminhos relativos permitem transportar o projeto sem depender deste endereço do computador.

Repositório público para consulta do avaliador: https://github.com/Danpixels/PokeVault-PIM. A publicação foi realizada em 06/11/2026; commit inicial de referência 30bf512, branch main. Essa fotografia inclui código, scripts, harnesses, registros de sprints e evidências. O histórico Git começa na publicação e não comprova commits retroativos de setembro ou outubro. Os SHA256 do Quadro A.7 identificam os bytes das cópias locais transcritas, e não IDs de commit; diferenças de final de linha podem alterar esses hashes sem alterar a lógica do código.

Quadro A.7 – SHA256 dos arquivos transcritos

| Arquivo | SHA256 da cópia local |
| --- | --- |
| database/001_schema.sql | 3E25BDAC29A092E0C73601517D5F2A368A528D2A576AB999B934018F3087507D |
| database/024_fluxo_catalogo151.sql | 01089CA3228E974F86A6CA48240878AC16B3FBE60ED8E6D12D358B369B52DD63 |
| database/003_procedures_compra.sql | 546F70BD0AC8F075B5C799DC6DDABBD20EE3C630B1E9F85D4758EF2EB192E599 |
| database/009_sprint2_ajustes.sql | 456D5A42F8C0068C8E3F90E52A65A1254E0F45EBF410D9F7695F80E06C54C803 |
| src/PokeVault.Web/Controllers/LojaApiController.cs | EAACC94BCA4528401A57526F43437C174859F8E15E200A5D1000B5B40C59A329 |
| src/PokeVault.Web/Application/LojaService.cs | 96A7822706F02ED124F9C822F28B613EECEF451F8331F7C54659A9FFB71249A1 |
| src/PokeVault.Web/Infrastructure/SqlLojaRepository.cs | FA318D7DD2E96267C341E69DE27CE06EF4F2BAC87485B8C0EBA2DF61672D29BD |
| src/PokeVault.Web/Application/TcgDexService.cs | C16D3E84FD0DC75E2004F403FDC4C68573426893FC678000B73E6B148DAFA093 |

Fonte: elaborado a partir dos arquivos do projeto PokéVault (2026).

## A.7.2 Roteiro de reprodução baseado nos arquivos existentes

Pré-requisitos registrados: SDK .NET 10.0.400 (global.json), SQL Server local e sqlcmd; a base deve estar identificada como pertencente ao projeto. Os comandos são roteiro para execução posterior; não foram executados durante esta elaboração. A preparação altera somente a base escolhida e precisa seguir as guardas e instruções de database/Preparar.ps1.

1. Ler database/README.md, docs/DECISOES.md e os contratos; conferir a instância e se a base já existe.
2. Compilar: .\dev.ps1 -Acao Restore e .\dev.ps1 -Acao Build.
3. Para uma base nova do projeto: .\database\Preparar.ps1 -Banco PokeVaultTests -Modo Inicializar. Para uma base já identificada: usar -Modo Atualizar conforme o preparador; não republicar scripts antigos sobre procedures posteriores.
4. Preparar as contas conforme o bootstrap/fixture documentado, sem incluir credenciais neste apêndice. Iniciar a demonstração por .\Abrir-PokeVault.ps1 ou .\dev.ps1 -Acao Run -Base PokeVault.
5. Para a campanha isolada, ler tests/Sprint2/README.md e tests/Executar-Sprint2.ps1; escolher nome de rodada novo e verificar porta 5188 livre, base própria e versão esperada antes de iniciar.
6. Preservar manifesto, resultados e encerramento em nova pasta; conferir o SHA256 do binário em vez de atribuir à versão atual resultados antigos.

O preparador atual lista 000–006 para inicialização e os scripts 008–010, 013, 019–021, 024 e 026 para evolução. A existência de arquivos 014–018 no diretório não significa que pertençam à versão demonstrativa adotada: o diário registra uma rodada suspensa. Não aplicar indiscriminadamente todos os .sql por ordem de nome.

## A.7.3 Fontes documentais

PIM IV Pokévault — documento principal: https://docs.google.com/document/d/1o8YF_JAcOOCXBm_UjFSTxEmodSgwhrPXrnT9C8uZ5WU/edit

PokeVault_trabalho_reescrito_2026-11-04 — índice original E1–E8 e limites técnicos: https://docs.google.com/document/d/1esdvMuUiRFs_EmRzEKmxqh6xTSQ8QKztnzV5LOC9fEE/edit

Plano_de_Correcoes_PIM_PokeVault_Atualizado.docx — arquivo fornecido pelo autor; consultado como parecer e roteiro de organização, especialmente a seção 6. O parecer não é evidência de implementação. Suas sugestões de revisão integral não foram aplicadas ao documento principal nesta tarefa.

Fontes técnicas internas: docs/diario-sprints.md; docs/DECISOES.md; contratos sprint1/sprint2/tcgdex151; entregas de sprint1/sprint2, TCGdex, fluxo151 e contas; arquivos listados em A.1 e A.6. Não foram acrescentadas referências externas ou datas de acesso não verificadas.

## A.7.4 Lacunas e revisão antes da incorporação

Quadro A.8 – Pendências explícitas

| Material / ajuste | Lacuna e ação necessária |
| --- | --- |
| Trello e sprints coletivas | PENDENTE: exportar cartões/backlogs ou capturas legíveis e atas, com datas, responsáveis e aceite reais. As sínteses locais não substituem esses registros. |
| Capturas da interface | PENDENTE: selecionar telas reais de catálogo, detalhe, carrinho, login e administração; ocultar informações pessoais e relacionar legenda ao fluxo. |
| Versão de entrega | DISPONÍVEL: repositório público Danpixels/PokeVault-PIM, branch main, commit inicial de referência 30bf512, publicado em 06/11/2026. O acesso aos arquivos foi verificado sem autenticação; manter essa versão como referência do código e dos registros. |
| Mobile e infraestrutura | PENDENTE de evidência funcional: manter como planejado/não demonstrado quando não houver código, execução e teste correspondente. Conta SQL externa exclusiva também permanece sem comprovação. |
| Integração ABNT no PIM | PENDENTE: compatibilizar quadros/listagens com a numeração final, paginação contínua, chamadas no texto e sumário; revisar quebras no PDF. Este rascunho usa Arial 12, texto justificado, entrelinhas 1,5, recuo de 1,25 cm; fontes/quadros/código menores. |
| Manual acadêmico e acesso aos artefatos | PENDENTE: conferir o manual/enunciado vigente e garantir que os arquivos locais citados acompanhem a entrega. Caminho local não é link de acesso para o avaliador. |
| Lembretes editoriais | PENDENTE antes da entrega: resolver ou delimitar as lacunas e retirar avisos de rascunho; preservar no texto os limites reais do sistema. |

Fonte: elaborado a partir dos arquivos do projeto PokéVault (2026).


Documento Google Docs: https://docs.google.com/document/d/1If7Q7HWr6PTF9KlO_313HMaFrLKSEdKjfVNZ6HjBGc8/edit

# A.8 REGISTROS DOCUMENTAIS DAS SPRINTS E DOS INCREMENTOS

Esta seção reúne registros efetivamente produzidos durante o desenvolvimento, preservados em docs/diario-sprints.md. São transcrições selecionadas do diário técnico solicitado por Daniel, com apoio de IA, e não atas de reuniões presenciais com participantes ou assinaturas presumidos. O documento integral acompanha o repositório; os relatórios de cada frente complementam esses registros.

O pedido original foi localizado no chat “PokéVault — Coordenação”, no turno de 21/10/2026 que solicitou um diário único para cada sprint, com acordos, tarefas por departamento e problemas. O início de referência de 07/09/2026 foi informado naquele pedido; as atividades documentadas abaixo mantêm suas datas reais.

Forma de leitura: ID e data identificam a entrada original; “Registro” e “Resultado/limite” transcrevem as respectivas células do diário. A seleção não altera o estado histórico das entregas, e os limites de uma versão podem ter sido superados por incrementos posteriores.

## A.8.1 Sprint técnica 1 — abertura, acompanhamento e encerramento

Objetivo registrado: demonstrar o fluxo de autenticação, catálogo público, compra selecionada com saldo fictício, histórico e cancelamento integral administrativo. Frentes: Coordenação, Banco de dados, Desenvolvimento e Testes/documentação. O ciclo foi encerrado com o fluxo básico funcional e aceite completo ainda parcial.

### R01 — 21/10/2026

Registro: Leitura e conciliação dos quatro planos pela coordenação com apoio de IA

Resultado/limite: Documentos de planejamento disponíveis; escolhas técnicas e divergências registradas; nenhum teste funcional alegado

### R02 — 21/10/2026

Registro: Daniel responde às dúvidas de funcionamento

Resultado/limite: Catálogo público, atribuições do atendente, saldo por substituição, checkout selecionado e reconfirmação de preço registrados

### R06 — 21/10/2026

Registro: Contrato SQL publicado por Banco, lido pela coordenação e encaminhado a Desenvolvimento e Testes

Resultado/limite: docs/contrato-sprint1.md contém TVP, procedures, resultados, erros e regras de seleção/idempotência; execução SQL ainda não comprovada neste acompanhamento

### R13 — 21/10/2026, acompanhamento iniciado às 14h23 (São Paulo)

Registro: Entrega inicial de Banco concluída e relatório lido

Resultado/limite: docs/entrega-sprint1-banco.md e database/000–007 disponíveis: 11 tabelas de negócio + SchemaVersao, TVP, oito procedures, seed conciliado e instruções/permissões. Banco fez inspeções estáticas, mas não aplicou os scripts nem criou base: suas duas consultas falharam em ODBC/SSL. Não confundir entrega dos scripts com aceite de integridade

### R17 — 21/10/2026

Registro: Entrega final de Desenvolvimento recebida; coordenação leu relatório e JSONs de verificação

Resultado/limite: docs/entrega-sprint1-desenvolvimento.md registra scripts 000–006 aplicados em PokeVault, motor SQLEXPRESS 17.0.1000.7; reconciliação 007 com saída 0 e zero divergências. .local/smoke-desenvolvimento-resultado.json contém 30 verificações positivas e .local/smoke-mvc-resultado.json contém sete; datas UTC 17:23:59 e 17:23:33. São checks de desenvolvimento, não os 30 casos formais da matriz. Relatório esclarece que a saída agregada 1 de R15 veio do roteiro e a reconciliação foi executada separadamente com sucesso

### R18 — 21/10/2026

Registro: Daniel informa o fim da primeira sprint; coordenação registra balanço do ciclo

Resultado/limite: Fluxo básico funcional entregue; aceite completo não comprovado, com pendências explícitas abaixo. Os três chats terminaram suas entregas iniciais; automação de acompanhamento consultada com status PAUSED. Nenhuma sprint seguinte iniciada

Fonte: docs/diario-sprints.md, entradas identificadas acima; registros originais de 2026.

## A.8.2 Sprint técnica 2 — administração, testes e pendências

Meta registrada: permitir administrar contas, saldo e catálogo pelas telas, mantendo autorização, integridade e auditoria, e incorporar as verificações críticas pendentes de S1. A permissão SQL interna foi verificada; a conexão externa por identidade dedicada permaneceu como ressalva histórica.

### S2-R01 — 21/10/2026

Registro: Daniel solicita “Então vamos pra Sprint 2! Comece”; coordenação confere decisões, fechamento da sprint 1 e código existente

Resultado/limite: Sprint autorizada; capítulos acadêmicos aguardam sistema final. Implementação e testes desta sprint ainda não executados no momento da abertura

### S2-R05 — 21/10/2026, acompanhamento às 14h54 (São Paulo)

Registro: Entrega parcial de Banco e contrato aditivo lidos

Resultado/limite: Contrato usa rowversion em Carta/SaldoCliente, Base64 de oito bytes e procedures Versionado com conflito 51012/409; scripts 008–010 aplicados em PokeVault e PokeVaultTests. Relatório registra liberação da base de testes às 14h49 e evidências em database/evidencias/sprint2/; comparação antes/depois preservou o conteúdo das 11 tabelas comerciais da demonstração

### S2-R06 — 21/10/2026

Registro: Privilégios internos verificados por Banco; limite de conexão externa registrado

Resultado/limite: Principal WITHOUT LOGIN exclusivo do banco comprovou SELECT/EXECUTE e recusas 229 de DML e de ajustes legados sem versão. Isso não comprova conexão runtime independente: servidor somente Windows e identidade dedicada externa ainda não provisionada. Nenhuma mudança global de segurança foi feita ou autorizada

### S2-R08 — 21/10/2026

Registro: Lacuna de auditoria identificada por Banco e correção encaminhada

Resultado/limite: Ajuste somente de preço/ativação exigia motivo sem persistir todo o registro. Script aditivo 013_sprint2_auditoria_carta.sql propõe tabela AlteracaoCarta com autor/motivo/valores anteriores e novos, sem movimento estoque zero e sem mudar assinaturas. Coordenação avisou as três frentes e pediu janela livre de cenários mutantes antes de aplicar em PokeVaultTests, caso ainda não aplicado; Testes deve conferir versão e adicionar caso/regressão de auditoria. Aplicação do 013 ainda não confirmada neste acompanhamento

### S2-R10 — 21/10/2026

Registro: Resultados finais e versão do produto confrontados pela coordenação

Resultado/limite: Produto com build 0 avisos/erros; Desenvolvimento registra 54 checks integrados e 22 MVC positivos. Testes consolida 45 cenários distintos com último estado aprovado (21 SQL, 18 HTTP, 6 MVC). Rodada04 teve 44 aprovações e uma expectativa de roteiro inadequada; reteste HTTP18 passou exigindo 400 e nenhuma conta criada. Não há rodada única 45/45 nem soma desses cenários com checks de Desenvolvimento

### S2-R12 — 21/10/2026

Registro: Balanço final e encerramento do acompanhamento da rodada

Resultado/limite: Administração funcional, regressão e cenários críticos comprovados nos limites dos relatórios. Conta Windows externa runtime de menor privilégio continua pendente; permissões de banco comprovadas por WITHOUT LOGIN não substituem essa implantação. Acompanhamento deve ser pausado após consolidar este balanço; sprint3 não iniciada

### S2-R13 — 21/10/2026

Registro: Automação de acompanhamento pausada após consolidação

Resultado/limite: Ferramenta confirmou status PAUSED da automação existente. Três entregas concluídas, evidências conferidas e ressalvas registradas; não haverá início automático de outra sprint

Fonte: docs/diario-sprints.md, entradas identificadas acima; registros originais de 2026.

## A.8.3 Incremento TCGdex 151 — execução e retorno manual

O diário registra este trabalho em 03/11/2026 como integração específica da coleção sv03.5, distinta das sprints coletivas de setembro. Entradas com horário UTC foram mantidas como no documento de origem; 22h UTC corresponde a 19h em São Paulo.

### TCG-R01 — 03/11/2026

Registro: Link/documentacao/exemplos recebidos e autorizacao para tentar apenas151; coordenacao consultou fonteoficial e endpoints reais e registrou recorte. Integracao ainda nao implementada nesta abertura

### TCG-R06 — 03/11/2026, acompanhamento22:47UTC

Registro: Importacao real da demonstracao concluiu22:46:20UTC: importacao-resultado.json registra207processados/importados,207imagenslocais,0ausentes e0fallbackingles. ConsultaSQL registra207referenciasdetalhadaspt/importacaoCONCLUIDAsem pendentes e3vinculos. imagens-cache.json registra207arquivos(4017294bytes). preservacao-importacao.json compara12tabelascomerciais comtodascolunas/rowversions/hash/contagens e registraiguais=true. Fonte das evidencias: src/evidencias/tcgdex151/. Confirmadoimportreal,nãoaceitefinaldeoffline/repetição/regressão; Desenvolvimentoverificaessescriterios eTestes segueativoemPokeVaultTests

### TCG-R07 — 03/11/2026, acompanhamento22:58UTC

Registro: Versao finalD226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C liberada schema10/snapshot. Devcorrigiu validacaorespostaAPI antesde substituircache (165/207/IDs/precos/URLs) e rotulohistorico compara datas exatamente. Testes preservouidentity01 com1reprovacao de rotulohistorico naA68 e reteste, depoisexecutou finalD226. Coordenacao leu live-final23aprovados, offline-final2aprovados e regressao-final45aprovados; sao conjuntos separados,naosomar repeticoes. Relatoriosfinais/reconciliacao em consolidacao,semnovo bloqueio paraDaniel

### TCG-R09 — 03/11/2026, acompanhamento23:08UTC

Registro: Coordenacao leu os tres relatorios finais e snapshots individuais confirmaram chats concluidos. Na DLLD226BAE28EB913DE3B0D4490AABAF3018C2C64A7DD855B569E5F3CD30C3BE94C: live-final23aprovados, offline-final2aprovados, regressao-final45aprovados; hashes e versao do produto conferidos. Nenhum resultadoA68 atribuido como aceite daD226

### TCG-R12 — 03/11/2026

Registro: Balanco consolidado e automacao de acompanhamento removida apos concluir a entrega; ferramenta confirmoudeleteStatus=deleted. Nao ha nova rodada/mobile iniciada nem verificacoesperiodicas pendentes desta integracao

### TCG-R13 — 03/11/2026

Registro: Daniel relata manualmente: “Consegui comprar a carta!”, apos orientacao para cadastrar oferta da bibliotecaTCGdex e comprar como cliente. Compra bem-sucedida relatada pelo usuario; carta/CompraId/quantidade/valores e capturas nao informados, sem atribuir numeros do exemplo como observados ou alegar conferencia independente de saldo/estoque/estorno

### TCG-R14 — 03/11/2026

Registro: Apos orientacao de cancelamento administrativo e conferencia de saldo/estoque/historico, Daniel responde “Perfeito! Funcionou! E agora?”. Registrar sucesso do cancelamento conforme relato manual, sem inferir IDs/valores ou nova verificacao independente. Proxima conversa proposta: definir fluxo de compras/UX e revisar backlog das melhorias adiadas; nenhum novo desenvolvimento/mobile autorizado apenas por esta pergunta

Fonte: docs/diario-sprints.md, entradas identificadas acima; registros originais de 2026.

## A.8.4 Toques finais da web e atualização acadêmica

Após a integração, o diário conserva as entradas WEB151-01 a WEB151-06 e ACA-R01. A seguir apresenta-se uma síntese editorial dessas entradas, identificada como síntese, sem atribuir a elas novas sprints ou reuniões.

WEB151-01 e WEB151-02 — 03/11/2026: gestão unificada da coleção 151, variantes por idioma, conservação e acabamento, preço em reais definido localmente, carrinho cumulativo e snapshots de compra; script 024/schema 11, com preservação dos registros anteriores.

WEB151-03 e WEB151-04 — 03/11/2026: conflito inicial de DLL em uso e expectativa de teste ainda voltada à tela antiga; correção do roteiro, regressão final 45/45 e 10/10 do fluxo no produto 1F34…; demonstração local disponibilizada com limites de verificação HTML/HTTP. Essa é uma versão anterior a FDCE e 64F1.

WEB151-05 — 03/11/2026, aproximadamente 21h19: retorno positivo de Daniel e solicitação de olho de senha, controles recolhidos, média simplificada e 207 cartas em página única. O diário registra implementação e 10/10 cenários de fluxo na pasta acabamento-visual151, correspondente a E4. A inspeção visual manual continuou separada dos checks HTML/HTTP.

WEB151-06 — 03/11/2026, aproximadamente 21h32: cadastro público e alteração da própria senha nos três perfis; migração 026/schema 12, 12 tabelas comerciais preservadas, 10/10 contas e 45/45 regressão no binário 64F1. As credenciais da demonstração não foram alteradas automaticamente.

ACA-R01 — 04/11/2026: início autorizado da reescrita acadêmica, com primeira cópia editável separada, diagramas e referências vinculadas ao sistema real. O registro mantém como pendentes as contribuições individuais não fornecidas, capturas finais, conferência do manual e mobile. Trata-se de revisão documental, sem novo teste funcional.

Fonte: docs/diario-sprints.md, seções “Toques finais da web — fluxo e Gestão de Catálogo151” e “Reescrita acadêmica iniciada em04/11/2026”; docs/entrega-fluxo151.md e docs/entrega-contas.md.

## A.8.5 Arquivos completos e relação com o corpo do PIM

Diário integral: docs/diario-sprints.md. Planejamento e decisões: docs/planejamento.md e docs/DECISOES.md. Entregas por frente: docs/entrega-sprint1-{banco,desenvolvimento,testes}.md e docs/entrega-sprint2-{banco,desenvolvimento,testes}.md. Os contratos e resultados referidos nesses documentos permanecem nos caminhos apresentados em A.1 e A.6.

Uso no PIM: seções 5.1–5.5 para acordos, distribuição e acompanhamento; 5.6–5.8 para problemas, correções e critérios de aceite. O planejamento mobile das sprints propostas 3 e 4 não constitui evidência de execução. Diário integral no commit de referência: https://github.com/Danpixels/PokeVault-PIM/blob/30bf512/docs/diario-sprints.md. As atas da equipe e exportações do Trello, quando existentes fora deste acervo, continuam sendo materiais complementares a conferir.



