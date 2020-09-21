using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace quizz.Models
{
    public partial class ARQuizz
    {
        /// <summary>
        /// Represent answers on question
        /// </summary>
        public class Answer // User
        {
            public int ID { get; set; }
            public string Text { get; set; }
            public int Score { get; set; }
            public bool IsAnswerd { get; set; } = false; // 8 * 16 byte


            public async static Task<List<Answer>> List(int type)
            {
                
                return await Task.Run(() =>
                {
                    List<Answer> list = new List<Answer>();
                    try
                    {
                        using (MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                        {
                            con.Open();
                            using(MySqlCommand cmd = new MySqlCommand("SELECT ANSWERS FROM QUESTION WHERE TYPE = @Ty", con))
                            {
                                cmd.Parameters.AddWithValue("@Ty", type);
                                using(MySqlDataReader dt = cmd.ExecuteReader())
                                {
                                    while (dt.Read())
                                    {
                                        list.Add(JsonConvert.DeserializeObject<Answer>(dt["ANSWERS"].ToString()));
                                        
                                    }
                                }
                            }

                        }
                        return list;
                    }
                    catch (Exception) { return null; }
                });
            }
        }
    }
}
