using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjekatBrabo.Models
{
    public class ZdravstveniKarton
    {
        public List<Termin> Termini { get; set; }
        public string Pacijent { get; set; }
    }
}