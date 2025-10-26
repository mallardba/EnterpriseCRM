# Enterprise CRM - Deployment Guide

## 🚀 Deployment Overview

This guide covers various deployment options for the Enterprise CRM system, from local development to production environments. The application is designed to be cloud-native and containerized for maximum flexibility and scalability.

## 🏗️ Deployment Architecture

### **Production Architecture**
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Load Balancer  │    │   Web Servers   │    │   Database      │
│   (Azure LB)     │────│   (App Service) │────│   (SQL Server)  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   CDN           │    │   Redis Cache   │    │   Blob Storage  │
│   (Azure CDN)   │    │   (Redis)       │    │   (Files)       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🌐 Deployment Options

### **1. Azure App Service**
**Best for:** Managed hosting with minimal configuration

**Benefits:**
- **Managed Service:** No server management required
- **Auto-scaling:** Automatic scaling based on demand
- **SSL Certificates:** Built-in SSL certificate management
- **CI/CD Integration:** Direct deployment from Git
- **Monitoring:** Built-in application insights

**Deployment Steps:**
```bash
# 1. Create App Service
az webapp create --resource-group myResourceGroup --plan myAppServicePlan --name myWebApp --runtime "DOTNET|8.0"

# 2. Configure connection string
az webapp config connection-string set --resource-group myResourceGroup --name myWebApp --connection-string-type SQLServer --settings DefaultConnection="Server=tcp:myserver.database.windows.net,1433;Initial Catalog=EnterpriseCRM;Persist Security Info=False;User ID=myuser;Password=mypassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# 3. Deploy application
az webapp deployment source config --resource-group myResourceGroup --name myWebApp --repo-url https://github.com/yourusername/EnterpriseCRM.git --branch main --manual-integration
```

### **2. Docker Containers**
**Best for:** Consistent environments and microservices

**Dockerfile:**
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/EnterpriseCRM.WebAPI/EnterpriseCRM.WebAPI.csproj", "src/EnterpriseCRM.WebAPI/"]
COPY ["src/EnterpriseCRM.Application/EnterpriseCRM.Application.csproj", "src/EnterpriseCRM.Application/"]
COPY ["src/EnterpriseCRM.Infrastructure/EnterpriseCRM.Infrastructure.csproj", "src/EnterpriseCRM.Infrastructure/"]
COPY ["src/EnterpriseCRM.Core/EnterpriseCRM.Core.csproj", "src/EnterpriseCRM.Core/"]

# Restore dependencies
RUN dotnet restore "src/EnterpriseCRM.WebAPI/EnterpriseCRM.WebAPI.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/src/EnterpriseCRM.WebAPI"
RUN dotnet build "EnterpriseCRM.WebAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "EnterpriseCRM.WebAPI.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure ports
EXPOSE 80
EXPOSE 443

# Start application
ENTRYPOINT ["dotnet", "EnterpriseCRM.WebAPI.dll"]
```

**Docker Compose:**
```yaml
version: '3.8'

services:
  webapi:
    build: .
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=EnterpriseCRM;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
    depends_on:
      - db

  blazor:
    build: 
      context: .
      dockerfile: Dockerfile.Blazor
    ports:
      - "8081:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ApiBaseUrl=http://webapi:80
    depends_on:
      - webapi

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

volumes:
  sqlserver_data:
  redis_data:
```

### **3. Kubernetes**
**Best for:** Large-scale production deployments

**Deployment YAML:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: enterprise-crm-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: enterprise-crm-api
  template:
    metadata:
      labels:
        app: enterprise-crm-api
    spec:
      containers:
      - name: api
        image: yourregistry/enterprise-crm-api:latest
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: enterprise-crm-api-service
spec:
  selector:
    app: enterprise-crm-api
  ports:
  - port: 80
    targetPort: 80
  type: LoadBalancer
```

### **4. On-Premises IIS**
**Best for:** Traditional Windows Server environments

**IIS Configuration:**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments=".\EnterpriseCRM.WebAPI.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        <environmentVariable name="ConnectionStrings__DefaultConnection" value="Server=localhost;Database=EnterpriseCRM;Trusted_Connection=true;" />
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>
```

## 🔧 Environment Configuration

### **Development Environment**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EnterpriseCRM;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "development-secret-key-not-for-production",
    "Issuer": "EnterpriseCRM",
    "Audience": "EnterpriseCRM",
    "ExpiryMinutes": 60
  }
}
```

### **Production Environment**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:prodserver.database.windows.net,1433;Initial Catalog=EnterpriseCRM;Persist Security Info=False;User ID=produser;Password=SecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "JwtSettings": {
    "SecretKey": "production-secret-key-from-key-vault",
    "Issuer": "EnterpriseCRM",
    "Audience": "EnterpriseCRM",
    "ExpiryMinutes": 30
  },
  "Redis": {
    "ConnectionString": "prod-redis.redis.cache.windows.net:6380,password=RedisPassword123!,ssl=True,abortConnect=False"
  }
}
```

## 🗄️ Database Deployment

### **SQL Server Deployment**
```sql
-- 1. Create database
CREATE DATABASE EnterpriseCRM;
GO

-- 2. Create login and user
CREATE LOGIN [EnterpriseCRMUser] WITH PASSWORD = 'SecurePassword123!';
USE EnterpriseCRM;
CREATE USER [EnterpriseCRMUser] FOR LOGIN [EnterpriseCRMUser];
ALTER ROLE db_owner ADD MEMBER [EnterpriseCRMUser];
GO

-- 3. Run Entity Framework migrations
dotnet ef database update --project src/EnterpriseCRM.Infrastructure --connection "Server=prodserver;Database=EnterpriseCRM;User Id=EnterpriseCRMUser;Password=SecurePassword123!;TrustServerCertificate=true;"
```

### **Azure SQL Database**
```bash
# Create Azure SQL Database
az sql db create --resource-group myResourceGroup --server myserver --name EnterpriseCRM --service-objective S2

# Configure firewall rule
az sql server firewall-rule create --resource-group myResourceGroup --server myserver --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# Run migrations
dotnet ef database update --project src/EnterpriseCRM.Infrastructure --connection "Server=myserver.database.windows.net;Database=EnterpriseCRM;User Id=myuser;Password=mypassword;TrustServerCertificate=true;"
```

## 🔐 Security Configuration

### **SSL/TLS Configuration**
```csharp
// Program.cs
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 443;
});

// Force HTTPS in production
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });
}
```

### **Authentication Configuration**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });
```

## 📊 Monitoring and Logging

### **Application Insights**
```csharp
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);
```

### **Health Checks**
```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .AddRedis(builder.Configuration.GetConnectionString("Redis"));

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### **Logging Configuration**
```csharp
builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces));
```

## 🚀 CI/CD Pipeline

### **GitHub Actions**
```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish src/EnterpriseCRM.WebAPI/EnterpriseCRM.WebAPI.csproj --configuration Release --output ./publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'myWebApp'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: './publish'
```

### **Azure DevOps**
```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

stages:
- stage: Build
  jobs:
  - job: BuildJob
    steps:
    - task: DotNetCoreCLI@2
      displayName: 'Restore packages'
      inputs:
        command: 'restore'
        projects: '**/*.csproj'
    
    - task: DotNetCoreCLI@2
      displayName: 'Build solution'
      inputs:
        command: 'build'
        projects: '**/*.csproj'
        arguments: '--configuration $(buildConfiguration)'
    
    - task: DotNetCoreCLI@2
      displayName: 'Run tests'
      inputs:
        command: 'test'
        projects: '**/*Tests/*.csproj'
        arguments: '--configuration $(buildConfiguration) --collect:"XPlat Code Coverage"'
    
    - task: DotNetCoreCLI@2
      displayName: 'Publish'
      inputs:
        command: 'publish'
        publishWebProjects: true
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'

- stage: Deploy
  dependsOn: Build
  jobs:
  - deployment: DeployJob
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: 'Azure Service Connection'
              appName: 'myWebApp'
              package: '$(Pipeline.Workspace)/**/*.zip'
```

## 🔄 Blue-Green Deployment

### **Azure App Service Slots**
```bash
# Create staging slot
az webapp deployment slot create --name myWebApp --resource-group myResourceGroup --slot staging

# Deploy to staging
az webapp deployment source config --name myWebApp --resource-group myResourceGroup --slot staging --repo-url https://github.com/yourusername/EnterpriseCRM.git --branch staging

# Swap slots
az webapp deployment slot swap --name myWebApp --resource-group myResourceGroup --slot staging --target-slot production
```

## 📈 Performance Optimization

### **Caching Configuration**
```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddMemoryCache();
```

### **Response Compression**
```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
```

### **Database Optimization**
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
    
    options.EnableSensitiveDataLogging(false);
    options.EnableServiceProviderCaching();
    options.EnableDetailedErrors(false);
});
```

## 🛡️ Security Best Practices

### **Environment Variables**
- Store sensitive configuration in Azure Key Vault
- Use managed identities for Azure resources
- Rotate secrets regularly
- Never commit secrets to source control

### **Network Security**
- Use VNet integration for Azure App Service
- Configure firewall rules for SQL Server
- Enable DDoS protection
- Use private endpoints for internal communication

### **Application Security**
- Enable HTTPS only
- Use strong authentication
- Implement rate limiting
- Regular security updates

This deployment guide provides comprehensive options for deploying the Enterprise CRM system in various environments, from development to production, with proper security, monitoring, and performance considerations.
