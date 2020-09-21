using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using quizz.Models;

namespace quizz.Controllers
{
    public class MojController : ArClass
    {
      
    }

    public class ArClass : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

       
        [Route("/A/Answer")]
        public IActionResult Answer(int id)
        {
            string un = Request.Cookies["username"];

            if (string.IsNullOrWhiteSpace(un))
                return View("Error", "Invalid username!");

            ARQuizz.Session s = ARQuizz.Session.GetCurrentSession(un);

            if (s == null)
                return View("Error", "Session timed out!");

            s.Answer(id);

            if (s.IsLastQuestion())
                return Redirect("/Home/GetQuizzResult/-1");

            return Redirect("/Home/GetNextQuestion?UserName=" + un);
        }
        [Route("/Quizz/GetResult/{id}")]
        public IActionResult GetQizzResult(int id)
        {
            // Ucitati rezultat iz baze. Ukoliko je id == -1 onda ucitati poslednji rezultat za username iz cookija

            return View("Result", null);
        }
    }
}
