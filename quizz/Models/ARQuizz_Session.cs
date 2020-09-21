using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace quizz.Models
{
    public partial class ARQuizz
    {
        public class Session
        {
            #region Variables
            public int ID { get; set; }
            public string UserName { get; set; } 
            public List<Question> Questions { get; set; }

            public int currentQuestion = -1;
            public int Score { get; set; } = 0;
            /// <summary>
            /// Total duration of session in seconds
            /// </summary>
            public int Duration
            {
                get
                {
                    //Probaj da pronadjes vreme trajanja kviza ako ne onda je 30sek
                   return ARQuizz.Buffer.QuestionTypes.Where(t => t.ID == Questions[0].Type).Select(t => t.Duration).FirstOrDefault() == 0 ? 30 : ARQuizz.Buffer.QuestionTypes.Where(t => t.ID == Questions[0].Type).Select(t => t.Duration).FirstOrDefault();
          //       return Questions == null || Questions.Count == 0 ? 0 : Convert.ToInt32(Questions.Sum(x => x.Duration));
                }
            }
            public DateTime Start { get; set; } = DateTime.Now;
            public DateTime End { get; set; }
            public double TimeLeft
            {
                get
                {
                    return Duration - (DateTime.Now - Start).TotalSeconds;
                }
            }

            public static ManualResetEvent ___manualResetEvent_DBInsert = new ManualResetEvent(false); // Stavljamo state na unsignalled
            #endregion

            public Session() { }
            public Session(string UserName, int type)
            {
                this.UserName = UserName;
                List<Question> qu = Question.List(type);
                //Pronalazi duzinu(koliko pitanja ima) kviz u question_type 
                int leng = ARQuizz.Buffer.QuestionTypes.Where(t => t.ID == type).Select(t => t.Lenght).FirstOrDefault();
                //Pita da li taj kviz ima vise od 0 pitanja ako nema pusti miks kviz
                Questions = qu.Take(leng).Count() == 0 ? qu : qu.Take(leng).ToList();
                Sessions.Add(this);
            }

            public static Session GetCurrentSession(string UserName)
            {
                return Sessions.Where(t => t.UserName == UserName).FirstOrDefault();
            }
            public static List<Session> List()
            {
                List<Session> s = new List<Session>();
                using (MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT ID, USERNAME, QUESTIONS, SCORE, START, END FROM SESSION", con))
                    {
                        using (MySqlDataReader dt = cmd.ExecuteReader())
                        {
                            while (dt.Read())
                            {
                                Session p = new Session();
                                p.ID = Convert.ToInt32(dt["ID"]);
                                p.UserName = dt["USERNAME"].ToString();
                                p.Score = Convert.ToInt32(dt["SCORE"]);
                                p.Start = Convert.ToDateTime(dt["START"]);
                                p.End = Convert.ToDateTime(dt["END"]);
                                p.Questions = JsonConvert.DeserializeObject<List<ARQuizz.Question>>(dt["QUESTIONS"].ToString());
                                s.Add(p);
                            }
                        }
                    }
                }
                return s;
            }

            public static int GetUserScore(string UserName)
            {
                return Sessions.Where(t => t.UserName == UserName).Select(t => t.Score).FirstOrDefault();
            }

            public async Task<List<Answer>> List(int type, int lenght)
            {
                List<Answer> list = new List<Answer>();
                List<Answer> l = await ARQuizz.Answer.List(type);
                for(int i = 0; i < lenght; i++)
                {
                    if (l[i] != null)
                    {
                        list.Add(l[i]);
                    }
                }
                return list;
            }

            public void UnasweredQuestion()
            {
                if(this.TimeLeft <= 0)
                {
                    this.currentQuestion++;
                }
            }
            public bool Check()
            {
                if ((currentQuestion + 1) == Questions.Count)
                    return true;
                return false;
            }
            public Question GetNextQuestion()
            {
                currentQuestion++;
                Questions[currentQuestion].Start = DateTime.Now;
                return Questions[currentQuestion];
            }
            public async Task<int> Answer(int id)
            {
               return await Task.Run(() =>
                {
                    Question q = Questions[currentQuestion];
                    Answer a = q.Answers.Where(t => t.ID == id).FirstOrDefault();
                    a.IsAnswerd = true;

                    if (a == null)
                        return -99995;

                    this.Score += a.Score;


                    if (IsLastQuestion())
                        Task.WaitAll(Finish());


                    return a.Score;
                });
            }
            public async Task Finish()
            {
              await  Task.Run(() =>
                {
                    Sessions.Remove(this);
                    Task.WaitAll(DBInsertAsync());
                });
            }
            public void DBInsert()
            {
                _ = DBInsertAsync();
            }

            public async Task DBInsertAsync()
            {
                if(ID != 0)
                    throw new Exception("This session is already in database under ID: " + ID);

                await Task.Run((async () =>
                {
                    ___manualResetEvent_DBInsert.Reset(); // setujemo state na signaled / true
                    Thread.Sleep(2000);
                    try
                    {
                        using (MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                        {
                            await con.OpenAsync();

                            using (MySqlCommand cmd = new MySqlCommand(@"INSERT INTO SESSION 
                            (USERNAME, QUESTIONS, SCORE, START, END)
                            VALUES
                            (@UN, @Q, @SC, @ST, @EN)", con))
                            {
                                cmd.Parameters.AddWithValue("@UN", UserName);
                                cmd.Parameters.AddWithValue("@Q", JsonConvert.SerializeObject(Questions));
                                cmd.Parameters.AddWithValue("@SC", Score);
                                cmd.Parameters.AddWithValue("@ST", Start);
                                cmd.Parameters.AddWithValue("@EN", End);

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                    ___manualResetEvent_DBInsert.Set(); // setujemo state na unsignaled
                }));
            }
            public bool IsLastQuestion()
            {
                if (currentQuestion + 1 >= Questions.Count)
                {
                    this.End = DateTime.Now;
                    return true;
                }
                return false;
            }
            public bool IsQuestionTimeOut()
            {
                if (this.Questions[currentQuestion].TimeLeft <= 0)
                {
                    return true;
                }
                else return false;
            }
            
        }
    }
  
}
