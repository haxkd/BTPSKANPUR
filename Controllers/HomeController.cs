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
using System.Net.Mail;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Web.Helpers;

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

            var data = btps.Courses.FirstOrDefault(c => c.id == id);

            if (data == null)
            {
                return RedirectToAction("Index");

            }

            return View(data);
        }

        public ActionResult enroll(int? id)
        {
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


                var payment = btps.CoursePayments.FirstOrDefault(x => x.userid == userid && x.courseid == courseid && x.status == "pending");
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

                    CoursePayment p = new CoursePayment()
                    {
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
        public ActionResult enroll(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
        {
            var payment = btps.CoursePayments.FirstOrDefault(x => x.orderid == razorpay_order_id);
            int userid = (int)payment.userid;
            int courseid = (int)payment.courseid;

            var course = btps.Courses.FirstOrDefault(x=>x.id == courseid);
            var user = btps.Users.FirstOrDefault(x => x.id == userid);

            string name = user.name;
            string useremail = user.email;
            string coursename = course.name;
            string price = course.price;



            payment.payid = razorpay_payment_id;
            payment.signature = razorpay_signature;
            payment.status = "success";
            btps.SaveChanges();

            BoughtCours bought = new BoughtCours()
            {
                userid = userid,
                courseid = courseid,
                purchased = DateTime.Now.Date,
                courseName = coursename,
                courseprice = price
            };
            btps.BoughtCourses.Add(bought);
            btps.SaveChanges();

           
            sendMail(useremail, razorpay_order_id, name, coursename, price, courseid.ToString(), razorpay_payment_id);
            return RedirectToAction("dashboard");
        }

        public ActionResult register()
        {
            if (Session["userid"] != null)
            {
                return RedirectToAction("dashboard");
            }
            return View();
        }
        [HttpPost]
        public ActionResult register(UserModel user)
        {
            if (ModelState.IsValid)
            {
                var existuser = btps.Users.FirstOrDefault(db => db.email == user.email);
                if (existuser != null)
                {
                    ModelState.AddModelError("email", "Email already exist");
                }
                else
                {
                    User u = new User()
                    {
                        name = user.name,
                        email = user.email,
                        password = user.password,
                    };
                    btps.Users.Add(u);
                    btps.SaveChanges();

                    ModelState.Clear();
                    ModelState.AddModelError("msg", "User registration sucessfully");
                }
            }
            return View();
        }
        public ActionResult login()
        {
            if (Session["userid"] != null)
            {
                return RedirectToAction("dashboard");
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session["userid"] = null;
            return RedirectToAction("login");
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
                    if (existuser.password == user.password)
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
                List<BoughtCours> bought = btps.BoughtCourses.Where(x => x.userid == userid).ToList();
                UserCourse us = new UserCourse()
                {
                    user = user,
                    bought = bought
                };
                return View(us);
            }
            return View();
        }
        public void sendMail(string useremail, string orderid, string name, string coursename, string price, string courseid, string payid)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/js/courseMail.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{orderid}", orderid);
            body = body.Replace("{orderdate}", DateTime.Now.ToString());
            body = body.Replace("{username}", name);
            body = body.Replace("{useremail}", useremail);
            body = body.Replace("{coursename}", coursename);
            body = body.Replace("{amount}", price);
            body = body.Replace("{courseid}", courseid);
            body = body.Replace("{payid}", payid);

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("haxkdmail@gmail.com");
                mail.To.Add(useremail);
                mail.Subject = "Purchased Course Summary";
                mail.Body = body;
                mail.IsBodyHtml = true;
                //mail.Attachments.Add(new Attachment("D:\\TestFile.txt"));//--Uncomment this to send any attachment  
                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("haxkdmail@gmail.com", "geneqppuemqmupls");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
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