using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Saalut.TSD
{
    public partial class Default : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;
        int jourID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

        }


        protected void Gridview1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            jourID = (int)Gridview1.DataKeys[e.RowIndex].Value;

            var journ = (from j in context.PriceChangeJours
                         where j.ID == jourID
                         select j).FirstOrDefault();
            journ.Active = false;
            context.SubmitChanges();

            PriceJoursLinqDataSource1.DataBind();
            Gridview1.DataBind();

            e.Cancel = true;
        }

        protected void Gridview1_RowDeleted(object sender, GridViewDeletedEventArgs e)
        {
            PriceJoursLinqDataSource1.DataBind();
            Gridview1.DataBind();
        }

        protected void Gridview1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                PriceChangeJour row = (PriceChangeJour)e.Row.DataItem;

                ImageButton GoToLinesImageButton1 = (ImageButton)e.Row.FindControl("GoToLinesImageButton1");
                GoToLinesImageButton1.PostBackUrl = "TSDJourLines.aspx?ID=" + row.ID.ToString();
            }
        }

        protected void CreateNewButton1_Click(object sender, EventArgs e)
        {
            int MobilePrinterNum = 0;
            if (ddlMobilePrinterNum.SelectedIndex != -1)
                MobilePrinterNum = Int32.Parse(ddlMobilePrinterNum.SelectedItem.Value.ToString());

            PriceChangeJour np = new PriceChangeJour();
            np.Change_log_id_UKM = 0;
            np.Order_no = MobilePrinterNum;
            np.FromTerminal = true;
            np.Active = true;
            np.TimeStamp = DateTime.Now;
            np.InUse = false;
            context.PriceChangeJours.InsertOnSubmit(np);
            context.SubmitChanges();

            PriceJoursLinqDataSource1.DataBind();
            Gridview1.DataBind();
        }
    }
}