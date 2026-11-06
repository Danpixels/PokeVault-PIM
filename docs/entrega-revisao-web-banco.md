# Revisão web local — Banco

**03/11/2026, America/Sao_Paulo. Revisão encerrada nesta frente, somente por leitura.** Leituras principais às 19h09 locais; credenciais conferidas às 19h13. Sem DDL, DML, fixture, reset, instalação, alteração de processo/segurança ou criação de identidade/infraestrutura. Complemento e mobile continuam suspensos.

## Resultado observado

| Verificação | PokeVault | PokeVaultTests |
| --- | --- | --- |
| Instância/motor | DESKTOP-641C55N\SQLEXPRESS, 17.0.1000.7 | Mesma instância/motor |
| Marcador | PIM-2026-Sprint1 | PIM-2026-Sprint1 |
| SchemaVersao | 1, 2, 3, **4** | 1, 2, 3, **4** |
| Vestígios do complemento | Coluna QuantidadeTotalCartas, constraint CK_Colecao_TotalCartas, duas procedures novas e versões posteriores: **todos ausentes** | **Todos ausentes** |
| Reconciliação 007 | Exit 0; zero linhas nas oito consultas de divergência | Exit 0; zero linhas nas oito consultas de divergência |
| Rowversion/rotinas de ajuste | Duas colunas de 8 bytes NOT NULL; ANSI_NULLS/QUOTED_IDENTIFIER=1 nas duas rotinas versionadas | Mesmo resultado |

007 conferiu saldo/estoque contra movimentos, total contra itens, lançamentos de compra/estorno e vínculos cliente/carteira/carrinho. As datas de modificação das procedures de compra, cancelamento, cadastro de carta e carrinho absoluto continuam em 21/10/2026. Não foi encontrado DDL parcial implantado do complemento suspenso; nenhum script 014–018 foi aplicado ou corrigido nesta revisão.

A parte pertinente de **011** foi executada por consultas de metadados e HAS_PERMS_BY_NAME, sob EXECUTE AS USER do principal de banco já existente: SELECT e EXECUTE versionado permitidos; UPDATE/INSERT/DELETE diretos e execução dos dois ajustes legados negados. **011 não foi executado integralmente**, pois contém tentativas de DML, ainda que sem linhas, e chamadas de procedures; esta rodada exigia somente leitura. A adaptação está registrada no arquivo SQL de evidência.

## Contas e dados de demonstração

As quatro contas originais identificadas no bootstrap (IDs 1–4) foram encontradas, ativas e com os perfis esperados: administrador, dois clientes e atendente. **4/4 senhas do arquivo DPAPI atual foram verificadas com sucesso em memória pelo PasswordHasher da plataforma contra os hashes SQL**, sem login HTTP, rehash ou escrita. O resultado persistido contém apenas IDs/perfis/booleanos; não foram registrados senhas ou hashes individuais.

As três cartas das chaves naturais de demonstração MEW-151 foram encontradas: Pikachu 025 NM R$30/estoque2, Pikachu 025 SP R$20/estoque3 e Charmander 004 NM R$15,50/estoque1, ativas e ainda com raridade Comum. Saldos observados dos clientes originais: R$100 e R$10. Isso descreve o **estado presente**: semelhança com valores de referência não prova igualdade de todo o histórico. Compatibilidade com o arquivo protegido atual também não prova que senha/hash jamais mudou.

Contagens atuais não foram impostas como iguais às antigas: PokeVault tinha 11 usuários e 7 compras; PokeVaultTests, 87 usuários e 60 compras. Não se atribuiu autoria dessas operações nem se removeu dado para reproduzir uma massa anterior. A reconciliação verifica consistência observada, não uma comparação histórica integral ou ausência universal de defeitos.

## Execução e evidências

Arquivos em `database/evidencias/revisao-web/`:

- `execucoes.json`: quatro execuções SQL com horário, consulta e exit 0;
- `PokeVault-007_verificar.txt` e `PokeVaultTests-007_verificar.txt`: resultados completos de 007;
- `estado-e-permissoes-leitura.sql` e resultados `<base>-estado-e-permissoes-leitura.txt`: SELECTs de schema/contagens e parte somente leitura de 011;
- `originais-estado-observado.txt`, `ambiente.txt`: dados atuais de referência e ambiente;
- `credenciais-verificacao-sanitizada.json`: verificação final válida das quatro contas;
- `credenciais-tentativa-invalida.json`: diagnóstico anterior explicitamente inválido, preservado para não confundir erro do roteiro com conta ausente.

Forma dos comandos executados: `sqlcmd -S '.\SQLEXPRESS' -E -C -I -b -W -w 240 -f 65001 -l 5 -t 30 -v DatabaseName='<base>' -i '<consulta>' -o '<evidência>'`. Somente as duas bases marcadas foram consultadas. O contexto restrito reproduziu o erro ODBC/SSL; repetição autorizada funcionou, sem mudar configuração global. Não houve rejeição de aprovação nesta rodada.

Na conferência de credenciais, a primeira combinação de opções `-h`/`-y 0` foi recusada pelo cliente; a tentativa seguinte via PRINT não retornou JSON e gerou um diagnóstico inválido de ausência. Isso era erro do roteiro, **não falha de credenciais**. Corrigiu-se para SELECT/FOR JSON com `-y 0`, captura somente em memória e validação de quatro registros antes da comparação. A execução final às 22:13:22 UTC passou nas quatro contas. Valores privados não foram expostos nas tentativas.

## Limites e encaminhamento

Conta Windows dedicada/conexão runtime externa **não é requisito nem bloqueio desta demonstração acadêmica local**, conforme decisão de Daniel. Mantém-se a identidade Windows de desenvolvimento; menor privilégio em eventual uso futuro é apenas recomendação. A checagem do principal WITHOUT LOGIN demonstra permissões internas, não uma conexão externa própria.

As consultas foram pontuais, não um snapshot histórico global; operações legítimas posteriores podem mudar contagens/saldos. Regressão funcional, navegador e fluxos concorrentes pertencem à frente de Testes, sem somar estas leituras como casos HTTP aprovados. Não foram alterados src/, tests/ ou documentos centrais nem enviados contatos a outros chats. **Nenhuma ação adicional de banco é necessária nesta revisão local com base nos resultados observados.**
