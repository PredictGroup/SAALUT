using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Saalut
{
    public partial class TermoTickets : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;

        protected void Page_Load(object sender, EventArgs e)
        {
            context = new SaalutDataClasses1DataContext();

            if (!IsPostBack)
            {
                DataVremTextBox1.Text = String.Format("{0:d/M/yyyy HH:mm}", DateTime.Now);
            }
        }

        protected void CennicButton1_Click(object sender, EventArgs e)
        {
            int goodid = 0;
            foreach (GridViewRow row in GoodsGridView1.Rows)
            {
                Label GoodIDLabel1 = (Label)row.FindControl("GoodIDLabel1");
                goodid = Int32.Parse(GoodIDLabel1.Text);
            }

            if (goodid == 0)
            {
                return;
            }

            Session["DataVremTextBox1"] = DataVremTextBox1.Text;

            string url = "TermoLabelToLPT.aspx?GID=" + goodid.ToString() + "&TID=" + TypeDropDownList1.SelectedValue + "&QTY=" + QtyTextBox1.Text + "&PID=" + TermPrinterDropDownList1.SelectedValue;
            //string redirectScript = "<script>window.open('" + url + "');</script>";
            //Response.Write(redirectScript);
            Response.Redirect(url);
        }

    }
}