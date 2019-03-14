using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Saalut
{
    public partial class TermoLabel : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;
        int goodID = 0;
        int typeID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            context = new SaalutDataClasses1DataContext();

            if (Request.QueryString["GID"] == null)
                //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
                Response.Redirect("TermoTickets.aspx");


            Int32.TryParse(Request.QueryString["GID"], out goodID);
            Int32.TryParse(Request.QueryString["TID"], out typeID);


            if (goodID == 0)
                //ClientScript.RegisterStartupScript(typeof(Page), "closePage", "window.close();", true);
                Response.Redirect("TermoTickets.aspx");

            //------

            Image img = new Image();

            if(typeID == 0)
                img.ImageUrl = "Services/TermoLabelImage.ashx?GID=" + goodID.ToString();
            else
                img.ImageUrl = "Services/TermoLabelProizvodstvo.ashx?GID=" + goodID.ToString();

            CennicPlaceHolder1.Controls.Add(img);

        }
    }
}