using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ubiety.Dns.Core;

namespace quizz.Models
{
    public partial class ARQuizz
    {
        /// <summary>
        /// Question class represent questions table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public partial class Question
        {

            #region Variables
            public int ID { get; set; }
            public int Type { get; set; } // QuestionType
            public string Title { get; set; }
            public int Duration { get; set; } = 5;
            public DateTime Start { get; set; }
            public double TimeLeft
            {
                get
                {
                    return Duration - (DateTime.Now - Start).TotalSeconds;
                }
            }
            public List<Answer> Answers { get; set; }

            public Question()
            {

            }

            public Question(int ID)
            {
                try
                {
                    using (MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                    {
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand("SELECT ID, TYPE, TITLE, ANSWERS FROM QUESTION WHERE ID = @ID", con))
                        {
                            using (MySqlDataReader dt = cmd.ExecuteReader())
                            {
                                if(dt.Read())
                                {
                                    ID = Convert.ToInt32(dt["ID"]);
                                    Type = Convert.ToInt32(dt["TYPE"]);
                                    Title = dt["TITLE"].ToString();
                                    Answers = JsonConvert.DeserializeObject<List<Answer>>(dt["ANSWERS"].ToString());
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    // ex
                }
            }

            #endregion
            /// <summary>
            /// type = 0 Vraca listu svih pitanja
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public static List<Question> List(int type)
            {
                List<Question> q = new List<Question>();
                string command = "";
                if (type == 0)
                {
                    command = "SELECT ID, TYPE, TITLE, ANSWERS FROM QUESTION";
                }
                else
                {
                    command = "SELECT ID, TYPE, TITLE, ANSWERS FROM QUESTION WHERE TYPE=" + type;
                }
                using (MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                {
                    try
                    {
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand(command, con))
                        {
                            using (MySqlDataReader dt = cmd.ExecuteReader())
                            {
                                while (dt.Read())
                                {
                                    q.Add(new Question()
                                    {
                                        ID = Convert.ToInt32(dt["ID"]),
                                        Type = Convert.ToInt32(dt["TYPE"]),
                                        Title = dt["TITLE"].ToString(),
                                        Answers = JsonConvert.DeserializeObject<List<Answer>>(dt["ANSWERS"].ToString())
                                    });
                                }
                            }
                        }
                    }
                    catch(Exception Ex) { }
                }
                return q;
            }

           
            public static async Task<List<Question>> ListaAsync()
            {
                List<Question> list = new List<Question>();
                await Task.Run(() =>
                {
                    using (MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                    {
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand("SELECT ID, TYPE, TITLE, ANSWERS FROM QUESTION", con))
                        {
                            using (MySqlDataReader dt = cmd.ExecuteReader())
                            {
                                while (dt.Read())
                                {
                                    list.Add(new Question()
                                    {
                                        ID = Convert.ToInt32(dt["ID"]),
                                        Type = Convert.ToInt32(dt["TYPE"]),
                                        Title = dt["TITLE"].ToString(),
                                        Answers = JsonConvert.DeserializeObject<List<Answer>>(dt["ANSWERS"].ToString())
                                    });
                                }
                            }
                        }
                    }

                });
                return list;
            }
            public static void QuestionTimeLeft(Session s)
            {

                if (s.Questions[s.currentQuestion].Duration <= 0)
                {
                    s.Questions[s.currentQuestion].Duration = 10;
                    s.currentQuestion++;
                }
            }

            public async void DBInsertasync()
            {
                await Task.Run(() =>
                {
                    try
                    {
                        using (MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                        {
                            con.OpenAsync();
                            using (MySqlCommand cmd = new MySqlCommand("INSERT INTO QUESTION(TYPE, TITLE, ANSWERS) VALUES(@Type,@Title, @Answers", con))
                            {
                                cmd.Parameters.AddWithValue("@Type", this.Type);
                                cmd.Parameters.AddWithValue("@Title", this.Title);
                                cmd.Parameters.AddWithValue("@Asnwers", JsonConvert.SerializeObject(this.Answers));
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex) { }
                });
            }
            public void DBUpdate()
            {
                try
                {
                    using(MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                    {
                        con.Open();
                        using(MySqlCommand cmd = new MySqlCommand("UPDATE QUESTION SET TYPE = @type, TITLE=@title, ANSWERS=@AN WHERE ID = @id", con))
                        {
                            cmd.Parameters.AddWithValue("@type", this.Type);
                            cmd.Parameters.AddWithValue("@title", this.Title);
                            cmd.Parameters.AddWithValue("@AN", JsonConvert.SerializeObject(this.Answers));
                            cmd.Parameters.AddWithValue("@id", this.ID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch(Exception ex)
                {

                }
            }

        }
    }
}
