using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project_Hail_Mary.Models
{
	public class Cart
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int UserId { get; set; }

		public string? ProductSlug { get; set; }
		public string? ProductName { get; set; }
		public int Quantity { get; set; } = 1;
		public string? Size { get; set; }
		public DateTime AddedAt { get; set; } = DateTime.Now;

		[ForeignKey("UserId")]
		public Users? User { get; set; }
	}
}