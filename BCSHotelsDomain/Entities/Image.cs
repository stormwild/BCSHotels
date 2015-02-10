using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCSHotelsDomain.Entities
{
	public class Image
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public string Path { get; set; }

		public long HotelId { get; set; }
		
		public virtual Hotel Hotel { get; set; }

	}
}
