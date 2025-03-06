using ProjekatBrabo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;

namespace ProjekatBrabo.Controllers
{
    public class AdminController : Controller
    {
        private readonly string filePath;
        private readonly string filePath2;

        public AdminController()
        {
            filePath = HostingEnvironment.MapPath("~/App_Data/pacijenti.txt");
            filePath2 = HostingEnvironment.MapPath("~/App_Data/users.txt");
        }

        private List<Pacijent> GetAllPacijenti()
        {
            var pacijenti = new List<Pacijent>();
            var lines = System.IO.File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 7) // Adjust the length based on your file structure
                {
                    pacijenti.Add(new Pacijent
                    {
                        KorisnickoIme = parts[0],
                        JMBG = parts[1],
                        Sifra = parts[2],
                        Ime = parts[3],
                        Prezime = parts[4],
                        DatumRodjenja = DateTime.Parse(parts[5]),
                        ElektronskaPosta = parts[6],
                        // ZakazaniTermini = Deserialize the scheduled appointments if any
                    });
                }
            }
            return pacijenti;
        }
        private List<User> GetAllUsers()
        {
            var useri = new List<User>();
            var lines = System.IO.File.ReadAllLines(filePath2);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 3) // Adjust the length based on your file structure
                {
                    useri.Add(new User
                    {
                        KorisnickoIme = parts[0],
                        
                        Sifra = parts[1],
                        Tip=parts[2]
                        
                    });
                }
            }
            return useri;
        }

        private void AppendPacijent(Pacijent pacijent)
        {
            string s= "pacijent";
            var line = $"{pacijent.KorisnickoIme}:{pacijent.JMBG}:{pacijent.Sifra}:{pacijent.Ime}:{pacijent.Prezime}:{pacijent.DatumRodjenja.ToString("yyyy-MM-dd")}:{pacijent.ElektronskaPosta}";
            System.IO.File.AppendAllLines(filePath, new[] { line });
            var line2 = $"{pacijent.KorisnickoIme}:{pacijent.Sifra}:{s}";
            System.IO.File.AppendAllLines(filePath2, new[] { line2 });

            // Logovanje za proveru pisanja

        }

        private void SavePacijenti(List<Pacijent> pacijenti)
        {
            var lines = pacijenti.Select(p => $"{p.KorisnickoIme}:{p.JMBG}:{p.Sifra}:{p.Ime}:{p.Prezime}:{p.DatumRodjenja.ToString("yyyy-MM-dd")}:{p.ElektronskaPosta}");
            System.IO.File.WriteAllLines(filePath, lines);

            // Logovanje za proveru pisanja
            
            foreach (var line in lines)
            {
                System.Diagnostics.Debug.WriteLine(line);
            }
        }

        // GET: Admin/Index
        public ActionResult Index()
        {
            return View(GetAllPacijenti());
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pacijent pacijent)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    // Logovanje grešaka
                    System.Diagnostics.Debug.WriteLine(error);
                }
                return View(pacijent);
            }

            var pacijenti = GetAllPacijenti();
            var useri = GetAllUsers();

            // Provera jedinstvenosti korisničkog imena, emaila i JMBG-a
            if (pacijenti.Any(p => p.KorisnickoIme == pacijent.KorisnickoIme))
            {
                ModelState.AddModelError("", "Korisničko ime već postoji u korisnicima.");
                return View(pacijent);
            } else if (pacijenti.Any(p => p.JMBG == pacijent.JMBG))
            {
                ModelState.AddModelError("", " jbmg vec postoji.");
                return View(pacijent);
            }
            else if (pacijenti.Any(p => p.ElektronskaPosta == pacijent.ElektronskaPosta))
            {
                ModelState.AddModelError("", "E posta vec postoji ime već postoji.");
                return View(pacijent);

            } else if (useri.Any(p => p.KorisnickoIme == pacijent.KorisnickoIme)) {
                ModelState.AddModelError("", "To je korisnicko ime admina admina ili doktora.");
                return View(pacijent);
            }




                AppendPacijent(pacijent); // Dodajemo novog pacijenta u fajl
            return RedirectToAction("Index");
        }

        // GET: Admin/Edit/{id}
        public ActionResult Edit(string id)
        {
            var pacijent = GetAllPacijenti().FirstOrDefault(p => p.KorisnickoIme == id);
            if (pacijent == null)
            {
                return HttpNotFound();
            }
            return View(pacijent);
        }

        // POST: Admin/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Pacijent pacijent)
        {
            if (ModelState.IsValid)
            {
                var pacijenti = GetAllPacijenti();
                var existingPacijent = pacijenti.FirstOrDefault(p => p.KorisnickoIme == pacijent.KorisnickoIme);
                if(existingPacijent.Sifra != pacijent.Sifra){
                    var users = GetAllUsers();
                    var existinguser = users.FirstOrDefault(p => p.KorisnickoIme == pacijent.KorisnickoIme);
                    existinguser.Sifra = pacijent.Sifra;

                    var lines = users.Select(p => $"{p.KorisnickoIme}:{p.Sifra}:{p.Tip}");
                    System.IO.File.WriteAllLines(filePath2, lines);


                }

                if (existingPacijent != null)
                {
                    // Ažuriranje polja osim KorisnickoIme i JMBG
                    existingPacijent.Sifra = pacijent.Sifra;
                    existingPacijent.Ime = pacijent.Ime;
                    existingPacijent.Prezime = pacijent.Prezime;
                    existingPacijent.DatumRodjenja = pacijent.DatumRodjenja;
                    existingPacijent.ElektronskaPosta = pacijent.ElektronskaPosta;

                    SavePacijenti(pacijenti); // Sačuvamo sve pacijente u fajl
                    return RedirectToAction("Index");
                }
            }
            return View(pacijent);
        }

        // GET: Admin/Delete/{id}
        public ActionResult Delete(string id)
        {
            var pacijent = GetAllPacijenti().FirstOrDefault(p => p.KorisnickoIme == id);
            if (pacijent == null)
            {
                return HttpNotFound();
            }
            return View(pacijent);
        }

        // POST: Admin/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var pacijenti = GetAllPacijenti();
            var pacijent = pacijenti.FirstOrDefault(p => p.KorisnickoIme == id);
            if (pacijent != null)
            {
                var users = GetAllUsers();
                var user = users.FirstOrDefault(p => p.KorisnickoIme == id);
                users.Remove(user);
                pacijenti.Remove(pacijent);
                SavePacijenti(pacijenti); // Sačuvamo sve pacijente u fajl

                var lines = users.Select(p => $"{p.KorisnickoIme}:{p.Sifra}:{p.Tip}");//cuvanje u user fajl
                System.IO.File.WriteAllLines(filePath2, lines);
            }
            return RedirectToAction("Index");
        }

        // GET: Admin/Details/{id}
        public ActionResult Details(string id)
        {
            var pacijent = GetAllPacijenti().FirstOrDefault(p => p.KorisnickoIme == id);
            if (pacijent == null)
            {
                return HttpNotFound();
            }
            return View(pacijent);
        }
    }
}
