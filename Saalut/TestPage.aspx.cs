using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using MySql.Data.Common;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data;
using System.Transactions;

namespace Saalut
{
    public partial class TestPage : System.Web.UI.Page
    {
        // Connection string for a typical local MySQL installation
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;


        protected void Page_Load(object sender, EventArgs e)
        {

            SaalutDataClasses1DataContext context = new SaalutDataClasses1DataContext();
            CheckBoxList chb = new CheckBoxList();
            chb.ID = "WTCheckBoxList1";

            var weights = from w in context.WeightDeparts
                          where w.Active == true
                          orderby w.Num
                          select w;

            string lastW = "";
            foreach (WeightDepart wt in weights)
            {
                if (lastW == wt.Num)
                    continue;

                string weightName = "Весы №" + wt.Num + " ";
                var wds = from w in context.WeightDeparts
                          where w.Num == wt.Num
                          && w.Active == true
                          select w;
                foreach (WeightDepart wtd in wds)
                {
                    var dept = (from d in context.Departments
                                where d.ID == wtd.DepartmentID
                                && d.Active == true
                                select d).FirstOrDefault();

                    if (dept == null)
                        continue;

                    weightName += dept.DepartName + " ";
                }

                ListItem itm = new ListItem();
                itm.Value = wt.Num;
                itm.Text = weightName;
                chb.Items.Add(itm);

                lastW = wt.Num;
            }

            if (weights.Count() > 0)
                WeightsPlaceHolder1.Controls.Add(chb);

        }

        protected void SelectTestButton1_Click(object sender, EventArgs e)
        {
            UKMDataBaseConnects ukm = new UKMDataBaseConnects();
            MessageTextBox1.Text = ukm.InitialDB();
        }

        protected void LoadJoursButton1_Click(object sender, EventArgs e)
        {
            UKMDataBaseConnects ukm = new UKMDataBaseConnects();
            MessageTextBox1.Text = ukm.ReadNewData_from_client();
        }

        protected void ExportToUKMButton1_Click(object sender, EventArgs e)
        {
            WeighingEquipUtils we = new WeighingEquipUtils();
            MessageTextBox1.Text = we.ExportAllToWE();
        }

        protected void Start2002Button1_Click(object sender, EventArgs e)
        {
            // если процесс выгрузки на весы не запущен, запускаем его.
            if (!IsProcessOpen("SiS2002.exe"))
            {
                Process.Start("C:\\Program Files\\DIGI\\SIS2002\\SiS2002.exe");
            }
        }

        public bool IsProcessOpen(string name)
        {
            //here we're going to get a list of all running processes on
            //the computer
            foreach (Process clsProcess in Process.GetProcesses())
            {
                //now we're going to see if any of the running processes
                //match the currently running processes. Be sure to not
                //add the .exe to the name you provide, i.e: NOTEPAD,
                //not NOTEPAD.EXE or false is always returned even if
                //notepad is running.
                //Remember, if you have the process running more than once, 
                //say IE open 4 times the loop thr way it is now will close all 4,
                //if you want it to just close the first one it finds
                //then add a return; after the Kill
                if (clsProcess.ProcessName.StartsWith(name))
                {
                    //if the process is found to be running then we
                    //return a true
                    return true;
                }
            }
            //otherwise we return a false
            return false;
        }

        protected void ThermolabelButton1_Click(object sender, EventArgs e)
        {
            TermLabelUtils utl = new TermLabelUtils();
            utl.PrintTermoLabel(20791, 1, 3, 0, "01.01.2012");
        }

        protected void DirectExportToWeightButton1_Click(object sender, EventArgs e)
        {
            WeighingEquipUtils utl = new WeighingEquipUtils();
            utl.ExportFromUKMDirect();
        }

        protected void DirectExportToWeightQload_Click(object sender, EventArgs e)
        {
            WeighingEquipUtils utl = new WeighingEquipUtils();
            utl.ExportAllToQload();
        }

        protected void LoadQWeighButton1_Click(object sender, EventArgs e)
        {
            MessageTextBox1.Text = "";

            WeighingEquipUtils utl = new WeighingEquipUtils();

            CheckBoxList chb = (CheckBoxList)WeightsPlaceHolder1.FindControl("WTCheckBoxList1");

            if (chb == null)
                return;

            foreach (ListItem itm in chb.Items)
            {
                if (itm.Selected)
                {
                    MessageTextBox1.Text += utl.ExportToWE_qload_by_weights_UKMPrice_by_format(itm.Value) + "  ";
                }
            }
        }

        protected void Button1QuiqInit_Click(object sender, EventArgs e)
        {
            UKMDataBaseConnects utl = new UKMDataBaseConnects();
            utl.UKM_Quick_LoadsToDB();
        }

        protected void LoadArtButton2_Click(object sender, EventArgs e)
        {
            SaalutDataClasses1DataContext context = new SaalutDataClasses1DataContext();

            var good = (from g in context.Goods
                        where g.Articul == ArtTextBox1.Text
                        select g).FirstOrDefault();

            UKMDataBaseConnects utl = new UKMDataBaseConnects();
            utl.UpdateGood(good, ArtTextBox1.Text);
            //2
            utl.UpdateGood(good, ArtTextBox1.Text);
        }

        protected void btnInitAkcionnieCeny_Click(object sender, EventArgs e)
        {
            string ret = "";

        
            SaalutDataClasses1DataContext context = new SaalutDataClasses1DataContext();

            //-------------------------------

            var delStoreInfo = (from d in context.StoreInfos
                               select d).FirstOrDefault();

            string jurCo = delStoreInfo.Company;
            string jurF = delStoreInfo.AddressFact;
            context.StoreInfos.DeleteOnSubmit(delStoreInfo);
            context.SubmitChanges();

            UKMDataBaseConnects utl = new UKMDataBaseConnects();
            utl.InitialStoreInfoIns();

            var newStoreInfo = (from d in context.StoreInfos
                                select d).FirstOrDefault();

            newStoreInfo.Company = jurCo;
            newStoreInfo.AddressFact = jurF;
            context.SubmitChanges();
            //---------------------------------

            var pricesAkcion = (from p in context.PricesAkcionnies
                                select p).FirstOrDefault();
            if (pricesAkcion != null)
                return;

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();

            CommittableTransaction tx = new CommittableTransaction();

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // = делаем текущий прайс лист магазина


                // Выбираем магазин
                string cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select item, price, version, deleted from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.ActPriceList_ID_UKM.ToString() + "'  and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                DataTable pricesAkcionnie = ds.Tables[0];
                if (pricesAkcionnie != null)
                {
                    foreach (DataRow row1 in pricesAkcionnie.Rows)
                    {
                        string artikulA = (string)row1[0];
                        decimal priceA = (decimal)row1[1];
                        int version_priceA = (int)row1[2];
                        bool delete_priceA = (bool)row1[3];


                        double newPriceA = Double.Parse(priceA.ToString());

                        var good = (from g in context.Goods
                                    where g.Articul == artikulA
                                    select g).FirstOrDefault();

                        if (good == null)
                            continue;

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

            return;
        }

        protected void InitMatWeightButton1_Click(object sender, EventArgs e)
        {
            UKMDataBaseConnects ukm = new UKMDataBaseConnects();
            MessageTextBox1.Text = ukm.InitialMatWeightDB();
        }


    }
}