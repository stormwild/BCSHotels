# .NET 10 Migration Plan for BCSHotels

## Executive Summary

This document outlines the strategy to migrate the BCSHotels codebase to .NET 10. The repository currently contains 7 projects across multiple .NET versions:
- **2 projects** on .NET Framework 4.5.1 (legacy)
- **1 project** on .NET Core 3.1 (intermediate)
- **4 projects** on .NET 8.0 (modern)

**Note:** As of January 2025, .NET 9 is the latest stable release. .NET 10 is expected in November 2026. This plan can be adapted for either .NET 9 (immediate) or .NET 10 (future).

---

## Current State Analysis

### Project Inventory

| Project | Current Version | Type | Complexity |
|---------|----------------|------|------------|
| BCSHotels | .NET Framework 4.5.1 | ASP.NET MVC 5 | **HIGH** |
| BCSHotelsDomain | .NET Framework 4.5.1 | Class Library | **MEDIUM** |
| RazorPagesMovie | .NET Core 3.1 | Razor Pages | **LOW** |
| FineHotelsApp | .NET 8.0 | ASP.NET Core | **LOW** |
| FineHotelsDb | .NET 8.0 | Class Library | **LOW** |
| FineHotelsDomain | .NET 8.0 | Class Library | **LOW** |
| FineHotelsWeb | .NET 8.0 | ASP.NET Core | **LOW** |

### Key Challenges

#### 1. **Legacy BCSHotels Projects** (Highest Complexity)
- **Framework:** .NET Framework 4.5.1 (2013)
- **Web Stack:** ASP.NET MVC 5 (non-Core)
- **Authentication:** ASP.NET Identity 2.1.0 with OWIN
- **ORM:** Entity Framework 6.1.1 (non-Core)
- **Dependencies:**
  - Old project format (.csproj XML)
  - packages.config for NuGet
  - Legacy ASP.NET stack (Razor, WebPages, etc.)

#### 2. **RazorPagesMovie** (Medium Complexity)
- **Framework:** .NET Core 3.1 (end-of-life)
- **Upgrade Path:** .NET Core 3.1 → .NET 8/9/10

#### 3. **FineHotels Projects** (Low Complexity)
- **Framework:** .NET 8.0 (modern)
- **Upgrade Path:** .NET 8 → .NET 10 (straightforward)

---

## Migration Strategy

### Option A: Phased Migration (Recommended)

Migrate projects in stages, prioritizing based on risk and business value.

#### **Phase 1: Low-Hanging Fruit** (1-2 weeks)
Upgrade modern projects with minimal changes.

1. **FineHotels Solution** (.NET 8.0 → .NET 10)
   - Update all 4 projects: FineHotelsApp, FineHotelsDb, FineHotelsDomain, FineHotelsWeb
   - Update target framework in .csproj files
   - Update NuGet packages to .NET 10 versions
   - Test and validate

2. **RazorPagesMovie** (.NET Core 3.1 → .NET 10)
   - Update target framework
   - Update Entity Framework Core packages
   - Address breaking changes (3.1 → 6 → 8 → 10)
   - Test and validate

#### **Phase 2: Legacy Migration Planning** (2-3 weeks)
Analyze and prepare BCSHotels migration.

1. **Code Analysis**
   - Identify incompatible APIs and patterns
   - Map Entity Framework 6 to EF Core
   - Map ASP.NET MVC 5 to ASP.NET Core MVC
   - Map ASP.NET Identity to ASP.NET Core Identity
   - Map OWIN middleware to ASP.NET Core middleware

2. **Dependency Audit**
   - Identify third-party packages without .NET Core/10 equivalents
   - Find alternatives for obsolete packages
   - Document breaking changes

3. **Database Considerations**
   - Review Entity Framework 6 migrations
   - Plan migration to EF Core migrations
   - Test database compatibility

#### **Phase 3: Legacy Migration Execution** (4-6 weeks)
Execute the migration for BCSHotels projects.

1. **BCSHotelsDomain** (Class Library)
   - Migrate to SDK-style project format
   - Update to .NET 10
   - Migrate Entity Framework 6 → EF Core 10
   - Update data models and configurations
   - Run unit tests

2. **BCSHotels** (Web Application)
   - Migrate to SDK-style project format
   - Update to .NET 10
   - Migrate ASP.NET MVC 5 → ASP.NET Core MVC
   - Migrate ASP.NET Identity → ASP.NET Core Identity
   - Migrate OWIN → ASP.NET Core middleware
   - Update views (Razor syntax changes)
   - Update configuration (web.config → appsettings.json)
   - Migrate authentication/authorization
   - Run integration tests

#### **Phase 4: Testing & Validation** (2-3 weeks)
Comprehensive testing across all projects.

1. Unit testing
2. Integration testing
3. Performance testing
4. Security testing
5. User acceptance testing

---

### Option B: Big Bang Migration

Migrate all projects simultaneously. **Not recommended** due to high risk.

---

## Detailed Migration Steps

### A. Migrating FineHotels Projects (.NET 8 → .NET 10)

**Complexity:** LOW
**Estimated Effort:** 1-2 days

#### Steps:

1. **Update Target Framework**
   ```xml
   <!-- Before -->
   <TargetFramework>net8.0</TargetFramework>

   <!-- After -->
   <TargetFramework>net10.0</TargetFramework>
   ```

2. **Update NuGet Packages**
   - Microsoft.AspNetCore.* packages: 8.0.1 → 10.0.x
   - Microsoft.EntityFrameworkCore.* packages: 8.0.1 → 10.0.x
   - Other dependencies as needed

3. **Review Breaking Changes**
   - Check [.NET 10 breaking changes documentation](https://learn.microsoft.com/en-us/dotnet/core/compatibility/)
   - Address API changes
   - Update obsolete patterns

4. **Test**
   - Run all unit tests
   - Run integration tests
   - Manual testing of key features

---

### B. Migrating RazorPagesMovie (.NET Core 3.1 → .NET 10)

**Complexity:** MEDIUM
**Estimated Effort:** 3-5 days

#### Steps:

1. **Update Target Framework**
   ```xml
   <TargetFramework>net10.0</TargetFramework>
   ```

2. **Update NuGet Packages**
   - Microsoft.EntityFrameworkCore.* packages: 3.1.7 → 10.0.x
   - Microsoft.VisualStudio.Web.CodeGeneration.Design: 3.1.4 → 10.0.x

3. **Address Breaking Changes** (.NET Core 3.1 → 6 → 8 → 10)
   - **Nullable reference types:** Consider enabling
   - **Implicit usings:** Consider enabling
   - **Minimal APIs:** Review if applicable
   - **Authentication:** Review changes in authentication middleware
   - **EF Core:** Review query behavior changes, migration changes

4. **Update Code Patterns**
   - Program.cs/Startup.cs consolidation (if using old pattern)
   - Update dependency injection patterns
   - Update middleware configuration

5. **Test**
   - Database migrations
   - CRUD operations
   - All features

---

### C. Migrating BCSHotels Projects (.NET Framework 4.5.1 → .NET 10)

**Complexity:** HIGH
**Estimated Effort:** 4-6 weeks

This is the most complex migration requiring careful planning.

#### C.1. BCSHotelsDomain Migration

**Estimated Effort:** 1-2 weeks

1. **Convert to SDK-Style Project**
   - Use `dotnet-try-convert` tool or manual conversion
   - Remove legacy XML cruft
   - Move to PackageReference from packages.config

2. **Update Target Framework**
   ```xml
   <TargetFramework>net10.0</TargetFramework>
   ```

3. **Migrate Entity Framework 6 → EF Core 10**
   - **Major Challenge:** EF6 and EF Core have significant differences
   - Convert DbContext configurations
   - Convert model configurations (Fluent API changes)
   - Migrate database migrations:
     - Option 1: Convert existing migrations
     - Option 2: Create fresh EF Core migration baseline
   - Update LINQ queries (some behaviors differ)
   - Replace unsupported EF6 features:
     - Lazy loading (needs explicit configuration in EF Core)
     - Some query patterns

4. **Update NuGet Packages**
   - Remove: EntityFramework 6.1.1
   - Add: Microsoft.EntityFrameworkCore 10.0.x
   - Add: Microsoft.EntityFrameworkCore.SqlServer 10.0.x
   - Add: Microsoft.EntityFrameworkCore.Design 10.0.x

5. **Code Changes**
   - Update using statements
   - Fix compilation errors
   - Update data annotations if needed
   - Update any EF6-specific patterns

6. **Testing**
   - Unit test all data access
   - Verify database operations
   - Compare queries with EF6 version for correctness

#### C.2. BCSHotels Web Application Migration

**Estimated Effort:** 3-4 weeks

This is the most complex migration in the entire plan.

##### 1. **Project Structure Conversion**

- Convert to SDK-style .csproj
- Remove legacy project structure
- Update to modern .csproj format

##### 2. **Update Target Framework**
```xml
<TargetFramework>net10.0</TargetFramework>
```

##### 3. **ASP.NET MVC 5 → ASP.NET Core MVC**

Major architectural changes required:

**Configuration Migration:**
- `web.config` → `appsettings.json` + `Program.cs`
- `Global.asax` → `Program.cs` startup configuration
- App_Start folder → `Program.cs` configurations
- Remove System.Web dependencies

**Application Startup:**
```csharp
// OLD (Global.asax.cs)
protected void Application_Start()
{
    AreaRegistration.RegisterAllAreas();
    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
    RouteConfig.RegisterRoutes(RouteTable.Routes);
    BundleConfig.RegisterBundles(BundleTable.Bundles);
}

// NEW (Program.cs)
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
// Add other services...

var app = builder.Build();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
```

**Controllers:**
- Update using statements
- Remove `System.Web.Mvc` → use `Microsoft.AspNetCore.Mvc`
- Update base controller classes
- Update action results (some types renamed)
- Update model binding attributes
- Update filter attributes

**Views:**
- Most Razor syntax compatible
- Update `@using` statements
- Update HTML helpers (some changed)
- Update validation helpers
- Update bundling/minification approach
- Update `_ViewStart.cshtml`, `_Layout.cshtml`

##### 4. **Authentication & Authorization Migration**

**ASP.NET Identity 2.1 → ASP.NET Core Identity:**

- Migrate identity models
- Update `IdentityDbContext` inheritance
- Convert OWIN authentication → ASP.NET Core authentication
- Update login/register logic
- Update cookie authentication
- Update authorization attributes
- Migrate user stores and managers

**OWIN Middleware → ASP.NET Core Middleware:**
```csharp
// OLD (Startup.cs with OWIN)
app.UseOAuthBearerTokens(options);
app.UseCookieAuthentication(new CookieAuthenticationOptions());

// NEW (Program.cs)
builder.Services.AddAuthentication(options => { ... })
    .AddCookie(options => { ... });
app.UseAuthentication();
app.UseAuthorization();
```

##### 5. **Dependency Injection**

- Convert dependency resolution to built-in DI
- Remove third-party IoC containers if used
- Register services in `Program.cs`

##### 6. **Static Files & Content**

- Move to wwwroot folder structure
- Update static file paths
- Update bundling (BundleConfig → consider using built-in or webpack/vite)

##### 7. **NuGet Packages Migration**

**Remove (ASP.NET MVC 5):**
- Microsoft.AspNet.Mvc 5.2.2
- Microsoft.AspNet.WebPages 3.2.2
- Microsoft.AspNet.Razor 3.2.2
- Microsoft.AspNet.Identity.* 2.1.0
- Microsoft.Owin.*
- Newtonsoft.Json (if not needed)

**Add (ASP.NET Core):**
- Microsoft.AspNetCore.Mvc
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.x
- Microsoft.AspNetCore.Authentication.Cookies
- Microsoft.EntityFrameworkCore.SqlServer 10.0.x

**Keep/Update:**
- Elmah → Consider Serilog, NLog, or built-in logging
- PagedList → Consider X.PagedList or manual implementation

##### 8. **Configuration System**

```xml
<!-- OLD (web.config) -->
<appSettings>
  <add key="Setting1" value="Value1" />
</appSettings>
<connectionStrings>
  <add name="DefaultConnection" connectionString="..." />
</connectionStrings>
```

```json
// NEW (appsettings.json)
{
  "Settings": {
    "Setting1": "Value1"
  },
  "ConnectionStrings": {
    "DefaultConnection": "..."
  }
}
```

Update code to use `IConfiguration` instead of `ConfigurationManager`.

##### 9. **Error Handling**

- Remove Elmah (if used)
- Implement ASP.NET Core error handling middleware
- Use built-in logging or add Serilog/NLog
- Update error pages

##### 10. **Testing Strategy**

- Create integration tests
- Test all controllers
- Test authentication flows
- Test database operations
- Performance testing
- Security testing

---

## Breaking Changes to Address

### Entity Framework 6 → EF Core

1. **Lazy loading:** Disabled by default, must enable explicitly
2. **Database initialization:** No automatic migration/seed
3. **Complex types:** Now called "owned types"
4. **Many-to-many:** Explicit join entities in older EF Core (simplified in later versions)
5. **LINQ translation:** Some queries may need adjustment
6. **SQL generation:** May produce different queries
7. **Change tracking:** Some behavioral differences

### ASP.NET MVC 5 → ASP.NET Core MVC

1. **No System.Web:** Complete rewrite, no HttpContext.Current
2. **Startup:** Different application startup model
3. **Middleware:** Different pipeline model
4. **Dependency injection:** Built-in, different registration
5. **Configuration:** JSON-based instead of XML
6. **Routing:** Attribute routing preferred
7. **Model binding:** Some changes in behavior
8. **Filters:** Different filter pipeline
9. **Areas:** Different registration approach

### .NET Framework → .NET (Core)

1. **APIs removed:** Some .NET Framework APIs don't exist
2. **Platform:** Cross-platform considerations
3. **Deployment:** Different deployment models
4. **Performance:** Better performance, different optimization patterns

---

## Tools & Resources

### Migration Tools

1. **dotnet try-convert**
   - Converts old .csproj to SDK-style
   - `dotnet tool install -g try-convert`

2. **.NET Upgrade Assistant**
   - Microsoft's official migration tool
   - `dotnet tool install -g upgrade-assistant`
   - Can automate many migration steps

3. **Portability Analyzer**
   - Analyzes .NET Framework code for .NET compatibility
   - Identifies problematic APIs

### Documentation

- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Migrate from ASP.NET MVC to ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/migration/mvc)
- [Migrate from ASP.NET Identity to ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/migration/identity)
- [EF6 to EF Core Migration](https://learn.microsoft.com/en-us/ef/efcore-and-ef6/porting/)
- [Breaking Changes in .NET](https://learn.microsoft.com/en-us/dotnet/core/compatibility/)

---

## Risk Assessment

### High Risk Areas

1. **BCSHotels Web Application**
   - Authentication/authorization changes
   - Potential data loss if migrations fail
   - User experience disruption
   - Custom middleware/filters

2. **Database Migrations**
   - EF6 → EF Core migration complexity
   - Potential data integrity issues
   - Migration downtime

3. **Third-Party Dependencies**
   - Some packages may not have .NET Core/.NET 10 equivalents
   - Breaking changes in updated packages

### Mitigation Strategies

1. **Version Control**
   - Create feature branch for migration
   - Commit frequently
   - Tag stable versions

2. **Testing**
   - Comprehensive test coverage before migration
   - Test after each phase
   - Automated testing where possible

3. **Rollback Plan**
   - Keep .NET Framework version running
   - Document rollback procedures
   - Have database backups

4. **Incremental Approach**
   - Migrate in phases (as outlined)
   - Validate each phase before proceeding
   - Can run multiple versions in parallel if needed

---

## Timeline Estimation

### Option 1: Migrate to .NET 9 (Current Stable)

| Phase | Duration | Tasks |
|-------|----------|-------|
| **Phase 1** | 1-2 weeks | FineHotels (.NET 8→9), RazorPagesMovie (.NET 3.1→9) |
| **Phase 2** | 2-3 weeks | BCSHotels migration planning & analysis |
| **Phase 3** | 4-6 weeks | BCSHotels migration execution |
| **Phase 4** | 2-3 weeks | Testing & validation |
| **Total** | **9-14 weeks** | Complete migration |

### Option 2: Wait for .NET 10 (November 2026)

| Phase | Duration | Tasks |
|-------|----------|-------|
| **Preparation** | Now - Nov 2026 | Migrate to .NET 8/9 first, prepare for .NET 10 |
| **Phase 1** | 1 week | All projects .NET 9→10 (straightforward if already on .NET 9) |
| **Phase 2** | 1-2 weeks | Testing & validation |
| **Total** | **2-3 weeks** | .NET 9→10 upgrade |

**Recommendation:** Migrate to .NET 9 now, then upgrade to .NET 10 when available.

---

## Recommended Approach

### Step-by-Step Execution Plan

1. **Create Migration Branch**
   ```bash
   git checkout -b feature/dotnet10-migration
   ```

2. **Phase 1: Quick Wins** (Start Here)
   - [ ] Upgrade FineHotels projects to .NET 9/10
   - [ ] Upgrade RazorPagesMovie to .NET 9/10
   - [ ] Test and validate
   - [ ] Commit and tag

3. **Phase 2: Analysis**
   - [ ] Run .NET Upgrade Assistant on BCSHotels
   - [ ] Document breaking changes
   - [ ] Create detailed migration checklist
   - [ ] Plan database migration strategy

4. **Phase 3: Domain Layer**
   - [ ] Migrate BCSHotelsDomain
   - [ ] Test thoroughly
   - [ ] Commit

5. **Phase 4: Web Application**
   - [ ] Migrate BCSHotels in sub-steps
   - [ ] Test each component
   - [ ] Commit frequently

6. **Phase 5: Final Testing**
   - [ ] End-to-end testing
   - [ ] Performance testing
   - [ ] Security review
   - [ ] Deploy to staging
   - [ ] UAT

7. **Phase 6: Production**
   - [ ] Deploy to production
   - [ ] Monitor closely
   - [ ] Have rollback ready

---

## Success Criteria

- [ ] All projects target .NET 10
- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] No regression in functionality
- [ ] No regression in performance (ideally improved)
- [ ] Security maintained or improved
- [ ] Documentation updated
- [ ] Team trained on new patterns

---

## Appendix A: Branch Strategy

### Recommended Git Workflow

```
main
  └── feature/dotnet10-migration (main migration branch)
       ├── feature/dotnet10-finehotels (FineHotels migration)
       ├── feature/dotnet10-razorpages (RazorPagesMovie migration)
       └── feature/dotnet10-bcshotels (BCSHotels migration)
            ├── feature/dotnet10-bcshotels-domain
            └── feature/dotnet10-bcshotels-web
```

Each sub-branch merges back to `feature/dotnet10-migration`, which eventually merges to `main`.

---

## Appendix B: Compatibility Matrix

| Feature | .NET Fx 4.5.1 | .NET Core 3.1 | .NET 8 | .NET 10 |
|---------|---------------|---------------|--------|---------|
| Windows Only | ✓ | ✗ | ✗ | ✗ |
| Cross-platform | ✗ | ✓ | ✓ | ✓ |
| ASP.NET Core | ✗ | ✓ | ✓ | ✓ |
| EF Core | ✗ | ✓ | ✓ | ✓ |
| Performance | Baseline | Better | Much Better | Best |
| Support Status | EOL | EOL | LTS (2026) | LTS (2029) |

---

## Conclusion

Migrating to .NET 10 is a significant undertaking, especially for the legacy BCSHotels projects. However, the benefits include:

- **Long-term support** through 2029
- **Performance improvements** (30-50% in many scenarios)
- **Cross-platform** capabilities
- **Modern development** patterns and tooling
- **Security improvements**
- **Community support** and ecosystem

The phased approach minimizes risk and allows for learning and adjustment throughout the process.

**Next Steps:**
1. Review and approve this plan
2. Set up development environment with .NET 9/10 SDK
3. Begin Phase 1 with FineHotels projects
4. Schedule planning sessions for BCSHotels migration

---

**Document Version:** 1.0
**Last Updated:** 2026-01-01
**Author:** Claude (AI Assistant)
