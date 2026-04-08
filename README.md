# Demandas da Comunidade - API

## 1. O Problema Real
Em muitas comunidades e bairros locais, os moradores não possuem um canal direto, centralizado e transparente para relatar problemas de infraestrutura urbana (como buracos na via, falta de iluminação, vazamentos, calçadas quebradas, etc.) aos órgãos competentes ou associações de bairro. Isso gera perda de informações, duplicidade de reclamações e demora na resolução dos problemas.

## 2. A Proposta da Solução
Desenvolver uma API REST simples e ágil que sirva como um registro central (livro de ocorrências digital) para essas demandas. O sistema permite que os problemas sejam relatados e listados publicamente, criando um mapa claro do que precisa ser resolvido na comunidade e facilitando o acompanhamento das requisições.

## 3. Público-Alvo
- **Moradores locais:** Cidadãos que precisam de uma forma fácil para relatar problemas no bairro.
- **Associações de Moradores / Líderes Comunitários:** Entidades que organizam e encaminham as demandas para a prefeitura.
- **Órgãos Públicos Municipais:** Setores de zeladoria que desejam monitorar os relatos de forma centralizada.

## 4. Funcionalidades Principais
- **Registro de Ocorrências (`POST /demands`):** Permite cadastrar uma nova demanda informando um título claro e uma descrição detalhada.
- **Listagem Pública (`GET /demands`):** Retorna o histórico de todas as demandas cadastradas na comunidade.
- **Geração de Metadados:** Criação automática de um identificador único seguro (UUID) e registro de data/hora (`createdAt`) para cada nova demanda inserida.

## 5. Tecnologias Utilizadas
- **Framework:** .NET 10 SDK
- **Web API:** Minimal APIs (C#)
- **Persistência de Dados:** Armazenamento local em arquivo JSON (`demands.json`)
- **Testes Automatizados:** xUnit integrado com `Microsoft.AspNetCore.Mvc.Testing`
- **Padronização e Qualidade (Lint):** Regras nativas via `.editorconfig`
- **Pipeline de CI/CD:** GitHub Actions (Windows Server)

## 6. Instruções de Instalação
Certifique-se de ter o [.NET 10 SDK](https://dotnet.microsoft.com/) instalado em sua máquina. 
Clone o repositório para o seu ambiente local:
```bash
git clone https://github.com/petercodez/DemandasComunidade
cd DemandasComunidade