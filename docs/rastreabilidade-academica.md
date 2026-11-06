# Rastreabilidade acadêmica — PokéVault

Registro: 21/10/2026. Esta matriz é insumo para revisão posterior do escrito; não altera o PDF nem comprova implementação. Fontes: DECISOES.md, diario-sprints.md, contrato-sprint1.md e texto extraído em tmp/pdfs/pokevault.txt. Capítulos/seções são referências estáveis; a numeração física de páginas do PDF difere do sumário.

## Matriz requisito → artefato → teste → evidência → escrito

O inventário estático em docs/evidencias/sprint1-preparacao-inventario.json comprova somente presença/hash no instante registrado. Não há evidência funcional nesta rodada. Artefato previsto não é artefato entregue; contrato publicado não é procedure executada.

| Requisito | Artefato real ou pendência na inspeção | Caso | Evidência existente / faltante | Seções do PDF e revisão necessária |
| --- | --- | --- | --- | --- |
| MVC e REST compartilhando serviços | Projeto src/PokeVault.Web/PokeVault.Web.csproj e Program.cs existem; estrutura MVC inicial lida; camadas/rotas de domínio pendentes de validação | T01/T09/T10/T17 | Inventário estático; faltam execução HTTP, fluxo MVC e persistência correlacionados | 3.1–3.4, 5.7 e 6.1: distinguir estrutura criada de solução integrada verificada |
| Catálogo público e filtros nome/coleção/conservação com páginas | DECISOES.md e contrato publicados; endpoints ainda em desenvolvimento na inspeção | T09 | Falta resposta anônima, filtros e captura real | 3.1, 3.3–3.4 e 6.1: consulta pública; compra autenticada |
| Admin fixo, perfis e conta ativa | Contrato publica Usuario/Perfil e procedures; hash/bootstrap C# ainda não verificados | T01–T05/T24/T26 | Faltam login/logout, 401/403, sessão invalidada, antifalsificação e isolamento | 3.2–3.4, 4.1–4.6, 6.1: não afirmar segurança validada por configuração planejada |
| Atendente mantém cartas/preço/estoque, sem saldo/estorno | Regra aprovada; dbo.usp_CadastrarCarta/usp_AjustarCarta no contrato, sem execução comprovada | T02/T36 | Falta negação HTTP no núcleo; telas/manutenção completa S2 | 3.1/3.3/3.4 e 5.2–5.4: preservar recorte das sprints |
| Mesma carta em conservações independentes | Contrato Carta por conservação; database/ é fonte futura do DDL; códigos NM/NEAR_MINT e SP/SLIGHTLY_PLAYED | T08/T11/T18/T25 | Faltam DDL executado, variantes e valores persistidos | 4.1–4.5 e 3.3: usar schema realmente entregue; não criar entidade Oferta adicional sem alinhamento |
| Definir saldo substitui atual e gera delta auditado | Contrato dbo.usp_DefinirSaldo real como especificação, execução não comprovada | T06/T07/T35 | Faltam retorno Saldo/Delta, movimento e autorização | 3.1, 4.1/4.6 e 6.1: esclarecer saldo fictício e substituição, sem pagamento bancário |
| Compra apenas das linhas selecionadas | Contrato dbo.ItemCompraSelecionadoType e dbo.usp_ConcluirCompra publicados | T10/T11/T15/T16/T31 | Falta comparação antes/depois incluindo carrinho remanescente | 3.1/3.3/3.4, 4.6, 5.7 e 6.1: substituir checkout do carrinho inteiro por seleção atômica |
| Preço muda somente após novo aceite | DECISOES.md e erro contratual 51007 PRECO_ALTERADO | T27 | Falta conflito real e nova confirmação na interface | 3.3/3.4, 4.6 e 6.1: não afirmar cobrança correta sem caso executado |
| Saldo/estoque insuficientes sem efeitos parciais | Erros 51008/51009 e transações definidos no contrato | T12–T16/T23 | Faltam SQL real, movimentos e prova após falha controlada | 4.6 e 6.1: separar recusa antes da escrita de rollback após escrita |
| Reenvio de UUID sem duplicar compra | Chave por cliente e conteúdo canônico definidos no contrato | T19/T28–T30/T32/T33 | Faltam Repetida/CompraId e contagens reais de compra/movimentos | 3.4, 4.5/4.6 e 6.1: descrever algoritmo somente após conferir implementação |
| Última unidade e saldo concorrentes | Ordem de bloqueios decidida, SQL ainda não executado por esta frente | T21/T22/T30 | Faltam duas conexões, sincronização, respostas e estado final | 4.6 e 6.1: não inferir ausência de inconsistência a partir de código/índices |
| Cancelamento integral único, histórico e valor original | dbo.usp_CancelarCompra no contrato; sem execução comprovada | T17–T20/T25/T34/T35 | Faltam histórico, autor/motivo e um ESTORNO de saldo/por carta, inclusive simultâneo | 3.3/3.4, 4.6 e 6.1: tirar estorno por trigger e prazo de três dias |
| Modelos, constraints e scripts | contrato-sprint1.md entregue; inventário encontrou zero .sql naquele instante; DDL não validado nesta frente | T08/T12/T16/T20–T23 | Inventário real; faltam schema implantado/versão SQL e integridade executada | 4.1–4.6: alinhar MER/lógico/físico aos scripts reais; dinheiro decimal(12,2), UTC e histórico |
| Acesso ao SQL sem DML direto no runtime | Permissões EXECUTE/SELECT previstas em 006_permissoes.sql pelo contrato, arquivo/execução ainda não verificados | T02/T23, inspeção de permissões futura | Falta consulta de permissões e tentativa controlada de DML com credencial runtime | 4.6/4.7 e 6.1: não alegar restrição aplicada só por existir regra |
| Capacitor e mesma base entre canais | Previsto para S3; app mobile não verificado | Caso futuro mobile; regressão T10/T17/T24 | Nenhuma evidência mobile; web não a substitui | 3.5, 5.7 e 6.1: integração multicanal é objetivo pendente |
| Gestão/cronologia e responsabilidades reais | diario-sprints.md é diário único real; esta preparação gera entrega-sprint1-testes.md para consolidação | Inspeção documental; não é teste funcional | Decisões/diário e artefatos desta frente; nenhuma reunião inferida | 5.1–5.6/5.8 e 6.2: 07/09 é referência informada; registros reais em 21/10; não atribuir autoria/contribuição individual sem confirmação |

## Alegações acadêmicas a revisar

| Local da referência | Alegação/problema | Tratamento proposto, ainda sem editar o PDF |
| --- | --- | --- |
| 3.2–3.5 | Instruções de preenchimento no corpo | Substituir depois por organização/rotas/telas reais e suas evidências; instruções não são relatos de implementação |
| 4.1–4.5 | Modelo conceitual/lógico/físico anterior inclui entidades/fluxos de escopo maior | Confrontar com DDL efetivamente entregue: Carta como registro comercial, Compra/ItemCompra, saldo/carrinho/movimentos e dois estados. Não reproduzir nomes de tabelas inexistentes |
| 4.6 | sp_RegistrarVenda, rotinas buyback/dashboards e triggers são descritos como implementados | Contrato atual usa usp_ConcluirCompra e usp_CancelarCompra; scripts/execuções precisam ser verificados. Não há trigger financeira no desenho aprovado; buyback/relatórios são posteriores |
| 4.6 | Cancelamento automático após três dias e estorno por trigger | Remover do escopo demonstrado. Cancelamento é administrativo, integral, transacional e idempotente |
| 4.6 | Menor latência/ausência de inconsistência afirmadas como consequência garantida | Não há medição de desempenho. Alegar somente resultados delimitados por testes reais; existência de procedure não prova latência nem concorrência correta |
| 4.8–4.9 | Azure App Service/SQL/Blob, CI/CD, Docker e monitoramento são descritos operacionalmente | Demonstração local é prioridade. Tratar esses elementos como proposta futura sem alegar implantação, pipeline ou testes automáticos já existentes |
| 5.1–5.6 e metodologia na introdução | Scrum, reuniões às segundas-feiras, Trello e contribuições podem ser interpretados como fatos | Relatar organização real por frentes e diário único. Não há comprovação de cerimônias/Trello; não inventar atas, sprints encerradas ou datas retroativas |
| 5.7 | Integração final web/mobile | Manter pendente até verificar Capacitor e intercâmbio com a mesma base |
| 6.1 | Objetivo atingido, divergências eliminadas e todos os requisitos cumpridos | Conclusão prematura: testes funcionais desta frente ainda não executados. Substituir depois por resultados e limites específicos, sem universalizar ausência de defeitos |
| 6.1 e referência de acessibilidade | HandTalk/LIBRAS e usabilidade inclusiva alegados como implementados | Não há evidência coletada dessas funcionalidades. Tratar como pendente/fora desta rodada; aparência responsiva não comprova acessibilidade |
| 6.2 | Reflexões individuais | Somente integrantes reais podem confirmar atividades, dificuldades e aprendizados próprios; apoio de IA deve ser descrito com transparência |

## Texto utilizável agora — somente fatos comprovados

Para capítulos 3/4: “O planejamento do PokéVault adotou ASP.NET Core MVC com API REST e SQL Server para uma demonstração local. O contrato SQL publicado em 21 de outubro de 2026 especificou a compra atômica das linhas selecionadas do carrinho e o cancelamento integral com estorno único. A publicação desse contrato não constitui evidência de execução das rotinas no banco.”

Para capítulo 5: “O trabalho foi organizado por frentes de coordenação, banco de dados, desenvolvimento e testes/documentação, com decisões e acompanhamento registrados em um diário único. A frente de testes preparou critérios de aceite e rastreabilidade com apoio de IA. O início de referência informado foi 7 de setembro de 2026; os registros desta preparação foram produzidos em 21 de outubro de 2026, sem reconstrução de atividades anteriores.”

Para capítulo 6, como balanço parcial: “Nesta etapa foram conciliados os critérios de verificação de autenticação, catálogo, compra selecionada e cancelamento. A inspeção de arquivos identificou a estrutura inicial do projeto MVC e o contrato SQL. Os testes funcionais e de integridade permaneceram bloqueados por dependências de aplicação e banco de testes; portanto, não é possível concluir a eficácia do fluxo ou a ausência de inconsistências.”

Esses parágrafos descrevem esta rodada. Antes de inserir no escrito final, revisar o estado mais recente e citar evidências reais. Não usar os textos como conclusão final de funcionalidades que ainda serão executadas.

## Atualização de artefatos em 21/10/2026

A segunda inspeção, em evidencias/sprint1-preparacao-inventario-02.json, identificou oito scripts SQL reais (000 a 007) e a leitura dos controllers/modelos identificou rotas reais, relacionadas em testes-sprint1.md. O DDL 001_schema.sql e o seed 005_seed.sql foram lidos: existem definições para as entidades/colunas das consultas preparadas e valores de massa alinhados ao plano. Nas linhas da matriz que descrevem DDL/rotas como pendentes, a pendência de publicação foi superada por essa atualização; permanecem implantação, execução e comprovação funcional. Não houve conexão SQL ou teste HTTP. Os textos utilizáveis continuam balanços parciais; nenhum resultado final foi validado.

