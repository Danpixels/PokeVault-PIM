**Atualização de contas —03/11/2026:**026 após024 publica schema12, cadastro público sempre CLIENTE/saldo0/carrinho vazio e troca de hash condicionada ao anterior. Não regrava usuários existentes. Preparador inclui026. Demais registros abaixo são históricos da respectiva rodada.

**Atualização03/11/2026 — toques finais em chat único:** aplicar024 depois de021 para schema11. Não aplicar014–018. O024 preserva IDs/preços/estoques/saldos/histórico e acrescenta idioma/acabamento (legado não informado), identidade única por variante e snapshots para novas compras. Publica `usp_AdicionarItemCarrinho` nova, com soma atômica/validação de estoque sem reserva. `025_fluxo151_preservacao.sql` verifica as12 tabelas, excluindo somente as seis colunas novas para comparar antes/depois. O preparador já inclui024 e ignora versão11 previamente aplicada. Os registros abaixo descrevem a entrega anterior em schema10.

# Banco SQL Server — preparação e atualizações

**Estado atual em 03/11/2026: integração TCGdex 151 autorizada e schema com versões 1,2,3,4,8,9,10.** Scripts 014–018 são rascunhos suspensos: não executar, corrigir ou incluí-los no preparo por esta autorização. A revisão web anterior em schema4 permanece histórica. Para esta extensão, consultar `../docs/contrato-tcgdex151.md` e `../docs/entrega-tcgdex151-banco.md`.

019 cria cinco tabelas e uma view de biblioteca, sem alterar dbo.Carta/Colecao. 020 publica manifesto/lotes/falhas; 021 publica vínculo opcional seguro e cadastro comercial explícito a partir de referência. 022 verifica metadados/permissões; 023 compara as 12 tabelas comerciais existentes, incluindo todas as colunas. Migração não importa cartas, não cria ofertas, não baixa imagens e não redefine preço/saldo/estoque. Importação HTTP é executada pela aplicação depois do schema, sem transação SQL aberta durante rede.

O preparador atual acrescenta 019→020→021 depois de 013, pulando versões 8/9/10 já aplicadas. Não percorre 014–018. O upgrade real desta rodada foi aplicado por chamadas diretas 019–021 e verificado por 022/007 na janela descrita na entrega. A atualização final do wrapper tem parser validado, mas não foi reexecutada contra a base já liberada a Testes. Não preparar/aplicar DDL durante cenários mutantes.

Exemplo de preparação futura fora de janela de testes: `./database/Preparar.ps1 -Banco PokeVault -Modo Atualizar`. Base nova identificada: `-Banco PokeVaultTests -Modo Inicializar`. Continuam obrigatórios sqlcmd -I/-b e a guarda de propriedade. A seção histórica abaixo documenta a evolução anterior, sem autorizar as melhorias suspensas.

Atualização efetiva: 21/10/2026, sprint 2. Contratos: `../docs/contrato-sprint1.md` e `../docs/contrato-sprint2.md`. Resultados reais: `../docs/entrega-sprint2-banco.md`; evidências em `evidencias/sprint2/`.

## Ambiente e autorização

Instância comprovada: `.\SQLEXPRESS`, servidor DESKTOP-641C55N\SQLEXPRESS, motor 17.0.1000.7 Express Edition (64-bit). Scripts usam sintaxe disponível desde SQL Server 2017. A conexão Windows no contexto restrito falhou em ODBC/SSL; sua repetição autorizada fora da restrição funcionou. Não alterar protocolos, autenticação, certificados ou segurança global para contornar esse erro; não instalar ferramentas nesta rodada.

Antes de qualquer escrita, executar leitura de identificação:

```powershell
sqlcmd -S '.\SQLEXPRESS' -E -C -I -b -W -w 240 -l 5 -t 10 -Q "SET NOCOUNT ON; SELECT CONVERT(nvarchar(80),@@SERVERNAME) AS Servidor,CONVERT(nvarchar(30),SERVERPROPERTY('ProductVersion')) AS Versao; SELECT name FROM sys.databases WHERE name IN ('PokeVault','PokeVaultTests');"
```

`-E` usa conta Windows; `-C` confia no certificado da instância local selecionada somente nessa chamada. **`-I` é obrigatório** para QUOTED_IDENTIFIER nos índices filtrados existentes; a instrução da primeira entrega sem -I era incompleta e provocou erro 1934 na execução relatada por Desenvolvimento. `-b` interrompe em erro; `-f 65001` lê os arquivos UTF-8. Falha na consulta exige parar a preparação e registrar/obter execução autorizada, sem presumir defeito no banco.

Somente PokeVault e PokeVaultTests são aceitos pelos scripts. Marcador de propriedade: `PokeVaultProjeto=PIM-2026-Sprint1`, mantido também na sprint 2 para reconhecer a mesma base. Script 000 só marca uma base criada por ele; recusa adotar base existente sem marca. Não adicionar marca manualmente a banco desconhecido, não resetar nem remover dados de demonstração.

## Caminho único de preparação

Executar a partir da raiz do projeto. O preparador usa sqlcmd já instalado, consulta a instância primeiro, para no primeiro erro e grava logs sanitizados em `database/evidencias/sprint2/`.

**Atualizar PokeVault existente e identificada**, preservando contas/credenciais/histórico:

```powershell
./database/Preparar.ps1 -Servidor '.\SQLEXPRESS' -Banco PokeVault -Modo Atualizar
```

Aplica somente 008–010 e 013, depois consulta 007/011. Nesta rodada 008–010 foram executados pelo preparador e 013 foi aplicado como complemento separado, sem reescrever scripts já executados. **Não reaplicar 003/004/006 como migração**: são a publicação histórica da sprint 1. O 009/010 restringe ao runtime os ajustes legados sem versão; não desfazer essa proteção reaplicando publicações antigas.

O preparador final consulta SchemaVersao e pula 008/009/013 já aplicados; isso evita republicar a versão de 009 sobre a auditoria de 013. A versão final do wrapper passou no parser PowerShell; sua sequência completa após esse complemento não foi repetida contra a base já liberada. Os logs de execução descrevem a sequência inicial e a chamada separada de 013.

**Preparar base de testes nova**, somente antes da passagem da massa para Testes:

```powershell
./database/Preparar.ps1 -Servidor '.\SQLEXPRESS' -Banco PokeVaultTests -Modo Inicializar
```

Ordem: 000,001,002,003,004,005,006,008,009,010,013; consultas 007 e 011 ao final. PokeVaultTests já foi criada/preparada e liberada nesta rodada às 14h49 locais de 21/10/2026; o complemento 013 foi aplicado às 14h54min38s, antes do aviso de execução concorrente pela frente de Testes. Versões finais 1,2,3,4. Não executar novo DDL sem janela coordenada. Não repetir inicialização ou cenários mutantes enquanto Testes usa a base. Não há reset no preparador; um seed repetido não redefine preços/estoque, mas pode incluir registros ausentes, portanto não deve disputar a massa de outra frente.

Exemplo de chamada direta para um script, com todas as opções relevantes:

```powershell
sqlcmd -S '.\SQLEXPRESS' -E -C -I -b -W -w 240 -l 5 -t 30 -f 65001 -v DatabaseName='PokeVaultTests' -i 'database/008_sprint2_versoes.sql'
```

Não executar sem DatabaseName e -b. Os scripts possuem guardas de nome/marcador/versão. 008 é transacional, acrescenta rowversions e registra versão 2, validando a estrutura em reexecução. 009 publica duas procedures novas e versão 3 em uma transação que atravessa GO; se um batch falhar, sqlcmd -b encerra a conexão e desfaz a transação pendente. 010 configura somente permissões e principal no banco identificado. 013 publica AlteracaoCarta e complementa a procedure versionada sem mudar assinatura, registrando autor/motivo e valores antes/depois também em alterações só de preço ou ativação (versão 4). Não cria auditoria retroativa nem movimento de estoque com delta zero. Para bases novas, a versão 1 vem do 001. Registrar falhas e corrigir/revisar antes de prosseguir; não apagar objetos/dados para forçar sucesso.

## Integração com gestão e compatibilidade

- Ler Versao de SaldoCliente/Carta para telas de gestão; converter bytes em Base64 e exigir exatamente 8 bytes na submissão.
- Chamar `usp_DefinirSaldoVersionado` e `usp_AjustarCartaVersionado` com parâmetro obrigatório Binary(8); erro 51012 significa conflito/HTTP 409 e exige recarga/nova confirmação. Não buscar token novo e reenviar automaticamente um valor antigo.
- SQL rowversion não é horário e se altera automaticamente em UPDATE por compra/estorno/ajuste. Não mudar assinatura de checkout/estorno, nem duplicar efeitos financeiros em C#.
- Ajustes legados da sprint 1 são preservados para preparação/compatibilidade, mas negados ao papel runtime. Bootstrap usa credencial de preparação e pode continuar com `usp_DefinirSaldo` para a primeira carga.
- Cadastro de conta/carta não exige versão prévia; SELECT após criação obtém a versão para edição futura.
- Todas as procedures operacionais recusam transação externa com 51103. Não passar SqlTransaction/TransactionScope para simular rollback.

## Bootstrap e identidade SQL mínima

O administrador de demonstração existente e seus hashes/credenciais foram preservados. Para base de testes, Testes provisiona suas próprias contas/hashes com PasswordHasher<Usuario>, sem reutilizar o arquivo DPAPI da demonstração. Cliente nasce com saldo zero e carrinho vazio; saldo de fixture é definido pela procedure com motivo. Não imprimir senha/hash nem retornar em listagens.

010 prepara o principal de banco `PokeVaultRuntimeVerificacao WITHOUT LOGIN`, associado ao papel PokeVaultRuntime. Não é login externo. 011 comprovou via EXECUTE AS USER: SELECT permitido, EXECUTE versionado permitido, DML direto e rotina legada negados. Privilégios por encadeamento de propriedade não substituem autorização da API: o ID do ator deve vir da sessão e a conta ativa precisa ser revalidada.

**Pendente para runtime externo:** servidor configurado somente para Windows; nenhum login do projeto já existente foi identificado. Operador precisa fornecer identidade Windows dedicada do processo e seu mapeamento na base, sem sysadmin/db_owner. Os scripts não criam conta Windows, login de servidor, serviço ou alteração de autenticação global. A comprovação do principal de banco não deve ser descrita como aplicação já conectada por conta própria mínima.

## Consultas de verificação e preservação

```powershell
sqlcmd -S '.\SQLEXPRESS' -E -C -I -b -W -w 240 -f 65001 -v DatabaseName='PokeVaultTests' -i 'database/007_verificar.sql'
sqlcmd -S '.\SQLEXPRESS' -E -C -I -b -W -w 240 -f 65001 -v DatabaseName='PokeVaultTests' -i 'database/011_sprint2_verificar.sql'
```

007 reconcilia saldo/estoque/movimentos e totais/itens; as consultas de divergência retornaram zero linhas nas duas bases na preparação. 011 verifica metadados/permissões sem alterar massa: tentativas DML sem linhas foram negadas 229 e chamada versionada sem ator válido chegou à autorização e recusou 51000. Isso não comprova ainda atualização real com token, HTTP/UI ou concorrência/rollback.

012 gera contagens e SHA256 do conteúdo comercial ordenado, sem expor dados/credenciais individuais e excluindo novos tokens Versao e a tabela técnica SchemaVersao. Impressões antes/depois do primeiro upgrade de PokeVault coincidiram nas 11 tabelas anteriores. A checagem integral posterior ao 013 detectou novos cadastros/movimentos e registrou false; a comparação dos prefixos preexistentes limitados às contagens originais confirmou os mesmos digests. O relatório mantém ambos os resultados; não atribui a origem das novas escritas. A nova tabela AlteracaoCarta foi conferida separadamente, vazia na publicação. Para tabela vazia, digest NULL e contagem 0. Não confundir a comparação do estado observado com garantia contra qualquer mudança futura; aplicação/testes ainda precisam de verificação própria.

Testes de falha após escrita são responsabilidade da frente Testes em PokeVaultTests marcada. Uma fixture temporária de restrição/trigger de teste pode provocar a falha em escrita posterior, com guarda de base/marcador e remoção em finally, sem modificar procedures da demonstração. Nunca instalar esse mecanismo em PokeVault, nem adicionar parâmetro de falha/backdoor ao produto.

Massa inicial SQL fictícia: coleção MEW-151, Pikachu 025 NM R$30/2 e SP R$20/3; Charmander 004 NM R$15,50/1. IDs são consultados por chave natural, não assumidos. Entrada inicial tem autor NULL/motivo SEED_SPRINT1, exceção explícita anterior ao bootstrap; operações normais exigem responsável. Depois da liberação, Testes possui sua massa e registra os cenários/resultados reais; Banco não executa mutações concorrentes nela.


