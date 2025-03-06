using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjekatBrabo.Models
{
    public class Doktor
    {
      

       

        public string KorisnickoIme { get; set; }
        public string Sifra { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public DateTime DatumRodjenja { get; set; }
        public string ElektronskaPosta { get; set; }
        public List<Termin> Termini { get; set; }
    }

}