# Quick Start: Scaffold from Existing Database & Deploy

Based on your current Azure deployment:
- **Live Site:** http://bcshotels.azurewebsites.net
- **SQL Server:** teyfcq76qi.database.windows.net
- **Database:** bcshotels_db (created 2015, ~10 years old)
- **Current Tier:** Free App Service (no deployment slots)

## Strategy

Create a **new** .NET 10 App Service that uses your **existing** database, running side-by-side with the old app.

---

## Step 1: Create New .NET 10 Project (5 minutes)

```bash
# Navigate to repository
cd /home/user/BCSHotels

# Create new solution folder
mkdir BCSHotels.Modern
cd BCSHotels.Modern

# Create Razor Pages web app with .NET 10
dotnet new webapp -n BCSHotels.Web -f net10.0

# Create class library for data access
dotnet new classlib -n BCSHotels.Data -f net10.0

# Create solution
dotnet new sln -n BCSHotels.Modern

# Add projects to solution
dotnet sln add BCSHotels.Web/BCSHotels.Web.csproj
dotnet sln add BCSHotels.Data/BCSHotels.Data.csproj

# Add project reference
cd BCSHotels.Web
dotnet add reference ../BCSHotels.Data/BCSHotels.Data.csproj
```

---

## Step 2: Install Required Packages (2 minutes)

```bash
# In BCSHotels.Data
cd ../BCSHotels.Data
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.0
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 10.0.0

# In BCSHotels.Web
cd ../BCSHotels.Web
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 10.0.0
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 10.0.0
dotnet add package Microsoft.AspNetCore.Identity.UI --version 10.0.0
dotnet add package Microsoft.ApplicationInsights.AspNetCore --version 2.22.0

# Install EF Core CLI tools globally
dotnet tool install --global dotnet-ef --version 10.0.0
```

---

## Step 3: Scaffold from Existing Azure SQL Database (10 minutes)

**You'll need:**
- SQL Server name: `teyfcq76qi.database.windows.net`
- Database name: `bcshotels_db`
- SQL username and password (or use Azure AD authentication)

### Option A: Using SQL Authentication

```bash
cd BCSHotels.Data

# Scaffold all tables from existing database
dotnet ef dbcontext scaffold \
  "Server=tcp:teyfcq76qi.database.windows.net,1433;Database=bcshotels_db;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --output-dir Entities \
  --context-dir . \
  --context ApplicationDbContext \
  --data-annotations \
  --force
```

### Option B: Using Azure AD Interactive Authentication (Recommended)

```bash
cd BCSHotels.Data

# This will open a browser for Azure authentication
dotnet ef dbcontext scaffold \
  "Server=tcp:teyfcq76qi.database.windows.net,1433;Database=bcshotels_db;Authentication=Active Directory Interactive;Encrypt=True;" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --output-dir Entities \
  --context-dir . \
  --context ApplicationDbContext \
  --data-annotations \
  --force
```

**This will generate:**
- `ApplicationDbContext.cs` - DbContext matching your database
- `Entities/` folder - C# classes for every table in your database

---

## Step 4: Review Generated Models (5 minutes)

After scaffolding, review the generated files:

```bash
# List generated entity classes
ls -la BCSHotels.Data/Entities/

# View the DbContext
cat BCSHotels.Data/ApplicationDbContext.cs
```

**Expected entities based on typical hotel booking system:**
- Hotels
- Rooms
- Bookings
- AspNetUsers (Identity tables)
- AspNetRoles
- etc.

---

## Step 5: Configure Web Application (10 minutes)

### 5.1 Update Program.cs

**BCSHotels.Web/Program.cs:**
```csharp
using BCSHotels.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity if you have AspNetUsers table
// Uncomment if your database has Identity tables:
// builder.Services.AddDefaultIdentity<IdentityUser>(options => {
//     options.SignIn.RequireConfirmedAccount = false;
// })
// .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Uncomment if using Identity:
// app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
```

### 5.2 Update appsettings.json

**BCSHotels.Web/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=bcshotels_db-local;Trusted_Connection=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**BCSHotels.Web/appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=bcshotels_db-local;Trusted_Connection=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

**BCSHotels.Web/appsettings.Production.json:** (create this file)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

---

## Step 6: Create Initial Pages (15 minutes)

### 6.1 Update Home Page

**BCSHotels.Web/Pages/Index.cshtml.cs:**
```csharp
using BCSHotels.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BCSHotels.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Adjust this based on your actual entity names from scaffolding
    // Example: public List<Hotel> FeaturedHotels { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Example if you have a Hotels table:
        // FeaturedHotels = await _context.Hotels
        //     .OrderByDescending(h => h.Rating)
        //     .Take(6)
        //     .ToListAsync();

        _logger.LogInformation("Home page loaded");
    }
}
```

### 6.2 Test Locally

```bash
cd BCSHotels.Web

# Update database to local (optional - for development)
# Skip this if you want to test against production database directly

# Run the application
dotnet run

# Open browser to: https://localhost:5001
```

---

## Step 7: Create Azure Resources (10 minutes)

### 7.1 Create App Service Plan

```bash
# Create a Linux App Service Plan (Basic B1 - $12.41/month)
az appservice plan create \
  --name bcshotels-modern-plan \
  --resource-group Default-Web-WestUS \
  --location westus \
  --is-linux \
  --sku B1

# Or Windows if you prefer (same price):
az appservice plan create \
  --name bcshotels-modern-plan \
  --resource-group Default-Web-WestUS \
  --location westus \
  --sku B1
```

### 7.2 Create Web App

```bash
# For Linux:
az webapp create \
  --name bcshotels-modern \
  --resource-group Default-Web-WestUS \
  --plan bcshotels-modern-plan \
  --runtime "DOTNET|10.0"

# For Windows:
az webapp create \
  --name bcshotels-modern \
  --resource-group Default-Web-WestUS \
  --plan bcshotels-modern-plan \
  --runtime "DOTNET:10"
```

### 7.3 Configure Connection String in Azure

```bash
# Set connection string as App Service configuration
az webapp config connection-string set \
  --name bcshotels-modern \
  --resource-group Default-Web-WestUS \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:teyfcq76qi.database.windows.net,1433;Database=bcshotels_db;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;"
```

**Or use Azure Portal:**
1. Go to Azure Portal → bcshotels-modern → Configuration
2. Connection strings → New connection string
3. Name: `DefaultConnection`
4. Value: `Server=tcp:teyfcq76qi.database.windows.net,1433;Database=bcshotels_db;...`
5. Type: SQLAzure

### 7.4 Configure Application Settings

```bash
# Enable HTTPS only
az webapp update \
  --name bcshotels-modern \
  --resource-group Default-Web-WestUS \
  --https-only true

# Enable detailed error messages (for initial deployment)
az webapp config appsettings set \
  --name bcshotels-modern \
  --resource-group Default-Web-WestUS \
  --settings ASPNETCORE_ENVIRONMENT=Production
```

---

## Step 8: Deploy to Azure (10 minutes)

### Option A: Deploy from VS Code (Easiest)

1. **Install Azure App Service Extension**
   - Open VS Code
   - Install "Azure App Service" extension

2. **Sign in to Azure**
   - Click Azure icon in sidebar
   - Sign in with your account

3. **Deploy**
   - Right-click `BCSHotels.Web` folder
   - Select "Deploy to Web App..."
   - Choose `bcshotels-modern`
   - Confirm deployment

4. **Browse**
   - After deployment, click "Browse Website"
   - Your app opens at: https://bcshotels-modern.azurewebsites.net

### Option B: Deploy using Azure CLI

```bash
cd BCSHotels.Web

# Publish the app
dotnet publish -c Release -o ./publish

# Zip the published files
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to Azure
az webapp deploy \
  --resource-group Default-Web-WestUS \
  --name bcshotels-modern \
  --src-path deploy.zip \
  --type zip

# Browse the site
az webapp browse \
  --name bcshotels-modern \
  --resource-group Default-Web-WestUS
```

### Option C: Set up GitHub Actions (Recommended for CI/CD)

**Create `.github/workflows/azure-deploy.yml`:**

```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: bcshotels-modern
  DOTNET_VERSION: '10.0.x'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Set up .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Build
      run: dotnet build BCSHotels.Modern/BCSHotels.Web/BCSHotels.Web.csproj --configuration Release

    - name: Publish
      run: dotnet publish BCSHotels.Modern/BCSHotels.Web/BCSHotels.Web.csproj --configuration Release --output ./publish

    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

**Get Publish Profile:**
```bash
# Download publish profile
az webapp deployment list-publishing-profiles \
  --name bcshotels-modern \
  --resource-group Default-Web-WestUS \
  --xml > publish-profile.xml

# Add contents to GitHub Secrets as AZURE_WEBAPP_PUBLISH_PROFILE
```

---

## Step 9: Configure Database Firewall (if needed)

If you get connection errors, add Azure services to SQL firewall:

```bash
# Allow Azure services to access SQL Server
az sql server firewall-rule create \
  --resource-group Default-SQL-WestUS \
  --server teyfcq76qi \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

---

## Step 10: Verify Deployment (5 minutes)

1. **Browse to new site:**
   - https://bcshotels-modern.azurewebsites.net

2. **Check Application Insights (if configured):**
   - Azure Portal → bcshotels-modern → Application Insights

3. **Check Logs:**
   ```bash
   az webapp log tail \
     --name bcshotels-modern \
     --resource-group Default-Web-WestUS
   ```

4. **Compare with old site:**
   - Old: http://bcshotels.azurewebsites.net
   - New: https://bcshotels-modern.azurewebsites.net

---

## Architecture Overview

```
┌─────────────────────────────────────────────┐
│         Both Apps Share Same Database        │
├─────────────────────────────────────────────┤
│                                             │
│  ┌────────────────┐    ┌─────────────────┐ │
│  │  Old App       │    │  New App        │ │
│  │  .NET 4.5.1    │    │  .NET 10        │ │
│  │  ASP.NET MVC   │    │  Razor Pages    │ │
│  │  Free Tier     │    │  Basic B1       │ │
│  └────────┬───────┘    └────────┬────────┘ │
│           │                     │          │
│           └──────────┬──────────┘          │
│                      │                     │
│           ┌──────────▼──────────┐          │
│           │   Azure SQL         │          │
│           │   bcshotels_db      │          │
│           │   Basic Tier        │          │
│           └─────────────────────┘          │
│                                             │
└─────────────────────────────────────────────┘
```

---

## Cost Breakdown

| Resource | Tier | Monthly Cost |
|----------|------|--------------|
| Azure SQL Database | Basic | $4.99 |
| Old App Service | Free | $0 |
| **New App Service** | **Basic B1** | **$12.41** |
| **Total** | | **~$17.40/month** |

---

## Next Steps After Deployment

1. **Test thoroughly:**
   - Browse all pages
   - Test database connectivity
   - Check logs for errors

2. **Implement actual pages:**
   - Use scaffolded entities to build pages
   - Hotels list, details, booking pages
   - User authentication

3. **Set up monitoring:**
   - Enable Application Insights
   - Configure alerts

4. **Performance testing:**
   - Compare with old app
   - Optimize queries if needed

5. **Plan cutover:**
   - When ready, update DNS or use Traffic Manager
   - Redirect http://bcshotels.azurewebsites.net to new app
   - Decommission old app

---

## Troubleshooting

### Can't connect to database
```bash
# Check firewall rules
az sql server firewall-rule list \
  --resource-group Default-SQL-WestUS \
  --server teyfcq76qi

# Add your IP if needed
az sql server firewall-rule create \
  --resource-group Default-SQL-WestUS \
  --server teyfcq76qi \
  --name AllowMyIP \
  --start-ip-address YOUR_IP \
  --end-ip-address YOUR_IP
```

### Scaffolding fails
- Ensure EF Core tools are installed: `dotnet tool list -g`
- Check SQL credentials are correct
- Verify database exists and is accessible

### Deployment fails
- Check App Service logs: `az webapp log tail`
- Verify connection string is set in Azure
- Ensure .NET 10 runtime is selected

### Site shows error after deployment
- Check Application Insights or logs
- Verify connection string
- Ensure database is accessible from Azure

---

## Summary

You now have:
- ✅ New .NET 10 app with models from your existing database
- ✅ Deployed to Azure at https://bcshotels-modern.azurewebsites.net
- ✅ Connected to existing SQL database
- ✅ Old app still running at http://bcshotels.azurewebsites.net
- ✅ Both apps sharing same data

**Total time:** ~1-2 hours
**Total cost:** ~$17/month

Next: Build out the Razor Pages using the scaffolded entities!
