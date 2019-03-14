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
using System.Data.SqlTypes;
using System.Text;
using MySql.Data.Common;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data;
using System.Transactions;
using System.Drawing;

namespace Saalut.Services
{
    /// <summary>
    /// Сводное описание для PrintTermoCennicNow
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Чтобы разрешить вызывать веб-службу из скрипта с помощью ASP.NET AJAX, раскомментируйте следующую строку. 
    // [System.Web.Script.Services.ScriptService]
    public class PrintTermoCennicNow : System.Web.Services.WebService
    {
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;
        string Division = WebConfigurationManager.AppSettings["Division"];

        SaalutDataClasses1DataContext context;

        [WebMethod(Description = "Печать термо ценников"), SoapDocumentMethod(OneWay = true)]
        public void PrintAllQuoue()
        {
            // Get the physical path of the current application.
            string appPath = HttpRuntime.AppDomainAppPath;
            string termLabelFolder = appPath + @"TRMLabels\";
            string termLabelExport = appPath + @"TRMExportCennic\";


            if (context == null)
                context = new SaalutDataClasses1DataContext();


            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();


            var setting = (from s in context.Settings
                           select s).FirstOrDefault();

            var cfps = (from c in context.TermoCennicQuoue
                        where c.Active == true
                        && c.TimeStamp >= DateTime.Now.AddHours(-3)
                        select c);

            foreach (TermoCennicQuoue q in cfps)
            {
                var priceChangeJour = (from p in context.PriceChangeJours
                                       where p.ID == q.JournalID
                                       select p).FirstOrDefault();


                //Encoding enc = Encoding.GetEncoding(866);
                Encoding enc = Encoding.GetEncoding(1251);
                //Encoding enc = Encoding.GetEncoding(850);

                string filePath = termLabelFolder + q.TermoCennic.FileLabelName;
                string toFileName = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "").Replace(" ", "").Replace(":", "").Replace("-", "").Replace("T", "").Replace("+", "") + "_" + q.TermoCennic.FileLabelName;
                string toFilePath = termLabelExport + toFileName;
                string batFilePath = termLabelExport + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "").Replace(" ", "").Replace(":", "").Replace("-", "").Replace("T", "").Replace("+", "") + ".bat";



                // + цена по новому
                DataTable prices;

                MySqlConnection cnx = null;
                try
                {
                    cnx = new MySqlConnection(connStr);
                    MySqlDataAdapter adapter = new MySqlDataAdapter();

                    // Prices
                    string cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select item, price, version, deleted from ukmserver.trm_in_pricelist_items where item = '" + q.Good.Articul + "' and pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                    MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds5 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds5);

                    prices = ds5.Tables[0];

                }
                catch (MySqlException ex)
                {
                    return;
                }
                finally
                {
                    if (cnx != null)
                    {
                        cnx.Close();
                    }
                }

                // - цена по новому


                //var price = (from p in context.Prices
                //             where p.GoodID == good.ID
                //             && p.Active == true
                //             select p).FirstOrDefault();

                string priceVal = "нет цены";
                //var price = (from p in dataContext.Prices
                //             where p.GoodID == good.ID
                //             && p.Active == true
                //             select p).FirstOrDefault();
                //if (price != null)
                //{
                //    double priceFromDB = price.Price1.Value;

                //    if (template.EdinicZa100Gr != null)
                //    {
                //        if (template.EdinicZa100Gr.Value)
                //        {
                //            priceFromDB = priceFromDB / 10;
                //        }
                //    }
                //    priceVal = priceFromDB.ToString();
                //}

                // + новая цена
                decimal price = 0;
                if (prices != null)
                {
                    foreach (DataRow row in prices.Rows)
                    {
                        price = (decimal)row[1];
                    }
                }
                if (price != 0)
                {
                    priceVal = price.ToString();
                    if (Division == "RB")
                    {
                        int priceValZap = priceVal.IndexOf(",");
                        if (priceValZap != -1)
                        {
                            priceVal = priceVal.Substring(0, priceValZap);
                        }
                    }
                    else
                        if (Division == "RF")
                        {
                            int priceValZap = priceVal.IndexOf(",");
                            if (priceValZap != -1)
                            {
                                if ((priceValZap + 3) < priceVal.Length)
                                    priceVal = priceVal.Substring(0, priceValZap + 3);
                            }
                        }
                }
                // - новая цена


                StringBuilder str = new StringBuilder();

                try
                {
                    using (StreamReader sr = new StreamReader(filePath, enc))
                    {
                        while (sr.Peek() >= 0)
                        {
                            string toStr = "";

                            toStr = sr.ReadLine();

                            // + goodName
                            if (toStr.Contains("%goodname"))
                            {
                                string descr = q.Good.Name;
                                StringBuilder strName = new StringBuilder();

                                descr = descr.Replace("\r\n", "\n");
                                descr = descr.Replace("\r", "\n");
                                descr = descr.Replace("\t", " ");

                                int length = descr.Length;
                                int from = 0;

                                while (length >= q.TermoCennic.GoodNameVStroke) //количество символов в строке
                                {
                                    strName.Append(descr.Substring(from, q.TermoCennic.GoodNameVStroke.Value) + "\n");
                                    from += q.TermoCennic.GoodNameVStroke.Value;
                                    length -= q.TermoCennic.GoodNameVStroke.Value;
                                }
                                if (length < q.TermoCennic.GoodNameVStroke.Value)
                                    strName.Append(descr.Substring(from, length));

                                string[] textParagraphs = strName.ToString().Split('\n');
                                int i = 1;
                                foreach (string strText in textParagraphs.ToArray())
                                {
                                    using (StringFormat sf = new StringFormat())
                                    {
                                        //sf.Alignment = StringAlignment.Near;
                                        //sf.LineAlignment = StringAlignment.Near;
                                        //sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
                                        sf.FormatFlags = StringFormatFlags.NoWrap;

                                        toStr = toStr.Replace("%goodname" + i.ToString() + "%", strText.Trim());
                                    }
                                    i++;
                                }

                                if (toStr.Contains("%goodname"))
                                {
                                    toStr = toStr.Substring(0, toStr.IndexOf("%"));
                                }
                            }
                            // - goodName

                            // + goodContents
                            if (toStr.Contains("%contents") && q.Good.Contents != null)
                            {
                                string descr = q.Good.Contents;
                                StringBuilder strName = new StringBuilder();

                                descr = descr.Replace("\r\n", "\n");
                                descr = descr.Replace("\r", "\n");
                                descr = descr.Replace("\t", " ");

                                int length = descr.Length;
                                int from = 0;

                                while (length >= q.TermoCennic.ContentsVStroke) //количество символов в строке
                                {
                                    strName.Append(descr.Substring(from, q.TermoCennic.ContentsVStroke.Value) + "\n");
                                    from += q.TermoCennic.ContentsVStroke.Value;
                                    length -= q.TermoCennic.ContentsVStroke.Value;
                                }
                                if (length < q.TermoCennic.ContentsVStroke.Value)
                                    strName.Append(descr.Substring(from, length));

                                string[] textParagraphs = strName.ToString().Split('\n');
                                int i = 1;
                                foreach (string strText in textParagraphs.ToArray())
                                {
                                    using (StringFormat sf = new StringFormat())
                                    {
                                        //sf.Alignment = StringAlignment.Near;
                                        //sf.LineAlignment = StringAlignment.Near;
                                        //sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
                                        sf.FormatFlags = StringFormatFlags.NoWrap;

                                        toStr = toStr.Replace("%contents" + i.ToString() + "%", strText.Trim());
                                    }
                                    i++;
                                }

                                if (toStr.Contains("%contents"))
                                {
                                    toStr = toStr.Substring(0, toStr.IndexOf("%"));
                                }
                            }
                            // - goodContents

                            toStr = toStr.Replace("%docdate%", DateTime.Now.ToString("dd.MM.yyyy"));
                            toStr = toStr.Replace("%article%", q.Good.Articul);
                            toStr = toStr.Replace("%barcode%", q.Good.Barcode);
                            toStr = toStr.Replace("%edinica%", q.Good.Edinic);
                            toStr = toStr.Replace("%producer%", q.Good.Producer + " " + q.Good.Country);

                            toStr = toStr.Replace("%company%", store.Company.ToString());
                            toStr = toStr.Replace("%address%", store.AddressFact.ToString());


                            string priceRub = "";
                            string priceKop = "";
                            if (Division == "RF")
                            {
                                int priceValZap = priceVal.IndexOf(",");
                                if (priceValZap != -1)
                                {
                                    priceRub = priceVal.Substring(0, priceValZap);
                                    if ((priceValZap + 3) <= priceVal.Length)
                                        priceKop = priceVal.Substring(priceValZap + 1, 2);
                                }
                                else
                                {
                                    priceValZap = priceVal.IndexOf(".");
                                    if (priceValZap != -1)
                                    {
                                        priceRub = priceVal.Substring(0, priceValZap);
                                        if ((priceValZap + 3) <= priceVal.Length)
                                            priceKop = priceVal.Substring(priceValZap + 1, 2);
                                    }
                                }
                            }


                            toStr = toStr.Replace("%pricerub%", priceRub);
                            toStr = toStr.Replace("%pricekop%", priceKop);



                            str.AppendLine(toStr);
                        }
                    }

                    string[] textParagraphsExport = str.ToString().Split('\n');

                    using (StreamWriter sr = new StreamWriter(toFilePath, false, enc, 512))
                    {
                        foreach (string strText in textParagraphsExport.ToArray())
                        {
                            using (StringFormat sf = new StringFormat())
                            {
                                //sf.Alignment = StringAlignment.Near;
                                //sf.LineAlignment = StringAlignment.Near;
                                //sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
                                sf.FormatFlags = StringFormatFlags.NoWrap;

                                sr.WriteLine(strText.Trim());
                            }
                        }

                        sr.Flush();
                        sr.Close();
                    }

                    // + bat file
                    using (StreamWriter sr = new StreamWriter(batFilePath, false, enc, 512))
                    {
                        if (priceChangeJour != null)
                        {
                            if (priceChangeJour.Order_no > 0)
                            {
                                var mobPr = (from m in context.MobileTermoPrinters
                                             where m.Num == priceChangeJour.Order_no
                                             select m).FirstOrDefault();
                                if (mobPr != null)
                                {
                                    sr.WriteLine(@"cd " + termLabelExport);
                                    sr.WriteLine(@"copy " + toFileName + " " + mobPr.NetPath + " /b");
                                }
                                else
                                {
                                    sr.WriteLine(@"cd " + termLabelExport);
                                    sr.WriteLine(@"copy " + toFileName + " " + setting.MobileTermoPrinterNetPath + " /b");
                                }
                            }
                            else
                            {
                                sr.WriteLine(@"cd " + termLabelExport);
                                sr.WriteLine(@"copy " + toFileName + " " + setting.MobileTermoPrinterNetPath + " /b");
                            }
                        }
                        else
                        {
                            sr.WriteLine(@"cd " + termLabelExport);
                            sr.WriteLine(@"copy " + toFileName + " " + setting.MobileTermoPrinterNetPath + " /b");
                        }


                        sr.Flush();
                        sr.Close();

                        //System.Diagnostics.Process proc = new System.Diagnostics.Process();
                        //proc.StartInfo.FileName = batFilePath;
                        //proc.StartInfo.RedirectStandardError = false;
                        //proc.StartInfo.RedirectStandardOutput = false;
                        //proc.StartInfo.UseShellExecute = false;
                        //proc.Start();
                        //proc.WaitForExit();

                        //Process proc = Process.Start("cmd.exe", @"/C " + batFilePath);
                    }
                    // - bat file


                    //if (File.Exists(toFilePath))
                    //    File.Delete(toFilePath);
                    //if (File.Exists(batFilePath))
                    //    File.Delete(batFilePath);
                }
                catch (Exception e)
                {
                    return;
                }


                q.Active = false;
                context.SubmitChanges();
            }

            ExecPrinting();
        }

        private void ExecPrinting()
        {
            string[] Files = Directory.GetFiles(Server.MapPath("..\\TRMExportCennic"), "*.bat");
            foreach (string file in Files)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = Server.MapPath("..\\TRMExportCennic") + "\\" + fileToCopy;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;


                proc.Start();
                proc.WaitForExit();
                string output1 = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                string output2 = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                //System.Diagnostics.Process proc = new System.Diagnostics.Process();
                //proc.StartInfo.FileName = "C:\\inetpub\\wwwroot\\TRMExport\\" + fileToCopy;
                //proc.StartInfo.RedirectStandardError = false;
                //proc.StartInfo.RedirectStandardOutput = false;
                //proc.StartInfo.UseShellExecute = false;
                //proc.Start();
                //proc.WaitForExit();
            }

            string[] Files2 = Directory.GetFiles(Server.MapPath("..\\TRMExportCennic"));
            foreach (string file in Files2)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);
                File.Delete(file);
            }

        }

        [WebMethod(Description = "Печать термо ценников (очистка)"), SoapDocumentMethod(OneWay = true)]
        public void deleteAllQuoue()
        {
            //+ termo cennic

            string[] Files3 = Directory.GetFiles(Server.MapPath("..\\TRMExportCennic"), "*.bat");
            foreach (string file in Files3)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = Server.MapPath("..\\TRMExportCennic") + "\\" + fileToCopy;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;


                proc.Start();
                proc.WaitForExit();
                string output1 = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                string output2 = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                //System.Diagnostics.Process proc = new System.Diagnostics.Process();
                //proc.StartInfo.FileName = "C:\\inetpub\\wwwroot\\TRMExport\\" + fileToCopy;
                //proc.StartInfo.RedirectStandardError = false;
                //proc.StartInfo.RedirectStandardOutput = false;
                //proc.StartInfo.UseShellExecute = false;
                //proc.Start();
                //proc.WaitForExit();
            }

            string[] Files4 = Directory.GetFiles(Server.MapPath("..\\TRMExportCennic"));
            foreach (string file in Files4)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);
                File.Delete(file);
            }
        }

    }
}
