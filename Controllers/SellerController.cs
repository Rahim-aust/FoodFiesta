using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FoodFiesta.Models;

namespace FoodFiesta.Controllers
{
    public class SellerController : Controller
    {
        private FoodFiestaEntities db = new FoodFiestaEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddIngredient()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddIngredient([Bind(Include = "Id,Name,Unit_type")] Ingredient ingredient, string Name)
        {
            //try
            {
                if (ModelState.IsValid)
                {
                    db.Ingredients.Add(ingredient);
                    db.SaveChanges();
                }

                var ingDetails = db.Ingredients.Where(user => user.Name == Name).FirstOrDefault();

                int ingId = ingDetails.Id;

                string sellerEmail = (string)Session["userEmail"];
                var sellerDetails = db.Sellers.Where(user => user.User.Email == sellerEmail).FirstOrDefault();
                int branchId = (int)sellerDetails.Branch;

                var stockDetails = db.Stocks.Where(user => user.Branch == branchId).FirstOrDefault();

                int stkId = stockDetails.Id;

                SqlConnection con = new SqlConnection(@"Data Source=MEGATRONM609\SQLEXPRESS;Initial Catalog=FoodFiesta; Integrated Security=True");
                SqlCommand sql;
                con.Open();
                
                sql = new SqlCommand("INSERT INTO StockElement ([Stock],[Ingredient],[Quantity]) VALUES ("+stkId+","+ ingId + ",0)", con);
                sql.ExecuteNonQuery();
                con.Close();

                return RedirectToAction("IngredientList", db.Ingredients.ToList());
            }
            //catch
            {
                //Response.Write("<script>alert('Invalid input');</script>");
                //return View();
            }


            //return View("IngredientList");
        }

        public ActionResult IngredientList()
        {
            return View(db.Ingredients.ToList());
        }


        public ActionResult AddFood()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFood([Bind(Include = "Name,Price")] Food food, string Name, int Price)
        {
            if (ModelState.IsValid)
            {
                ViewBag.newFoodName = Name;
                db.Foods.Add(food);
                db.SaveChanges();

                var foodDetails = db.Foods.Where(fd => fd.Name == Name && fd.Price == Price).FirstOrDefault();
                Session["lastAddedFoodId"] = foodDetails.Id;
                Session["lastAddedFood"] = Name;

                return View("AssignIngredient", db.Ingredients.ToList());
            }

            return View(food);
        }

        public ActionResult FoodList()
        {
            return View(db.Foods.ToList());
        }

        public ActionResult AssignIngredient()
        {
            return View(db.Ingredients.ToList());
        }

        
        public ActionResult Assign(int ingredient_id)
        {
            SqlConnection con = new SqlConnection(@"Data Source=MEGATRONM609\SQLEXPRESS;Initial Catalog=FoodFiesta; Integrated Security=True");
            SqlCommand sql;
            con.Open();

            int id = (int)Session["lastAddedFoodId"];
            var foodDetails = db.Foods.Where(fd => fd.Id == id).FirstOrDefault();

            sql = new SqlCommand("INSERT INTO FoodElement (Food, Ingredient, Quantity) VALUES(" + Session["lastAddedFoodId"] + ", " + ingredient_id + ", " + 1 + ")", con);
            sql.ExecuteNonQuery();
            con.Close();

            Response.Write("<script>alert('Ingredient added successfully.');</script>");

            Session["lastAddedFoodId"] = foodDetails.Id;
            Session["lastAddedFood"] = foodDetails.Name;

            return View("AssignIngredient", db.Ingredients.ToList());
        }
        

        public ActionResult FoodElements()
        {
            int food_id = (int)Session["lastAddedFood"];

            var sql = "SELECT * FROM FoodElement WHERE Food = "+food_id+"";
            List<FoodElement> selectedFoodElements = db.FoodElements.SqlQuery(sql).ToList();

            return View(selectedFoodElements);
        }

        public ActionResult StockElements()
        {
            string sellerEmail = (string)Session["userEmail"];
            var sellerDetails = db.Sellers.Where(user => user.User.Email == sellerEmail).FirstOrDefault();
            int branchId = (int)sellerDetails.Branch;

            var stockElements = db.StockElements.Where(s => s.Stock1.Branch == branchId).FirstOrDefault();
            return View(db.StockElements.Where(s => s.Stock1.Branch == branchId));
        }

        public ActionResult UpdateStock(int? ing, int? stk)
        {
            if (stk == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockElement stockElement = db.StockElements.Find(stk, ing);
            if (stockElement == null)
            {
                return HttpNotFound();
            }
            ViewBag.Stock = new SelectList(db.Stocks, "Id", "Id", stockElement.Stock);
            return View(stockElement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStock([Bind(Include = "Stock,Ingredient,Quantity")] StockElement stockElement)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stockElement).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Stock = new SelectList(db.Stocks, "Id", "Id", stockElement.Stock);
            //return View(stockElement);
            return View(stockElement);
        }

        public ActionResult SellFood()
        {
            return View(db.Foods.ToList());
        }

        public bool CartPossible()
        {
            Cart cart = (Cart)Session["cart"];
            foreach (CartItem item in cart.cartItems)
            {
                int branchId = (int)Session["userBranchId"];
                var query = db.StockElements.Where(elem => (elem.Ingredient == item.Food.Id) && (elem.Stock1.Branch == branchId)).FirstOrDefault();
                float qty = query?.Quantity ?? 0;
                if (qty < item.Quantity) return false;
            }
            return true;
        }

        public ActionResult Cart()
        {
            return View(db.Foods.ToList());
        }

        public ActionResult IncreaseFood(int foodId)
        {
            Cart cart = (Cart)Session["cart"];
            cart.Increase(foodId);

            if (!CartPossible())
            {
                Response.Write("<script>alert('Insufficient food element.');</script>");
                cart.Decrease(foodId);
            }

            return View("SellFood", db.Foods.ToList());
        }

        public ActionResult DecreaseFood(int foodId)
        {
            Cart cart = (Cart)Session["cart"];
            cart.Decrease(foodId);
            return View("SellFood", db.Foods.ToList());
        }

        public ActionResult ConfirmOrder()
        {
            Cart cart = (Cart)Session["cart"];
            int branchId = (int)Session["userBranchId"];
            Seller seller = (Seller)Session["seller"];

            Order order = db.Orders.Add(new Order()
            {
                Seller = seller.Id,
                Time = DateTime.Now
            });

            foreach (CartItem item in cart.cartItems)
            {
                // Decrease Stock Element
                StockElement elem = db.StockElements.Where(e => (e.Ingredient == item.Food.Id) && (e.Stock1.Branch == branchId)).FirstOrDefault();
                int qty = (int)((elem.Quantity ?? 0) - item.Quantity);
                qty = Math.Max(qty, 0);

                System.Data.SqlClient.SqlConnection con = new SqlConnection(@"Data Source=MEGATRONM609\SQLEXPRESS;Initial Catalog=FoodFiesta; Integrated Security=True");
                SqlCommand sql;
                con.Open();

                sql = new SqlCommand("UPDATE [StockElement] SET Quantity = "+qty+" WHERE Stock='" + elem.Stock + "' AND Ingredient='" + elem.Ingredient + "'", con);
                sql.ExecuteNonQuery();
                con.Close();


                // Create order element
                db.OrderElements.Add(new OrderElement()
                {
                    Food = item.Food.Id,
                    Order = order.Id,
                    Quantity = item.Quantity,
                    Price = item.Food.Price,
                });
            }

            db.SaveChanges();
            cart.Clear();
            return View("Index");
        }

        // GET: Sellers
        public ActionResult IndexCopy()
        {
            var sellers = db.Sellers.Include(s => s.Branch1).Include(s => s.User);
            return View(sellers.ToList());
        }

        // GET: Sellers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seller seller = db.Sellers.Find(id);
            if (seller == null)
            {
                return HttpNotFound();
            }
            return View(seller);
        }

        // GET: Sellers/Create
        public ActionResult Create()
        {
            ViewBag.Branch = new SelectList(db.Branches, "Id", "Name");
            ViewBag.Email = new SelectList(db.Users, "Email", "Password");
            return View();
        }

        // POST: Sellers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Time,Email,Branch")] Seller seller)
        {
            if (ModelState.IsValid)
            {
                db.Sellers.Add(seller);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Branch = new SelectList(db.Branches, "Id", "Name", seller.Branch);
            ViewBag.Email = new SelectList(db.Users, "Email", "Password", seller.Email);
            return View(seller);
        }

        // GET: Sellers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seller seller = db.Sellers.Find(id);
            if (seller == null)
            {
                return HttpNotFound();
            }
            ViewBag.Branch = new SelectList(db.Branches, "Id", "Name", seller.Branch);
            ViewBag.Email = new SelectList(db.Users, "Email", "Password", seller.Email);
            return View(seller);
        }

        // POST: Sellers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Time,Email,Branch")] Seller seller)
        {
            if (ModelState.IsValid)
            {
                db.Entry(seller).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Branch = new SelectList(db.Branches, "Id", "Name", seller.Branch);
            ViewBag.Email = new SelectList(db.Users, "Email", "Password", seller.Email);
            return View(seller);
        }

        // GET: Sellers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Seller seller = db.Sellers.Find(id);
            if (seller == null)
            {
                return HttpNotFound();
            }
            return View(seller);
        }

        // POST: Sellers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Seller seller = db.Sellers.Find(id);
            db.Sellers.Remove(seller);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
