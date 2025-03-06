using ProjekatBrabo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;

namespace ProjekatBrabo.Controllers
{
    public class PacijentController : Controller
    {
        private readonly string terminiFilePath = HostingEnvironment.MapPath("~/App_Data/termini.txt");
        private readonly string doktorifile = HostingEnvironment.MapPath("~/App_Data/lekari.txt");
        private readonly string knjigafile = HostingEnvironment.MapPath("~/App_Data/knjizica.txt");

        // Metoda za dobijanje svih lekara iz fajla
        private List<Doktor> GetAllDoctors()
        {
            var lekari = new List<Doktor>();
            var lines = System.IO.File.ReadAllLines(doktorifile);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 6)
                {
                    var lekar = new Doktor
                    {
                        KorisnickoIme = parts[0],
                        Sifra = parts[1],
                        Ime = parts[2],
                        Prezime = parts[3],
                        DatumRodjenja = DateTime.Parse(parts[4]),
                        ElektronskaPosta = parts[5],
                        Termini = new List<Termin>()
                    };

                    lekari.Add(lekar);
                }
            }
            return lekari;
        }

        // Metoda za dobijanje svih termina iz fajla
        private List<Termin> GetAllAppointments()
        {
            var termini = new List<Termin>();
            var lines = System.IO.File.ReadAllLines(terminiFilePath);
            foreach (var line in lines)
            {
                var parts = line.Split('*');
                if (parts.Length == 4)
                {
                    var termin = new Termin
                    {
                        Lekar = parts[0],
                        Status = bool.Parse(parts[1]),
                        DatumVreme = DateTime.Parse(parts[2]),
                        OpisTerapije = parts[3]
                    };
                    termini.Add(termin);
                }
            }
            return termini;
        }

        private List<ZdravstveniKarton> GetAllknijizice()
        {
            var knjizica = new List<ZdravstveniKarton>();
            var lines = System.IO.File.ReadAllLines(knjigafile);
            foreach (var line in lines)
            {
                var parts = line.Split('!');
                if (parts.Length == 2)
                {
                    var z = new ZdravstveniKarton
                    {
                        Pacijent = parts[0],
                        Termini = new List<Termin>()
                    };
                    var parts2 = parts[1].Split('+');

                    foreach (var line2 in parts2)
                    {
                        var parts3 = line2.Split('*');
                        if (parts3.Length == 4)
                        {
                            var termin = new Termin
                            {
                                Lekar = parts3[0],
                                Status = bool.Parse(parts3[1]),
                                DatumVreme = DateTime.Parse(parts3[2]),
                                OpisTerapije = parts3[3]
                            };
                            z.Termini.Add(termin);
                        }
                    }

                    knjizica.Add(z);
                }
            }
            return knjizica;
        }

        private void SaveAllKnjizice(List<ZdravstveniKarton> knjizice)
        {
            var lines = new List<string>();
            foreach (var knjizica in knjizice)
            {
                var termini = knjizica.Termini.Select(t => $"{t.Lekar}*{t.Status}*{t.DatumVreme}*{t.OpisTerapije}");
                var line = $"{knjizica.Pacijent}!{string.Join("+", termini)}";
                lines.Add(line);
            }
            System.IO.File.WriteAllLines(knjigafile, lines);
        }

        // GET: Pacijent/Zakazi
        public ActionResult Zakazi(string doktorKorisnickoIme, DateTime datumVreme)
        {
            var doktori = GetAllDoctors();
            var izabraniDoktor = doktori.FirstOrDefault(d => d.KorisnickoIme == doktorKorisnickoIme);

            if (izabraniDoktor != null)
            {
                var model = new Termin
                {
                    Lekar = izabraniDoktor.KorisnickoIme,
                    Status = false, // Termin je zauzet kada ga pacijent zakazuje
                    DatumVreme = datumVreme,
                    OpisTerapije = " "
                };

                var termini = GetAllAppointments();
                var termin = termini.FirstOrDefault(d => d.Lekar == doktorKorisnickoIme && d.DatumVreme == datumVreme);
                termini.Remove(termin);
                termini.Add(model);
                var lines = termini.Select(t => $"{t.Lekar}*{t.Status}*{t.DatumVreme}*{t.OpisTerapije}");
                System.IO.File.WriteAllLines(terminiFilePath, lines);

                var knjizice = GetAllknijizice();
                var knjizica = knjizice.FirstOrDefault(d => d.Pacijent == Session["Username"] as string);
                if (knjizica != null)
                {
                    knjizica.Termini.Add(model);
                }
                else
                {
                    var k = new ZdravstveniKarton
                    {
                        Pacijent = Session["Username"] as string,
                        Termini = new List<Termin> { model }
                    };
                    knjizice.Add(k);
                }
                SaveAllKnjizice(knjizice);

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index"); // Preusmeravanje na početnu stranicu u slučaju greške
        }

        // GET: Pacijent
        public ActionResult Index()
        {
            string pacijent = Session["Username"] as string;

            var sviTermini = GetAllAppointments();
            var sveknjige = GetAllknijizice();
            var model = new PacijentViewModel
            {
                ZauzetiTermini = new List<Termin>(),
                SlobodniTermini = sviTermini.Where(t => t.Status == true).ToList()
            };

            foreach (var knjizica in sveknjige)
            {
                if (knjizica.Pacijent == pacijent)
                {
                    model.ZauzetiTermini = knjizica.Termini;
                    break;
                }
            }

            return View(model);
        }

        // GET: Pacijent/ViewTherapy
        public ActionResult ViewTherapy(DateTime datumVreme)
        {
            var termin = GetAllAppointments().FirstOrDefault(t => t.DatumVreme == datumVreme);
            if (termin == null)
            {
                return HttpNotFound();
            }
            return View(termin);
        }
    }

    public class PacijentViewModel //za prikaz tabela
    {
        public List<Termin> ZauzetiTermini { get; set; }
        public List<Termin> SlobodniTermini { get; set; }
    }
}
