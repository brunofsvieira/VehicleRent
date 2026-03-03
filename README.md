# VehicleRent

Plataforma de gestão de aluguer de veículos construída em **ASP.NET Core MVC + API REST**, com **Entity Framework Core (SQL Server)**, **Redis** e **testes unitários**.

## Objetivo do projeto

O projeto foi desenhado para suportar:

- gestão de veículos
- gestão de clientes
- gestão de contratos de aluguer
- evolução futura para frontend desacoplado (SPA/JS), mantendo o backend atual

## Stack técnica

- **.NET 10**
- **ASP.NET Core MVC** (Views Razor)
- **ASP.NET Core Web API** (endpoints dedicados)
- **Entity Framework Core (Code First)**
- **SQL Server**
- **Redis (IDistributedCache + StackExchangeRedis)**
- **xUnit** para testes unitários

## Configuração de ligação a base de dados

As connection strings estão definidas em:

- `appsettings.json`
- `appsettings.Development.json`

Incluem as ligações para:

- `DefaultConnection` (SQL Server / Entity Framework)
- `Redis` (cache distribuída)

## Arquitetura e decisões

### 1. Separação de responsabilidades

Foi adotada separação por camadas:

- `Controllers` (MVC + API)
- `Services` (regras de negócio)
- `Repositories` (acesso a dados)
- `Models` (entidades, DTOs, view models, validações)

Isto permite:

- manter lógica de negócio fora dos controladores
- testar o domínio com facilidade
- trocar UI sem alterar regras de negócio

### 2. MVC + API em paralelo

Cada módulo principal tem:

- controller para **Views** (experiência server-side atual)
- controller para **API** (integração futura com frontend JS/mobile)

Benefícios:

- aplicação utilizável hoje com Razor
- backend já preparado para migração gradual para SPA (React/Vue/Angular) sem reescrever domínio

### 3. Validação em duas camadas

- **Frontend/ViewModel**: validação de UX imediata (mensagens no formulário)
- **Backend/Service/Entity**: validação final e obrigatória (source of truth)

Mesmo que o frontend seja contornado, o backend mantém integridade.

### 4. Soft delete

Foi implementado `soft delete` na `BaseEntity`:

- campo `Deleted` (bool)
- método `MarkDeleted()`
- filtros globais EF (`HasQueryFilter`) para esconder registos apagados

O `DeleteAsync` já **não remove fisicamente**: marca `Deleted = true`.

Benefícios:

- preserva histórico
- reduz risco de perda de dados
- facilita auditoria e recuperação futura

### 5. Regras de remoção protegidas (frontend + backend)

Bloqueios implementados:

- não permite apagar **veículos alugados**
- não permite apagar **clientes com aluguer em curso**
- não permite apagar **contratos em curso**

A proteção existe em dois níveis:

- **frontend**: botão `Apagar` desativado quando aplicável
- **backend**: validação no serviço e erro de negócio se tentarem forçar

## Regras de negócio implementadas

### Veículos

- matrícula obrigatória, normalizada para maiúsculas
- matrícula única
- formatos aceites:
  - `11-AA-11`
  - `AA-11-11`
  - `11-11-AA`
  - `AA-11-AA`
- estado dinâmico na listagem: `Alugado` / `Disponível` (com base em contratos ativos)

### Clientes

- campos obrigatórios: nome, email, telefone, carta de condução
- email único
- carta de condução única
- telefone validado no formato `+<codigo-pais><9digitos>`

### Contratos

- cliente e veículo obrigatórios
- data início e fim obrigatórias
- início >= data atual
- fim > início
- não permite sobreposição de contratos para o mesmo veículo
- estado na listagem: `Em curso` / `Terminado`

## Redis: onde e porquê

Redis foi aplicado via `IDistributedCache` para caches de leitura frequente:

- listas de filtros (clientes/veículos)
- dados usados em ecrãs de listagem com repetição de consultas

Objetivo:

- reduzir carga no SQL Server
- melhorar tempo de resposta em páginas de listagem/filtros

As chaves de cache estão centralizadas em:

- `Infrastructure/CacheKeys.cs`

## Testes unitários

Foi feita cobertura abrangente de:

- entidades (validação de regras)
- serviços (fluxos de negócio, erros e exceções)
- repositories (queries/filtros/paginação/soft delete)
- controllers MVC e API (resultados HTTP, tratamento de erros)

## Migrations e base de dados

A abordagem é Code First.

Para atualizar a base local:

```bash
dotnet ef database update
```

A migration de soft delete já foi criada.

## Estrutura principal

- `Controllers/` -> MVC e API
- `Services/` -> regras de negócio
- `Repositories/` -> acesso a dados
- `Models/Entities/` -> entidades de domínio
- `Models/DTOs/` -> contratos da API
- `Models/ViewModels/` -> modelos para views
- `Data/` -> `ApplicationDbContext`
- `Infrastructure/` -> utilitários (cache, mensagens)
- `VehicleRent.Tests/` -> testes unitários

## Evolução futura recomendada

- adicionar autenticação/autorização (roles)
- adicionar observabilidade (logs estruturados + métricas)
- adicionar paginação/sorting mais avançados nos endpoints API
- criar frontend SPA consumindo os endpoints já existentes
- adicionar testes de integração (API + EF + SQL/containers)
