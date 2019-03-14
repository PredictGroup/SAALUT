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
using System.Web.Configuration;

namespace Saalut
{
    public partial class ByFind : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;
        int goodID = 0;
        int jourID = 0;

        string Division = WebConfigurationManager.AppSettings["Division"];


        // Connection string for a typical local MySQL installation
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            if (!IsPostBack)
            {
                if (Session["JourCart"] == null)
                    Session["JourCart"] = new JourCart();

                if (Session["PrintTemplateCart"] == null)
                    Session["PrintTemplateCart"] = new PrintTemplateCart();
            }
        }

        protected void FindButton1_Click(object sender, EventArgs e)
        {
            Session["PrintTemplateCart"] = new PrintTemplateCart();

            string[] textParagraphs = ForFindTextBox1.Text.Split('\n');
            string[] newTP = new string[textParagraphs.Length];

            int i = 0;
            foreach (string str in textParagraphs)
            {
                string strT = str.Replace("\r", "");
                newTP[i] = strT;
                i++;
            }


            var goods = from g in context.Goods
                        where newTP.Contains(g.Articul)
                        select g;

            if (goods.Count() == 0)
            {
                var barcodes = from b in context.Barcodes
                               where newTP.Contains(b.Barcode1)
                               && b.Active == true // bug fix barkode
                               select b.GoodID.Value;
                List<int> listOfIDs = barcodes.ToList();
                var goodsBC = from g in context.Goods
                              where listOfIDs.Contains(g.ID)
                              select g;
                GoodsGridView1.DataSource = goodsBC;
                GoodsGridView1.DataBind();
            }
            else
            {
                GoodsGridView1.DataSource = goods;
                GoodsGridView1.DataBind();
            }

            foreach (Good good in goods)
            {
                AddToCartTemplate(good.ID, 0,good.Country, good.Descr);
            }
        }

        class Barcki
        {
            public string Val { get; set; }
        }

        protected void AddToCartTemplate(int jourLineID, int printTemplID, string proizvoditel, string goodName)
        {
            PrintTemplateCart cart = (PrintTemplateCart)Session["PrintTemplateCart"];

            int i = 0;
            //Boolean inCart = false;
            foreach (PrintTemplateCartItem items in cart)
            {
                if (items.JourLineID == jourLineID)
                {
                    cart.RemoveAt(i);
                    //inCart = true;
                    break;
                }
                i++;
            }

            //if (!inCart)
            {
                PrintTemplateCartItem item = new PrintTemplateCartItem(jourLineID, printTemplID, proizvoditel, goodName);
                cart.Add(item);
            }
        }

        protected void AddToCart(int jourLineID)
        {
            JourCart cart = (JourCart)Session["JourCart"];

            int i = 0;
            Boolean inCart = false;
            foreach (JourCartItem items in cart)
            {
                if (items.JourLineID == jourLineID)
                {
                    cart.RemoveAt(i);
                    inCart = true;
                    break;
                }
                i++;
            }

            if (!inCart)
            {
                JourCartItem item = new JourCartItem(jourLineID);
                cart.Add(item);
            }
        }


        protected void GoodsGridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int LineID = (int)DataBinder.Eval(e.Row.DataItem, "ID");

                var store = (from s in context.StoreInfos
                             where s.Active == true
                             select s).FirstOrDefault();

                var good = (from g in context.Goods
                            where g.ID == LineID
                            select g).FirstOrDefault();

                TextBox ProizvoditelTextBox1 = (TextBox)e.Row.FindControl("ProizvoditelTextBox1");
                TextBox GoodNameTextBox1 = (TextBox)e.Row.FindControl("GoodNameTextBox1");

                // set def value
                if(GoodNameTextBox1.Text == "")
                    GoodNameTextBox1.Text = good.Descr;
                if (ProizvoditelTextBox1.Text == "")
                    ProizvoditelTextBox1.Text = good.Country;

                // шаблон ценников

                DropDownList PrintTemplateDropDownList1 = (DropDownList)e.Row.FindControl("PrintTemplateDropDownList1");
                //int oldTemplateID = 0;
                //Int32.TryParse(PrintTemplateDropDownList1.SelectedValue, out oldTemplateID);

                var printTempls = from t in context.PrintTemplates
                                  select t;

                PrintTemplateDropDownList1.DataSource = printTempls;


                int templateIDInCart = 0;
                PrintTemplateCart templCart = (PrintTemplateCart)Session["PrintTemplateCart"];
                foreach (PrintTemplateCartItem item in templCart)
                {
                    if (item.JourLineID == LineID)
                    {
                        templateIDInCart = item.TemplateID;

                        //if (item.Proizvoditel != ProizvoditelTextBox1.Text)
                        //    ProizvoditelTextBox1.Text = item.Proizvoditel;

                        //if (item.GoodName != GoodNameTextBox1.Text)
                        //    GoodNameTextBox1.Text = item.GoodName;

                        break;
                    }
                }

                if (templateIDInCart == 0)
                {
                    if (good != null)
                    {

                        if (good.PrintTemplateID != null)
                        {
                            templateIDInCart = good.PrintTemplateID.Value;
                        }
                        else
                        {
                            // ищем шаблон на верхнем уровне.
                            int printTemplGroupId = 0;

                            int upGroupID = 0;
                            if (good.Group.GroupRangeID != 0 && good.Group.GroupRangeID != null)
                                upGroupID = good.Group.GroupRangeID.Value;
                            while (upGroupID != 0)
                            {
                                var grp = (from g in context.Groups
                                           where g.ID == upGroupID
                                           select g).FirstOrDefault();
                                if (grp == null)
                                    continue;

                                if (grp.PrintTemplateID == null)
                                {
                                    if (grp.GroupRangeID != 0 && grp.GroupRangeID != null)
                                    {
                                        upGroupID = grp.GroupRangeID.Value;
                                        continue;
                                    }
                                    else
                                        upGroupID = 0;
                                }
                                else
                                {
                                    printTemplGroupId = grp.PrintTemplateID.Value;
                                    break;
                                }

                                if (good.Group.GroupRangeID != 0 && good.Group.GroupRangeID != null)
                                    upGroupID = good.Group.GroupRangeID.Value;
                            }

                            if (printTemplGroupId != 0)
                                templateIDInCart = printTemplGroupId;
                            else
                                templateIDInCart = printTempls.FirstOrDefault().ID;

                        }
                    }
                    else
                        templateIDInCart = printTempls.FirstOrDefault().ID;

                }

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


                string priceVal = "Нет цены";
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
                    if(Division == "RF")
                        priceVal = priceVal.Substring(0, priceVal.Length - 2);
                    else
                        if(Division == "RB")
                            priceVal = priceVal.Substring(0, priceVal.Length - 5);
                }
                // - новая цена
                Label GoodPriceLabel1 = (Label)e.Row.FindControl("GoodPriceLabel1");
                GoodPriceLabel1.Text = priceVal;

                PrintTemplateDropDownList1.SelectedValue = templateIDInCart.ToString();
                PrintTemplateDropDownList1.DataBind();


                AddToCartTemplate(LineID, templateIDInCart, ProizvoditelTextBox1.Text, GoodNameTextBox1.Text);
                //- конец шаблоны ценников
            }
        }

        protected void CennicButton1_Click(object sender, EventArgs e)
        {
            int newJournalID = 0;
            DateTime tst = DateTime.Now;

            // создадим темповый журнал
            if (GoodsGridView1.Rows.Count != 0)
            {
                PriceChangeJour np = new PriceChangeJour();
                np.Change_log_id_UKM = 0;
                np.Order_no = 0;
                np.InUse = false;
                np.TimeStamp = tst;
                np.Active = true;
                context.PriceChangeJours.InsertOnSubmit(np);
                context.SubmitChanges();
            }
            else
                return;

            var journTMP = (from t in context.PriceChangeJours
                            where t.TimeStamp == tst
                            select t).FirstOrDefault();
            newJournalID = journTMP.ID;


            PrintTemplateCart templCart = (PrintTemplateCart)Session["PrintTemplateCart"];
            foreach (PrintTemplateCartItem item in templCart)
            {
                var good = (from g in context.Goods
                            where g.ID == item.JourLineID
                            select g).FirstOrDefault();
                if (good == null)
                    continue;

                if (CheckBox1WhithoutPrice.Checked == true)
                {
                    var prs = (from p in context.Prices
                               where p.GoodID == good.ID
                               && p.Active == true
                               select p).FirstOrDefault();
                    if (prs == null)
                        continue;
                }

                PriceChangeLine nl = new PriceChangeLine();
                nl.Good = good;
                nl.JournalID = newJournalID;
                nl.ItemID_UKM = good.Articul;
                Price price = (from p in context.Prices
                               where p.GoodID == good.ID
                               && p.Active == true
                               select p).FirstOrDefault();
                if (price != null)
                    nl.NewPrice = price.Price1.Value;
                else
                    nl.NewPrice = 0;
                nl.Active = true;
                nl.TimeStamp = tst;
                context.PriceChangeLine.InsertOnSubmit(nl);
            }
            context.SubmitChanges();


            //foreach (GridViewRow row in GoodsGridView1.Rows)
            //{
            //    Label GoodIDLabel1 = (Label)row.FindControl("GoodIDLabel1");
            //    DropDownList PrintTemplateDropDownList1 = (DropDownList)row.FindControl("PrintTemplateDropDownList1");

            //    int goodid = Int32.Parse(GoodIDLabel1.Text);
            //    int printTempl = Int32.Parse(PrintTemplateDropDownList1.SelectedValue);

            //    var good = (from g in context.Goods
            //                where g.ID == goodid
            //                select g).FirstOrDefault();
            //    if (good == null)
            //        continue;

            //    PriceChangeLine nl = new PriceChangeLine();
            //    nl.Good = good;
            //    nl.JournalID = newJournalID;
            //    nl.ItemID_UKM = good.Articul;
            //    Price price = (from p in context.Prices
            //                   where p.GoodID == goodid
            //                   && p.Active == true
            //                   select p).FirstOrDefault();
            //    if (price != null)
            //        nl.NewPrice = price.Price1.Value;
            //    else
            //        nl.NewPrice = 0;
            //    nl.Active = true;
            //    nl.TimeStamp = tst;
            //    context.PriceChangeLine.InsertOnSubmit(nl);
            //}
            //context.SubmitChanges();


            string url = "CennicList.aspx?ID=" + newJournalID.ToString();
            ////string redirectScript = "<script>window.open('" + url + "');</script>";
            ////Response.Write(redirectScript);


            Response.Redirect(url);
        }

        protected void PrintTemplateDropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl = (DropDownList)sender;
            string val = ddl.SelectedItem.Value;

            GridViewRow gvr = (GridViewRow)(((Control)sender).NamingContainer);

            // Get the reference of this DropDownlist
            Label IDLineDDLLabel1 = (Label)gvr.FindControl("IDLineDDLLabel1");

            int printID = 0;
            Int32.TryParse(val, out printID);
            int jrlID = 0;
            Int32.TryParse(IDLineDDLLabel1.Text, out jrlID);


            TextBox ProizvoditelTextBox1 = (TextBox)gvr.FindControl("ProizvoditelTextBox1");
            TextBox GoodNameTextBox1 = (TextBox)gvr.FindControl("GoodNameTextBox1");

            AddToCartTemplate(jrlID, printID, ProizvoditelTextBox1.Text, GoodNameTextBox1.Text);

        }

        protected void GoodsGridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            string[] textParagraphs = ForFindTextBox1.Text.Split('\n');
            string[] newTP = new string[textParagraphs.Length];

            int i = 0;
            foreach (string str in textParagraphs)
            {
                string strT = str.Replace("\r", "");
                newTP[i] = strT;
                i++;
            }


            var goods = from g in context.Goods
                        where newTP.Contains(g.Articul)
                        select g;

            if (goods.Count() == 0)
            {
                var barcodes = from b in context.Barcodes
                               where newTP.Contains(b.Barcode1)
                               && b.Active == true
                               select b.GoodID.Value;
                List<int> listOfIDs = barcodes.ToList();
                var goodsBC = from g in context.Goods
                              where listOfIDs.Contains(g.ID)
                              select g;
                GoodsGridView1.DataSource = goodsBC;
                GoodsGridView1.DataBind();
            }
            else
            {
                GoodsGridView1.DataSource = goods;
                GoodsGridView1.DataBind();
            }

            GoodsGridView1.PageIndex = e.NewPageIndex;
            GoodsGridView1.DataBind();
        }

        protected void SetAllDDLButton1_Click(object sender, EventArgs e)
        {
            int pID = 0;
            Int32.TryParse(SomeCennicsDropDownList1.SelectedValue, out pID);

            string[] textParagraphs = ForFindTextBox1.Text.Split('\n');
            string[] newTP = new string[textParagraphs.Length];

            int i = 0;
            foreach (string str in textParagraphs)
            {
                string strT = str.Replace("\r", "");
                newTP[i] = strT;
                i++;
            }

            var goods = from g in context.Goods
                        where newTP.Contains(g.Articul)
                        select g;

            foreach (Good good in goods)
            {
                AddToCartTemplate(good.ID, pID, good.Producer, good.Descr);
            }

            if (goods.Count() == 0)
            {
                var barcodes = from b in context.Barcodes
                               where newTP.Contains(b.Barcode1)
                               && b.Active == true
                               select b.GoodID.Value;
                List<int> listOfIDs = barcodes.ToList();
                var goodsBC = from g in context.Goods
                              where listOfIDs.Contains(g.ID)
                              select g;
                GoodsGridView1.DataSource = goodsBC;
                GoodsGridView1.DataBind();
            }
            else
            {
                GoodsGridView1.DataSource = goods;
                GoodsGridView1.DataBind();
            }

        }


        protected void TermoCennicButton1_Click(object sender, EventArgs e)
        {
            int newJournalID = 0;
            DateTime tst = DateTime.Now;

            // создадим темповый журнал
            if (GoodsGridView1.Rows.Count != 0)
            {
                PriceChangeJour np = new PriceChangeJour();
                np.Change_log_id_UKM = 0;
                np.Order_no = 0;
                np.InUse = false;
                np.TimeStamp = tst;
                np.Active = true;
                context.PriceChangeJours.InsertOnSubmit(np);
                context.SubmitChanges();
            }
            else
                return;

            var journTMP = (from t in context.PriceChangeJours
                            where t.TimeStamp == tst
                            select t).FirstOrDefault();
            newJournalID = journTMP.ID;


            PrintTemplateCart templCart = (PrintTemplateCart)Session["PrintTemplateCart"];
            foreach (PrintTemplateCartItem item in templCart)
            {
                var good = (from g in context.Goods
                            where g.ID == item.JourLineID
                            select g).FirstOrDefault();
                if (good == null)
                    continue;

                PriceChangeLine nl = new PriceChangeLine();
                nl.Good = good;
                nl.JournalID = newJournalID;
                nl.ItemID_UKM = good.Articul;
                Price price = (from p in context.Prices
                               where p.GoodID == good.ID
                               && p.Active == true
                               select p).FirstOrDefault();
                if (price != null)
                    nl.NewPrice = price.Price1.Value;
                else
                    nl.NewPrice = 0;
                nl.Active = true;
                nl.TimeStamp = tst;
                context.PriceChangeLine.InsertOnSubmit(nl);
            }
            context.SubmitChanges();

            var jourlines = from j in context.PriceChangeLine
                            where j.JournalID == newJournalID
                            select j;
            foreach (PriceChangeLine ln in jourlines)
            {
                AddToCart(ln.ID);
            }


            string url = "CennicListTermo.aspx?ID=" + newJournalID.ToString() + "&q=" + QtyTermoPTextBox1.Text + "&f=" + TermPrinterDropDownList1.SelectedValue;
            ////string redirectScript = "<script>window.open('" + url + "');</script>";
            ////Response.Write(redirectScript);


            Response.Redirect(url);
        }

        protected void ProizvoditelTextBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            GridViewRow gvr = (GridViewRow)(((Control)sender).NamingContainer);

            // Get the reference of this DropDownlist
            Label IDLineDDLLabel1 = (Label)gvr.FindControl("IDLineDDLLabel1");
            DropDownList ddl = (DropDownList)gvr.FindControl("PrintTemplateDropDownList1");
            string val = ddl.SelectedItem.Value;

            int printID = 0;
            Int32.TryParse(val, out printID);
            int jrlID = 0;
            Int32.TryParse(IDLineDDLLabel1.Text, out jrlID);


            TextBox ProizvoditelTextBox1 = (TextBox)gvr.FindControl("ProizvoditelTextBox1");
            TextBox GoodNameTextBox1 = (TextBox)gvr.FindControl("GoodNameTextBox1");

            AddToCartTemplate(jrlID, printID, tb.Text, GoodNameTextBox1.Text);
        }

        protected void GoodNameTextBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;

            GridViewRow gvr = (GridViewRow)(((Control)sender).NamingContainer);

            // Get the reference of this DropDownlist
            Label IDLineDDLLabel1 = (Label)gvr.FindControl("IDLineDDLLabel1");
            DropDownList ddl = (DropDownList)gvr.FindControl("PrintTemplateDropDownList1");
            string val = ddl.SelectedItem.Value;

            int printID = 0;
            Int32.TryParse(val, out printID);
            int jrlID = 0;
            Int32.TryParse(IDLineDDLLabel1.Text, out jrlID);


            TextBox ProizvoditelTextBox1 = (TextBox)gvr.FindControl("ProizvoditelTextBox1");
            TextBox GoodNameTextBox1 = (TextBox)gvr.FindControl("GoodNameTextBox1");

            AddToCartTemplate(jrlID, printID, ProizvoditelTextBox1.Text, tb.Text);
        }

    }
}