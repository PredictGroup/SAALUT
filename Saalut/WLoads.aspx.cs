using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;

namespace Saalut
{
    public partial class WLoads : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SaalutDataClasses1DataContext context = new SaalutDataClasses1DataContext();
            CheckBoxList chb = new CheckBoxList();
            chb.ID = "WTCheckBoxList1";

            var weights = from w in context.WeightDeparts
                          where w.Active == true
                          orderby w.Num
                          select w;

            string lastW = "";
            foreach (WeightDepart wt in weights)
            {
                if (lastW == wt.Num)
                    continue;

                string weightName = "Весы №" + wt.Num + " ";

                var wname = (from wn in context.WeightNames
                             where wn.WeightNum == wt.Num
                             select wn).FirstOrDefault();
                if (wname != null)
                    weightName += " (" + wname.Name + "), привязаны группы: ";
                
                var wds = from w in context.WeightDeparts
                          where w.Num == wt.Num
                          && w.Active == true
                          select w;

                foreach (WeightDepart wtd in wds)
                {
                    var dept = (from d in context.Departments
                                where d.ID == wtd.DepartmentID
                                && d.Active == true
                                select d).FirstOrDefault();

                    if (dept == null)
                        continue;

                    weightName += dept.DepartName + " ";
                }

                ListItem itm = new ListItem();
                itm.Value = wt.Num;
                itm.Text = weightName;
                chb.Items.Add(itm);

                lastW = wt.Num;
            }

            if (weights.Count() > 0)
                WeightsPlaceHolder1.Controls.Add(chb);
        }

        protected void SendAllToWEButton1_Click(object sender, EventArgs e)
        {
            WeighingEquipUtils utl = new WeighingEquipUtils();
            utl.ExportAllToWE();
        }

        protected void LoadQWeighButton1_Click(object sender, EventArgs e)
        {
            WLMessageTextBox1.Text = "";

            WeighingEquipUtils utl = new WeighingEquipUtils();

            CheckBoxList chb = (CheckBoxList)WeightsPlaceHolder1.FindControl("WTCheckBoxList1");
            if (chb == null)
                return;

            bool loaded = false;

            foreach (ListItem itm in chb.Items)
            {
                if (itm.Selected)
                {
                    WLMessageTextBox1.Text += utl.ExportToWE_qload_by_weights_UKMPrice_by_format(itm.Value) + " /n ";

                    loaded = true;

                    Thread.Sleep(1000);
                }
            }

            if (loaded)
            {
                WLMessageTextBox1.Text += " Весы загружены.";
            }
        }


        protected void LoadArtButton2_Click(object sender, EventArgs e)
        {
            SaalutDataClasses1DataContext context = new SaalutDataClasses1DataContext();

            var good = (from g in context.Goods
                        where g.Articul == ArtTextBox1.Text
                        select g).FirstOrDefault();

            UKMDataBaseConnects utl = new UKMDataBaseConnects();
            utl.UpdateGood(good, ArtTextBox1.Text);
            //2
            utl.UpdateGood(good, ArtTextBox1.Text);
        }


        protected void SelectTestButton1_Click(object sender, EventArgs e)
        {
            UKMDataBaseConnects ukm = new UKMDataBaseConnects();
            ukm.InitialDB();
        }

    }
}