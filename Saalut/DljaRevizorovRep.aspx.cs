using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using MySql.Data.Common;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Data;

namespace Saalut
{
    public partial class DljaRevizorovRep : System.Web.UI.Page
    {

        SaalutDataClasses1DataContext context;

        // Connection string for a typical local MySQL installation
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FillControls();
            }
        }

        protected void FillControls()
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            Dictionary<string, string> groupsUp = new Dictionary<string, string>();

            MySqlConnection cnx = null;

            cnx = new MySqlConnection(connStr);

            string cmdText = "select * from trm_in_classif where owner in (select id from trm_in_classif where owner = 0) order by name";
            MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
            cmd.CommandTimeout = 18000;

            MySqlDataAdapter adapter = new MySqlDataAdapter();

            // Create a fill a Dataset
            DataSet ds = new DataSet();
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string id = row.Field<string>("id");

                    string cmdText2 = "select * from trm_in_classif where owner=" + id + "; ";
                    MySqlCommand cmd2 = new MySqlCommand(cmdText2, cnx);
                    // Create a fill a Dataset
                    DataSet ds2 = new DataSet();
                    adapter.SelectCommand = cmd2;
                    adapter.Fill(ds2);

                    if (ds2.Tables[0].Rows.Count != 0)
                    {
                        foreach (DataRow row2 in ds2.Tables[0].Rows)
                        {
                            string id2 = row2.Field<string>("id");
                            string name2 = row2.Field<string>("name");

                            string intGroupN = "";
                            if (id2 != "")
                            {
                                if (!groupsUp.TryGetValue(id2, out intGroupN))
                                {
                                    //var grpnfrr = (from g in context.Groups
                                    //               where g.GroupNum == id2
                                    //               select g).FirstOrDefault();

                                    groupsUp.Add(id2, name2);
                                }
                            }
                        }

                    }

                }
            }



            //-
            foreach (KeyValuePair<string, string> pair in groupsUp.OrderBy(key => key.Value))
            {
                ListItem itm = new ListItem();
                itm.Text = pair.Value;
                itm.Value = pair.Key;
                SelectListBox1.Items.Add(itm);
            }

        }

        protected void FillControlsDeeper2()
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();

            Dictionary<string, string> groupsUp = new Dictionary<string, string>();

            MySqlConnection cnx = null;

            cnx = new MySqlConnection(connStr);

            string cmdText = "select * from trm_in_classif where owner in (select id from trm_in_classif where owner = 0) order by name";
            MySqlCommand cmd = new MySqlCommand(cmdText, cnx);

            MySqlDataAdapter adapter = new MySqlDataAdapter();

            // Create a fill a Dataset
            DataSet ds = new DataSet();
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string id = row.Field<string>("id");

                    string cmdText2 = "select * from trm_in_classif where owner=" + id + "; ";
                    MySqlCommand cmd2 = new MySqlCommand(cmdText2, cnx);
                    // Create a fill a Dataset
                    DataSet ds2 = new DataSet();
                    adapter.SelectCommand = cmd2;
                    adapter.Fill(ds2);

                    if (ds2.Tables[0].Rows.Count != 0)
                    {
                        foreach (DataRow row2 in ds2.Tables[0].Rows)
                        {
                            string id2 = row2.Field<string>("id");
                            string name2 = row2.Field<string>("name");

                            string cmdText3 = "select * from trm_in_classif where owner=" + id2 + "; ";
                            MySqlCommand cmd3 = new MySqlCommand(cmdText3, cnx);
                            // Create a fill a Dataset
                            DataSet ds3 = new DataSet();
                            adapter.SelectCommand = cmd3;
                            adapter.Fill(ds3);

                            if (ds3.Tables[0].Rows.Count != 0)
                            {
                                foreach (DataRow row3 in ds3.Tables[0].Rows)
                                {
                                    string id3 = row3.Field<string>("id");
                                    string name3 = row3.Field<string>("name");

                                    //+
                                    string intGroupN = "";
                                    if (id3 != "")
                                    {
                                        if (!groupsUp.TryGetValue(id3, out intGroupN))
                                        {
                                            //var grpnfrr = (from g in context.Groups
                                            //               where g.GroupNum == id2
                                            //               select g).FirstOrDefault();

                                            groupsUp.Add(id3, name3);
                                        }
                                    }
                                    //-
                                }
                            }
                        }

                    }

                }
            }



            //-
            foreach (KeyValuePair<string, string> pair in groupsUp.OrderBy(key => key.Value))
            {
                ListItem itm = new ListItem();
                itm.Text = pair.Value;
                itm.Value = pair.Key;
                SelectListBox1.Items.Add(itm);
            }

        }
        
        protected void SubmitButton1_Click(object sender, EventArgs e)
        {
            string smenaN = NumTextBox1.Text;
            string kashN = CashTextBox1.Text;

            MySqlConnection cnx = null;

            cnx = new MySqlConnection(connStr);
            MySqlDataAdapter adapter = new MySqlDataAdapter();

            StringBuilder sb = new StringBuilder();

            int i = 1;
            foreach (ListItem itm in CheckedListBox1.Items)
            {
                string id = itm.Value;
                sb.Append(id);

                string cmdText2 = "select * from trm_in_classif where owner=" + id + "; ";
                MySqlCommand cmd2 = new MySqlCommand(cmdText2, cnx);
                // Create a fill a Dataset
                DataSet ds2 = new DataSet();
                adapter.SelectCommand = cmd2;
                adapter.Fill(ds2);

                if (ds2.Tables[0].Rows.Count != 0)
                {
                    sb.Append(",");

                    int i2 = 1;
                    foreach (DataRow row2 in ds2.Tables[0].Rows)
                    {
                        string id2 = row2.Field<string>("id");
                        string name2 = row2.Field<string>("name");

                        sb.Append(id2);

                        // 3
                        string cmdText3 = "select * from trm_in_classif where owner=" + id2 + "; ";
                        MySqlCommand cmd3 = new MySqlCommand(cmdText3, cnx);
                        // Create a fill a Dataset
                        DataSet ds3 = new DataSet();
                        adapter.SelectCommand = cmd3;
                        adapter.Fill(ds3);

                        if (ds3.Tables[0].Rows.Count != 0)
                        {
                            sb.Append(",");

                            int i3 = 1;
                            foreach (DataRow row3 in ds3.Tables[0].Rows)
                            {
                                string id3 = row3.Field<string>("id");
                                string name3 = row3.Field<string>("name");

                                sb.Append(id3);

                                // 4 
                                string cmdText4 = "select * from trm_in_classif where owner=" + id3 + "; ";
                                MySqlCommand cmd4 = new MySqlCommand(cmdText4, cnx);
                                // Create a fill a Dataset
                                DataSet ds4 = new DataSet();
                                adapter.SelectCommand = cmd4;
                                adapter.Fill(ds4);
                                if (ds4.Tables[0].Rows.Count != 0)
                                {
                                    sb.Append(",");

                                    int i4 = 1;
                                    foreach (DataRow row4 in ds4.Tables[0].Rows)
                                    {
                                        string id4 = row4.Field<string>("id");
                                        string name4 = row4.Field<string>("name");

                                        sb.Append(id4);

                                        // 5
                                        string cmdText5 = "select * from trm_in_classif where owner=" + id4 + "; ";
                                        MySqlCommand cmd5 = new MySqlCommand(cmdText5, cnx);
                                        // Create a fill a Dataset
                                        DataSet ds5 = new DataSet();
                                        adapter.SelectCommand = cmd5;
                                        adapter.Fill(ds5);

                                        if (ds5.Tables[0].Rows.Count != 0)
                                        {
                                            sb.Append(",");

                                            int i5 = 1;
                                            foreach (DataRow row5 in ds5.Tables[0].Rows)
                                            {
                                                string id5 = row5.Field<string>("id");
                                                string name5 = row5.Field<string>("name");

                                                sb.Append(id5);

                                                // 6
                                                string cmdText6 = "select * from trm_in_classif where owner=" + id5 + "; ";
                                                MySqlCommand cmd6 = new MySqlCommand(cmdText6, cnx);
                                                // Create a fill a Dataset
                                                DataSet ds6 = new DataSet();
                                                adapter.SelectCommand = cmd6;
                                                adapter.Fill(ds6);

                                                if (ds6.Tables[0].Rows.Count != 0)
                                                {
                                                    sb.Append(",");

                                                    int i6 = 1;
                                                    foreach (DataRow row6 in ds6.Tables[0].Rows)
                                                    {
                                                        string id6 = row6.Field<string>("id");
                                                        string name6 = row6.Field<string>("name");

                                                        sb.Append(id6);

                                                        // 7
                                                        string cmdText7 = "select * from trm_in_classif where owner=" + id6 + "; ";
                                                        MySqlCommand cmd7 = new MySqlCommand(cmdText7, cnx);
                                                        // Create a fill a Dataset
                                                        DataSet ds7 = new DataSet();
                                                        adapter.SelectCommand = cmd7;
                                                        adapter.Fill(ds7);

                                                        if (ds7.Tables[0].Rows.Count != 0)
                                                        {
                                                            sb.Append(",");

                                                            int i7 = 1;
                                                            foreach (DataRow row7 in ds7.Tables[0].Rows)
                                                            {
                                                                string id7 = row7.Field<string>("id");
                                                                string name7 = row7.Field<string>("name");

                                                                sb.Append(id7);

                                                                // 8 
                                                                string cmdText8 = "select * from trm_in_classif where owner=" + id7 + "; ";
                                                                MySqlCommand cmd8 = new MySqlCommand(cmdText8, cnx);
                                                                // Create a fill a Dataset
                                                                DataSet ds8 = new DataSet();
                                                                adapter.SelectCommand = cmd8;
                                                                adapter.Fill(ds8);

                                                                if (ds8.Tables[0].Rows.Count != 0)
                                                                {
                                                                    sb.Append(",");

                                                                    int i8 = 1;
                                                                    foreach (DataRow row8 in ds8.Tables[0].Rows)
                                                                    {
                                                                        string id8 = row8.Field<string>("id");
                                                                        string name8 = row8.Field<string>("name");

                                                                        sb.Append(id8);
                                                                        if (i8 != ds8.Tables[0].Rows.Count)
                                                                            sb.Append(",");

                                                                        i8++;
                                                                    }
                                                                }

                                                                // -8

                                                                if (i7 != ds7.Tables[0].Rows.Count)
                                                                    sb.Append(",");

                                                                i7++;
                                                            }
                                                        }

                                                        // -7


                                                        if (i6 != ds6.Tables[0].Rows.Count)
                                                            sb.Append(",");

                                                        i6++;
                                                    }
                                                }

                                                // -6

                                                if (i5 != ds5.Tables[0].Rows.Count)
                                                    sb.Append(",");

                                                i5++;
                                            }
                                        }

                                        if (i4 != ds4.Tables[0].Rows.Count)
                                            sb.Append(",");


                                        // -5
                                        i4++;
                                    }
                                }

                                if (i3 != ds3.Tables[0].Rows.Count)
                                    sb.Append(",");


                                // -4

                                i3++;
                            }
                        }

                        if (i2 != ds2.Tables[0].Rows.Count)
                            sb.Append(",");

                        // -3


                        i2++;
                    }

                }

                if (i != CheckedListBox1.Items.Count)
                    sb.Append(",");
                i++;

                //---         
            }
            string fromSB = sb.ToString();

            string fZap = fromSB.Substring(fromSB.Length - 1);
            if (fZap == ",")
                fromSB = fromSB.Substring(0, fromSB.Length - 2);

            if (fromSB != "")
            {
                // TODO THIS: исправить запрос чтобы выборка фильтровалась по полю items classif
                StringBuilder cmdText = new StringBuilder();
                cmdText.Append("SELECT i.item 'Item', i.name 'Name', classif.id 'classif_id', classif.name 'classif', SUM(IF(h.type IN (0,5), 1, -1) * i.quantity) 'Qty', SUM(IF(h.type IN (0,5), 1, -1) * i.total + i.discount) 'Sum' FROM trm_in_pos c INNER JOIN trm_out_receipt_header h ON h.cash_id = c.cash_id INNER JOIN trm_out_receipt_item i ON i.cash_id = h.cash_id AND i.receipt_header = h.id  LEFT JOIN trm_out_receipt_item i2 ON (h.cash_id = i2.cash_id AND h.id = i2.receipt_header AND i2.link_item = i.id) INNER JOIN trm_out_receipt_footer f ON f.cash_id = h.cash_id AND f.id = h.id INNER JOIN trm_in_classif classif ON (i.classif = classif.id ) INNER JOIN trm_out_login lg ON (h.cash_id = lg.cash_id AND h.login = lg.id) INNER JOIN trm_in_users usr ON (c.store_id = usr.store_id AND lg.user_id = usr.id) INNER join trm_out_shift_open so on (so.id = h.shift_open AND so.cash_id = h.cash_id) WHERE i2.link_item IS NULL AND i.type = 0 AND h.type IN (0,5,1,4) AND f.result IN (0) AND classif.id in (" + fromSB + ") AND c.number = '" + kashN + "' AND so.number = '" + smenaN + "' GROUP BY i.item ORDER BY classif.id, i.item;");
                MySqlCommand cmd = new MySqlCommand(cmdText.ToString(), cnx);
                // Create a fill a Dataset
                DataSet ds = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds);

                //DebugLabel1.Text = cmdText.ToString();

                if (ds.Tables[0].Rows.Count != 0)
                {
                    object objSum;
                    objSum = ds.Tables[0].Compute("Sum(Sum)", "");

                    object objQty;
                    objQty = ds.Tables[0].Compute("Sum(Qty)", "");

                    GridView ReportGridView1 = new GridView();
                    ReportGridView1.Width = new Unit(950);
                    ReportGridView1.AutoGenerateColumns = false;

                    //BoundField fld10 = new System.Web.UI.WebControls.BoundField();
                    //fld10.HeaderText = "Магазин";
                    //fld10.DataField = "Shop";
                    //ReportGridView1.Columns.Add(fld10);

                    //BoundField fld2 = new System.Web.UI.WebControls.BoundField();
                    //fld2.HeaderText = "Кассир";
                    //fld2.DataField = "Casher";
                    //ReportGridView1.Columns.Add(fld2);

                    //BoundField fld11 = new System.Web.UI.WebControls.BoundField();
                    //fld11.HeaderText = "Время";
                    //fld11.DataField = "Time";
                    //ReportGridView1.Columns.Add(fld11);

                    //BoundField fld1 = new System.Web.UI.WebControls.BoundField();
                    //fld1.HeaderText = "Касса";
                    //fld1.DataField = "POSN";
                    //ReportGridView1.Columns.Add(fld1);

                    //BoundField fld12 = new System.Web.UI.WebControls.BoundField();
                    //fld12.HeaderText = "Смена";
                    //fld12.DataField = "Smena";
                    //ReportGridView1.Columns.Add(fld12);

                    //BoundField fld13 = new System.Web.UI.WebControls.BoundField();
                    //fld13.HeaderText = "Чек";
                    //fld13.DataField = "Cheque";
                    //ReportGridView1.Columns.Add(fld13);

                    BoundField fld3 = new System.Web.UI.WebControls.BoundField();
                    fld3.HeaderText = "Арт.";
                    fld3.DataField = "Item";
                    ReportGridView1.Columns.Add(fld3);

                    BoundField fld5 = new System.Web.UI.WebControls.BoundField();
                    fld5.HeaderText = "Наименование";
                    fld5.DataField = "Name";
                    ReportGridView1.Columns.Add(fld5);

                    BoundField fld54 = new System.Web.UI.WebControls.BoundField();
                    fld54.HeaderText = "Группа";
                    fld54.DataField = "classif_id";
                    ReportGridView1.Columns.Add(fld54);

                    BoundField fld55 = new System.Web.UI.WebControls.BoundField();
                    fld55.HeaderText = "Наим. группы";
                    fld55.DataField = "classif";
                    ReportGridView1.Columns.Add(fld55);

                    BoundField fld6 = new System.Web.UI.WebControls.BoundField();
                    fld6.HeaderText = "Кол.";
                    fld6.DataField = "Qty";
                    fld6.DataFormatString = "{0:N}";
                    ReportGridView1.Columns.Add(fld6);

                    BoundField fld7 = new System.Web.UI.WebControls.BoundField();
                    fld7.HeaderText = "Сумма";
                    fld7.DataField = "Sum";
                    fld7.DataFormatString = "{0:N}";
                    ReportGridView1.Columns.Add(fld7);


                    ReportGridView1.DataSource = ds.Tables[0];
                    ReportGridView1.DataBind();
                    ReportPlaceHolder1.Controls.Add(ReportGridView1);

                    Label lblGroup = new Label();
                    lblGroup.Text = "Итоги: Кол.: " + objQty.ToString() + "  Сумма: " + objSum.ToString();
                    ReportPlaceHolder1.Controls.Add(lblGroup);
                }
                else
                {
                    ErrorLabel1.Text = "В выборке нет данных";
                }

            }


            NumLabel1.Text = NumTextBox1.Text;
            CashLabel1.Text = CashTextBox1.Text;

            Wizard1.ActiveStepIndex = 1;
        }

        protected void AddButton1_Click(object sender, EventArgs e)
        {
            CheckedListBox1.Items.Add(new ListItem(SelectListBox1.SelectedItem.Text, SelectListBox1.SelectedValue));
        }

        protected void PreRepButton1_Click(object sender, EventArgs e)
        {
            FillControls();
        }

        protected void DeleteButton1_Click(object sender, EventArgs e)
        {
            CheckedListBox1.Items.RemoveAt(CheckedListBox1.SelectedIndex);
        }

    }
}