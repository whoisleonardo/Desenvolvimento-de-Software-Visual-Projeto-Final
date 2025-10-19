# 📚 MediaShelf API

Uma API completa para gerenciamento de catálogo de mídias (filmes, séries, jogos) com sistema de avaliações e usuários.

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-green)
![SQLite](https://img.shields.io/badge/SQLite-Database-lightgrey)
![Minimal API](https://img.shields.io/badge/API-Minimal-orange)

## 🎯 **Sobre o Projeto**

O MediaShelf é um sistema de catalogação de mídias que permite aos usuários:
- 📝 Cadastrar e gerenciar suas mídias favoritas
- ⭐ Avaliar mídias com notas de 0 a 5
- 💬 Deixar comentários e reviews
- 📊 Visualizar médias de avaliações
- 👥 Sistema completo de usuários com autenticação

## 🏗️ **Arquitetura**

### **Entidades Principais**
- **User**: Gerenciamento de usuários
- **Media**: Catálogo de mídias (filmes, séries, etc.)
- **Review**: Sistema de avaliações e comentários

### **Relacionamentos**
```
User (1) ←→ (N) Media    # Um usuário pode cadastrar várias mídias
User (1) ←→ (N) Review   # Um usuário pode fazer várias reviews
Media (1) ←→ (N) Review  # Uma mídia pode ter várias reviews
```

## 🛠️ **Tecnologias Utilizadas**

- **Backend**: C# 8.0 com ASP.NET Core (Minimal API)
- **ORM**: Entity Framework Core
- **Banco de Dados**: SQLite
- **Validação**: Data Annotations
- **Arquitetura**: Clean Architecture com separação de responsabilidades

## 📋 **Funcionalidades**

### 👥 **Usuários**
- ✅ Cadastro de usuários
- ✅ Sistema de login
- ✅ Atualização de perfil (PUT/PATCH)
- ✅ Exclusão de usuários

### 📺 **Mídias**
- ✅ CRUD completo de mídias
- ✅ Relacionamento com usuários proprietários
- ✅ Cálculo automático de médias de avaliações
- ✅ Contagem total de reviews

### ⭐ **Reviews**
- ✅ Sistema de avaliações (0 a 5 estrelas)
- ✅ Comentários (máx. 1000 caracteres)
- ✅ Prevenção de reviews duplicadas por usuário
- ✅ Consultas por mídia ou usuário
- ✅ Atualização e exclusão de reviews

## 🚀 **Como Executar**

### **Pré-requisitos**
- .NET 8.0 SDK
- Visual Studio Code ou Visual Studio
- REST Client (extensão VS Code) - opcional

### **Passos**

1. **Clone o repositório**
   ```bash
   git clone https://github.com/seu-usuario/MediaShelf.git
   cd MediaShelf
   ```

2. **Instale as dependências**
   ```bash
   dotnet restore
   ```

3. **Execute as migrações**
   ```bash
   dotnet ef database update
   ```

4. **Execute a aplicação**
   ```bash
   dotnet run
   ```

5. **Acesse a API**
   - URL Base: `http://localhost:5116`
   - Use o arquivo `teste.http` para testar todos os endpoints

## 📚 **Documentação da API**

### **Base URL**: `http://localhost:5116`

### 👥 **Endpoints de Usuários**

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/users/listar` | Lista todos os usuários |
| `POST` | `/users/registrar` | Cadastra novo usuário |
| `PUT` | `/users/atualizar/{id}` | Atualiza usuário completo |
| `PATCH` | `/users/alterar/{id}` | Atualização parcial |
| `DELETE` | `/users/delete/{id}` | Remove usuário |
| `POST` | `/users/login` | Autenticação de usuário |

### 📺 **Endpoints de Mídias**

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/media/listar` | Lista todas as mídias |
| `GET` | `/media/pesquisar/{id}` | Busca mídia por ID |
| `POST` | `/media/criar` | Cadastra nova mídia |
| `PUT` | `/media/atualizar/{id}` | Atualiza mídia |
| `DELETE` | `/media/remover/{id}` | Remove mídia |

### ⭐ **Endpoints de Reviews**

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/reviews/listar` | Lista todas as reviews |
| `GET` | `/reviews/pesquisar/{id}` | Busca review por ID |
| `GET` | `/reviews/media/{mediaId}` | Reviews de uma mídia |
| `GET` | `/reviews/usuario/{userId}` | Reviews de um usuário |
| `POST` | `/reviews/criar` | Cria nova review |
| `PUT` | `/reviews/atualizar/{id}` | Atualiza review |
| `DELETE` | `/reviews/remover/{id}` | Remove review |

## 🧪 **Testes**

### **Arquivo de Testes**
O projeto inclui um arquivo `teste.http` com mais de 60 testes organizados:

- ✅ Testes funcionais (casos de sucesso)
- ✅ Testes de validação (casos de erro)
- ✅ Testes de relacionamentos
- ✅ Testes de regras de negócio

### **Como Executar os Testes**
1. Instale a extensão **REST Client** no VS Code
2. Abra o arquivo `teste.http`
3. Execute os testes clicando em "Send Request"

## 📊 **Exemplos de Uso**

### **Cadastrar Usuário**
```http
POST /users/registrar
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao@email.com",
  "password": "senha123"
}
```

### **Cadastrar Mídia**
```http
POST /media/criar
Content-Type: application/json

{
  "title": "Vingadores: Ultimato",
  "description": "Filme épico de super-heróis",
  "userId": 1
}
```

### **Criar Review**
```http
POST /reviews/criar
Content-Type: application/json

{
  "rating": 4.5,
  "comment": "Filme incrível!",
  "userId": 1,
  "mediaId": 1
}
```

## 🔒 **Validações e Regras de Negócio**

### **Usuários**
- ✅ Email único e formato válido
- ✅ Nome obrigatório
- ✅ Senha obrigatória

### **Mídias**
- ✅ Título e descrição obrigatórios
- ✅ Deve pertencer a um usuário válido

### **Reviews**
- ✅ Rating entre 0 e 5
- ✅ Comentário máximo 1000 caracteres
- ✅ Um usuário só pode avaliar uma mídia uma vez
- ✅ Usuário e mídia devem existir

## 📁 **Estrutura do Projeto**

```
MediaShelf/
├── models/
│   ├── User.cs              # Entidade usuário
│   ├── Media.cs             # Entidade mídia
│   ├── Review.cs            # Entidade review
│   ├── AppDataContext.cs    # Contexto do banco
│   └── LoginRequest.cs      # DTO para login
├── Migrations/              # Migrações do EF
├── MediaShelf.db           # Banco SQLite
├── Program.cs              # Configuração da API
├── teste.http              # Arquivo de testes
└── README.md               # Documentação
```

## 🎯 **Diferenciais do Projeto**

- 🔄 **CRUD Completo**: Todas as entidades têm operações completas
- 🔗 **Relacionamentos Funcionais**: Dados interconectados e úteis
- 📊 **Cálculos Automáticos**: Médias de avaliações em tempo real
- 🛡️ **Validações Robustas**: Prevenção de dados inválidos
- 🚫 **Regras de Negócio**: Prevenção de duplicatas e inconsistências
- 🧪 **Testabilidade**: Suite completa de testes automatizados
- 📚 **Documentação**: Código bem documentado e README completo

## 👨‍💻 **Autor**

Desenvolvido como projeto acadêmico para demonstração de:
- Desenvolvimento de APIs RESTful
- Entity Framework Core
- Relacionamentos de banco de dados
- Arquitetura limpa e organizadas
- Testes automatizados

## 📄 **Licença**

Este projeto é desenvolvido para fins educacionais.

---

⭐ **MediaShelf API** - Sistema completo de catalogação de mídias com avaliações
