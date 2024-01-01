using BTPSKANPUR.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static System.Net.WebRequestMethods;

namespace BTPSKANPUR.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        BTPSKANPUREntities btps = new BTPSKANPUREntities();
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddCourse(CourseModel course)
        {
            if (ModelState.IsValid)
            {
                Random rnd = new Random();
                string mypath = "~/img/Course";
                string _FileName = Path.GetFileName(course.image.FileName);

                _FileName = rnd.Next() + _FileName;
                string _path = Path.Combine(Server.MapPath(mypath), _FileName);
                course.image.SaveAs(_path);
                Course cr = new Course()
                {
                    name = course.name,
                    image = mypath + "/" + _FileName,
                    price = course.price,
                    description = course.description
                };
                btps.Courses.Add(cr);
                btps.SaveChanges();
                return RedirectToAction("showCourses");
            }
            else
            {
                return RedirectToAction("Index");
            }
            //return View();
        }
   
        public ActionResult showCourses()
        {
            var data = btps.Courses.ToList();
            return View(data);
        }
        
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("showCourses");
            }
            var data = btps.Courses.FirstOrDefault(x => x.id == id);
            
            
            if (data == null)
            {
                return RedirectToAction("showCourses");
            }
            return View(data);
        }


        [HttpPost]
        public ActionResult Edit(int id,CourseModel course)
        {

            Course cr = btps.Courses.FirstOrDefault(x => x.id == id);


            HttpPostedFileBase image = course.image;
            if (image != null)
            {
                string oldpath = Path.Combine(Server.MapPath(cr.image));
                System.IO.File.Delete(oldpath);
                Random rnd = new Random();
                string mypath = "~/img/Course";
                string _FileName = Path.GetFileName(course.image.FileName);

                _FileName = rnd.Next() + _FileName;
                string _path = Path.Combine(Server.MapPath(mypath), _FileName);
                course.image.SaveAs(_path);
                cr.image = mypath + "/" + _FileName;
            }
            

            cr.name = course.name;
            cr.price = course.price;
            cr.description = course.description;
            btps.SaveChanges();
            return View(cr);
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("showCourses");
            }
            var data = btps.Courses.FirstOrDefault(x => x.id == id);


            if (data == null)
            {
                return RedirectToAction("showCourses");
            }

            string oldpath = Path.Combine(Server.MapPath(data.image));
            System.IO.File.Delete(oldpath);
            btps.Courses.Remove(data);
            btps.SaveChanges();
            return RedirectToAction("showCourses");
        }



    }
}
