using System.ComponentModel.DataAnnotations.Schema;

namespace RedMangoAPI.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public List<CartItem> CartItems { get; set; }

        [NotMapped]
        public double CartTotal { get; set; }
        [NotMapped]
        public string StripePaymentIntentId { get; set; }
        [NotMapped]
        public string ClientSecret { get; set; }
    }
}
