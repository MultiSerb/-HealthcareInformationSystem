using ProjekatBrabo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace ProjekatBrabo.Controllers
{
    public class AccountController : Controller
    {
        private readonly string filePath;

        public AccountController()
        {
            // Direktna definicija relativne putanje i konverzija u apsolutnu putanju
            filePath = HostingEnvironment.MapPath("~/App_Data/users.txt");
        }
        private List<User> GetAllUsers()
        {
            var users = new List<User>();
            var lines = System.IO.File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 3)
                {
                    users.Add(new User { KorisnickoIme = parts[0], Sifra = parts[1], Tip = parts[2] });
                }
            }
            return users;
        }

        private User GetUser(string username, string password)
        {
            return GetAllUsers().SingleOrDefault(u => u.KorisnickoIme == username && u.Sifra == password);
        }

        public ActionResult Logout()
        {
            // Očistite sesiju
            Session.Clear();

            // Preusmerite korisnika na početnu stranicu ili stranicu za prijavu
            return RedirectToAction("login", "account");
        }



        // GET: Account
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User model)
        {
            if (ModelState.IsValid)
            {
                // Provera da li korisnik postoji u txt fajlu
                var user = GetUser(model.KorisnickoIme, model.Sifra);
                if (user != null)
                {
                    // Korisnik je pronađen, implementirati logiku za prijavu
                    Session["Username"] = user.KorisnickoIme;
                    Session["UserType"] = user.Sifra; // Čuvanje tipa korisnika u sesiji
                   
                    // Preusmeravanje na različite prikaze u zavisnosti od tipa korisnika
                    switch (user.Tip.ToLower())
                    {
                        case "pacijent":
                            return RedirectToAction("Index", "Pacijent");
                        case "doctor":
                            return RedirectToAction("index", "Doctor");
                        case "admin":
                            return RedirectToAction("Index", "Admin");
                        default:
                            ModelState.AddModelError("", "Los tip.");
                            break;
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Ne postoji korisnik taj");
                }
            }

            return View(model);
        }

        
    }
}