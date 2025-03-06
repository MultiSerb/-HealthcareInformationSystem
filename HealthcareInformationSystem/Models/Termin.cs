using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProjekatBrabo.Models
{
    public class Termin
    {
        
        public string Lekar { get; set; }
        public bool Status { get; set; }
        [Display(Name = "Datum i vreme")]
        [Required(ErrorMessage = " obavezano")]
        public DateTime DatumVreme { get; set; }
        public string OpisTerapije { get; set; }
    }


}