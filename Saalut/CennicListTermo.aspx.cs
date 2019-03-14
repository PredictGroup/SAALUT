using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Saalut
{
    public partial class CennicListTermo : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;
        int jourID = 0;
        int qty = 1;
        int termPrinterID = 0;

        bool WhithoutPrice = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Dictionary<int, int> groupTemlates = new Dictionary<int, int>();


                context = new SaalutDataClasses1DataContext();

                if (Request.QueryString["ID"] == null)
                    //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
                    Response.Redirect("ByFind.aspx");

                Int32.TryParse(Request.QueryString["ID"], out jourID);

                if (jourID == 0)
                    //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
                    Response.Redirect("ByFind.aspx");


                if (Request.QueryString["WP"] != null)
                {
                    int tmpI = 0;
                    Int32.TryParse(Request.QueryString["WP"], out jourID);
                    if (tmpI == 1)
                        WhithoutPrice = true;
                }


                if (Request.QueryString["q"] != null)
                {
                    Int32.TryParse(Request.QueryString["q"], out qty);
                }

                if (Request.QueryString["f"] != null)
                {
                    Int32.TryParse(Request.QueryString["f"], out termPrinterID);
                }

                //if (Session["JourCart"] == null)
                //    //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true); 
                //    Response.Redirect("NewJours.aspx");

                if (termPrinterID == 0)
                {

                    // Ищем термо ценник для печати на принтере
                    var termCennic = (from c in context.ThermoLabels
                                      where c.FileLabelName.EndsWith(".slb")
                                      select c).FirstOrDefault();
                    if (termCennic == null)
                        Response.Redirect("ByFind.aspx");

                    var termPrnt = (from t in context.TermoPrinters
                                    where t.TermoPrinterPort == "lpt4"
                                    select t).FirstOrDefault();
                    if (termPrnt == null)
                        Response.Redirect("ByFind.aspx");

                    string dateForLabel = DateTime.Today.ToString("d");

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

                        TermLabelUtils utl = new TermLabelUtils();

                        JourCart cart = (JourCart)Session["JourCart"];
                        foreach (JourCartItem items in cart)
                        {
                            if (items.JourLineID == ln.ID)
                            {
                                string proizvoditel = "";
                                PrintTemplateCart templCart = (PrintTemplateCart)Session["PrintTemplateCart"];
                                foreach (PrintTemplateCartItem itemTempl in templCart)
                                {
                                    if (itemTempl.JourLineID == ln.ID)
                                    {
                                        proizvoditel = itemTempl.Proizvoditel;
                                        break;
                                    }
                                }

                                // входит в список распечатки.
                                int n = 0;
                                while (n < qty)
                                {
                                    utl.PrintTermoCennic(ln.GoodID.Value, termCennic.ID, 1, termPrnt.ID, dateForLabel, proizvoditel);
                                    n++;
                                }

                                break;
                            }
                        }

                    }

                }
                else // большой ценник
                {

                    // Ищем термо ценник для печати на принтере
                    var termCennic2 = (from c in context.ThermoLabels
                                      where c.FileLabelName.EndsWith(".blb")
                                      && c.Active == true
                                      select c).FirstOrDefault();
                    var termCennic1 = (from c in context.ThermoLabels
                                       where c.FileLabelName.EndsWith(".slb")
                                       && c.Active == true
                                       select c).FirstOrDefault();

                    var termPrnt = (from t in context.TermoPrinters
                                    where t.ID == termPrinterID
                                    && t.Active == true
                                    select t).FirstOrDefault();
                    if (termPrnt == null)
                        Response.Redirect("ByFind.aspx");

                    string dateForLabel = DateTime.Today.ToString("d");

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

                        TermLabelUtils utl = new TermLabelUtils();

                        JourCart cart = (JourCart)Session["JourCart"];
                        foreach (JourCartItem items in cart)
                        {
                            if (items.JourLineID == ln.ID)
                            {
                                string proizvoditel = "";
                                PrintTemplateCart templCart = (PrintTemplateCart)Session["PrintTemplateCart"];
                                foreach (PrintTemplateCartItem itemTempl in templCart)
                                {
                                    if (itemTempl.JourLineID == ln.ID)
                                    {
                                        proizvoditel = itemTempl.Proizvoditel;
                                        break;
                                    }
                                }


                                // входит в список распечатки.
                                int n = 0;
                                while (n < qty)
                                {
                                    if(termPrnt.TermoPrinterPort == "lpt4")
                                        utl.PrintTermoCennic(ln.GoodID.Value, termCennic1.ID, 1, termPrinterID, dateForLabel, proizvoditel);
                                    else
                                        if(termPrnt.TermoPrinterPort == "lpt8")
                                            utl.PrintTermoCennic(ln.GoodID.Value, termCennic2.ID, 1, termPrinterID, dateForLabel, proizvoditel);
                                    n++;
                                }

                                break;
                            }
                        }

                    }

                } // большой ценник
            }
        }

        protected void BackButton1_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ByFind.aspx");
        }
    }
}