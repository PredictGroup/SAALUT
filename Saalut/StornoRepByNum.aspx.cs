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
    public partial class StornoRepByNum : System.Web.UI.Page
    {
        SaalutDataClasses1DataContext context;
        int groupID;

        // Connection string for a typical local MySQL installation
        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (context == null)
                context = new SaalutDataClasses1DataContext();
        }


        protected void SubmitButton1_Click(object sender, EventArgs e)
        {
            MySqlConnection cnx = null;

            cnx = new MySqlConnection(connStr);

            MySqlDataAdapter adapter = new MySqlDataAdapter();

            string dateTod = DateTime.Today.ToString("yyy-MM-dd");

            string smenaN = NumTextBox1.Text;
            string kashN = CashTextBox1.Text;

            string cmdText = "SELECT cl.name 'Shop', usr.name 'Casher', DATE_FORMAT(f.date, '%d/%m/%Y %H:%i:%s') 'Time', c.number 'POSN', h.shift_open 'Smena', h.local_number 'Cheque', it.item 'Item', it.name 'Name', it.total_quantity 'Qty', it.total + it.discount 'Sum' FROM trm_in_store cl INNER JOIN trm_in_pos c ON c.store_id = cl.store_id INNER JOIN trm_out_receipt_header h ON h.cash_id = c.cash_id INNER JOIN trm_out_receipt_footer f ON (h.cash_id = f.cash_id AND h.id = f.id) INNER JOIN trm_out_receipt_item it ON (h.cash_id = it.cash_id AND h.id = it.receipt_header) INNER JOIN trm_out_shift_open s ON (h.cash_id = s.cash_id AND h.shift_open = s.id) LEFT JOIN trm_out_login lg ON (h.cash_id = lg.cash_id AND h.login = lg.id) LEFT JOIN trm_in_users usr ON (c.store_id = usr.store_id AND lg.user_id = usr.id) WHERE (h.type IN (0, 5)) AND (f.result = 0) AND (it.type IN (1, 2)) AND s.number = '" + smenaN + "' AND c.number = '" + kashN + "' ORDER BY f.date, c.number, s.number, h.local_number, it.position;";
            MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
            // Create a fill a Dataset
            DataSet ds = new DataSet();
            adapter.SelectCommand = cmd;
            adapter.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                object objSum;
                objSum = ds.Tables[0].Compute("Sum(Sum)", "");

                object objQty;
                objQty = ds.Tables[0].Compute("Sum(Qty)", "");

                Label lblGroup = new Label();
                lblGroup.Text = "Кол.: " + objQty.ToString() + "  Сумма: " + objSum.ToString();
                ReportPlaceHolder1.Controls.Add(lblGroup);

                GridView ReportGridView1 = new GridView();
                ReportGridView1.Width = new Unit(950);
                ReportGridView1.AutoGenerateColumns = false;

                BoundField fld10 = new System.Web.UI.WebControls.BoundField();
                fld10.HeaderText = "Магазин";
                fld10.DataField = "Shop";
                ReportGridView1.Columns.Add(fld10);

                BoundField fld2 = new System.Web.UI.WebControls.BoundField();
                fld2.HeaderText = "Кассир";
                fld2.DataField = "Casher";
                ReportGridView1.Columns.Add(fld2);

                BoundField fld11 = new System.Web.UI.WebControls.BoundField();
                fld11.HeaderText = "Время";
                fld11.DataField = "Time";
                ReportGridView1.Columns.Add(fld11);

                BoundField fld1 = new System.Web.UI.WebControls.BoundField();
                fld1.HeaderText = "Касса";
                fld1.DataField = "POSN";
                ReportGridView1.Columns.Add(fld1);

                BoundField fld12 = new System.Web.UI.WebControls.BoundField();
                fld12.HeaderText = "Смена";
                fld12.DataField = "Smena";
                ReportGridView1.Columns.Add(fld12);

                BoundField fld13 = new System.Web.UI.WebControls.BoundField();
                fld13.HeaderText = "Чек";
                fld13.DataField = "Cheque";
                ReportGridView1.Columns.Add(fld13);

                BoundField fld3 = new System.Web.UI.WebControls.BoundField();
                fld3.HeaderText = "Арт.";
                fld3.DataField = "Item";
                ReportGridView1.Columns.Add(fld3);

                BoundField fld5 = new System.Web.UI.WebControls.BoundField();
                fld5.HeaderText = "Наименование";
                fld5.DataField = "Name";
                ReportGridView1.Columns.Add(fld5);

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
            }
            else
            {
                ErrorLabel1.Text = "В выборке нет данных, не было возвратов и аннуляций.";
            }

            NumLabel1.Text = NumTextBox1.Text;
            CashLabel1.Text = CashTextBox1.Text;

            Wizard1.ActiveStepIndex = 1;
        }
    }
}