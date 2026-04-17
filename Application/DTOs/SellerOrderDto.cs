using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MadeByMe.Application.DTOs
{
    public class SellerOrderDto
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Ім'я покупця є обов'язковим")]
        public string BuyerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email є обов'язковим")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email")]
        public string BuyerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Номер телефону є обов'язковим")]
        public string BuyerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Адреса доставки є обов'язковою")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public decimal TotalPrice { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public List<SellerOrderItemDto> Items { get; set; } = new();
    }

    public class SellerOrderItemDto
    {
        [Required]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}