using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using MySql.Data.Common;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data;
using SAP.Middleware.Connector;
using System.Data.Odbc;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Configuration;

namespace Saalut
{
    public partial class UKM_SAPERP_OpenStocks : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;


        // Connection string for a typical local MySQL installation
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;

        string connStrSQL = global::System.Configuration.ConfigurationManager.ConnectionStrings["SaalutExpressConnectionString"].ConnectionString;



        protected void Page_Load(object sender, EventArgs e)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            if(!IsPostBack)
                txbOnDate.Text = DateTime.Today.ToString("yyyyMMdd");
        }

        protected void SubmitButton1_Click(object sender, EventArgs e)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();


            string[] textParagraphs = txbMaters.Text.Split('\n');
            string[] newTP = new string[textParagraphs.Length];
            string outMaters = "";

            int i = 0;
            foreach (string str in textParagraphs)
            {
                string strT = str.Replace("\r", "");
                newTP[i] = strT;
                i++;
            }

            outMaters += "'";
            i = 1;
            foreach (string str in newTP)
            {
                outMaters += str;
                if (i != newTP.Count())
                    outMaters += "','";
                else
                    outMaters += "'";
                i++;
            }

            if (cbxGetData.Checked)
            {
                // загружаем данные из ЕРП
                LoadDataFromSAPERP(newTP);

                // загружаем из укм
                UKMSold_Info_load(outMaters);
            }


            //Connect to the database for Collections Table
            SqlConnection thisConnection = new SqlConnection(connStrSQL);

            //Create DataAdapter Object
            SqlDataAdapter thisAdapter = new SqlDataAdapter("  SELECT ROW_NUMBER() OVER (ORDER BY Artikul) as NUM, [Artikul]      ,[GoodName]  ,'1000' as Sklad     ,Sum([Sold]) as Qty  FROM [SaalutExpress].[dbo].[UKMSoldGoods]  where [KassaSmena] not in (SELECT [SMENA]  FROM [SaalutExpress].[dbo].[SAPOUT_SMENA]) group by [Artikul],[GoodName] ", thisConnection);

            // Create a fill a Dataset
            DataSet ds = new DataSet();
            thisAdapter.Fill(ds);


            List<string> artikulsInSap = new List<string>();



            DataRow[] result = ds.Tables[0].Select();
            foreach (DataRow row in result)
            {
                string artikul = row[1].ToString();
                string sklad = row[3].ToString();

                var sapdata = (from s in context.SAPOUT_REST
                               where s.MATNR == artikul
                               && s.LGORT == sklad
                               select s).FirstOrDefault();
                if (sapdata != null)
                {
                    double ukmqty = Double.Parse(row[4].ToString());
                    row[4] = sapdata.LABST - ukmqty;

                    row.EndEdit();
                    ds.Tables[0].AcceptChanges();

                    artikulsInSap.Add(artikul);
                }
            }

            string[] artikulsIn = artikulsInSap.ToArray();

            var sapdanns = from s in context.SAPOUT_REST
                           where (!artikulsIn.Contains(s.MATNR))
                           select s;
            i = 0;
            foreach (SAPOUT_REST so in sapdanns)
            {
                if (so.LGORT == "4000" && so.LABST == 0)
                    continue;

                DataRow workRow;
                workRow = ds.Tables[0].NewRow();
                workRow[0] = i;
                workRow[1] = so.MATNR;
                workRow[2] = so.MAKTX;
                workRow[3] = so.LGORT;
                workRow[4] = so.LABST;
                ds.Tables[0].Rows.Add(workRow);
                workRow.EndEdit();
                ds.Tables[0].AcceptChanges();

                i++;
            }


            // счетчик
            i = 1;
            result = ds.Tables[0].Select();
            foreach (DataRow row in result)
            {
                row[0] = i;

                row.EndEdit();
                ds.Tables[0].AcceptChanges();

                i++;
            }

            // выводим
            if (ds.Tables[0].Rows.Count != 0)
            {
                object objSumUKM;
                objSumUKM = ds.Tables[0].Compute("Sum(Qty)", "");


                GridView ReportGridView1 = new GridView();
                ReportGridView1.Width = new Unit(950);
                ReportGridView1.AutoGenerateColumns = false;

                BoundField fld0 = new System.Web.UI.WebControls.BoundField();
                fld0.HeaderText = "N";
                fld0.DataField = "NUM";
                ReportGridView1.Columns.Add(fld0);

                BoundField fld3 = new System.Web.UI.WebControls.BoundField();
                fld3.HeaderText = "Арт.";
                fld3.DataField = "Artikul";
                ReportGridView1.Columns.Add(fld3);


                BoundField fld4 = new System.Web.UI.WebControls.BoundField();
                fld4.HeaderText = "Наименование";
                fld4.DataField = "GoodName";
                ReportGridView1.Columns.Add(fld4);

                BoundField fld5 = new System.Web.UI.WebControls.BoundField();
                fld5.HeaderText = "Склад";
                fld5.DataField = "Sklad";
                ReportGridView1.Columns.Add(fld5);


                BoundField fld6 = new System.Web.UI.WebControls.BoundField();
                fld6.HeaderText = "Остаток.";
                fld6.DataField = "Qty";
                fld6.DataFormatString = "{0:F3}";
                ReportGridView1.Columns.Add(fld6);


                ReportGridView1.DataSource = ds.Tables[0];
                ReportGridView1.DataBind();
                ReportPlaceHolder1.Controls.Add(ReportGridView1);

                Label lblGroup = new Label();
                lblGroup.Text = "Итоги: " + objSumUKM.ToString() + ".";
                ReportPlaceHolder1.Controls.Add(lblGroup);
            }


            //Close Connection
            thisConnection.Close();

            Wizard1.ActiveStepIndex = 1;
        }

        public void LoadDataFromSAPERP(string[] newTP)
        {
            string werk = "";
            string onDate = txbOnDate.Text;

            var settingsSP = (from s in context.SettingsSAPERPTbls
                              select s).FirstOrDefault();
            werk = settingsSP.Werk;

            if (werk == null || werk == "")
            {
                ErrorLabel1.Text = "Не указан в настройках завод магазина";
                Wizard1.ActiveStepIndex = 1;
                return;
            }

            MyBackendConfig2 cfg = new MyBackendConfig2();
            try
            {
                // delete all
                var sap1Del = from p in context.SAPOUT_REST
                              select p;
                context.SAPOUT_REST.DeleteAllOnSubmit(sap1Del);
                context.SubmitChanges();
                var sap2Del = from p in context.SAPOUT_SMENA
                              select p;
                context.SAPOUT_SMENA.DeleteAllOnSubmit(sap2Del);
                context.SubmitChanges();
                //-----------------------------

                RfcDestinationManager.RegisterDestinationConfiguration(cfg);//1             
                RfcDestination prd = RfcDestinationManager.GetDestination("AEP");//2  


                RfcRepository repo = prd.Repository;//3        

                IRfcFunction pricesBapi =
                    repo.CreateFunction("Y_GET_MATNR_REST");//4                               
                pricesBapi.SetValue("I_WERKS", werk); //5     
                //pricesBapi.SetValue("I_DATE", onDate); //5 

                IRfcTable detail1 = pricesBapi.GetTable("TAB");

                foreach (string strs in newTP)
                {
                    detail1.Append();

                    detail1.SetValue("MATNR", strs);
                    detail1.SetValue("MAKTX", "");
                    detail1.SetValue("LGORT", "");
                    detail1.SetValue("LABST", 0.0);
                    detail1.SetValue("MEINS", "");
                }

                pricesBapi.Invoke(prd); //6                 
                IRfcTable detail3 = pricesBapi.GetTable("TAB");


                DateTime tsmp = DateTime.Now;

                int i = 0;
                foreach (IRfcStructure elem in detail3)
                {
                    string matnr = elem[0].GetString();
                    string maktx = elem[1].GetString();
                    string lgort = elem[2].GetString();
                    double labst = elem[3].GetDouble();
                    string meins = elem[4].GetString();

                    SAPOUT_REST np = new SAPOUT_REST();
                    np.MATNR = matnr;
                    np.MAKTX = maktx;
                    np.LGORT = lgort;
                    np.LABST = labst;
                    np.MEINS = meins;
                    np.TimeStamp = tsmp;
                    context.SAPOUT_REST.InsertOnSubmit(np);

                    if (i == 100)
                    {
                        context.SubmitChanges();
                        i = 0;
                    }
                    i++;
                }
                context.SubmitChanges();



                RfcRepository repo2 = prd.Repository;//3 

                IRfcFunction pricesBapi2 =
                    repo2.CreateFunction("Y_GET_SMENA");//4                               
                pricesBapi2.SetValue("I_WERKS", werk); //5     
                pricesBapi2.SetValue("I_DATE", onDate); //5 
                pricesBapi2.Invoke(prd); //6     
                IRfcTable detail2 = pricesBapi2.GetTable("TAB");

                tsmp = DateTime.Now;

                i = 0;
                foreach (IRfcStructure elem in detail2)
                {
                    string sm_date = elem[0].GetString();
                    string smena = elem[1].GetString();

                    SAPOUT_SMENA np = new SAPOUT_SMENA();
                    np.SM_DATE = DateTime.Parse(sm_date);
                    np.SMENA = smena;
                    np.TimeStamp = tsmp;
                    context.SAPOUT_SMENA.InsertOnSubmit(np);

                    if (i == 100)
                    {
                        context.SubmitChanges();
                        i = 0;
                    }
                    i++;
                }
                context.SubmitChanges();



                //String companyName = detail.
                //    GetString("NAME1");//7                                 
                //Console.WriteLine(companyName);
                //Console.Read();
            }
            catch (RfcInvalidStateException e)
            {
                // cascade up callstack
                ErrorLabel1.Text = "Проблема повторного подключения к SAP: " + e.Message;
                Wizard1.ActiveStepIndex = 1;

                Log l = new Log();
                l.Message = e.Message;
                l.Type = "SAP";
                l.TimeStamp = DateTime.Now;
                context.Logs.InsertOnSubmit(l);
                context.SubmitChanges();
            }
            catch (RfcCommunicationException e)
            {
                // network problem...
                ErrorLabel1.Text = "Сетевая проблема подключения к SAP: " + e.Message;
                Wizard1.ActiveStepIndex = 1;

                Log l = new Log();
                l.Message = e.Message;
                l.Type = "SAP";
                l.TimeStamp = DateTime.Now;
                context.Logs.InsertOnSubmit(l);
                context.SubmitChanges();
            }
            catch (RfcLogonException e)
            {
                // user could not logon...
                ErrorLabel1.Text = "Не правильный пользователь для подключения к SAP: " + e.Message;
                Wizard1.ActiveStepIndex = 1;

                Log l = new Log();
                l.Message = e.Message;
                l.Type = "SAP";
                l.TimeStamp = DateTime.Now;
                context.Logs.InsertOnSubmit(l);
                context.SubmitChanges();
            }
            catch (RfcAbapRuntimeException e)
            {
                // serious problem on ABAP system side...
                ErrorLabel1.Text = "Серьезная проблема на стороне SAP обратитесь в службу поддержки SAP (serious problem on ABAP system side...): " + e.Message;
                Wizard1.ActiveStepIndex = 1;

                Log l = new Log();
                l.Message = e.Message;
                l.Type = "SAP";
                l.TimeStamp = DateTime.Now;
                context.Logs.InsertOnSubmit(l);
                context.SubmitChanges();
            }
            catch (RfcAbapBaseException e)
            {
                // The function module returned an ABAP exception, an ABAP message
                // or an ABAP class-based exception...
                ErrorLabel1.Text = "Серьезная проблема на стороне SAP обратитесь в службу поддержки SAP (The function module returned an ABAP exception, an ABAP message): " + e.Message;
                Wizard1.ActiveStepIndex = 1;

                Log l = new Log();
                l.Message = e.Message;
                l.Type = "SAP";
                l.TimeStamp = DateTime.Now;
                context.Logs.InsertOnSubmit(l);
                context.SubmitChanges();
            }
            finally
            {
                RfcDestinationManager.UnregisterDestinationConfiguration(cfg);//1   
            }

        }


        public string UKMSold_Info_load(string outMaters)
        {
            string ret = "";
            int newPriceCounter = 0;

            if (context == null)
                context = new SaalutDataClasses1DataContext();


            string txtDate = txbOnDate.Text;
            txtDate = txtDate.Substring(0, 4) + "-" + txtDate.Substring(4, 2) + "-" + txtDate.Substring(6, 2);


            // Del
            var ukmInfoDel = from p in context.UKMSoldGoods
                             select p;
            context.UKMSoldGoods.DeleteAllOnSubmit(ukmInfoDel);
            context.SubmitChanges();
            //-


            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();


            DataTable sales;

            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);
                MySqlDataAdapter adapter = new MySqlDataAdapter();


                // 
                string cmdText = " SELECT usr.name 'Casher', c.number 'POSN', c.cash_id 'CASH', so.id 'Smena', i.item 'Item', i.name 'Name', classif.id 'classif_id', classif.name 'classif', SUM(IF(h.type IN (0,5), 1, -1) * i.quantity) 'Qty', SUM(IF(h.type IN (0,5), 1, -1) * i.total + i.discount) 'Sum' FROM trm_in_pos c INNER JOIN trm_out_receipt_header h ON h.cash_id = c.cash_id INNER JOIN trm_out_receipt_item i ON i.cash_id = h.cash_id AND i.receipt_header = h.id  LEFT JOIN trm_out_receipt_item i2 ON (h.cash_id = i2.cash_id AND h.id = i2.receipt_header AND i2.link_item = i.id) INNER JOIN trm_out_receipt_footer f ON f.cash_id = h.cash_id AND f.id = h.id INNER JOIN trm_in_classif classif ON (i.classif = classif.id ) INNER JOIN trm_out_login lg ON (h.cash_id = lg.cash_id AND h.login = lg.id) INNER JOIN trm_in_users usr ON (c.store_id = usr.store_id AND lg.user_id = usr.id) INNER join trm_out_shift_open so on (so.id = h.shift_open AND so.cash_id = h.cash_id) WHERE i2.link_item IS NULL AND i.type = 0 AND h.type IN (0,5,1,4) AND f.result IN (0) AND i.item in (" + outMaters + ") AND so.date >= '" + txtDate + "'  GROUP BY h.shift_open, usr.name, i.item ORDER BY h.shift_open, usr.name, i.item;";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 0;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                sales = ds.Tables[0];

                if (sales != null)
                {
                    DateTime now = DateTime.Now;

                    int y = 0;
                    foreach (DataRow row in sales.Rows)
                    {
                        string casher = (string)row[0];
                        string pos = row[1].ToString();
                        string cash = row[2].ToString();
                        string smena = row[3].ToString();
                        string artikul = row[4].ToString();
                        string name = (string)row[5];
                        string classifid = row[6].ToString();
                        string classif = row[7].ToString();

                        decimal qty = (decimal)row[8];
                        double nqty = Double.Parse(qty.ToString());

                        decimal summ = (decimal)row[9];
                        double nsumm = Double.Parse(summ.ToString());


                        UKMSoldGoods uk = new UKMSoldGoods();
                        uk.Casher = casher;
                        uk.POSN = pos;
                        uk.CASH = cash;
                        uk.Smena = smena;
                        uk.KassaSmena = cash + "/" + smena;
                        uk.Artikul = artikul;
                        uk.GoodName = name;
                        uk.ClassifId = classifid;
                        uk.ClassifName = classif;
                        uk.Sold = nqty;
                        uk.Summ = nsumm;

                        uk.TimeStamp = now;
                        context.UKMSoldGoods.InsertOnSubmit(uk);
                        if (y == 100)
                        {
                            y = 0;
                            context.SubmitChanges();
                        }

                        newPriceCounter++;
                    }
                    context.SubmitChanges();
                }
                else
                {
                    return ret += "Нет продаж. Можно воспользоваться остатками из,,в САП или повторите позднее... ";
                }

                if (newPriceCounter != 0)
                    ret += "Загружено из УКМ данных с количеством строк " + newPriceCounter.ToString() + ". ";
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
            // -  по новому


            return ret;
        }

        protected void RefreshLinkButton1_Click(object sender, EventArgs e)
        {
            Wizard1.ActiveStepIndex = 0;
        }

    }


    public class MyBackendConfig2 : IDestinationConfiguration
    {
        SaalutDataClasses1DataContext contextD;

        public RfcConfigParameters GetParameters(String destinationName)
        {
            if (contextD == null)
                contextD = new SaalutDataClasses1DataContext();

            if ("AEP".Equals(destinationName))
            {
                var settingsSP = (from s in contextD.SettingsSAPERPTbls
                                  select s).FirstOrDefault();

                RfcConfigParameters parms = new RfcConfigParameters();
                parms.Add(RfcConfigParameters.Name, settingsSP.SystemID);
                parms.Add(RfcConfigParameters.AppServerHost, settingsSP.MessageServerHost);
                if (settingsSP.LogonGroup != null || settingsSP.LogonGroup != "")
                    parms.Add(RfcConfigParameters.LogonGroup, settingsSP.LogonGroup);
                parms.Add(RfcConfigParameters.SystemID, settingsSP.SystemID);
                parms.Add(RfcConfigParameters.SystemNumber, settingsSP.SystemNumber);
                if (settingsSP.SAPRouter != null || settingsSP.SAPRouter != "")
                    parms.Add(RfcConfigParameters.SAPRouter, settingsSP.SAPRouter);
                parms.Add(RfcConfigParameters.User, settingsSP.SAPUser);
                parms.Add(RfcConfigParameters.Password, settingsSP.SAPPassword);
                parms.Add(RfcConfigParameters.Client, settingsSP.Client);
                parms.Add(RfcConfigParameters.Language, "en");
                parms.Add(RfcConfigParameters.PoolSize, "5");
                parms.Add(RfcConfigParameters.MaxPoolSize, "10");
                parms.Add(RfcConfigParameters.IdleTimeout, "600");
                return parms;
            }
            else return null;
        }

        public bool ChangeEventsSupported()
        { return false; }
        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;
    }

}