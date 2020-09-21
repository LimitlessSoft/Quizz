using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using quizz.Models;
using Ubiety.Dns.Core;

namespace quizz.Controllers
{
    public class HomeController : ARMVCQuizz
    {
        public HomeController() : base(ARMVCQuizzIdentifier.ID, new ARMVCQuizzSettings(ASD))
        {

        }


        public static int ASD()
        {
            int a = 0;
            int b = 2;
            return 1;
        }
    }

    //public int z() // Usao u radnju (1 po 1 sa razmakom koliko treba da se izvrsi radnja za tog jednog (10 sekundi))
    //{
    //    int b = 10; // Izabrao namirnice
    //    Thread.Sleep(10000); // Mi mu kucamo 10s
    //    return b; // Nakon 10 sekundi izlazi iz radnje, a potom primamo novog kupca
    //}
    //public int c() // Usao u radnju (1 po 1 sa razmakom koliko treba da se izvrsi radnja za tog jednog (0.001 sekundi))
    //{
    //    int b = 10; // Izabrao namirnice
    //    Task.Run(() => {
    //        Thread.Sleep(10000);
    //    }); // Krenuli da kucamo racun i u isto vreme rekli da ide kuci
    //    return b; // Otisao kuci
    //    // mi tek nakon 10 sekunid zavrsili racun i bacili ga u kantu zajedno sa namirnicima jer je ludak otisao kuci
    //}
    //public async Task<int> a() // Usao u radnju (1 po 1 sa razmakom koliko treba da se izvrsi radnja za tog jednog (0.001 sekundi))
    //{
    //    int b = 10; // Izabrao namirnice

    //    await Task.Run(() => {
    //        Thread.Sleep(10000);
    //    }); // Otisao na posebnu kasu sa novim kasirom koji mu kuca racun i tamo ceka da se zavrsi (10 sekundi), a nakon toga ide kuci

    //    // Primamo novog kupca odmah sada jer nam je slobodna kasa

    //    return b;
    //}

    public enum ARMVCQuizzIdentifier
    {
        ID = 0,
        Text = 1
    }

    public class ARMVCQuizzSettings
    {
        public delegate int GetIdentifierDel();

        public GetIdentifierDel GetIdentifier;

        public ARMVCQuizzSettings(GetIdentifierDel getIdentifier)
        {
            this.GetIdentifier = getIdentifier;
        }
    }

    public abstract class ARMVCQuizz : Controller
    {
        private ARMVCQuizzIdentifier _Identifier = ARMVCQuizzIdentifier.ID;
        private ARMVCQuizzSettings _Settings = null;
        private string rute = "";
        static ARMVCQuizz()
        {
            Models.ARQuizz.Buffer.Initialize();
        }
        public ARMVCQuizz(ARMVCQuizzIdentifier identifier, ARMVCQuizzSettings settings)
        {
            this._Identifier = identifier;
            this._Settings = settings;
        }

        #region Routes
        #region Answer
        [Route("/Quizz/Answer")]
        public async Task<IActionResult> Answer(int id)
        {
            return await Task.Run<IActionResult>(() =>
            {
                string un = Request.Cookies["username"];

                if (string.IsNullOrWhiteSpace(un))
                    return View("Error", "Invalid username!");

                ARQuizz.Session s = ARQuizz.Session.GetCurrentSession(un);

                if (s == null)
                    return Redirect("/Quizz/ShowYourResult/" + un);

               Task.WaitAll(s.Answer(id));

                if (s.IsLastQuestion())
                    return Redirect("/Quizz/Result/-1");

                if (id == -1)
                    return Redirect("/Quizz/GetQuizzResult?UserName=" + un);

                return Redirect("/Quizz/Question/Next?UserName=" + un);
            });
        }
        #endregion

        #region Result
        #region Global

        /// <summary>
        /// You need
        /// </summary>
        /// <returns></returns>
        [Route("/Quizz")]
        [Route("/Quizz/{id}/{username}")]
        public IActionResult Index(int? id, string? username)
        {
          if(username != null && id != null)
            {

            }
        }

        public IActionResult IndexID()
        {
            if (_Identifier == ARMVCQuizzIdentifier.Text)
                return RedirectToAction("IndexText");

            // Provera da li je korisnik logovan
            
            return View("/Views/ARQuizz/IndexID.cshtml");
        }

        public IActionResult IndexText()
        {
            if(_Identifier == ARMVCQuizzIdentifier.ID)
                return RedirectToAction("IndexID");

            return View("/Views/ARQuizz/IndexText.cshtml");
        }
        [Route("/Quizz/Result/{id}")]
        public async Task<IActionResult> GetQizzResult(int id)
        {
            
            return await Task.Run<IActionResult>(() =>
            {
              
                List<ARQuizz.Session> list = ARQuizz.Session.List();

                list = ARQuizz.Session.List();

                if (id == -1)
                {
                    string un = Request.Cookies["username"];
                    if (string.IsNullOrEmpty(un))
                        return View("Error", "User does not exist");

                    ARQuizz.Session  s = list.Where(x => x.UserName == un).OrderBy(t => t.End).Take(1).FirstOrDefault();
                    return View("Session", list.Where(x => x.UserName == un).OrderBy(t => t.End).Take(1).ToList());
                }

                // Ucitati rezultat iz baze. Ukoliko je id == -1 onda ucitati poslednji rezultat za username iz cookija
                return View("Session", list);
            });

        }
        [Route("/Quizz/Results/{UserName}")]
        public async Task<IActionResult> ShowAllUserResult(string UserName)
        {
            return await Task.Run<IActionResult>(() =>
            {
                List<ARQuizz.Session> ls = ARQuizz.Session.List().Where(x => x.UserName == UserName).ToList(); ;
                if (ls.Count > 0)
                    return View("SessionResults", ls);
                return View("Error", "The user has not complited quizz");
            });
        }
        [Route("/Quizz/Results/Best")]
        public async Task<IActionResult> BestUserResult()
        {
            return await Task.Run<IActionResult>(() =>
            {
                List<ARQuizz.Session> session = ARQuizz.Session.List();
                List<ARQuizz.Session> best = new List<ARQuizz.Session>();
                List<string> username = session.Select(t => t.UserName).Distinct().ToList();
                for (int i = 0; i < username.Count; i++)
                {
                    best.Add(session.Where(t => t.UserName == username[i] && t.Score == session.Where(t => t.UserName == username[i]).Max(t => t.Score)).FirstOrDefault());

                }

                return View("Session", best);


            });

        }

        [Route("/Quizz/Result/{name}/{id}")]
        public async Task<IActionResult> ResultUser(string name, int id)
        {
            return await Task.Run<IActionResult>(() => {
                List<ARQuizz.Session> list = ARQuizz.Session.List();
                return View("Session", list.Where(t => t.UserName == name && t.ID == id).ToList());
            });
        }
        #endregion
        #endregion
        #region Admin

        [Route("/Quizz/ControlPanel")]
        public IActionResult ControlPanelIndex()
        {
            return View();

        }

        [HttpGet]
        [Route("/Quizz/ControlPanel/RefreshBuffer")]
        public async Task<IActionResult> ControlPanelRefreshBuffers()
        {
            return await Task.Run<IActionResult>(async () =>
            {
                await ARQuizz.Buffer.RefreshAsync();
                return Json("Succes refresh all buffers");
            });
        }


        #endregion

        #region Question
        #region Type
        [Route("/Quizz/Question/Type/{id}/{name}/{time}/{lenght}")]
        public async Task<IActionResult> TypeChange(int id, string name, int time, int lenght)
        {
           return await Task.Run<IActionResult>(() => {
               Task.WaitAll(ARQuizz.QuestionType.DBUpdateAsync(id, name, time, lenght));
                return Redirect("/Quizz/Question/Types");
            });
        }
        [Route("/Quizz/Question/Type/{id}")]
        public IActionResult Type(int id)
        {
            return View("TypeChange",ARQuizz.Buffer.QuestionTypes.Where(t => t.ID == id).FirstOrDefault());
        }
        [Route("/Quizz/Question/Types")]
        public IActionResult Types()
        {
            return View("TypeList", ARQuizz.Buffer.QuestionTypes);
        }

        [Route("/Quizz/Type/New")]
        public IActionResult TypesAdd()
        {
            return View("TypeNew");
        }
        #endregion

        [Route("/Quizz/Question/New")]
        public IActionResult QuestionNew()
        {
            ARQuizz.Question question = new ARQuizz.Question();
            return View(question);
        }
        [Route("/Quizz/Question/List")]
        public async Task<IActionResult> QuestionList()
        {
            return await Task.Run<IActionResult>(() =>
            {
                return View("QuestionList", ARQuizz.Question.List(0));
            });
        }
        [Route("/Quizz/Question/Change/{id}")]
        public IActionResult QuestionChange(int ID)
        {
            return View("Questionchange", ARQuizz.Buffer.Questions.Where(t=>t.ID == ID).FirstOrDefault());
        }
        [Route("/Quizz/Question/Next")]
        public async Task<IActionResult> QuestionNext(string UserName, int Type)
        {
          

            return await Task.Run<IActionResult>(() => {
                ARQuizz.Question q = null;
                ARQuizz.Session s = null;

                if (string.IsNullOrWhiteSpace(Request.Cookies["username"]))
                    Response.Cookies.Append("username", UserName);

                s = ARQuizz.Session.GetCurrentSession(Request.Cookies["username"]);
                if (s == null)
                    s = new ARQuizz.Session(UserName, Type);
                if (!s.Check())
                    q = s.GetNextQuestion();


                if (s == null)
                {
                    return View("Error", "Session time out!");
                }

                if (q == null)
                {
                    Task.WaitAll(s.Finish());
                    return Redirect("/Quizz/Result/-1");
                }

                return View("Question", q);
            });

        }
        #endregion

        #endregion

        #region API
        [HttpPost]
        [Route("/Quizz/Question/Update")]
        public async Task<IActionResult> QuestionUpdate([FromBody]ARQuizz.Question question)
        {
            if(question.Answers.Count < 0)
                return Json("Morate uneti odgovor na pitanje");
            
            if(question.Answers.Count == 1)
                return Json("Morate imati vise od jednog odgovora na pitanje");

            if(question.Answers.Any(t=>t.Score == 0))
                return Json("Odgovor na pitanje ne moze nositi 0 poena");

            if(question.Answers.Any(t=>t.Text == ""))
                return Json("Morate popuniti odgovor na pitanje");
           
            return await Task.Run(() =>
            {
                try
                {
                    question.DBUpdate();
                    return Json("1");
                }
                catch(Exception)
                {
                    return Json("0");
                }
            });
        }
        [HttpGet]
        [Route("/Quizz/Answer/Update")]
        public async Task<IActionResult> AnswerUpdate(int QuestionID, ARQuizz.Answer answer)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Validacija da li je administrator
                    // Validacija da li answer ima ispravne podatke

                    ARQuizz.Question q = new ARQuizz.Question(QuestionID);
                    q.Answers.RemoveAll(x => x.ID == answer.ID);
                    q.Answers.Add(answer);
                    q.DBUpdate();

                    return Json("1");
                }
                catch(Exception)
                {
                    return Json("0");
                }
            });
        }

        [HttpGet]
        [Route("/Quizz/GetCurrentSession")]
        public async Task<IActionResult> GetCurrentSession()
        {
            return  await  Task.Run(() =>
            {
                string un = Request.Cookies["username"];
                ARQuizz.Session s = ARQuizz.Session.GetCurrentSession(un);
                if (s != null && 
                    s.TimeLeft <= 0)
                {
                   Task.WaitAll(s.Finish());
                }
                JsonResult r = Json(JsonConvert.SerializeObject(s));
                return r;
            });
        }
        [HttpPost]
        [Route("/Quizz/Question/Insert")]
        public async Task<IActionResult> QuestionInsert([FromBody]object question)
        {
            ARQuizz.Question dd = JsonConvert.DeserializeObject<ARQuizz.Question>(question.ToString());
            if(question == null)
            {
                return Json("dsad");
            }
            return await Task.Run(() =>
            {
                try
                {
                   // q.DBInsertasync();
                    return Json("Succes!");
                }catch(Exception ex)
                {
                    //ArDebug.log(ex.Message())
                    return Json("Faild!");
                }
                
            });
        }
        
        [HttpPost]
        [Route("/Quizz/Type/Insert")]
        public async Task<IActionResult> TypeInsert([FromBody]List<ARQuizz.QuestionType> list)
        {
            if(list.Count == 0)
            {
                return Json("Morate uneti bar jedan type kviza");
            }

            return await Task.Run<IActionResult>(() =>
            {
               Task.WaitAll(ARQuizz.QuestionType.DBInsertListAsync(list));
                return Json("ok");
            });

        }

        [HttpPost]
        [Route("/Quizz/Mod/on")]
        public IActionResult RoutMaker()
        {
            this.rute = "/Quizz/Question/Next/{username}/{id}";

        }
        #endregion
    }

}
