using BTPSKANPUR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
    }
}