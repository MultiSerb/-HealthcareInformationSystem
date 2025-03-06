using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProjekatBrabo.Models
{
    public class Pacijent
    {
        [Required(ErrorMessage = " obavezano")]
        [Display(Name = "Korisničko Ime")]
        public string KorisnickoIme { get; set; }


        [Required(ErrorMessage = " obavezano")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG mora imati tačno 13 cifara.")]
        [RegularExpression(@"\d{13}", ErrorMessage = "JMBG mora sadržavati samo cifre.")]
        public string JMBG { get; set; }
        [Required(ErrorMessage = " obavezano")]
        public string Sifra { get; set; }
        [Required(ErrorMessage = " obavezano")]
        public string Ime { get; set; }
        [Required(ErrorMessage = " obavezano")]
        public string Prezime { get; set; }
        [Display(Name = "Datum Rodjenja ")]
        [Required(ErrorMessage = "obavezano")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DatumRodjenja { get; set; }
        [Required(ErrorMessage = " obavezano")]
        [Display(Name = "Elektronska Posta ")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Mora biti Gmail adresa (npr. ime@gmail.com).")]
        public string ElektronskaPosta { get; set; }
        public List<Termin> ZakazaniTermini { get; set; }
    }
}