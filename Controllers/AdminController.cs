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
    public class AdminController : Controller
    {
        private FoodFiestaEntities db = new FoodFiestaEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddSeller()
        {
            var sql = "SELECT * FROM [User] WHERE Type=0";
            List<User> unconfirmedUsers = db.Users.SqlQuery(sql).ToList();

            return View(unconfirmedUsers);
        }

        public ActionResult ConfirmSeller(string confirmedEmail)
        {

            SqlConnection con = new SqlConnection(@"Data Source=MEGATRONM609\SQLEXPRESS;Initial Catalog=FoodFiesta; Integrated Security=True");
            SqlCommand sql;
            con.Open();

            sql = new SqlCommand("UPDATE [User] SET Type=2 WHERE Email='"+ confirmedEmail + "'", con);
            sql.ExecuteNonQuery();
            con.Close();

            Session["lastAddedSeller"] = confirmedEmail;

            return View("AssignBranch", db.Branches.ToList());
        }

        public ActionResult AssignBranch()
        {
            return View(db.Branches.ToList());
        }

        public ActionResult Assign(int branch_id)
        {
            string sellerEmail = (string)Session["lastAddedSeller"];
            DateTime dateTime = DateTime.Now;

            db.Sellers.Add(new Seller() {
                Email = sellerEmail,
                Branch = branch_id,
                Time = dateTime,
            });

            db.SaveChanges();

            return RedirectToAction("SellerList");
        }

        public ActionResult SellerList()
        {
            return View(db.Sellers.ToList());
        }


        public ActionResult AddBranch()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBranch([Bind(Include = "Id,Name,Location")] Branch branch, string Name)
        {
            if (ModelState.IsValid)
            {
                db.Branches.Add(branch);
                db.SaveChanges();

                int bncId = branch.Id;

                SqlConnection con = new SqlConnection(@"Data Source=MEGATRONM609\SQLEXPRESS;Initial Catalog=FoodFiesta; Integrated Security=True");
                SqlCommand sql;
                con.Open();

                sql = new SqlCommand("INSERT INTO Stock (Branch) VALUES(" + bncId + ")", con);
                sql.ExecuteNonQuery();
                con.Close();
            }

            return RedirectToAction("BranchList");
        }

        public ActionResult BranchList()
        {
            return View(db.Branches.ToList());
        }






















        // GET: Admin/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Email,Password,Name,Type,Registration_date,Phone")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Email,Password,Name,Type,Registration_date,Phone")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
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
