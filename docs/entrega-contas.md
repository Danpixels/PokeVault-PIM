# Cadastro e troca de senha —03/11/2026

Solicitado por Daniel para finalizar a demonstração web, executado apenas neste chat. Esta autorização substitui a restrição anterior de cadastro exclusivamente administrativo e permite ao titular trocar sua senha; não muda o perfil de nenhuma conta existente.

- `/criar-conta`: formulário público de email/senha, com olho. Perfil CLIENTE fixo no servidor/SQL, saldo0 e carrinho vazio criados na mesma transação. Nome de exibição derivado da parte anterior ao@, sem outro campo. Email normalizado único; senha de6 a128 caracteres, hash PasswordHasher. Sem confirmação por email ou crédito automático.
- `/conta`: acessível clicando no nome no cabeçalho, para todos os perfis. Exige senha atual, nova senha e confirmação. O ID vem da sessão; não aceita alvo arbitrário. Atualização condicionada ao hash anterior evita sobrescrita concorrente. A sessão atual é renovada; outras sessões são recusadas na próxima requisição por divergência da versão de senha derivada do hash. Cookie nunca recebe a senha ou o hash SQL em claro. Cookies da versão anterior, sem o novo identificador de senha, também exigem novo login.
- MVC com antiforgery e limite de tentativas por IP no cadastro/por conta na troca. RateLimiter executa depois da autenticação para identificar a conta. Formulários de erro não devolvem senhas preenchidas. Cadastro administrativo anterior permanece disponível.
- `database/026_contas_senhas.sql`, schema12: publica somente duas procedures e versão, sem regravar contas existentes. Preparador/launcher/readiness atualizados. Demonstração comparada antes/depois pelo script023:12 tabelas idênticas, incluindo todas as colunas atuais. Contas, credenciais iniciais, saldo/estoque/histórico não foram resetados.

## Evidências

DLL SHA256 `64F1DECDB77CBC6F5C0126C18864A8EC50E2986C6A702E6B06F7FBD0BC01514A`, build do produto0 erros/avisos.

- [10/10 contas](evidencias/contas/contas-validacao/resultados.json): formulário/proteção, CSRF/entradas inválidas, perfil fixo/saldo0/hash, duplicidade, validação da troca, alvo próprio, invalidação de outra sessão, login antigo/novo, três perfis e concorrência.
- [45/45 regressão](evidencias/contas/regressao-contas/resultados.json), mesmo produto: financeiro/catálogo/SQL/HTTP/MVC, autorização, isolamento, concorrência, rollback e idempotência.
- [Preservação e prontidão](evidencias/contas/demonstracao/resultado.json), com arquivos antes/depois e log de migração na mesma pasta. HTTP5187 pronto/schema12.
- Primeiro build do harness falhou por nomes distintos de campo nos dois helpers Form existentes (`html`/`text`); corrigidas as referências no teste de contas. Não houve execução dos cenários nesse ensaio; a pasta contas-inicial preserva o encerramento. Aviso NU1900 de consulta NuGet indisponível permanece no harness sem impedir a execução final. Nenhuma configuração global alterada.

## Limites

Verificações de interface são HTML/HTTP, sem alegar inspeção visual. Não há recuperação de senha por email nem troca de email/perfil. Arquivo DPAPI do bootstrap guarda as credenciais **iniciais**; após troca pelo titular, a senha válida é a escolhida por ele, não o valor histórico do arquivo. Novas senhas não são copiadas para esse arquivo ou evidências. Nenhuma senha da demonstração foi trocada automaticamente nesta entrega.
