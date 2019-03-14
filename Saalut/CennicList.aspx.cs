using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
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

namespace Saalut
{
    public partial class CennicList : System.Web.UI.Page
    {
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;

        SaalutDataClasses1DataContext context;
        int jourID = 0;

        bool WhithoutPrice = false;

        protected void Page_Load_old(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Dictionary<int, int> groupTemlates = new Dictionary<int, int>();


                context = new SaalutDataClasses1DataContext();

                if (Request.QueryString["ID"] == null)
                    //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
                    Response.Redirect("NewJours.aspx");

                Int32.TryParse(Request.QueryString["ID"], out jourID);

                if (jourID == 0)
                    //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
                    Response.Redirect("NewJours.aspx");


                if (Request.QueryString["WP"] != null)
                {
                    int tmpI = 0;
                    Int32.TryParse(Request.QueryString["WP"], out tmpI);
                    if (tmpI == 1)
                        WhithoutPrice = true;
                }


                //if (Session["JourCart"] == null)
                //    //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true); 
                //    Response.Redirect("NewJours.aspx");

                var jLines = from j in context.PriceChangeLine
                             where j.JournalID == jourID
                             select j;
                foreach (PriceChangeLine ln in jLines)
                {

                    if (WhithoutPrice == true)
                    {
                        var prs = (from p in context.Prices
                                   where p.GoodID == ln.GoodID
                                   && p.Active == true
                                   select p).FirstOrDefault();
                        if (prs == null)
                            continue;
                    }

                    JourCart cart = (JourCart)Session["JourCart"];
                    foreach (JourCartItem items in cart)
                    {
                        if (items.JourLineID == ln.ID)
                        {
                            // входит в список распечатки.
                            groupTemlates.Add(ln.ID, FindTemplate(ln.ID));

                            break;
                        }
                    }
                }


                // Выбираем данные из УКМ по ценам и сохраняем их в таблицу
                var store = (from s in context.StoreInfos
                             where s.Active == true
                             select s).FirstOrDefault();
                if (store == null)
                    return;



                //------
                // выводим на экран с группировкой по типу ценника
                int currCountCennic = 0;
                foreach (var item in groupTemlates.OrderBy(key => key.Value))
                {

                    int templateIDInCart = 0;
                    PrintTemplateCart templCart = (PrintTemplateCart)Session["PrintTemplateCart"];
                    foreach (PrintTemplateCartItem itemTempl in templCart)
                    {
                        if (itemTempl.JourLineID == item.Key)
                        {
                            templateIDInCart = itemTempl.TemplateID;

                            break;
                        }
                    }

                    Image img = new Image();
                    if (templateIDInCart == 0)
                        img.ImageUrl = "Services/Cennic.ashx?LID=" + item.Key.ToString() + "&TID=" + item.Value.ToString();
                    else
                        img.ImageUrl = "Services/Cennic.ashx?LID=" + item.Key.ToString() + "&TID=" + templateIDInCart.ToString();

                    CennicPlaceHolder1.Controls.Add(img);

                    currCountCennic++;
                }

                currCountCennic = 0;
                if (groupTemlates.Count == 0)
                {
                    var lines = from l in context.PriceChangeLine
                                where l.JournalID == jourID
                                && l.Active == true
                                select l;

                    foreach (PriceChangeLine line in lines)
                    {
                        if (WhithoutPrice == true)
                        {
                            var prs = (from p in context.Prices
                                       where p.GoodID == line.GoodID
                                       && p.Active == true
                                       select p).FirstOrDefault();
                            if (prs == null)
                                continue;
                        }


                        int templateIDInCart = 0;
                        PrintTemplateCart templCart = (PrintTemplateCart)Session["PrintTemplateCart"];
                        foreach (PrintTemplateCartItem itemTempl in templCart)
                        {
                            if (itemTempl.JourLineID == line.GoodID)
                            {
                                templateIDInCart = itemTempl.TemplateID;

                                break;
                            }
                        }
                        Image img = new Image();
                        img.ImageUrl = "Services/Cennic.ashx?LID=" + line.ID.ToString() + "&TID=" + templateIDInCart.ToString();

                        CennicPlaceHolder1.Controls.Add(img);

                        currCountCennic++;

                    }
                }

            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string Division = WebConfigurationManager.AppSettings["Division"];

            if (!IsPostBack)
            {
                Dictionary<int, int> groupTemlates = new Dictionary<int, int>();

                context = new SaalutDataClasses1DataContext();

                if (Request.QueryString["ID"] == null)
                    //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
                    Response.Redirect("NewJours.aspx");

                Int32.TryParse(Request.QueryString["ID"], out jourID);

                if (jourID == 0)
                    //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
                    Response.Redirect("NewJours.aspx");


                if (Request.QueryString["WP"] != null)
                {
                    int tmpI = 0;
                    Int32.TryParse(Request.QueryString["WP"], out tmpI);
                    if (tmpI == 1)
                        WhithoutPrice = true;
                }

                var store = (from s in context.StoreInfos
                             where s.Active == true
                             select s).FirstOrDefault();
                if (store == null)
                    Response.Redirect("NewJours.aspx");



                //if (Session["JourCart"] == null)
                //    //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true); 
                //    Response.Redirect("NewJours.aspx");

                var jLines = from j in context.PriceChangeLine
                             where j.JournalID == jourID
                             select j;
                foreach (PriceChangeLine ln in jLines)
                {

                    if (WhithoutPrice == true)
                    {
                        //var prs = (from p in context.Prices
                        //           where p.GoodID == ln.GoodID
                        //           && p.Active == true
                        //           select p).FirstOrDefault();
                        //if (prs == null)
                        //    continue;

                        decimal price = 0;

                        MySqlConnection cnx = null;
                        try
                        {
                            DataTable prices;

                            cnx = new MySqlConnection(connStr);
                            MySqlDataAdapter adapter = new MySqlDataAdapter();

                            // Prices
                            string cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and item = '" + ln.Good.Articul.ToString() + "'  and deleted = 0 ";
                            MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                            cmd.CommandTimeout = 30000;

                            // Create a fill a Dataset
                            DataSet ds5 = new DataSet();
                            adapter.SelectCommand = cmd;
                            adapter.Fill(ds5);

                            prices = ds5.Tables[0];
                           
                            if (prices != null)
                            {
                                // select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and deleted = 0
                                DataRow[] prsLsts = prices.Select("item = '" + ln.Good.Articul + "'");

                                foreach (DataRow row in prsLsts)
                                {
                                    price = (decimal)row[1];
                                }
                            }
                        }
                        catch (MySqlException ex)
                        {
                            Response.Redirect("ErrorPage.aspx");
                        }
                        finally
                        {
                            if (cnx != null)
                            {
                                cnx.Close();
                            }
                        }

                        if (price == 0)
                            continue;

                    } // if (WhithoutPrice == true)

                    JourCart cart = (JourCart)Session["JourCart"];
                    foreach (JourCartItem items in cart)
                    {
                        if (items.JourLineID == ln.ID)
                        {
                            // входит в список распечатки.
                            groupTemlates.Add(ln.ID, FindTemplate(ln.ID));

                            break;
                        }
                    }
                }


                //------
                // выводим на экран с группировкой по типу ценника
                int currCountCennic = 0;
                foreach (var item in groupTemlates.OrderBy(key => key.Value))
                {

                    string proizvoditel = "";
                    string goodName = "";

                    int templateIDInCart = 0;
                    PrintTemplateCart templCart = (PrintTemplateCart)Session["PrintTemplateCart"];
                    foreach (PrintTemplateCartItem itemTempl in templCart)
                    {
                        if (itemTempl.JourLineID == item.Key)
                        {
                            templateIDInCart = itemTempl.TemplateID;
                            proizvoditel = itemTempl.Proizvoditel;
                            goodName = itemTempl.GoodName;
                            break;
                        }
                    }

                    Image img = new Image();
                    if (templateIDInCart == 0)
                        img.ImageUrl = "Services/Cennic.ashx?LID=" + item.Key.ToString() + "&TID=" + item.Value.ToString() + "&PID=" + Server.UrlEncode(proizvoditel) + "&GNID=" + Server.UrlEncode(goodName);
                    else
                        img.ImageUrl = "Services/Cennic.ashx?LID=" + item.Key.ToString() + "&TID=" + templateIDInCart.ToString() + "&PID=" + Server.UrlEncode(proizvoditel) + "&GNID=" + Server.UrlEncode(goodName);

                    CennicPlaceHolder1.Controls.Add(img);

                    currCountCennic++;
                }

                currCountCennic = 0;
                if (groupTemlates.Count == 0)
                {
                    var lines = from l in context.PriceChangeLine
                                where l.JournalID == jourID
                                && l.Active == true
                                select l;

                    foreach (PriceChangeLine line in lines)
                    {
                        if (WhithoutPrice == true)
                        {
                            //var prs = (from p in context.Prices
                            //           where p.GoodID == ln.GoodID
                            //           && p.Active == true
                            //           select p).FirstOrDefault();
                            //if (prs == null)
                            //    continue;

                            decimal price = 0;

                            MySqlConnection cnx = null;
                            try
                            {
                                DataTable prices;

                                cnx = new MySqlConnection(connStr);
                                MySqlDataAdapter adapter = new MySqlDataAdapter();

                                // Prices
                                string cmdText = "select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and item = '" + line.Good.Articul.ToString() + "'  and deleted = 0 ";
                                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                                cmd.CommandTimeout = 30000;

                                // Create a fill a Dataset
                                DataSet ds5 = new DataSet();
                                adapter.SelectCommand = cmd;
                                adapter.Fill(ds5);

                                prices = ds5.Tables[0];                            
                                if (prices != null)
                                {
                                    // select item, price, version, deleted	from ukmserver.trm_in_pricelist_items where pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "' and deleted = 0
                                    DataRow[] prsLsts = prices.Select("item = '" + line.Good.Articul + "'");

                                    foreach (DataRow row in prsLsts)
                                    {
                                        price = (decimal)row[1];
                                    }
                                }
                            }
                            catch (MySqlException ex)
                            {
                                Response.Redirect("ErrorPage.aspx");
                            }
                            finally
                            {
                                if (cnx != null)
                                {
                                    cnx.Close();
                                }
                            }

                            if (price == 0)
                                continue;

                        } // if (WhithoutPrice == true)

                        string proizvoditel = "";
                        string goodName = "";

                        int templateIDInCart = 0;
                        PrintTemplateCart templCart = (PrintTemplateCart)Session["PrintTemplateCart"];
                        foreach (PrintTemplateCartItem itemTempl in templCart)
                        {
                            if (itemTempl.JourLineID == line.GoodID)
                            {
                                templateIDInCart = itemTempl.TemplateID;
                                proizvoditel = itemTempl.Proizvoditel;
                                goodName = itemTempl.GoodName;
                                break;
                            }
                        }
                        Image img = new Image();
                        img.ImageUrl = "Services/Cennic.ashx?LID=" + line.ID.ToString() + "&TID=" + templateIDInCart.ToString() + "&PID=" + Server.UrlEncode(proizvoditel) + "&GNID=" + Server.UrlEncode(goodName);

                        CennicPlaceHolder1.Controls.Add(img);

                        currCountCennic++;

                    }
                }

            }
        }


        private int FindTemplate(int priceJourLine)
        {
            var priceLine = (from p in context.PriceChangeLine
                             where p.ID == priceJourLine
                             select p).FirstOrDefault();
            if (priceLine == null)
                return 0;

            int ptID = 0;

            if (priceLine.Good != null)
            {
                if (priceLine.Good.PrintTemplateID != null)
                    ptID = priceLine.Good.PrintTemplateID.Value;
            }

            if (ptID == 0)
            {
                var good = (from g in context.Goods
                            where g.ID == priceLine.GoodID
                            select g).FirstOrDefault();

                if (good == null)
                    return 0;

                // ищем шаблон на верхнем уровне.
                if (good.Group.PrintTemplateID == null)
                {
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
                            ptID = grp.PrintTemplateID.Value;
                            break;
                        }

                        if (good.Group.GroupRangeID != 0 && good.Group.GroupRangeID != null)
                            upGroupID = good.Group.GroupRangeID.Value;
                    }

                }
                else
                    ptID = good.Group.PrintTemplateID.Value;

            }


            PrintTemplateCart templCart = (PrintTemplateCart)Session["PrintTemplateCart"];
            foreach (PrintTemplateCartItem itemTempl in templCart)
            {
                if (itemTempl.JourLineID == priceJourLine)
                {
                    ptID = itemTempl.TemplateID;

                    break;
                }
            }

            return ptID;

        }

    }
}