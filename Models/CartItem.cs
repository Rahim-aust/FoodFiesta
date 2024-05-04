using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodFiesta.Models
{
    public class CartItem
    {
        public Food Food { get; set; }
        public int Quantity { get; set; }
    }
}