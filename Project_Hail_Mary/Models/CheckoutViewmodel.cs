using System.ComponentModel.DataAnnotations;

namespace Project_Hail_Mary.Models
{
    public class CheckoutViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string ZipCode { get; set; }

        [Required]
        public string Phone { get; set; }

        public bool IsDefaultAddress { get; set; }
    }
}