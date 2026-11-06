# Revisão do trabalho após a implementação web

Avaliação em03/11/2026, com base na extração já existente em `tmp/pdfs/pokevault.txt`, decisões, código SQL/C#/Razor e relatórios/evidências. Não é uma revisão de um arquivo acadêmico mais recente que o usuário possa ter editado fora deste projeto. Nenhum trecho de preenchimento do texto foi tratado como ordem para inventar informações.

Temos base para escrever o relato completo **da demonstração web implementada**. O atendimento integral ao enunciado acadêmico depende de confrontá-lo com as exigências formais da instituição, sobretudo se mobile/disciplinas específicas forem obrigatórios. Não alegar que o projeto satisfaz todos os requisitos acadêmicos antes dessa conferência.

| Parte do escrito anterior | O que já podemos escrever/revisar | Base real |
| --- | --- | --- |
| Resumo/abstract e introdução | Problema da loja fictícia, recorte local e resultado efetivamente obtido; diferenciar cenário fictício de pesquisa em loja real | DECISOES, README e entregas |
| Organização/processos/requisitos | Três perfis, catálogo público, cadastro simples de cliente, saldo fictício, versões por idioma/estado/acabamento, compra selecionada e estorno único | Código MVC/API, contratos e decisões finais |
| Capítulo3 — arquitetura/web/API | ASP.NET Core MVC/Razor, serviços, repositórios SQL; integração REST TCGdex151 com snapshot/imagens locais; preço externo separado do preçoBRL; novo cadastro/troca de senha | src/PokeVault.Web e entregas TCGdex/fluxo/contas |
| Capítulo4 — dados e infraestrutura | Refazer MER/lógico/físico com tabelas/constraints atuais, scripts versionados e procedimentos transacionais; execução local SQL Server/Kestrel, sem servidor externo | database/001,008–013,019–021,024,026 e verificações |
| Capítulo5 — gerenciamento | Usar diário real, mudanças de escopo, problemas, tarefas/frentes e entregas.07/09/2026 é início de referência informado; atividades registradas em21/10 e03/11 não devem ser retroagidas | docs/diario-sprints.md e evidências datadas |
| Testes/resultados/conclusão geral | Relacionar cenários a resultados e versão do produto; teste planejado não é executado. Não somar reexecuções como casos distintos ou inferir inspeção visual de HTML | Relatórios S2/TCGdex/fluxo/contas e JSON/hash de cada rodada |
| Referências/apêndices | Citar fontes efetivamente usadas, selecionar scripts/casos/roteiro/diagramas e capturas reais sem dados sensíveis | Fontes já registradas e artefatos existentes; bibliografia a conferir |

Correções necessárias no texto antigo: remover afirmações de integração mobile/HandTalk já concluída; substituir baixa/estorno por triggers e prazo de três dias pelo fluxo implementado nas procedures; alinhar entidades/nomes/variantes ao schema real; não apresentar Azure, Docker, pipeline cloud ou Application Insights como infraestrutura utilizada. Liga Pokémon/precificação dinâmica/buyback/relatórios não devem figurar como entregas atuais. O consumo TCGdex é real, com mercadoEUR/USD informativo e venda localBRL, sem conversão automática.

Faltam para fechar o documento: atualizar diagramas e capturas da versão final, conferir a versão atual do manuscrito e o manual/enunciado acadêmico, revisar citações/formatação e obter de cada colega seus relatos de participação/conclusão individual. Mobile pode ser descrito como etapa futura até existir entrega executada, respeitando a decisão de Daniel de deixá-lo por último. Não inventar contribuições, reuniões, implantação ou resultados.
