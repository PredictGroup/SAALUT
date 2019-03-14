using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Security;
using System.Web.Services.Protocols;
using System.Globalization;
using System.Net;
using System.IO;
using System.Web.Configuration;
using System.Threading;

namespace Saalut.Services
{
    /// <summary>
    /// Summary description for WeighingEquipLoader
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WeighingEquipLoader : System.Web.Services.WebService
    {

        [WebMethod(Description = "Выгрузить все на весы."), SoapDocumentMethod(OneWay = true)]
        public void ExportAllToWE()
        {
            WeighingEquipUtils utl = new WeighingEquipUtils();
            utl.ExportAllToWE();
        }

        [WebMethod(Description = "Печать термо этикетов"), SoapDocumentMethod(OneWay = true)]
        public void HourOfExportToWeightNighth()
        {
            // перенесли данный метод в метод печати вернули



            string[] Files = Directory.GetFiles(Server.MapPath("..\\TRMExport"), "*.bat");
            foreach (string file in Files)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = Server.MapPath("..\\TRMExport") + "\\" + fileToCopy;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;


                proc.Start();
                proc.WaitForExit();
                string output1 = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                string output2 = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();


                //------------------------

                //System.Diagnostics.Process proc = new System.Diagnostics.Process();
                //proc.StartInfo.FileName = "C:\\inetpub\\wwwroot\\TRMExport\\" + fileToCopy;
                //proc.StartInfo.RedirectStandardError = false;
                //proc.StartInfo.RedirectStandardOutput = false;
                //proc.StartInfo.UseShellExecute = false;
                //proc.Start();
                //proc.WaitForExit();


                //System.Diagnostics.Process proc = System.Diagnostics.Process.Start("cmd.exe", @"/C " + Server.MapPath("..\\TRMExport") + "\\" + fileToCopy);
            }

            Thread.Sleep(5000);

            string[] Files2 = Directory.GetFiles(Server.MapPath("..\\TRMExport"));
            foreach (string file in Files2)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);
                File.Delete(file);
            }

        }

        [WebMethod(Description = "Выгрузка на весы по новому через Qload."), SoapDocumentMethod(OneWay = true)]
        public void ExportToWeightByDocumentUKM(string doc_ukm)
        {
            string Division = WebConfigurationManager.AppSettings["Division"];
            string dirPath = WebConfigurationManager.AppSettings["UKM_Import_Path"];

            // чистим папку весов.
            string[] Files2 = Directory.GetFiles(dirPath);
            foreach (string file in Files2)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);
                File.Delete(file);
            }

            if (Division == "RB")
            {
                WeighingEquipUtils utl = new WeighingEquipUtils();
                utl.ExportAllToQload();
            }else
                if (Division == "RF")
                {
                    WeighingEquipUtils utl = new WeighingEquipUtils();
                    utl.ExportAllToWE();
                }
        }

    }
}
