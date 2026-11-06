# PokéVault — roteiro de apresentação local

Roteiro proposto para apresentar a versão web acadêmica. Os passos abaixo não são um registro de testes executados. Consultar o diário e os relatórios de revisão para resultados e limites reais.

## Preparação

- Manter o SQL Server local disponível e dar duplo clique em `Abrir-PokeVault.cmd`, na pasta PIM. O abridor inicia ou reutiliza a aplicação e abre o navegador, sem exigir digitar comandos.
- Acessar http://localhost:5187. Fechar abas de testes para evitar confusão entre contas.
- Ter as credenciais de administrador, atendente e cliente à mão, sem incluí-las em capturas, slides ou no trabalho.
- Conferir saldo, preços e estoque atuais. Os valores podem ter mudado em operações legítimas; não apagar histórico ou restaurar dados para aparentar sucesso.

## Percurso sugerido

| Etapa | Ação pela interface | O que explicar/conferir |
| --- | --- | --- |
| 1 | Abrir o catálogo sem login e abrir uma carta | Vitrine apenas com estoque; detalhe lista idiomas, estados e acabamentos com preços/estoques próprios |
| 2 | Entrar como administrador e cadastrar um cliente fictício de apresentação | Cadastro com saldo zero; definir saldo fictício pela tela e informar motivo. Anotar o saldo antes da compra |
| 3 | Entrar como atendente e abrir a gestão do catálogo | Gestão única com207 cartas; cadastrar/ajustar versões abaixo da imagem. Zero unidades tira a versão da vitrine. Auditoria automática sem motivo digitado; usuários/saldo/estorno permanecem com administrador |
| 4 | Entrar como cliente e montar carrinho com dois ou mais itens | No detalhe, Comprar soma unidades ao carrinho; a sacola ao lado do nome abre o carrinho. Conferir coleção/número/idioma/estado/acabamento e totais. Edição no carrinho define quantidade absoluta; não reserva estoque nem debita saldo |
| 5 | Selecionar somente parte dos itens, revisar total e confirmar | Compra selecionada: conferir saldo, estoque, comprovante/histórico e permanência dos itens não selecionados |
| 6 | Como administrador, localizar a compra e cancelar com motivo | Crédito pelo valor comprado e reposição das quantidades; histórico permanece como cancelado |
| 7 | Voltar à conta do cliente e conferir histórico/saldo | A devolução é única. Se a interface permitir nova chamada de cancelamento, demonstrar que repetir não gera outra devolução; não manipular banco para produzir o resultado |

## Encerramento da apresentação

Explicar que é uma demonstração acadêmica local, com saldo fictício e sem cobrança bancária, entrega ou retirada. A aplicação web e a API usam o SQL Server local. Mobile e melhorias adiadas são etapas posteriores, não funcionalidades entregues nesta apresentação.

Se ocorrer uma recusa por saldo, estoque ou formulário desatualizado, apresentar a mensagem e conferir o estado antes de tentar novamente. Uma recusa coerente é parte do comportamento esperado, não exige esconder o evento. Registrar defeitos encontrados com conta/perfil, passos e resultado, sem expor a senha.

Este roteiro não exige hospedagem externa ou conta Windows de serviço dedicada. A identidade de desenvolvimento local é uma limitação de implantação documentada, não um bloqueio ao aceite acadêmico acordado.
