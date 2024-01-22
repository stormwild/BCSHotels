namespace FineHotelsDb;

public class FineHotelsDbContext
{
    public FineHotelsDbContext() : base("DefaultConnection")
    { }

    public DbSet<Hotel> Hotels { get; set; }

    public DbSet<Image> Images { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Conventions
            .Remove<PluralizingTableNameConvention>();
    }
}
