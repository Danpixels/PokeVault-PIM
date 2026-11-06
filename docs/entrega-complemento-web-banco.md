# Estado Banco — complemento web interrompido

Registro efetivo: **03/11/2026**, atualizado às **15h14min45s, America/Sao_Paulo / 18:14:45 UTC**. A coordenação transmitiu a mudança direta de prioridade de Daniel: discutir as quatro melhorias por último, junto do fluxo de compras, e interromper sua implementação. **Janela de aplicação encerrada; complemento adiado. Não executar 014–018 nem iniciar testes dessas melhorias sem nova autorização.**

## Ponto seguro atual

**Ambas as bases continuam no schema versão 4. Nenhuma migração deste complemento foi aplicada com sucesso.** A única tentativa de migração, 014 em PokeVault, falhou na compilação antes de executar seu batch; 015/016 não foram chamados e nenhum DDL deste complemento foi enviado a PokeVaultTests. Não houve necessidade de reversão ou estabilização por escrita após a interrupção.

Uma leitura autorizada posterior confirmou, nas duas bases, MAX(SchemaVersao)=4, ausência da coluna Colecao.QuantidadeTotalCartas e ausência das novas procedures usp_CadastrarColecao/usp_AdicionarItemCarrinho. Em PokeVault ainda constam três cartas com raridade Comum: a normalização planejada não foi executada. As demais raridades observadas foram Demonstração (3) e Double Rare (1), preservadas sem inferência.

A demonstração **5187** respondeu **HTTP 200 em /health e /api/v1/ofertas** na checagem somente leitura posterior. Isso comprova disponibilidade dessas duas rotas nesse instante, não uma regressão completa de interface/compra. Banco não alterou código, processo, configuração, credenciais, saldo, estoque, compras ou histórico para estabilizar a demonstração.

## Trabalho efetivamente escrito, preservado

| Arquivo | Estado ao interromper |
| --- | --- |
| docs/contrato-complemento-web.md | Publicado primeiro; proposta de coleções/raridades/soma, agora adiada. Não representa autorização ativa para aplicar |
| database/014_web_colecoes_raridade.sql | Escrito; uma tentativa em PokeVault falhou por erro 207 de compilação. **Ainda precisa de correção, não aplicar** |
| database/015_web_cadastros.sql | Rascunho de procedures de coleção/raridade; não enviado ao motor, não compilado/verificado |
| database/016_web_carrinho_aditivo.sql | Rascunho de soma atômica e validação do teto da edição absoluta; não enviado ao motor, não compilado/verificado |
| database/017_web_verificar.sql | Consultas preparadas para o schema novo; não executadas |
| database/018_web_preservacao.sql | Consulta de preservação escrita; executada somente uma vez para snapshot anterior de PokeVault; sem comparação posterior |
| docs/entrega-complemento-web-banco.md | Este estado real e ponto de interrupção |

Preparar.ps1, README do banco e scripts históricos 000–013 **não foram alterados nesta rodada**. Nenhum arquivo escrito foi apagado ou revertido. src/, tests/, decisões e diário centrais não foram modificados por Banco; não foram criados chats nem enviadas mensagens a chats.

## Execução, janela e falha reais

1. Leitura de DECISOES.md/diário e código existente; publicação do contrato e rascunhos. Não houve instalação de ferramenta.
2. Consulta sqlcmd restrita falhou com erro ODBC/SSL já conhecido. Repetição autorizada fora da restrição funcionou: **DESKTOP-641C55N\SQLEXPRESS, motor 17.0.1000.7**, PokeVault/PokeVaultTests marcadas como PIM-2026-Sprint1, versões 1–4. Nenhuma alteração global de segurança.
3. A primeira inspeção mostrou uma requisição ativa em PokeVaultTests; nova leitura não encontrou requisições/transações. O documento de Testes registrava “nenhuma escrita mutante nesta preparação antes da liberação do schema”. Essa evidência foi preservada antes de abrir a janela às 15h13 locais. Não se presumiu autorização por silêncio.
4. Guarda imediatamente anterior à tentativa retornou **SEM_REQUISICOES_TRANSACOES_TESTES**, às **18:14:17.519 UTC**. Foi capturado o snapshot somente leitura de PokeVault, antes de qualquer tentativa de DDL.
5. Tentativa de 014 em **PokeVault**, iniciada às **18:14:17.778 UTC**, usando `sqlcmd -S '.\SQLEXPRESS' -E -C -I -b -W -w 240 -f 65001 -l 5 -t 30 -v DatabaseName=PokeVault -i database/014_web_colecoes_raridade.sql`, terminou com **exit 1**, erro abaixo. O comando agregado interrompeu e não avançou para 015/016 ou PokeVaultTests.

```text
Mensagem 207, Nível 16, Estado 1, Servidor DESKTOP-641C55N\SQLEXPRESS, Linha 22
Nome de coluna 'QuantidadeTotalCartas' inválido.
```

A compilação do CHECK que referencia a coluna acrescentada no mesmo batch precisa ser separada/deferida quando o trabalho for novamente autorizado. **Essa correção não foi realizada**, pois a ordem de interrupção chegou após o retorno da falha. Não houve repetição da migração ou teste novo após a ordem. Nenhuma transação foi cancelada à força e nenhum reset foi executado.

6. Após a interrupção, apenas SELECTs de metadados/raridade e GETs de disponibilidade confirmaram o estado consistente descrito acima, com exit 0. Não houve necessidade de escrita corretiva.

## Evidências existentes

Em `database/evidencias/complemento-web/`:

- janela-testes-aguardando.txt: cópia do início do documento de Testes que aguardava liberação, capturada antes da tentativa;
- aplicacao.txt: guarda de janela e erro 207 real de 014;
- PokeVault-antes.txt: snapshot de 12 tabelas de negócio, com contagens/SHA256 e projeção intencional Comum→Common para comparação planejada; **não** é prova de normalização aplicada nem de igualdade antes/depois;
- estado-interrupcao.txt: versões 4, ausência da coluna/procedures novas, raridades observadas e HTTP200 nas duas rotas da demonstração.

Não foram gerados snapshot posterior, relatório de preservação aprovado ou evidência de caso funcional mutante. Não expor senhas/hashes individuais/credenciais; os arquivos de evidência não contêm esses valores.

## Dependências e próximo ponto seguro

O complemento aguarda **nova autorização de Daniel pela coordenação**, junto da discussão final de compras. Até lá: manter schema 4, demonstração funcional e massa de PokeVaultTests com Testes; não aplicar os rascunhos ou integrar chamadas que dependam das procedures ausentes.

Se autorizado futuramente, retomar pela revisão/correção do 014, confirmar novamente janela com Testes e estado das bases, revisar rascunhos contra as decisões então vigentes e somente depois aplicar/testar. O teto 10000, política sem reserva, coleção legada nullable e lista de raridades permanecem propostas documentadas, não comportamento novo implantado por esta frente. A política de conta runtime externa continua a ressalva anterior, sem mudança de segurança global.

Nenhuma melhoria adiada foi concluída artificialmente para fechar a rodada. Mobile, nova sprint e reescrita acadêmica final não foram iniciados.
