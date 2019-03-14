using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.Common;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data;
using System.Transactions;

namespace Saalut
{
    public partial class TestPage2 : System.Web.UI.Page
    {
                SaalutDataClasses1DataContext context;

        // Connection string for a typical local MySQL installation
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            context = new SaalutDataClasses1DataContext();

            string ret = "";

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return;

            // Create a connection object and data adapter
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                MySqlCommand cmd = new MySqlCommand("SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ;", cnx);
                cnx.Open();
                cmd.ExecuteNonQuery();




                //------------------------------------

                var goods = from g in context.Goods
                            select g;

                // items
                string cmdText = "select id, name, descr, measure, classif, version, deleted from ukmserver.trm_in_items where id in ('9001373','9001452','9001515','9002997','9002999','9003013','9003015','9003019','9003020','9003024','9003027','9003028','9003030','9003031','9003032','9003033','9003035','9003036','9003037','9003039','9003040','9003111','9003164','9003165','9003176','9003292','9003296','9003304','9003317','9003346','9003349','9003358','9003359') ";
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
                    UKMDataBaseConnects utl = new UKMDataBaseConnects();
                    if (good == null)
                    {
                        // товара нету - создаем
                        
                        utl.CreateNewGood(item, barcodes, assorts, structures, prices, printTemplGood);
                    }
                    else
                    {
                        utl.UpdateGood(good, item, barcodes, assorts, structures, prices, printTemplGood);
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

            return;
        }

        protected void LoadArtButton2_Click(object sender, EventArgs e)
        {
            context = new SaalutDataClasses1DataContext();

            var good = (from g in context.Goods
                       where g.Articul == ArtTextBox1.Text
                       select g).FirstOrDefault();

            UKMDataBaseConnects utl = new UKMDataBaseConnects();
            utl.UpdateGood(good, ArtTextBox1.Text);
        }
    }
}