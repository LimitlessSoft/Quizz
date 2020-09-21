using System;
using System.Collections.Generic;
using System.Linq;
using MySql;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.SignalR.Protocol;
using Newtonsoft.Json;
using System.Xml;
using System.Runtime.CompilerServices;
using MySqlX.XDevAPI;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Threading;

namespace quizz.Models
{
    public partial class ARQuizz
    {
        #region Variables
        //ConnectionString for database
        public static List<Session> Sessions = new List<Session>();
        #endregion
    }
}
