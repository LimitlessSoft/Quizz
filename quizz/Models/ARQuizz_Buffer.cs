using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace quizz.Models
{
    public partial class ARQuizz
    {
        public class Buffer
        {
            private static bool _Initialized = false;
            private static int RefreshInterval = (int)TimeSpan.FromMinutes(30).TotalSeconds;

            public static List<ARQuizz.QuestionType> QuestionTypes = new List<ARQuizz.QuestionType>();
            public static List<ARQuizz.Question> Questions = new List<Question>();


            public static void Initialize()
            {
                if (_Initialized)
                    return;

                Task.Run(() =>
                {
                    while (true)
                    {
                        RefreshAsync();

                        Thread.Sleep(RefreshInterval);
                    }
                });
            }

            public static void Refresh()
            {
                QuestionTypes = ARQuizz.QuestionType.List();
            }
            public async static Task RefreshAsync()
            {
                QuestionTypes = await ARQuizz.QuestionType.ListAsync();
                Questions = await Question.ListaAsync();
            }
        }
    }
}
