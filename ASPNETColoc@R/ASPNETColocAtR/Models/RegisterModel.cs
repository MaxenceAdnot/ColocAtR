using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ASPNETColocAtR.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Nom d'utilisateur")]
        [Display(Name = "Nom d'utilisateur")]
        public string username { get; set; }

        [Required(ErrorMessage = "Adresse mail requise")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string email { get; set; }

        [Required(ErrorMessage = "Mot de passe requis")]
        [Display(Name = "Mot de passe")]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [Required(ErrorMessage="Prénom requis")]
        [Display(Name = "Prénom")]
        public string firstname { get; set; }

        [Required(ErrorMessage="Nom requis")]
        [Display(Name = "Nom")]
        public string lastname { get; set; }
    }
}
