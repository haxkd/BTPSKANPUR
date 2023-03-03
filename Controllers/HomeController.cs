using BTPSKANPUR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.ComponentModel.DataAnnotations;
using Razorpay.Api;
using Razorpay;

namespace BTPSKANPUR.Controllers
{
    public class HomeController : Controller
    {
        BTPSKANPUREntities btps = new BTPSKANPUREntities();
        public ActionResult Index()
        {
            var data = btps.Courses.ToList();
            return View(data);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult Courses()
        {
            var data = btps.Courses.ToList();
            return View(data);
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var data = btps.Courses.FirstOrDefault(c=>c.id == id);

            if (data == null)
            {
                return RedirectToAction("Index");

            }

            return View(data);
        }


        public ActionResult enroll(int? id)
        {
            Session["userid"] = 1;
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var data = btps.Courses.FirstOrDefault(c => c.id == id);
            if (data == null)
            {
                return RedirectToAction("Index");
            }

            if (Session["userid"] == null)
            {
                return RedirectToAction("login");
            }
            else
            {

                int courseid = Convert.ToInt32(id);
                int userid = Convert.ToInt32(Session["userid"]);
                int amount = Convert.ToInt32(data.price) * 100;             


                string orderId;


                var payment = btps.CoursePayments.FirstOrDefault(x=>x.userid == userid && x.courseid==courseid && x.status=="pending");
                if (payment != null)
                {
                    orderId = payment.orderid;
                }
                else
                {
                    Dictionary<string, object> input = new Dictionary<string, object>();
                    input.Add("amount", amount); // this amount should be same as transaction amount
                    input.Add("currency", "INR");
                    //input.Add("receipt", "12121");
                    string key = "rzp_test_WLhn7rrKlopFCG";
                    string secret = "Av4Rufmm7fDnY4k9N3rnu9NJ";
                    RazorpayClient client = new RazorpayClient(key, secret);
                    Order order = client.Order.Create(input);
                    orderId = order["id"].ToString();

                    CoursePayment p = new CoursePayment() {
                        userid = userid,
                        courseid = courseid,
                        orderid = orderId,
                        status = "pending",
                    };
                    btps.CoursePayments.Add(p);
                    btps.SaveChanges();
                }


                ViewBag.orderId = orderId.ToString();
                ViewBag.amount = amount.ToString();


                //int userid = Convert.ToInt32(Session["userid"]);

                //BoughtCours cr = new BoughtCours()
                //{
                //    courseid = id,
                //    userid = userid,
                //    purchased = DateTime.Now,
                //};
                //btps.BoughtCourses.Add(cr);
                //btps.SaveChanges();
            }



            return View();
        }

        [HttpPost]
        public ActionResult enroll(string razorpay_payment_id,string razorpay_order_id,string razorpay_signature)
        {
            var payment = btps.CoursePayments.FirstOrDefault(x => x.orderid == razorpay_order_id);
            payment.payid = razorpay_payment_id;
            payment.signature = razorpay_signature;
            payment.status = "success";
            btps.SaveChanges();
            return RedirectToAction("dashboard");
        }

            public ActionResult register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult register(UserModel user)
        {
            if (ModelState.IsValid)
            {
                var existuser = btps.Users.FirstOrDefault(db=>db.email == user.email);
                if (existuser != null) {
                    ModelState.AddModelError("email", "Email already exist");
                }
                else
                {
                    User u = new User() { 
                        name = user.name,
                        email = user.email,
                        password= user.password,
                    };
                    btps.Users.Add(u);
                    btps.SaveChanges();

                    ModelState.Clear();
                    ModelState.AddModelError("msg","User registration sucessfully");
                }
            }
            return View();
        }
        public ActionResult login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult login(LoginUserModel user)
        {
            if (ModelState.IsValid)
            {
                var existuser = btps.Users.FirstOrDefault(db => db.email == user.email);
                if (existuser == null)
                {
                    ModelState.AddModelError("email", "Email not exist....!");
                }
                else
                {
                    if(existuser.password == user.password)
                    {
                        Session["userid"] = existuser.id;
                        return RedirectToAction("dashboard");
                    }
                    else
                    {
                        ModelState.AddModelError("password", "Password is wrong....!");
                    }
                }
            }
            return View();
        }

        public ActionResult dashboard()
        {
            if (Session["userid"] == null)
            {
                return RedirectToAction("login");
            }
            else
            {
                int userid = Convert.ToInt32(Session["userid"]);
                User user = btps.Users.FirstOrDefault(x => x.id == userid);
                List<BoughtCours> bought = btps.BoughtCourses.Where(x=>x.userid==userid).ToList();
                UserCourse us = new UserCourse()
                {
                    user = user,
                    bought = bought

                };
                return View(us);
            }
            return View();
        }


       

    }

    public class LoginUserModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string password { get; set; }
    }

   
}