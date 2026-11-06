# Toques finais — gestão da coleção151 e fluxo de compra

Executado exclusivamente pelo chat de coordenação, a pedido de Daniel, em **03/11/2026**, fuso America/Sao_Paulo. Não representa abertura ou aceite de nova sprint.

- Gestão de Catálogo única, usando a biblioteca local207/165, com9 páginas e formulários abaixo das imagens. Cadastro de versões por idioma/estado/acabamento; edição de preçoBRL/unidades sem justificativa manual, com auditoria automática e proteção contra formulário desatualizado. Zero unidades retira a versão da vitrine. Média Cardmarket compacta em moeda original e com data, quando disponível; não converte/preenche BRL. Detalhes técnicos da API deixaram a navegação principal.
- Vitrine pública agrupada por carta em estoque, nome/unidades/preço mínimo; detalhe com versões e botão Comprar que adiciona ao carrinho. Sacola ao lado do nome do cliente. Carrinho com coleção/número/estado/idioma/acabamento, quantidade, preço unitário/subtotal e total selecionado. Mantidas seleção parcial, edição absoluta/remover, confirmação de preço, saldo fictício e estorno.
- `database/024_fluxo_catalogo151.sql`: schema11, identidade comercial ampliada, idioma/acabamento sem presumir dados legados, snapshots em novas compras, operação aditiva atômica. Rascunhos014–018 continuam suspensos. Preparador e prontidão atualizados. As rotas MVC antigas `/gestao/ofertas` e `/gestao/ofertas/nova` redirecionam para a gestão única; contratos API legados continuam disponíveis.

## Verificação executada

- Build final: zero erros/avisos. DLL SHA256 `1F34AC2895889603D276A39C971AD2ADBE7847A27B8663C02966D03389C6C089`.
- **45/45 regressão**: [resultado final](evidencias/fluxo151/regressao-fluxo151-final/resultados.json). Inclui SQL/HTTP/MVC, saldo, autorizações, CSRF, concorrência, rollback e idempotência.
- **10/10 fluxo151** no mesmo executável: [resultado final](evidencias/fluxo151/fluxo151-final/resultados.json). Gestão207 em9 páginas; versões independentes; duplicidade/entrada inválida/CSRF/perfis; auditoria automática/conflito de versão; detalhe/vitrine; soma concorrente ao carrinho; metadados/linhas; compra selecionada com snapshots; replay/estorno; estoque zero/remover.
- Primeiro ensaio da regressão:44/45, preservado em [resultado inicial](evidencias/fluxo151/regressao-fluxo151/resultados.json). Falha esperava HTTP200 na antiga gestão; o redirecionamento302 é parte da unificação solicitada. A verificação foi atualizada para conferir destino e gestão nova200. Nenhum resultado inicial sobrescrito.
- Migração aplicada primeiro em PokeVaultTests. Demonstração atualizada após testes: **conteúdo de todas as colunas preexistentes das12 tabelas comerciais idêntico antes/depois**, incluindo rowversion. Somente as seis colunas novas foram excluídas da comparação, pelo script025. [Antes](evidencias/fluxo151/demonstracao/antes.txt), [depois](evidencias/fluxo151/demonstracao/depois.txt), [prontidão/hash](evidencias/fluxo151/demonstracao/resultado.json).
- Servidor5187 reiniciado, `/health/ready`200/schema11 e home200. Nenhum reset de credenciais/saldo/estoque/histórico. Nenhuma instrução enviada aos outros chats nesta rodada.

## Limites e observações

Verificação de telas foi HTML/HTTP; não é inspeção visual no navegador. O limite anterior de automação do navegador não foi contornado. Total selecionado tem comportamento JavaScript implementado; a execução visual desse controle aguarda revisão manual. O harness apresentou aviso NU1900 porque a consulta de vulnerabilidades NuGet estava indisponível, sem falha de compilação/testes; não foi alterada configuração global ou desativada essa consulta.

Idioma/acabamento antigos aparecem como não informados; os formulários de versão existente preservam sua identidade. Para outra combinação, usar Adicionar outra versão. Saldos e cancelamentos continuam com seus contratos de motivo, enquanto ajustes do catálogo usam registro automático. Olho de senha, coleções genéricas, raridades fechadas e mobile permanecem fora desta rodada. Reescrita acadêmica e aceite manual de Daniel pendentes.
