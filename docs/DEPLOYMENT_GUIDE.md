# Enterprise CRM Deployment Guide

## Overview

This guide covers deployment options for the Enterprise CRM project, including Azure deployment and Kubernetes containerization strategies.

## Deployment Strategy Recommendation

### **Phase 1: Azure Deployment (Recommended First)**
- ✅ **Faster to deploy** - no Docker learning curve
- ✅ **Managed services** - Azure handles infrastructure
- ✅ **Built-in CI/CD** - GitHub Actions integration
- ✅ **Database hosting** - Azure SQL Database
- ✅ **Easy scaling** - App Service handles load balancing

### **Phase 2: Containerization (Future Enhancement)**
- ✅ **Portability** - Run anywhere (AWS, GCP, on-premises)
- ✅ **Consistency** - Same environment everywhere
- ✅ **Microservices** - Split into smaller services
- ✅ **Kubernetes** - Advanced orchestration

## Azure Deployment

### Prerequisites
- Azure subscription
- Azure CLI installed
- .NET 8.0 SDK
- Git repository with your code

### 1. Database Setup (Azure SQL Database)

#### Create Azure SQL Database
```bash
# Login to Azure
az login

# Create resource group
az group create --name EnterpriseCRM-RG --location eastus

# Create SQL Server
az sql server create \
  --name enterprisecrm-sql-server \
  --resource-group EnterpriseCRM-RG \
  --location eastus \
  --admin-user sqladmin \
  --admin-password "YourSecurePassword123!"

# Create SQL Database
az sql db create \
  --resource-group EnterpriseCRM-RG \
  --server enterprisecrm-sql-server \
  --name EnterpriseCRM \
  --service-objective S0
```

#### Configure Firewall
```bash
# Allow Azure services
az sql server firewall-rule create \
  --resource-group EnterpriseCRM-RG \
  --server enterprisecrm-sql-server \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Allow your IP (replace with your IP)
az sql server firewall-rule create \
  --resource-group EnterpriseCRM-RG \
  --server enterprisecrm-sql-server \
  --name AllowMyIP \
  --start-ip-address YOUR_IP_ADDRESS \
  --end-ip-address YOUR_IP_ADDRESS
```

#### Update Connection String
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:enterprisecrm-sql-server.database.windows.net,1433;Initial Catalog=EnterpriseCRM;Persist Security Info=False;User ID=sqladmin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### 2. Web API Deployment (Azure App Service)

#### Create App Service Plan
```bash
az appservice plan create \
  --name EnterpriseCRM-Plan \
  --resource-group EnterpriseCRM-RG \
  --sku B1 \
  --is-linux
```

#### Create Web API App Service
```bash
az webapp create \
  --resource-group EnterpriseCRM-RG \
  --plan EnterpriseCRM-Plan \
  --name enterprisecrm-webapi \
  --runtime "DOTNET|8.0"
```

#### Configure App Settings
```bash
az webapp config appsettings set \
  --resource-group EnterpriseCRM-RG \
  --name enterprisecrm-webapi \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="Server=tcp:enterprisecrm-sql-server.database.windows.net,1433;Initial Catalog=EnterpriseCRM;Persist Security Info=False;User ID=sqladmin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### 3. Blazor Server Deployment

#### Create Blazor Server App Service
```bash
az webapp create \
  --resource-group EnterpriseCRM-RG \
  --plan EnterpriseCRM-Plan \
  --name enterprisecrm-blazor \
  --runtime "DOTNET|8.0"
```

#### Configure Blazor App Settings
```bash
az webapp config appsettings set \
  --resource-group EnterpriseCRM-RG \
  --name enterprisecrm-blazor \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="Server=tcp:enterprisecrm-sql-server.database.windows.net,1433;Initial Catalog=EnterpriseCRM;Persist Security Info=False;User ID=sqladmin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### 4. Database Migration

#### Deploy Database Schema
```bash
# Navigate to Infrastructure project
cd src/EnterpriseCRM.Infrastructure

# Update database
dotnet ef database update --connection "Server=tcp:enterprisecrm-sql-server.database.windows.net,1433;Initial Catalog=EnterpriseCRM;Persist Security Info=False;User ID=sqladmin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### 5. Deploy Applications

#### Deploy Web API
```bash
# Navigate to WebAPI project
cd src/EnterpriseCRM.WebAPI

# Deploy using Azure CLI
az webapp deployment source config-zip \
  --resource-group EnterpriseCRM-RG \
  --name enterprisecrm-webapi \
  --src publish.zip
```

#### Deploy Blazor Server
```bash
# Navigate to Blazor Server project
cd src/EnterpriseCRM.BlazorServer

# Deploy using Azure CLI
az webapp deployment source config-zip \
  --resource-group EnterpriseCRM-RG \
  --name enterprisecrm-blazor \
  --src publish.zip
```

### 6. CI/CD Pipeline (GitHub Actions)

#### Create GitHub Actions Workflow
```yaml
# .github/workflows/azure-deploy.yml
name: Deploy to Azure

on:
  push:
    branches: [ main ]
  pull_request:
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
    
    - name: Publish Web API
      run: dotnet publish src/EnterpriseCRM.WebAPI/EnterpriseCRM.WebAPI.csproj -c Release -o ./publish/webapi
    
    - name: Publish Blazor Server
      run: dotnet publish src/EnterpriseCRM.BlazorServer/EnterpriseCRM.BlazorServer.csproj -c Release -o ./publish/blazor
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'enterprisecrm-webapi'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish/webapi
```

## Kubernetes Containerization

### Prerequisites
- Docker installed
- Kubernetes cluster (AKS, EKS, GKE, or local minikube)
- kubectl configured
- Azure Container Registry (ACR) or Docker Hub

### 1. Create Dockerfiles

#### Web API Dockerfile
```dockerfile
# src/EnterpriseCRM.WebAPI/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EnterpriseCRM.WebAPI/EnterpriseCRM.WebAPI.csproj", "src/EnterpriseCRM.WebAPI/"]
COPY ["src/EnterpriseCRM.Application/EnterpriseCRM.Application.csproj", "src/EnterpriseCRM.Application/"]
COPY ["src/EnterpriseCRM.Core/EnterpriseCRM.Core.csproj", "src/EnterpriseCRM.Core/"]
COPY ["src/EnterpriseCRM.Infrastructure/EnterpriseCRM.Infrastructure.csproj", "src/EnterpriseCRM.Infrastructure/"]
RUN dotnet restore "src/EnterpriseCRM.WebAPI/EnterpriseCRM.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/EnterpriseCRM.WebAPI"
RUN dotnet build "EnterpriseCRM.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnterpriseCRM.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnterpriseCRM.WebAPI.dll"]
```

#### Blazor Server Dockerfile
```dockerfile
# src/EnterpriseCRM.BlazorServer/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EnterpriseCRM.BlazorServer/EnterpriseCRM.BlazorServer.csproj", "src/EnterpriseCRM.BlazorServer/"]
COPY ["src/EnterpriseCRM.Application/EnterpriseCRM.Application.csproj", "src/EnterpriseCRM.Application/"]
COPY ["src/EnterpriseCRM.Core/EnterpriseCRM.Core.csproj", "src/EnterpriseCRM.Core/"]
COPY ["src/EnterpriseCRM.Infrastructure/EnterpriseCRM.Infrastructure.csproj", "src/EnterpriseCRM.Infrastructure/"]
RUN dotnet restore "src/EnterpriseCRM.BlazorServer/EnterpriseCRM.BlazorServer.csproj"
COPY . .
WORKDIR "/src/src/EnterpriseCRM.BlazorServer"
RUN dotnet build "EnterpriseCRM.BlazorServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EnterpriseCRM.BlazorServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EnterpriseCRM.BlazorServer.dll"]
```

### 2. Build and Push Images

#### Build Images
```bash
# Build Web API image
docker build -f src/EnterpriseCRM.WebAPI/Dockerfile -t enterprisecrm-webapi:latest .

# Build Blazor Server image
docker build -f src/EnterpriseCRM.BlazorServer/Dockerfile -t enterprisecrm-blazor:latest .
```

#### Push to Registry
```bash
# Tag for Azure Container Registry
docker tag enterprisecrm-webapi:latest yourregistry.azurecr.io/enterprisecrm-webapi:latest
docker tag enterprisecrm-blazor:latest yourregistry.azurecr.io/enterprisecrm-blazor:latest

# Push to registry
docker push yourregistry.azurecr.io/enterprisecrm-webapi:latest
docker push yourregistry.azurecr.io/enterprisecrm-blazor:latest
```

### 3. Kubernetes Manifests

#### Namespace
```yaml
# k8s/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: enterprisecrm
```

#### ConfigMap
```yaml
# k8s/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: enterprisecrm-config
  namespace: enterprisecrm
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  ConnectionStrings__DefaultConnection: "Server=tcp:your-sql-server.database.windows.net,1433;Initial Catalog=EnterpriseCRM;Persist Security Info=False;User ID=sqladmin;Password=YourPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

#### Web API Deployment
```yaml
# k8s/webapi-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: enterprisecrm-webapi
  namespace: enterprisecrm
spec:
  replicas: 2
  selector:
    matchLabels:
      app: enterprisecrm-webapi
  template:
    metadata:
      labels:
        app: enterprisecrm-webapi
    spec:
      containers:
      - name: webapi
        image: yourregistry.azurecr.io/enterprisecrm-webapi:latest
        ports:
        - containerPort: 80
        envFrom:
        - configMapRef:
            name: enterprisecrm-config
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
---
apiVersion: v1
kind: Service
metadata:
  name: enterprisecrm-webapi-service
  namespace: enterprisecrm
spec:
  selector:
    app: enterprisecrm-webapi
  ports:
  - port: 80
    targetPort: 80
  type: ClusterIP
```

#### Blazor Server Deployment
```yaml
# k8s/blazor-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: enterprisecrm-blazor
  namespace: enterprisecrm
spec:
  replicas: 2
  selector:
    matchLabels:
      app: enterprisecrm-blazor
  template:
    metadata:
      labels:
        app: enterprisecrm-blazor
    spec:
      containers:
      - name: blazor
        image: yourregistry.azurecr.io/enterprisecrm-blazor:latest
        ports:
        - containerPort: 80
        envFrom:
        - configMapRef:
            name: enterprisecrm-config
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
---
apiVersion: v1
kind: Service
metadata:
  name: enterprisecrm-blazor-service
  namespace: enterprisecrm
spec:
  selector:
    app: enterprisecrm-blazor
  ports:
  - port: 80
    targetPort: 80
  type: ClusterIP
```

#### Ingress
```yaml
# k8s/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: enterprisecrm-ingress
  namespace: enterprisecrm
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  rules:
  - host: api.enterprisecrm.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: enterprisecrm-webapi-service
            port:
              number: 80
  - host: app.enterprisecrm.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: enterprisecrm-blazor-service
            port:
              number: 80
```

### 4. Deploy to Kubernetes

#### Apply Manifests
```bash
# Create namespace
kubectl apply -f k8s/namespace.yaml

# Apply configurations
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/webapi-deployment.yaml
kubectl apply -f k8s/blazor-deployment.yaml
kubectl apply -f k8s/ingress.yaml
```

#### Check Deployment Status
```bash
# Check pods
kubectl get pods -n enterprisecrm

# Check services
kubectl get services -n enterprisecrm

# Check ingress
kubectl get ingress -n enterprisecrm
```

### 5. CI/CD Pipeline for Kubernetes

#### GitHub Actions for Kubernetes
```yaml
# .github/workflows/k8s-deploy.yml
name: Deploy to Kubernetes

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v2
    
    - name: Login to Azure Container Registry
      uses: azure/docker-login@v1
      with:
        login-server: yourregistry.azurecr.io
        username: ${{ secrets.ACR_USERNAME }}
        password: ${{ secrets.ACR_PASSWORD }}
    
    - name: Build and push Web API
      uses: docker/build-push-action@v3
      with:
        context: .
        file: ./src/EnterpriseCRM.WebAPI/Dockerfile
        push: true
        tags: yourregistry.azurecr.io/enterprisecrm-webapi:latest
    
    - name: Build and push Blazor Server
      uses: docker/build-push-action@v3
      with:
        context: .
        file: ./src/EnterpriseCRM.BlazorServer/Dockerfile
        push: true
        tags: yourregistry.azurecr.io/enterprisecrm-blazor:latest
    
    - name: Deploy to Kubernetes
      uses: azure/k8s-deploy@v1
      with:
        manifests: |
          k8s/namespace.yaml
          k8s/configmap.yaml
          k8s/webapi-deployment.yaml
          k8s/blazor-deployment.yaml
          k8s/ingress.yaml
        kubeconfig: ${{ secrets.KUBE_CONFIG }}
```

## Comparison Table

| Aspect | Azure Deployment | Kubernetes |
|--------|------------------|------------|
| **Setup Time** | ⭐⭐⭐ Fast (1-2 hours) | ⭐⭐ Medium (4-6 hours) |
| **Learning Curve** | ⭐⭐⭐ Easy | ⭐⭐ Medium |
| **Cost** | ⭐⭐ Pay-per-use | ⭐⭐⭐ Flexible |
| **Portability** | ⭐ Azure-only | ⭐⭐⭐ Anywhere |
| **Scaling** | ⭐⭐⭐ Auto | ⭐⭐ Manual |
| **Maintenance** | ⭐⭐⭐ Managed | ⭐ Self-managed |
| **Monitoring** | ⭐⭐⭐ Built-in | ⭐⭐ Custom setup |
| **Security** | ⭐⭐⭐ Managed | ⭐⭐ Self-configured |

## Recommendations

### **Start with Azure if:**
- You want to deploy quickly
- You're learning cloud deployment
- You need managed services
- You want to focus on application features

### **Choose Kubernetes if:**
- You need multi-cloud deployment
- You want to learn containerization
- You plan to split into microservices
- You need advanced orchestration

## Next Steps

1. **Choose your deployment strategy**
2. **Set up Azure resources** (if going with Azure)
3. **Create Docker images** (if going with Kubernetes)
4. **Configure CI/CD pipeline**
5. **Deploy and test**
6. **Set up monitoring and logging**

## Security Considerations

### Azure Deployment
- Use Azure Key Vault for secrets
- Enable managed identity
- Configure network security groups
- Use Azure Application Gateway for SSL termination

### Kubernetes Deployment
- Use Kubernetes secrets for sensitive data
- Implement network policies
- Use RBAC for access control
- Enable pod security policies
- Use service mesh (Istio) for advanced security

## Monitoring and Logging

### Azure
- Azure Application Insights
- Azure Monitor
- Azure Log Analytics

### Kubernetes
- Prometheus + Grafana
- ELK Stack (Elasticsearch, Logstash, Kibana)
- Jaeger for distributed tracing
