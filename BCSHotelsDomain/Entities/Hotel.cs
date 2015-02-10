using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCSHotelsDomain.Entities
{
	public class Hotel
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public decimal Price { get; set; }

		public int Rating { get; set; }

		public virtual ICollection<Image> Images { get; set; }
	}
}
