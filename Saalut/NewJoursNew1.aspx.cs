using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Saalut
{
    public partial class NewJoursNew1 : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            if (!IsPostBack)
            {
                Session["JourCart"] = new JourCart();

                Session["PrintTemplateCart"] = new PrintTemplateCart();
            }

        }

        protected void NewJoursGridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                PriceChangeJour row = (PriceChangeJour)e.Row.DataItem;

                HyperLink GoToLinesHyperLink1 = (HyperLink)e.Row.FindControl("GoToLinesHyperLink1");
                GoToLinesHyperLink1.NavigateUrl = "NewJoursLines.aspx?ID=" + row.ID.ToString();

            }
        }

        protected void NewJoursGridView1_SelectedIndexChanged(object sender, EventArgs e)
        {

            int journalID = (int)NewJoursGridView1.SelectedDataKey.Value;

            var journal = (from j in context.PriceChangeJours
                           where j.ID == journalID
                           select j).FirstOrDefault();

            journal.Active = false;
            context.SubmitChanges();

            NewJoursGridView1.DataBind();
        }

        protected void LoadNewJoursButton1_Click(object sender, EventArgs e)
        {
            MessageLabel1.Text = "Загрузка данных... ждите...";
            UKMDataBaseConnects utl = new UKMDataBaseConnects();
            MessageLabel1.Text = utl.ReadNewData_from_UKM_New_Price();

            NewJoursLinqDataSource1.DataBind();
            NewJoursGridView1.DataBind();
        }
    }
}