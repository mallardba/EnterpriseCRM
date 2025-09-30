# Enterprise CRM - Containerization with Docker

## 🐳 Docker Overview

### **What is Docker?**

Docker is a containerization platform that packages applications and their dependencies into lightweight, portable containers. It ensures consistent environments across development, testing, and production, making deployment and scaling more reliable.

### **Why Use Docker?**

**Consistency:**
- Same environment across all stages
- Eliminates "it works on my machine" issues
- Predictable deployments

**Portability:**
- Run anywhere Docker is supported
- Cloud-agnostic deployments
- Easy environment replication

**Isolation:**
- Application isolation
- Resource management
- Security boundaries

**Scalability:**
- Easy horizontal scaling
- Container orchestration
- Microservices architecture

### **Docker Benefits**

- **Consistency**: Same environment everywhere
- **Portability**: Run anywhere
- **Efficiency**: Resource optimization
- **Scalability**: Easy scaling
- **Security**: Application isolation

### **When to Use Docker**

**Good Use Cases:**
- Microservices architecture
- Cloud deployments
- Development environment consistency
- CI/CD pipelines

**Avoid When:**
- Simple desktop applications
- Performance-critical systems
- Over-engineering simple projects
- Legacy monolithic applications

## 🎯 Core Concepts

### **1. Dockerfile**
Blueprint for creating Docker images

```dockerfile
# Multi-stage build for .NET application
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

# Install additional tools
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Configure ports
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Start application
ENTRYPOINT ["dotnet", "EnterpriseCRM.WebAPI.dll"]
```

### **2. Docker Compose**
Multi-container application orchestration

```yaml
version: '3.8'

services:
  # Web API Service
  webapi:
    build:
      context: .
      dockerfile: Dockerfile.WebAPI
    ports:
      - "8080:80"
      - "8443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80;https://+:443
      - ConnectionStrings__DefaultConnection=Server=db;Database=EnterpriseCRM;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
      - Redis__ConnectionString=redis:6379
    depends_on:
      - db
      - redis
    networks:
      - enterprise-network
    volumes:
      - app-logs:/app/logs
    restart: unless-stopped

  # Blazor Server Service
  blazor:
    build:
      context: .
      dockerfile: Dockerfile.Blazor
    ports:
      - "8081:80"
      - "8444:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80;https://+:443
      - ApiBaseUrl=http://webapi:80
    depends_on:
      - webapi
    networks:
      - enterprise-network
    restart: unless-stopped

  # Database Service
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
      - ./scripts:/docker-entrypoint-initdb.d
    networks:
      - enterprise-network
    restart: unless-stopped

  # Redis Cache Service
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - enterprise-network
    restart: unless-stopped
    command: redis-server --appendonly yes

  # Nginx Reverse Proxy
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf
      - ./nginx/ssl:/etc/nginx/ssl
    depends_on:
      - webapi
      - blazor
    networks:
      - enterprise-network
    restart: unless-stopped

volumes:
  sqlserver_data:
  redis_data:
  app-logs:

networks:
  enterprise-network:
    driver: bridge
```

## 🔧 Advanced Docker Patterns

### **1. Multi-Stage Builds**
Optimized builds with multiple stages

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore dependencies
COPY ["src/EnterpriseCRM.sln", "."]
COPY ["src/EnterpriseCRM.WebAPI/EnterpriseCRM.WebAPI.csproj", "src/EnterpriseCRM.WebAPI/"]
COPY ["src/EnterpriseCRM.Application/EnterpriseCRM.Application.csproj", "src/EnterpriseCRM.Application/"]
COPY ["src/EnterpriseCRM.Infrastructure/EnterpriseCRM.Infrastructure.csproj", "src/EnterpriseCRM.Infrastructure/"]
COPY ["src/EnterpriseCRM.Core/EnterpriseCRM.Core.csproj", "src/EnterpriseCRM.Core/"]

RUN dotnet restore "EnterpriseCRM.sln"

# Copy source and build
COPY . .
RUN dotnet build "EnterpriseCRM.sln" -c Release --no-restore

# Stage 2: Test
FROM build AS test
WORKDIR /src
RUN dotnet test "tests/EnterpriseCRM.UnitTests/EnterpriseCRM.UnitTests.csproj" -c Release --no-build --verbosity normal

# Stage 3: Publish Web API
FROM build AS publish-webapi
WORKDIR "/src/src/EnterpriseCRM.WebAPI"
RUN dotnet publish "EnterpriseCRM.WebAPI.csproj" -c Release -o /app/publish --no-restore

# Stage 4: Publish Blazor
FROM build AS publish-blazor
WORKDIR "/src/src/EnterpriseCRM.BlazorServer"
RUN dotnet publish "EnterpriseCRM.BlazorServer.csproj" -c Release -o /app/publish --no-restore

# Stage 5: Runtime Web API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS webapi-runtime
WORKDIR /app
COPY --from=publish-webapi /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "EnterpriseCRM.WebAPI.dll"]

# Stage 6: Runtime Blazor
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS blazor-runtime
WORKDIR /app
COPY --from=publish-blazor /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "EnterpriseCRM.BlazorServer.dll"]
```

### **2. Development Docker Compose**
```yaml
version: '3.8'

services:
  # Development Web API
  webapi-dev:
    build:
      context: .
      dockerfile: Dockerfile.WebAPI
      target: build
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Server=db-dev;Database=EnterpriseCRM_Dev;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;
    volumes:
      - ./src:/src
      - /src/bin
      - /src/obj
    depends_on:
      - db-dev
    networks:
      - dev-network

  # Development Database
  db-dev:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=DevPassword123!
      - MSSQL_PID=Express
    ports:
      - "1434:1433"
    volumes:
      - sqlserver_dev_data:/var/opt/mssql
    networks:
      - dev-network

  # Development Redis
  redis-dev:
    image: redis:7-alpine
    ports:
      - "6380:6379"
    volumes:
      - redis_dev_data:/data
    networks:
      - dev-network

volumes:
  sqlserver_dev_data:
  redis_dev_data:

networks:
  dev-network:
    driver: bridge
```

### **3. Production Docker Compose**
```yaml
version: '3.8'

services:
  # Production Web API
  webapi-prod:
    image: enterprisecrm/webapi:latest
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - Redis__ConnectionString=${REDIS_CONNECTION_STRING}
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
    depends_on:
      - db-prod
      - redis-prod
    networks:
      - prod-network
    restart: unless-stopped
    deploy:
      replicas: 3
      resources:
        limits:
          memory: 512M
          cpus: '0.5'
        reservations:
          memory: 256M
          cpus: '0.25'

  # Production Database
  db-prod:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${DB_SA_PASSWORD}
      - MSSQL_PID=Standard
    volumes:
      - sqlserver_prod_data:/var/opt/mssql
      - ./backups:/backups
    networks:
      - prod-network
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '1.0'

  # Production Redis
  redis-prod:
    image: redis:7-alpine
    volumes:
      - redis_prod_data:/data
    networks:
      - prod-network
    restart: unless-stopped
    deploy:
      resources:
        limits:
          memory: 256M
          cpus: '0.25'

volumes:
  sqlserver_prod_data:
  redis_prod_data:

networks:
  prod-network:
    driver: bridge
```

## 🎯 Container Orchestration

### **1. Kubernetes Deployment**
```yaml
# Web API Deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: enterprise-crm-webapi
  labels:
    app: enterprise-crm-webapi
spec:
  replicas: 3
  selector:
    matchLabels:
      app: enterprise-crm-webapi
  template:
    metadata:
      labels:
        app: enterprise-crm-webapi
    spec:
      containers:
      - name: webapi
        image: enterprisecrm/webapi:latest
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
# Web API Service
apiVersion: v1
kind: Service
metadata:
  name: enterprise-crm-webapi-service
spec:
  selector:
    app: enterprise-crm-webapi
  ports:
  - port: 80
    targetPort: 80
  type: LoadBalancer
---
# Database Deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: enterprise-crm-db
spec:
  replicas: 1
  selector:
    matchLabels:
      app: enterprise-crm-db
  template:
    metadata:
      labels:
        app: enterprise-crm-db
    spec:
      containers:
      - name: sqlserver
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
        - name: ACCEPT_EULA
          value: "Y"
        - name: SA_PASSWORD
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: sa-password
        ports:
        - containerPort: 1433
        volumeMounts:
        - name: sqlserver-storage
          mountPath: /var/opt/mssql
      volumes:
      - name: sqlserver-storage
        persistentVolumeClaim:
          claimName: sqlserver-pvc
---
# Database Service
apiVersion: v1
kind: Service
metadata:
  name: enterprise-crm-db-service
spec:
  selector:
    app: enterprise-crm-db
  ports:
  - port: 1433
    targetPort: 1433
  type: ClusterIP
```

### **2. Docker Swarm**
```yaml
version: '3.8'

services:
  webapi:
    image: enterprisecrm/webapi:latest
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    deploy:
      replicas: 3
      placement:
        constraints:
          - node.role == worker
      resources:
        limits:
          memory: 512M
          cpus: '0.5'
        reservations:
          memory: 256M
          cpus: '0.25'
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
    networks:
      - enterprise-network

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    deploy:
      replicas: 1
      placement:
        constraints:
          - node.role == manager
      resources:
        limits:
          memory: 2G
          cpus: '1.0'
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - enterprise-network

volumes:
  sqlserver_data:
    driver: local

networks:
  enterprise-network:
    driver: overlay
```

## 🔄 CI/CD Integration

### **1. GitHub Actions Docker Build**
```yaml
name: Docker Build and Push

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Set up Docker Buildx
      uses: docker/setup-buildx-action@v3
      
    - name: Login to Docker Hub
      uses: docker/login-action@v3
      with:
        username: ${{ secrets.DOCKER_USERNAME }}
        password: ${{ secrets.DOCKER_PASSWORD }}
        
    - name: Build and push Web API image
      uses: docker/build-push-action@v5
      with:
        context: .
        file: ./Dockerfile.WebAPI
        push: true
        tags: |
          enterprisecrm/webapi:latest
          enterprisecrm/webapi:${{ github.sha }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
        
    - name: Build and push Blazor image
      uses: docker/build-push-action@v5
      with:
        context: .
        file: ./Dockerfile.Blazor
        push: true
        tags: |
          enterprisecrm/blazor:latest
          enterprisecrm/blazor:${{ github.sha }}
        cache-from: type=gha
        cache-to: type=gha,mode=max
```

### **2. Azure Container Registry**
```yaml
name: Build and Push to ACR

on:
  push:
    branches: [ main ]

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
        
    - name: Build and push to ACR
      uses: azure/docker-login@v1
      with:
        login-server: ${{ secrets.ACR_LOGIN_SERVER }}
        username: ${{ secrets.ACR_USERNAME }}
        password: ${{ secrets.ACR_PASSWORD }}
        
    - name: Build and push image
      run: |
        docker build -f Dockerfile.WebAPI -t ${{ secrets.ACR_LOGIN_SERVER }}/enterprisecrm-webapi:latest .
        docker push ${{ secrets.ACR_LOGIN_SERVER }}/enterprisecrm-webapi:latest
```

## 🧪 Testing Containers

### **1. Container Testing**
```csharp
public class DockerIntegrationTests : IClassFixture<DockerFixture>
{
    private readonly DockerFixture _dockerFixture;

    public DockerIntegrationTests(DockerFixture dockerFixture)
    {
        _dockerFixture = dockerFixture;
    }

    [Fact]
    public async Task WebApiContainer_ShouldBeHealthy()
    {
        // Arrange
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:8080");

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DatabaseContainer_ShouldBeAccessible()
    {
        // Arrange
        var connectionString = "Server=localhost,1433;Database=EnterpriseCRM;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;";

        // Act
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Assert
        connection.State.Should().Be(ConnectionState.Open);
    }
}

public class DockerFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // Start Docker containers
        await StartContainersAsync();
        
        // Wait for services to be ready
        await WaitForServicesAsync();
    }

    public async Task DisposeAsync()
    {
        // Stop Docker containers
        await StopContainersAsync();
    }

    private async Task StartContainersAsync()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker-compose",
                Arguments = "up -d",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();
    }

    private async Task WaitForServicesAsync()
    {
        var maxAttempts = 30;
        var attempt = 0;

        while (attempt < maxAttempts)
        {
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync("http://localhost:8080/health");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // Service not ready yet
            }

            await Task.Delay(2000);
            attempt++;
        }

        throw new TimeoutException("Services did not start within expected time");
    }

    private async Task StopContainersAsync()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker-compose",
                Arguments = "down",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();
    }
}
```

## 🚀 Performance Optimization

### **1. Multi-Stage Build Optimization**
```dockerfile
# Use specific base images for smaller size
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Install only necessary packages
RUN apk add --no-cache \
    curl \
    && rm -rf /var/cache/apk/*

# Copy published application
COPY --from=publish /app/publish .

# Use non-root user
RUN adduser -D -s /bin/sh appuser
USER appuser

EXPOSE 80
ENTRYPOINT ["dotnet", "EnterpriseCRM.WebAPI.dll"]
```

### **2. Layer Caching**
```dockerfile
# Copy package files first for better caching
COPY ["src/EnterpriseCRM.WebAPI/EnterpriseCRM.WebAPI.csproj", "src/EnterpriseCRM.WebAPI/"]
COPY ["src/EnterpriseCRM.Application/EnterpriseCRM.Application.csproj", "src/EnterpriseCRM.Application/"]

# Restore dependencies (this layer will be cached)
RUN dotnet restore "src/EnterpriseCRM.WebAPI/EnterpriseCRM.WebAPI.csproj"

# Copy source code (this layer will be rebuilt when source changes)
COPY . .
```

### **3. Health Checks**
```dockerfile
# Health check configuration
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1
```

## 📊 Best Practices

### **1. Use .dockerignore**
```dockerignore
# .dockerignore
**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md
```

### **2. Security Best Practices**
```dockerfile
# Use non-root user
RUN adduser --disabled-password --gecos '' appuser
USER appuser

# Use specific versions
FROM mcr.microsoft.com/dotnet/aspnet:8.0.0-alpine

# Remove unnecessary packages
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Use secrets for sensitive data
RUN --mount=type=secret,id=db_password \
    echo "Database password: $(cat /run/secrets/db_password)"
```

### **3. Resource Limits**
```yaml
# docker-compose.yml
services:
  webapi:
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: '0.5'
        reservations:
          memory: 256M
          cpus: '0.25'
```

### **4. Environment Configuration**
```dockerfile
# Use environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Use build args for build-time configuration
ARG BUILD_CONFIGURATION=Release
RUN dotnet build -c $BUILD_CONFIGURATION
```

Docker containerization provides a robust foundation for deploying the Enterprise CRM system consistently across different environments, ensuring reliability, scalability, and ease of management.
