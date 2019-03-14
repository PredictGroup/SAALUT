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
    public class WeighingEquipUtils
    {
        // Connection string for a typical local MySQL installation
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;


        private static readonly Encoding enc1251 = Encoding.GetEncoding(1251);

        private int contenstLength = 30;


        SaalutDataClasses1DataContext context;

        // загрузка через кулоад по отделам
        public string ExportToWE_qload_by_weights_old(string weightNum)
        {

            string dirPath = WebConfigurationManager.AppSettings["UKM_Import_Path"];
            string exportDir = HttpRuntime.AppDomainAppPath + "\\Export\\";

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return "Нет информации о магазине (Saalut). ";


            var weigdep = from d in context.WeightDeparts
                          where d.Num == weightNum
                          && d.Active == true
                          select d.DepartmentID;
            if (weigdep.Count() == 0)
                return "Не существует записи на весы: " + weightNum;

            string fileName = "exp" + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "_").Replace(" ", "_").Replace(":", "_").Replace("-", "_").Replace(@"\", "_").Replace(@"/", "_").Replace("+", "_") + "_" + weightNum + ".csv";

            StringBuilder sb = new StringBuilder();

            var prices = from p in context.Prices
                         where p.Active == true
                         select p.GoodID;

            int ii = 0;

            var goods = from g in context.Goods
                        where g.Active == true
                        && g.PLU != null
                        && g.PLU != 0
                        && weigdep.Contains(g.DepartmentID)
                        && prices.Contains(g.ID)
                        orderby g.DepartmentID ascending
                        select g;
            foreach (Good good in goods)
            {

                string str = "";

                var price = (from p in context.Prices
                             where p.GoodID == good.ID
                             && p.Active == true
                             select p).FirstOrDefault();

                str += ("A;");
                str += (good.PLU + ";");
                str += (good.Barcode + ";");
                str += (price.Price1.ToString().Replace(".", ",") + ";");
                str += ("0;");
                str += ("0;"); // ценовая база, исправлять
                str += (good.Exp_Qty.ToString() + ";"); // срок годности дни

                string goodName = good.Name;
                if (goodName.Length > 80)
                {
                    goodName = goodName.Substring(0, 79);
                }
                str += (goodName);

                sb.AppendLine(str); // записали строку.

                str = "";
                string contentsStr = good.Contents;
                if (contentsStr != "")
                {
                    str += ("I;");
                    str += (good.PLU + ";");

                    if (good.Contents.Length > 400)
                        contentsStr = contentsStr.Substring(0, 400);

                    string contents = "";
                    if (good.Contents != null && good.Contents != "")
                        contents = "Состав:" + contentsStr.Replace(";", ",");
                    if (good.Producer != null && good.Producer != "")
                        contents += " Пр-ль:" + good.Producer;

                    int length = contents.Length;
                    int from = 0;
                    while (length >= contenstLength)
                    {
                        str += (contents.Substring(from, contenstLength) + ";");
                        from += contenstLength;
                        length -= contenstLength;
                    }
                    if (length != 0)
                        str += (contents.Substring(from) + ";");

                    sb.AppendLine(str);
                }
                ii++;
            }

            StreamWriter sw = new StreamWriter(exportDir + fileName, false, Encoding.GetEncoding(1251), 2048);
            sw.Write(sb);
            sw.Close();

            int i = 0;

            string[] Files = Directory.GetFiles(exportDir);
            foreach (string file in Files)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);
                File.Copy(file, dirPath + fileToCopy, true);
                File.Delete(file);
                i++;
            }

            SaveLogs log = new SaveLogs();
            log.SaveToLog("Весы", "Сформированы файлы для выгрузки на весы: " + i.ToString() + " файлов, " + ii.ToString() + " товаров.");

            return "ok";
        }

        public string ExportToWE_qload_by_weights_UKMPrice(string weightNum)
        {
            string retText = "";

            string Division = WebConfigurationManager.AppSettings["Division"];

            string dirPath = WebConfigurationManager.AppSettings["UKM_Import_Path"];
            string exportDir = HttpRuntime.AppDomainAppPath + "\\Export\\";

            bool firstCycle = true;

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return "Нет информации о магазине (Saalut). ";

            var weigdep = from d in context.WeightDeparts
                          where d.Num == weightNum
                          && d.Active == true
                          select d.DepartmentID;
            if (weigdep.Count() == 0)
                return "Не существует записи на весы: " + weightNum;

            //+14052014 весы самообслуживания
            var weightNameE = (from w in context.WeightNames
                               where w.WeightNum == weightNum
                               select w).FirstOrDefault();
            //-14052014 весы самообслуживания

            string fileName = "exp" + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "_").Replace(" ", "_").Replace(":", "_").Replace("-", "_").Replace(@"\", "_").Replace(@"/", "_").Replace("+", "_") + "_" + weightNum + ".csv";

            StringBuilder sb = new StringBuilder();

            // цены укм
            // Create a connection object and data adapter
            DataTable prices;

            int ii = 0;

            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);
                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // Prices
                string cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds5 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds5);

                prices = ds5.Tables[0];

                //-

                var goods = from g in context.Goods
                            where g.Active == true
                            && g.PLU != null
                            && g.PLU != 0
                            && weigdep.Contains(g.DepartmentID)
                            orderby g.DepartmentID ascending
                            select g;
                foreach (Good good in goods)
                {

                    decimal price = 0;
                    if (prices != null)
                    {
                        // select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and deleted = 0
                        DataRow[] prsLsts = prices.Select("item = '" + good.Articul + "'");

                        foreach (DataRow row in prsLsts)
                        {
                            price = (decimal)row[1];
                        }
                    }

                    if (price == 0)
                        continue; // нет цены - пропускаем

                    string str = "";

                    str += ("A;");
                    str += (good.PLU + ";");
                    str += (good.Barcode + ";");
                    if (Division == "RB")
                    {
                        double priceD = 0;
                        Double.TryParse(price.ToString().Substring(0, price.ToString().Length - 5), out priceD);
                        str += ((priceD / 100).ToString() + ";");
                    }
                    else
                        if (Division == "RF")
                        {
                            string price_rf = price.ToString().Substring(0, price.ToString().Length - 2);
                            // bug fix 18-12-12
                            price_rf = price_rf.Replace(",", ".");

                            str += (price_rf + ";");
                        }
                    str += ("0;");
                    str += ("0;"); // ценовая база, исправлять

                    if (Division == "RF")
                    {
                        str += (good.Exp_Qty.ToString() + ";"); // срок годности дни
                    }
                    else
                        if (Division == "RB")
                        {
                            string Srok_Godnosti_Chasov = WebConfigurationManager.AppSettings["Srok_Godnosti_Chasov"];
                            str += (Srok_Godnosti_Chasov + ";"); // срок годности дни
                        }
                        else
                        {
                            str += ("0;"); // срок годности дни
                        }

                    string goodName = good.Name;
                    if (goodName.Length > 80)
                    {
                        goodName = goodName.Substring(0, 79);
                    }
                    str += (goodName);

                    sb.AppendLine(str); // записали строку.

                    str = "";
                    string contentsStr = good.Contents;
                    if (contentsStr != "")
                    {
                        str += ("I;");
                        str += (good.PLU + ";");

                        if (good.Contents.Length > 400)
                            contentsStr = contentsStr.Substring(0, 400);

                        string contents = "";
                        if (good.Contents != null && good.Contents != "")
                            contents = "Состав:" + contentsStr.Replace(";", ",");
                        if (good.Producer != null && good.Producer != "")
                            contents += " Пр-ль:" + good.Producer;

                        int length = contents.Length;
                        int from = 0;
                        while (length >= contenstLength)
                        {
                            str += (contents.Substring(from, contenstLength) + ";");
                            from += contenstLength;
                            length -= contenstLength;
                        }
                        if (length != 0)
                            str += (contents.Substring(from) + ";");

                        sb.AppendLine(str);
                    }

                    //+14052014 весы самообслуживания
                    if (weightNameE != null)
                    {
                        retText += " Весы есть: " + weightNameE.Name;
                        if (weightNameE.SamoObsluzivanie == true)
                        {
                            retText += " Весы самообсл: " + weightNameE.SamoObsluzivanie.Value.ToString();
                            if (firstCycle)
                            {
                                // группы
                                var groups = from g in context.WeightGroups
                                             where g.Active == true
                                             select g;
                                foreach (WeightGroups group in groups)
                                {

                                    string str1 = "";

                                    str1 += ("K;");
                                    str1 += ("0;");
                                    str1 += (group.GroupNum + ";");
                                    str1 += (group.ButtonNum + ";");
                                    str1 += (group.ButtonName + ";");
                                    str1 += (group.ButtonPic);

                                    sb.AppendLine(str1);
                                }
                                firstCycle = false;
                            }

                            // элементы

                            var elem = (from e in context.WeightGoodsPlu
                                        where e.PLU == good.PLU
                                        && e.Active == true
                                        select e).FirstOrDefault();
                            if (elem != null)
                            {
                                string str2 = "";

                                str2 += ("K;");
                                str2 += (elem.PLU + ";");
                                str2 += (elem.GroupNum + ";");
                                str2 += (elem.ButtonNum + ";");
                                str2 += (elem.ButtonName + ";");
                                str2 += (elem.ButtonPic);

                                sb.AppendLine(str2);
                            }

                        }
                    }
                    //-14052014 весы самообслуживания


                    ii++;
                }


            }
            catch (MySqlException ex)
            {
                return "Ошибка загрузки весов " + weightNum + ": " + ex.ToString();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            // добавляем в новом релизе удаление. 2.5.4.
            string[] FilesForDel = Directory.GetFiles(dirPath);
            foreach (string file in FilesForDel)
            {
                File.Delete(file);
            }



            StreamWriter sw = new StreamWriter(exportDir + fileName, false, Encoding.GetEncoding(1251), 2048);
            sw.Write(sb);
            sw.Close();

            int i = 0;

            string[] Files = Directory.GetFiles(exportDir);
            foreach (string file in Files)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);
                File.Copy(file, dirPath + fileToCopy, true);
                File.Delete(file);
                i++;
            }

            SaveLogs log = new SaveLogs();

            string weightName = "не указано";
            if (weightNameE != null)
                weightName = weightNameE.Name;


            string retStr = "Сформированы файлы для выгрузки на весы " + weightNum + "(" + weightName + "): " + i.ToString() + " файлов, " + ii.ToString() + " товаров. " + retText;
            log.SaveToLog("Весы", retStr);


            return retStr;
        }

        public string ExportToWE_qload_by_weights_UKMPrice_by_format(string weightNum)
        {
            string FormatWigruzkiVesoff = WebConfigurationManager.AppSettings["Format_Wigruzki_Vesov"];

            string Division = WebConfigurationManager.AppSettings["Division"];

            string dirPath = WebConfigurationManager.AppSettings["UKM_Import_Path"];
            string exportDir = HttpRuntime.AppDomainAppPath + "\\Export\\";

            bool firstCycle = true;

            string contLength = WebConfigurationManager.AppSettings["Content_Length_Weight"];
            Int32.TryParse(contLength, out contenstLength);

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return "Нет информации о магазине (Saalut). ";

            var weigdep = from d in context.WeightDeparts
                          where d.Num == weightNum
                          && d.Active == true
                          select d.DepartmentID;
            if (weigdep.Count() == 0)
                return "Не существует записи на весы: " + weightNum;

            //+14052014 весы самообслуживания
            var weightNameE = (from w in context.WeightNames
                               where w.WeightNum == weightNum
                               select w).FirstOrDefault();
            //-14052014 весы самообслуживания

            string fileName = "exp" + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "_").Replace(" ", "_").Replace(":", "_").Replace("-", "_").Replace(@"\", "_").Replace(@"/", "_").Replace("+", "_") + "_" + weightNum + ".csv";

            if (FormatWigruzkiVesoff == "SM300")
            {
                if (weightNum.Substring(0, 1) == "0")
                    fileName = weightNum.Substring(1) + ".csv";
                else
                    fileName = weightNum + ".csv";
            }
            else
                if (FormatWigruzkiVesoff == "dat")
                {
                    fileName = "SM" + weightNameE.WeightIP.ToString() + "f37.dat";
                }

            StringBuilder sb = new StringBuilder();

            // цены укм
            // Create a connection object and data adapter
            DataTable prices;

            int ii = 0;

            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);
                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // Prices
                string cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds5 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds5);

                prices = ds5.Tables[0];




                //-

                var goods = from g in context.Goods
                            where g.Active == true
                            && g.PLU != null
                            && g.PLU != 0
                            && weigdep.Contains(g.DepartmentID)
                            orderby g.DepartmentID ascending
                            select g;
                foreach (Good good in goods)
                {

                    decimal price = 0;
                    if (prices != null)
                    {
                        // select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and deleted = 0
                        DataRow[] prsLsts = prices.Select("item = '" + good.Articul + "'");

                        foreach (DataRow row in prsLsts)
                        {
                            price = (decimal)row[1];
                        }
                    }

                    if (price == 0)
                        continue; // нет цены - пропускаем

                    string str = "";

                    if (FormatWigruzkiVesoff == "CSV" || FormatWigruzkiVesoff == "")
                    {

                        str += ("A;");
                        str += (good.PLU + ";");
                        str += (good.Barcode + ";");
                        if (Division == "RB")
                        {
                            double priceD = 0;
                            Double.TryParse(price.ToString().Substring(0, price.ToString().Length - 5), out priceD);
                            str += ((priceD / 100).ToString() + ";");
                        }
                        else
                            if (Division == "RF")
                            {
                                string price_rf = price.ToString().Substring(0, price.ToString().Length - 2);
                                // bug fix 18-12-12
                                price_rf = price_rf.Replace(",", ".");

                                str += (price_rf + ";");
                            }
                        str += ("0;");
                        str += ("0;"); // ценовая база, исправлять

                        if (Division == "RF")
                        {
                            str += (good.Exp_Qty.ToString() + ";"); // срок годности дни
                        }
                        else
                            if (Division == "RB")
                            {
                                string Srok_Godnosti_Chasov = WebConfigurationManager.AppSettings["Srok_Godnosti_Chasov"];
                                str += (Srok_Godnosti_Chasov + ";"); // срок годности дни
                            }
                            else
                            {
                                str += ("0;"); // срок годности дни
                            }

                        string goodName = good.Name;
                        if (goodName.Length > 80)
                        {
                            goodName = goodName.Substring(0, 79);
                        }
                        str += (goodName);

                        sb.AppendLine(str); // записали строку.

                        str = "";
                        string contentsStr = good.Contents;
                        if (contentsStr != "")
                        {
                            str += ("I;");
                            str += (good.PLU + ";");

                            if (good.Contents.Length > 512)
                                contentsStr = contentsStr.Substring(0, 512);

                            string contents = "";
                            if (good.Contents != null && good.Contents != "")
                                contents = "Состав:" + contentsStr.Replace(";", ",");
                            if (good.Producer != null && good.Producer != "")
                                contents += " Пр-ль:" + good.Producer;

                            int length = contents.Length;
                            int from = 0;
                            while (length >= contenstLength)
                            {
                                str += (contents.Substring(from, contenstLength) + ";");
                                from += contenstLength;
                                length -= contenstLength;
                            }
                            if (length != 0)
                                str += (contents.Substring(from) + ";");

                            sb.AppendLine(str);
                        }

                        //+14052014 весы самообслуживания
                        if (weightNameE != null)
                        {
                            if (weightNameE.SamoObsluzivanie == true)
                            {
                                if (firstCycle)
                                {
                                    // группы
                                    var groups = from g in context.WeightGroups
                                                 where g.Active == true
                                                 select g;
                                    foreach (WeightGroups group in groups)
                                    {

                                        string str1 = "";

                                        str1 += ("K;");
                                        str1 += ("0;");
                                        str1 += (group.GroupNum + ";");
                                        str1 += (group.ButtonNum + ";");
                                        str1 += (group.ButtonName + ";");
                                        str1 += (group.ButtonPic);

                                        sb.AppendLine(str1);
                                    }
                                    firstCycle = false;
                                }

                                // элементы

                                var elem = (from e in context.WeightGoodsPlu
                                            where e.PLU == good.PLU
                                            && e.Active == true
                                            select e).FirstOrDefault();
                                if (elem != null)
                                {
                                    string str2 = "";

                                    str2 += ("K;");
                                    str2 += (elem.PLU + ";");
                                    str2 += (elem.GroupNum + ";");
                                    str2 += (elem.ButtonNum + ";");
                                    str2 += (elem.ButtonName + ";");
                                    str2 += (elem.ButtonPic);

                                    sb.AppendLine(str2);
                                }

                            }
                        }
                        //-14052014 весы самообслуживания

                        ii++;

                    }//FormatWigruzkiVesoff
                    else
                        if (FormatWigruzkiVesoff == "SIS2002")
                        {

                            str += ("A;");
                            str += (good.Barcode + ";");
                            str += (good.PLU + ";");
                            str += ("1;");
                            str += (good.Department.DepartNum_UKM.ToString() + ";");
                            str += ("0;");
                            str += ("0;"); // Ценовая база (0 - 1 Кг, 1 – 100 г)


                            // цена укм
                            if (Division == "RB")
                            {
                                double priceD = 0;
                                Double.TryParse(price.ToString().Substring(0, price.ToString().Length - 5), out priceD);
                                str += ((priceD / 100).ToString() + ";");
                            }
                            else
                                if (Division == "RF")
                                    str += (price.ToString().Substring(0, price.ToString().Length - 2) + ";");
                            // конец цены из укм по новому


                            str += (good.Exp_Qty.ToString() + ";"); // срок годности дни
                            str += ("0;"); // номер тары

                            string goodName = good.Descr;
                            if (goodName.Length > 80)
                            {
                                goodName.Substring(0, 79);
                            }

                            str += (goodName);

                            string contentsStr = good.Contents;
                            if (good.Contents.Length > 512)
                                contentsStr = contentsStr.Substring(0, 512);

                            string contents = "";
                            if (good.Contents != null && good.Contents != "")
                                contents = ";Состав:" + contentsStr.Replace(";", ",");
                            if (good.Producer != null && good.Producer != "")
                                contents += "; Пр-ль:" + good.Producer;

                            int length = contents.Length;
                            int from = 0;
                            while (length >= 50)
                            {
                                str += (contents.Substring(from, 50) + ";");
                                from += 50;
                                length -= 50;
                            }
                            if (length != 0)
                                str += (contents.Substring(from) + ";");

                            sb.AppendLine(str);

                            //+14052014 весы самообслуживания
                            if (weightNameE != null)
                            {
                                if (weightNameE.SamoObsluzivanie == true)
                                {
                                    if (firstCycle)
                                    {
                                        // группы
                                        var groups = from g in context.WeightGroups
                                                     where g.Active == true
                                                     select g;
                                        foreach (WeightGroups group in groups)
                                        {

                                            string str1 = "";

                                            str1 += ("K;");
                                            str1 += ("0;");
                                            str1 += (group.GroupNum + ";");
                                            str1 += (group.ButtonNum + ";");
                                            str1 += (group.ButtonName + ";");
                                            str1 += (group.ButtonPic);

                                            sb.AppendLine(str1);
                                        }
                                        firstCycle = false;
                                    }

                                    // элементы

                                    var elem = (from e in context.WeightGoodsPlu
                                                where e.PLU == good.PLU
                                                && e.Active == true
                                                select e).FirstOrDefault();
                                    if (elem != null)
                                    {
                                        string str2 = "";

                                        str2 += ("K;");
                                        str2 += (elem.PLU + ";");
                                        str2 += (elem.GroupNum + ";");
                                        str2 += (elem.ButtonNum + ";");
                                        str2 += (elem.ButtonName + ";");
                                        str2 += (elem.ButtonPic);

                                        sb.AppendLine(str2);
                                    }

                                }
                            }
                            //-14052014 весы самообслуживания

                            ii++;
                        }//FormatWigruzkiVesoff
                        else
                            if (FormatWigruzkiVesoff == "SM300")
                            {


                                str += ("A;");
                                str += (good.Barcode + ";");
                                str += (good.PLU + ";");
                                str += ("1;");
                                str += (good.Department.DepartNum_UKM.ToString() + ";");
                                str += ("0;");
                                str += ("0;"); // Ценовая база (0 - 1 Кг, 1 – 100 г)


                                // цена укм
                                if (Division == "RB")
                                {
                                    double priceD = 0;
                                    Double.TryParse(price.ToString().Substring(0, price.ToString().Length - 5), out priceD);
                                    str += ((priceD / 100).ToString() + ";");
                                }
                                else
                                    if (Division == "RF")
                                        str += (price.ToString().Substring(0, price.ToString().Length - 2) + ";");
                                // конец цены из укм по новому


                                str += (good.Exp_Qty.ToString() + ";"); // срок годности дни
                                str += ("0;"); // номер тары



                                str += ("0;17;"); // добавилось обновление формата для КУЛОАДА 27-05-2014



                                string goodName = good.Descr;
                                if (goodName.Length > 80)
                                {
                                    goodName.Substring(0, 79);
                                }

                                str += (goodName);

                                string contentsStr = good.Contents;
                                if (good.Contents.Length > 512)
                                    contentsStr = contentsStr.Substring(0, 512);

                                string contents = "";
                                if (good.Contents != null && good.Contents != "")
                                    contents = ";Состав:" + contentsStr.Replace(";", ",");
                                if (good.Producer != null && good.Producer != "")
                                    contents += "; Пр-ль:" + good.Producer;

                                int length = contents.Length;
                                int from = 0;
                                while (length >= 50)
                                {
                                    str += (contents.Substring(from, 50) + ";");
                                    from += 50;
                                    length -= 50;
                                }
                                if (length != 0)
                                    str += (contents.Substring(from) + ";");

                                sb.AppendLine(str);

                                //+14052014 весы самообслуживания
                                if (weightNameE != null)
                                {
                                    if (weightNameE.SamoObsluzivanie == true)
                                    {
                                        if (firstCycle)
                                        {
                                            // группы
                                            var groups = from g in context.WeightGroups
                                                         where g.Active == true
                                                         select g;
                                            foreach (WeightGroups group in groups)
                                            {

                                                string str1 = "";

                                                str1 += ("K;");
                                                str1 += ("0;");
                                                str1 += (group.GroupNum + ";");
                                                str1 += (group.ButtonNum + ";");
                                                str1 += (group.ButtonName + ";");
                                                str1 += (group.ButtonPic);

                                                sb.AppendLine(str1);
                                            }
                                            firstCycle = false;
                                        }

                                        // элементы

                                        var elem = (from e in context.WeightGoodsPlu
                                                    where e.PLU == good.PLU
                                                    && e.Active == true
                                                    select e).FirstOrDefault();
                                        if (elem != null)
                                        {
                                            string str2 = "";

                                            str2 += ("K;");
                                            str2 += (elem.PLU + ";");
                                            str2 += (elem.GroupNum + ";");
                                            str2 += (elem.ButtonNum + ";");
                                            str2 += (elem.ButtonName + ";");
                                            str2 += (elem.ButtonPic);

                                            sb.AppendLine(str2);
                                        }

                                    }
                                }
                                //-14052014 весы самообслуживания

                                ii++;
                            }
                            else
                                if (FormatWigruzkiVesoff == "dat")
                                {

                                    str += good.PLU.ToString().PadLeft(8, '0');
                                    //  str+="00020347004754000DE0110005555011052120347000000100000000071881A8E1AAA2A8E281A5ABAEE0E3E1E1AAA8A98AAEE0A8E68D0D0709AEA282A5E193AFA0AA0C02000C";
                                    //  str += "003A54000DE011000150001105210019900000010000000008179FA1ABAEAAA883AEABECA4A5AD82A5E12088ACAFAEE0E20C02000C00009999980024740010008000000000FF0998000000000000000100000000FFFFFFFFFFC8009999990024750010008000000000FF0999000000000000000100000000FFFFFFFFFF";
                                    /*    str += "004974009DE00300100000110523000010000001000100000000000107058DAEA2EBA90D0715E2AEA2A0E0204130303030303030303030303030";
                                                byte  hash = 0;
                                                string hasht="";
                                                for (int idel = 0; idel < str.Length; idel+=2)
                                                { hash += Convert.ToByte(str.Substring(idel, 2),16);
                                                hasht += hash.ToString("X");

                                                }
                                                byte debag = 110;
                                
                                 
                                                    hash += debag; //коррекция найти почему
                                 
                                                str+=hash.ToString("X");
                                                str += "0C0201200C0101200C00";
                                      */
                                    str += "****54009DE003";
                                    str += price.ToString().Substring(0, price.ToString().Length - 5).PadLeft(8, '0');


                                    str += "1105";
                                    if (good.Barcode.Length == 7)
                                    {
                                        str += (good.Barcode.ToString());
                                    }
                                    else str += 000000;
                                    str += "00000010001";
                                    if ((good.Exp_Qty != 0) && (good.Exp_Qty < 3999))
                                    {
                                        str += good.Exp_Qty.ToString().PadLeft(4, '0');
                                    }
                                    else str += "0000";
                                    str += "00000001";
                                    int namefontweight = weightNameE.Name1Font.Value;      //shrift
                                    int namesimvstr = weightNameE.Name1simvolv.Value;   //число символов в названии в строку
                                    //     byte[] bytarr =  {13,3 };
                                    //     byte[] bytarr1 =Encoding.Convert(Encoding.GetEncoding(866), Encoding.GetEncoding(1251), bytarr);
                                    //     string strent = bytarr1[0].ToString("X").PadLeft(2, '0');
                                    //     strent += bytarr1[1].ToString("X").PadLeft(2, '0');


                                    string goodName = good.Descr; ;
                                    string str1 = goodName;
                                    string str2 = "";
                                    if (str1.Length >= 2 * weightNameE.Name2simvolv)
                                    {
                                        goodName = goodName.Substring(0, 2 * weightNameE.Name2simvolv.Value);
                                        namefontweight = weightNameE.Name2Font.Value;
                                        namesimvstr = weightNameE.Name2simvolv.Value;
                                    }
                                    if (str1.Length >= weightNameE.Name2simvolv + weightNameE.Name1simvolv)
                                    {
                                        namefontweight = weightNameE.Name2Font.Value;
                                        namesimvstr = weightNameE.Name2simvolv.Value;
                                    }

                                    int kor = 0;
                                    // Boolean boo = false;
                                    //  str += namefontweight.ToString("X").PadLeft(2, '0');
                                    //       str += str1.Length.ToString("X").PadLeft(2, '0');
                                    for (int tmpi = 0; tmpi < goodName.Length; tmpi++)
                                    {
                                        if (kor == tmpi)
                                        {
                                            // boo = false;
                                            if (tmpi != 0)
                                            {
                                                str2 += "0D";//новая строка
                                            }
                                            if ((goodName.Length > 2 * weightNameE.Name1simvolv) && (tmpi == weightNameE.Name1simvolv))
                                            {
                                                namefontweight = weightNameE.Name2Font.Value;
                                                namesimvstr = weightNameE.Name2simvolv.Value;


                                            }

                                            str2 += namefontweight.ToString("X").PadLeft(2, '0');
                                            if (goodName.Length - tmpi <= namesimvstr)
                                            {
                                                str2 += (goodName.Length - tmpi).ToString("X").PadLeft(2, '0');
                                            }
                                            else str2 += namesimvstr.ToString("X").PadLeft(2, '0');

                                            kor += namesimvstr;

                                        }

                                        // str2 += ((byte)str1[tmpi]).ToString("X");
                                        str2 += ((byte)Encoding.Convert(Encoding.GetEncoding(1251), Encoding.GetEncoding(866), Encoding.GetEncoding(1251).GetBytes(str1.Substring(tmpi, 1)))[0]).ToString("X");


                                    }
                                    str += str2;
                                    //  str += "0C0201200C0101200C00";
                                    str += "0C";
                                    // str += "02";//шрифт ингридиентов
                                    int contfontweight = weightNameE.contentFont.Value;      //
                                    int contsimvstr = weightNameE.contentsimvol.Value;   //
                                    if (good.Contents != "")
                                    {

                                        string goodcont = good.Contents;
                                        if (goodcont.Length < weightNameE.contentallsimvolLarge)//если маленький состав меняем шрифт
                                        {
                                            contfontweight = weightNameE.contentFontLarge.Value;      //
                                            contsimvstr = weightNameE.contentsimvolLarge.Value;
                                        }
                                        string goodcontt = goodcont;
                                        string goodconthex = "";

                                        //  str += namefontweight.ToString("X").PadLeft(2, '0');
                                        //       str += str1.Length.ToString("X").PadLeft(2, '0');
                                        for (int tmpi = 0; tmpi < goodcontt.Length; tmpi++)
                                        {
                                            if ((tmpi % contsimvstr == 0) && (tmpi != str1.Length))
                                            {
                                                if (tmpi != 0)
                                                {
                                                    goodconthex += "0D";//
                                                }
                                                goodconthex += contfontweight.ToString("X").PadLeft(2, '0');
                                                if (goodcontt.Length - tmpi < contsimvstr)
                                                {
                                                    goodconthex += (goodcontt.Length - tmpi).ToString("X").PadLeft(2, '0');
                                                }
                                                else goodconthex += contsimvstr.ToString("X").PadLeft(2, '0');
                                            }
                                            // str2 += ((byte)str1[tmpi]).ToString("X");
                                            goodconthex += ((byte)Encoding.Convert(Encoding.GetEncoding(1251), Encoding.GetEncoding(866), Encoding.GetEncoding(1251).GetBytes(goodcontt.Substring(tmpi, 1)))[0]).ToString("X");


                                        }
                                        str += goodconthex;
                                        str += "0C0101200C00";
                                    }
                                    else str += "0201200C0101200C00";
                                    int lenghtstr = str.Length / 2;
                                    str = str.Replace("****", lenghtstr.ToString("X").PadLeft(4, '0'));
                                    sb.Append(str);

                                    ii++;
                                }//FormatWigruzkiVesoff

                }


            }
            catch (MySqlException ex)
            {
                return "Ошибка загрузки весов " + weightNum + ": " + ex.ToString();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            string retStr = "";

            if (FormatWigruzkiVesoff != "dat")
            {
                // добавляем в новом релизе удаление. 2.5.4.
                string[] FilesForDel = Directory.GetFiles(dirPath);
                foreach (string file in FilesForDel)
                {
                    File.Delete(file);
                }


                StreamWriter sw = new StreamWriter(exportDir + fileName, false, Encoding.GetEncoding(1251), 2048);
                sw.Write(sb);
                sw.Close();

                int i = 0;

                string[] Files = Directory.GetFiles(exportDir);
                foreach (string file in Files)
                {
                    string fileToCopy = System.IO.Path.GetFileName(file);
                    File.Copy(file, dirPath + fileToCopy, true);
                    File.Delete(file);
                    i++;
                }

                SaveLogs log = new SaveLogs();

                string weightName = "не указано";
                var wtname = (from w in context.WeightNames
                              where w.WeightNum == weightNum
                              select w).FirstOrDefault();
                if (wtname != null)
                    weightName = wtname.Name;


                retStr = "Сформированы файлы для выгрузки на весы " + weightNum + "(" + weightName + "): " + i.ToString() + " файлов, " + ii.ToString() + " товаров. ";
                log.SaveToLog("Весы", retStr);

            }
            else
            {
                //-


                StreamWriter sw = new StreamWriter(exportDir + fileName, false, Encoding.GetEncoding(1251), 2048);
                sw.Write(sb);
                sw.Close();

                int i = 0;
                string rezult = "";

                string[] Files = Directory.GetFiles(exportDir, fileName);
                foreach (string file in Files)
                {
                    string fileToCopy = System.IO.Path.GetFileName(file);
                    File.Copy(file, "C:\\inetpub\\wwwroot\\DIGIWTCP\\DATA\\" + fileToCopy, true);

                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = "C:\\inetpub\\wwwroot\\DIGIWTCP\\отпр_plu.bat";
                    proc.StartInfo.RedirectStandardError = false;
                    proc.StartInfo.RedirectStandardOutput = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.WorkingDirectory = "C:\\inetpub\\wwwroot\\DIGIWTCP\\";
                    proc.StartInfo.Arguments = weightNameE.WeightIP.ToString();
                    proc.Start();
                    proc.WaitForExit();
                    File.Delete(file);
                    i++;
                    string text = System.IO.File.ReadAllText(@"C:\inetpub\wwwroot\DIGIWTCP\result");
                    if (text.Length > 2)
                    {
                        rezult = text.Substring(text.Length - 2).Trim(':', '-');
                    }
                    switch (rezult)
                    {
                        case "0":
                            rezult = "Весы прогружены";
                            break;
                        case "1":
                            rezult = "Ошибка открытия файла";
                            break;
                        case "2":
                            rezult = "Ошибка чтения из файла ";
                            break;
                        case "3":
                            rezult = "Ошибка записи в файл ";
                            break;
                        case "4":
                            rezult = "Ошибка инициализации сети ";
                            break;
                        case "5":
                            rezult = "Ошибка открытия сетевого соединения ";
                            break;
                        case "6":
                            rezult = "Ошибка чтения из сетевого соединения ";
                            break;
                        case "7":
                            rezult = "Ошибка записи в сетевое соединение ";
                            break;
                        case "8":
                            rezult = "Ошиска чтения с весов ";
                            break;
                        case "9":
                            rezult = "Ошибка записи в весы ";
                            break;
                        case "10":
                            rezult = "Записи нет в весах ";
                            break;
                        case "11":
                            rezult = "В весах недостаточно памяти ";
                            break;
                        default:
                            rezult = "Неопределенная ошибка ";
                            break;

                    }

                }

                SaveLogs log = new SaveLogs();

                string weightName = "не указано";

                if (weightNameE != null)
                    weightName = weightNameE.Name;


                retStr = "Весы " + weightNum + "(" + weightName + ") отправлено:" + i.ToString() + " файл, " + ii.ToString() + " товаров. Результат прогрузки: " + rezult;
                log.SaveToLog("Весы", retStr);

            }

            return retStr;
        }

        public string ExportToWE_qload_by_label_UKMPrice_by_format(string weightNum)
        {
            string FormatWigruzkiVesoff = WebConfigurationManager.AppSettings["Format_Wigruzki_Vesov"];

            string Division = WebConfigurationManager.AppSettings["Division"];

            string dirPath = WebConfigurationManager.AppSettings["UKM_Import_Path"];
            string exportDir = HttpRuntime.AppDomainAppPath + "\\Export\\";

            string contLength = WebConfigurationManager.AppSettings["Content_Length_Weight"];
            Int32.TryParse(contLength, out contenstLength);

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();

            string weightName = "не указано";
            var wtname = (from w in context.WeightNames
                          where w.WeightNum == weightNum
                          select w).FirstOrDefault();
            if (wtname != null)
                weightName = wtname.Name;
            if (store == null)
                return "Нет информации о магазине (Saalut). ";

            var weigdep = from d in context.WeightDeparts
                          where d.Num == weightNum
                          && d.Active == true
                          select d.DepartmentID;
            if (weigdep.Count() == 0)
                return "Не существует записи на весы: " + weightNum;



            string fileName = "";

            fileName = "SM" + wtname.WeightIP.ToString() + "f61.dat";

            StringBuilder sb = new StringBuilder();



            int ii = 0;


            MySqlConnection cnx = null;
            try
            {
                string str = "";
                str += "00000001****0011";
                string magazfont = "1";
                str += magazfont.PadLeft(2, '0');
                str += (store.Company.Length + store.AddressFact.Length + 1).ToString("X");
                string storecont = store.Company + ' ' + store.AddressFact;
                string storecontt = storecont;
                string storeconthex = "";

                //  str += namefontweight.ToString("X").PadLeft(2, '0');
                //       str += str1.Length.ToString("X").PadLeft(2, '0');
                for (int tmpi = 0; tmpi < storecontt.Length; tmpi++)
                {


                    // str2 += ((byte)str1[tmpi]).ToString("X");
                    storeconthex += ((byte)Encoding.Convert(Encoding.GetEncoding(1251), Encoding.GetEncoding(866), Encoding.GetEncoding(1251).GetBytes(storecontt.Substring(tmpi, 1)))[0]).ToString("X");


                }
                storeconthex += "0D";
                storeconthex += magazfont.PadLeft(2, '0');
                storeconthex += store.AddressJur.Length.ToString("X");

                storecont = store.AddressJur;
                storecontt = storecont;
                for (int tmpi = 0; tmpi < storecontt.Length; tmpi++)
                {


                    // str2 += ((byte)str1[tmpi]).ToString("X");
                    storeconthex += ((byte)Encoding.Convert(Encoding.GetEncoding(1251), Encoding.GetEncoding(866), Encoding.GetEncoding(1251).GetBytes(storecontt.Substring(tmpi, 1)))[0]).ToString("X");


                }
                str += storeconthex;
                str += "0C";
                int lenghtstr = str.Length / 2;
                str = str.Replace("****", lenghtstr.ToString("X").PadLeft(4, '0'));
                sb.Append(str);
            }
            catch (MySqlException ex)
            {
                Log l = new Log();
                l.Message = ex.Message;
                l.TimeStamp = DateTime.Now;
                l.Type = "";
                context.Logs.InsertOnSubmit(l);
                context.SubmitChanges();

                return "Ошибка загрузки весов " + weightNum + ": " + ex.ToString();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }


            StreamWriter sw = new StreamWriter(exportDir + fileName, false, Encoding.GetEncoding(1251), 2048);
            sw.Write(sb);
            sw.Close();

            int i = 0;

            string[] Files = Directory.GetFiles(exportDir);
            foreach (string file in Files)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);
                File.Copy(file, "C:\\inetpub\\wwwroot\\DIGIWTCP\\DATA\\" + fileToCopy, true);
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "C:\\inetpub\\wwwroot\\DIGIWTCP\\отпр_файла_названия_магазина.bat";
                proc.StartInfo.RedirectStandardError = false;
                proc.StartInfo.RedirectStandardOutput = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.WorkingDirectory = "C:\\inetpub\\wwwroot\\DIGIWTCP\\";
                proc.StartInfo.Arguments = wtname.WeightIP.ToString();
                proc.Start();
                proc.WaitForExit();
                File.Delete(file);
                i++;
            }

            string[] files34 = Directory.GetFiles("C:\\inetpub\\wwwroot\\DIGIWTCP\\DATA\\label\\" + wtname.WeightIP.ToString(), "*34.dat");
            string[] files38 = Directory.GetFiles("C:\\inetpub\\wwwroot\\DIGIWTCP\\DATA\\label\\" + wtname.WeightIP.ToString(), "*38.dat");
            string[] files65 = Directory.GetFiles("C:\\inetpub\\wwwroot\\DIGIWTCP\\DATA\\label\\" + wtname.WeightIP.ToString(), "*65.dat");
            foreach (string file in files34)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);
                File.Copy(file, "C:\\inetpub\\wwwroot\\DIGIWTCP\\DATA\\" + "sm" + wtname.WeightIP.ToString() + "f52.dat", true);
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "C:\\inetpub\\wwwroot\\DIGIWTCP\\отпр_формата_печати.bat";
                proc.StartInfo.RedirectStandardError = false;
                proc.StartInfo.RedirectStandardOutput = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.WorkingDirectory = "C:\\inetpub\\wwwroot\\DIGIWTCP\\";
                proc.StartInfo.Arguments = wtname.WeightIP.ToString();
                proc.Start();
                proc.WaitForExit();
                i++;
            }
            foreach (string file in files38)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);
                File.Copy(file, "C:\\inetpub\\wwwroot\\DIGIWTCP\\DATA\\" + "sm" + wtname.WeightIP.ToString() + "f56.dat", true);
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "C:\\inetpub\\wwwroot\\DIGIWTCP\\отпр_файла_текстов.bat";
                proc.StartInfo.RedirectStandardError = false;
                proc.StartInfo.RedirectStandardOutput = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.WorkingDirectory = "C:\\inetpub\\wwwroot\\DIGIWTCP\\";
                proc.StartInfo.Arguments = wtname.WeightIP.ToString();
                proc.Start();
                proc.WaitForExit();
                i++;
            }
            if (wtname.keyboard == true)
            {
                foreach (string file in files65)
                {
                    string fileToCopy = System.IO.Path.GetFileName(file);
                    File.Copy(file, "C:\\inetpub\\wwwroot\\DIGIWTCP\\DATA\\" + "sm" + wtname.WeightIP.ToString() + "f65.dat", true);
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = "C:\\inetpub\\wwwroot\\DIGIWTCP\\отпр_файла_назначеных_клавиш.bat";
                    proc.StartInfo.RedirectStandardError = false;
                    proc.StartInfo.RedirectStandardOutput = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.WorkingDirectory = "C:\\inetpub\\wwwroot\\DIGIWTCP\\";
                    proc.StartInfo.Arguments = wtname.WeightIP.ToString();
                    proc.Start();
                    proc.WaitForExit();
                    i++;
                }
            }
            if (wtname.timesynh == true)
            {

                DateTime saveNow = DateTime.Now;

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "C:\\inetpub\\wwwroot\\DIGIWTCP\\digi_time.exe";
                proc.StartInfo.RedirectStandardError = false;
                proc.StartInfo.RedirectStandardOutput = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.WorkingDirectory = "C:\\inetpub\\wwwroot\\DIGIWTCP\\";
                // proc.StartInfo.Arguments = wtname.WeightIP.ToString();
                char[] charsToTrim = { '2', '0' };
                proc.StartInfo.Arguments = "-i" + wtname.WeightIP.ToString() + " -p2250 -d" + saveNow.Day.ToString().PadLeft(2, '0') + saveNow.Month.ToString().PadLeft(2, '0') + saveNow.Year.ToString().TrimStart(charsToTrim) + " -t" + saveNow.Hour.ToString().PadLeft(2, '0') + saveNow.Minute.ToString().PadLeft(2, '0');
                proc.Start();
                proc.WaitForExit();
            }

            SaveLogs log = new SaveLogs();




            string retStr = "Сформированы файлы для выгрузки на весы " + weightNum + "(" + weightName + "): " + i;
            log.SaveToLog("Весы", retStr);


            return retStr;


        }



        public string ExportToWE_qload_by_weights_UKMPrice_BY_Journ(int journalID)
        {
            // переделать под журнал а не выгрузку всего на все.

            ExportAllToQload();

            return "ok";
        }


        public string ExportAllToQload()
        {
            string strRet = "";

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return "Нет информации о магазине (Saalut). ";


            var weigdeps = from d in context.WeightDeparts
                           select d;
            if (weigdeps.Count() == 0)
                return "Не существует записей весы-отдел с кодом.";

            string lastWT = "";
            foreach (WeightDepart dpt in weigdeps)
            {
                if (dpt.Num == lastWT)
                    continue;

                this.ExportToWE_qload_by_weights_UKMPrice_by_format(dpt.Num);

                lastWT = dpt.Num;
            }

            return strRet;
        }
        //-


        public string HourOfExportToWeightNighth()
        {
            string hour = WebConfigurationManager.AppSettings["Export_Night_To_Weight"];
            return hour;
        }


        public string ExportAllToWE()
        {
            string Division = WebConfigurationManager.AppSettings["Division"];
            string FormatWigruzkiVesoff = WebConfigurationManager.AppSettings["Format_Wigruzki_Vesov"];

            if (FormatWigruzkiVesoff == "SM300" && Division == "RF")
            {
                ExportAllToQload();
                return "SM300 - RF - ok";
            }


            string dirPath = WebConfigurationManager.AppSettings["UKM_Import_Path"];
            string exportDir = HttpRuntime.AppDomainAppPath + "\\Export\\";

            context = new SaalutDataClasses1DataContext();


            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return "Нет информации о магазине (Saalut). ";

            //+ load prices
            // цены укм
            // Create a connection object and data adapter
            DataTable prices;

            int ii = 0;

            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);
                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // Prices
                string cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
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
                return "Error: " + ex.ToString();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            //- load prices


            string fileName = "exp" + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "_").Replace(" ", "_").Replace(":", "_").Replace("-", "_").Replace(@"\", "_").Replace(@"/", "_").Replace("+", "_") + ".csv";


            StringBuilder sb = new StringBuilder();

            sb.AppendLine("T;1;Магазин " + store.StoreName); // одна запись для магазина



            int lastDeptID = 0;

            var goods = from g in context.Goods
                        where g.Active == true
                        && g.PLU != null
                        && g.PLU != 0
                        && g.Department != null
                        orderby g.DepartmentID ascending
                        select g;
            foreach (Good good in goods)
            {
                string str = "";

                if (lastDeptID != good.DepartmentID)
                {
                    sb.AppendLine("B;" + good.Department.DepartNum_UKM.ToString() + ";1;" + good.Department.DepartName);
                }
                lastDeptID = good.DepartmentID.Value;


                str += ("A;");
                str += (good.Barcode + ";");
                str += (good.PLU + ";");
                str += ("1;");
                str += (good.Department.DepartNum_UKM.ToString() + ";");
                str += ("0;");
                str += ("0;"); // Ценовая база (0 - 1 Кг, 1 – 100 г)


                // цена укм

                decimal price = 0;
                if (prices != null)
                {
                    // select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and deleted = 0
                    DataRow[] prsLsts = prices.Select("item = '" + good.Articul + "'");

                    foreach (DataRow row in prsLsts)
                    {
                        price = (decimal)row[1];
                    }
                }

                if (price == 0)
                    continue; // нет цены - пропускаем


                if (Division == "RB")
                {
                    double priceD = 0;
                    Double.TryParse(price.ToString().Substring(0, price.ToString().Length - 5), out priceD);
                    str += ((priceD / 100).ToString() + ";");
                }
                else
                    if (Division == "RF")
                        str += (price.ToString().Substring(0, price.ToString().Length - 2) + ";");

                // конец цены из укм по новому


                str += (good.Exp_Qty.ToString() + ";"); // срок годности дни
                str += ("0;"); // номер тары

                string goodName = good.Descr;
                if (goodName.Length > 80)
                {
                    goodName.Substring(0, 79);
                }

                str += (goodName);

                string contentsStr = good.Contents;
                if (good.Contents.Length > 450)
                    contentsStr = contentsStr.Substring(0, 450);

                string contents = "";
                if (good.Producer != null && good.Producer != "")
                    contents = ";Пр-ль:" + good.Producer;
                if (good.Contents != null && good.Contents != "")
                    contents += ";Состав:" + contentsStr.Replace(";", ",");

                contents += ";" + store.JurNameTermoPrn + " ;" + store.JurAddrTermoPrn;

                int length = contents.Length;
                int from = 0;
                while (length >= 50)
                {
                    str += (contents.Substring(from, 50) + ";");
                    from += 50;
                    length -= 50;
                }
                if (length != 0)
                    str += (contents.Substring(from) + ";");

                sb.AppendLine(str);



                ii++;
            }


            // сюда тоже удаление добавить

            string[] FilesForDel = Directory.GetFiles(dirPath);
            foreach (string file in FilesForDel)
            {
                File.Delete(file);
            }

            StreamWriter sw = new StreamWriter(exportDir + fileName, false, Encoding.GetEncoding(1251), 2048);
            sw.Write(sb);
            sw.Close();

            int i = 0;

            string[] Files = Directory.GetFiles(exportDir);
            foreach (string file in Files)
            {
                string fileToCopy = System.IO.Path.GetFileName(file);
                File.Copy(file, dirPath + fileToCopy, true);
                File.Delete(file);
                i++;
            }

            SaveLogs log = new SaveLogs();
            log.SaveToLog("Весы", "Сформированы файлы для выгрузки на весы: " + i.ToString() + " файлов, " + ii.ToString() + " товаров.");


            return "ok";
        }


        public string ExportFromUKMDirect()
        {
            context = new SaalutDataClasses1DataContext();


            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return "Нет информации о магазине (Saalut). ";


            string dirPath = WebConfigurationManager.AppSettings["UKM_Import_Path"];
            string exportDir = HttpRuntime.AppDomainAppPath + "\\Export\\";
            string fileName = "exp" + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK").Replace(".", "_").Replace(" ", "_").Replace(":", "_").Replace("-", "_").Replace(@"\", "_").Replace(@"/", "_").Replace("+", "_") + ".csv";
            StreamWriter sw = new StreamWriter(exportDir + fileName, false, Encoding.GetEncoding(1251));
            sw.WriteLine("T;1;Магазин " + store.StoreName); // одна запись для магазина


            CommittableTransaction tx = new CommittableTransaction();

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {

                int ii = 0;

                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // Выбираем магазин
                string cmdText = "";
                MySqlCommand cmd = null;

                cmdText = "select  id, name, version, deleted	from 	ukmserver.srv_assortment_groups where store_id = '" + store.StoreID_UKM.ToString() + "' ";
                cmd = new MySqlCommand(cmdText, cnx);

                // Create a fill a Dataset
                DataSet dsGr = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(dsGr);

                DataTable groups = dsGr.Tables[0];


                // items
                cmdText = "select id, name, descr, measure, classif, version, deleted	 from ukmserver.trm_in_items where deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);

                // Create a fill a Dataset
                DataSet ds1 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds1);

                DataTable items = ds1.Tables[0];


                // Barcode
                cmdText = "select id, item, version, deleted from ukmserver.trm_in_var ";
                cmd = new MySqlCommand(cmdText, cnx);

                // Create a fill a Dataset
                DataSet ds2 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds2);

                DataTable barcodes = ds2.Tables[0];


                // Assortment group items
                cmdText = "select ag_id, var, plu, exp_date1, exp_date2, version, deleted from ukmserver.srv_assortment_group_items where store_id = '" + store.StoreID_UKM.ToString() + "' and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);

                // Create a fill a Dataset
                DataSet ds3 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds3);

                DataTable assorts = ds3.Tables[0];


                // Structure
                cmdText = "select id, country, structure, version, deleted, producer_marking from ukmserver.trm_in_item_cc ";
                cmd = new MySqlCommand(cmdText, cnx);

                // Create a fill a Dataset
                DataSet ds4 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds4);

                DataTable structures = ds4.Tables[0];


                // Prices
                cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);

                // Create a fill a Dataset
                DataSet ds5 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds5);

                DataTable prices = ds5.Tables[0];

                if (assorts != null)
                {
                    foreach (DataRow row in assorts.Rows)
                    {
                        int ag_id = 0;
                        string variable = "";
                        int plu = 0;
                        DateTime exp_date1 = DateTime.MinValue;
                        int exp_date2 = 0;

                        ag_id = (int)row[0];
                        variable = (string)row[1];
                        plu = Int32.Parse(row[2].ToString());
                        DateTime.TryParse(row[3].ToString(), out exp_date1);
                        exp_date2 = (int)row[4];

                        string item = "";

                        // select id, item, version, deleted from ukmserver.trm_in_var
                        DataRow[] bcs = barcodes.Select("id = '" + variable + "'");

                        foreach (DataRow rowBC in bcs)
                        {
                            item = rowBC[1].ToString();
                        }

                        string id = "";
                        string name = "";
                        string descr = "";
                        string measure = "";
                        string classif = "";

                        DataRow[] goods = items.Select("item = '" + item + "'");

                        foreach (DataRow rowItem in goods)
                        {
                            id = rowItem[0].ToString();
                            name = rowItem[1].ToString();
                            descr = rowItem[2].ToString();
                            measure = rowItem[3].ToString();
                            classif = rowItem[4].ToString();
                        }



                        DataRow[] assortGrp = groups.Select("id = '" + ag_id + "'");
                        string nameGroup = "";
                        foreach (DataRow rowGrp in goods)
                        {
                            nameGroup = rowGrp[1].ToString();
                        }


                        // Состав
                        string country = "";
                        string structure = "";
                        string producer = "";

                        if (structures != null)
                        {
                            // select id, country, structure, version, deleted, producer_marking	 from ukmserver.trm_in_item_cc
                            DataRow[] strs = structures.Select("id = '" + id + "'");
                            foreach (DataRow rowStruc in strs)
                            {
                                country = rowStruc[1].ToString();
                                structure = rowStruc[2].ToString();
                                producer = rowStruc[5].ToString();
                            }
                        }


                        decimal price = 0;
                        if (prices != null)
                        {
                            // select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and deleted = 0
                            DataRow[] prsLsts = prices.Select("item = '" + id + "'");

                            foreach (DataRow rowPr in prsLsts)
                            {
                                price = (decimal)rowPr[1];
                            }
                        }

                        if (plu == 0)
                            continue;
                        if (price == 0)
                            continue;


                        //--------------------
                        sw.WriteLine("B;" + ag_id + ";1;" + nameGroup);


                        string str = "";

                        str += ("A;");
                        str += (variable + ";");
                        str += (plu + ";");
                        str += ("1;");
                        str += (nameGroup + ";");
                        str += ("0;");
                        str += ("0;"); // Ценовая база (0 - 1 Кг, 1 – 100 г)
                        str += (price.ToString() + ";");
                        str += (exp_date2.ToString() + ";"); // срок годности дни
                        str += ("0;"); // номер тары
                        str += (descr + ";");

                        string contentsStr = structure;
                        if (contentsStr.Length > 400)
                            contentsStr = contentsStr.Substring(0, 400);

                        string contents = "";
                        if (contentsStr != null && contentsStr != "")
                            contents = "Состав:" + contentsStr.Replace(";", ",");
                        if (producer != null && producer != "")
                            contents += " Пр-ль:" + producer;

                        int length = contents.Length;
                        int from = 0;
                        while (length >= 50)
                        {
                            str += (contents.Substring(from, 50) + ";");
                            from += 50;
                            length -= 50;
                        }
                        if (length != 0)
                            str += (contents.Substring(from) + ";");

                        sw.WriteLine(str);


                        ii++;
                    }
                }

                sw.Close();

                string[] FilesForDel = Directory.GetFiles(dirPath);
                foreach (string file in FilesForDel)
                {
                    File.Delete(file);
                }

                int i = 0;

                string[] Files = Directory.GetFiles(exportDir);
                foreach (string file in Files)
                {
                    string fileToCopy = System.IO.Path.GetFileName(file);
                    File.Copy(file, dirPath + fileToCopy, true);
                    File.Delete(file);
                    i++;
                }

                SaveLogs log = new SaveLogs();
                log.SaveToLog("Весы", "Сформированы файлы для выгрузки на весы: " + i.ToString() + " файлов, " + ii.ToString() + " товаров.");


                tx.Commit();
            }
            catch (MySqlException ex)
            {
                return "Error: " + ex.ToString();
                tx.Rollback();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            return "ok";

        }

    }
}