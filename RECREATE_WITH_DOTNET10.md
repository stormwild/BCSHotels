# Recreate BCSHotels with .NET 10 and Razor Pages

## Strategy: Clean Slate Approach

Instead of migrating the legacy .NET Framework 4.5.1 MVC application, we'll **recreate** the functionality using modern .NET 10 and ASP.NET Core Razor Pages. This approach is faster, cleaner, and results in a more maintainable application.

**Timeline:** 2-3 weeks (vs 4-6 weeks for migration)

---

## Why Recreate Instead of Migrate?

### Benefits of Recreation

1. **Faster Development** - No fighting legacy code
2. **Modern Patterns** - Built-in DI, middleware, configuration from day one
3. **Cleaner Codebase** - No technical debt
4. **Better Architecture** - Razor Pages for CRUD, cleaner separation
5. **Learning Opportunity** - Team learns .NET 10 best practices
6. **Parallel Development** - Old app keeps running

### What We'll Reuse

- ✅ **Database Schema** - Existing Azure SQL database
- ✅ **Business Logic** - Core hotel/booking rules (refactored)
- ✅ **UI Concepts** - Page layouts, user flows
- ✅ **Azure Resources** - SQL Server, App Service Plan, etc.

### What We'll Replace

- ❌ ASP.NET MVC 5 → **ASP.NET Core Razor Pages**
- ❌ Entity Framework 6 → **Entity Framework Core 10**
- ❌ ASP.NET Identity 2.1 + OWIN → **ASP.NET Core Identity**
- ❌ Web.config → **appsettings.json**
- ❌ Elmah → **Application Insights + ILogger**
- ❌ Old packages.config → **PackageReference**

---

## Current Application Analysis

Based on the existing BCSHotels project, here's what we need to recreate:

### Existing Models (from BCSHotelsDomain)

We'll need to examine and recreate these entities:
- Hotels
- Rooms
- Bookings
- Customers/Users
- Reviews (if any)

### Existing Features (to recreate)

Common hotel booking features to implement:
1. **Public Pages**
   - Home page
   - Hotel listing/search
   - Hotel details
   - Room availability
   - Booking flow

2. **User Features**
   - User registration/login
   - Profile management
   - View bookings
   - Cancel/modify bookings

3. **Admin Features** (if exists)
   - Manage hotels
   - Manage rooms
   - View all bookings
   - Reports

---

## Phase 1: Project Setup (1-2 days)

### 1.1 Create New Project

```bash
# Create new solution folder
mkdir BCSHotels.Modern
cd BCSHotels.Modern

# Create Razor Pages web app
dotnet new webapp -n BCSHotels.Web -f net10.0

# Create class library for data/domain
dotnet new classlib -n BCSHotels.Data -f net10.0
dotnet new classlib -n BCSHotels.Domain -f net10.0

# Create solution
dotnet new sln -n BCSHotels.Modern

# Add projects to solution
dotnet sln add BCSHotels.Web/BCSHotels.Web.csproj
dotnet sln add BCSHotels.Data/BCSHotels.Data.csproj
dotnet sln add BCSHotels.Domain/BCSHotels.Domain.csproj

# Add project references
cd BCSHotels.Web
dotnet add reference ../BCSHotels.Data/BCSHotels.Data.csproj
dotnet add reference ../BCSHotels.Domain/BCSHotels.Domain.csproj

cd ../BCSHotels.Data
dotnet add reference ../BCSHotels.Domain/BCSHotels.Domain.csproj
```

### 1.2 Project Structure

```
BCSHotels.Modern/
├── BCSHotels.Web/                    # ASP.NET Core Razor Pages
│   ├── Pages/
│   │   ├── Index.cshtml
│   │   ├── Hotels/
│   │   │   ├── List.cshtml
│   │   │   ├── Details.cshtml
│   │   │   └── Book.cshtml
│   │   ├── Bookings/
│   │   │   ├── MyBookings.cshtml
│   │   │   └── Confirmation.cshtml
│   │   ├── Account/
│   │   │   ├── Login.cshtml
│   │   │   ├── Register.cshtml
│   │   │   └── Manage.cshtml
│   │   └── Shared/
│   │       ├── _Layout.cshtml
│   │       └── _LoginPartial.cshtml
│   ├── wwwroot/
│   ├── Program.cs
│   └── appsettings.json
│
├── BCSHotels.Data/                   # Data access layer
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   ├── Migrations/
│   └── Repositories/
│
├── BCSHotels.Domain/                 # Domain models
│   ├── Entities/
│   │   ├── Hotel.cs
│   │   ├── Room.cs
│   │   ├── Booking.cs
│   │   └── ApplicationUser.cs
│   └── Services/
│       └── BookingService.cs
│
└── BCSHotels.Modern.sln
```

### 1.3 Install Required Packages

```bash
# In BCSHotels.Web
cd BCSHotels.Web
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Identity.UI
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.ApplicationInsights.AspNetCore

# In BCSHotels.Data
cd ../BCSHotels.Data
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

---

## Phase 2: Database & Domain Models (2-3 days)

### 2.1 Analyze Existing Database

First, we need to understand the current database schema:

```bash
# Connect to your existing Azure SQL database
# Use Azure Data Studio or SQL Server Management Studio

# Generate script of existing schema
# This will help us create matching EF Core models
```

### 2.2 Create Domain Models

Example based on typical hotel booking system:

**BCSHotels.Domain/Entities/Hotel.cs**
```csharp
namespace BCSHotels.Domain.Entities;

public class Hotel
{
    public int HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
```

**BCSHotels.Domain/Entities/Room.cs**
```csharp
namespace BCSHotels.Domain.Entities;

public class Room
{
    public int RoomId { get; set; }
    public int HotelId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int Capacity { get; set; }
    public bool IsAvailable { get; set; }

    // Navigation properties
    public Hotel Hotel { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
```

**BCSHotels.Domain/Entities/Booking.cs**
```csharp
namespace BCSHotels.Domain.Entities;

public class Booking
{
    public int BookingId { get; set; }
    public int RoomId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Room Room { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}
```

**BCSHotels.Domain/Entities/ApplicationUser.cs**
```csharp
using Microsoft.AspNetCore.Identity;

namespace BCSHotels.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
```

### 2.3 Create DbContext

**BCSHotels.Data/ApplicationDbContext.cs**
```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BCSHotels.Domain.Entities;

namespace BCSHotels.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Hotel)
            .WithMany(h => h.Rooms)
            .HasForeignKey(r => r.HotelId);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId);

        // Configure decimal precision
        modelBuilder.Entity<Room>()
            .Property(r => r.PricePerNight)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Hotel>()
            .Property(h => h.Rating)
            .HasPrecision(3, 2);
    }
}
```

### 2.4 Configure Connection to Existing Database

**BCSHotels.Web/appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:YOUR-SQL-SERVER.database.windows.net,1433;Database=BCSHotelsDb;Authentication=Active Directory Default;Encrypt=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ApplicationInsights": {
    "ConnectionString": ""
  }
}
```

**BCSHotels.Web/appsettings.Development.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BCSHotelsDb-Dev;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### 2.5 Scaffold from Existing Database (Alternative Approach)

If you want to generate models from existing database:

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Scaffold DbContext and entities from existing database
cd BCSHotels.Data
dotnet ef dbcontext scaffold \
  "Server=tcp:YOUR-SERVER.database.windows.net,1433;Database=BCSHotelsDb;Authentication=Active Directory Interactive;" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --output-dir Entities \
  --context-dir . \
  --context ApplicationDbContext \
  --force
```

This will generate models matching your existing database schema.

---

## Phase 3: Web Application Setup (2-3 days)

### 3.1 Configure Program.cs

**BCSHotels.Web/Program.cs**
```csharp
using BCSHotels.Data;
using BCSHotels.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

// Add Application Insights
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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
```

### 3.2 Create Initial Migration

```bash
cd BCSHotels.Web

# Create initial migration
dotnet ef migrations add InitialCreate --project ../BCSHotels.Data

# Review the migration - it should match existing database
# If database already exists with data, DON'T apply this migration yet

# Instead, create a baseline migration:
dotnet ef migrations add InitialCreate --project ../BCSHotels.Data --no-build
# Then manually edit or skip initial migration since DB exists
```

---

## Phase 4: Implement Core Pages (4-5 days)

### 4.1 Home Page

**Pages/Index.cshtml**
```cshtml
@page
@model IndexModel
@{
    ViewData["Title"] = "Home page";
}

<div class="text-center">
    <h1 class="display-4">Welcome to BCS Hotels</h1>
    <p class="lead">Find and book your perfect hotel</p>
</div>

<div class="row mt-5">
    <div class="col-md-4">
        <h3>Search Hotels</h3>
        <p>Browse our collection of premium hotels</p>
        <a asp-page="/Hotels/List" class="btn btn-primary">View Hotels</a>
    </div>
    <div class="col-md-4">
        <h3>Your Bookings</h3>
        <p>Manage your reservations</p>
        <a asp-page="/Bookings/MyBookings" class="btn btn-primary">My Bookings</a>
    </div>
    <div class="col-md-4">
        <h3>Special Offers</h3>
        <p>Check out our latest deals</p>
        <a href="#" class="btn btn-primary">View Offers</a>
    </div>
</div>

@if (Model.FeaturedHotels.Any())
{
    <div class="mt-5">
        <h2>Featured Hotels</h2>
        <div class="row">
            @foreach (var hotel in Model.FeaturedHotels)
            {
                <div class="col-md-4 mb-4">
                    <div class="card">
                        <img src="@hotel.ImageUrl" class="card-img-top" alt="@hotel.Name">
                        <div class="card-body">
                            <h5 class="card-title">@hotel.Name</h5>
                            <p class="card-text">@hotel.City, @hotel.Country</p>
                            <p class="card-text">
                                <small class="text-muted">Rating: @hotel.Rating / 5.0</small>
                            </p>
                            <a asp-page="/Hotels/Details" asp-route-id="@hotel.HotelId"
                               class="btn btn-primary">View Details</a>
                        </div>
                    </div>
                </div>
            }
        </div>
    </div>
}
```

**Pages/Index.cshtml.cs**
```csharp
using BCSHotels.Data;
using BCSHotels.Domain.Entities;
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

    public List<Hotel> FeaturedHotels { get; set; } = new();

    public async Task OnGetAsync()
    {
        FeaturedHotels = await _context.Hotels
            .OrderByDescending(h => h.Rating)
            .Take(6)
            .ToListAsync();
    }
}
```

### 4.2 Hotel List Page

**Pages/Hotels/List.cshtml**
```cshtml
@page
@model ListModel
@{
    ViewData["Title"] = "Hotels";
}

<h1>Available Hotels</h1>

<form method="get" class="mb-4">
    <div class="row">
        <div class="col-md-4">
            <input type="text" name="searchString" value="@Model.SearchString"
                   class="form-control" placeholder="Search by name or city" />
        </div>
        <div class="col-md-2">
            <button type="submit" class="btn btn-primary">Search</button>
        </div>
    </div>
</form>

@if (!Model.Hotels.Any())
{
    <p>No hotels found.</p>
}
else
{
    <div class="row">
        @foreach (var hotel in Model.Hotels)
        {
            <div class="col-md-4 mb-4">
                <div class="card h-100">
                    <img src="@hotel.ImageUrl" class="card-img-top" alt="@hotel.Name"
                         style="height: 200px; object-fit: cover;">
                    <div class="card-body">
                        <h5 class="card-title">@hotel.Name</h5>
                        <p class="card-text">
                            <i class="bi bi-geo-alt"></i> @hotel.City, @hotel.Country
                        </p>
                        <p class="card-text">
                            <small class="text-muted">
                                <i class="bi bi-star-fill"></i> @hotel.Rating / 5.0
                            </small>
                        </p>
                        <p class="card-text">@hotel.Description</p>
                    </div>
                    <div class="card-footer">
                        <a asp-page="Details" asp-route-id="@hotel.HotelId"
                           class="btn btn-primary w-100">View Details</a>
                    </div>
                </div>
            </div>
        }
    </div>
}

<!-- Pagination if needed -->
@if (Model.TotalPages > 1)
{
    <nav aria-label="Hotel pagination">
        <ul class="pagination">
            @for (int i = 1; i <= Model.TotalPages; i++)
            {
                <li class="page-item @(i == Model.CurrentPage ? "active" : "")">
                    <a class="page-link"
                       asp-page="List"
                       asp-route-pageNumber="@i"
                       asp-route-searchString="@Model.SearchString">@i</a>
                </li>
            }
        </ul>
    </nav>
}
```

**Pages/Hotels/List.cshtml.cs**
```csharp
using BCSHotels.Data;
using BCSHotels.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BCSHotels.Web.Pages.Hotels;

public class ListModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private const int PageSize = 9;

    public ListModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Hotel> Hotels { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int TotalPages { get; set; }

    public async Task OnGetAsync()
    {
        var query = _context.Hotels.AsQueryable();

        if (!string.IsNullOrWhiteSpace(SearchString))
        {
            query = query.Where(h =>
                h.Name.Contains(SearchString) ||
                h.City.Contains(SearchString));
        }

        var totalCount = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        Hotels = await query
            .OrderBy(h => h.Name)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }
}
```

### 4.3 Hotel Details & Booking

**Pages/Hotels/Details.cshtml**
```cshtml
@page "{id:int}"
@model DetailsModel
@{
    ViewData["Title"] = Model.Hotel?.Name ?? "Hotel Details";
}

@if (Model.Hotel == null)
{
    <p>Hotel not found.</p>
}
else
{
    <div class="row">
        <div class="col-md-8">
            <img src="@Model.Hotel.ImageUrl" class="img-fluid mb-3" alt="@Model.Hotel.Name">

            <h1>@Model.Hotel.Name</h1>
            <p class="lead">
                <i class="bi bi-geo-alt"></i> @Model.Hotel.Address, @Model.Hotel.City, @Model.Hotel.Country
            </p>
            <p>
                <i class="bi bi-star-fill"></i> Rating: @Model.Hotel.Rating / 5.0
            </p>

            <h3>About this hotel</h3>
            <p>@Model.Hotel.Description</p>

            <h3 class="mt-4">Available Rooms</h3>
            @if (!Model.Hotel.Rooms.Any())
            {
                <p>No rooms available.</p>
            }
            else
            {
                <div class="list-group">
                    @foreach (var room in Model.Hotel.Rooms.Where(r => r.IsAvailable))
                    {
                        <div class="list-group-item">
                            <div class="row align-items-center">
                                <div class="col-md-6">
                                    <h5>@room.Type - Room @room.RoomNumber</h5>
                                    <p class="mb-0">Capacity: @room.Capacity guests</p>
                                </div>
                                <div class="col-md-3">
                                    <p class="mb-0 fw-bold">$@room.PricePerNight / night</p>
                                </div>
                                <div class="col-md-3">
                                    <a asp-page="/Hotels/Book"
                                       asp-route-roomId="@room.RoomId"
                                       class="btn btn-primary">Book Now</a>
                                </div>
                            </div>
                        </div>
                    }
                </div>
            }
        </div>

        <div class="col-md-4">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">Quick Booking</h5>
                    <form method="get" asp-page="/Hotels/Book">
                        <input type="hidden" name="hotelId" value="@Model.Hotel.HotelId" />
                        <div class="mb-3">
                            <label class="form-label">Check-in</label>
                            <input type="date" name="checkIn" class="form-control"
                                   min="@DateTime.Today.ToString("yyyy-MM-dd")" required />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Check-out</label>
                            <input type="date" name="checkOut" class="form-control"
                                   min="@DateTime.Today.AddDays(1).ToString("yyyy-MM-dd")" required />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Guests</label>
                            <input type="number" name="guests" class="form-control"
                                   min="1" max="10" value="2" required />
                        </div>
                        <button type="submit" class="btn btn-primary w-100">
                            Check Availability
                        </button>
                    </form>
                </div>
            </div>
        </div>
    </div>
}
```

**Pages/Hotels/Details.cshtml.cs**
```csharp
using BCSHotels.Data;
using BCSHotels.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BCSHotels.Web.Pages.Hotels;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Hotel? Hotel { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Hotel = await _context.Hotels
            .Include(h => h.Rooms)
            .FirstOrDefaultAsync(h => h.HotelId == id);

        if (Hotel == null)
        {
            return NotFound();
        }

        return Page();
    }
}
```

---

## Phase 5: Authentication (1-2 days)

Identity scaffolding makes this easy:

```bash
# Scaffold Identity pages
dotnet aspnet-codegenerator identity \
  --dbContext ApplicationDbContext \
  --files "Account.Register;Account.Login;Account.Logout;Account.Manage.Index"
```

This generates login, register, and account management pages automatically.

---

## Phase 6: Deploy to Azure (1 day)

### Option A: Deploy to Staging Slot

```bash
# Using Azure CLI
az webapp deployment source config-zip \
  --resource-group <your-rg> \
  --name <your-app-name> \
  --slot staging \
  --src ./publish.zip
```

### Option B: Deploy from VS Code

1. Install **Azure App Service** extension
2. Right-click on `BCSHotels.Web` project
3. Select "Deploy to Web App..."
4. Choose:
   - Create new Web App (for separate deployment), OR
   - Deploy to existing Web App's staging slot

### Option C: Set up GitHub Actions

Create `.github/workflows/azure-webapps-dotnet.yml` using the template from `AZURE_DEPLOYMENT_MIGRATION.md`

---

## Timeline Summary

| Phase | Duration | Deliverable |
|-------|----------|-------------|
| **Phase 1** | 1-2 days | Project structure, solution setup |
| **Phase 2** | 2-3 days | Domain models, DbContext, database connection |
| **Phase 3** | 2-3 days | Web app configuration, Identity setup |
| **Phase 4** | 4-5 days | Core pages (Home, Hotels, Booking) |
| **Phase 5** | 1-2 days | Authentication pages |
| **Phase 6** | 1 day | Azure deployment |
| **Testing** | 2-3 days | QA, bug fixes |
| **TOTAL** | **13-19 days** | **~3 weeks** |

---

## Comparison: Recreate vs Migrate

| Aspect | Recreate (Recommended) | Migrate |
|--------|----------------------|---------|
| **Time** | 3 weeks | 4-6 weeks |
| **Risk** | Low (old app keeps running) | Medium-High |
| **Code Quality** | Clean, modern | Mixed legacy/modern |
| **Learning** | Team learns .NET 10 | Fighting old patterns |
| **Maintenance** | Easy | Technical debt remains |
| **Testing** | Easier (smaller surface area) | Complex (large surface area) |

---

## Next Steps

1. **Review existing BCSHotels functionality**
   - What pages exist?
   - What features are critical?
   - What can be simplified or improved?

2. **Examine existing database**
   - Connect to Azure SQL
   - Document schema
   - Export some test data if needed

3. **Create new project structure**
   - Follow Phase 1 above
   - Set up modern .NET 10 solution

4. **Start with core features**
   - Hotel listing
   - Hotel details
   - Basic booking

5. **Deploy to Azure staging slot**
   - Test alongside existing app
   - Use same database (be careful with migrations!)

6. **Iterate and improve**
   - Add features incrementally
   - Get feedback
   - Swap to production when ready

---

## Database Strategy

Since you're reusing the existing Azure SQL database:

### Option 1: Shared Database (Recommended for transition)

Both apps use same database:
- Old .NET 4.5.1 app continues working
- New .NET 10 app reads/writes same data
- **Risk:** Schema changes could break old app
- **Mitigation:** Don't run migrations until ready to retire old app

### Option 2: Separate Databases

Create new database for .NET 10 app:
- Full isolation
- Can migrate data gradually
- Rollback is easy
- **Downside:** Data sync complexity

---

Would you like me to:
1. **Analyze the existing BCSHotels code** to understand what features to recreate?
2. **Create the new project structure** now?
3. **Generate the domain models** by scaffolding from your existing Azure SQL database?
4. **Set up deployment to a staging slot** on your existing App Service?

Let me know which approach resonates with you!
