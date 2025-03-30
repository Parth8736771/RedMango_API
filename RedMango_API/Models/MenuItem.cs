using System.ComponentModel.DataAnnotations;

namespace RedMango_API.Models
{
	public class MenuItem
	{
		[Key]
		public int Id { get; set; }
		[Required]
		public string Name { get; set; }
		public string Description { get; set; }
		public string SpecialTag { get; set; }
		public string Category { get; set; }
		[Required]
		public double Price { get; set; }
		public string? Image { get; set; }
	}
}
