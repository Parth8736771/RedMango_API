﻿using System.ComponentModel.DataAnnotations;

namespace RedMango_API.Models.DTO
{
    public class OrderDetailsCreateDTO
    {
        [Required]
        public int MenuItemId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public string ItemName { get; set; }
    }
}
