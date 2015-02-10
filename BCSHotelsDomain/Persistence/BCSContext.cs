using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using BCSHotelsDomain.Entities;

namespace BCSHotelsDomain.Persistence
{
	public class BCSContext : DbContext
	{
		public BCSContext()
			: base("DefaultConnection")
		{
		}

		public DbSet<Hotel> Hotels { get; set; }

		public DbSet<Image> Images { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Conventions
				.Remove<PluralizingTableNameConvention>();
		}
	}
}
