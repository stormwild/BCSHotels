using System.Collections.Generic;
using BCSHotelsDomain.Entities;
using BCSHotelsDomain.Persistence;

namespace BCSHotelsDomain.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BCSContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(BCSContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

	        var r = new Random();
			var hotels = new List<Hotel>();

	        for (var i = 0; i < 100; i++)
	        {
		        hotels.Add(new Hotel
		        {
					Name = "Hotel " + i,
					Description = "Hotel Description " + i,
					Price = r.Next(10, 500),
					Rating = r.Next(1, 5)
		        });
	        }

	        if (!context.Hotels.Any())
	        {
		        hotels.ForEach(h => context.Hotels.AddOrUpdate(h));
		        context.SaveChanges();
	        }

	        var images = new List<Image>();

	        foreach (var hotel in context.Hotels)
	        {
		        images.Add(new Image
		        {
			        Name = "Image for " + hotel.Name,
			        Path = "http://lorempixel.com/400/200/city/" + hotel.Name + "/",
			        HotelId = hotel.Id
		        });

		        images.Add(new Image
		        {
			        Name = "Image 2 for " + hotel.Name,
			        Path = "http://lorempixel.com/400/200/city/" + hotel.Name + "/",
			        HotelId = hotel.Id
		        });
	        }

	        if (!context.Images.Any())
			{
				images.ForEach(img => context.Images.AddOrUpdate(img));
				context.SaveChanges();
			}
	        
        }
    }
}
