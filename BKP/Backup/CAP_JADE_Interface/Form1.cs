using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace CAP_JADE_Interface
{
    public partial class Form1 : Form
    {
        Data_Base_MNG.SQL DBMNG_FS = new Data_Base_MNG.SQL("192.168.0.15", "FSDBMR", "amalgamma", "capsonic1");//el paso

        //FTP_Service ftpClient = new ftp(@"ftp://10.10.10.10/", "user", "password");
        FTP_Service ftpClient;
        int timelapse = 0;

        string Jade_ItemMaster = "JadeItemMaster";
        string Jade_ItemFCSTMap = "JadeItemFCSTMap";
        string Jade_POREQ = "JadePOREQ";
        string Jade_VendorMaster = "Jade VendorMaster";

        public Form1()
        {
            InitializeComponent();
            ftpClient = new FTP_Service(FTP_URL.Text, FTP_User.Text, FTP_Pass.Text);

            timelapse = (Convert.ToInt32(FTPTimelapse.Text)) * 3600000;
            timer1.Interval = timelapse;
            MakeExtractionFiles();
            //makeFile();
            ftpUpload();
        }

        private void Table2File(DataTable table,string FileName)
        {
            FileStream fileStream = new FileStream(FileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fileStream);
            string cabecera = "";
            string separator = ",";
            string row = "";
            for (int y = 0; y < table.Columns.Count; y++)
            {
                if (y == (table.Columns.Count - 1))
                {
                    cabecera += table.Columns[y].ColumnName.ToString();
                }
                else
                {
                    cabecera += table.Columns[y].ColumnName.ToString() + separator;
                }
            }
            writer.WriteLine(cabecera);
            for (int j = 0; j < table.Rows.Count; j++)
            {
                row = "";
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (i == (table.Columns.Count - 1))
                    {
                        row += table.Rows[j][i].ToString();
                    }
                    else
                    {
                        row += table.Rows[j][i].ToString() + separator;
                    }
                }
                writer.WriteLine(row);
            }
            writer.Close();
 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] list2 = ftpClient.directoryListDetailed(".");
            string[] list = ftpClient.directoryListSimple("inbox");

            listBox1.Items.Clear();
            listBox1.Items.AddRange(list);
            listBox1.Items.Add("=========================================");
            listBox1.Items.AddRange(list2);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            makeFile();
        }

        private void ftpUpload()
        {
            string response = "";
            //se sube JadeItemMaster.csv
            response = ftpClient.upload(@"inbox/JadeItemMaster.csv", "JadeItemMaster.csv");
            listBox2.Items.Add("");
            listBox2.Items.Add(DateTime.Now.ToString());
            listBox2.Items.Add(response);

            //se sube JadeItemFCSTMap.csv
            response = ftpClient.upload(@"inbox/JadeItemFCSTMap.csv", "JadeItemFCSTMap.csv");
            listBox2.Items.Add("");
            listBox2.Items.Add(DateTime.Now.ToString());
            listBox2.Items.Add(response);

            //se sube JadePOREQ.csv
            response = ftpClient.upload(@"inbox/JadePOREQ.csv", "JadePOREQ.csv");
            listBox2.Items.Add("");
            listBox2.Items.Add(DateTime.Now.ToString());
            listBox2.Items.Add(response);

            //se sube JadeVendorMaster.csv
            response = ftpClient.upload(@"inbox/JadeVendorMaster.csv", "JadeVendorMaster.csv");
            listBox2.Items.Add("");
            listBox2.Items.Add(DateTime.Now.ToString());
            listBox2.Items.Add(response); 
        }

        private void MakeExtractionFiles()
        {
            DataTable table = null;
            string query = "";
            //JadeItemMaster
            query = "SELECT * FROM _CAP_Jade_ItemMaster";
            table = null;
            table = DBMNG_FS.Execute_Query(query);
            Table2File(table, Jade_ItemMaster + ".csv"); 
            listBox2.Items.Add("");
            listBox2.Items.Add(DateTime.Now.ToString() + Jade_ItemMaster + ".csv was created succesfully.");

            //JadeItemFCSTMap
            query = "SELECT * FROM _CAP_Jade_Item_VendorFCSTs";
            table = null;
            table = DBMNG_FS.Execute_Query(query);
            Table2File(table, Jade_ItemFCSTMap + ".csv");
            listBox2.Items.Add("");
            listBox2.Items.Add(DateTime.Now.ToString() + Jade_ItemFCSTMap + ".csv was created succesfully.");

            //JadePOREQ
            query = "SELECT * FROM _CAP_Jade_PORequests";
            table = null;
            table = DBMNG_FS.Execute_Query(query);
            Table2File(table, Jade_POREQ + ".csv");
            listBox2.Items.Add("");
            listBox2.Items.Add(DateTime.Now.ToString() + Jade_POREQ + ".csv was created succesfully.");

            //JadeVendorMaster
            query = "SELECT * FROM _CAP_Jade_VendorMaster";
            table = null;
            table = DBMNG_FS.Execute_Query(query);
            Table2File(table, Jade_VendorMaster + ".csv");
            listBox2.Items.Add("");
            listBox2.Items.Add(DateTime.Now.ToString() + Jade_VendorMaster + ".csv was created succesfully.");

 
        }

        private void makeFile()
        {
            //string query = "SELECT ItemNumber, ItemDescription, ItemUM, ItemRevision, MakeBuyCode, ItemType, ItemStatus, ItemReference1, Planner, Buyer FROM FS_Item";
            //JadeItemMaster
            string query = "SELECT * FROM _CAP_Jade_ItemMaster";
            DataTable table = DBMNG_FS.Execute_Query(query);
            Table2File(table, @"JadeItemMaster.csv");
            dataGridView1.DataSource = table;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ftpUpload();
        }

        private void button4_Click(object sender, EventArgs e)
        {

            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(FTP_URL.Text+"/inbox/file2.txt");
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential(FTP_User.Text, FTP_Pass.Text);

            // Copy the contents of the file to the request stream.
            StreamReader sourceStream = new StreamReader("file.txt");
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            MessageBox.Show("Upload File Complete, status " + response.StatusDescription);

            response.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            MakeExtractionFiles();
            //makeFile();
            ftpUpload();
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                timelapse = (Convert.ToInt32(FTPTimelapse.Text)) * 3600000;
                MessageBox.Show("The Update Time Lapse was changed.");
            }
        }

    }
}
