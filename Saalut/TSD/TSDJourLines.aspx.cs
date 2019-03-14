using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Data.SqlTypes;
using System.Text;
using MySql.Data.Common;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data;
using System.Transactions;

namespace Saalut.TSD
{
    public partial class TSDJourLines : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;
        int jourID = 0;

        // Connection string for a typical local MySQL installation
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            Int32.TryParse(Request.QueryString["ID"], out jourID);

            if (jourID == 0)
                Response.Redirect("~/TSD/Default.aspx");

            JourNumLabel1.Text = "Журнал N " + jourID.ToString();
            Page.Title = "Журнал N " + jourID.ToString();

            JourLinesLinqDataSource1.WhereParameters[0].DefaultValue = jourID.ToString();

            BarcodeTextBox1.Focus();
        }

        protected void BarcodeTextBox1_TextChanged(object sender, EventArgs e)
        {
            string barcodeText = BarcodeTextBox1.Text.Trim();

            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();

            var barcode = (from p in context.Barcodes
                           where p.Barcode1 == barcodeText
                           && p.Active == true // bug fix barkode
                           select p).FirstOrDefault();
            if (barcode == null)
            {
                PriceLabel1.Text = "Ш/К не найден.";
                AddToJournalButton1.Enabled = false;
                PrintRightNow.Enabled = false;
                BarcodeTextBox1.Text = "";
                BarcodeTextBox1.Focus();
                return;
            }


            // цены укм
            // Create a connection object and data adapter
            DataTable prices;
            decimal price = 0;
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);
                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // Prices
                string cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where item = '" + barcode.Good.Articul + "' and pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
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
                        price = (decimal)row[1];
                    }
                }
            }
            catch (MySqlException ex)
            {
                PriceLabel1.Text = "Нет цены.";
                AddToJournalButton1.Enabled = false;
                PrintRightNow.Enabled = false;
                BarcodeTextBox1.Focus();
                return;
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }


            //var price = (from p in context.Prices
            //             where p.GoodID == barcode.GoodID
            //             && p.Active == true
            //             select p).FirstOrDefault();
            //if (price == null)
            //{
            //    PriceLabel1.Text = "Нет цены.";
            //    AddToJournalButton1.Enabled = false;
            //    BarcodeTextBox1.Focus();
            //    return;
            //}

            if (price == 0)
            {
                PriceLabel1.Text = "Нет цены.";
                AddToJournalButton1.Enabled = false;
                PrintRightNow.Enabled = false;
                BarcodeTextBox1.Focus();
                return;
            }

            PriceLabel1.Text = "Текущая цена: " + price.ToString();
            AddToJournalButton1.Enabled = true;
            PrintRightNow.Enabled = true;
            AddToJournalButton1.Focus();
        }

        protected void AddToJournalButton1_Click(object sender, EventArgs e)
        {
            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();

            string barcodeText = BarcodeTextBox1.Text.Trim();

            var barcode = (from p in context.Barcodes
                           where p.Barcode1 == barcodeText
                           && p.Active == true // bug fix barkode
                           select p).FirstOrDefault();
            if (barcode == null)
            {
                PriceLabel1.Text = "Ш/К не найден.";
                return;
            }

            var lineCheck = (from l in context.PriceChangeLine
                             where l.JournalID == jourID
                             && l.GoodID == barcode.GoodID
                             select l).FirstOrDefault();
            if (lineCheck != null)
            {
                PriceLabel1.Text = "Уже в журнале.";
                return;
            }

            PriceChangeLine nl = new PriceChangeLine();
            nl.GoodID = barcode.GoodID;
            nl.JournalID = jourID;
            nl.ItemID_UKM = barcode.Good.Articul;

            //var price = (from p in context.Prices
            //             where p.GoodID == barcode.GoodID
            //             && p.Active == true
            //             select p).FirstOrDefault();
            //if (price == null)
            //{

            //    nl.NewPrice = 0;
            //}
            //else
            //    nl.NewPrice = price.Price1;

            // цены укм
            // Create a connection object and data adapter
            DataTable prices;
            decimal price = 0;
            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);
                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // Prices
                string cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where item = '" + barcode.Good.Articul + "' and pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0 ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
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
                        price = (decimal)row[1];
                    }
                }
            }
            catch (MySqlException ex)
            {
                nl.NewPrice = 0;
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            Double dPrice = 0;
            Double.TryParse(price.ToString(), out dPrice);
            nl.NewPrice = dPrice;

            nl.Active = true;
            nl.TimeStamp = DateTime.Now;
            nl.Akcionniy = false;
            context.PriceChangeLine.InsertOnSubmit(nl);
            context.SubmitChanges();

            PriceLabel1.Text = "";
            BarcodeTextBox1.Text = "";

            JourLinesLinqDataSource1.DataBind();
            JourLinesGridView1.DataBind();
        }

        protected void DeleteBCImageButton1_Click(object sender, ImageClickEventArgs e)
        {
            PriceLabel1.Text = "";
            BarcodeTextBox1.Text = "";
            BarcodeTextBox1.Focus();
        }

        protected void PrintRightNow_Click(object sender, EventArgs e)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            var setting = (from s in context.Settings
                           select s).FirstOrDefault();


            if (setting.MobileTermoPrinterNetPath == null || setting.MobileTermoPrinterNetPath == "")
            {
                PriceLabel1.Text = "Нет настроек печати!";
                return;
            }


            var template = (from t in context.TermoCennic
                            where t.Active == true
                            select t).FirstOrDefault();

            if (template == null)
            {
                PriceLabel1.Text = "Нет шаблона печати!";
                return;
            }



            var store = (from s in context.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();

            string barcodeText = BarcodeTextBox1.Text.Trim();

            var barcode = (from p in context.Barcodes
                           where p.Barcode1 == barcodeText
                           && p.Active == true // bug fix barkode
                           select p).FirstOrDefault();
            if (barcode == null)
            {
                PriceLabel1.Text = "Ш/К не найден.";
                return;
            }

            var lineCheck = (from l in context.PriceChangeLine
                             where l.JournalID == jourID
                             && l.GoodID == barcode.GoodID
                             select l).FirstOrDefault();
            if (lineCheck == null)
            {
                PriceChangeLine nl = new PriceChangeLine();
                nl.GoodID = barcode.GoodID;
                nl.JournalID = jourID;
                nl.ItemID_UKM = barcode.Good.Articul;

                nl.Active = true;
                nl.TimeStamp = DateTime.Now;
                nl.Akcionniy = false;
                context.PriceChangeLine.InsertOnSubmit(nl);
            }

            TermoCennicQuoue cq = new TermoCennicQuoue();
            cq.JournalID = jourID;
            cq.GoodID = barcode.GoodID;
            cq.TermoCennicID = template.ID;
            cq.TimeStamp = DateTime.Now;
            cq.Active = true;
            context.TermoCennicQuoue.InsertOnSubmit(cq);


            context.SubmitChanges();

            PriceLabel1.Text = "";
            BarcodeTextBox1.Text = "";


            PrintRightNow.Enabled = false;
            AddToJournalButton1.Focus();

            PriceLabel1.Text = "Отправлено на печать!";
        }
    }
}