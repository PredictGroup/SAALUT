using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using System.Data.SqlTypes;
using System.Text;
using MySql.Data.Common;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Transactions;
using System.Web.Configuration;

namespace Saalut
{
    public partial class WeightRep : System.Web.UI.Page
    {

        // Connection string for a typical local MySQL installation
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;
        SaalutDataClasses1DataContext context;

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void SubmitButton1_Click(object sender, EventArgs e)
        {
            OutInfoLiteral1.Text = "";

            if (context == null)
                context = new SaalutDataClasses1DataContext();

            string findStr = ForFindTextBox1.Text;

            var goodArt = (from g in context.Goods
                           where g.Articul == findStr
                           select g).FirstOrDefault();
            if (goodArt == null)
            {
                Int32 findInt = 0;
                Int32.TryParse(findStr, out findInt);

                var goodPlu = (from g in context.Goods
                               where g.PLU == findInt
                               select g).FirstOrDefault();
                if (goodPlu == null)
                {
                    var barcode = (from b in context.Barcodes
                                   where b.Barcode1 == findStr
                                   select b).FirstOrDefault();
                    if (barcode == null)
                    {
                        OutInfoLiteral1.Text = "Товар не найден, попробуйте найти по Артикулу или Штрих-коду или обратитесь в ИТ службу.";
                    }
                    else
                    {
                        UKMDataBaseConnects utl = new UKMDataBaseConnects();
                        utl.UpdateGood(barcode.Good, barcode.Good.Articul);
                        //2
                        utl.UpdateGood(barcode.Good, barcode.Good.Articul);
                        GoodInfo(barcode.Good);
                    }
                }
                else
                {
                    UKMDataBaseConnects utl = new UKMDataBaseConnects();
                    utl.UpdateGood(goodPlu, goodPlu.Articul);
                    //2
                    utl.UpdateGood(goodPlu, goodPlu.Articul);
                    GoodInfo(goodPlu);
                }
            }
            else
            {
                UKMDataBaseConnects utl = new UKMDataBaseConnects();
                utl.UpdateGood(goodArt, goodArt.Articul);
                //2
                utl.UpdateGood(goodArt, goodArt.Articul);
                GoodInfo(goodArt);
            }

        }

        protected void GoodInfo(Good good)
        {
            string Division = WebConfigurationManager.AppSettings["Division"];


            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var store = (from s in context.StoreInfos
                         select s).FirstOrDefault();

            OutInfoLiteral1.Text += "Артикул: " + good.Articul + ".<br />";
            OutInfoLiteral1.Text += "Наименование: " + good.Name + ".<br />";
            OutInfoLiteral1.Text += "Подробно: " + good.Descr + ".<br /><br />";

            if (good.PLU == 0)
                OutInfoLiteral1.Text += "Нет PLU на указанный товар в УКМ.<br />";
            else
                OutInfoLiteral1.Text += "PLU: " + good.PLU + ".<br />";


            OutInfoLiteral1.Text += "Ш/К: " + good.Barcode + ".<br />";

            // ищем цену

            // + цена по новому
            DataTable prices;

            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);
                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // Prices
                string cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select item, price, version, deleted from ukmserver.trm_in_pricelist_items where item = '" + good.Articul + "' and pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
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


            string priceVal = "Нет цены на указанный товар в УКМ.<br />";
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
                if (Division == "RF")
                {
                    string pricet = price.ToString();
                    pricet = pricet.Substring(0, pricet.Length - 2);
                    priceVal = "Цена: " + pricet + "<br />";
                }
                else
                    if (Division == "RB")
                    {
                        string pricet = price.ToString();
                        pricet = pricet.Substring(0, pricet.Length - 5);
                        priceVal = "Цена: " + pricet + "<br />";
                    }
            }
            // - новая цена
            OutInfoLiteral1.Text += priceVal;

            if (good.DepartmentID == null)
                OutInfoLiteral1.Text += "Не указан отдел на указанный товар в УКМ.<br />";
            else
            {
                var weights = from w in context.WeightDeparts
                              where w.Active == true
                              && w.DepartmentID == good.DepartmentID
                              select w;
                if (weights.Count() > 0)
                    OutInfoLiteral1.Text += "Товар привязан к следующим весам:<br />";
                else
                    OutInfoLiteral1.Text += "Товар не имеет привязки к весам.<br />";

                foreach (WeightDepart wt in weights)
                {
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

                    OutInfoLiteral1.Text += weightName + "<br />";
                }
            }

        }

    }
}