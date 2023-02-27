using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
namespace BTPSKANPUR.Models
{
    public class UserModel
    {
        [Required]
        public string name { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        [Compare("password",ErrorMessage ="Password and confirm passsword doesnt matched")]
        public string cpassword { get; set; }
    }
}