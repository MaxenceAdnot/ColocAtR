using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ASPNETColocAtR.Models
{
    public class ChangePasswordModel
    {
        [Required]
        [Display(Name = "Ancien mot de passe")]
        [DataType(DataType.Password)]
        public string oldPassword { get; set; }

        [Required]
        [Display(Name = "Nouveau mot de passe")]
        [DataType(DataType.Password)]
        public string newPassword { get; set; }

        [Required]
        [Display(Name = "Confirmation")]
        [DataType(DataType.Password)]
        //[Compare("newPassword", ErrorMessage = "Mots de passe différents")]
        public string confirmPassword { get; set; }
    }
}