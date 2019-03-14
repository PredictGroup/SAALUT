using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Saalut
{
    public partial class TermoLabelToLPT : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;
        int goodID = 0;
        int typeID = 0;
        int qty = 1;
        int printerID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            context = new SaalutDataClasses1DataContext();

            if (Request.QueryString["GID"] == null)
                //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
                Response.Redirect("TermoTickets.aspx");


            Int32.TryParse(Request.QueryString["GID"], out goodID);
            Int32.TryParse(Request.QueryString["TID"], out typeID);
            Int32.TryParse(Request.QueryString["QTY"], out qty);
            Int32.TryParse(Request.QueryString["PID"], out printerID);

            string dateTm = Session["DataVremTextBox1"].ToString();

            if (goodID == 0)
                //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
                Response.Redirect("TermoTickets.aspx");



            //------
            if (!IsPostBack)
            {
                TermLabelUtils utl = new TermLabelUtils();
                utl.PrintTermoLabel(goodID, typeID, qty, printerID, dateTm);
            }
        }

        protected void BackButton1_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/TermoTickets.aspx");
        }
    }
}