using ProjekatBrabo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;

namespace ProjekatBrabo.Controllers
{
    public class DoctorController : Controller
    {
        private readonly string terminiFilePath = HostingEnvironment.MapPath("~/App_Data/termini.txt");
        private readonly string doktorifile = HostingEnvironment.MapPath("~/App_Data/lekari.txt");
        private readonly string knjigafile = HostingEnvironment.MapPath("~/App_Data/knjizica.txt");

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

        private List<Doktor> Getlekari()
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

        // Metoda za dobijanje svih zdravstvenih kartona iz fajla
        private List<ZdravstveniKarton> GetAllKnjizice()
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

        // Metoda za dobijanje termina koje je kreirao određeni lekar
        private List<Termin> GetAppointmentsByDoctor(string doctorUsername)
        {
            return GetAllAppointments().Where(t => t.Lekar == doctorUsername).ToList();
        }

        // GET: Doctor/Index
        public ActionResult Index()
        {
            var doctorUsername = (string)Session["Username"];
            var appointments = GetAppointmentsByDoctor(doctorUsername);

            return View(appointments);
        }

        // GET: Doctor/CreateAppointment
        public ActionResult CreateAppointment()
        {
            return View();
        }

        // POST: Doctor/CreateAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult CreateAppointment(Termin model)
        {
            if (ModelState.IsValid)
            {
                var doctorUsername = (string)Session["Username"];
                var newAppointment = new Termin
                {
                    Lekar = doctorUsername,
                    Status = true, // Novi termin je slobodan
                    DatumVreme = model.DatumVreme,
                    OpisTerapije = string.Empty // Nema opisa terapije pri kreiranju termina
                };

                // Proveri da li termin već postoji
                var existingAppointments = System.IO.File.ReadAllLines(terminiFilePath)
                    .Select(appointmentLine => appointmentLine.Split('*'))
                    .Select(parts => new Termin
                    {
                        Lekar = parts[0],
                        Status = bool.Parse(parts[1]),
                        DatumVreme = DateTime.Parse(parts[2]),
                        OpisTerapije = parts[3]
                    });

                if (existingAppointments.Any(a => a.DatumVreme == newAppointment.DatumVreme && a.Lekar == doctorUsername))
                {
                    ViewBag.ErrorMessage = "Termin već postoji za zadato vreme. Molimo odaberite drugi datum i vreme.";
                    return View(model);
                }

                // Dodavanje novog termina u fajl
                var newLine = $"{newAppointment.Lekar}*{newAppointment.Status}*{newAppointment.DatumVreme}*{newAppointment.OpisTerapije}";
                System.IO.File.AppendAllLines(terminiFilePath, new[] { newLine });

                return RedirectToAction("Index");
            }

            return View(model);
        }




        // GET: Doctor/PrescribeTherapy
        public ActionResult PrescribeTherapy(DateTime datumVreme)
        {
            var doctorUsername = (string)Session["Username"];
            var appointment = GetAllAppointments().SingleOrDefault(t => t.Lekar == doctorUsername && t.DatumVreme == datumVreme);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }

        // POST: Doctor/PrescribeTherapy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PrescribeTherapy(DateTime datumVreme, string therapyDescription)
        {
            var doctorUsername = (string)Session["Username"];
            var appointments = GetAllAppointments();
            var appointment = appointments.SingleOrDefault(t => t.Lekar == doctorUsername && t.DatumVreme == datumVreme);
            if (appointment == null)
            {
                return HttpNotFound();
            }

            appointment.OpisTerapije = therapyDescription;

            // Ažuriranje termina u fajlu termini.txt
            var lines = appointments.Select(t => $"{t.Lekar}*{t.Status}*{t.DatumVreme}*{t.OpisTerapije}");
            System.IO.File.WriteAllLines(terminiFilePath, lines);

            // Ažuriranje termina u zdravstvenom kartonu pacijenta
            var knjizice = GetAllKnjizice();
            foreach (var knjizica in knjizice)
            {
                var pacijentTermin = knjizica.Termini.FirstOrDefault(t => t.Lekar == doctorUsername && t.DatumVreme == datumVreme);
                if (pacijentTermin != null)
                {
                    pacijentTermin.OpisTerapije = therapyDescription;
                }
            }
            SaveAllKnjizice(knjizice);

            return RedirectToAction("Index");
        }

        // GET: Doctor/ViewTherapy
        public ActionResult ViewTherapy(DateTime datumVreme)
        {
            var doctorUsername = (string)Session["Username"];
            var appointment = GetAllAppointments().SingleOrDefault(t => t.Lekar == doctorUsername && t.DatumVreme == datumVreme);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }
    }
}
