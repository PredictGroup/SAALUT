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
    public partial class UKM_SAPERP_CheckPrices : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;


        // Connection string for a typical local MySQL installation
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;

        string connStrSQL = global::System.Configuration.ConfigurationManager.ConnectionStrings["SaalutExpressConnectionString"].ConnectionString;



        protected void Page_Load(object sender, EventArgs e)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();
        }

        protected void SubmitButton1_Click(object sender, EventArgs e)
        {

            // загружаем данные из ЕРП
            LoadDataFromSAPERP();

            // загружаем цены из укм
            NewJourPrice_load();



            //Connect to the database for Collections Table
            SqlConnection thisConnection = new SqlConnection(connStrSQL);

            //Create DataAdapter Object
            SqlDataAdapter thisAdapter = new SqlDataAdapter("  SELECT ROW_NUMBER() OVER (ORDER BY [MATNR]) as NUM, g.[Name] Name, s.[MATNR] MATNR     ,s.[KBETR] KBETR     ,s.[KSCHL] KSCHL ,s.[ASSORT] ASSORT    ,u.[Artikul] Artikul     ,u.[PriceUKM] PriceUKM    FROM dbo.SAPPriceTbl s, dbo.UKMPriceTbl u, dbo.Goods g where  s.[MATNR] = u.[Artikul] and s.[KBETR] <> u.[PriceUKM] and u.[Artikul] = g.[Articul]", thisConnection);

            // Create a fill a Dataset
            DataSet ds = new DataSet();
            thisAdapter.Fill(ds);


            if (ds.Tables[0].Rows.Count != 0)
            {
                object objSumSap;
                objSumSap = ds.Tables[0].Compute("Sum(KBETR)", "");

                object objSumUKM;
                objSumUKM = ds.Tables[0].Compute("Sum(PriceUKM)", "");


                GridView ReportGridView1 = new GridView();
                ReportGridView1.Width = new Unit(950);
                ReportGridView1.AutoGenerateColumns = false;

                BoundField fld0 = new System.Web.UI.WebControls.BoundField();
                fld0.HeaderText = "N";
                fld0.DataField = "NUM";
                ReportGridView1.Columns.Add(fld0);

                BoundField fld3 = new System.Web.UI.WebControls.BoundField();
                fld3.HeaderText = "Арт.";
                fld3.DataField = "MATNR";
                ReportGridView1.Columns.Add(fld3);


                BoundField fld4 = new System.Web.UI.WebControls.BoundField();
                fld4.HeaderText = "Наименование";
                fld4.DataField = "NAME";
                ReportGridView1.Columns.Add(fld4);

                BoundField fld5 = new System.Web.UI.WebControls.BoundField();
                fld5.HeaderText = "Вид условия";
                fld5.DataField = "KSCHL";
                ReportGridView1.Columns.Add(fld5);

                BoundField fld51 = new System.Web.UI.WebControls.BoundField();
                fld51.HeaderText = "В ассорт.";
                fld51.DataField = "ASSORT";
                ReportGridView1.Columns.Add(fld51);


                BoundField fld6 = new System.Web.UI.WebControls.BoundField();
                fld6.HeaderText = "SAP цена руб.";
                fld6.DataField = "KBETR";
                fld6.DataFormatString = "{0:N}";
                ReportGridView1.Columns.Add(fld6);

                BoundField fld7 = new System.Web.UI.WebControls.BoundField();
                fld7.HeaderText = "УКМ цена руб.";
                fld7.DataField = "PriceUKM";
                fld7.DataFormatString = "{0:N}";
                ReportGridView1.Columns.Add(fld7);

                ReportGridView1.DataSource = ds.Tables[0];
                ReportGridView1.DataBind();
                ReportPlaceHolder1.Controls.Add(ReportGridView1);

                Label lblGroup = new Label();
                lblGroup.Text = "Итоги: SAP: " + objSumSap.ToString() + "  УКМ: " + objSumUKM.ToString() + ".";
                ReportPlaceHolder1.Controls.Add(lblGroup);
            }
            else
            {
                ErrorLabel1.Text = "В выборке нет данных, либо в отчете нет расхождений.";
            }


            //Close Connection
            thisConnection.Close();

            Wizard1.ActiveStepIndex = 1;
        }

        public void LoadDataFromSAPERP()
        {
            string werk = "";
            var settingsSP = (from s in context.SettingsSAPERPTbls
                              select s).FirstOrDefault();
            werk = settingsSP.Werk;

            if (werk == null || werk == "")
            {
                ErrorLabel1.Text = "Не указан в настройках завод магазина";
                Wizard1.ActiveStepIndex = 1;
                return;
            }

            MyBackendConfig cfg = new MyBackendConfig();
            try
            {
                // delete all
                var sapPricesDel = from p in context.SAPPriceTbls
                                   select p;
                context.SAPPriceTbls.DeleteAllOnSubmit(sapPricesDel);
                context.SubmitChanges();
                //-----------------------------

                RfcDestinationManager.RegisterDestinationConfiguration(cfg);//1             
                RfcDestination prd = RfcDestinationManager.GetDestination("AEP");//2  


                RfcRepository repo = prd.Repository;//3                                     
                IRfcFunction pricesBapi =
                    repo.CreateFunction("Y_GET_MATNR_PRICE");//4                               
                pricesBapi.SetValue("I_WERKS", werk); //5                 
                pricesBapi.Invoke(prd); //6                 
                IRfcTable detail = pricesBapi.GetTable("TAB");

                int i = 0;
                foreach (IRfcStructure elem in detail)
                {
                    string matnr = elem[0].GetString();
                    double kbetr = elem[1].GetDouble();
                    string kschl = elem[2].GetString();
                    string assort = elem[3].GetString();

                    SAPPriceTbl np = new SAPPriceTbl();
                    np.MATNR = matnr;
                    np.KBETR = kbetr;
                    np.KSCHL = kschl;
                    np.ASSORT = assort;
                    context.SAPPriceTbls.InsertOnSubmit(np);

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
                ErrorLabel1.Text = "Проблема повторного подключения к SAP";
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
                ErrorLabel1.Text = "Сетевая проблема подключения к SAP";
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
                ErrorLabel1.Text = "Не правильный пользователь для подключения к SAP";
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
                ErrorLabel1.Text = "Серьезная проблема на стороне SAP обратитесь в службу поддержки SAP (serious problem on ABAP system side...)";
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
                ErrorLabel1.Text = "Серьезная проблема на стороне SAP обратитесь в службу поддержки SAP (The function module returned an ABAP exception, an ABAP message)";
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


        public string NewJourPrice_load()
        {
            string ret = "";
            int newPriceCounter = 0;

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            // Del
            var ukmprices = from p in context.UKMPriceTbls
                            select p;
            context.UKMPriceTbls.DeleteAllOnSubmit(ukmprices);
            context.SubmitChanges();
            //-


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



                // Prices
                string cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select item, price, version, deleted from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds5 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds5);

                prices = ds5.Tables[0];

                if (prices != null)
                {
                    int y = 0;
                    foreach (DataRow row in prices.Rows)
                    {
                        string artikul = (string)row[0];
                        decimal price = (decimal)row[1];
                        int version_price = (int)row[2];
                        bool delete_price = (bool)row[3];


                        double newPrice = Double.Parse(price.ToString());


                        DateTime now = DateTime.Now;

                        // Price tbl
                        UKMPriceTbl npr = new UKMPriceTbl();
                        npr.Artikul = artikul;
                        npr.PriceUKM = newPrice;
                        npr.Version = version_price;
                        npr.TimeStamp = now;
                        context.UKMPriceTbls.InsertOnSubmit(npr);
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
                    return ret += "Нет цен. Повторите позднее... ";
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
            // - цена по новому


            return ret;
        }

        protected void RefreshLinkButton1_Click(object sender, EventArgs e)
        {
            Wizard1.ActiveStepIndex = 0;
        }

    }

    public class MyBackendConfig : IDestinationConfiguration
    {
        SaalutDataClasses1DataContext context;

        public RfcConfigParameters GetParameters(String destinationName)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            if ("AEP".Equals(destinationName))
            {
                var settingsSP = (from s in context.SettingsSAPERPTbls
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