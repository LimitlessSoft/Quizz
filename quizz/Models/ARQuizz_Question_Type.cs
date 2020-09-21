using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace quizz.Models
{
    public partial class ARQuizz
    {
        public class QuestionType
        {
            public int ID { get; set; }
            public string Name { get; set; }

            public int Duration { get; set; }

            public int Lenght { get; set; }
            public static List<QuestionType> List()
            {
                return ListAsync().Result;
            }
            public static async Task<List<QuestionType>> ListAsync()
            {
                List<QuestionType> n = new List<QuestionType>();

                await Task.Run(() =>
                {
                    n.Add(new QuestionType() { ID = 0, Name = "Miks" });

                    int nTries = 10;

                    for (int i = nTries; i >= 0; i--)
                    {
                        try
                        {
                            using (MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                            {
                                con.Open();
                                using (MySqlCommand cmd = new MySqlCommand("SELECT ID, TYPE, DURATION, LENGHT FROM QUESTIONs_TYPE", con))
                                {
                                    using (MySqlDataReader dt = cmd.ExecuteReader())
                                    {
                                        while (dt.Read())
                                        {
                                            n.Add(new QuestionType()
                                            {
                                                ID = Convert.ToInt32(dt["ID"]),
                                                Name = dt["TYPE"].ToString(),
                                                Duration = Convert.ToInt32(dt["DURATION"]),
                                                Lenght = Convert.ToInt32(dt["LENGHT"])
                                            });
                                        }
                                    }
                                }
                            }
                            return;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(1000);
                        }
                    }

                    throw new Exception($"Error connecting to database after {nTries} tries!");
                });

                return n;
            }

            public static async Task DBInsertAsync(string Name)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        using (MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                        {
                            con.OpenAsync();
                            using (MySqlCommand cmd = new MySqlCommand("INSERT INTO QUESTIONS_TYPE(TYPE) VALUES(@Type)", con))
                            {
                                cmd.Parameters.AddWithValue("@Type", Name);
                                cmd.ExecuteNonQuery();
                            }

                        }
                    }
                    catch (Exception) { }
                });
            }
            public static async Task DBUpdateAsync(int ID, string Name, int Duration, int Lenght)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        using (MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                        {
                            con.OpenAsync();
                            using (MySqlCommand cmd = new MySqlCommand("UPDATE QUESTIONS_TYPE SET TYPE = @Type, LENGHT = @LEN, DURATION = @DUR WHERE ID = @ID", con))
                            {
                                cmd.Parameters.AddWithValue("@Type", Name);
                                cmd.Parameters.AddWithValue("@ID", ID);
                                cmd.Parameters.AddWithValue("@DUR", Duration);
                                cmd.Parameters.AddWithValue("@LEN", Lenght);
                                cmd.ExecuteNonQuery();
                            }

                        }

                    Task.WaitAll(ARQuizz.Buffer.RefreshAsync());
                    }
                    catch (Exception ex) 
                    {
                        
                    }
                });
            }

            public static async Task DBInsertListAsync(List<QuestionType> list)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        using (MySqlConnection con = new MySqlConnection(Program.CONNECTION_STRING))
                        {
                            con.OpenAsync();
                            using (MySqlCommand cmd = new MySqlCommand("INSERT INTO QUESTION_TYPE(TYPE) VALUES(@m)", con))
                            {
                                cmd.Parameters.Add("@m", MySqlDbType.VarChar);

                                for (int i = 0; i < list.Count; i++)
                                {
                                    cmd.Parameters["@m"].Value = list[i].Name;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }catch(Exception ex) { }

                });
            }
        }
    }
}
