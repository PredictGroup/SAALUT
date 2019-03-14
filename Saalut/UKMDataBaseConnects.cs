using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.Common;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data;
using System.Transactions;
using System.Web.Configuration;

namespace Saalut
{
    public class UKMDataBaseConnects
    {
        SaalutDataClasses1DataContext context;

        // Connection string for a typical local MySQL installation
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;

        // Инициализация таблиц БД

        public string InitialDB()
        {
            string ret = "";
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            // + Инициализация магазина
            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                ret += "Инициализация магазина: " + InitialStoreInfoIns() + ". ";
            else
                ret += "Магазин инициализирован ранее. ";
            // - Инициализация магазина

            // + Инициализация шаблонов ценников
            ret += "Инициализация ценников: " + InitialPriceTemplatesIns();
            // - Инициализация шаблонов ценников

            // + Инициализация классификатора
            ret += "Инициализация классификатора: " + InitialItemClassifIns();
            // - Инициализация классификатора

            // + Инициализация отделов
            ret += "Инициализация отделов: " + InitialDepartsIns();
            // - Инициализация отделов

            ret += "Обновление товаров: " + UpdateGoodsOptimized() + ". ";

            SaveLogs log = new SaveLogs();
            log.SaveToLog("Данные", "Полная инициализация завершена. " + ret);


            return ret;
        }

        public string InitialMatWeightDB()
        {
            string ret = "";
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            // + Инициализация магазина
            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                ret += "Инициализация магазина: " + InitialStoreInfoIns() + ". ";
            else
                ret += "Магазин инициализирован ранее. ";
            // - Инициализация магазина

            // + Инициализация классификатора
            ret += "Инициализация классификатора: " + InitialItemClassifIns();
            // - Инициализация классификатора

            // + Инициализация отделов
            ret += "Инициализация отделов: " + InitialDepartsIns();
            // - Инициализация отделов


            SaveLogs log = new SaveLogs();
            log.SaveToLog("Данные", "Весовых инициализация завершена. " + ret);


            return ret;
        }


        public string NightLoadsToDB()
        {
            string ret = "";
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            // + Инициализация классификатора
            ret += "Инициализация классификатора: " + InitialItemClassifIns();
            // - Инициализация классификатора

            // + Инициализация отделов
            ret += "Инициализация отделов: " + InitialDepartsIns();
            // - Инициализация отделов

            ret += "Обновление товаров: " + UpdateGoods() + ". ";

            return ret;
        }

        public string UKM_Quick_LoadsToDB()
        { // быстрая загрузка товаров
            string ret = "";

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return "Не найдет магазин в настройках. ";

            // загружаем по артикулу, новые товары. и только
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                string cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; select count(id) from ukmserver.trm_in_items ;SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ;";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                DataTable itemSum = ds.Tables[0];
                Int64 counter = 0;
                if (itemSum != null)
                {
                    foreach (DataRow row in itemSum.Rows)
                    {
                        counter = (Int64)row[0];
                    }
                }

                var countGoods = (from g in context.Goods
                                  select g).Count();

                if (counter == countGoods)
                    return ret = "Нет информации для обновления. ";
                //---------------------------

                ret += "Для обновления поставлено " + (counter - countGoods).ToString() + " строк. ";

                // items
                cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select id, name, descr, measure, classif, version, deleted from ukmserver.trm_in_items; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;


                // Create a fill a Dataset
                DataSet ds1 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds1);

                DataTable items = ds1.Tables[0];

                // выбираем все артикулы базы данных
                Dictionary<string, string> goodsArtsDict = new Dictionary<string, string>();

                var goodsArtsInDB = from g in context.Goods
                                    select g.Articul;

                foreach (string gd in goodsArtsInDB)
                {
                    goodsArtsDict.Add(gd, gd);
                }

                // проверяем чего нет у нас в базе
                Dictionary<string, string> goodsNotInDB = new Dictionary<string, string>();

                foreach (DataRow row in items.Rows)
                {
                    string articul = row[0].ToString();

                    string articulInDB = "";
                    if (!goodsArtsDict.TryGetValue(articul, out articulInDB))
                    {
                        goodsNotInDB.Add(articul, articul);
                    }
                }

                if (goodsNotInDB.Values.Count == 0)
                    return ret = "Не выбрано позиций для обновления. ";

                string goodsForSelect = "";
                int i = 1;
                foreach (KeyValuePair<string, string> pair in goodsNotInDB)
                {
                    goodsForSelect += "'" + pair.Key + "'";
                    if (goodsNotInDB.Values.Count != i)
                    {
                        goodsForSelect += ",";
                        i++;
                    }
                }

                // делаем селект товаров тех что нужно обновить
                cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; select id, name, descr, measure, classif, version, deleted from ukmserver.trm_in_items where id in (" + goodsForSelect + ") ;SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ;";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds9 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds9);

                DataTable goodItems = ds9.Tables[0];

                // Barcode
                //cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; select id, item, version, deleted from ukmserver.trm_in_var where item in (" + goodsForSelect + ") and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; select id, item, version, deleted from ukmserver.trm_in_var where item = 0 and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                // здесь передаем пустую таблицу цен для того чтобы в дальнейшем в журнал изменения цен попали новые цены от новых товаров. 29-07-12
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds2 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds2);

                DataTable barcodes = ds2.Tables[0];


                cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; select ag_id, var, plu, exp_date1, exp_date2, version, deleted from ukmserver.srv_assortment_group_items where var in (" + goodsForSelect + ") and deleted = 0 ; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ;";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds3 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds3);

                DataTable assorts = ds3.Tables[0];


                // Состав
                cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; select id, country, structure, version, deleted, producer_marking from ukmserver.trm_in_item_cc where id in (" + goodsForSelect + ") and deleted = 0 ; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ;";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds4 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds4);

                DataTable structures = ds4.Tables[0];


                cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; select item, price, version, deleted from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and item in (" + goodsForSelect + ") and deleted = 0 ; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds5 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds5);

                DataTable prices = ds5.Tables[0];

                // PrintTemplate good
                cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; select item_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_item where item_id in (" + goodsForSelect + ") ; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ;";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds6 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds6);

                DataTable printTemplGood = ds6.Tables[0];


                CreateNewGoods(goodItems, barcodes, assorts, structures, prices, printTemplGood);

            }
            catch (MySqlException ex)
            {
                return ex.Message;
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            return ret;
        }

        public string InitialStoreInfoIns()
        {
            string ret = "";

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            CommittableTransaction tx = new CommittableTransaction();

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // = делаем текущий прайс лист магазина


                // Выбираем магазин
                string cmdText = "select store_id, name, pricetype_id from ukmserver.trm_in_store where deleted = 0";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                int store_id = 0;
                string name = "";
                string pricetype_id = "";

                if (ds.Tables.Count != 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        store_id = (int)row[0];
                        name = row[1].ToString();
                        pricetype_id = row[2].ToString();
                        break;
                    }
                }
                else
                    ret += "УКМ Магазин - пусто (trm_in_store). ";


                int pricelist = 0;
                string pricelistName = "";

                if (pricetype_id != "")
                {
                    cmdText = "select pricelist from ukmserver.trm_in_pricetype_pricelist where store_id = '" + store_id + "' and pricetype = '" + pricetype_id + "' and deleted = 0";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds2 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds2);

                    if (ds2.Tables.Count != 0)
                    {
                        foreach (DataRow row2 in ds2.Tables[0].Rows)
                        {
                            pricelist = (int)row2[0];
                            break;
                        }
                    }

                    if (pricelist != 0)
                    {
                        cmdText = "select 	name	from 	ukmserver.trm_in_pricelist where pricelist_id = '" + pricelist.ToString() + "' and deleted = 0";
                        cmd = new MySqlCommand(cmdText, cnx);
                        cmd.CommandTimeout = 30000;

                        // Create a fill a Dataset
                        DataSet ds3 = new DataSet();
                        adapter.SelectCommand = cmd;
                        adapter.Fill(ds3);

                        if (ds3.Tables.Count != 0)
                        {
                            foreach (DataRow row3 in ds3.Tables[0].Rows)
                            {
                                pricelistName = row3[0].ToString();
                                break;
                            }
                        }
                    }
                }
                else
                    ret += "УКМ Прайс-лист - пусто (trm_in_pricetype_pricelist). ";

                // = делаем акционный прайс лист

                string AkcPricetype_id = "3";

                int AkcPricelist = 0;
                string AkcPricelistName = "";

                cmdText = "select pricelist from ukmserver.trm_in_pricetype_pricelist where store_id = '" + store_id + "' and pricetype = '" + AkcPricetype_id + "' and deleted = 0";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds21 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds21);

                if (ds21.Tables.Count != 0)
                {
                    foreach (DataRow row2 in ds21.Tables[0].Rows)
                    {
                        AkcPricelist = (int)row2[0];
                        break;
                    }
                }

                if (AkcPricelist != 0)
                {
                    cmdText = "select 	name	from 	ukmserver.trm_in_pricelist where pricelist_id = '" + AkcPricelist.ToString() + "' and deleted = 0";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds3 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds3);

                    if (ds3.Tables.Count != 0)
                    {
                        foreach (DataRow row3 in ds3.Tables[0].Rows)
                        {
                            AkcPricelistName = row3[0].ToString();
                            break;
                        }
                    }
                }


                // сохраняем

                if (store_id != 0 && pricelist != 0)
                {
                    StoreInfos n = new StoreInfos();
                    n.StoreName = name;
                    n.Company = "";
                    n.AddressFact = "";
                    n.AddressJur = "";
                    n.StoreID_UKM = store_id;
                    n.PriceType_ID_UKM = pricetype_id;
                    n.PriceList_ID_UKM = pricelist;
                    n.PriceListName_UKM = pricelistName;
                    n.ActPriceType_ID_UKM = AkcPricetype_id;
                    n.ActPriceList_ID_UKM = AkcPricelist;
                    n.ActPriceListName_UKM = AkcPricelistName;
                    n.TimeStamp = DateTime.Now;
                    n.Active = true;

                    context.StoreInfos.InsertOnSubmit(n);
                    context.SubmitChanges();

                    ret += "ok. ";
                }


                tx.Commit();
            }
            catch (MySqlException ex)
            {
                ret += "Error: " + ex.ToString();
                tx.Rollback();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            return ret;
        }

        public string InitialPriceTemplatesIns()
        {
            string ret = "";

            CommittableTransaction tx = new CommittableTransaction();

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // Выбираем магазин
                string cmdText = "select id, description from ukmserver.srv_rd_template where type = 8 and 	deleted = 0";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                if (ds.Tables.Count != 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        int id = (int)row[0];
                        string description = (string)row[1];

                        var templ = (from t in context.PrintTemplates
                                     where t.UKM_ID == id
                                     && t.Active == true
                                     select t).FirstOrDefault();
                        if (templ == null)
                        {
                            PrintTemplates t = new PrintTemplates();
                            t.UKM_ID = id;
                            t.TemplateName = description;
                            t.FileName = "";
                            t.QtyWide = 1;
                            t.QtyHigh = 1;
                            t.SkipBetweenPagesMM = 0;
                            t.TimeStamp = DateTime.Now;
                            t.Active = true;
                            context.PrintTemplates.InsertOnSubmit(t);
                            context.SubmitChanges();

                            ret += "Новый ценник: " + id.ToString() + " " + description + ". ";
                        }
                    }
                }
                else
                    ret += "УКМ Шаблоны ценников - пусто (srv_rd_template). ";


                tx.Commit();
            }
            catch (MySqlException ex)
            {
                ret += "Error: " + ex.ToString();
                tx.Rollback();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            return ret;
        }

        public string InitialItemClassifIns()
        {
            //string retStr = "ok";

            //CommittableTransaction tx = new CommittableTransaction();

            Int64 allCountUKM = 0;
            int allCountSaalut = 0;

            var countClassif = (from g in context.Groups
                                select g.Version_UKM).Sum();
            if (countClassif != null)
                allCountSaalut = countClassif.Value;

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                string cmdText = "select  count(version)	from ukmserver.trm_in_classif ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                DataTable countVerClassif = ds.Tables[0];

                if (countVerClassif != null)
                {
                    foreach (DataRow row in countVerClassif.Rows)
                    {
                        allCountUKM = (Int64)row[0];
                    }
                }

                // проверим изменение шаблонов select 	classif_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_classif 
                Int64 allTemplateUKM = 0;
                int allCountSaalutTempl = 0;

                var countTempls = (from g in context.Groups
                                   select g.Version_Template_UKM).Sum();
                if (countTempls != null)
                    allCountSaalutTempl = countTempls.Value;

                cmdText = "select 	count(version) from ukmserver.srv_pricetags_classif  ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds20 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds20);

                DataTable printTemplatesGroup = ds20.Tables[0];

                if (printTemplatesGroup != null)
                {
                    foreach (DataRow row in countVerClassif.Rows)
                    {
                        allTemplateUKM = (Int64)row[0];
                    }
                }


                // обработки при измении таблиц
                if (allCountSaalut != allCountUKM || allCountSaalutTempl != allTemplateUKM)
                {

                    //+ шаблоны
                    cmdText = "select 	classif_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_classif  ";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds21 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds21);

                    DataTable printTemplGood = ds21.Tables[0];
                    //- шаблоны

                    cmdText = "select  id, owner, name, version, deleted	from ukmserver.trm_in_classif order by owner asc ";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds2 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds2);

                    DataTable dt = ds2.Tables[0];

                    Dictionary<string, int> groupsSaalut = new Dictionary<string, int>();

                    var groups = from g in context.Groups
                                 select g;

                    foreach (Group group in groups)
                    {
                        groupsSaalut.Add(group.GroupNum, group.Version_UKM.Value);
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        // Читаем УКМ ищем группу, не находим - создаем, находим - проверяем версию, не сходится - обновляем все поля.

                        string id = row[0].ToString();
                        string owner = row[1].ToString();
                        string name = row[2].ToString();
                        int version = Int32.Parse(row[3].ToString());
                        bool deleted = (bool)row[4];

                        // Ценник
                        int pricetag_id = 0;
                        int version_printtempl = 0;
                        bool deleted_printtempl = false;
                        if (printTemplGood != null)
                        {
                            // select classif_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_classif 
                            DataRow[] strs = printTemplGood.Select("classif_id = '" + id + "'");
                            foreach (DataRow rowTmpl in strs)
                            {
                                pricetag_id = Int32.Parse(rowTmpl[1].ToString());
                                version_printtempl = Int32.Parse(rowTmpl[2].ToString());
                                deleted_printtempl = (bool)rowTmpl[3];
                            }
                        }
                        // ценник


                        int value = 0;
                        if (groupsSaalut.TryGetValue(id, out value))
                        {
                            // группа существует, проверяем наличие изменений
                            if (value != version)
                            {
                                // есть изменения.
                                var grp = (from g in context.Groups
                                           where g.GroupNum == id
                                           select g).FirstOrDefault();
                                grp.GroupName = name;
                                grp.Active = !deleted;
                                grp.TimeStamp = DateTime.Now;

                                if (owner != "0")
                                {
                                    var ownGrp = (from g in context.Groups
                                                  where g.GroupNum == owner
                                                  select g).FirstOrDefault();
                                    if (ownGrp == null) // если группу не нашли. оставляем для изменения
                                        grp.Version_UKM = 0;
                                    else
                                    {
                                        grp.GroupRangeID = ownGrp.ID;
                                        grp.Version_UKM = version;
                                    }
                                }
                                else
                                    grp.Version_UKM = version;

                                // шаблон
                                var ptm = (from p in context.PrintTemplates
                                           where p.UKM_ID == pricetag_id
                                           select p).FirstOrDefault();
                                if (ptm != null)
                                {
                                    if (deleted_printtempl)
                                    {
                                        grp.PrintTemplates = null;
                                        grp.Version_Template_UKM = version_printtempl;
                                    }
                                    else
                                    {
                                        grp.PrintTemplates = ptm;
                                        grp.Version_Template_UKM = version_printtempl;
                                    }
                                }
                                else
                                {
                                    grp.PrintTemplateID = null;
                                    grp.Version_Template_UKM = 0;
                                }


                                context.SubmitChanges();
                            }
                        }
                        else
                        {
                            // группа отсутствует - добавляем
                            Group ng = new Group();
                            ng.GroupNum = id;
                            ng.GroupName = name;
                            ng.Active = !deleted;
                            ng.TimeStamp = DateTime.Now;

                            if (owner == "0")
                            {
                                ng.Version_UKM = version;
                            }
                            else
                            {
                                var ownGrp = (from g in context.Groups
                                              where g.GroupNum == owner
                                              select g).FirstOrDefault();
                                if (ownGrp == null) // если группу не нашли. оставляем для изменения
                                    ng.Version_UKM = 0;
                                else
                                {
                                    ng.GroupRangeID = ownGrp.ID;
                                    ng.Version_UKM = version;
                                }
                            }

                            // шаблон
                            var ptm = (from p in context.PrintTemplates
                                       where p.UKM_ID == pricetag_id
                                       select p).FirstOrDefault();
                            if (ptm != null)
                            {
                                if (deleted_printtempl)
                                {
                                    ng.PrintTemplates = null;
                                    ng.Version_Template_UKM = version_printtempl;
                                }
                                else
                                {
                                    ng.PrintTemplates = ptm;
                                    ng.Version_Template_UKM = version_printtempl;
                                }
                            }
                            else
                            {
                                ng.PrintTemplateID = null;
                                ng.Version_Template_UKM = 0;
                            }

                            context.Groups.InsertOnSubmit(ng);
                            context.SubmitChanges();
                        }
                    }


                }//if(countClassif != allCountUKM)
                //else
                //    retStr += "Классификатор не изменялся. ";


                //tx.Commit();
            }
            catch (MySqlException ex)
            {
                //retStr += "Error: " + ex.ToString();
                //tx.Rollback();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            return "";// retStr;
        }

        public string InitialDepartsIns()
        {
            string ret = "";

            CommittableTransaction tx = new CommittableTransaction();

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return ret = "Нет активного магазина в Saalut. ";

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // Выбираем магазин
                string cmdText = "select  id, name, version, deleted	from 	ukmserver.srv_assortment_groups where store_id = '" + store.StoreID_UKM.ToString() + "' ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                int id = 0;
                string name = "";
                int version = 0;
                bool deleted = false;

                if (ds.Tables.Count != 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        id = (int)row[0];
                        name = row[1].ToString();
                        version = (int)row[2];
                        deleted = (bool)row[3];

                        // 
                        var assrtGrp = (from a in context.Departments
                                        where a.DepartNum_UKM == id
                                        select a).FirstOrDefault();
                        if (assrtGrp == null)
                        {
                            Department nd = new Department();
                            nd.DepartNum_UKM = id;
                            nd.DepartName = name;
                            nd.Version_UKM = version;
                            nd.Active = !deleted;
                            nd.TimeStamp = DateTime.Now;
                            context.Departments.InsertOnSubmit(nd);
                            context.SubmitChanges();
                        }
                        else
                        {
                            if (version != assrtGrp.Version_UKM)
                            {
                                assrtGrp.DepartName = name;
                                assrtGrp.Active = !deleted;
                                assrtGrp.TimeStamp = DateTime.Now;
                                context.SubmitChanges();
                            }
                        }
                    }
                }

                tx.Commit();
            }
            catch (MySqlException ex)
            {
                ret += "Error: " + ex.ToString();
                tx.Rollback();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            return ret;
        }


        // Работаем с журналом изменения цен УКМ
        public string ReadNewData_from_client()
        {
            string ret = "";

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            // + Инициализация магазина
            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                ret += "Инициализация магазина: " + InitialStoreInfoIns() + ". ";
            else
                ret += "Магазин инициализирован ранее. ";
            // - Инициализация магазина

            // + Инициализация классификатора
            //     ret += "Инициализация классификатора: " + InitialItemClassifIns();
            // - Инициализация классификатора

            // + Инициализация отделов
            //      ret += "Инициализация отделов: " + InitialDepartsIns();
            // - Инициализация отделов

            //ret += "Обновление товаров: " + UpdateGoods() + ". ";

            ret += "Новые цены: " + ReadPricesJournal_new_w_del_j() + ". ";

            //ret += "Обновление журналов: " + UpdatePricesJournals() + ". ";

            return ret;
        }


        public string ReadNewData()
        {
            string ret = "";

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            // + Инициализация магазина
            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                ret += "Инициализация магазина: " + InitialStoreInfoIns() + ". ";
            else
                ret += "Магазин инициализирован ранее. ";
            // - Инициализация магазина

            // + Инициализация классификатора
            ret += "Инициализация классификатора: " + InitialItemClassifIns();
            // - Инициализация классификатора

            // + Инициализация отделов
            ret += "Инициализация отделов: " + InitialDepartsIns();
            // - Инициализация отделов

            //ret += "Обновление товаров: " + UpdateGoods() + ". ";

            ret += "Новые цены: " + ReadPricesJournal_new() + ". ";

            //ret += "Обновление журналов: " + UpdatePricesJournals() + ". ";

            return ret;
        }

        public string ReadPricesJournal()
        {
            string ret = "";

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return ret = "Нет активного магазина в Saalut. ";

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // Выбираем магазин
                string cmdText = "select change_log_id from ukmserver.local_data_change_log_status where store_id = '" + store.StoreID_UKM.ToString() + "' and (status = 0 or status = 1)";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                if (ds.Tables.Count != 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        int journalID = 0;

                        int change_log_id = Int32.Parse(row[0].ToString());
                        int order_no = 0;

                        cmdText = "select order_no from ukmserver.local_data_change_log where store_id = '" + store.StoreID_UKM.ToString() + "' and id = '" + change_log_id.ToString() + "' ";
                        cmd = new MySqlCommand(cmdText, cnx);
                        cmd.CommandTimeout = 30000;

                        // Create a fill a Dataset
                        DataSet dsO = new DataSet();
                        adapter.SelectCommand = cmd;
                        adapter.Fill(dsO);

                        if (dsO.Tables.Count != 0)
                        {
                            foreach (DataRow row1 in dsO.Tables[0].Rows)
                            {
                                order_no = Int32.Parse(row1[0].ToString());
                            }
                        }


                        var journ = (from j in context.PriceChangeJours
                                     where j.Order_no == order_no
                                     select j).FirstOrDefault();
                        if (journ == null)
                        {
                            cmdText = "select sum(new_value) from ukmserver.local_data_change_log_entity_prices where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' and pricelist_name = '" + store.PriceListName_UKM + "'";
                            cmd = new MySqlCommand(cmdText, cnx);
                            cmd.CommandTimeout = 30000;

                            // Create a fill a Dataset
                            DataSet dsT = new DataSet();
                            adapter.SelectCommand = cmd;
                            adapter.Fill(dsT);

                            //Double countNum = 0;
                            //if (dsT.Tables.Count != 0)
                            //{
                            //    foreach (DataRow rowT in dsT.Tables[0].Rows)
                            //    {
                            //        Double.TryParse(rowT[0].ToString(), out countNum);
                            //    }
                            //}
                            //if (countNum != 0)
                            journalID = AddPricesNewJournal(change_log_id, order_no);
                            //else
                            //    continue;

                            cmdText = "select number, item_id from ukmserver.local_data_change_log_entities where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' ";
                            cmd = new MySqlCommand(cmdText, cnx);
                            cmd.CommandTimeout = 30000;

                            // Create a fill a Dataset
                            DataSet ds2 = new DataSet();
                            adapter.SelectCommand = cmd;
                            adapter.Fill(ds2);

                            if (ds2.Tables.Count != 0)
                            {
                                foreach (DataRow row2 in ds2.Tables[0].Rows)
                                {
                                    int number = Int32.Parse(row2[0].ToString());
                                    string item_id = row2[1].ToString();

                                    cmdText = "select new_value from ukmserver.local_data_change_log_entity_prices where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' and number = '" + number.ToString() + "' and pricelist_name = '" + store.PriceListName_UKM + "'";
                                    cmd = new MySqlCommand(cmdText, cnx);
                                    cmd.CommandTimeout = 30000;

                                    // Create a fill a Dataset
                                    DataSet ds3 = new DataSet();
                                    adapter.SelectCommand = cmd;
                                    adapter.Fill(ds3);

                                    if (ds3.Tables.Count != 0)
                                    {
                                        string new_value = "0";
                                        foreach (DataRow row3 in ds3.Tables[0].Rows)
                                        {
                                            new_value = row3[0].ToString();
                                        }
                                        string strRet = AddPricesNewLine(journalID, item_id, new_value);
                                        ret += strRet;
                                    }
                                    else
                                    {
                                        string strRet = AddPricesNewLine(journalID, item_id, "0");
                                        ret += strRet;
                                    }
                                    //----
                                }
                            }
                            //----
                        }//if (journ == null)
                        else
                        {
                            //continue;
                            // обновляем журнал

                            journ.Active = true;
                            context.SubmitChanges();
                            // если был удален



                            journalID = journ.ID;

                            cmdText = "select number, item_id from ukmserver.local_data_change_log_entities where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' ";
                            cmd = new MySqlCommand(cmdText, cnx);
                            cmd.CommandTimeout = 30000;

                            // Create a fill a Dataset
                            DataSet ds2 = new DataSet();
                            adapter.SelectCommand = cmd;
                            adapter.Fill(ds2);

                            if (ds2.Tables.Count != 0)
                            {
                                foreach (DataRow row2 in ds2.Tables[0].Rows)
                                {
                                    int number = Int32.Parse(row2[0].ToString());
                                    string item_id = row2[1].ToString();

                                    var jourLine = from j in context.PriceChangeLine
                                                   where j.JournalID == journalID
                                                   && j.ItemID_UKM == item_id
                                                   select j;
                                    context.PriceChangeLine.DeleteAllOnSubmit(jourLine);
                                    context.SubmitChanges();

                                    cmdText = "select new_value from ukmserver.local_data_change_log_entity_prices where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' and number = '" + number.ToString() + "' and pricelist_name = '" + store.PriceListName_UKM + "'";
                                    cmd = new MySqlCommand(cmdText, cnx);
                                    cmd.CommandTimeout = 30000;

                                    // Create a fill a Dataset
                                    DataSet ds3 = new DataSet();
                                    adapter.SelectCommand = cmd;
                                    adapter.Fill(ds3);

                                    if (ds3.Tables.Count != 0)
                                    {
                                        string new_value = "0";
                                        foreach (DataRow row3 in ds3.Tables[0].Rows)
                                        {
                                            new_value = row3[0].ToString();
                                        }
                                        string strRet = AddPricesNewLine(journalID, item_id, new_value);
                                        ret += strRet;
                                    }
                                    else
                                    {
                                        string strRet = AddPricesNewLine(journalID, item_id, "0");
                                        ret += strRet;
                                    }
                                    //----
                                }
                            }

                        }//if (journ == null)



                    }
                }
                else
                    ret += "УКМ Журнал цен - пусто (local_data_change_log_status). ";
            }
            catch (MySqlException ex)
            {
                ret += "Error: " + ex.ToString();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            if (ret == "")
                ret = "пусто";

            return ret;
        }

        public int AddPricesNewJournal(int change_log_id, int order_no)
        {
            int ret = 0;

            CommittableTransaction tx = new CommittableTransaction();
            try
            {

                var journal = (from j in context.PriceChangeJours
                               where j.Order_no == order_no
                               select j).FirstOrDefault();

                if (journal == null)
                {
                    PriceChangeJour n = new PriceChangeJour();
                    n.Change_log_id_UKM = change_log_id;
                    n.Order_no = order_no;
                    n.TimeStamp = DateTime.Now;
                    n.InUse = false; // не будем использовать пока что
                    n.Active = true;
                    context.PriceChangeJours.InsertOnSubmit(n);
                    context.SubmitChanges();

                    //----

                    var jour2 = (from j in context.PriceChangeJours
                                 where j.Order_no == order_no
                                 select j).FirstOrDefault();

                    ret = jour2.ID;
                }
                else
                    ret = journal.ID;

                tx.Commit();


            }
            catch (Exception ex)
            {
                tx.Rollback();
            }

            return ret;
        }

        public string AddPricesNewLine(int journalID, string item_id, string new_value)
        {
            string ret = "";

            CommittableTransaction tx = new CommittableTransaction();
            try
            {

                var good = (from g in context.Goods
                            where g.Articul == item_id
                            select g).FirstOrDefault();

                if (good == null)
                {
                    // номенклатуры нету

                    AddNewGood(item_id);

                    ret += "Товар " + item_id + " - нет в справочнике товаров! Журнал цен: " + journalID + ". ";

                    PriceChangeLine nl = new PriceChangeLine();
                    nl.GoodID = null; // товара нет будем потом его искать
                    nl.ItemID_UKM = item_id;
                    nl.JournalID = journalID;

                    double price = 0;
                    string new_value2 = new_value.Replace(".", ",");
                    Double.TryParse(new_value2, out price);

                    nl.NewPrice = price;
                    nl.TimeStamp = DateTime.Now;
                    nl.Active = false;
                    context.PriceChangeLine.InsertOnSubmit(nl);
                    context.SubmitChanges();

                    if (price == 0)
                        ret += "Товар " + item_id + " цена 0! Журнал цен: " + journalID + ". ";

                    // догрузим его при обновлении журнала
                }
                else
                {

                    UpdateGood(good, item_id);

                    PriceChangeLine nl = new PriceChangeLine();
                    nl.GoodID = good.ID;
                    nl.ItemID_UKM = item_id;
                    nl.JournalID = journalID;

                    double price = 0;
                    string new_value2 = new_value.Replace(".", ",");
                    Double.TryParse(new_value2, out price);

                    nl.NewPrice = price;
                    nl.TimeStamp = DateTime.Now;
                    nl.Active = true;
                    context.PriceChangeLine.InsertOnSubmit(nl);
                    context.SubmitChanges();

                    if (price == 0)
                        ret += "Товар " + item_id + " цена 0! Журнал цен: " + journalID + ". ";

                }

                tx.Commit();


            }
            catch (Exception ex)
            {
                tx.Rollback();
            }

            return ret;
        }

        // Догрузить журналы, где не были загружены товары.
        public string UpdatePricesJournals()
        {
            string ret = "";

            CommittableTransaction tx = new CommittableTransaction();
            try
            {

                var jours = (from j in context.PriceChangeLine
                             where j.PriceChangeJour.Active == true // только активные
                             && j.GoodID == null
                             group j by j.JournalID into g
                             select new
                             {
                                 journaln = g.Key
                             }).ToList();

                foreach (var js in jours)
                {
                    ret += UpdatePricesJournGoods(js.journaln.Value);
                }

                tx.Commit();

            }
            catch (Exception ex)
            {
                tx.Rollback();
            }

            return ret;
        }

        public string UpdatePricesJournGoods(int journalID)
        {
            string ret = "";

            CommittableTransaction tx = new CommittableTransaction();
            try
            {

                bool update = false;

                var journal = (from j in context.PriceChangeJours
                               where j.ID == journalID
                               select j).FirstOrDefault();

                if (journal == null)
                    return ret = "Журнал " + journalID.ToString() + " в Saalut не найден. ";


                var lines = from l in context.PriceChangeLine
                            where l.JournalID == journal.ID
                            && l.GoodID == null
                            select l;
                foreach (PriceChangeLine line in lines)
                {
                    var good = (from g in context.Goods
                                where g.Articul == line.ItemID_UKM
                                select g).FirstOrDefault();
                    if (good != null)
                    {
                        line.GoodID = good.ID;
                        UpdateGood(good, line.ItemID_UKM);
                        update = true;
                    }
                    else
                    {
                        int goodID = AddNewGood(line.ItemID_UKM);

                        if (goodID == 0)
                            // опять не нашли товар
                            ret += "Нет товара в справочнике Saalut. " + line.ItemID_UKM + ". ";
                        else
                        {
                            line.GoodID = goodID;
                            update = true;
                        }
                    }
                }

                if (update)
                    context.SubmitChanges();

                tx.Commit();


            }
            catch (Exception ex)
            {
                tx.Rollback();
            }

            return ret;
        }


        // Номенклатурный справочник, цены

        public int AddNewGood(string item_id)
        {

            int ret = 0;

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return ret;


            string id = "";
            DataRow item = null;

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                string cmdText = "select 	id, name, descr, measure, classif, version, deleted	 from ukmserver.trm_in_items where id = '" + item_id + "' ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                if (ds.Tables.Count != 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        id = row[0].ToString();
                        item = row;
                    }
                }

                if (id != "")
                {
                    // Barcode
                    cmdText = "select id, item, version, deleted from ukmserver.trm_in_var where item = '" + id + "' and deleted = 0 ";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds2 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds2);

                    DataTable barcodes = ds2.Tables[0];


                    cmdText = "select ag_id, var, plu, exp_date1, exp_date2, version, deleted from ukmserver.srv_assortment_group_items where store_id = '" + store.StoreID_UKM.ToString() + "' and deleted = 0 ";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds3 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds3);

                    DataTable assorts = ds3.Tables[0];


                    // Состав
                    cmdText = "select id, country, structure, version, deleted, producer_marking from ukmserver.trm_in_item_cc where id = '" + id + "' and deleted = 0";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds4 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds4);

                    DataTable structures = ds4.Tables[0];


                    cmdText = "select item, price, version, deleted from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and item = '" + id + "' and deleted = 0 ";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds5 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds5);

                    DataTable prices = ds5.Tables[0];

                    // PrintTemplate good
                    cmdText = "select item_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_item where item_id = '" + id + "' ";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds6 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds6);

                    DataTable printTemplGood = ds6.Tables[0];


                    CreateNewGood(item, barcodes, assorts, structures, prices, printTemplGood);

                }
            }
            catch (MySqlException ex)
            {
                ret = 0;
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            return ret;
        }

        public int UpdateGood(Good good, string item_id)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            int ret = 0;

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return ret;


            string id = "";
            DataRow item = null;

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                string cmdText = "select 	id, name, descr, measure, classif, version, deleted	 from ukmserver.trm_in_items where id = '" + item_id + "' ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                if (ds.Tables.Count != 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        id = row[0].ToString();
                        item = row;
                    }
                }

                if (id != "")
                {
                    // Barcode
                    cmdText = "select id, item, version, deleted from ukmserver.trm_in_var where item = '" + id + "' and deleted = 0 ";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds2 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds2);

                    DataTable barcodes = ds2.Tables[0];


                    cmdText = "select ag_id, var, plu, exp_date1, exp_date2, version, deleted from ukmserver.srv_assortment_group_items where store_id = '" + store.StoreID_UKM.ToString() + "' and deleted = 0 ";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds3 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds3);

                    DataTable assorts = ds3.Tables[0];


                    // Состав
                    cmdText = "select id, country, structure, version, deleted, producer_marking from ukmserver.trm_in_item_cc where id = '" + id + "' and deleted = 0";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds4 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds4);

                    DataTable structures = ds4.Tables[0];


                    cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and item = '" + id + "' and deleted = 0 ";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds5 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds5);

                    DataTable prices = ds5.Tables[0];

                    // PrintTemplate good
                    cmdText = "select item_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_item where item_id = '" + id + "' ";
                    cmd = new MySqlCommand(cmdText, cnx);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds6 = new DataSet();
                    adapter.SelectCommand = cmd;
                    adapter.Fill(ds6);

                    DataTable printTemplGood = ds6.Tables[0];


                    UpdateGood(good, item, barcodes, assorts, structures, prices, printTemplGood);

                }
            }
            catch (MySqlException ex)
            {
                ret = 0;
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            return ret;
        }

        // все товары
        public string UpdateGoods()
        {
            string ret = "";

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return ret;

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                MySqlCommand cmd = new MySqlCommand("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ;", cnx);
                cnx.Open();
                cmd.ExecuteNonQuery();


                string cmdText = "select sum(version) from ukmserver.trm_in_items ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;



                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                DataTable itemSum = ds.Tables[0];
                decimal counter = 0;

                if (itemSum != null)
                {
                    foreach (DataRow row in itemSum.Rows)
                    {
                        counter = (decimal)row[0];
                    }
                }


                // удалено изза переполнения INT
                //if (counter != 0)
                //{
                //    var countGoods = (from g in context.Goods
                //                      select g.Version_UKM).Sum();
                //    if (countGoods != null)
                //    {
                //        if (countGoods.Value == counter)
                //            ret += "Изменения отсутствуют. ";//return ret += "Изменения отсутствуют. ";
                //    }
                //}

                //------------------------------------

                var goods = from g in context.Goods
                            select g;

                // items
                cmdText = "select id, name, descr, measure, classif, version, deleted from ukmserver.trm_in_items ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds1 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds1);

                DataTable items = ds1.Tables[0];



                // Barcode
                cmdText = "select id, item, version, deleted from ukmserver.trm_in_var ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds2 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds2);

                DataTable barcodes = ds2.Tables[0];


                // Assortment group items
                cmdText = "select ag_id, var, plu, exp_date1, exp_date2, version, deleted from ukmserver.srv_assortment_group_items where store_id = '" + store.StoreID_UKM.ToString() + "' and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds3 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds3);

                DataTable assorts = ds3.Tables[0];


                // Structure
                cmdText = "select id, country, structure, version, deleted, producer_marking from ukmserver.trm_in_item_cc ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds4 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds4);

                DataTable structures = ds4.Tables[0];


                // Prices
                cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds5 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds5);

                DataTable prices = ds5.Tables[0];


                // PrintTemplate good
                cmdText = "select item_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_item ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds6 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds6);

                DataTable printTemplGood = ds6.Tables[0];



                foreach (DataRow item in items.Rows)
                {
                    string id = item[0].ToString();

                    var good = (from g in goods
                                where g.Articul == id
                                select g).FirstOrDefault();
                    if (good == null)
                    {
                        // товара нету - создаем
                        CreateNewGood(item, barcodes, assorts, structures, prices, printTemplGood);
                    }
                    else
                    {
                        UpdateGood(good, item, barcodes, assorts, structures, prices, printTemplGood);
                    }
                }

                cmd = new MySqlCommand("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ;", cnx);
                cmd.ExecuteNonQuery();

            }
            catch (MySqlException ex)
            {
                ret += "Error: " + ex.ToString();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            return ret;
        }

        // пришло время оптимизировать загрузку материалов
        // все товары
        public string UpdateGoodsOptimized()
        {
            string ret = "";

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return ret;

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                MySqlCommand cmd = new MySqlCommand("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ;", cnx);
                cnx.Open();
                cmd.ExecuteNonQuery();


                // items
                string cmdText = "select id, name, descr, measure, classif, version, deleted from ukmserver.trm_in_items ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds1 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds1);

                DataTable items = ds1.Tables[0];

                // Barcode
                cmdText = "select id, item, version, deleted from ukmserver.trm_in_var ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds2 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds2);

                DataTable barcodes = ds2.Tables[0];


                // Assortment group items
                cmdText = "select ag_id, var, plu, exp_date1, exp_date2, version, deleted from ukmserver.srv_assortment_group_items where store_id = '" + store.StoreID_UKM.ToString() + "' and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds3 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds3);

                DataTable assorts = ds3.Tables[0];


                // Structure
                cmdText = "select id, country, structure, version, deleted, producer_marking from ukmserver.trm_in_item_cc ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds4 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds4);

                DataTable structures = ds4.Tables[0];


                // Prices
                cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds5 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds5);

                DataTable prices = ds5.Tables[0];


                // PrintTemplate good
                cmdText = "select item_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_item ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds6 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds6);

                DataTable printTemplGood = ds6.Tables[0];


                cmd = new MySqlCommand("SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ;", cnx);
                cmd.ExecuteNonQuery();


                //+ версия укм
                Dictionary<string, int> goodsInDB = new Dictionary<string, int>();

                var goods1 = from g in context.Goods
                             orderby g.PLU descending
                             select g;

                foreach (Good row in goods1)
                {
                    int versionInDB = 0;
                    if (!goodsInDB.TryGetValue(row.Articul, out versionInDB))
                    {
                        goodsInDB.Add(row.Articul, row.Version_UKM.Value);
                    }
                }


                //-
                int obnovl = 0;
                int dobavl = 0;
                foreach (DataRow item in items.Rows)
                {
                    string artikul = item[0].ToString();
                    int version_good = (int)item[5];

                    //select id, name, descr, measure, classif, version, deleted from ukmserver.trm_in_items
                    int versionInDB = 0;
                    if (!goodsInDB.TryGetValue(artikul, out versionInDB))
                    {
                        CreateNewGood(item, barcodes, assorts, structures, prices, printTemplGood);
                        dobavl++;
                    }
                    else
                    {
                        if (versionInDB != version_good)
                        {
                            var good = (from g in context.Goods
                                        where g.Articul == artikul
                                        select g).FirstOrDefault();

                            UpdateGood(good, item, barcodes, assorts, structures, prices, printTemplGood);
                            obnovl++;
                        }
                    }
                }
                ret += " Обновлено: " + obnovl.ToString() + ", добавлено: " + dobavl.ToString() + " товаров. ";
            }
            catch (MySqlException ex)
            {
                ret += "Error: " + ex.ToString();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            return ret;
        }



        public void CreateNewGood(DataRow item, DataTable barcodes, DataTable assortments, DataTable structures, DataTable prices, DataTable printTemplGood)
        {
            CommittableTransaction tx = new CommittableTransaction();
            try
            {

                string id = item[0].ToString();
                string name = item[1].ToString();
                string descr = item[2].ToString();
                string measure = item[3].ToString();
                string classif = item[4].ToString();
                int version_good = (int)item[5];
                bool deleted = (bool)item[6];


                string firstBarcode = "";
                string barcode = "";

                bool assortmentFinded = false;


                int ag_id = 0;
                int plu = 0;
                DateTime exp_date1 = DateTime.MinValue;
                int exp_date2 = 0;
                int version_assort = 0;

                // Barcode
                if (barcodes != null)
                {
                    // select id, item, version, deleted from ukmserver.trm_in_var
                    DataRow[] bcs = barcodes.Select("item = '" + id + "'");

                    foreach (DataRow row in bcs)
                    {
                        barcode = row[0].ToString();

                        if (firstBarcode == "")
                            firstBarcode = barcode;

                        if (assortments != null)
                        {
                            // select ag_id, var, plu, exp_date1, exp_date2, version, deleted from ukmserver.srv_assortment_group_items where store_id = '" + store.StoreID_UKM.ToString() + "'
                            DataRow[] asrts = assortments.Select("var = '" + barcode + "'");

                            foreach (DataRow row2 in asrts)
                            {
                                ag_id = (int)row2[0];
                                plu = Int32.Parse(row2[2].ToString());

                                DateTime.TryParse(row2[3].ToString(), out exp_date1);

                                if (exp_date2 == 0)
                                    exp_date2 = (int)row2[4];
                                version_assort = (int)row2[5];

                                assortmentFinded = true;
                            }
                        }

                        if (assortmentFinded)
                            break;
                    }
                }

                // Состав
                string country = "";
                string structure = "";
                int version_contents = 0;
                string producer = "";

                if (structures != null)
                {
                    // select id, country, structure, version, deleted, producer_marking	 from ukmserver.trm_in_item_cc
                    DataRow[] strs = structures.Select("id = '" + id + "'");
                    foreach (DataRow row in strs)
                    {
                        country = row[1].ToString();
                        structure = row[2].ToString();
                        version_contents = (int)row[3];
                        producer = row[5].ToString();
                    }
                }

                // Ценник
                int pricetag_id = 0;
                int version_printtempl = 0;
                bool deleted_printtempl = false;
                if (printTemplGood != null)
                {
                    // select item_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_item
                    DataRow[] strs = printTemplGood.Select("item_id = '" + id + "'");
                    foreach (DataRow row in strs)
                    {
                        pricetag_id = Int32.Parse(row[1].ToString());
                        version_printtempl = Int32.Parse(row[2].ToString());
                        deleted_printtempl = (bool)row[3];
                    }
                }


                Good ng = new Good();
                ng.Articul = id;
                ng.Name = name;
                ng.Descr = descr;
                ng.PLU = plu;
                ng.Exp_Date = null;
                ng.Exp_Qty = exp_date2;

                if (assortmentFinded)
                    ng.Barcode = barcode;
                else
                    ng.Barcode = firstBarcode;
                ng.Edinic = measure;

                if (ag_id != 0)
                {
                    var dept = (from d in context.Departments
                                where d.DepartNum_UKM == ag_id
                                select d).FirstOrDefault();
                    if (dept != null)
                        ng.Department = dept;
                }
                else
                    ng.DepartmentID = null;

                var group = (from g in context.Groups
                             where g.GroupNum == classif
                             select g).FirstOrDefault();
                if (group != null)
                {
                    ng.Group = group;
                    ng.Version_UKM = version_good;
                }
                else
                {
                    ng.GroupID = null;
                    ng.Version_UKM = 0;
                }

                //if (pricetag_id != 0)
                //{
                //    var prt = (from p in context.PrintTemplates
                //               where p.UKM_ID == pricetag_id
                //               select p).FirstOrDefault();
                //    if (prt != null)
                //    {
                //        if (deleted_printtempl)
                //            ng.PrintTemplateID = null;
                //        else
                //            ng.PrintTemplates = prt;

                //        ng.Version_PrintTemplate_UKM = version_printtempl;
                //    }
                //    else
                //    {
                //        ng.Version_PrintTemplate_UKM = 0;
                //    }
                //}
                //else
                //{
                //    ng.PrintTemplateID = null;
                //    ng.Version_PrintTemplate_UKM = version_printtempl;
                //}


                ng.TimeStamp = DateTime.Now;
                ng.Active = !deleted;
                ng.Version_Assort_UKM = version_assort;
                ng.Country = country;
                ng.Contents = structure;
                ng.Producer = producer;
                ng.Version_Contenst_UKM = version_contents;

                context.Goods.InsertOnSubmit(ng);
                context.SubmitChanges();


                // Добавим цену, если есть
                decimal price = 0;
                int version_price = 0;

                if (prices != null)
                {
                    // select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'
                    DataRow[] prsLsts = prices.Select("item = '" + id + "'");

                    foreach (DataRow row in prsLsts)
                    {
                        price = (decimal)row[1];
                        version_price = (int)row[2];
                    }
                }

                var gdN = (from g in context.Goods
                           where g.Articul == id
                           select g).FirstOrDefault();

                if (version_price != 0)
                {
                    Price np = new Price();
                    np.GoodID = gdN.ID;
                    np.Price1 = Double.Parse(price.ToString());
                    np.Version_UKM = version_price;
                    np.TimeStamp = DateTime.Now;
                    np.Active = true;
                    context.Prices.InsertOnSubmit(np);
                    context.SubmitChanges();
                }

                // Barcodes
                if (barcodes != null)
                {
                    // select id, item, version, deleted from ukmserver.trm_in_var
                    DataRow[] bcs = barcodes.Select("item = '" + id + "'");

                    foreach (DataRow row in bcs)
                    {
                        barcode = row[0].ToString();
                        int version_barcode = (int)row[2];
                        bool deleted_barcode = (bool)row[3];

                        // bug fix del barcodes other good
                        var barkOld = from b in context.Barcodes
                                      where b.Barcode1 == barcode
                                      && b.Active == true
                                      select b;
                        foreach (Barcode bc in barkOld)
                        {
                            bc.Active = false;
                        }
                        //-

                        Barcode nb = new Barcode();
                        nb.GoodID = gdN.ID;
                        nb.Barcode1 = barcode;
                        nb.Version_UKM = version_barcode;
                        nb.TimeStamp = DateTime.Now;
                        nb.Active = !deleted_barcode;
                        context.Barcodes.InsertOnSubmit(nb);
                        context.SubmitChanges();
                    }
                }

                tx.Commit();


            }
            catch (Exception ex)
            {
                tx.Rollback();
            }
        }

        public void CreateNewGoods(DataTable items, DataTable barcodes, DataTable assortments, DataTable structures, DataTable prices, DataTable printTemplGood)
        {
            CommittableTransaction tx = new CommittableTransaction();
            try
            {
                foreach (DataRow item in items.Rows)
                {
                    CreateNewGood(item, barcodes, assortments, structures, prices, printTemplGood);
                }
            }
            catch (Exception ex)
            {
                tx.Rollback();
            }
        }


        public void UpdateGood(Good good, DataRow item, DataTable barcodes, DataTable assortments, DataTable structures, DataTable prices, DataTable printTemplGood)
        {
            CommittableTransaction tx = new CommittableTransaction();
            try
            {

                // проверяем наличие изменений по товару и связанным таблицам, используем версию

                string id = item[0].ToString();
                string name = item[1].ToString();
                string descr = item[2].ToString();
                string measure = item[3].ToString();
                string classif = item[4].ToString();
                int version_good = (int)item[5];
                bool deleted = (bool)item[6];


                string firstBarcode = "";
                string barcode = "";

                bool assortmentFinded = false;


                int ag_id = 0;
                int plu = 0;
                DateTime exp_date1 = DateTime.MinValue;
                int exp_date2 = 0;
                int version_assort = 0;

                int barcodes_ver_sum = 0;

                // Barcode
                if (barcodes != null)
                {
                    // select id, item, version, deleted from ukmserver.trm_in_var
                    DataRow[] bcs = barcodes.Select("item = '" + id + "'");

                    foreach (DataRow row in bcs)
                    {
                        barcode = row[0].ToString();
                        barcodes_ver_sum += (int)row[2];

                        if (firstBarcode == "")
                            firstBarcode = barcode;

                        if (assortments != null)
                        {
                            // select ag_id, var, plu, exp_date1, exp_date2, version, deleted from ukmserver.srv_assortment_group_items where store_id = '" + store.StoreID_UKM.ToString() + "'
                            DataRow[] asrts = assortments.Select("var = '" + barcode + "'");

                            foreach (DataRow row2 in asrts)
                            {
                                ag_id = (int)row2[0];
                                plu = Int32.Parse(row2[2].ToString());

                                DateTime.TryParse(row2[3].ToString(), out exp_date1);

                                if(exp_date2 == 0)
                                    exp_date2 = (int)row2[4];
                                version_assort = (int)row2[5];

                                assortmentFinded = true;
                            }
                        }

                        if (assortmentFinded)
                            break;
                    }
                }

                // Состав
                string country = "";
                string structure = "";
                int version_contents = 0;
                string producer = "";

                if (structures != null)
                {
                    // select id, country, structure, version, deleted, producer_marking	 from ukmserver.trm_in_item_cc
                    DataRow[] strs = structures.Select("id = '" + id + "'");
                    foreach (DataRow row in strs)
                    {
                        country = row[1].ToString();
                        structure = row[2].ToString();
                        version_contents = (int)row[3];
                        producer = row[5].ToString();
                    }
                }

                // Ценник
                int pricetag_id = 0;
                int version_printtempl = 0;
                bool deleted_printtempl = false;
                if (printTemplGood != null)
                {
                    // select item_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_item
                    DataRow[] strs = printTemplGood.Select("item_id = '" + id + "'");
                    foreach (DataRow row in strs)
                    {
                        pricetag_id = Int32.Parse(row[1].ToString());
                        version_printtempl = Int32.Parse(row[2].ToString());
                        deleted_printtempl = (bool)row[3];
                    }
                }

                decimal price = 0;
                int version_price = 0;
                if (prices != null)
                {
                    // select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and deleted = 0
                    DataRow[] prsLsts = prices.Select("item = '" + id + "'");

                    foreach (DataRow row in prsLsts)
                    {
                        price = (decimal)row[1];
                        version_price = (int)row[2];
                    }
                }
                var goodPriceActive = (from gp in context.Prices
                                       where gp.GoodID == good.ID
                                       && gp.Active == true
                                       select gp).FirstOrDefault();
                double goodPriceAct = 0;
                if (goodPriceActive != null)
                    goodPriceAct = goodPriceActive.Price1.Value;

                var countBarcodes = (from g in context.Barcodes
                                     where g.GoodID == good.ID
                                     select g.Version_UKM).Sum();
                int countBarcodesSum = 0;
                if (countBarcodes != null)
                    countBarcodesSum = countBarcodes.Value;

                double priceDouble = Double.Parse(price.ToString());

                int depID = 0;
                if (good.Department != null)
                    depID = good.Department.DepartNum_UKM.Value;

                if ((good.Version_Assort_UKM != version_assort || depID != ag_id) ||
                    good.Version_Contenst_UKM != version_contents ||
                    good.Version_UKM != version_good ||
                    countBarcodesSum != barcodes_ver_sum ||
                    goodPriceAct != priceDouble ||
                    good.Version_PrintTemplate_UKM != version_printtempl)
                {

                    var ng = (from g in context.Goods
                              where g.ID == good.ID
                              select g).FirstOrDefault();

                    ng.Name = name;
                    ng.Descr = descr;
                    ng.PLU = plu;
                    ng.Exp_Date = null;
                    ng.Exp_Qty = exp_date2;

                    if (assortmentFinded)
                        ng.Barcode = barcode;
                    else
                        ng.Barcode = firstBarcode;
                    ng.Edinic = measure;


                    var departament = (from d in context.Departments
                                       where d.DepartNum_UKM == ag_id
                                       select d).FirstOrDefault();
                    if (departament != null)
                        ng.Department = departament;
                    //else
                    //    ng.DepartmentID = null;

                    var group = (from g in context.Groups
                                 where g.GroupNum == classif
                                 select g).FirstOrDefault();
                    if (group != null)
                    {
                        ng.Group = group;
                        ng.Version_UKM = version_good;
                    }
                    else
                    {
                        ng.GroupID = null;
                        ng.Version_UKM = 0;
                    }

                    //// шаблоны ценников
                    //if (pricetag_id != 0)
                    //{
                    //    var prt = (from p in context.PrintTemplates
                    //               where p.UKM_ID == pricetag_id
                    //               select p).FirstOrDefault();
                    //    if (prt != null)
                    //    {
                    //        if (deleted_printtempl)
                    //            ng.PrintTemplateID = null;
                    //        else
                    //            ng.PrintTemplates = prt;

                    //        ng.Version_PrintTemplate_UKM = version_printtempl;
                    //    }
                    //    else
                    //        ng.Version_PrintTemplate_UKM = 0;
                    //}
                    //else
                    //{
                    //    ng.PrintTemplateID = null;

                    //    ng.Version_PrintTemplate_UKM = version_printtempl;
                    //}


                    ng.TimeStamp = DateTime.Now;
                    ng.Active = !deleted;
                    ng.Version_Assort_UKM = version_assort;
                    ng.Country = country;
                    ng.Contents = structure;
                    ng.Producer = producer;
                    ng.Version_Contenst_UKM = version_contents;

                    // Добавим цену, если есть

                    double gpaPrice1 = 0;
                    if (goodPriceActive != null)
                        gpaPrice1 = goodPriceActive.Price1.Value;

                    if (gpaPrice1 != priceDouble)
                    {
                        if (goodPriceActive != null)
                        {
                            goodPriceActive.TimeStamp = DateTime.Now;
                            goodPriceActive.Active = false;
                        }

                        if (version_price != 0)
                        {
                            Price np = new Price();
                            np.GoodID = good.ID;
                            np.Price1 = priceDouble;
                            np.Version_UKM = version_price;
                            np.TimeStamp = DateTime.Now;
                            np.Active = true;
                            context.Prices.InsertOnSubmit(np);
                        }
                    }

                    // Barcodes
                    if (barcodes != null)
                    {
                        // select id, item, version, deleted from ukmserver.trm_in_var
                        DataRow[] bcs = barcodes.Select("item = '" + id + "'");

                        foreach (DataRow row in bcs)
                        {
                            barcode = row[0].ToString();
                            int ver_bk = (int)row[2];
                            bool del_bk = (bool)row[3];

                            // bug fix del barcodes other good
                            var barkOld = from b in context.Barcodes
                                          where b.Barcode1 == barcode
                                          && b.GoodID != good.ID
                                          && b.Active == true
                                          select b;
                            foreach (Barcode bc in barkOld)
                            {
                                bc.Active = false;
                            }
                            //-

                            var barcodeGood = (from bk in context.Barcodes
                                               where bk.GoodID == good.ID
                                               && bk.Barcode1 == barcode
                                               select bk).FirstOrDefault();
                            if (barcodeGood != null)
                            {
                                if (barcodeGood.Version_UKM == ver_bk)
                                {
                                    if (del_bk == false && barcodeGood.Active == true)
                                        continue;
                                    if (del_bk == false && barcodeGood.Active == false)
                                        continue;
                                }

                                int version_barcode = (int)row[2];
                                bool deleted_barcode = (bool)row[3];

                                barcodeGood.GoodID = good.ID;
                                barcodeGood.Barcode1 = barcode;
                                barcodeGood.Version_UKM = ver_bk;
                                barcodeGood.TimeStamp = DateTime.Now;
                                barcodeGood.Active = !deleted_barcode;
                            }
                            else
                            {
                                Barcode nb = new Barcode();
                                nb.GoodID = good.ID;
                                nb.Barcode1 = barcode;
                                nb.Version_UKM = ver_bk;
                                nb.TimeStamp = DateTime.Now;
                                nb.Active = !del_bk;
                                context.Barcodes.InsertOnSubmit(nb);
                            }
                            context.SubmitChanges();
                        }
                    }

                    context.SubmitChanges();
                }

                tx.Commit();
            }
            catch (Exception ex)
            {
                tx.Rollback();
            }
        }



        // new
        public string ReadPricesJournal_new_w_del_j()
        {
            string ret = "";

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return ret = "Нет активного магазина в Saalut. ";

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();
                cnx.Open();
                //+goods


                // items
                string cmdText = "select id, name, descr, measure, classif, version, deleted	 from ukmserver.trm_in_items ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds1 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds1);

                DataTable items = ds1.Tables[0];



                // Barcode
                cmdText = "select id, item, version, deleted from ukmserver.trm_in_var ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds2 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds2);

                DataTable barcodes = ds2.Tables[0];


                // Assortment group items
                cmdText = "select ag_id, var, plu, exp_date1, exp_date2, version, deleted from ukmserver.srv_assortment_group_items where store_id = '" + store.StoreID_UKM.ToString() + "' and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds3 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds3);

                DataTable assorts = ds3.Tables[0];


                // Structure
                cmdText = "select id, country, structure, version, deleted, producer_marking from ukmserver.trm_in_item_cc ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds4 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds4);

                DataTable structures = ds4.Tables[0];


                // Prices
                cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds5 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds5);

                DataTable prices = ds5.Tables[0];

                //+ akcionnie
                // Prices
                cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.ActPriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds51 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds51);

                DataTable pricesAkcionnie = ds51.Tables[0];
                //-


                // PrintTemplate good
                cmdText = "select item_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_item ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds6 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds6);

                DataTable printTemplGood = ds6.Tables[0];
                //-goods


                // Выбираем магазин
                cmdText = "select change_log_id from ukmserver.local_data_change_log_status where store_id = '" + store.StoreID_UKM.ToString() + "' and (status = 0 or status = 1)";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);


                if (ds.Tables.Count != 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        int journalID = 0;

                        int change_log_id = Int32.Parse(row[0].ToString());
                        int order_no = 0;

                        cmdText = "select order_no from ukmserver.local_data_change_log where store_id = '" + store.StoreID_UKM.ToString() + "' and id = '" + change_log_id.ToString() + "' ";
                        cmd = new MySqlCommand(cmdText, cnx);
                        cmd.CommandTimeout = 30000;

                        // Create a fill a Dataset
                        DataSet dsO = new DataSet();
                        adapter.SelectCommand = cmd;
                        adapter.Fill(dsO);

                        if (dsO.Tables.Count != 0)
                        {
                            foreach (DataRow row1 in dsO.Tables[0].Rows)
                            {
                                order_no = Int32.Parse(row1[0].ToString());
                            }
                        }


                        var journ = (from j in context.PriceChangeJours
                                     where j.Order_no == order_no
                                     select j).FirstOrDefault();
                        if (journ == null)
                        {
                            //cmdText = "select sum(new_value) from ukmserver.local_data_change_log_entity_prices where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' and pricelist_name = '" + store.PriceListName_UKM + "'";
                            //cmd = new MySqlCommand(cmdText, cnx);

                            //// Create a fill a Dataset
                            //DataSet dsT = new DataSet();
                            //adapter.SelectCommand = cmd;
                            //adapter.Fill(dsT);

                            //Double countNum = 0;
                            //if (dsT.Tables.Count != 0)
                            //{
                            //    foreach (DataRow rowT in dsT.Tables[0].Rows)
                            //    {
                            //        Double.TryParse(rowT[0].ToString(), out countNum);
                            //    }
                            //}
                            //if (countNum != 0)
                            journalID = AddPricesNewJournal(change_log_id, order_no);
                            //else
                            //    continue;

                            cmdText = "select number, item_id from ukmserver.local_data_change_log_entities where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' ";
                            cmd = new MySqlCommand(cmdText, cnx);
                            cmd.CommandTimeout = 30000;

                            // Create a fill a Dataset
                            DataSet ds21 = new DataSet();
                            adapter.SelectCommand = cmd;
                            adapter.Fill(ds21);

                            if (ds21.Tables.Count != 0)
                            {
                                foreach (DataRow row2 in ds21.Tables[0].Rows)
                                {
                                    int number = Int32.Parse(row2[0].ToString());
                                    string item_id = row2[1].ToString();

                                    cmdText = "select new_value from ukmserver.local_data_change_log_entity_prices where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' and number = '" + number.ToString() + "' and pricelist_name = '" + store.PriceListName_UKM + "'";
                                    cmd = new MySqlCommand(cmdText, cnx);
                                    cmd.CommandTimeout = 30000;

                                    // Create a fill a Dataset
                                    DataSet ds31 = new DataSet();
                                    adapter.SelectCommand = cmd;
                                    adapter.Fill(ds31);

                                    if (ds31.Tables.Count != 0)
                                    {
                                        decimal new_value = 0;
                                        foreach (DataRow row3 in ds31.Tables[0].Rows)
                                        {
                                            Decimal.TryParse((string)row3[0], out new_value);
                                        }
                                        //---------------------------------------
                                        string strRet = AddPricesNewLine_new(journalID, item_id, new_value, items, barcodes, assorts, structures, prices, printTemplGood, pricesAkcionnie);
                                        ret += strRet;
                                    }
                                    else
                                    {
                                        //---------------------------------------------
                                        string strRet = AddPricesNewLine_new(journalID, item_id, 0, items, barcodes, assorts, structures, prices, printTemplGood, pricesAkcionnie);
                                        ret += strRet;
                                    }
                                    //----
                                }
                            }
                            //----
                        }//if (journ == null)
                        else
                        {
                            //continue;
                            // обновляем журнал
                            var jourLine_d = from j in context.PriceChangeLine
                                             where j.JournalID == journ.ID
                                             select j;
                            context.PriceChangeLine.DeleteAllOnSubmit(jourLine_d);
                            context.SubmitChanges();

                            var journ_d = from j in context.PriceChangeJours
                                          where j.ID == journ.ID
                                          select j;
                            context.PriceChangeJours.DeleteAllOnSubmit(journ_d);
                            context.SubmitChanges();



                            journalID = AddPricesNewJournal(change_log_id, order_no);

                            cmdText = "select number, item_id from ukmserver.local_data_change_log_entities where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' ";
                            cmd = new MySqlCommand(cmdText, cnx);
                            cmd.CommandTimeout = 30000;

                            // Create a fill a Dataset
                            DataSet ds21 = new DataSet();
                            adapter.SelectCommand = cmd;
                            adapter.Fill(ds21);

                            if (ds21.Tables.Count != 0)
                            {
                                foreach (DataRow row2 in ds21.Tables[0].Rows)
                                {
                                    int number = Int32.Parse(row2[0].ToString());
                                    string item_id = row2[1].ToString();


                                    cmdText = "select new_value from ukmserver.local_data_change_log_entity_prices where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' and number = '" + number.ToString() + "' and pricelist_name = '" + store.PriceListName_UKM + "'";
                                    cmd = new MySqlCommand(cmdText, cnx);
                                    cmd.CommandTimeout = 30000;

                                    // Create a fill a Dataset
                                    DataSet ds32 = new DataSet();
                                    adapter.SelectCommand = cmd;
                                    adapter.Fill(ds32);

                                    if (ds32.Tables.Count != 0)
                                    {
                                        decimal new_value = 0;
                                        foreach (DataRow row3 in ds32.Tables[0].Rows)
                                        {
                                            Decimal.TryParse((string)row3[0], out new_value);
                                        }

                                        //--------------------------------
                                        string strRet = AddPricesNewLine_new(journalID, item_id, new_value, items, barcodes, assorts, structures, prices, printTemplGood, pricesAkcionnie);
                                        ret += strRet;
                                    }
                                    else
                                    {

                                        //--------------------------------
                                        string strRet = AddPricesNewLine_new(journalID, item_id, 0, items, barcodes, assorts, structures, prices, printTemplGood, pricesAkcionnie);
                                        ret += strRet;
                                    }
                                    //----
                                }
                            }

                        }//if (journ == null)

                    }
                }
                else
                    ret += "УКМ Журнал цен - пусто (local_data_change_log_status). ";
            }
            catch (MySqlException ex)
            {
                ret += "Error: " + ex.ToString();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            if (ret == "")
                ret = "пусто";

            return ret;
        }


        public string ReadPricesJournal_new()
        {
            string ret = "";

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return ret = "Нет активного магазина в Saalut. ";

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();
                cnx.Open();
                //+goods


                // items
                string cmdText = "select id, name, descr, measure, classif, version, deleted from ukmserver.trm_in_items ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds1 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds1);

                DataTable items = ds1.Tables[0];



                // Barcode
                cmdText = "select id, item, version, deleted from ukmserver.trm_in_var ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds2 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds2);

                DataTable barcodes = ds2.Tables[0];


                // Assortment group items
                cmdText = "select ag_id, var, plu, exp_date1, exp_date2, version, deleted from ukmserver.srv_assortment_group_items where store_id = '" + store.StoreID_UKM.ToString() + "' and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds3 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds3);

                DataTable assorts = ds3.Tables[0];


                // Structure
                cmdText = "select id, country, structure, version, deleted, producer_marking from ukmserver.trm_in_item_cc ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds4 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds4);

                DataTable structures = ds4.Tables[0];


                // Prices
                cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds5 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds5);

                DataTable prices = ds5.Tables[0];

                //+ akcionnie
                // Prices
                cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.ActPriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds51 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds51);

                DataTable pricesAkcionnie = ds51.Tables[0];
                //-


                // PrintTemplate good
                cmdText = "select item_id, pricetag_id, version, deleted from ukmserver.srv_pricetags_item ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds6 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds6);

                DataTable printTemplGood = ds6.Tables[0];
                //-goods


                // Выбираем магазин
                cmdText = "select change_log_id from ukmserver.local_data_change_log_status where store_id = '" + store.StoreID_UKM.ToString() + "' and (status = 0 or status = 1)";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);


                if (ds.Tables.Count != 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        int journalID = 0;

                        int change_log_id = Int32.Parse(row[0].ToString());
                        int order_no = 0;

                        cmdText = "select order_no from ukmserver.local_data_change_log where store_id = '" + store.StoreID_UKM.ToString() + "' and id = '" + change_log_id.ToString() + "' ";
                        cmd = new MySqlCommand(cmdText, cnx);
                        cmd.CommandTimeout = 30000;

                        // Create a fill a Dataset
                        DataSet dsO = new DataSet();
                        adapter.SelectCommand = cmd;
                        adapter.Fill(dsO);

                        if (dsO.Tables.Count != 0)
                        {
                            foreach (DataRow row1 in dsO.Tables[0].Rows)
                            {
                                order_no = Int32.Parse(row1[0].ToString());
                            }
                        }


                        var journ = (from j in context.PriceChangeJours
                                     where j.Order_no == order_no
                                     select j).FirstOrDefault();
                        if (journ == null)
                        {
                            //cmdText = "select sum(new_value) from ukmserver.local_data_change_log_entity_prices where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' and pricelist_name = '" + store.PriceListName_UKM + "'";
                            //cmd = new MySqlCommand(cmdText, cnx);

                            //// Create a fill a Dataset
                            //DataSet dsT = new DataSet();
                            //adapter.SelectCommand = cmd;
                            //adapter.Fill(dsT);

                            //Double countNum = 0;
                            //if (dsT.Tables.Count != 0)
                            //{
                            //    foreach (DataRow rowT in dsT.Tables[0].Rows)
                            //    {
                            //        Double.TryParse(rowT[0].ToString(), out countNum);
                            //    }
                            //}
                            //if (countNum != 0)
                            journalID = AddPricesNewJournal(change_log_id, order_no);
                            //else
                            //    continue;

                            cmdText = "select number, item_id from ukmserver.local_data_change_log_entities where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' ";
                            cmd = new MySqlCommand(cmdText, cnx);
                            cmd.CommandTimeout = 30000;

                            // Create a fill a Dataset
                            DataSet ds21 = new DataSet();
                            adapter.SelectCommand = cmd;
                            adapter.Fill(ds21);

                            if (ds21.Tables.Count != 0)
                            {
                                foreach (DataRow row2 in ds21.Tables[0].Rows)
                                {
                                    int number = Int32.Parse(row2[0].ToString());
                                    string item_id = row2[1].ToString();

                                    cmdText = "select new_value from ukmserver.local_data_change_log_entity_prices where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' and number = '" + number.ToString() + "' and pricelist_name = '" + store.PriceListName_UKM + "'";
                                    cmd = new MySqlCommand(cmdText, cnx);
                                    cmd.CommandTimeout = 30000;

                                    // Create a fill a Dataset
                                    DataSet ds31 = new DataSet();
                                    adapter.SelectCommand = cmd;
                                    adapter.Fill(ds31);

                                    if (ds31.Tables.Count != 0)
                                    {
                                        decimal new_value = 0;
                                        foreach (DataRow row3 in ds31.Tables[0].Rows)
                                        {
                                            Decimal.TryParse((string)row3[0], out new_value);
                                        }
                                        //---------------------------------------
                                        string strRet = AddPricesNewLine_new(journalID, item_id, new_value, items, barcodes, assorts, structures, prices, printTemplGood, pricesAkcionnie);
                                        ret += strRet;
                                    }
                                    else
                                    {
                                        //---------------------------------------------
                                        string strRet = AddPricesNewLine_new(journalID, item_id, 0, items, barcodes, assorts, structures, prices, printTemplGood, pricesAkcionnie);
                                        ret += strRet;
                                    }
                                    //----
                                }
                            }
                            //----
                        }//if (journ == null)
                        else
                        {
                            continue; // данный метод запускается от сервиса SaalutServ
                            // обновляем журнал
                            //var jourLine_d = from j in context.PriceChangeLine
                            //                 where j.JournalID == journ.ID
                            //                    select j;
                            //context.PriceChangeLine.DeleteAllOnSubmit(jourLine_d);
                            //context.SubmitChanges();

                            //var journ_d = from j in context.PriceChangeJours
                            //              where j.ID == journ.ID
                            //            select j;
                            //context.PriceChangeJours.DeleteAllOnSubmit(journ_d);
                            //context.SubmitChanges();



                            journalID = AddPricesNewJournal(change_log_id, order_no);

                            cmdText = "select number, item_id from ukmserver.local_data_change_log_entities where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' ";
                            cmd = new MySqlCommand(cmdText, cnx);
                            cmd.CommandTimeout = 30000;

                            // Create a fill a Dataset
                            DataSet ds21 = new DataSet();
                            adapter.SelectCommand = cmd;
                            adapter.Fill(ds21);

                            if (ds21.Tables.Count != 0)
                            {
                                foreach (DataRow row2 in ds21.Tables[0].Rows)
                                {
                                    int number = Int32.Parse(row2[0].ToString());
                                    string item_id = row2[1].ToString();


                                    cmdText = "select new_value from ukmserver.local_data_change_log_entity_prices where store_id = '" + store.StoreID_UKM.ToString() + "' and change_log_id = '" + change_log_id.ToString() + "' and number = '" + number.ToString() + "' and pricelist_name = '" + store.PriceListName_UKM + "'";
                                    cmd = new MySqlCommand(cmdText, cnx);
                                    cmd.CommandTimeout = 30000;

                                    // Create a fill a Dataset
                                    DataSet ds32 = new DataSet();
                                    adapter.SelectCommand = cmd;
                                    adapter.Fill(ds32);

                                    if (ds32.Tables.Count != 0)
                                    {
                                        decimal new_value = 0;
                                        foreach (DataRow row3 in ds32.Tables[0].Rows)
                                        {
                                            Decimal.TryParse((string)row3[0], out new_value);
                                        }

                                        //--------------------------------
                                        string strRet = AddPricesNewLine_new(journalID, item_id, new_value, items, barcodes, assorts, structures, prices, printTemplGood, pricesAkcionnie);
                                        ret += strRet;
                                    }
                                    else
                                    {

                                        //--------------------------------
                                        string strRet = AddPricesNewLine_new(journalID, item_id, 0, items, barcodes, assorts, structures, prices, printTemplGood, pricesAkcionnie);
                                        ret += strRet;
                                    }
                                    //----
                                }
                            }

                        }//if (journ == null)

                    }
                }
                else
                    ret += "УКМ Журнал цен - пусто (local_data_change_log_status). ";
            }
            catch (MySqlException ex)
            {
                ret += "Error: " + ex.ToString();
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            if (ret == "")
                ret = "пусто";

            return ret;
        }

        public string AddPricesNewLine_new(int journalID, string item_id, decimal new_value, DataTable items, DataTable barcodes, DataTable assortments, DataTable structures, DataTable prices, DataTable printTemplGood, DataTable pricesAkcionnie)
        {
            string ret = "";

            CommittableTransaction tx = new CommittableTransaction();
            try
            {

                var good = (from g in context.Goods
                            where g.Articul == item_id
                            select g).FirstOrDefault();

                if (good == null)
                {
                    // номенклатуры нету

                    DataRow[] item = items.Select("id = '" + item_id + "'");
                    foreach (DataRow row in item)
                    {
                        CreateNewGood(row, barcodes, assortments, structures, prices, printTemplGood);
                    }


                    ret += "Товар " + item_id + " - нет в справочнике товаров! Журнал цен: " + journalID + ". ";


                    //+
                    bool AkcionnayaCena = false;
                    DataRow[] aktPrices = pricesAkcionnie.Select("item = '" + item_id + "'");
                    foreach (DataRow row1 in aktPrices)
                    {

                        string artikulA = (string)row1[0];
                        decimal priceA = (decimal)row1[1];
                        int version_priceA = (int)row1[2];
                        bool delete_priceA = (bool)row1[3];

                        double newPriceA = Double.Parse(priceA.ToString());

                        var pricesSaalutA = (from v in context.PricesAkcionnies
                                             where v.GoodID == good.ID
                                             && v.Active == true
                                             select v).FirstOrDefault();

                        if (pricesSaalutA != null)
                        {
                            if (version_priceA == pricesSaalutA.Version_UKM)
                                continue; // пропускаем если версия совпадает

                            if (newPriceA == pricesSaalutA.Price.Value)
                                continue; // пропускаем если цена не поменялась

                            pricesSaalutA.Active = false;
                            context.SubmitChanges();

                            //+
                            PricesAkcionnie npr1 = new PricesAkcionnie();
                            npr1.GoodID = good.ID;
                            npr1.Price = newPriceA;
                            npr1.Version_UKM = version_priceA;
                            npr1.TimeStamp = DateTime.Now;
                            npr1.Active = true;
                            context.PricesAkcionnies.InsertOnSubmit(npr1);
                            context.SubmitChanges();
                            //-


                            AkcionnayaCena = true;

                            //if (delete_price)
                            //{
                            //    continue;
                            //}
                        }

                    }
                    //-

                    PriceChangeLine nl = new PriceChangeLine();
                    nl.GoodID = null; // товара нет будем потом его искать
                    nl.ItemID_UKM = item_id;
                    nl.JournalID = journalID;

                    double price = 0;
                    Double.TryParse(new_value.ToString(), out price);

                    nl.NewPrice = price;
                    nl.TimeStamp = DateTime.Now;
                    nl.Akcionniy = AkcionnayaCena;
                    nl.Active = false;
                    context.PriceChangeLine.InsertOnSubmit(nl);
                    context.SubmitChanges();

                    if (price == 0)
                        ret += "Товар " + item_id + " цена 0! Журнал цен: " + journalID + ". ";

                    // догрузим его при обновлении журнала
                }
                else
                {
                    DataRow[] item = items.Select("id = '" + item_id + "'");
                    foreach (DataRow row in item)
                    {
                        UpdateGood(good, row, barcodes, assortments, structures, prices, printTemplGood);
                    }

                    //+
                    bool AkcionnayaCena = false;
                    DataRow[] aktPrices = pricesAkcionnie.Select("item = '" + item_id + "'");
                    foreach (DataRow row1 in aktPrices)
                    {

                        string artikulA = (string)row1[0];
                        decimal priceA = (decimal)row1[1];
                        int version_priceA = (int)row1[2];
                        bool delete_priceA = (bool)row1[3];

                        double newPriceA = Double.Parse(priceA.ToString());

                        var pricesSaalutA = (from v in context.PricesAkcionnies
                                             where v.GoodID == good.ID
                                             && v.Active == true
                                             select v).FirstOrDefault();

                        if (pricesSaalutA != null)
                        {
                            if (version_priceA == pricesSaalutA.Version_UKM)
                                continue; // пропускаем если версия совпадает

                            if (newPriceA == pricesSaalutA.Price.Value)
                                continue; // пропускаем если цена не поменялась

                            pricesSaalutA.Active = false;
                            context.SubmitChanges();

                            //+
                            PricesAkcionnie npr1 = new PricesAkcionnie();
                            npr1.GoodID = good.ID;
                            npr1.Price = newPriceA;
                            npr1.Version_UKM = version_priceA;
                            npr1.TimeStamp = DateTime.Now;
                            npr1.Active = true;
                            context.PricesAkcionnies.InsertOnSubmit(npr1);
                            context.SubmitChanges();
                            //-


                            AkcionnayaCena = true;

                            //if (delete_price)
                            //{
                            //    continue;
                            //}
                        }

                    }
                    //-

                    PriceChangeLine nl = new PriceChangeLine();
                    nl.GoodID = good.ID;
                    nl.ItemID_UKM = item_id;
                    nl.JournalID = journalID;

                    double price = 0;
                    Double.TryParse(new_value.ToString(), out price);

                    nl.NewPrice = price;
                    nl.TimeStamp = DateTime.Now;
                    nl.Active = true;
                    nl.Akcionniy = AkcionnayaCena;
                    context.PriceChangeLine.InsertOnSubmit(nl);
                    context.SubmitChanges();

                    if (price == 0)
                        ret += "Товар " + item_id + " цена 0! Журнал цен: " + journalID + ". ";

                }

                tx.Commit();


            }
            catch (Exception ex)
            {
                tx.Rollback();
            }

            return ret;
        }

        /// <summary>
        /// Проверка разности версий в ценах, вывод журнала изменения цен в сюда
        /// </summary>
        /// <returns></returns>
        public string ReadNewData_from_UKM_New_Price()
        {
            string ret = "";

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            // + Инициализация магазина
            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                InitialStoreInfoIns();
            // - Инициализация магазина

            ret += "Новые материалы: " + UKM_Quick_LoadsToDB();

            ret += "Проверка цен: " + NewJourPrice_versions_check();

            return ret;
        }

        public string NewJourPrice_versions_check()
        {
            string ret = "";
            bool journalCreated = false;
            int journalID = 0;
            int newPriceCounter = 0;

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();

            // + цена по новому
            DataTable prices;

            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);
                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // + counter price
                string cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select sum(version) from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                DataTable itemSum = ds.Tables[0];
                decimal counter = 0;

                if (itemSum != null)
                {
                    foreach (DataRow row in itemSum.Rows)
                    {
                        counter = (decimal)row[0];
                    }
                }
                // - counter price
                // убираем т.к. иногда ошибка преобразования типа INT
                //// + counter akcion price
                //cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select sum(version) from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.ActPriceList_ID_UKM.ToString() + "'; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                //cmd = new MySqlCommand(cmdText, cnx);
                //cmd.CommandTimeout = 30000;

                //// Create a fill a Dataset
                //DataSet ds2 = new DataSet();
                //adapter.SelectCommand = cmd;
                //adapter.Fill(ds2);

                //DataTable itemSumAkc = ds2.Tables[0];
                decimal counterAkc = 0;

                //if (itemSumAkc != null)
                //{
                //    foreach (DataRow row in itemSumAkc.Rows)
                //    {
                //        counterAkc = (decimal)row[0];
                //    }
                //}
                //// - counter akcion price

                if (counter != 0 && counterAkc != 0)
                {
                    var countPrices = (from g in context.Prices
                                       where g.Active == true
                                       select g.Version_UKM).Sum();
                    var countPricesAkc = (from g in context.PricesAkcionnies
                                          where g.Active == true
                                          select g.Version_UKM).Sum();
                    if (countPrices != null && countPricesAkc != null)
                    {
                        if (countPrices.Value == counter && countPricesAkc.Value == counterAkc)
                            return ret += "Изменения отсутствуют. Повторите позднее... ";
                    }
                }


                //------------------------------------


                // Prices
                cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select item, price, version, deleted from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds5 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds5);

                prices = ds5.Tables[0];

                if (prices != null)
                {

                    foreach (DataRow row in prices.Rows)
                    {
                        string artikul = (string)row[0];
                        decimal price = (decimal)row[1];
                        int version_price = (int)row[2];
                        bool delete_price = (bool)row[3];

                        var good = (from g in context.Goods
                                    where g.Articul == artikul
                                    select g).FirstOrDefault();
                        if (good == null)
                            continue;

                        double newPrice = Double.Parse(price.ToString());

                        var pricesSaalut = (from v in context.Prices
                                            where v.GoodID == good.ID
                                            && v.Active == true
                                            select v).FirstOrDefault();

                        if (pricesSaalut != null)
                        {
                            if (version_price == pricesSaalut.Version_UKM)
                                continue; // пропускаем если версия совпадает

                            if (newPrice == pricesSaalut.Price1.Value)
                                continue; // пропускаем если цена не поменялась

                            pricesSaalut.Active = false;
                            context.SubmitChanges();

                            if (delete_price)
                            {
                                continue;
                            }
                        }

                        DateTime now = DateTime.Now;
                        if (!journalCreated)
                        {
                            PriceChangeJour n = new PriceChangeJour();
                            n.Change_log_id_UKM = 1;
                            n.Order_no = 1;
                            n.TimeStamp = now;
                            n.InUse = false; // не будем использовать пока что
                            n.Active = true;
                            context.PriceChangeJours.InsertOnSubmit(n);
                            context.SubmitChanges();

                            journalCreated = true;

                            var newJourn = (from j in context.PriceChangeJours
                                            where j.TimeStamp == now
                                            select j).FirstOrDefault();

                            newJourn.Change_log_id_UKM = newJourn.ID;
                            newJourn.Order_no = newJourn.ID;
                            journalID = newJourn.ID;
                        }

                        // + проверка акционной цены
                        bool AkcionnayaCena = false;

                        cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select item, price, version, deleted from ukmserver.trm_in_pricelist_items where item = '" + artikul + "' and pricelist_id = '" + store.ActPriceList_ID_UKM.ToString() + "'  and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                        cmd = new MySqlCommand(cmdText, cnx);
                        cmd.CommandTimeout = 30000;

                        // Create a fill a Dataset
                        DataSet ds7 = new DataSet();
                        adapter.SelectCommand = cmd;
                        adapter.Fill(ds7);

                        DataTable pricesAkcionnie = ds7.Tables[0];
                        if (pricesAkcionnie != null)
                        {
                            foreach (DataRow row1 in pricesAkcionnie.Rows)
                            {
                                string artikulA = (string)row1[0];
                                decimal priceA = (decimal)row1[1];
                                int version_priceA = (int)row1[2];
                                bool delete_priceA = (bool)row1[3];


                                double newPriceA = Double.Parse(priceA.ToString());

                                var pricesSaalutA = (from v in context.PricesAkcionnies
                                                     where v.GoodID == good.ID
                                                     && v.Active == true
                                                     select v).FirstOrDefault();

                                if (pricesSaalutA != null)
                                {
                                    if (version_priceA == pricesSaalutA.Version_UKM)
                                        continue; // пропускаем если версия совпадает

                                    if (newPriceA == pricesSaalutA.Price.Value)
                                        continue; // пропускаем если цена не поменялась

                                    pricesSaalutA.Active = false;
                                    context.SubmitChanges();

                                    //+
                                    PricesAkcionnie npr1 = new PricesAkcionnie();
                                    npr1.GoodID = good.ID;
                                    npr1.Price = newPriceA;
                                    npr1.Version_UKM = version_priceA;
                                    npr1.TimeStamp = now;
                                    npr1.Active = true;
                                    context.PricesAkcionnies.InsertOnSubmit(npr1);
                                    context.SubmitChanges();
                                    //-


                                    AkcionnayaCena = true;

                                    //if (delete_price)
                                    //{
                                    //    continue;
                                    //}
                                }
                            }


                        }
                        // - проверка акционной цены

                        PriceChangeLine nl = new PriceChangeLine();
                        nl.GoodID = good.ID;
                        nl.ItemID_UKM = good.Articul;
                        nl.JournalID = journalID;
                        nl.NewPrice = newPrice;
                        nl.TimeStamp = now;
                        nl.Active = true;
                        nl.Akcionniy = AkcionnayaCena; // акционная цена
                        context.PriceChangeLine.InsertOnSubmit(nl);

                        // Price tbl
                        Price npr = new Price();
                        npr.GoodID = good.ID;
                        npr.Price1 = newPrice;
                        npr.Version_UKM = version_price;
                        npr.TimeStamp = now;
                        npr.Active = true;
                        context.Prices.InsertOnSubmit(npr);
                        context.SubmitChanges();

                        newPriceCounter++;
                    }

                }
                else
                {
                    return ret += "Нет цен. Повторите позднее... ";
                }

                if (newPriceCounter != 0)
                    ret += "Создан журнал " + journalID.ToString() + " с количеством строк " + newPriceCounter.ToString() + ". ";
            }
            catch (MySqlException ex)
            {
                return ex.Message;
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }
            // - цена по новому


            return ret;
        }


    }
}