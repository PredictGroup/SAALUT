using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;

namespace Saalut
{
    public partial class NewJoursLines : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;
        int jourID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            context = new SaalutDataClasses1DataContext();

            if (Request.QueryString["ID"] == null)
                Response.Redirect("~/NewJours.aspx");

            Int32.TryParse(Request.QueryString["ID"], out jourID);

            if (jourID == 0)
                Response.Redirect("~/NewJours.aspx");

            NewJourLineLinqDataSource1.WhereParameters[0].DefaultValue = jourID.ToString();
            NewJourLineLinqDataSource1.DataBind();


            if (!IsPostBack)
            {
                if (Session["JourCart"] == null)
                    Session["JourCart"] = new JourCart();

                if (Session["PrintTemplateCart"] == null)
                    Session["PrintTemplateCart"] = new PrintTemplateCart();
            }

            var jour = (from j in context.PriceChangeJours
                        where j.ID == jourID
                        select j).FirstOrDefault();

            if (jour == null)
                Response.Redirect("~/Default.aspx");

            if (jour.Order_no != 0)
                JournalNumLabel1.Text = "Журнал УКМ N: " + jour.Order_no.ToString();
            else
                JournalNumLabel1.Text = "Журнал УКМ N: " + jour.ID.ToString();
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

        protected void NewJourLinesGridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                PriceChangeLine row = (PriceChangeLine)e.Row.DataItem;

                int LineID = (int)DataBinder.Eval(e.Row.DataItem, "ID");
                string ItemID_UKM = (string)DataBinder.Eval(e.Row.DataItem, "ItemID_UKM");

                TextBox ProizvoditelTextBox1 = (TextBox)e.Row.FindControl("ProizvoditelTextBox1");
                TextBox GoodNameTextBox1 = (TextBox)e.Row.FindControl("GoodNameTextBox1");

                var good = (from g in context.Goods
                            where g.Articul == ItemID_UKM
                            select g).FirstOrDefault();

                // set def value
                if (GoodNameTextBox1.Text == "")
                    GoodNameTextBox1.Text = good.Descr;
                if (ProizvoditelTextBox1.Text == "")
                    ProizvoditelTextBox1.Text = good.Country;


                LinkButton SelectLinkButton1 = (LinkButton)e.Row.FindControl("SelectLinkButton1");

                if (Session["JourCart"] != null)
                {
                    JourCart cart = (JourCart)Session["JourCart"];
                    foreach (JourCartItem items in cart)
                    {
                        if (items.JourLineID == LineID)
                        {
                            SelectLinkButton1.Text = "Исключить";

                            e.Row.Style.Add(HtmlTextWriterStyle.BackgroundColor, "LightGrey");
                            break;
                        }
                    }
                }

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

                                if (grp.GroupRangeID == null)
                                    break;
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


                PrintTemplateDropDownList1.SelectedValue = templateIDInCart.ToString();
                PrintTemplateDropDownList1.DataBind();

                //- конец шаблоны ценников
                AddToCartTemplate(LineID, templateIDInCart, ProizvoditelTextBox1.Text, GoodNameTextBox1.Text);
            }
        }

        protected void NewJourLinesGridView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int jourLineID = (int)NewJourLinesGridView1.SelectedDataKey.Value; // selected data key = PortfolioList ID
            this.AddToCart(jourLineID);

            NewJourLinesGridView1.DataBind();
        }

        protected void SetAllButton1_Click(object sender, EventArgs e)
        {
            var jourLines = from j in context.PriceChangeLine
                            where j.JournalID == jourID
                            select j;
            foreach (PriceChangeLine line in jourLines)
            {
                this.AddToCart(line.ID);
            }

            NewJourLinesGridView1.DataBind();
        }

        protected void CennicButton1_Click(object sender, EventArgs e)
        {
            string url = "";
            if (CheckBox1WhithoutPrice.Checked == true)
            {
                url = "CennicList.aspx?ID=" + jourID.ToString() + "&WP=1";
            }
            else
            {
                url = "CennicList.aspx?ID=" + jourID.ToString();
            }
            //string redirectScript = "<script>window.open('" + url + "');</script>";
            //Response.Write(redirectScript);
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

        protected void NewJourLinesGridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            NewJourLineLinqDataSource1.DataBind();

            NewJourLinesGridView1.PageIndex = e.NewPageIndex;
            NewJourLinesGridView1.DataBind();
        }

        protected void SetAllDDLButton1_Click(object sender, EventArgs e)
        {
            int pID = 0;
            Int32.TryParse(SomeCennicsDropDownList1.SelectedValue, out pID);

            var jourLines = from j in context.PriceChangeLine
                            where j.JournalID == jourID
                            select j;
            foreach (PriceChangeLine line in jourLines)
            {
                AddToCartTemplate(line.ID, pID, line.Good.Producer, line.Good.Descr);
            }

            NewJourLineLinqDataSource1.DataBind();
            NewJourLinesGridView1.DataBind();

        }

        protected void TermoCennicButton1_Click(object sender, EventArgs e)
        {
            string url = "";
            if (CheckBox1WhithoutPrice.Checked == true)
            {
                url = "CennicListTermo.aspx?ID=" + jourID.ToString() + "&WP=1&q=" + QtyTermoPTextBox1.Text + "&f=" + TermPrinterDropDownList1.SelectedValue;
            }
            else
            {
                url = "CennicListTermo.aspx?ID=" + jourID.ToString() + "&q=" + QtyTermoPTextBox1.Text + "&f=" + TermPrinterDropDownList1.SelectedValue;
            }
            //string redirectScript = "<script>window.open('" + url + "');</script>";
            //Response.Write(redirectScript);
            Response.Redirect(url);
        }

        protected void WeightLoadButton1_Click(object sender, EventArgs e)
        {
            string Division = WebConfigurationManager.AppSettings["Division"];

            if (Division == "RF")
            {
                WeighingEquipUtils utl = new WeighingEquipUtils();
                utl.ExportAllToWE();
            }
            else
                if (Division == "RB")
                {
                    WeighingEquipUtils utl = new WeighingEquipUtils();
                    utl.ExportToWE_qload_by_weights_UKMPrice_BY_Journ(jourID);
                }
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