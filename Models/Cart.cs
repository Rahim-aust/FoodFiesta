using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FoodFiesta.Models
{
    public class Cart
    {
        private FoodFiestaEntities db = new FoodFiestaEntities();
        public List<CartItem> cartItems { get; set; }

        public Cart()
        {
            cartItems = new List<CartItem>();
        }

        public void Clear()
        {
            cartItems.Clear();
        }

        public void Add(CartItem item)
        {
            foreach (CartItem cartItem in cartItems)
            {
                if (cartItem.Food.Id == item.Food.Id) return;
            }
            cartItems.Add(item);
        }

        public void Increase(int foodId) {
            int id = FindIndex(foodId);
            if (id == -1)
            {
                Food food = db.Foods.Find(foodId);
                if (food != null)
                {
                    cartItems.Add(new CartItem()
                    {
                        Food = food,
                        Quantity = 1
                    });
                }
                return;
            }
            cartItems[id].Quantity += 1;
        }

        public void Decrease(int foodId)
        {
            int id = FindIndex(foodId);
            if (id == -1) return;
            cartItems[id].Quantity -= 1;
            if (cartItems[id].Quantity <= 0)
            {
                cartItems.RemoveAt(id);
            }
        }

        public int FindIndex(int foodId)
        {
            for (int i = 0; i < cartItems.Count; i++)
            {
                if (cartItems[i].Food.Id == foodId) return i;
            }
            return -1;
        }
    }
}