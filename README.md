# PokéVault — PIM IV

Projeto acadêmico de uma loja fictícia de cartas Pokémon TCG: aplicação web ASP.NET Core MVC/Razor, API REST e SQL Server. A demonstração usa saldo fictício e uma coleção de referência, Pokémon 151.

## Para o avaliador

- [Apêndice A no Google Docs](https://docs.google.com/document/d/1If7Q7HWr6PTF9KlO_313HMaFrLKSEdKjfVNZ6HjBGc8/edit).
- [Rascunho do Apêndice A em Markdown](docs/academico/APENDICE-A-rascunho-20261106.md).
- [Registros de sprints acrescentados ao final do apêndice](docs/academico/REGISTROS-SPRINTS-APENDICE-A.md).
- [Diário integral de sprints](docs/diario-sprints.md): acordos, tarefas por frente, problemas, resultados e limites.
- [Decisões](docs/DECISOES.md), [planejamento](docs/planejamento.md) e [roteiro de demonstração](docs/ROTEIRO-DEMONSTRACAO-WEB.md).

Este repositório disponibiliza uma fotografia do projeto para consulta acadêmica. O primeiro commit de publicação foi feito em novembro de 2026; ele não reconstrói um histórico Git de setembro ou outubro. As datas anteriores pertencem aos registros preservados, com seus próprios limites. O PIM principal no Drive é mantido separadamente.

## Entrega implementada

- Catálogo e detalhes com ofertas por conservação, idioma e acabamento; preço e estoque independentes.
- Autenticação de cliente, atendente e administrador; cadastro público de cliente e alteração da própria senha.
- Carrinho persistente e adição cumulativa; compra de linhas selecionadas com confirmação de preço.
- Compra transacional, chave UUID para reenvio idempotente, histórico e estorno administrativo integral.
- Gestão de contas, saldo fictício, catálogo e estoque, com controle de concorrência e auditoria.
- Integração TCGdex específica de sv03.5: 207 referências, dados/imagens locais e preços externos informativos, sem converter automaticamente para reais ou criar estoque.

Mobile/Capacitor não foi demonstrado. Nuvem, pagamentos reais, logística, buyback e relatórios gerenciais não integram a entrega web documentada. Não há alegação de certificação WCAG, segurança de produção ou ensaio de carga.

## Organização

| Caminho | Conteúdo |
| --- | --- |
| src/PokeVault.Web/ | Controllers, serviços, modelos, infraestrutura, páginas Razor e recursos locais |
| database/ | Esquema, procedures, atualizações numeradas e verificações |
| tests/ | Harnesses SQL/HTTP/MVC e fixtures |
| docs/ | Contratos, planejamento, diário, relatórios e evidências |
| src/evidencias/ e database/evidencias/ | Registros das respectivas frentes |
| dev.ps1 e Abrir-PokeVault.* | Compilação, execução e abertura local |

Relatórios históricos conservam decisões de versões anteriores. Para o estado mais recente, consultar as entregas de [fluxo](docs/entrega-fluxo151.md), [contas](docs/entrega-contas.md) e os registros WEB151 do diário. O [README anterior](docs/README-historico.md) foi preservado como histórico e contém descrições superadas.

## Executar em uma instalação nova

Ambiente registrado: Windows, SDK .NET 10.0.400 conforme global.json, SQL Server e sqlcmd. A configuração de exemplo usa autenticação Windows integrada e a instância local .\\SQLEXPRESS, sem senha no arquivo versionado. O bootstrap usa DPAPI do Windows; suas credenciais são geradas no computador do avaliador.

1. Clonar ou baixar este repositório.
2. Conferir a instância SQL e os requisitos em [database/README.md](database/README.md). Se necessário, configurar ConnectionStrings__PokeVault/ConnectionStrings__Preparacao por variáveis locais ou appsettings.Local.json, que é ignorado pelo Git.
3. Em terminal na raiz, restaurar e compilar:

~~~powershell
.\dev.ps1 -Acao Restore
.\dev.ps1 -Acao Build
~~~

4. Para criar somente uma base nova do projeto, após conferir a instância:

~~~powershell
.\database\Preparar.ps1 -Servidor '.\SQLEXPRESS' -Banco PokeVault -Modo Inicializar
~~~

O preparador recusa bases existentes sem a identificação do projeto. Para uma base identificada já existente, seguir o modo Atualizar descrito no arquivo. Não executar todos os scripts indiscriminadamente: 014–018 pertencem a uma rodada suspensa e não integram a preparação final.

5. Provisionar as contas demonstrativas na instalação nova e consultar as credenciais em terminal local interativo:

~~~powershell
.\dev.ps1 -Acao Bootstrap -Base PokeVault
.\dev.ps1 -Acao Credenciais -Base PokeVault
~~~

As senhas são geradas aleatoriamente e protegidas por DPAPI; não estão publicadas. A reexecução não redefine automaticamente contas ou senhas existentes. O cadastro público cria apenas clientes com saldo zero.

6. Iniciar:

~~~powershell
.\dev.ps1 -Acao Run -Base PokeVault
~~~

Abrir http://localhost:5187. Depois da preparação, também é possível usar Abrir-PokeVault.cmd. O abridor não cria banco nem executa bootstrap. A conexão TCGdex para a primeira importação usa a API externa; o cache baixado no computador original não acompanha o repositório.

## Testes e rastreabilidade

Os resultados publicados são registros históricos; publicar o repositório não significa reexecutá-los.

| Evidência | Conjunto | Resultado registrado |
| --- | --- | --- |
| E1 | Diário e decisões | Registro documental |
| E2 | Integração TCGdex (live-final + offline-final) | 25/25 |
| E3 | Regressão TCGdex | 45/45, produto D226 |
| E4 | Fluxo/apresentação HTML/HTTP | 10/10, produto FDCE |
| E5 | Cadastro e senha | 10/10, produto 64F1 |
| E6 | Regressão final de contas | 45/45, produto 64F1 |
| E7 | Preservação/schema 12 | 12 tabelas preservadas no registro |
| E8 | Implementação | Código, scripts e harnesses |

Os caminhos e SHA256 integrais estão no [Apêndice A](docs/academico/APENDICE-A-rascunho-20261106.md). E3 e E6 são campanhas distintas do conjunto de regressão; suas contagens não representam 90 cenários independentes.

Para nova execução, usar PokeVaultTests separada da demonstração, ler [tests/Sprint2/README.md](tests/Sprint2/README.md), [tests/Tcgdex151/README.md](tests/Tcgdex151/README.md) e o wrapper tests/Executar-Sprint2.ps1. Preservar nova pasta de resultados e manifesto; não reaplicar DDL enquanto testes mutantes estiverem em execução. A publicação não modifica a aplicação ou o banco demonstrativo original.

## Conteúdo excluído da publicação

Credenciais e chaves locais, DPAPI, configurações privadas, banco físico, caches de imagens/JSON de execução, dependências restauradas, binários, temporários e a implementação WIP suspensa. Permanecem o código da versão ativa e os registros necessários para examinar o desenvolvimento. Os dados de testes publicados são massa sintética identificada nos relatórios.

Bibliotecas de terceiros conservam seus arquivos de licença. Pokémon e os dados/imagens da TCGdex pertencem aos respectivos titulares; a aplicação é um exercício acadêmico sem operação comercial real.

