# 🏦 .NET Bank

> Sistema bancário digital completo com gerenciamento de contas, transações e usuários por perfis de acesso. Possui frontend integrado servido diretamente pela API.
> Este repositório contém o projeto de Desenvolvimento de uma API REST e Frontend Web para um Sistema Bancário Digital, desenvolvido como parte dos meus estudos em C# e .NET Programa Ford <Enter>. O projeto abrange desde a modelagem do banco de dados e criação das migrations, até a implementação de uma API completa com autenticação JWT, operações bancárias (depósito, saque e transferência) e um frontend interativo servido diretamente pela aplicação. Inclui também um painel administrativo para gerenciamento de usuários e visualização do extrato geral do banco.
---

## 📑 Índice

- [Requisitos](#-requisitos)
- [Configuração do Banco de Dados](#️-configuração-do-banco-de-dados)
- [Como Rodar o Projeto](#-como-rodar-o-projeto)
- [Recuperação do Banco (Migrations)](#️-recuperação-do-banco-migrations)
- [Usuário Padrão](#-usuário-padrão-admin)
- [Perfis de Usuário](#-perfis-de-usuário)
- [Autenticação](#-autenticação)
- [Regras de Negócio e Taxas](#-regras-de-negócio-e-taxas)
- [Documentação da API (Swagger)](#-documentação-da-api-swagger)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Problemas Comuns](#-problemas-comuns)

---

## 📋 Requisitos

Antes de rodar o projeto, certifique-se de ter instalado:

| Ferramenta | Versão recomendada | Obrigatório |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 9.0 ou superior | ✅ |
| [SQL Server / LocalDB](https://learn.microsoft.com/pt-br/sql/database-engine/configure-windows/sql-server-express-localdb) | Qualquer versão recente | ✅ |
| [Git](https://git-scm.com/) | Qualquer versão recente | ✅ |
| [Visual Studio 2022+](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/) | Qualquer versão recente | ⬜ Opcional |

---

## ⚙️ Configuração do Banco de Dados

O projeto utiliza **SQL Server LocalDB** por padrão. A connection string já está pré-configurada no `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BancoDigital;Trusted_Connection=True;"
}
```

> ⚠️ Se você usa uma instância diferente do SQL Server, substitua `(localdb)\\mssqllocaldb` pelo endereço do seu servidor e ajuste as credenciais conforme necessário.

---

## 🚀 Como Rodar o Projeto

Siga os passos abaixo na ordem:

### 1. Clone o repositório

```bash
git clone https://github.com/ErickRochaNascimento/desafio_backend_sprint3_ErickNascimento.git
```

### 2. Restaure as dependências

```bash
dotnet restore
```

### 3. Aplique as migrations

```bash
dotnet ef database update
```

> 💡 Esse comando cria todas as tabelas automaticamente na ordem correta. Não é necessário executar nenhum script SQL manualmente.

Caso o comando `dotnet ef` não seja reconhecido, instale a ferramenta globalmente:

```bash
dotnet tool install --global dotnet-ef
```

### 4. Inicie a aplicação

```bash
dotnet run --urls "https://localhost:7200;http://localhost:5200"
```

A porta exata depende do seu `launchSettings.json` (pasta `Properties/`). Por padrão:

```
http://localhost:5118
https://localhost:7142
```

O **frontend é servido automaticamente** — acesse pelo navegador na mesma porta da API.

---

## 🗄️ Recuperação do Banco (Migrations)

As migrations estão versionadas no projeto e representam o histórico completo do banco:

| # | Migration | O que faz |
|---|---|---|
| 1 | `CriacaoInicial` | Cria as tabelas `Usuarios`, `Contas` e `Transacoes` |
| 2 | `AddNomeRemetente` | Adiciona o campo `NomeRemetente` em `Transacoes` |
| 3 | `NomeRemetente` | Ajuste na migration de remetente |
| 4 | `Destinatario` | Adiciona o campo `NomeDestinatario` em `Transacoes` |
| 5 | `AdicionarCpfDataNascimento` | Adiciona os campos `Cpf` e `DataNascimento` em `Usuarios` |

## 👤 Usuário Padrão (Admin)

Ao iniciar a aplicação pela **primeira vez**, um usuário administrador é criado automaticamente:

| Campo | Valor |
|---|---|
| 📧 Email | `admin@bank.com` |
| 🔑 Senha | `admin123` |
| 👑 Perfil | `admin` |

> ⚠️ **Recomendado:** altere a senha imediatamente após o primeiro acesso.

---

## 👥 Perfis de Usuário

O sistema possui dois níveis de acesso:

| Perfil | Descrição |
|---|---|
| 👑 `admin` | Acesso total: visualiza todos os usuários, contas e extrato geral do banco. Pode criar novos administradores. |
| 🙋 `cliente` | Gerencia suas próprias contas bancárias, realiza depósitos, saques e transferências, e consulta seu extrato. |

---

## 🔑 Autenticação

O sistema utiliza **JWT Bearer Token**. Após realizar o login, inclua o token retornado no header de todas as requisições protegidas:

```
Authorization: Bearer {seu_token}
```

> ⏱️ O token expira em **8 horas**.

---

## 💸 Regras de Negócio e Taxas

### Tipos de Conta

| Tipo | Taxa de Saque | Taxa de Transferência |
|---|---|---|
| Corrente | R$ 5,00 fixo | R$ 5,00 fixo |
| Poupança | Isento | Isento |
| Empresarial | 1% sobre o valor | 1% sobre o valor |

### Operações disponíveis

- **Depósito** — Sem taxa, crédito imediato na conta.
- **Saque** — Sujeito à taxa conforme o tipo de conta. O saldo precisa cobrir valor + taxa.
- **Transferência** — Débita valor + taxa na conta de origem e credita o valor líquido na conta destino. Ambos os extratos são registrados com remetente/destinatário.

### Encerramento de Conta

Ao encerrar uma conta com saldo, o sistema realiza um **saque automático** descontando as taxas aplicáveis antes de remover a conta.

---

## 📖 Documentação da API (Swagger)

Com a aplicação rodando em ambiente de desenvolvimento, acesse a documentação interativa:

```
https://localhost:7142/swagger
```

> Disponível apenas em ambiente de desenvolvimento (`ASPNETCORE_ENVIRONMENT=Development`).

---

## 📁 Estrutura do Projeto

```
BancoDigital/
│
├── Controllers/        # Endpoints da API (Auth, Contas, Transações, Admin)
├── Data/               # AppDbContext — configuração do EF Core
├── DTOs/               # Objetos de transferência de dados
├── Migrations/         # Histórico de versões do banco de dados
├── Models/             # Entidades mapeadas (Usuario, Conta, Transacao)
├── Services/           # Regras de negócio (TransacaoService)
├── wwwroot/            # Frontend estático (HTML, CSS, JS e imagens)
│   ├── index.html      # Tela principal do cliente
│   ├── admin.html      # Painel do administrador
│   ├── style.css       # Estilos globais
│   └── utils.js        # Funções utilitárias compartilhadas
│
└── appsettings.json    # Configurações gerais da aplicação
```

---

## ❗ Problemas Comuns

**Erro de conexão com o banco de dados**

Verifique se o SQL Server / LocalDB está em execução e se a connection string no `appsettings.json` está correta.

**`dotnet ef` não é reconhecido**

Instale a ferramenta globalmente:

```bash
dotnet tool install --global dotnet-ef
```

**Porta já em uso**

Abra o arquivo `Properties/launchSettings.json` e altere os campos `applicationUrl`.

**Token inválido ou acesso negado (401/403)**

- Verifique se está enviando o header `Authorization: Bearer {token}` corretamente.
- Confirme se o token não expirou (validade de 8 horas).
- Certifique-se de que o perfil do usuário tem permissão para o endpoint acessado.

**Saldo insuficiente no saque/transferência**

Lembre-se que a taxa é cobrada junto com o valor da operação. O saldo precisa cobrir **valor + taxa**.

---

## 🧑‍💻 Autor

**Erick Rocha Nascimento**  
🔗 [LinkedIn](https://www.linkedin.com/in/erickrochanascimento) | [GitHub](https://github.com/ErickRochaNascimento) | [Portfolio](https://ericknascimento.vercel.app/)
