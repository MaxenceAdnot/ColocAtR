using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ASPNETColocAtR.Models
{
    public class ProfilWizardModel
    {
        [Required(ErrorMessage = "Type de profil requis")]
        [Display(Name = "Type de profil")]
        public bool type { get; set; }

        [Required(ErrorMessage = "Age requis")]
        [Display(Name = "Age")]
        public int age { get; set; }

        [Required(ErrorMessage = "Prix requis")]
        [Display(Name = "Loyer")]
        public int price { get; set; }

        [Required(ErrorMessage = "Ville requise")]
        [Display(Name = "Ville")]
        public string city { get; set; }

        [Display(Name = "Description")]
        public string desc { get; set; }

        [Display(Name = "M² recherchés")]
        public int m2 { get; set; }

    }
}