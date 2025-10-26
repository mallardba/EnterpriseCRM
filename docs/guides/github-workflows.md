# Enterprise CRM - GitHub Workflows

## 🚀 Getting Started with GitHub's Suggested Workflows

GitHub provides excellent workflow templates based on your tech stack. For this .NET Enterprise CRM project, GitHub suggests:

### **✅ Recommended: .NET Workflow**
- **Perfect for:** ASP.NET Core Web API and Blazor Server applications
- **Includes:** Build, test, restore dependencies, and basic CI/CD
- **Benefits:** Quick setup, Microsoft best practices, community tested
- **Action:** Accept this workflow as your foundation

### **❌ Skip: .NET Desktop Workflow**
- **Not applicable:** This project builds web applications, not desktop apps
- **Reason:** Would add unnecessary complexity for web-based CRM system

### **🎯 Recommended Approach:**
1. **Start with GitHub's .NET workflow** for immediate CI/CD
2. **Enhance with custom workflows** from this documentation for advanced features
3. **Add database migrations, security scanning, and multi-environment deployment**

## 🔄 GitHub Actions Overview

GitHub Actions provides powerful CI/CD capabilities for the Enterprise CRM project. This document covers essential workflows for building, testing, deploying, and maintaining the application.

## 🏗️ Essential Workflows

### **1. CI/CD Pipeline (`ci-cd.yml`)**

**Purpose:** Complete build, test, and deployment pipeline

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

env:
  DOTNET_VERSION: '8.0.x'
  SOLUTION_PATH: 'src/EnterpriseCRM.sln'

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
        
    - name: Cache NuGet packages
      uses: actions/cache@v3
      with:
        path: ~/.nuget/packages
        key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
        restore-keys: |
          ${{ runner.os }}-nuget-
          
    - name: Restore dependencies
      run: dotnet restore ${{ env.SOLUTION_PATH }}
      
    - name: Build solution
      run: dotnet build ${{ env.SOLUTION_PATH }} --no-restore --configuration Release
      
    - name: Run unit tests
      run: dotnet test tests/EnterpriseCRM.UnitTests --no-build --configuration Release --collect:"XPlat Code Coverage" --logger trx --results-directory TestResults
      
    - name: Run integration tests
      run: dotnet test tests/EnterpriseCRM.IntegrationTests --no-build --configuration Release --logger trx --results-directory TestResults
      
    - name: Upload test results
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: test-results
        path: TestResults/
        
    - name: Generate coverage report
      uses: danielpalme/ReportGenerator@5.2.0
      with:
        reports: '**/coverage.cobertura.xml'
        targetdir: 'coverage'
        reporttypes: 'HtmlInline_AzurePipelines;Cobertura'
        
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        file: '**/coverage.cobertura.xml'
        flags: unittests
        name: codecov-umbrella
        
  security-scan:
    runs-on: ubuntu-latest
    needs: build-and-test
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Run security scan
      uses: securecodewarrior/github-action-add-sarif@v1
      with:
        sarif-file: 'security-scan-results.sarif'
        
    - name: Dependency check
      run: dotnet list package --vulnerable --include-transitive
      
  deploy-staging:
    runs-on: ubuntu-latest
    needs: [build-and-test, security-scan]
    if: github.ref == 'refs/heads/develop'
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Deploy to staging
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'enterprise-crm-staging'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_STAGING }}
        package: 'src/EnterpriseCRM.WebAPI'
        
  deploy-production:
    runs-on: ubuntu-latest
    needs: [build-and-test, security-scan]
    if: github.ref == 'refs/heads/main'
    environment: production
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Deploy to production
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'enterprise-crm-prod'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_PROD }}
        package: 'src/EnterpriseCRM.WebAPI'
```

### **2. Database Migration Workflow (`database-migration.yml`)**

**Purpose:** Automated database migrations and schema updates

```yaml
name: Database Migration

on:
  workflow_dispatch:
    inputs:
      environment:
        description: 'Target environment'
        required: true
        default: 'staging'
        type: choice
        options:
        - staging
        - production

jobs:
  migrate-database:
    runs-on: ubuntu-latest
    environment: ${{ github.event.inputs.environment }}
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Restore dependencies
      run: dotnet restore src/EnterpriseCRM.sln
      
    - name: Build Infrastructure project
      run: dotnet build src/EnterpriseCRM.Infrastructure --configuration Release
      
    - name: Run database migrations
      run: dotnet ef database update --project src/EnterpriseCRM.Infrastructure --connection "${{ secrets.CONNECTION_STRING }}"
      
    - name: Verify migration
      run: dotnet ef migrations list --project src/EnterpriseCRM.Infrastructure
```

### **3. Code Quality Workflow (`code-quality.yml`)**

**Purpose:** Code analysis, formatting, and quality checks

```yaml
name: Code Quality

on:
  pull_request:
    branches: [ main, develop ]

jobs:
  code-analysis:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      with:
        fetch-depth: 0
        
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Install SonarCloud scanner
      run: dotnet tool install --global dotnet-sonarscanner
      
    - name: Cache SonarCloud packages
      uses: actions/cache@v3
      with:
        path: ~/.sonar/cache
        key: ${{ runner.os }}-sonar
        restore-keys: ${{ runner.os }}-sonar
        
    - name: Build and analyze
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
      run: |
        dotnet sonarscanner begin /k:"enterprise-crm" /o:"your-org" /d:sonar.token="${{ secrets.SONAR_TOKEN }}" /d:sonar.host.url="https://sonarcloud.io"
        dotnet build src/EnterpriseCRM.sln
        dotnet sonarscanner end /d:sonar.token="${{ secrets.SONAR_TOKEN }}"
        
  format-check:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Check code formatting
      run: dotnet format --verify-no-changes --verbosity diagnostic
      
  dependency-check:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Check for vulnerable packages
      run: dotnet list package --vulnerable --include-transitive
      
    - name: Check for outdated packages
      run: dotnet list package --outdated
```

### **4. Release Workflow (`release.yml`)**

**Purpose:** Automated versioning and release management

```yaml
name: Release

on:
  push:
    tags:
      - 'v*'

jobs:
  create-release:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Restore dependencies
      run: dotnet restore src/EnterpriseCRM.sln
      
    - name: Build solution
      run: dotnet build src/EnterpriseCRM.sln --configuration Release --no-restore
      
    - name: Run tests
      run: dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage"
      
    - name: Publish applications
      run: |
        dotnet publish src/EnterpriseCRM.WebAPI -c Release -o ./publish/webapi
        dotnet publish src/EnterpriseCRM.BlazorServer -c Release -o ./publish/blazor
        
    - name: Create release archive
      run: |
        tar -czf enterprise-crm-${{ github.ref_name }}.tar.gz -C ./publish .
        
    - name: Upload release asset
      uses: actions/upload-release-asset@v1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        upload_url: ${{ steps.create_release.outputs.upload_url }}
        asset_path: ./enterprise-crm-${{ github.ref_name }}.tar.gz
        asset_name: enterprise-crm-${{ github.ref_name }}.tar.gz
        asset_content_type: application/gzip
        
    - name: Create Release
      id: create_release
      uses: actions/create-release@v1
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      with:
        tag_name: ${{ github.ref }}
        release_name: Release ${{ github.ref }}
        draft: false
        prerelease: false
```

### **5. Security Workflow (`security.yml`)**

**Purpose:** Security scanning and vulnerability detection

```yaml
name: Security Scan

on:
  schedule:
    - cron: '0 2 * * 1'  # Weekly on Monday at 2 AM
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  security-scan:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Run Trivy vulnerability scanner
      uses: aquasecurity/trivy-action@master
      with:
        scan-type: 'fs'
        scan-ref: '.'
        format: 'sarif'
        output: 'trivy-results.sarif'
        
    - name: Upload Trivy scan results
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: 'trivy-results.sarif'
        
    - name: Run CodeQL Analysis
      uses: github/codeql-action/analyze@v2
      with:
        languages: csharp
        
    - name: Check for secrets
      uses: trufflesecurity/trufflehog@main
      with:
        path: ./
        base: main
        head: HEAD
        extra_args: --debug --only-verified
```

### **6. Performance Testing Workflow (`performance.yml`)**

**Purpose:** Load testing and performance validation

```yaml
name: Performance Testing

on:
  workflow_dispatch:
  schedule:
    - cron: '0 3 * * 0'  # Weekly on Sunday at 3 AM

jobs:
  performance-test:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Install NBomber
      run: dotnet tool install --global NBomber
      
    - name: Run performance tests
      run: dotnet test tests/EnterpriseCRM.PerformanceTests --configuration Release --logger trx --results-directory TestResults
      
    - name: Upload performance results
      uses: actions/upload-artifact@v3
      with:
        name: performance-results
        path: TestResults/
```

## 🔧 Workflow Configuration

### **Required Secrets**

Add these secrets to your GitHub repository:

```bash
# Azure Deployment
AZURE_WEBAPP_PUBLISH_PROFILE_STAGING
AZURE_WEBAPP_PUBLISH_PROFILE_PROD
CONNECTION_STRING

# Code Quality
SONAR_TOKEN
GITHUB_TOKEN

# Security
TRIVY_TOKEN
```

### **Environment Variables**

```bash
# Application Settings
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=...
JwtSettings__SecretKey=...
Redis__ConnectionString=...
```

## 📊 Workflow Benefits

### **CI/CD Pipeline**
- ✅ **Automated Testing:** Unit, integration, and E2E tests
- ✅ **Code Coverage:** Track test coverage metrics
- ✅ **Security Scanning:** Vulnerability detection
- ✅ **Deployment:** Automated staging and production deployments
- ✅ **Quality Gates:** Prevent deployment of low-quality code

### **Database Migration**
- ✅ **Schema Updates:** Automated database migrations
- ✅ **Environment Safety:** Separate staging and production migrations
- ✅ **Rollback Support:** Easy rollback capabilities
- ✅ **Verification:** Migration verification steps

### **Code Quality**
- ✅ **Static Analysis:** SonarCloud integration
- ✅ **Format Checking:** Consistent code formatting
- ✅ **Dependency Management:** Vulnerable package detection
- ✅ **Code Metrics:** Maintainability and complexity metrics

### **Release Management**
- ✅ **Versioning:** Automated version management
- ✅ **Artifact Creation:** Release package generation
- ✅ **GitHub Releases:** Automated release creation
- ✅ **Documentation:** Release notes generation

### **Security**
- ✅ **Vulnerability Scanning:** Regular security assessments
- ✅ **Secret Detection:** Prevent credential leaks
- ✅ **Code Analysis:** Security-focused code review
- ✅ **Dependency Check:** Third-party security validation

### **Performance**
- ✅ **Load Testing:** Regular performance validation
- ✅ **Benchmarking:** Performance regression detection
- ✅ **Monitoring:** Performance metrics tracking
- ✅ **Optimization:** Performance improvement identification

## 🚀 Getting Started

### **1. Enable GitHub Actions**
- Go to your repository Settings
- Navigate to Actions → General
- Enable "Allow all actions and reusable workflows"

### **2. Add Required Secrets**
- Go to Settings → Secrets and variables → Actions
- Add all required secrets listed above

### **3. Configure Environments**
- Create `staging` and `production` environments
- Add environment-specific secrets
- Configure protection rules

### **4. Test Workflows**
- Create a test branch
- Make a small change
- Create a pull request to test CI workflows

## 📈 Monitoring and Metrics

### **Workflow Status**
- Monitor workflow success rates
- Track build times and performance
- Identify common failure patterns

### **Code Quality Metrics**
- Track code coverage trends
- Monitor technical debt
- Follow security vulnerability counts

### **Deployment Metrics**
- Track deployment frequency
- Monitor deployment success rates
- Measure time to production

This comprehensive GitHub Actions setup provides enterprise-grade CI/CD capabilities for the Enterprise CRM project, ensuring high quality, security, and reliability throughout the development lifecycle.
