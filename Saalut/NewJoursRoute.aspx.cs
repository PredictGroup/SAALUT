using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;

namespace Saalut
{
    public partial class NewJoursRoute : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string Division = WebConfigurationManager.AppSettings["Division"];

            if (Division == "RF")
                Response.Redirect("~/NewJours.aspx");
            else
                if(Division == "RB")
                    Response.Redirect("~/NewJoursNew1.aspx"); // журнал без журнала изменения цен в укм
        }
    }
}