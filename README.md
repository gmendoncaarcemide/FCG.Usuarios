# 👤 FCG Usuários

Microserviço responsável pelo gerenciamento de usuários no sistema FIAP Cloud Games.Realiza cadastro, autenticação, atualização de perfil e controle de permissões para acesso aos demais serviços da plataforma.

## 🚀 Como rodar o projeto

### ✅ Pré-requisitos

- .NET 8 SDK  
- PostgreSQL (via Supabase)
- EF Core CLI:

```bash
  dotnet tool install --global dotnet-ef
```

### 📦 Restauração de Pacotes
Após clonar o repositório, navegue até a pasta do projeto e execute:
```bash
cd FCG_USUARIOS
dotnet restore
```

### 🛠️ Configuração do Banco de Dados

#### 🔄 **Supabase + PostgreSQL**
O projeto utiliza **Supabase** como provedor de PostgreSQL em nuvem:

**Connection String:**
```bash
Host=db.elcvczlnnzbgcpsbowkg.supabase.co  
Port=5432  
Database=postgres  
Username=postgres  
Password=Fiap@1234
```

#### 🗄️ **Aplicando Migrations**

Para aplicar as migrations no Supabase:
```bash
cd FCG.Usuarios.API
dotnet ef database update --project ../FCG.Usuarios.Infrastructure --startup-project .
```

#### 📝 **Criando Novas Migrations**
```bash
cd FCG.Usuarios.API
dotnet ef migrations add NomeDaMigration --project ../FCG.Usuarios.Infrastructure --startup-project .
```

## 🏗️ Arquitetura
### 📂 Estrutura do Projeto
```
FCG_USUARIOS/
├── FCG.Usuarios.API/             # Camada de API e Controllers
│   ├── Controllers/             # Controllers REST
│   │   └── UsuarioController.cs
│   ├── Program.cs               # Configuração da aplicação
│   └── appsettings.json         # Configurações
├── FCG.Usuarios.Application/     # Regras de negócio e serviços
│   ├── Usuarios/
│   │   ├── Interfaces/          # Interfaces dos serviços
│   │   ├── Services/            # Implementação dos serviços
│   │   └── ViewModels/          # DTOs e ViewModels
│   └── ServiceCollectionExtensions.cs
├── FCG.Usuarios.Domain/          # Entidades e interfaces
│   └── Usuarios/
│       ├── Entities/            # Entidades do domínio
│       └── Interfaces/          # Interfaces dos repositórios
└── FCG.Usuarios.Infrastructure/  # EF Core + Repositórios
    ├── Usuarios/
    │   ├── Repositories/        # Implementação dos repositórios
    │   └── Context/             # DbContext Factory
    ├── Migrations/              # Scripts de migração EF Core
    ├── UsuariosDbContext.cs     # Contexto do EF Core
    └── ServiceCollectionExtensions.cs
```

### 🔧 Tecnologias Utilizadas

- **.NET 8** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados (via Supabase)
- **Serilog** - Logging estruturado
- **Swagger/OpenAPI** - Documentação da API

## 📡 Endpoints da API
### 👤 Usuários

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/usuarios` | Cria um novo usuário. |
| `POST` | `/api/usuarios/login`	| Realiza o Login e retorna um Token JWT. |
| `GET` | `/api/usuarios` | Lista todos os usuários ativos. |
| `GET` | `/api/usuarios/{id:guid}` | Obtém usuário por ID. |
| `GET` | `/api/usuarios/email/{email}` | Obtém usuário por Email. |
| `GET`	| `/api/usuarios/verificar-email/{email}` | Verifica se um email está cadastrado. |
| `PUT`	| `/api/usuarios/{id:guid}` | Atualiza os dados de um usuário. |
| `POST` |	`/api/usuarios/{id:guid}/alterar-senha` | Altera a senha do usuário. |
| `DELETE` | `/api/usuarios/{id:guid}` | Desativa (exclui logicamente) o usuário. |

## 🗄️ Modelo de Dados
### 📊 **Tabela: Usuarios**
- `Id` (UUID)
- `Nome` (String)
- `Email` (String)
- `SenhaHash` (String)
- `DataNascimento` (DateTime)
- `Perfil` (Enum): Admin, Jogador, Desenvolvedor
- `DataCriacao` (DateTime)
- `DataAtualizacao` (DateTime?)


## 🐞 Logs e Monitoramento
### 📝 **Serilog**
- Logs estruturados com Serilog
- Arquivos de log por data em /logs/
- Logs de console para desenvolvimento
- Formato: usuarios-api-YYYY-MM-DD.txt

### 🔍 **Swagger**

- Documentação automática da API
- Interface interativa para testes
- Disponível em /swagger quando em desenvolvimento

## 🚀 Deploy e Produção

### ☁️ **Supabase**
- Banco de dados PostgreSQL gerenciado
- Migrations aplicadas automaticamente na inicialização

