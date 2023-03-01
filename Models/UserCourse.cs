using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTPSKANPUR.Models
{
    public class UserCourse
    {
        public User user { get; set; }
        public List<BoughtCours> bought { get; set; }
    }
}