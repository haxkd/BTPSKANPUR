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
            payment.payid = razorpay_payment_id;
            payment.signature = razorpay_signature;
            payment.status = "success";
            btps.SaveChanges();

            BoughtCours bought = new BoughtCours()
            {
                userid = userid,
                courseid = courseid,
                purchased = DateTime.Now.Date
            };
            btps.BoughtCourses.Add(bought);
            btps.SaveChanges();

            var user = btps.Users.FirstOrDefault(x => x.id == userid);
            var course = btps.Courses.FirstOrDefault(x => x.id == courseid);

            string name = user.name;
            string useremail = user.email;
            string orderid = razorpay_order_id;
            string coursename = course.name;
            string price = course.price;
            sendMail(useremail, orderid, name, coursename, price, courseid.ToString());
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
        public void sendMail(string useremail,string orderid,string name, string coursename,string price,string courseid)
        {
            using (MailMessage mail = new MailMessage())
            {
                string msg = "<!DOCTYPE html>" +
                    "<html lang=\"en\">" +
                    "<head itemscope=\"\" itemtype=\"http://schema.org/WebSite\">" +
                    "<title itemprop=\"name\">Preview Bootstrap snippets. company invoice</title>\r\n    " +
                    "<meta content=\"width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0\" name=\"viewport\">\r\n    " +
                    "<style type=\"text/css\">\r\n        body {\r\n            margin-top: 20px;\r\n            color: #484b51;\r\n        }\r\n\r\n        .text-secondary-d1 {\r\n            color: #728299 !important;\r\n        }\r\n\r\n        .page-header {\r\n            margin: 0 0 1rem;\r\n            padding-bottom: 1rem;\r\n            padding-top: .5rem;\r\n            border-bottom: 1px dotted #e2e2e2;\r\n            display: -ms-flexbox;\r\n            display: flex;\r\n            -ms-flex-pack: justify;\r\n            justify-content: space-between;\r\n            -ms-flex-align: center;\r\n            align-items: center;\r\n        }\r\n\r\n        .page-title {\r\n            padding: 0;\r\n            margin: 0;\r\n            font-size: 1.75rem;\r\n            font-weight: 300;\r\n        }\r\n\r\n        .brc-default-l1 {\r\n            border-color: #dce9f0 !important;\r\n        }\r\n        .ml-n1,\r\n        .mx-n1 {\r\n            margin-left: -.25rem !important;\r\n        }\r\n        .mr-n1,\r\n        .mx-n1 {\r\n            margin-right: -.25rem !important;\r\n        }\r\n        .mb-4,\r\n        .my-4 {\r\n            margin-bottom: 1.5rem !important;\r\n        }\r\n        hr {\r\n            margin-top: 1rem;\r\n            margin-bottom: 1rem;\r\n            border: 0;\r\n            border-top: 1px solid rgba(0, 0, 0, .1);\r\n        }\r\n\r\n        .text-grey-m2 {\r\n            color: #888a8d !important;\r\n        }\r\n\r\n        .text-success-m2 {\r\n            color: #86bd68 !important;\r\n        }\r\n\r\n        .font-bolder,\r\n        .text-600 {\r\n            font-weight: 600 !important;\r\n        }\r\n\r\n        .text-110 {\r\n            font-size: 110% !important;\r\n        }\r\n\r\n        .text-blue {\r\n            color: #478fcc !important;\r\n        }\r\n\r\n        .pb-25,\r\n        .py-25 {\r\n            padding-bottom: .75rem !important;\r\n        }\r\n\r\n        .pt-25,\r\n        .py-25 {\r\n            padding-top: .75rem !important;\r\n        }\r\n\r\n        .bgc-default-tp1 {\r\n            background-color: rgba(121, 169, 197, .92) !important;\r\n        }\r\n\r\n        .bgc-default-l4,\r\n        .bgc-h-default-l4:hover {\r\n            background-color: #f3f8fa !important;\r\n        }\r\n\r\n        .page-header .page-tools {\r\n            -ms-flex-item-align: end;\r\n            align-self: flex-end;\r\n        }\r\n\r\n        .btn-light {\r\n            color: #757984;\r\n            background-color: #f5f6f9;\r\n            border-color: #dddfe4;\r\n        }\r\n\r\n        .w-2 {\r\n            width: 1rem;\r\n        }\r\n\r\n        .text-120 {\r\n            font-size: 120% !important;\r\n        }\r\n\r\n        .text-primary-m1 {\r\n            color: #4087d4 !important;\r\n        }\r\n\r\n        .text-danger-m1 {\r\n            color: #dd4949 !important;\r\n        }\r\n\r\n        .text-blue-m2 {\r\n            color: #68a3d5 !important;\r\n        }\r\n\r\n        .text-150 {\r\n            font-size: 150% !important;\r\n        }\r\n\r\n        .text-60 {\r\n            font-size: 60% !important;\r\n        }\r\n\r\n        .text-grey-m1 {\r\n            color: #7b7d81 !important;\r\n        }\r\n\r\n        .align-bottom {\r\n            vertical-align: bottom !important;\r\n        }\r\n    </style>\r\n</head>\r\n\r\n<body>\r\n    <div id=\"snippetContent\">\r\n        <link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/bootstrap@4.4.1/dist/css/bootstrap.min.css\">\r\n        <script src=\"https://cdn.jsdelivr.net/npm/bootstrap@4.4.1/dist/js/bootstrap.bundle.min.js\"></script>\r\n        <link href=\"https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css\" rel=\"stylesheet\" />\r\n        <div class=\"page-content container\">\r\n            <div class=\"page-header text-blue-d2\">\r\n                <h1 class=\"page-title text-secondary-d1\"> Invoice <small class=\"page-info\"> <i\r\n                            class=\"fa fa-angle-double-right text-80\"></i> ID: #"+orderid+" </small></h1>\r\n                <div class=\"page-tools\">\r\n                    <div class=\"action-buttons\"> <a class=\"btn bg-white btn-light mx-1px text-95\" href=\"#\"\r\n                            data-title=\"Print\"> <i class=\"mr-1 fa fa-print text-primary-m1 text-120 w-2\"></i> Print </a>\r\n                        <a class=\"btn bg-white btn-light mx-1px text-95\" href=\"#\" data-title=\"PDF\"> <i\r\n                                class=\"mr-1 fa fa-file-pdf-o text-danger-m1 text-120 w-2\"></i> Export </a></div>\r\n                </div>\r\n            </div>\r\n            <div class=\"container px-0\">\r\n                <div class=\"row mt-4\">\r\n                    <div class=\"col-12 col-lg-12\">\r\n                        <div class=\"row\">\r\n                            <div class=\"col-12\">\r\n                                <div class=\"text-center text-150\"> <i class=\"fa fa-book fa-2x text-success-m2 mr-1\"></i>\r\n                                    <span class=\"text-default-d3\">Bootdey.com</span></div>\r\n                            </div>\r\n                        </div>\r\n                        <hr class=\"row brc-default-l1 mx-n1 mb-4\" />\r\n                        <div class=\"row\">\r\n                            <div class=\"col-sm-6\">\r\n                                <div> <span class=\"text-sm text-grey-m2 align-middle\">To:</span> <span\r\n                                        class=\"text-600 text-110 text-blue align-middle\">"+name+"</span></div>\r\n                                <div class=\"text-grey-m2\">\r\n                                    <div class=\"my-1\"> Street, City</div>\r\n                                    <div class=\"my-1\"> State, Country</div>\r\n                                    <div class=\"my-1\"><i class=\"fa fa-phone fa-flip-horizontal text-secondary\"></i> <b\r\n                                            class=\"text-600\">111-111-111</b></div>\r\n                                </div>\r\n                            </div>\r\n                            <div class=\"text-95 col-sm-6 align-self-start d-sm-flex justify-content-end\">\r\n                                <hr class=\"d-sm-none\" />\r\n                                <div class=\"text-grey-m2\">\r\n                                    <div class=\"mt-1 mb-2 text-secondary-m1 text-600 text-125\"> Invoice</div>\r\n                                    <div class=\"my-2\"><i class=\"fa fa-circle text-blue-m2 text-xs mr-1\"></i> <span\r\n                                            class=\"text-600 text-90\">ID:</span> "+orderid+"</div>\r\n                                    <div class=\"my-2\"><i class=\"fa fa-circle text-blue-m2 text-xs mr-1\"></i> <span\r\n                                            class=\"text-600 text-90\">Issue Date:</span> "+DateTime.Now.Date+"</div>\r\n                                    <div class=\"my-2\"><i class=\"fa fa-circle text-blue-m2 text-xs mr-1\"></i> <span\r\n                                            class=\"text-600 text-90\">Status:</span> <span\r\n                                            class=\"badge badge-warning badge-pill px-25\">paid</span></div>\r\n                                </div>\r\n                            </div>\r\n                        </div>\r\n                        <div class=\"mt-4\">\r\n                            <div class=\"row text-600 text-white bgc-default-tp1 py-25\">\r\n                                <div class=\"d-none d-sm-block col-1\">#</div>\r\n                                <div class=\"col-9 col-sm-5\">Description</div>\r\n                                <div class=\"d-none d-sm-block col-4 col-sm-2\">Qty</div>\r\n                                <div class=\"d-none d-sm-block col-sm-2\">Unit Price</div>\r\n                                <div class=\"col-2\">Amount</div>\r\n                            </div>\r\n                            <div class=\"text-95 text-secondary-d3\">\r\n                                <div class=\"row mb-2 mb-sm-0 py-25\">\r\n                                    <div class=\"d-none d-sm-block col-1\">1</div>\r\n                                    <div class=\"col-9 col-sm-5\">"+coursename+"</div>\r\n                                    <div class=\"d-none d-sm-block col-2\">1</div>\r\n                                    <div class=\"d-none d-sm-block col-2 text-95\">Rs "+price+"</div>\r\n                                    <div class=\"col-2 text-secondary-d2\">Rs"+price+ "</div>\r\n                                </div>\r\n     </div>\r\n                     <div class=\"row border-b-2 brc-default-l2\"></div>\r\n                            <div class=\"row mt-3\">\r\n                                <div class=\"col-12 col-sm-7 text-grey-d2 text-95 mt-2 mt-lg-0\"> Extra note such as\r\n                                    company or payment information...</div>\r\n                                <div class=\"col-12 col-sm-5 text-grey text-90 order-first order-sm-last\">\r\n                                    <div class=\"row my-2\">\r\n                                                         <div class=\"row my-2 align-items-center bgc-primary-l3 p-2\">\r\n        <div class=\"col-7 text-right\"> Total Amount</div>\r\n                                        <div class=\"col-5\"> <span\r\n                                                class=\"text-150 text-success-d3 opacity-2\">$2,475</span></div>\r\n                                    </div>\r\n                                </div>\r\n                            </div>\r\n                            <hr />\r\n                            <div> <span class=\"text-secondary-d1 text-105\">Thank you for purchasing the course...!</span> <a\r\n                                    href=\"http://localhost:44337/Home/Details/"+courseid+"\" class=\"btn btn-info btn-bold px-4 float-right mt-3 mt-lg-0\">See the Course</a>\r\n                         <br><br>   </div>\r\n                        </div>\r\n                    </div>\r\n                </div>\r\n            </div>\r\n        </div>\r\n        \r\n    </div>\r\n    \r\n</body>\r\n\r\n</html>";


                mail.From = new MailAddress("haxkdmail@gmail.com");
                mail.To.Add(useremail);
                mail.Subject = "Purchased Course Summary";
                mail.Body = msg;
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