using BTPSKANPUR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

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
            return View();
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
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var data = btps.Courses.FirstOrDefault(c => c.id == id);

            if (data == null)
            {
                return RedirectToAction("Index");
            }

            return View();
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


    }
}