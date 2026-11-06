# Entrega sprint 2 — testes e documentação

Registro real: **21/10/2026**, America/Sao_Paulo. Frente executada por Codex com apoio de IA. Entrega finita para consolidação da coordenação no diário único; não encerra automaticamente a sprint e não inicia a sprint 3.

## Resultado

**45 cenários executáveis distintos com último resultado aprovado: 21 SQL, 18 HTTP e 6 MVC.** Base real PokeVaultTests, schema versões 1/2/3/4, processo próprio na porta 5188. Servidor 5187 e dados/credenciais PokeVault não foram alterados por esta frente. Processo isolado foi encerrado ao fim; evidência disponível.

| Rodada real preservada | Resultado |
| --- | --- |
| 01 | 32/32 aprovados |
| 02 | 40/40 aprovados |
| 03 | 44/44 aprovados, incluindo auditoria 013 |
| 04 | 44 aprovados e 1 reprovado por expectativa inadequada do teste HTTP18 |
| 05 — reteste somente HTTP18 | 1/1 aprovado após corrigir o teste para exigir recusa 400 de campos extras e nenhuma conta criada |

Não declarar a rodada 04 como 45/45 nem o reteste isolado como suíte inteira. Último estado consolidado: 45 aprovados distintos, 0 defeitos funcionais confirmados abertos nesta execução. Uma reprovação do roteiro foi preservada; não houve mudança do produto para corrigir essa expectativa. Isso não representa execução completa de todos os 36 casos conceituais T01–T36 nem validação integral do sistema. Os 30 checks integrados+7 MVC anteriores de Desenvolvimento são referência separada, não somados.

## Evidências e artefatos entregues

- **docs/testes-sprint2.md:** ambiente, matriz requisito→cenário→evidência, comandos, valores, problemas/retestes e limites.
- **docs/evidencias/sprint2/:** rodadas01–05; manifestos de produto/harness/fontes/scripts; referências de Desenvolvimento; tentativas e retestes de reconciliação; encerramento do processo isolado.
- **tests/Sprint2/:** harness .NET 10 independente, fixture própria, concorrência com duas conexões/barreira, SQL/HTTP/MVC, auditoria e consultas finais. Senhas aleatórias somente em memória, sem credenciais de demonstração.
- **tests/Executar-Sprint2.ps1:** wrapper de reprodução isolada, com parser validado sem erros; ainda não executado integralmente. O harness e os fluxos descritos foram executados de fato por comandos diretos.
- **tests/prontidao-sprint2.md:** passagem de base livre e retomada após conferência da versão 4; estado final sem atividade mutante.
- **docs/plano-testes.md:** atualizado com cobertura real, rastros e critérios ainda não cobertos integralmente.

DLL da aplicação executada, igual ao binário de origem na conferência final: SHA256 `9B717A9B37A733A9213284C93A6DE2FC574CB66D383281AF1C5B20B7175A5EDC`. Manifestos identificam as versões do harness e scripts de cada rodada. Não houve modificação desta frente em src/, database/, arquivos centrais/diário ou documentos acadêmicos finais; não foram criados chats nem enviadas mensagens a chats.

## Resultados substantivos

Administrador cadastra cliente/atendente via API e formulários; cliente nasce com saldo 0/carrinho vazio; listagens não expõem senha/hash. Definição de saldo substitui o atual, audita delta/autor/motivo e não gera crédito/movimento/token extra quando não muda. Desativação invalida cookie existente do atendente; reativação e novo login funcionam; ADM fixo protegido.

Atendente cadastra/ajusta catálogo por API/MVC, mas recebe403 para saldo/usuários/estorno. Cliente e anônimo são recusados conforme401/403. Antifalsificação, duplicidades, escalas/valores inválidos, token de versão inválido e campos de perfil extras foram verificados. Ajustes com token antigo são409/51012, preservando venda/ajuste concorrente e exigindo recarga no HTML.

Regressão real cobriu compra selecionada e carrinho remanescente, conservação independente, preço alterado recusado antes de novo aceite, saldo/estoque insuficientes sem efeitos parciais, UUID sequencial/simultâneo, última unidade e saldo disputados, estorno único simultâneo, valor original e saldo corrente. Rollback de compra, estorno e auditoria foi provocado **após escrita real** por triggers temporárias exclusivas de PokeVaultTests, com alcance confirmado por SESSION_CONTEXT e sem transação externa/procedure de falha no produto.

Banco informou lacuna de autor/motivo em alterações só de preço/ativação e publicou 013. Esta frente registrou base livre; a leitura subsequente mostrou versão 4 já aplicada às 17:54:38.108UTC, antes da rodada 02. Casos específicos SQL19/SQL20/HTTP17 posteriores conferiram AlteracaoCarta: autor/motivo e valores anteriores/novos, sem movimento de estoque delta 0 e sem duplicação em no-op/conflito. A lacuna anterior não foi reproduzida por esta frente antes da correção.

**Reconciliação final:** scripts 007/011 e consulta própria retornaram exit0; zero divergências de saldo, estoque e totais; zero triggers temporárias remanescentes; versões instaladas4. O007 não retornou linhas nas oito consultas de divergência. Estado final preservado em PokeVaultTests:60 compras, 87 usuários e 115 cartas de teste/seed. Os históricos não foram apagados.

## Problemas, correções e limites

1. A conexão SQL restrita falhou com ODBC/criptografia; repetição autorizada fora da restrição funcionou. Não se alterou segurança global nem houve rejeição automática de aprovação.
2. Pós-verificação inicial falhou por uso conjunto de-W/-y no sqlcmd. Evidência mantida; repetição sem-W e verificação final passaram. Erro do roteiro, não do produto.
3. HTTP18 esperava201 apesar de campos de perfil proibidos; API respondeu400 por JsonUnmappedMemberHandling.Disallow. Expectativa corrigida para400 e ausência de cadastro; reteste focado aprovado. Falha original não foi apagada.
4. Build do harness aprovado com 0 erros e warning NU1900: consulta de vulnerabilidades NuGet indisponível no contexto restrito. Não confundir com build do produto, relatado por Desenvolvimento com 0 avisos/erros.
5. Permissões do principal de banco foram comprovadas por EXECUTE AS e verificações de SELECT/EXECUTE/negação de DML/legados; **conexão externa runtime com identidade Windows de menor privilégio permanece pendente**. Execução usou credencial de preparação. Não criar login/conta Windows nem mudar autenticação global nesta frente.
6. Não houve inspeção visual ampla/acessibilidade, mobile, ensaio de carga ou repetição estatística de concorrência. MVC foi testado por HTML/HTTP e submissões reais. Alguns subcasos conceituais T01–T36 ainda não foram executados integralmente; relatório/plano explicitam exemplos.

## Insumo factual ao diário

Em 21/10/2026, após a liberação de PokeVaultTests, a frente Testes provisionou massa própria e executou testes SQL Server/HTTP/MVC em cópia isolada da aplicação na porta 5188. Consolidou 45 cenários distintos aprovados após um reteste de expectativa do roteiro, mantendo todas as tentativas. Comprovou conflitos de versão, auditoria inclusive sem delta de estoque, concorrência e rollback após escrita. A reconciliação final não encontrou divergências nem objetos de falha remanescentes. O processo isolado foi encerrado; banco de demonstração e servidor 5187 foram preservados por esta frente. Permanece pendente a identidade externa runtime de menor privilégio, além dos limites de cobertura descritos. PDF/texto acadêmico final não foi reescrito, e sprint 3 não foi iniciada.


