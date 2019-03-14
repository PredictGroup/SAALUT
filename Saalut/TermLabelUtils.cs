using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Web.Configuration;
using System.Globalization;
using System.Data;
using System.Transactions;
using System.Data.SqlTypes;
using MySql.Data.Common;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace Saalut
{
    public class TermLabelUtils
    {
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;

        string Division = WebConfigurationManager.AppSettings["Division"];


        SaalutDataClasses1DataContext context;

        public void PrintTermoLabel(int goodID, int labelID, int qty, int printerID, string dateForLabel)
        {
            //string dateForLabel = "03.04.2012 08:00";

            // Get the physical path of the current application.
            string appPath = HttpRuntime.AppDomainAppPath;
            string termLabelFolder = appPath + @"TRMLabels\";
            string termLabelExport = appPath + @"TRMExport\";

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var settings = (from s in context.Settings
                            select s).FirstOrDefault();

            var storeInfo = (from i in context.StoreInfos
                             select i).FirstOrDefault();

            var good = (from g in context.Goods
                        where g.ID == goodID
                        select g).FirstOrDefault();
            if (good == null)
                return;

            var label = (from l in context.ThermoLabels
                         where l.ID == labelID
                         select l).FirstOrDefault();
            if (label == null)
                return;

            Encoding enc = Encoding.GetEncoding(866);

            string filePath = termLabelFolder + label.FileLabelName;
            string toFileName = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "").Replace(" ", "").Replace(":", "").Replace("-", "").Replace("T", "").Replace("+", "") + "_" + label.FileLabelName;
            string toFilePath = termLabelExport + toFileName;
            string batFilePath = termLabelExport + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "").Replace(" ", "").Replace(":", "").Replace("-", "").Replace("T", "").Replace("+", "") + ".bat";


            StringBuilder str = new StringBuilder();

            try
            {
                using (StreamReader sr = new StreamReader(filePath, enc))
                {
                    while (sr.Peek() >= 0)
                    {
                        string toStr = "";

                        toStr = sr.ReadLine();
                        toStr = toStr.Replace("\r\n", "\n");
                        toStr = toStr.Replace("\r", "\n");
                        toStr = toStr.Replace("\t", " ");


                        // + goodName
                        if (toStr.Contains("%goodname"))
                        {
                            string descr = good.Name;
                            StringBuilder strName = new StringBuilder();

                            descr = descr.Replace("\r\n", "\n");
                            descr = descr.Replace("\r", "\n");
                            descr = descr.Replace("\t", " ");

                            int length = descr.Length;
                            int from = 0;

                            while (length >= label.GoodNameVStroke) //количество символов в строке
                            {
                                strName.Append(descr.Substring(from, label.GoodNameVStroke.Value) + "\n");
                                from += label.GoodNameVStroke.Value;
                                length -= label.GoodNameVStroke.Value;
                            }
                            if (length < label.GoodNameVStroke.Value)
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
                        if (toStr.Contains("%contents") && good.Contents != null)
                        {
                            string descr = good.Contents;
                            StringBuilder strName = new StringBuilder();

                            descr = descr.Replace("\r\n", "\n");
                            descr = descr.Replace("\r", "\n");
                            descr = descr.Replace("\t", " ");

                            int length = descr.Length;
                            int from = 0;

                            while (length >= label.ContentsVStroke) //количество символов в строке
                            {
                                strName.Append(descr.Substring(from, label.ContentsVStroke.Value) + "\n");
                                from += label.ContentsVStroke.Value;
                                length -= label.ContentsVStroke.Value;
                            }
                            if (length < label.ContentsVStroke.Value)
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

                        toStr = toStr.Replace("%docdate%", dateForLabel);
                        toStr = toStr.Replace("%article%", good.Articul);
                        toStr = toStr.Replace("%barcode%", good.Barcode);
                        toStr = toStr.Replace("%qty%", qty.ToString());


                        toStr = toStr.Replace("%company%", storeInfo.JurNameTermoPrn.ToString());
                        toStr = toStr.Replace("%address%", storeInfo.JurAddrTermoPrn.ToString());


                        // + цена по новому
                        DataTable prices;

                        MySqlConnection cnx = null;
                        try
                        {
                            cnx = new MySqlConnection(connStr);
                            MySqlDataAdapter adapter = new MySqlDataAdapter();

                            // Prices
                            string cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select item, price, version, deleted from ukmserver.trm_in_pricelist_items where item = '" + good.Articul + "' and pricelist_id = '" + storeInfo.PriceList_ID_UKM.ToString() + "'  and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
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


                        toStr = toStr.Replace("%pricerub%", priceVal);




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
                bool flushed = false;
                using (StreamWriter sr = new StreamWriter(batFilePath, false, enc, 512))
                {
                    var printer = (from p in context.TermoPrinters
                                   where p.ID == printerID
                                   select p).FirstOrDefault();
                    if (printer == null)
                    {
                        //sr.WriteLine(@"net use " + settings.TermoPrinterPort + ": /delete");
                        //sr.WriteLine(@"net use " + settings.TermoPrinterPort + ": " + settings.TermoPrinterNetPath + " /PERSISTENT:YES");
                        sr.WriteLine(@"cd " + termLabelExport);
                        sr.WriteLine(@"copy " + toFileName + " " + settings.TermoPrinterNetPath + " /b");
                        flushed = true;
                    }
                    else
                    {
                        //sr.WriteLine(@"net use " + printer.TermoPrinterPort + ": /delete");
                        //sr.WriteLine(@"net use " + printer.TermoPrinterPort + ": " + printer.TermoPrinterNetPath + " /PERSISTENT:YES");
                        sr.WriteLine(@"cd " + termLabelExport);
                        sr.WriteLine(@"copy " + toFileName + " " + printer.TermoPrinterNetPath + " /b");
                        flushed = true;
                    }


                    sr.Flush();
                    sr.Close();
                }

                //    //System.Diagnostics.Process proc = new System.Diagnostics.Process();
                //    //proc.StartInfo.FileName = batFilePath;
                //    //proc.StartInfo.RedirectStandardError = false;
                //    //proc.StartInfo.RedirectStandardOutput = false;
                //    //proc.StartInfo.UseShellExecute = false;
                //    //proc.Start();
                //    //proc.WaitForExit();

                //    //Process proc = Process.Start("cmd.exe", @"/C " + batFilePath);
                //}
                //// - bat file

                //Thread.Sleep(5000);

                //if (flushed)
                //{
                //    if (File.Exists(toFilePath))
                //        File.Delete(toFilePath);
                //    if (File.Exists(batFilePath))
                //        File.Delete(batFilePath);
                //}

  //              workTermTicket();

            }
            catch (Exception e)
            {
                return;
            }

        }

        protected void workTermTicket()
        {
            string[] Files = Directory.GetFiles("C:\\inetpub\\wwwroot\\TRMExport\\", "*.bat");
            foreach (string file in Files)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "C:\\inetpub\\wwwroot\\TRMExport\\" + fileToCopy;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;


                proc.Start();
                proc.WaitForExit();
                string output1 = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                string output2 = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();


            }

            Thread.Sleep(TimeSpan.FromSeconds(5));

            string[] Files2 = Directory.GetFiles("C:\\inetpub\\wwwroot\\TRMExport\\");
            foreach (string file in Files2)
            {
                File.Delete(file);
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        public void PrintTermoCennic(int goodID, int labelID, int qty, int printerID, string dateForLabel, string proizvoditel)
        {
            //string dateForLabel = "03.04.2012 08:00";

            // Get the physical path of the current application.
            string appPath = HttpRuntime.AppDomainAppPath;
            string termLabelFolder = appPath + @"TRMLabels\";
            string termLabelExport = appPath + @"TRMExportCennic\";

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var settings = (from s in context.Settings
                            select s).FirstOrDefault();

            var storeInfo = (from s in context.StoreInfos
                             select s).FirstOrDefault();

            var good = (from g in context.Goods
                        where g.ID == goodID
                        select g).FirstOrDefault();
            if (good == null)
                return;

            var label = (from l in context.ThermoLabels
                         where l.ID == labelID
                         select l).FirstOrDefault();
            if (label == null)
                return;

            Encoding enc = Encoding.GetEncoding(866);
            Encoding endTo = Encoding.GetEncoding(866);

            string filePath = termLabelFolder + label.FileLabelName;
            string toFileName = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "").Replace(" ", "").Replace(":", "").Replace("-", "").Replace("T", "").Replace("+", "") + "_" + label.FileLabelName;
            string toFilePath = termLabelExport + toFileName;
            string batFilePath = termLabelExport + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "").Replace(" ", "").Replace(":", "").Replace("-", "").Replace("T", "").Replace("+", "") + ".bat";


            StringBuilder str = new StringBuilder();

            try
            {
                using (StreamReader sr = new StreamReader(filePath, enc))
                {
                    while (sr.Peek() >= 0)
                    {
                        string toStr = "";

                        toStr = sr.ReadLine();
                        toStr = toStr.Replace("\r\n", "\n");
                        toStr = toStr.Replace("\r", "\n");
                        toStr = toStr.Replace("\t", " ");


                        // + goodName
                        if (toStr.Contains("%goodname"))
                        {
                            string descr = good.Descr;
                            StringBuilder strName = new StringBuilder();

                            descr = descr.Replace("\r\n", "\n");
                            descr = descr.Replace("\r", "\n");
                            descr = descr.Replace("\t", " ");

                            int length = descr.Length;
                            int from = 0;

                            while (length >= label.GoodNameVStroke) //количество символов в строке
                            {
                                strName.Append(descr.Substring(from, label.GoodNameVStroke.Value) + "\n");
                                from += label.GoodNameVStroke.Value;
                                length -= label.GoodNameVStroke.Value;
                            }
                            if (length < label.GoodNameVStroke.Value)
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
                        if (toStr.Contains("%contents") && good.Contents != null)
                        {
                            string descr = good.Contents;
                            StringBuilder strName = new StringBuilder();

                            descr = descr.Replace("\r\n", "\n");
                            descr = descr.Replace("\r", "\n");
                            descr = descr.Replace("\t", " ");

                            int length = descr.Length;
                            int from = 0;

                            while (length >= label.ContentsVStroke) //количество символов в строке
                            {
                                strName.Append(descr.Substring(from, label.ContentsVStroke.Value) + "\n");
                                from += label.ContentsVStroke.Value;
                                length -= label.ContentsVStroke.Value;
                            }
                            if (length < label.ContentsVStroke.Value)
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

                        toStr = toStr.Replace("%upakovshik%", "");


                        toStr = toStr.Replace("%docdate%", dateForLabel);
                        toStr = toStr.Replace("%article%", good.Articul);
                        toStr = toStr.Replace("%barcode%", good.Barcode);
                        toStr = toStr.Replace("%qty%", qty.ToString());

                        toStr = toStr.Replace("%edinica%", good.Edinic);
                        toStr = toStr.Replace("%producer%", good.Producer);
                        toStr = toStr.Replace("%country%", proizvoditel);

                        // + цена по новому
                        DataTable prices;

                        MySqlConnection cnx = null;
                        try
                        {
                            cnx = new MySqlConnection(connStr);
                            MySqlDataAdapter adapter = new MySqlDataAdapter();

                            // Prices
                            string cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select item, price, version, deleted from ukmserver.trm_in_pricelist_items where item = '" + good.Articul + "' and pricelist_id = '" + storeInfo.PriceList_ID_UKM.ToString() + "'  and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
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


                        toStr = toStr.Replace("%pricerub%", priceVal);

                        DateTime valdays = DateTime.Today.AddDays(good.Exp_Qty.Value);

                        toStr = toStr.Replace("%validate%", valdays.ToString("d")); // Срок годности

                        toStr = toStr.Replace("%packsize%", ""); // упаковка

                        toStr = toStr.Replace("%companylogo%", "1Y12000sp"); // 1Y1200000550380sp

                        toStr = toStr.Replace("%company%", storeInfo.Company);
                        toStr = toStr.Replace("%address%", storeInfo.AddressFact);


                        str.AppendLine(toStr);
                    }
                }

                string[] textParagraphsExport = str.ToString().Split('\n');

                using (StreamWriter sr = new StreamWriter(toFilePath, false, endTo, 512))
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
                using (StreamWriter sr = new StreamWriter(batFilePath, false, endTo, 512))
                {
                    var printer = (from p in context.TermoPrinters
                                   where p.ID == printerID
                                   select p).FirstOrDefault();
                    if (printer == null)
                    {
                        //sr.WriteLine(@"net use " + settings.TermoPrinterPort + ": /delete");
                        //sr.WriteLine(@"net use " + settings.TermoPrinterPort + ": " + settings.TermoPrinterNetPath + " /PERSISTENT:YES");
                        sr.WriteLine(@"cd " + "C:\\inetpub\\wwwroot\\TRMExport\\");
                        sr.WriteLine(@"copy " + toFileName + " " + settings.TermoPrinterNetPath + " /b");
                    }
                    else
                    {
                        //sr.WriteLine(@"net use " + printer.TermoPrinterPort + ": /delete");
                        //sr.WriteLine(@"net use " + printer.TermoPrinterPort + ": " + printer.TermoPrinterNetPath + " /PERSISTENT:YES");
                        sr.WriteLine(@"cd " + "C:\\inetpub\\wwwroot\\TRMExport\\");
                        sr.WriteLine(@"copy " + toFileName + " " + printer.TermoPrinterNetPath + " /b");
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

                workTerm(); // запускаем на печать

            }
            catch (Exception e)
            {
                return;
            }

        }

        public void PrintTermoCennic_old(int goodID, int labelID, int qty, int printerID, string dateForLabel)
        {
            //string dateForLabel = "03.04.2012 08:00";

            // Get the physical path of the current application.
            string appPath = HttpRuntime.AppDomainAppPath;
            string termLabelFolder = appPath + @"TRMLabels\";
            string termLabelExport = appPath + @"TRMExportCennic\";

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var settings = (from s in context.Settings
                            select s).FirstOrDefault();

            var storeInfo = (from s in context.StoreInfos
                             select s).FirstOrDefault();

            var good = (from g in context.Goods
                        where g.ID == goodID
                        select g).FirstOrDefault();
            if (good == null)
                return;

            var label = (from l in context.ThermoLabels
                         where l.ID == labelID
                         select l).FirstOrDefault();
            if (label == null)
                return;

            Encoding enc = Encoding.GetEncoding(866);
            Encoding endTo = Encoding.GetEncoding(866);

            string filePath = termLabelFolder + label.FileLabelName;
            string toFileName = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "").Replace(" ", "").Replace(":", "").Replace("-", "").Replace("T", "").Replace("+", "") + "_" + label.FileLabelName;
            string toFilePath = termLabelExport + toFileName;
            string batFilePath = termLabelExport + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "").Replace(" ", "").Replace(":", "").Replace("-", "").Replace("T", "").Replace("+", "") + ".bat";


            StringBuilder str = new StringBuilder();

            try
            {
                using (StreamReader sr = new StreamReader(filePath, enc))
                {
                    while (sr.Peek() >= 0)
                    {
                        string toStr = "";

                        toStr = sr.ReadLine();
                        toStr = toStr.Replace("\r\n", "\n");
                        toStr = toStr.Replace("\r", "\n");
                        toStr = toStr.Replace("\t", " ");


                        // + goodName
                        if (toStr.Contains("%goodname"))
                        {
                            string descr = good.Descr;
                            StringBuilder strName = new StringBuilder();

                            descr = descr.Replace("\r\n", "\n");
                            descr = descr.Replace("\r", "\n");
                            descr = descr.Replace("\t", " ");

                            int length = descr.Length;
                            int from = 0;

                            while (length >= label.GoodNameVStroke) //количество символов в строке
                            {
                                strName.Append(descr.Substring(from, label.GoodNameVStroke.Value) + "\n");
                                from += label.GoodNameVStroke.Value;
                                length -= label.GoodNameVStroke.Value;
                            }
                            if (length < label.GoodNameVStroke.Value)
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
                        if (toStr.Contains("%contents") && good.Contents != null)
                        {
                            string descr = good.Contents;
                            StringBuilder strName = new StringBuilder();

                            descr = descr.Replace("\r\n", "\n");
                            descr = descr.Replace("\r", "\n");
                            descr = descr.Replace("\t", " ");

                            int length = descr.Length;
                            int from = 0;

                            while (length >= label.ContentsVStroke) //количество символов в строке
                            {
                                strName.Append(descr.Substring(from, label.ContentsVStroke.Value) + "\n");
                                from += label.ContentsVStroke.Value;
                                length -= label.ContentsVStroke.Value;
                            }
                            if (length < label.ContentsVStroke.Value)
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

                        toStr = toStr.Replace("%upakovshik%", "");


                        toStr = toStr.Replace("%docdate%", dateForLabel);
                        toStr = toStr.Replace("%article%", good.Articul);
                        toStr = toStr.Replace("%barcode%", good.Barcode);
                        toStr = toStr.Replace("%qty%", qty.ToString());

                        toStr = toStr.Replace("%edinica%", good.Edinic);
                        toStr = toStr.Replace("%producer%", good.Producer);

                        var price = (from p in context.Prices
                                     where p.GoodID == good.ID
                                     && p.Active == true
                                     select p).FirstOrDefault();

                        toStr = toStr.Replace("%pricerub%", price.Price1.ToString());

                        DateTime valdays = DateTime.Today.AddDays(good.Exp_Qty.Value);

                        toStr = toStr.Replace("%validate%", valdays.ToString("d")); // Срок годности

                        toStr = toStr.Replace("%packsize%", ""); // упаковка

                        toStr = toStr.Replace("%companylogo%", "1Y12000sp"); // 1Y1200000550380sp

                        toStr = toStr.Replace("%company%", storeInfo.Company);
                        toStr = toStr.Replace("%address%", storeInfo.AddressFact);


                        str.AppendLine(toStr);
                    }
                }

                string[] textParagraphsExport = str.ToString().Split('\n');

                using (StreamWriter sr = new StreamWriter(toFilePath, false, endTo, 512))
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
                using (StreamWriter sr = new StreamWriter(batFilePath, false, endTo, 512))
                {
                    var printer = (from p in context.TermoPrinters
                                   where p.ID == printerID
                                   select p).FirstOrDefault();
                    if (printer == null)
                    {
                        //sr.WriteLine(@"net use " + settings.TermoPrinterPort + ": /delete");
                        //sr.WriteLine(@"net use " + settings.TermoPrinterPort + ": " + settings.TermoPrinterNetPath + " /PERSISTENT:YES");
                        sr.WriteLine(@"cd " + termLabelExport);
                        sr.WriteLine(@"copy " + toFileName + " " + settings.TermoPrinterNetPath + " /b");
                    }
                    else
                    {
                        //sr.WriteLine(@"net use " + printer.TermoPrinterPort + ": /delete");
                        //sr.WriteLine(@"net use " + printer.TermoPrinterPort + ": " + printer.TermoPrinterNetPath + " /PERSISTENT:YES");
                        sr.WriteLine(@"cd " + termLabelExport);
                        sr.WriteLine(@"copy " + toFileName + " " + printer.TermoPrinterNetPath + " /b");
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

                workTerm(); // запускаем на печать

            }
            catch (Exception e)
            {
                return;
            }

        }


        protected void workTerm()
        {

            string[] Files = Directory.GetFiles("C:\\inetpub\\wwwroot\\TRMExportCennic\\", "*.bat");
            foreach (string file in Files)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "C:\\inetpub\\wwwroot\\TRMExportCennic\\" + fileToCopy;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;


                proc.Start();
                proc.WaitForExit();
                string output1 = proc.StandardError.ReadToEnd();
                proc.WaitForExit();
                string output2 = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
            }

            Thread.Sleep(TimeSpan.FromSeconds(5));


            string[] Files2 = Directory.GetFiles("C:\\inetpub\\wwwroot\\TRMExportCennic\\", "*.*");
            foreach (string file in Files2)
            {
                File.Delete(file);
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

    }
}