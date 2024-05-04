using FoodFiesta.Models;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace FoodFiesta.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            String type = (String)Session["userType"];
            if (type == "Seller") RedirectToAction("Index", "Seller");
                else if (type == "Admin") RedirectToAction("Index", "Admin");
            return View();
        }

        public ActionResult Signup()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult AuthorizeLogin(string userInputEmail1, string userInputPassword)
        {
            using (FoodFiestaEntities db = new FoodFiestaEntities())
            {
                var userDetails = db.Users.Where(user => user.Email == userInputEmail1 && user.Password == userInputPassword).FirstOrDefault();
                Session["cart"] = null;

                if (userDetails != null && userDetails.Type == 2)
                {
                    var seller = db.Sellers.Where(user => user.User.Email == userInputEmail1).FirstOrDefault();

                    Session["seller"] = seller;
                    Session["userEmail"] = userDetails.Email;
                    Session["userBranchId"] = seller.Branch;
                    Session["userBranchName"] = seller.Branch1.Name;
                    Session["userType"] = "Seller";
                    Session["cart"] = new Cart();
                    return RedirectToAction("Index", "Seller");
                }
                else if (userDetails != null && userDetails.Type == 1)
                {
                    Session["userEmail"] = userDetails.Email;
                    Session["userType"] = "Admin";
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    Response.Write("<script>alert('Incorrect Email or Password');</script>");
                    return View("Index");
                }
            }
        }

        public ActionResult AuthorizeSignup(string inputFullnameForSignup, string inputPhoneForSignup, string inputEmailForSignup, string inputPasswordForSignup)
        {
            using (FoodFiestaEntities db = new FoodFiestaEntities())
            {
                var userDetails = db.Users.Where(user => user.Email == inputEmailForSignup).FirstOrDefault();

                if (userDetails != null)
                {
                    Response.Write("<script>alert('Email already exists.');</script>");
                    return View("Signup");
                }
                else if (userDetails == null)
                {
                    DateTime dateTime = DateTime.Now;

                    System.Data.SqlClient.SqlConnection con = new SqlConnection(@"Data Source=MEGATRONM609\SQLEXPRESS;Initial Catalog=FoodFiesta; Integrated Security=True");
                    SqlCommand sql;
                    con.Open();

                    sql = new SqlCommand("INSERT INTO [User] VALUES('" + inputEmailForSignup + "','" + inputPasswordForSignup + "','" + inputFullnameForSignup + "','" + "0" + "','" + dateTime + "', '" + inputPhoneForSignup + "')", con);
                    sql.ExecuteNonQuery();
                    con.Close();

                    Response.Write("<script>alert('Registration info submitted successfully.');</script>");
                    return View("Index");
                }
            }

            return RedirectToAction("Signup", "Home");
        }

        public ActionResult KillSession()
        {
            Session.RemoveAll();
            return RedirectToAction("", "Home");
        }
    }
}