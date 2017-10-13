using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CAP_JADE_Interface
{
    public partial class PendingOrders : Form
    {

        Data_Base_MNG.SQL DBMNG;
        string query = "SELECT JadeKey " +
                "FROM _CAP_Jade_PORequests "+
                "WHERE (FSPONum = '%PO') "+
                "and (DueDate='%DATE') "+
                "and (VendorID='%VENDOR') "+
                "and (ItemNumber='%ITEM') "+
                "and (Quantity='%QTY')";

        public PendingOrders()
        {
            InitializeComponent();
            dataGridView1.DataSource = CSV2Datatable("orders.csv");
            this.Text = dataGridView1.Rows.Count.ToString() + " Lines %PROCESS";

            DBMNG = new Data_Base_MNG.SQL("SQLSERVER", "FSDBMR", "sa", "6rzq4d1");//el paso
        }
        private DataTable CSV2Datatable(string FileName)
        {
            DataTable table = new DataTable();
            string file = "";

            #region Open_file

            file = FileName;

            List<string> Lines = new List<string>();

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);

            StreamReader reader = new StreamReader(fileStream);

            while (!reader.EndOfStream)
            {
                Lines.Add(reader.ReadLine());
            }

            fileStream.Close();

            #endregion

            //Lines[0];
            try
            {
                string tableHeader = Lines[0];
                tableHeader = tableHeader.Replace("\"", "");
                string[] Headers = tableHeader.Split(',');
                for (int i = 0; i < Headers.Count(); i++)
                {
                    table.Columns.Add(Headers[i], typeof(string));
                }
                //table.Columns.Add("Extra", typeof(string));

                for (int j = 1; j < Lines.Count(); j++)
                {
                    string TableRow = Lines[j].Replace("\"", "");

                    string[] TableData = TableRow.Split(',');
                    table.Rows.Add(TableData);
                }
                return table;
            }
            catch (Exception ex)
            {
                //ProcessingLog(ActiveFileName + " Bad File - " + ex.Message.ToString(), "error");
                //FileMove(file, path + "BadFiles", ActiveFileName);
                return null;
            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyValue==116)
            {
                string Q="";
                DBMNG.Open_Connection();
                string date = "";
                string result = "";
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    Q = query;
                    Q = Q.Replace("%PO", dataGridView1.Rows[i].Cells["order_number"].Value.ToString());
                    date = Convert.ToDateTime(dataGridView1.Rows[i].Cells["order_due"].Value.ToString()).ToString("MM/dd/yyyy");
                    Q = Q.Replace("%DATE", date);
                    Q = Q.Replace("%VENDOR", dataGridView1.Rows[i].Cells["vendor_code"].Value.ToString());
                    Q = Q.Replace("%ITEM", dataGridView1.Rows[i].Cells["part"].Value.ToString());
                    Q = Q.Replace("%QTY", dataGridView1.Rows[i].Cells["order_qty"].Value.ToString());

                    result = DBMNG.Execute_Scalar_Open_Conn(Q).ToString();
                    if (result == "")
                    {
                        dataGridView1.Rows[i].Cells["Exist"].Value = "Not Exist";
                    }
                    else
                    {
                        dataGridView1.Rows[i].Cells["Exist"].Value = result;
                    }
                    this.Text = dataGridView1.Rows.Count.ToString() + " Lines -- Processed Lines: "+i.ToString();
                    this.Refresh();
                }
                DBMNG.Close_Open_Connection();
            }
        }

    }
}
