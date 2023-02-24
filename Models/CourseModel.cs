using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTPSKANPUR.Models
{
    public class CourseModel
    {
        public string name { get; set; }
        public string price { get; set; }
        public HttpPostedFileBase image { get; set; }
        public string description { get; set; }
    }
}