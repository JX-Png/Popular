using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Models
{
    public class PaymentCard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [StringLength(100)]
        public string CardName { get; set; }

        [Required]
        public string CardNumberHash { get; set; }

        [Required]
        [StringLength(100)]
        public string CardholderName { get; set; }

        [Required]
        public string ExpiryMonth { get; set; }

        [Required]
        public string ExpiryYear { get; set; }

        [NotMapped]
        public string CardNumberLastFour { get; set; }

        public bool IsDefault { get; set; }

        // Helper method to get masked card number for display
        public string GetMaskedCardNumber()
        {
            return $"•••• •••• •••• {CardNumberLastFour}";
        }
    }
}