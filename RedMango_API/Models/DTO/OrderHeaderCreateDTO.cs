using System.ComponentModel.DataAnnotations;

namespace RedMango_API.Models.DTO
{
    public class OrderHeaderCreateDTO
    {
        [Required]
        public string PickupEmail { get; set; }
        [Required]
        public string PickupPhoneNumber { get; set; }
        [Required]
        public string PickupLocation { get; set; }
        public string ApplicationUserId { get; set; }
        public double OrderTotal { get; set; }
        public string StripePaymentIntentId { get; set; }
        public int TotalItems { get; set; }
        public string Status { get; set; }
        public IEnumerable<OrderDetailsCreateDTO> OrderDetailsDTO { get; set; }
    }
}
