using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ProjekatBrabo.Models
{
    public class User
    {
        [Display(Name = "Korisničko Ime")]
        public string KorisnickoIme { get; set; }
        [Display(Name = "Šifra")]
        public string Sifra { get; set; }
        public string Tip { get; set; }
    }
}