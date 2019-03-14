using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Saalut
{
    public partial class PriceJoursTSD : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

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
    }
}