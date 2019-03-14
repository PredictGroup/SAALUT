using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Saalut
{
    public partial class Settings : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();
        }

        private void GetStoreInfo()
        {
            
        }


    }
}