# Azure Deployment Migration Guide: .NET Framework 4.5.1 → .NET 10

## Executive Summary

This document analyzes the current .NET Framework 4.5.1 deployment configuration for the BCSHotels application and provides a comprehensive guide for migrating to .NET 10 Azure deployment.

**Current State:** The main branch contains .NET Framework 4.5.1 applications configured for IIS deployment but **no explicit Azure deployment artifacts** were found.

**Target State:** .NET 10 applications deployed to Azure with modern CI/CD pipelines.

---

## Current Deployment Configuration Analysis

### Found Artifacts

#### 1. Web.config (.NET Framework 4.5.1)

**Location:** `BCSHotels/Web.config`

**Key .NET 4.5.1 Specific Configurations:**

```xml
<!-- Lines 27-28: Explicit .NET Framework 4.5.1 targeting -->
<compilation debug="true" targetFramework="4.5.1" />
<httpRuntime targetFramework="4.5.1" />
```

**IIS-Specific Modules:**
```xml
<!-- Lines 30-38: IIS modules for Elmah error logging -->
<httpModules>
  <add name="ErrorLog" type="Elmah.ErrorLogModule, Elmah" />
  <add name="ErrorMail" type="Elmah.ErrorMailModule, Elmah" />
  <add name="ErrorFilter" type="Elmah.ErrorFilterModule, Elmah" />
</httpModules>
<system.webServer>
  <modules>
    <remove name="FormsAuthentication" />
    <add name="ErrorLog" type="Elmah.ErrorLogModule, Elmah" preCondition="managedHandler" />
    <!-- ... -->
  </modules>
</system.webServer>
```

**Entity Framework 6 Configuration:**
```xml
<!-- Lines 84-93: EF6 using LocalDB -->
<entityFramework>
  <defaultConnectionFactory type="System.Data.Entity.Infrastructure.LocalDbConnectionFactory, EntityFramework">
    <parameters>
      <parameter value="mssqllocaldb" />
    </parameters>
  </defaultConnectionFactory>
</entityFramework>
```

**Connection String:**
```xml
<!-- Line 17: LocalDB connection -->
<add name="DefaultConnection"
     connectionString="Data Source=(LocalDb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\BCSHotelsDb.mdf;Initial Catalog=BCSHotelsDb;Integrated Security=True;MultipleActiveResultSets=True;"
     providerName="System.Data.SqlClient" />
```

#### 2. Web.Release.config (Transform File)

**Location:** `BCSHotels/Web.Release.config`

**Purpose:** XML transformations for production deployment
- Removes debug attribute from compilation
- Currently has only minimal transformations (template structure)
- **Would typically contain:** Production connection strings, Azure-specific settings

#### 3. BCSHotels.csproj (Project File)

**Location:** `BCSHotels/BCSHotels.csproj`

**Key Configurations:**
```xml
<!-- Lines 16-24: .NET Framework 4.5.1 and IIS Express -->
<TargetFrameworkVersion>v4.5.1</TargetFrameworkVersion>
<UseIISExpress>true</UseIISExpress>
<IISExpressSSLPort>44304</IISExpressSSLPort>
```

**Deployment Method:** Configured for IIS Express (local development), no publish profiles found

#### 4. .gitpod.Dockerfile (Development Environment)

**Location:** `.gitpod.Dockerfile`

**Configuration:** Installs .NET Core 3.1 SDK (not .NET Framework)
- **Note:** This is for the newer projects (RazorPagesMovie, FineHotels), not BCSHotels

---

## Missing Azure Deployment Artifacts

### What's NOT in the Repository

The following typical Azure deployment artifacts are **not present** in the main branch:

1. ❌ **Publish Profiles** (*.pubxml)
   - Location: `Properties/PublishProfiles/`
   - Purpose: Visual Studio publish settings for Azure App Service

2. ❌ **Azure Pipelines** (azure-pipelines.yml)
   - Purpose: Azure DevOps CI/CD configuration

3. ❌ **GitHub Actions** (.github/workflows/*.yml)
   - Purpose: GitHub-based CI/CD

4. ❌ **ARM Templates** (*.json)
   - Purpose: Infrastructure as Code for Azure resources

5. ❌ **Bicep Files** (*.bicep)
   - Purpose: Modern Azure IaC alternative to ARM templates

6. ❌ **Docker Support**
   - No Dockerfile for BCSHotels
   - No container deployment configuration

7. ❌ **Azure-specific Configuration Files**
   - No azure.json or .azure folder
   - No Azure Functions configurations

### Implications

**Current Deployment Method:** Likely one of:
- Manual deployment via Visual Studio "Publish" feature
- Manual FTP/WebDeploy to IIS server
- Local IIS hosting
- Not currently deployed to production

**For Migration:** We need to create these artifacts from scratch for .NET 10

---

## .NET Framework 4.5.1 vs .NET 10 Deployment Differences

### Deployment Model Comparison

| Aspect | .NET Framework 4.5.1 | .NET 10 |
|--------|---------------------|---------|
| **Runtime** | Windows-only, system-installed | Cross-platform, self-contained or framework-dependent |
| **Web Server** | IIS required | Kestrel (can use IIS as reverse proxy) |
| **Configuration** | Web.config (XML) | appsettings.json + environment variables |
| **Deployment** | WebDeploy, FTP, XCopy | Docker, zip deploy, CI/CD pipelines |
| **Azure Service** | Azure App Service (Windows) | Azure App Service (Windows/Linux), Container Apps, AKS |
| **Database** | Entity Framework 6, LocalDB | EF Core, Azure SQL |

---

## Migration Strategy for Azure Deployment

### Phase 1: Update Existing Deployment (If Currently on Azure)

If the .NET Framework 4.5.1 app is currently deployed to Azure App Service, these are the typical artifacts that exist but aren't checked into source control:

#### Typical Azure App Service Configuration (Framework 4.5.1)

**Azure Portal Settings:**
- **Runtime Stack:** ASP.NET V4.8 (or earlier)
- **Platform:** Windows
- **.NET Framework Version:** 4.8
- **Platform:** 32-bit or 64-bit
- **Always On:** Enabled
- **ARR Affinity:** Enabled (for session state)

**Application Settings (Environment Variables):**
```
WEBSITE_NODE_DEFAULT_VERSION=6.9.1
MSDEPLOY_RENAME_LOCKED_FILES=1
```

**Connection Strings:**
- Stored in Azure Portal (not in Web.config)
- Type: SQLAzure
- Connection string pointing to Azure SQL Database

**Deployment Method:**
- Visual Studio Publish (creates .pubxml locally, not committed)
- Or Azure DevOps with MSBuild tasks
- Or FTP/WebDeploy credentials

#### If Currently Deployed: Migration Path

**Option A: Side-by-Side Deployment (Recommended)**
1. Keep existing .NET Framework 4.5.1 app running
2. Create new Azure App Service for .NET 10 version
3. Migrate and test in parallel
4. Switch over when ready
5. Decommission old app

**Option B: In-Place Upgrade**
1. Deploy .NET 10 app to same App Service
2. Change runtime stack to .NET 10
3. Higher risk, requires rollback plan

---

### Phase 2: Create Modern Deployment Artifacts for .NET 10

After migrating code to .NET 10, create these deployment artifacts:

#### 1. Azure App Service Configuration (appsettings.json)

**Create:** `appsettings.json` and `appsettings.Production.json`

```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(LocalDb)\\MSSQLLocalDB;Database=BCSHotelsDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

```json
// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": ""  // Override with Azure App Service connection string
  }
}
```

**Migration Notes:**
- Replace Web.config `<appSettings>` with appsettings.json
- Replace Web.config `<connectionStrings>` with appsettings.json
- Use Azure App Service Application Settings for secrets

#### 2. GitHub Actions Workflow (Recommended)

**Create:** `.github/workflows/azure-deploy.yml`

```yaml
name: Deploy to Azure App Service

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: bcshotels-app    # Set to your Azure Web App name
  DOTNET_VERSION: '10.0.x'            # .NET 10
  WORKING_DIRECTORY: './BCSHotels'    # Path to your project

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Set up .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Restore dependencies
      run: dotnet restore ${{ env.WORKING_DIRECTORY }}

    - name: Build
      run: dotnet build ${{ env.WORKING_DIRECTORY }} --configuration Release --no-restore

    - name: Test
      run: dotnet test ${{ env.WORKING_DIRECTORY }} --configuration Release --no-build --verbosity normal

    - name: Publish
      run: dotnet publish ${{ env.WORKING_DIRECTORY }} --configuration Release --no-build --output ./publish

    - name: Upload artifact for deployment job
      uses: actions/upload-artifact@v4
      with:
        name: dotnet-app
        path: ./publish

  deploy:
    runs-on: ubuntu-latest
    needs: build
    environment:
      name: 'Production'
      url: ${{ steps.deploy-to-webapp.outputs.webapp-url }}

    steps:
    - name: Download artifact from build job
      uses: actions/download-artifact@v4
      with:
        name: dotnet-app

    - name: Deploy to Azure Web App
      id: deploy-to-webapp
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: .

```

**Setup Requirements:**
1. Create Azure App Service with .NET 10 runtime
2. Download publish profile from Azure Portal
3. Add as GitHub Secret: `AZURE_WEBAPP_PUBLISH_PROFILE`
4. Update `AZURE_WEBAPP_NAME` in workflow

#### 3. Azure Pipelines (Alternative to GitHub Actions)

**Create:** `azure-pipelines.yml`

```yaml
trigger:
  branches:
    include:
    - main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  dotnetSdkVersion: '10.x'
  workingDirectory: '$(System.DefaultWorkingDirectory)/BCSHotels'
  azureSubscription: 'YOUR_AZURE_SUBSCRIPTION_NAME'
  webAppName: 'bcshotels-app'

stages:
- stage: Build
  displayName: 'Build Stage'
  jobs:
  - job: Build
    displayName: 'Build Job'
    steps:
    - task: UseDotNet@2
      displayName: 'Use .NET SDK $(dotnetSdkVersion)'
      inputs:
        version: '$(dotnetSdkVersion)'

    - task: DotNetCoreCLI@2
      displayName: 'Restore NuGet Packages'
      inputs:
        command: 'restore'
        projects: '$(workingDirectory)/**/*.csproj'

    - task: DotNetCoreCLI@2
      displayName: 'Build Project'
      inputs:
        command: 'build'
        projects: '$(workingDirectory)/**/*.csproj'
        arguments: '--configuration $(buildConfiguration) --no-restore'

    - task: DotNetCoreCLI@2
      displayName: 'Run Tests'
      inputs:
        command: 'test'
        projects: '$(workingDirectory)/**/*Tests.csproj'
        arguments: '--configuration $(buildConfiguration) --no-build'

    - task: DotNetCoreCLI@2
      displayName: 'Publish Application'
      inputs:
        command: 'publish'
        publishWebProjects: false
        projects: '$(workingDirectory)/**/*.csproj'
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
        zipAfterPublish: true

    - task: PublishBuildArtifacts@1
      displayName: 'Publish Artifacts'
      inputs:
        pathToPublish: '$(Build.ArtifactStagingDirectory)'
        artifactName: 'drop'

- stage: Deploy
  displayName: 'Deploy Stage'
  dependsOn: Build
  condition: succeeded()
  jobs:
  - deployment: DeployWeb
    displayName: 'Deploy to Azure App Service'
    environment: 'Production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            displayName: 'Deploy Azure Web App'
            inputs:
              azureSubscription: '$(azureSubscription)'
              appType: 'webApp'
              appName: '$(webAppName)'
              package: '$(Pipeline.Workspace)/drop/**/*.zip'
              deploymentMethod: 'auto'
```

**Setup Requirements:**
1. Create Azure Service Connection in Azure DevOps
2. Update `azureSubscription` and `webAppName`
3. Create Azure App Service with .NET 10 runtime

#### 4. Dockerfile (For Container Deployment)

**Create:** `BCSHotels/Dockerfile`

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["BCSHotels/BCSHotels.csproj", "BCSHotels/"]
COPY ["BCSHotelsDomain/BCSHotelsDomain.csproj", "BCSHotelsDomain/"]

# Restore dependencies
RUN dotnet restore "BCSHotels/BCSHotels.csproj"

# Copy all source files
COPY . .

# Build and publish
WORKDIR "/src/BCSHotels"
RUN dotnet build "BCSHotels.csproj" -c Release -o /app/build
RUN dotnet publish "BCSHotels.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published app
COPY --from=build /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BCSHotels.dll"]
```

**Create:** `.dockerignore`

```
**/.git
**/.gitignore
**/.vs
**/.vscode
**/bin
**/obj
**/out
**/TestResults
**/*.md
**/.dockerignore
**/Dockerfile*
**/packages
**/node_modules
```

**Deployment Options:**
- Azure Container Apps
- Azure Kubernetes Service (AKS)
- Azure App Service (Linux with Docker)
- Azure Container Registry + App Service

#### 5. Bicep Infrastructure as Code

**Create:** `infrastructure/main.bicep`

```bicep
@description('Name of the application')
param appName string = 'bcshotels'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Environment name')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string = 'dev'

@description('SKU for App Service Plan')
param appServicePlanSku string = 'B1'

var webAppName = '${appName}-web-${environment}'
var appServicePlanName = '${appName}-plan-${environment}'
var sqlServerName = '${appName}-sql-${environment}-${uniqueString(resourceGroup().id)}'
var sqlDatabaseName = '${appName}Db'
var applicationInsightsName = '${appName}-insights-${environment}'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: webAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
      ]
      connectionStrings: [
        {
          name: 'DefaultConnection'
          connectionString: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Authentication=Active Directory Default;Encrypt=True;'
          type: 'SQLAzure'
        }
      ]
    }
    httpsOnly: true
  }
}

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: '' // Use Azure Key Vault reference or parameter
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// SQL Database
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648 // 2GB
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

// Firewall rule to allow Azure services
resource sqlFirewallRule 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Application Insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    RetentionInDays: 90
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Grant Web App access to SQL Database using Managed Identity
resource sqlAzureADAdmin 'Microsoft.Sql/servers/administrators@2023-05-01-preview' = {
  parent: sqlServer
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: webApp.name
    sid: webApp.identity.principalId
    tenantId: subscription().tenantId
  }
}

output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppName string = webApp.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
```

**Deploy with Azure CLI:**
```bash
# Create resource group
az group create --name rg-bcshotels-prod --location eastus

# Deploy infrastructure
az deployment group create \
  --resource-group rg-bcshotels-prod \
  --template-file infrastructure/main.bicep \
  --parameters environment=prod appServicePlanSku=P1v3
```

---

## Configuration Migration Checklist

### Web.config → appsettings.json + Azure App Settings

| Web.config Element | .NET 10 Equivalent | Notes |
|-------------------|-------------------|-------|
| `<appSettings>` | `appsettings.json` | Key-value pairs |
| `<connectionStrings>` | `appsettings.json` + Azure App Settings | Use Azure App Settings for production |
| `<system.web><compilation>` | Project file (.csproj) | Build configuration |
| `<system.web><httpRuntime>` | Not needed | Kestrel handles this |
| `<system.web><authentication>` | `Program.cs` middleware | ASP.NET Core Identity |
| `<system.web><customErrors>` | Exception middleware | `app.UseExceptionHandler()` |
| `<system.webServer><modules>` | Middleware in `Program.cs` | Custom middleware |
| `<entityFramework>` | EF Core configuration | `DbContext` configuration |
| Elmah (error logging) | ILogger + App Insights | Built-in logging |

### IIS Configuration → Kestrel + Azure App Service

| IIS Feature | .NET 10 Equivalent | Implementation |
|-------------|-------------------|----------------|
| Application Pool | App Service Plan | Azure resource |
| HTTP Modules | Middleware | `Program.cs` pipeline |
| URL Rewrite | Routing middleware | ASP.NET Core routing |
| Static Files | Static File Middleware | `app.UseStaticFiles()` |
| Authentication | Auth Middleware | `app.UseAuthentication()` |
| HTTPS Binding | HTTPS Redirection | `app.UseHttpsRedirection()` |
| Session State | Session Middleware | `app.UseSession()` |

### Database Migration

| .NET Framework 4.5.1 | .NET 10 | Migration Steps |
|---------------------|---------|-----------------|
| LocalDB | Azure SQL Database | 1. Create Azure SQL DB<br>2. Update connection string<br>3. Apply EF Core migrations |
| Entity Framework 6 | Entity Framework Core 10 | 1. Convert DbContext<br>2. Convert migrations<br>3. Update LINQ queries |
| SQL Server provider | EF Core SQL Server | Update NuGet package |

---

## Step-by-Step Migration Process

### Step 1: Prepare Azure Resources

#### Option A: Azure Portal (Manual)

1. **Create Resource Group**
   - Name: `rg-bcshotels-prod`
   - Region: East US (or your region)

2. **Create App Service Plan**
   - Name: `plan-bcshotels-prod`
   - OS: Linux
   - Pricing: B1 (Basic) or higher

3. **Create App Service**
   - Name: `bcshotels-web-prod`
   - Runtime: .NET 10
   - App Service Plan: Use existing (plan-bcshotels-prod)

4. **Create Azure SQL Database**
   - Server name: `sql-bcshotels-prod`
   - Database name: `BCSHotelsDb`
   - Pricing: Basic or Standard
   - Configure firewall: Allow Azure services

5. **Create Application Insights**
   - Name: `insights-bcshotels-prod`
   - Link to App Service

#### Option B: Azure CLI (Scripted)

```bash
#!/bin/bash

# Variables
RESOURCE_GROUP="rg-bcshotels-prod"
LOCATION="eastus"
APP_SERVICE_PLAN="plan-bcshotels-prod"
WEB_APP_NAME="bcshotels-web-prod"
SQL_SERVER="sql-bcshotels-prod"
SQL_DATABASE="BCSHotelsDb"
SQL_ADMIN="sqladmin"
SQL_PASSWORD="YourSecurePassword123!" # Use Azure Key Vault in production

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create App Service Plan (Linux, .NET 10)
az appservice plan create \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --is-linux \
  --sku B1

# Create Web App
az webapp create \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_SERVICE_PLAN \
  --runtime "DOTNET|10.0"

# Configure Web App settings
az webapp config set \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --always-on true \
  --min-tls-version 1.2 \
  --ftps-state Disabled

# Create SQL Server
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN \
  --admin-password $SQL_PASSWORD

# Create SQL Database
az sql db create \
  --name $SQL_DATABASE \
  --server $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --service-objective Basic

# Allow Azure services to access SQL Server
az sql server firewall-rule create \
  --server $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Get SQL connection string
SQL_CONNECTION=$(az sql db show-connection-string \
  --client ado.net \
  --server $SQL_SERVER \
  --name $SQL_DATABASE \
  --output tsv)

# Update connection string placeholders
SQL_CONNECTION="${SQL_CONNECTION/<username>/$SQL_ADMIN}"
SQL_CONNECTION="${SQL_CONNECTION/<password>/$SQL_PASSWORD}"

# Set connection string in App Service
az webapp config connection-string set \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$SQL_CONNECTION"

# Create Application Insights
az extension add --name application-insights
az monitor app-insights component create \
  --app insights-bcshotels-prod \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP \
  --application-type web

# Get App Insights connection string
APPINSIGHTS_CONNECTION=$(az monitor app-insights component show \
  --app insights-bcshotels-prod \
  --resource-group $RESOURCE_GROUP \
  --query connectionString \
  --output tsv)

# Set Application Insights in App Service
az webapp config appsettings set \
  --name $WEB_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="$APPINSIGHTS_CONNECTION"

echo "Azure resources created successfully!"
echo "Web App URL: https://$WEB_APP_NAME.azurewebsites.net"
```

#### Option C: Bicep/ARM Template (IaC - Recommended)

Use the Bicep template provided earlier in this document.

---

### Step 2: Configure Application for Azure

#### Update Connection String Handling

**Old (.NET Framework 4.5.1):**
```csharp
// In code
string connString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
```

**New (.NET 10):**
```csharp
// In Program.cs
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
builder.Configuration.AddEnvironmentVariables(); // Azure App Settings override

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

#### Add Application Insights

```bash
# Add NuGet package
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

```csharp
// In Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

#### Update Logging

**Remove:** Elmah

**Add:** Built-in logging + Application Insights

```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Information"
      }
    }
  }
}
```

---

### Step 3: Set Up CI/CD Pipeline

Choose one of the following:

#### Option A: GitHub Actions (Recommended)

1. **Create workflow file:** `.github/workflows/azure-deploy.yml` (see template above)
2. **Get publish profile:**
   ```bash
   az webapp deployment list-publishing-profiles \
     --name bcshotels-web-prod \
     --resource-group rg-bcshotels-prod \
     --xml
   ```
3. **Add GitHub Secret:**
   - Go to GitHub repo → Settings → Secrets → Actions
   - Add new secret: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Paste the XML from step 2
4. **Push to trigger deployment:**
   ```bash
   git push origin main
   ```

#### Option B: Azure DevOps Pipelines

1. **Create pipeline file:** `azure-pipelines.yml` (see template above)
2. **Create Service Connection:**
   - Azure DevOps → Project Settings → Service Connections
   - New Service Connection → Azure Resource Manager
   - Authenticate and select subscription
3. **Create Pipeline:**
   - Pipelines → New Pipeline → Azure Repos Git
   - Select repository and `azure-pipelines.yml`
4. **Run pipeline**

#### Option C: Azure App Service Deployment Center

1. Go to Azure Portal → App Service → Deployment Center
2. Choose source: GitHub or Azure Repos
3. Configure:
   - Organization
   - Repository
   - Branch
   - Build provider: GitHub Actions or Azure Pipelines
4. Save - auto-generates workflow/pipeline file

---

### Step 4: Database Migration

#### Migrate EF6 Migrations to EF Core

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# After converting to EF Core, create initial migration
dotnet ef migrations add InitialCreate --project BCSHotelsDomain --startup-project BCSHotels

# Review the migration, then apply to Azure SQL
dotnet ef database update --connection "YOUR_AZURE_SQL_CONNECTION_STRING"
```

#### Alternative: Generate SQL Scripts

```bash
# Generate SQL script
dotnet ef migrations script --output migration.sql

# Review and run manually in Azure SQL using Azure Data Studio or SSMS
```

---

### Step 5: Deploy and Test

#### Manual Deployment (First Time)

```bash
# Build and publish locally
dotnet publish BCSHotels/BCSHotels.csproj -c Release -o ./publish

# Deploy using Azure CLI
az webapp deploy \
  --resource-group rg-bcshotels-prod \
  --name bcshotels-web-prod \
  --src-path ./publish.zip \
  --type zip
```

#### Automated Deployment

```bash
# Push to main branch triggers pipeline
git add .
git commit -m "feat: migrate to .NET 10 with Azure deployment"
git push origin main
```

#### Verify Deployment

1. **Check App Service Logs:**
   ```bash
   az webapp log tail --name bcshotels-web-prod --resource-group rg-bcshotels-prod
   ```

2. **Browse Application:**
   ```bash
   az webapp browse --name bcshotels-web-prod --resource-group rg-bcshotels-prod
   ```

3. **Check Application Insights:**
   - Azure Portal → Application Insights → Live Metrics

---

## Troubleshooting Common Migration Issues

### Issue 1: Connection String Not Working

**Problem:** App can't connect to Azure SQL Database

**Solutions:**
- Verify firewall rules allow your App Service IP
- Check connection string format (different from LocalDB)
- Use Managed Identity instead of SQL auth (more secure)
- Enable "Allow Azure Services" in SQL Server firewall

### Issue 2: Runtime Errors (Missing Dependencies)

**Problem:** App crashes with "Could not load file or assembly"

**Solutions:**
- Ensure all dependencies are .NET 10 compatible
- Check .csproj has correct package versions
- Use `dotnet publish` not `dotnet build` for deployment
- Include runtime identifier if using self-contained deployment

### Issue 3: Configuration Not Loading

**Problem:** App can't read appsettings.json

**Solutions:**
- Ensure appsettings.json is copied to output (Copy to Output Directory: Copy if newer)
- Check JSON syntax is valid
- Use App Service Application Settings (override appsettings.json)
- Verify environment variable names match

### Issue 4: Performance Issues

**Problem:** App slower than on IIS

**Solutions:**
- Enable "Always On" in App Service (prevents cold starts)
- Scale up App Service Plan (B1 → P1v3 or higher)
- Add Application Insights for performance monitoring
- Review and optimize database queries (EF Core query translation may differ from EF6)

---

## Cost Optimization

### Development Environment

- **App Service Plan:** B1 Basic ($12.41/month)
- **Azure SQL:** Basic ($4.90/month)
- **Application Insights:** Free tier (5GB/month included)
- **Total:** ~$17/month

### Production Environment (Small-Medium Scale)

- **App Service Plan:** P1v3 Premium ($96.36/month)
- **Azure SQL:** S1 Standard ($30/month)
- **Application Insights:** Pay-as-you-go (~$10-50/month)
- **Total:** ~$136-176/month

### Cost-Saving Tips

1. Use B1/S1 tiers for dev/staging environments
2. Auto-shutdown dev resources after hours (Azure Automation)
3. Use Azure Reserved Instances for production (up to 72% savings)
4. Monitor Application Insights usage (can be expensive at scale)
5. Use Azure Cost Management alerts

---

## Security Best Practices

### 1. Managed Identity for Database Access

**Instead of SQL username/password in connection string:**

```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connString, sqlOptions =>
    {
        // Use Managed Identity authentication
        sqlOptions.UseAzureSqlDefaults();
    });
});
```

**Connection String:**
```
Server=tcp:sql-bcshotels-prod.database.windows.net,1433;Database=BCSHotelsDb;Authentication=Active Directory Default;Encrypt=True;
```

### 2. Store Secrets in Azure Key Vault

```bash
# Create Key Vault
az keyvault create \
  --name kv-bcshotels-prod \
  --resource-group rg-bcshotels-prod \
  --location eastus

# Add secrets
az keyvault secret set \
  --vault-name kv-bcshotels-prod \
  --name "ConnectionStrings--DefaultConnection" \
  --value "YOUR_CONNECTION_STRING"

# Grant App Service access
az keyvault set-policy \
  --name kv-bcshotels-prod \
  --object-id $(az webapp identity show --name bcshotels-web-prod --resource-group rg-bcshotels-prod --query principalId -o tsv) \
  --secret-permissions get list
```

```csharp
// In Program.cs
if (builder.Environment.IsProduction())
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}
```

### 3. Enable HTTPS Only

```bash
az webapp update \
  --name bcshotels-web-prod \
  --resource-group rg-bcshotels-prod \
  --https-only true
```

### 4. Configure CORS (If Using APIs)

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("https://yourdomain.com")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});
```

---

## Rollback Strategy

### If Deployment Fails

**Azure App Service has deployment slots (Standard tier and above):**

1. **Use Deployment Slots**
   ```bash
   # Create staging slot
   az webapp deployment slot create \
     --name bcshotels-web-prod \
     --resource-group rg-bcshotels-prod \
     --slot staging

   # Deploy to staging
   # Test thoroughly

   # Swap to production
   az webapp deployment slot swap \
     --name bcshotels-web-prod \
     --resource-group rg-bcshotels-prod \
     --slot staging
   ```

2. **Rollback** (if production has issues):
   ```bash
   # Swap back
   az webapp deployment slot swap \
     --name bcshotels-web-prod \
     --resource-group rg-bcshotels-prod \
     --slot staging
   ```

### If Need to Revert to .NET Framework 4.5.1

1. Keep old App Service running alongside new one
2. Use Traffic Manager to route traffic back to old version
3. Keep database compatible with both versions during transition period

---

## Summary

### Current State (Main Branch)
- ✅ .NET Framework 4.5.1 Web.config found
- ✅ IIS Express configuration identified
- ❌ No Azure deployment artifacts in repository
- ❌ No CI/CD pipelines configured

### Required Artifacts for .NET 10 Azure Deployment

**Essential:**
- [ ] `appsettings.json` + `appsettings.Production.json`
- [ ] GitHub Actions workflow OR Azure Pipelines YAML
- [ ] Azure App Service (Linux or Windows)
- [ ] Azure SQL Database
- [ ] Connection string configuration

**Recommended:**
- [ ] Dockerfile (for container deployment)
- [ ] Bicep/ARM templates (Infrastructure as Code)
- [ ] Application Insights integration
- [ ] Azure Key Vault for secrets
- [ ] Deployment slots for staging

**Optional:**
- [ ] Azure Front Door or Application Gateway
- [ ] Azure CDN for static assets
- [ ] Azure Redis Cache for session state
- [ ] Azure Service Bus or Event Grid for messaging

---

## Next Steps

1. ✅ **Review this migration plan**
2. **Choose deployment strategy:**
   - GitHub Actions (easiest)
   - Azure Pipelines (enterprise)
   - Bicep/IaC (most robust)
3. **Create Azure resources** (use provided scripts)
4. **Migrate BCSHotels code** to .NET 10 (see main migration plan)
5. **Create deployment artifacts** (workflows, Dockerfiles)
6. **Test deployment** to Azure dev/staging environment
7. **Deploy to production** with rollback plan ready

---

**Document Version:** 1.0
**Created:** 2026-01-01
**Author:** Claude (AI Assistant)
