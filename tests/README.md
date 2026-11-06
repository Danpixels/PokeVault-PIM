# Preparação de testes PokéVault

Não há suíte funcional executada nesta entrega. Não foi criado projeto de testes que exija mudar solução, src/ ou database/ enquanto contratos HTTP e aplicação ainda estão em construção.

- `Inspecionar-Prontidao.ps1`: inspeciona apenas presença/hash de arquivos e lista scripts SQL. Executar na raiz com `& ./tests/Inspecionar-Prontidao.ps1`. Não conecta banco nem inicia app. O resultado JSON pode ser salvo em uma nova evidência de rodada, sem sobrescrever inspeções anteriores.
- `Conferir-Estado.sql`: SELECTs preparados com base no contrato e no plano de banco; ainda não executados nem validados contra DDL implantado. Exige banco PokeVaultTests e ClienteId positivo preenchido no arquivo de execução; CompraId é opcional. Não contém DML/reset e não invoca procedures. Ler as restrições antes de usar. Uma cópia com IDs de uma execução deve ser guardada junto à sua evidência, mantendo este modelo genérico.

Roteiro e bloqueios: `../docs/testes-sprint1.md`. Critérios: `../docs/plano-testes.md`. Automação HTTP/transacional será acrescentada após a identificação das rotas/DTOs, sessão/antifalsificação, base própria, forma segura de provisionar credenciais e procedimentos reais de preparação. Usar SQL Server real; não simular integridade com banco em memória.

Para concorrência, documentar barreira de início, conexões independentes, payloads/UUIDs sanitizados, respostas de cada tentativa e consultas finais. Para rollback, comprovar que a falha ocorreu após escrita; rejeição de entrada antes de escrita não basta. Nenhum comando neste diretório cria/destroi banco ou altera arquivos das outras frentes.
