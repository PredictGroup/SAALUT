using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.IO;
using System.Data.SqlTypes;
using System.Text;
using MySql.Data.Common;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data;
using System.Transactions;

namespace Saalut
{
    public class SaveLogs
    {
        SaalutDataClasses1DataContext context;

        // загрузка через кулоад по отделам
        public void SaveToLog(string type, string message)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            Log log = new Log();
            log.TimeStamp = DateTime.Now;
            log.Type = type;
            log.Message = message;
            context.Logs.InsertOnSubmit(log);
            context.SubmitChanges();
        }
    }
}