# Complemento web — preparação e testes

Registro inicial em 03/11/2026, America/Sao_Paulo. Frente de testes com apoio de IA. Relato de Daniel sobre testes manuais é informação de usuário; não foi convertido em testes desta suíte. Banco e Desenvolvimento trabalham em paralelo; nenhuma escrita mutante nesta preparação antes da liberação do schema.

## Critérios preparados, ainda não executados

| Grupo | Cenários necessários | Aceite |
| --- | --- | --- |
| Coleção | ADM/atendente cadastram nome+total; seleção ao cadastrar carta; código técnico | Total inteiro positivo, código único gerado pelo servidor, nova coleção utilizável imediatamente |
| Coleção e legados | Nome vazio/espaços, zero/negativo/fracionário/overflow, duplicidade; cliente/anônimo | Recusa sem gravação indevida; coleção legada com total não informado permanece utilizável; total não baixa estoque nem limita número da carta |
| Raridades | As 12 opções aprovadas; API e SQL fora da lista; legados desconhecidos | Novos cadastros respeitam lista inglesa; nenhum dado legado desconhecido é inferido/reescrito; Secret Rare não é opção nova |
| Símbolos | Catálogo e gestão, SVG local e nome acessível | Círculo/losango/estrelas ingleses com nome, sem depender somente de cor ou de fonte externa |
| Senha | Login/cadastro; oculto inicial; botão type=button; teclado/aria | Alternância preserva valor, atualiza ação/estado e não envia formulário; credenciais nunca entram na evidência |
| Carrinho | POST novo adiciona 1+2=3; PUT antigo substitui; remoção | Operações distintas, sem reserva de estoque/débito ao adicionar/editar |
| Concorrência e limites | Inclusões simultâneas; zero/negativo/fracionário/overflow; inativos/perfil/CSRF | Nenhuma atualização perdida; recusa atômica e sem efeitos sobre saldo/estoque |
| Regressão | Seleção/remanescentes; preço muda; saldo/estoque insuficientes; estorno/UUID | Regras anteriores preservadas, SQL real e contas/massa próprias |

## Preparação real desta frente

Decisões/diário lidos. Criado projeto de harness independente em tests/ComplementoWeb, reutilizando uma cópia dos auxiliares SQL/HTTP anteriores sem alterar a evidência ou a suíte histórica. O corpo de execução aguarda nomes/contratos reais. Browser inventory inicialmente não listou conectores de navegador, mas há Chrome nativo; verificar viabilidade de interface real em janela própria na porta5188, sem interagir com a demonstração5187.

Todas as linhas acima são planejadas nesta abertura; não constituem aprovações. Relatório finito e rodadas serão acrescentados após liberação do banco e versão executável. PDF, documentos centrais e mobile ficam fora desta frente.

## Interrupção por mudança de prioridade — 03/11/2026

Daniel adiou as quatro melhorias para discussão posterior junto do fluxo de compras. Preparação/execução deste complemento foi interrompida. Não iniciar nova operação de teste/migração nem completar o recurso adiado. A compilação já em andamento foi aguardada: exit0,0 erros, um warningNU1900 pela consulta NuGet indisponível no contexto restrito. Isso não comprova funcionamento do complemento.

**Zero cenários funcionais executados; zero fixtures ou escritas SQL desta rodada.** Foi realizada somente leitura SQL do baseline, com schema máximo4 observado às18:12:12.7657833UTC, antes da fixture (que não chegou a iniciar). A evidência contém112 registros legados desconhecidos com valor Teste; nenhuma comparação pós-upgrade foi executada por esta frente. A consulta restrita inicial falhou com ODBC/criptografia; leitura autorizada fora da restrição passou, sem alteração global de segurança.

Harness/auxiliares/consulta e matriz foram escritos e preservados. Nenhum servidor5188, navegador de teste ou janela Chrome própria foi iniciado. Inventário de UI indicou Chrome nativo; tentativa de criar aba embutida IAB retornou indisponível. Não houve navegação, alternância de senha, screenshot ou entrada de credencial. Os cenários de navegador, acessibilidade e SQL/HTTP permanecem não executados.

A orientação anterior de base livre foi substituída em tests/prontidao-complemento-web.md. A última entrega de Banco lida já informava janela de aplicação em andamento; o schema final deve ser informado por Banco, sem presumir que continua4 ou que foi liberado. Esta frente não aplica/aborta/reverte migrações de Banco. Resumo finito: entrega-complemento-web-testes.md. Retomada exige nova autorização de Daniel; não iniciou mobile nem revisão acadêmica final.
