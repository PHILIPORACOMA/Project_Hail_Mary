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

        [Required]
        [StringLength(100)]
        public string? ProductSlug { get; set; }
        [Required]
        [StringLength(200)]
        public string? ProductName { get; set; }
		[Required]
		public int Quantity { get; set; } = 1;
		[Required]
		public string? Size { get; set; }
		public DateTime AddedAt { get; set; } = DateTime.UtcNow;

		[ForeignKey("UserId")]
		public Users? User { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}