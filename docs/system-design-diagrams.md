# Enterprise CRM - System Design Diagrams

## 🏗️ System Architecture Overview

This document provides visual representations of the Enterprise CRM system architecture, component relationships, and data flow patterns.

## 📊 High-Level System Architecture

```mermaid
graph TB
    subgraph "Client Layer"
        UI1[Blazor Server UI]
        UI2[Blazor WebAssembly]
        UI3[Mobile App]
    end
    
    subgraph "API Gateway"
        LB[Load Balancer]
        GW[API Gateway]
    end
    
    subgraph "Application Layer"
        API[Web API]
        AUTH[Authentication Service]
        CACHE[Redis Cache]
    end
    
    subgraph "Business Logic Layer"
        CORE[Core Domain]
        APP[Application Services]
        VALID[Validation Layer]
    end
    
    subgraph "Data Layer"
        REPO[Repository Pattern]
        UOW[Unit of Work]
        EF[Entity Framework]
    end
    
    subgraph "Database Layer"
        DB[(SQL Server)]
        BACKUP[(Backup DB)]
    end
    
    subgraph "External Services"
        EMAIL[Email Service]
        SMS[SMS Service]
        FILE[File Storage]
    end
    
    UI1 --> LB
    UI2 --> LB
    UI3 --> LB
    LB --> GW
    GW --> API
    API --> AUTH
    API --> CACHE
    API --> APP
    APP --> CORE
    APP --> VALID
    APP --> REPO
    REPO --> UOW
    UOW --> EF
    EF --> DB
    DB --> BACKUP
    API --> EMAIL
    API --> SMS
    API --> FILE
```

## 🔄 Clean Architecture Layers

```mermaid
graph TD
    subgraph "Presentation Layer"
        WEB[Web API Controllers]
        BLZ[Blazor Components]
        MVC[MVC Controllers]
    end
    
    subgraph "Application Layer"
        SVC[Application Services]
        DTO[DTOs]
        VAL[Validators]
        MAP[Mappers]
    end
    
    subgraph "Domain Layer"
        ENT[Entities]
        INT[Interfaces]
        ENUM[Enums]
        VO[Value Objects]
    end
    
    subgraph "Infrastructure Layer"
        REPO[Repositories]
        EF[Entity Framework]
        EXT[External Services]
        LOG[Logging]
    end
    
    WEB --> SVC
    BLZ --> SVC
    MVC --> SVC
    SVC --> ENT
    SVC --> INT
    SVC --> VAL
    SVC --> MAP
    SVC --> REPO
    REPO --> EF
    REPO --> EXT
    REPO --> LOG
```

## 🗄️ Database Entity Relationship Diagram

```mermaid
erDiagram
    USERS {
        int Id PK
        string FirstName
        string LastName
        string Email
        string Username
        string PasswordHash
        int Role
        int Status
        datetime LastLoginDate
        datetime CreatedAt
        string CreatedBy
    }
    
    CUSTOMERS {
        int Id PK
        string CompanyName
        string FirstName
        string LastName
        string Email
        string Phone
        string Address
        string City
        string State
        string PostalCode
        string Country
        string Industry
        int Type
        int Status
        datetime CreatedAt
        string CreatedBy
    }
    
    CONTACTS {
        int Id PK
        int CustomerId FK
        string FirstName
        string LastName
        string Email
        string Phone
        string JobTitle
        string Department
        int Role
        boolean IsPrimary
        datetime CreatedAt
        string CreatedBy
    }
    
    LEADS {
        int Id PK
        string CompanyName
        string FirstName
        string LastName
        string Email
        string Phone
        string JobTitle
        string Industry
        int Source
        int Status
        int Priority
        decimal EstimatedValue
        datetime ExpectedCloseDate
        int AssignedToUserId FK
        int CustomerId FK
        datetime CreatedAt
        string CreatedBy
    }
    
    OPPORTUNITIES {
        int Id PK
        int CustomerId FK
        string Name
        int Stage
        decimal Amount
        decimal Probability
        datetime ExpectedCloseDate
        datetime ActualCloseDate
        int Status
        string Product
        string Description
        int AssignedToUserId FK
        datetime CreatedAt
        string CreatedBy
    }
    
    TASKS {
        int Id PK
        string Title
        string Description
        int Type
        int Priority
        int Status
        datetime DueDate
        datetime CompletedDate
        int AssignedToUserId FK
        int CustomerId FK
        int LeadId FK
        int OpportunityId FK
        datetime CreatedAt
        string CreatedBy
    }
    
    USERS ||--o{ LEADS : "assigned to"
    USERS ||--o{ OPPORTUNITIES : "assigned to"
    USERS ||--o{ TASKS : "assigned to"
    CUSTOMERS ||--o{ CONTACTS : "has"
    CUSTOMERS ||--o{ LEADS : "converted from"
    CUSTOMERS ||--o{ OPPORTUNITIES : "has"
    CUSTOMERS ||--o{ TASKS : "related to"
    LEADS ||--o{ TASKS : "has"
    OPPORTUNITIES ||--o{ TASKS : "has"
```

## 🔄 Data Flow Architecture

```mermaid
sequenceDiagram
    participant UI as Blazor UI
    participant API as Web API
    participant AUTH as Auth Service
    participant SVC as Application Service
    participant REPO as Repository
    participant DB as Database
    participant CACHE as Redis Cache
    
    UI->>API: HTTP Request
    API->>AUTH: Validate Token
    AUTH-->>API: Token Valid
    API->>CACHE: Check Cache
    alt Cache Hit
        CACHE-->>API: Return Cached Data
    else Cache Miss
        API->>SVC: Process Request
        SVC->>REPO: Get Data
        REPO->>DB: Query Database
        DB-->>REPO: Return Data
        REPO-->>SVC: Return Entities
        SVC-->>API: Return DTOs
        API->>CACHE: Store in Cache
    end
    API-->>UI: Return Response
```

## 🚀 Deployment Architecture

```mermaid
graph TB
    subgraph "Azure Cloud"
        subgraph "Frontend"
            CDN[Azure CDN]
            STATIC[Static Web Apps]
        end
        
        subgraph "Application Tier"
            LB[Load Balancer]
            APP1[App Service 1]
            APP2[App Service 2]
            APP3[App Service 3]
        end
        
        subgraph "Data Tier"
            SQL[Azure SQL Database]
            REDIS[Azure Redis Cache]
            STORAGE[Blob Storage]
        end
        
        subgraph "Monitoring"
            INSIGHTS[Application Insights]
            LOGS[Log Analytics]
            ALERTS[Alert Rules]
        end
    end
    
    subgraph "External"
        USERS[End Users]
        DEV[Developers]
        ADMIN[Administrators]
    end
    
    USERS --> CDN
    DEV --> APP1
    ADMIN --> APP1
    CDN --> STATIC
    STATIC --> LB
    LB --> APP1
    LB --> APP2
    LB --> APP3
    APP1 --> SQL
    APP2 --> SQL
    APP3 --> SQL
    APP1 --> REDIS
    APP2 --> REDIS
    APP3 --> REDIS
    APP1 --> STORAGE
    APP2 --> STORAGE
    APP3 --> STORAGE
    APP1 --> INSIGHTS
    APP2 --> INSIGHTS
    APP3 --> INSIGHTS
    INSIGHTS --> LOGS
    LOGS --> ALERTS
```

## 🔐 Security Architecture

```mermaid
graph TB
    subgraph "Security Layers"
        subgraph "Network Security"
            FW[Firewall]
            WAF[Web Application Firewall]
            VPN[VPN Gateway]
        end
        
        subgraph "Application Security"
            JWT[JWT Authentication]
            RBAC[Role-Based Access Control]
            VALID[Input Validation]
            ENCRYPT[Data Encryption]
        end
        
        subgraph "Data Security"
            TLS[TLS/SSL]
            BACKUP[Encrypted Backups]
            AUDIT[Audit Logging]
            MASK[Data Masking]
        end
    end
    
    subgraph "External Threats"
        ATTACK[Malicious Requests]
        BREACH[Data Breach Attempts]
        INJECT[SQL Injection]
    end
    
    ATTACK --> FW
    BREACH --> WAF
    INJECT --> VALID
    FW --> JWT
    WAF --> RBAC
    VPN --> ENCRYPT
    JWT --> TLS
    RBAC --> BACKUP
    VALID --> AUDIT
    ENCRYPT --> MASK
```

## 📊 Microservices Architecture (Future)

```mermaid
graph TB
    subgraph "API Gateway"
        GW[Kong/Azure API Management]
    end
    
    subgraph "Core Services"
        CUST[Customer Service]
        LEAD[Lead Service]
        OPP[Opportunity Service]
        TASK[Task Service]
        USER[User Service]
    end
    
    subgraph "Supporting Services"
        AUTH[Auth Service]
        NOTIFY[Notification Service]
        REPORT[Reporting Service]
        AUDIT[Audit Service]
    end
    
    subgraph "Data Services"
        CUST_DB[(Customer DB)]
        LEAD_DB[(Lead DB)]
        OPP_DB[(Opportunity DB)]
        TASK_DB[(Task DB)]
        USER_DB[(User DB)]
    end
    
    subgraph "Message Bus"
        BUS[Azure Service Bus]
    end
    
    GW --> CUST
    GW --> LEAD
    GW --> OPP
    GW --> TASK
    GW --> USER
    CUST --> AUTH
    LEAD --> AUTH
    OPP --> AUTH
    TASK --> AUTH
    USER --> AUTH
    CUST --> CUST_DB
    LEAD --> LEAD_DB
    OPP --> OPP_DB
    TASK --> TASK_DB
    USER --> USER_DB
    CUST --> BUS
    LEAD --> BUS
    OPP --> BUS
    TASK --> BUS
    BUS --> NOTIFY
    BUS --> REPORT
    BUS --> AUDIT
```

## 🔄 CQRS Pattern Implementation

```mermaid
graph LR
    subgraph "Command Side"
        CMD[Commands]
        CMD_H[Command Handlers]
        CMD_DB[(Command Database)]
    end
    
    subgraph "Query Side"
        QRY[Queries]
        QRY_H[Query Handlers]
        QRY_DB[(Query Database)]
    end
    
    subgraph "Event Bus"
        EVT[Domain Events]
        BUS[Event Bus]
    end
    
    CMD --> CMD_H
    CMD_H --> CMD_DB
    CMD_H --> EVT
    EVT --> BUS
    BUS --> QRY_H
    QRY --> QRY_H
    QRY_H --> QRY_DB
```

## 📈 Performance Architecture

```mermaid
graph TD
    subgraph "Caching Strategy"
        L1[L1 Cache - In-Memory]
        L2[L2 Cache - Redis]
        L3[L3 Cache - CDN]
    end
    
    subgraph "Database Optimization"
        INDEX[Database Indexes]
        PART[Table Partitioning]
        REPLICA[Read Replicas]
    end
    
    subgraph "Application Optimization"
        POOL[Connection Pooling]
        ASYNC[Async/Await]
        COMPRESS[Response Compression]
    end
    
    subgraph "Monitoring"
        METRICS[Performance Metrics]
        PROFILER[Application Profiler]
        ALERTS[Performance Alerts]
    end
    
    L1 --> L2
    L2 --> L3
    INDEX --> PART
    PART --> REPLICA
    POOL --> ASYNC
    ASYNC --> COMPRESS
    METRICS --> PROFILER
    PROFILER --> ALERTS
```

## 🧪 Testing Architecture

```mermaid
graph TB
    subgraph "Test Pyramid"
        E2E[E2E Tests]
        INT[Integration Tests]
        UNIT[Unit Tests]
    end
    
    subgraph "Test Infrastructure"
        MOCK[Mock Services]
        TEST_DB[(Test Database)]
        FIXTURE[Test Fixtures]
    end
    
    subgraph "Test Execution"
        CI[CI Pipeline]
        PARALLEL[Parallel Execution]
        COVERAGE[Coverage Reports]
    end
    
    E2E --> MOCK
    INT --> TEST_DB
    UNIT --> FIXTURE
    MOCK --> CI
    TEST_DB --> PARALLEL
    FIXTURE --> COVERAGE
```

## 🔧 DevOps Pipeline Architecture

```mermaid
graph LR
    subgraph "Development"
        DEV[Developer]
        IDE[IDE/VS Code]
        GIT[Git Repository]
    end
    
    subgraph "CI/CD Pipeline"
        BUILD[Build]
        TEST[Test]
        SCAN[Security Scan]
        DEPLOY[Deploy]
    end
    
    subgraph "Environments"
        DEV_ENV[Development]
        STAGE[Staging]
        PROD[Production]
    end
    
    subgraph "Monitoring"
        LOGS[Logs]
        METRICS[Metrics]
        ALERTS[Alerts]
    end
    
    DEV --> IDE
    IDE --> GIT
    GIT --> BUILD
    BUILD --> TEST
    TEST --> SCAN
    SCAN --> DEPLOY
    DEPLOY --> DEV_ENV
    DEPLOY --> STAGE
    DEPLOY --> PROD
    PROD --> LOGS
    LOGS --> METRICS
    METRICS --> ALERTS
```

## 📋 Diagram Usage Guidelines

### **When to Use Each Diagram:**

- **High-Level Architecture:** Stakeholder presentations, system overview
- **Clean Architecture:** Developer onboarding, architectural decisions
- **Database ERD:** Database design, data modeling discussions
- **Data Flow:** Understanding request/response patterns
- **Deployment:** Infrastructure planning, DevOps discussions
- **Security:** Security reviews, compliance documentation
- **Microservices:** Future architecture planning
- **CQRS:** Advanced pattern implementation
- **Performance:** Optimization planning, bottleneck identification
- **Testing:** Test strategy, quality assurance planning
- **DevOps:** CI/CD pipeline design, automation planning

### **Tools for Creating Diagrams:**

- **Mermaid:** Used in this document (GitHub native support)
- **Draw.io:** Free online diagramming tool
- **Lucidchart:** Professional diagramming platform
- **Visio:** Microsoft's diagramming tool
- **PlantUML:** Text-based diagram generation

### **Best Practices:**

- **Keep diagrams simple** and focused on specific concerns
- **Use consistent notation** across all diagrams
- **Update diagrams** when architecture changes
- **Include legends** for complex diagrams
- **Version control** diagram source files

This comprehensive set of system design diagrams provides visual documentation for all aspects of the Enterprise CRM system architecture, from high-level overview to detailed implementation patterns.
