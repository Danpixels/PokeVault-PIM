# Sprint 2 — uso dos testes executáveis

O projeto PokeVault.Testes.csproj é console.NET 10 independente, sem alterar solução/src/database. Executa21 cenáriosSQL,18 HTTP e 6 MVC em PokeVaultTests real, schema 1/2/3/4. Não usar o banco demo, alterar senhas da demonstração nem reaplicar scripts durante fixture mutante.

Pré-condições: Banco liberou base marcada; Desenvolvimento compilou aplicação; .NET 10 e pacotes compatíveis disponíveis; credencial Windows local autorizada para fixture de instalação e criação/remoção de triggers na base exclusiva. Processos5187/demonstração ficam preservados. Cada rodada provisiona dados novos; não existe reset ou exclusão de histórico. Senhas aleatórias vivem só em memória.

Roteiro encapsulado proposto na raiz: `./tests/Executar-Sprint2.ps1 -Rodada minha-rodada-unica`. Recusa porta 5188 ocupada e nome de evidência existente; copia binários para tests/.runtime/, inicia só a cópia5188 e encerra só seu PID em finally. Parser validado sem erro; esse wrapper ainda não foi executado completo nesta rodada. O harness foi de fato compilado/executado por comandos diretos, descritos em docs/testes-sprint2.md.

O console recebe um caminho novo de JSON e recusa sobrescrita. PV_TEST_HTTP=1 habilita HTTP na porta 5188; sem isso registra bloqueio HTTP. PV_TEST_FOCUS=HTTP18 executa apenas reteste do cenário de perfil/anônimo. Não manter essa variável ao rodar toda a suíte. A aprovação depende dos registros JSON, nunca apenas do exitCode0 do console.

Core em Program.cs; versão/privilegio em VersionTests.cs; HTTP/MVC em HttpTests.cs/AdditionalTests.cs; auditoria013 em AuditTests.cs. Verificar-Integridade.sql faz somente leitura final da base de testes. Evidências reais: docs/evidencias/sprint2; relatório: docs/testes-sprint2.md. Roteiros e evidências não contêm senhas/tokens/strings de conexão com senha. Snapshot/runtime, bin eobj são ignorados em tests/.gitignore.


