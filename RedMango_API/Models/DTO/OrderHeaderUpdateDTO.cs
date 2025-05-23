﻿namespace RedMango_API.Models.DTO
{
    public class OrderHeaderUpdateDTO
    {
        public int OrderHeaderId { get; set; }
        public string PickupLocation { get; set; }
        public string PickupPhoneNumber { get; set; }
        public string PickupEmail { get; set; }

        public string StripePaymentIntentID { get; set; }
        public string Status { get; set; }
    }
}
