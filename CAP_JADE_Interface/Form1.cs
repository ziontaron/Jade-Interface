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
using System.Diagnostics;
using System.Threading;

namespace CAP_JADE_Interface
{
    public partial class Form1 : Form
    {
        Data_Base_MNG.SQL DBMNG;// = new Data_Base_MNG.SQL("192.168.0.15", "FSDBMR", "amalgamma", "capsonic1");//el paso
        TOOLS.Dataloger LOGGER; 
        SafeInvoke _safeInvoker = new SafeInvoke();
        //FTP_Service MyFTP = new ftp(@"ftp://10.10.10.10/", "user", "password");
        string ErrorDBinUse = "Data Temporarily In Use ... Please Try Again";

        
        //Titulo y Version
        string Titulo = "Production Jade Interface - Ver: 8.9";
        ////////////////

        ///inifile
        #region Ini File
        TOOLS.INIFile MyINIFile = new TOOLS.INIFile("JADE.ini");
        string ActiveInterface = "";
        List<string> ProductionConf = new List<string>();
        List<string> SandBoxConf = new List<string>();

        #endregion

        bool FSTI_Error_Flag = false;
        bool BadFile_Flag = false;

        #region FTPBehave
        //bool DoDownload = true;
        //bool DoUpload = true;
        #endregion

        #region FTP Setup
        string FTPAddress = "ftp://chq1z1.jadeinnovations.com";
        string FTPUserID = "capsonic@chq1z1.jadeinnovations.com";
        string FTPPass = "dt4qf4dsxm";
        #endregion 

        int timelapse = 0;

        string Jade_ItemMaster = "JadeItemMaster";
        string Jade_ItemFCSTMap = "JadeItemFCSTMap";
        string Jade_POREQ = "JadePOREQ";
        string Jade_VendorMaster = "Jade VendorMaster";

        #region File Processing Tab Conf
        FTP MyFTP;

        string CFG_File = @"M:\Mfgsys\fs.cfg";
        string User = "IMPT";
        string Pass = "fstiapp";
        List<string> listing = new List<string>();

        string path = Environment.CurrentDirectory.ToString();

        List<string> JadePOREQ_File = new List<string>();
        List<string> JadeRECV_File = new List<string>();
        List<string> JadePOCancel_File = new List<string>();
        List<string> JadePOUpdate_File = new List<string>();
        List<string> JadePOUpQTY_File = new List<string>();
        string FileRow = "";
        string ActiveFileName = "";

        int counter = 0;
        bool error = false;
        FS4Amalgamma.AmalgammaFSTI FSTI;

        //Data_Base_MNG.SQL DBMNG = new Data_Base_MNG.SQL("CAPTEST", "FSDBMR", "sa", "6rzq4d1");

        #endregion

        public Form1()
        {
            InitializeComponent();

            if (!IsSingleInstance())
            {
                this.Close();
            }

            VerifyWorkingFolders();
            LOGGER = new TOOLS.Dataloger("JadeLog", "log", "");

            ///Ini File Config
            #region INIFILE
            ActiveInterface = MyINIFile.GetValue("Active Interface", "Inteface", "");

            //load produccion configuration
            ProductionConf.Add(MyINIFile.GetValue("Production", "URL", ""));
            ProductionConf.Add(MyINIFile.GetValue("Production", "User", ""));
            ProductionConf.Add(MyINIFile.GetValue("Production", "Pass", ""));
            ProductionConf.Add(MyINIFile.GetValue("Production", "Server", ""));

            //load SandBox configuration
            SandBoxConf.Add(MyINIFile.GetValue("SandBox", "URL", ""));
            SandBoxConf.Add(MyINIFile.GetValue("SandBox", "User", ""));
            SandBoxConf.Add(MyINIFile.GetValue("SandBox", "Pass", ""));
            SandBoxConf.Add(MyINIFile.GetValue("SandBox", "Server", ""));

            if (ActiveInterface == "Production")
            {
                FTP_URL.Text = ProductionConf[0];
                FTP_User.Text = ProductionConf[1];
                FTP_Pass.Text = ProductionConf[2];
                SQLServer.Text = ProductionConf[3];                
            }
            else
            {
                FTP_URL.Text = SandBoxConf[0];
                FTP_User.Text = SandBoxConf[1];
                FTP_Pass.Text = SandBoxConf[2];
                SQLServer.Text = SandBoxConf[3];
            }
            DownloadInbox.Checked = Convert.ToBoolean(MyINIFile.GetValue("Startup Configuration", "DownloadInbox", ""));
            UploadOutbox.Checked = Convert.ToBoolean(MyINIFile.GetValue("Startup Configuration", "UploadOutbox", ""));

            //MyFTP = new FTP_Service(FTP_URL.Text, FTP_User.Text, FTP_Pass.Text);

            //FTP File Update Time
            FTPTimelapse.Text = MyINIFile.GetValue("Startup Configuration", "FTPUpdate", "");
            timelapse = Convert.ToInt32((float.Parse(FTPTimelapse.Text)) * 3600000);
            DBFiles.Interval = timelapse;
            //

            MyFTP = new FTP(FTPAddress, FTPUserID, FTPPass);
            
            VerifyFTP();
            #endregion
            ///

            //Titulo y Version
            this.Text = Titulo;
            ////////////////

            //DBMNG = new Data_Base_MNG.SQL(SQLServer.Text, "FSDBMR", "sa", "6rzq4d1");//el paso


            DBMNG = new Data_Base_MNG.SQL(SQLServer.Text, "FSDBMR", "AmalAdmin", "Amalgamma16");//el paso

            FSTI = new FS4Amalgamma.AmalgammaFSTI(CFG_File, User, Pass);

            Up_Since.Text = "Up Since: " + DateTime.Now.ToString("MM/dd/yyy hh:mm tt");

            //MakeExtractionFiles(JadeItemMaster_Make.Checked, JadeItemFCSTMap_Make.Checked, JadePOREQ_Make.Checked, Jade_VendorMaster_Make.Checked);
            //ftpUpload(JadeItemMaster_FTP.Checked, JadeItemFCSTMap_FTP.Checked, JadePOREQ_FTP.Checked, Jade_VendorMaster_FTP.Checked);
        }

        private bool IsSingleInstance()
        {
            string proceso = "", esta = "", name = "";
            int count = 0;
            foreach (Process process in Process.GetProcesses())
            {
                name = process.ProcessName;
                proceso = process.MainWindowTitle;
                esta = this.Text;
                if (process.MainWindowTitle == this.Text)
                {
                    //MessageBox.Show(name + " " + proceso + " " + esta+" "+count.ToString());
                    count++;
                }
            }
            if (count <= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region General Functions
        public void VerifyWorkingFolders()
        {
            path = Environment.CurrentDirectory.ToString() + "\\working\\";
            PathExist(path);
            PathExist(path + "\\Outbox");
            PathExist(path + "\\Inbox");
            PathExist(path + "\\Archive");

        }
        private void PathExist(string Path2Check)
        {
            //string path = @"C:\MP_Upload";
            if (!Directory.Exists(Path2Check))
            {
                Directory.CreateDirectory(Path2Check);
            }
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            FTPdataGridView1.Size = new Size(FTPdataGridView1.Size.Width, (this.Size.Height - 265));

            dataGridView1.Size = new Size(dataGridView1.Size.Width, (this.Size.Height - 190));
        }
        private void Capa_sys_SelectedIndexChanged(object sender, EventArgs e)
        {
            string CapaSystem = Capa_sys.SelectedItem.ToString();

            switch (CapaSystem)
            {
                case "LIVE SYSTEM":
                    {
                        
                        //FTP_URL.Text = "ftp://cjm7cu4m.jadeinnovations.com";
                        //FTP_User.Text = "capsonic@cjm7cu4m.jadeinnovations.com";
                        //FTP_Pass.Text = "fpe1ck9phd";
                        //SQLServer.Text = "SQLSERVER";

                        
                        FTP_URL.Text = ProductionConf[0];
                        FTP_User.Text = ProductionConf[1];
                        FTP_Pass.Text = ProductionConf[2];
                        SQLServer.Text = ProductionConf[3];

                        VerifyFTP();
                        MessageBox.Show("LIVE SYSTEM - SELECTED");
                        break;
                    }
                case "SAND BOX":
                    {
                        //FTP_URL.Text = "ftp://chq1z1.jadeinnovations.com";
                        //FTP_User.Text = "capsonic@chq1z1.jadeinnovations.com";
                        //FTP_Pass.Text = "dt4qf4dsxm";
                        //SQLServer.Text = "CAPTEST";
                        
                        FTP_URL.Text = SandBoxConf[0];
                        FTP_User.Text = SandBoxConf[1];
                        FTP_Pass.Text = SandBoxConf[2];
                        SQLServer.Text = SandBoxConf[3];

                        VerifyFTP();
                        MessageBox.Show("SAND BOX - SELECTED");
                        break;
                    }
            }
            DBMNG = null;
            DBMNG = new Data_Base_MNG.SQL(SQLServer.Text, "FSDBMR", "sa", "6rzq4d1");//el paso
//                    LIVE SYSTEM
//SAND BOX

        }
        private void VerifyFTP()
        {
            RefreshFTPSetup();
            string[] list2 = MyFTP.directoryListDetailed(".");
            string[] list = MyFTP.directoryListSimple("inbox");

            listBox1.Items.Clear();
            listBox1.Items.AddRange(list);
            listBox1.Items.Add("=========================================");
            listBox1.Items.AddRange(list2);
        }
        #endregion

        #region File Transfer Tab
        private bool IsNotEmpty(string FileCheck)
        {
            string text = File.ReadAllText(FileCheck);
            int size = text.Length;
            if (size > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            VerifyFTP();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //MakeExtractionFiles(true, true, true, true);

            //MakeExtractionFiles(JadeItemMaster_Make.Checked, JadeItemFCSTMap_Make.Checked, JadePOREQ_Make.Checked, Jade_VendorMaster_Make.Checked);
            Thread t2 = new Thread(() => MakeExtractionFiles(JadeItemMaster_Make.Checked, JadeItemFCSTMap_Make.Checked, JadePOREQ_Make.Checked, Jade_VendorMaster_Make.Checked));
            t2.Start();
        }
        private DataTable CSV2Datatable()
        {
            DataTable table = new DataTable();

            #region Open_file
            string file = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                file = openFileDialog1.FileName;
                try
                {
                    string text = File.ReadAllText(file);
                    int size = text.Length;
                }
                catch (IOException)
                {
                }
            }

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
            string tableHeader = Lines[0];
            tableHeader = tableHeader.Replace("\"", "");
            string[] Headers = tableHeader.Split(',');
            for (int i = 0; i < Headers.Count(); i++)
            {
                table.Columns.Add(Headers[i], typeof(string));
            }

            for (int j = 1; j < Lines.Count(); j++)
            {
                string TableRow = Lines[j].Replace("\"", "");
                TableRow = Lines[j].Replace("'", "");

                string[] TableData = TableRow.Split(',');
                table.Rows.Add(TableData);
            }
            return table;
        }
        //Cross thread Support
        public void FTPlistBox2CrossThread(string text)
        {
            if (this.FTPlistBox2.InvokeRequired)
            {
                this.FTPlistBox2.BeginInvoke(
                    new MethodInvoker(
                    delegate() { FTPlistBox2CrossThread(text); }));
            }
            else
            {
                this.FTPlistBox2.Items.Add(text);
            }
        }
        //
        private void ftpUpload(bool JadeItemMaster, bool JadeItemFCSTMap, bool JadePOREQ, bool JadeVendorMaster)
        {
            string response = "";
            //se sube JadeItemMaster.csv
            if (JadeItemMaster)
            {
                response = MyFTP.upload(@"inbox/JadeItemMaster.csv", Jade_ItemMaster + ".csv");
                FTPlistBox2CrossThread("");
                FTPlistBox2CrossThread(DateTime.Now.ToString());
                FTPlistBox2CrossThread(response);
                LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, "FTP: JadeItemMaster.csv " + response);
            }

            //se sube JadeItemFCSTMap.csv
            if (JadeItemFCSTMap)
            {
                response = MyFTP.upload(@"inbox/JadeItemFCSTMap.csv", Jade_ItemFCSTMap + ".csv");
                FTPlistBox2CrossThread("");
                FTPlistBox2CrossThread(DateTime.Now.ToString());
                FTPlistBox2CrossThread(response);
                //FTPlistBox2.Items.Add("");
                //FTPlistBox2.Items.Add(DateTime.Now.ToString());
                //FTPlistBox2.Items.Add(response);
                LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, "FTP: JadeItemMaster.csv " + response);
            }

            //se sube JadePOREQ.csv
            if (JadePOREQ)
            {
                response = MyFTP.upload(@"inbox/JadePOREQ.csv", Jade_POREQ + ".csv");
                FTPlistBox2CrossThread("");
                FTPlistBox2CrossThread(DateTime.Now.ToString());
                FTPlistBox2CrossThread(response);
                //FTPlistBox2.Items.Add("");
                //FTPlistBox2.Items.Add(DateTime.Now.ToString());
                //FTPlistBox2.Items.Add(response);
                LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, "FTP: JadePOREQ.csv " + response);
            }

            //se sube JadeVendorMaster.csv            
            if (JadeVendorMaster)
            {
                response = MyFTP.upload(@"inbox/JadeVendorMaster.csv", Jade_VendorMaster + ".csv");
                FTPlistBox2CrossThread("");
                FTPlistBox2CrossThread(DateTime.Now.ToString());
                FTPlistBox2CrossThread(response);
                //FTPlistBox2.Items.Add("");
                //FTPlistBox2.Items.Add(DateTime.Now.ToString());
                //FTPlistBox2.Items.Add(response);
                LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, "FTP: JadeVendorMaster.csv " + response);
            }
        }
        private void MakeExtractionFiles(bool JadeItemMaster, bool JadeItemFCSTMap, bool JadePOREQ, bool JadeVendorMaster)
        {
            DataTable table = null;
            string query = "";

            //JadeItemMaster
            if (JadeItemMaster)
            {
                query = "SELECT * FROM _CAP_Jade_ItemMaster_Extraction WITH (NOLOCK) ORDER BY VendorID, ItemNumber, BKPOExpireDate";
                table = null;
                table = DBMNG.Execute_Query(query);
                if (DBMNG.ErrorOccur)
                {
                    MessageBox.Show(DBMNG.Error_Mjs, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (table.Rows.Count > 0)
                {
                    Table2File(table, Jade_ItemMaster + ".csv");


                    FTPlistBox2CrossThread("");
                    FTPlistBox2CrossThread(DateTime.Now.ToString() + Jade_ItemMaster + ".csv was created succesfully.");

                    //FTPlistBox2.Items.Add("");
                    //FTPlistBox2.Items.Add(DateTime.Now.ToString() + Jade_ItemMaster + ".csv was created succesfully.");
                    if (IsNotEmpty(Jade_ItemMaster + ".csv"))
                    {
                        LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, Jade_ItemMaster + ".csv was created succesfully.");
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(15000);
                        MakeExtractionFiles(true, false, false, false);
                    }
                }
            }

            //JadeItemFCSTMap
            if (JadeItemFCSTMap)
            {
                query = "SELECT * FROM _CAP_Jade_Item_VendorFCSTs_Extraction WITH (NOLOCK)";
                table = null;
                table = DBMNG.Execute_Query(query);
                if (DBMNG.ErrorOccur)
                {
                    MessageBox.Show(DBMNG.Error_Mjs, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (table.Rows.Count > 0)
                {
                    Table2File(table, Jade_ItemFCSTMap + ".csv");
                    FTPlistBox2CrossThread("");
                    FTPlistBox2CrossThread(DateTime.Now.ToString() + Jade_ItemFCSTMap + ".csv was created succesfully.");

                    //FTPlistBox2.Items.Add("");
                    //FTPlistBox2.Items.Add(DateTime.Now.ToString() + Jade_ItemFCSTMap + ".csv was created succesfully.");

                    if (IsNotEmpty(Jade_ItemFCSTMap + ".csv"))
                    {
                        LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, Jade_ItemFCSTMap + ".csv was created succesfully.");
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(15000);
                        MakeExtractionFiles(false, true, false, false);
                    }
                }
            }


            //JadePOREQ
            if (JadePOREQ)
            {
                query = "SELECT * FROM _CAP_Jade_PORequests_Extraction WITH (NOLOCK)";
                table = null;
                table = DBMNG.Execute_Query(query);
                if (DBMNG.ErrorOccur)
                {
                    MessageBox.Show(DBMNG.Error_Mjs, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (table.Rows.Count > 0)
                {
                    Table2File(table, Jade_POREQ + ".csv");

                    FTPlistBox2CrossThread("");
                    FTPlistBox2CrossThread(DateTime.Now.ToString() + Jade_POREQ + ".csv was created succesfully.");

                    //FTPlistBox2.Items.Add("");
                    //FTPlistBox2.Items.Add(DateTime.Now.ToString() + Jade_POREQ + ".csv was created succesfully.");

                    if (IsNotEmpty(Jade_POREQ + ".csv"))
                    {
                        LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, Jade_POREQ + ".csv was created succesfully.");
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(15000);
                        MakeExtractionFiles(false, false, true, false);
                    }
                }
            }

            //JadeVendorMaster
            if (JadeVendorMaster)
            {
                query = "SELECT * FROM _CAP_Jade_VendorMaster_Extraction WITH (NOLOCK)";
                table = null;
                table = DBMNG.Execute_Query(query);
                if (DBMNG.ErrorOccur)
                {
                    MessageBox.Show(DBMNG.Error_Mjs, "ERROR",MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (table.Rows.Count > 1)
                {
                    Table2File(table, Jade_VendorMaster + ".csv");

                    FTPlistBox2CrossThread("");
                    FTPlistBox2CrossThread(DateTime.Now.ToString() + Jade_VendorMaster + ".csv was created succesfully.");

                    //FTPlistBox2.Items.Add("");
                    //FTPlistBox2.Items.Add(DateTime.Now.ToString() + Jade_VendorMaster + ".csv was created succesfully.");

                    if (IsNotEmpty(Jade_VendorMaster + ".csv"))
                    {
                        LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, Jade_VendorMaster + ".csv was created succesfully.");
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(15000);
                        MakeExtractionFiles(false, false, false, true);
                    }
                }
            }

        }
        private void LoadView(string View)
        {
            string query = "";
            switch (View)
            {
                case "_CAP_Jade_VendorMaster":
                    {
                        query = "SELECT * FROM _CAP_Jade_VendorMaster";
                        FTPdataGridView1.DataSource = null;
                        FTPdataGridView1.DataSource = DBMNG.Execute_Query(query);
                        break;
                    }
                case "_CAP_Jade_Item_VendorFCSTs":
                    {
                        query = "SELECT * FROM _CAP_Jade_Item_VendorFCSTs";
                        FTPdataGridView1.DataSource = null;
                        FTPdataGridView1.DataSource = DBMNG.Execute_Query(query);
                        break;
                    }
                case "_CAP_Jade_ItemMaster":
                    {
                        query = "SELECT * FROM _CAP_Jade_ItemMaster";
                        FTPdataGridView1.DataSource = null;
                        FTPdataGridView1.DataSource = DBMNG.Execute_Query(query);
                        break;
                    }
                case "_CAP_Jade_PORequests":
                    {
                        query = "SELECT * FROM _CAP_Jade_PORequests";
                        FTPdataGridView1.DataSource = null;
                        FTPdataGridView1.DataSource = DBMNG.Execute_Query(query);
                        break;
                    }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //ftpUpload(JadeItemMaster_FTP.Checked, JadeItemFCSTMap_FTP.Checked, JadePOREQ_FTP.Checked, Jade_VendorMaster_FTP.Checked);
            Thread t2 = new Thread(() => ftpUpload(JadeItemMaster_FTP.Checked, JadeItemFCSTMap_FTP.Checked, JadePOREQ_FTP.Checked, Jade_VendorMaster_FTP.Checked));
            t2.Start();
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
        private void DBFilesTread()
        {
            RefreshFTPSetup();
            MakeExtractionFiles(JadeItemMaster_Make.Checked, JadeItemFCSTMap_Make.Checked, JadePOREQ_Make.Checked, Jade_VendorMaster_Make.Checked);
            ftpUpload(JadeItemMaster_FTP.Checked, JadeItemFCSTMap_FTP.Checked, JadePOREQ_FTP.Checked, Jade_VendorMaster_FTP.Checked);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            Thread MakeFTPFiles = new Thread(new ThreadStart(DBFilesTread));
            MakeFTPFiles.Start();
            //RefreshFTPSetup();
            //MakeExtractionFiles(JadeItemMaster_Make.Checked, JadeItemFCSTMap_Make.Checked, JadePOREQ_Make.Checked, Jade_VendorMaster_Make.Checked);
            //ftpUpload(JadeItemMaster_FTP.Checked, JadeItemFCSTMap_FTP.Checked, JadePOREQ_FTP.Checked, Jade_VendorMaster_FTP.Checked);
        }
        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                timelapse = (Convert.ToInt32(FTPTimelapse.Text)) * 3600000;
                MessageBox.Show("The Update Time Lapse was changed.");
            }
        }

        #region Manual Control

        #region Individual Making
        private void JadeVendorMasterManual_Make_Click(object sender, EventArgs e)
        {
            MakeExtractionFiles(false, false, false, true);
        }
        private void JadeItemFCSTMapManual_Make_Click(object sender, EventArgs e)
        {
            MakeExtractionFiles(false, true, false, false);
        }
        private void JadeItemMasterManual_Make_Click(object sender, EventArgs e)
        {
            MakeExtractionFiles(true, false, false, false);
        }
        private void JadePOREQManual_Make_Click(object sender, EventArgs e)
        {
            MakeExtractionFiles(false, false, true, false);
        }
        #endregion
        #region Individual FTP Uploading
        private void Jade_VendorMasterManual_FTP_Click(object sender, EventArgs e)
        {

            string response = "";
            response = MyFTP.upload(@"inbox/JadeVendorMaster.csv", Jade_VendorMaster + ".csv");
            FTPlistBox2.Items.Add("");
            FTPlistBox2.Items.Add(DateTime.Now.ToString());
            FTPlistBox2.Items.Add(response);
            LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, "FTP: JadeVendorMaster.csv " + response);
        }
        private void JadeItemFCSTMapManual_FTP_Click(object sender, EventArgs e)
        {

            string response = "";
            response = MyFTP.upload(@"inbox/JadeItemFCSTMap.csv", Jade_ItemFCSTMap + ".csv");
            FTPlistBox2.Items.Add("");
            FTPlistBox2.Items.Add(DateTime.Now.ToString());
            FTPlistBox2.Items.Add(response);
            LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, "FTP: JadeItemFCSTMap.csv " + response);
        }
        private void JadeItemMasterManual_FTP_Click(object sender, EventArgs e)
        {

            string response = "";
            response = MyFTP.upload(@"inbox/JadeItemMaster.csv", Jade_ItemMaster + ".csv");
            FTPlistBox2.Items.Add("");
            FTPlistBox2.Items.Add(DateTime.Now.ToString());
            FTPlistBox2.Items.Add(response);
            LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, "FTP: JadeItemMaster.csv " + response);
        }
        private void JadePOREQManual_FTP_Click(object sender, EventArgs e)
        {
            string response = "";
            response = MyFTP.upload(@"inbox/JadePOREQ.csv", Jade_POREQ + ".csv");
            FTPlistBox2.Items.Add("");
            FTPlistBox2.Items.Add(DateTime.Now.ToString());
            FTPlistBox2.Items.Add(response);
            LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, "FTP: JadePOREQ.csv " + response);
        }
        #endregion
        private void button2_Click_1(object sender, EventArgs e)
        {
            FTPdataGridView1.DataSource = CSV2Datatable();
        }
        private void button7_Click(object sender, EventArgs e)
        {
            LoadView("_CAP_Jade_VendorMaster");
        }
        private void button6_Click(object sender, EventArgs e)
        {
            LoadView("_CAP_Jade_Item_VendorFCSTs");
        }
        private void button5_Click(object sender, EventArgs e)
        {
            LoadView("_CAP_Jade_ItemMaster");
        }
        private void button4_Click_1(object sender, EventArgs e)
        {
            LoadView("_CAP_Jade_PORequests");
            
        }
        #endregion

        #endregion

        #region File Processing

        #region Funtions
        private string process(string FIELDS, string func)
        {
            string response = "";
            FSTI_Error_Flag = false;

            try
            {
                if (FSTI.AmalgammaFSTI_Initialization())
                {
                    if (FSTI.AmalgammaFSTI_Logon())
                    {
                        switch (func)
                        {
                            #region POMT10
                            case "POMT":
                                {
                                    if (FSTI.AmalgammaFSTI_POMT10(FIELDS, "JadeInterface"))
                                    {
                                        //log(FIELDS + " - Transaction was successfully processed.");
                                        ProcessingLog(ActiveFileName + " POMT " + FIELDS + " - Transaction was successfully processed.", "info");
                                        counter++;
                                        response = "";
                                    }
                                    else
                                    {
                                        //MessageBox.Show(FSTI.Trans_Error_Msg, "Error During Procesing Transaction");
                                        string[] error = FSTI.DetailError.ToArray();
                                        LogListBox.Items.AddRange(error);

                                        ProcessingLog(ActiveFileName + " Error During Procesing Transaction - " + FSTI.Trans_Error_Msg, "error");
                                        response = FSTI.Trans_Error_Msg;
                                    }
                                    break;
                                }
                            #endregion
                            #region POMT11
                            case "POMT11":
                                {
                                    if (FSTI.AmalgammaFSTI_POMT11(FIELDS, "JadeInterface"))
                                    {
                                        //log(FIELDS + " - Transaction was successfully processed.");
                                        ProcessingLog(ActiveFileName + " POMT11 " + FIELDS + " - Transaction was successfully processed.", "info");
                                        counter++;
                                        response = "";
                                    }
                                    else
                                    {
                                        //MessageBox.Show(FSTI.Trans_Error_Msg, "Error During Procesing Transaction");
                                        ProcessingLog(ActiveFileName + " Error During Procesing Transaction - " + FSTI.Trans_Error_Msg, "error");
                                        string[] error = FSTI.DetailError.ToArray();
                                        LogListBox.Items.AddRange(error);
                                        response = FSTI.Trans_Error_Msg;
                                    }
                                    break;
                                }
                            #endregion
                            #region PORV01
                            case "PORV":
                                {
                                    if (FSTI.AmalgammaFSTI_PORV01(FIELDS, "JadeInterface"))
                                    {
                                        //log(FIELDS + " - Transaction was successfully processed.");
                                        ProcessingLog(ActiveFileName + " PORV " + FIELDS + " - Transaction was successfully processed.", "info");
                                        counter++;
                                        response = "";
                                    }
                                    else
                                    {
                                        //MessageBox.Show(FSTI.Trans_Error_Msg, "Error During Procesing Transaction");
                                        ProcessingLog(ActiveFileName + " Error During Procesing Transaction - " + FSTI.Trans_Error_Msg, "error");
                                        string[] error = FSTI.DetailError.ToArray();
                                        LogListBox.Items.AddRange(error);
                                        response = FSTI.Trans_Error_Msg;
                                    }

                                    break;
                                }
                            #endregion
                            #region POMT16
                            case "POMT16":
                                {
                                    if (FSTI.AmalgammaFSTI_POMT16(FIELDS, "JadeInterface"))
                                    {
                                        //log(FIELDS + " - Transaction was successfully processed.");
                                        ProcessingLog(ActiveFileName + " POMT16 " + FIELDS + " - Transaction was successfully processed.", "info");
                                        counter++;
                                        response = "";
                                    }
                                    else
                                    {
                                        //MessageBox.Show(FSTI.Trans_Error_Msg, "Error During Procesing Transaction");
                                        ProcessingLog(ActiveFileName + " Error During Procesing Transaction - " + FSTI.Trans_Error_Msg, "error");
                                        string[] error = FSTI.DetailError.ToArray();
                                        LogListBox.Items.AddRange(error);
                                        response = FSTI.Trans_Error_Msg;
                                    }
                                    break;
                                }
                            #endregion
                            #region POMT12
                            case "POMT12":
                                {
                                    if (FSTI.AmalgammaFSTI_POMT12(FIELDS, "JadeInterface"))
                                    {
                                        //log(FIELDS + " - Transaction was successfully processed.");
                                        ProcessingLog(ActiveFileName + " POMT12 " + FIELDS + " - Transaction was successfully processed.", "info");
                                        counter++;
                                        response = "";
                                    }
                                    else
                                    {
                                        //MessageBox.Show(FSTI.Trans_Error_Msg, "Error During Procesing Transaction");
                                        ProcessingLog(ActiveFileName + " Error During Procesing Transaction - " + FSTI.Trans_Error_Msg, "error");
                                        string[] error = FSTI.DetailError.ToArray();
                                        LogListBox.Items.AddRange(error);
                                        response = FSTI.Trans_Error_Msg;
                                    }
                                    break;
                                }
                            #endregion
                            #region POMT12UPD
                            case "POMT12UPD":
                                {
                                    if (FSTI.AmalgammaFSTI_POMT12_UPD(FIELDS, "JadeInterface"))
                                    {
                                        //log(FIELDS + " - Transaction was successfully processed.");
                                        ProcessingLog(ActiveFileName + " POMT12UPD " + FIELDS + " - Transaction was successfully processed.", "info");
                                        counter++;
                                        response = "";
                                    }
                                    else
                                    {
                                        //MessageBox.Show(FSTI.Trans_Error_Msg, "Error During Procesing Transaction");
                                        ProcessingLog(ActiveFileName + " Error During Procesing Transaction - " + FSTI.Trans_Error_Msg, "error");
                                        string[] error = FSTI.DetailError.ToArray();
                                        LogListBox.Items.AddRange(error);
                                        response = FSTI.Trans_Error_Msg;
                                    }
                                    break;
                                }
                            #endregion
                            #region POMT12UPQ
                            case "POMT12UPQ":
                                {
                                    if (FSTI.AmalgammaFSTI_POMT12_UPQ(FIELDS, "JadeInterface"))
                                    {
                                        //log(FIELDS + " - Transaction was successfully processed.");
                                        ProcessingLog(ActiveFileName + " POMT12UPQ " + FIELDS + " - Transaction was successfully processed.", "info");
                                        counter++;
                                        response = "";
                                    }
                                    else
                                    {
                                        //MessageBox.Show(FSTI.Trans_Error_Msg, "Error During Procesing Transaction");
                                        ProcessingLog(ActiveFileName + " Error During Procesing Transaction - " + FSTI.Trans_Error_Msg, "error");
                                        string[] error = FSTI.DetailError.ToArray();
                                        LogListBox.Items.AddRange(error);
                                        response = FSTI.Trans_Error_Msg;
                                    }
                                    break;
                                }
                            #endregion
                            //#region POMT12POUPL
                            //case "POMT12":
                            //    {
                            //        if (FSTI.AmalgammaFSTI_POMT12(FIELDS, "JadeInterface"))
                            //        {
                            //            //log(FIELDS + " - Transaction was successfully processed.");
                            //            ProcessingLog(ActiveFileName + " POMT12 " + FIELDS + " - Transaction was successfully processed.", "info");
                            //            counter++;
                            //            response = "";
                            //        }
                            //        else
                            //        {
                            //            //MessageBox.Show(FSTI.Trans_Error_Msg, "Error During Procesing Transaction");
                            //            ProcessingLog(ActiveFileName + " Error During Procesing Transaction - " + FSTI.Trans_Error_Msg, "error");
                            //            string[] error = FSTI.DetailError.ToArray();
                            //            LogListBox.Items.AddRange(error);
                            //            response = FSTI.Trans_Error_Msg;
                            //        }
                            //        break;
                            //    }
                            //#endregion
                        }

                    }
                    else
                    {
                        //MessageBox.Show(FSTI.ErrorMsg, "Error During Login");
                        ProcessingLog("FSTI-Error During Login - " + FSTI.FSTI_ErrorMsg, "error");
                        response = "FSTI-Error During Login - " + FSTI.FSTI_ErrorMsg;
                        FSTI_Error_Flag = true;
                    }
                }
                else
                {
                    //MessageBox.Show(FSTI.ErrorMsg, "Error During FSTI Inicialitation");
                    ProcessingLog("FSTI-Error During FSTI Inicialitation - " + FSTI.FSTI_ErrorMsg, "error");
                    response = "FSTI-Error During FSTI Inicialitation - " + FSTI.FSTI_ErrorMsg;
                    FSTI_Error_Flag = true;
                }
            }
            catch (Exception ex)
            {
                ProcessingLog("FSTI-Error Exeption - " + ex.Message, "error");
                //MessageBox.Show(ex.Message, "Error FSTI Exeption");
                error = true;
                FSTI_Error_Flag = true;
                FSTI.AmalgammaFSTI_Stop();
            }
            //response == ErrorDBinUse
            if (FSTI.DBinUseFlag)
            {
                FSTI_Error_Flag = true;                
            }
            return response;
        }//this function Inputs the transaction
        private string OpenFileDialog()
        {
            DataTable table = new DataTable();

            string file = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                file = openFileDialog1.FileName;
                //try
                //{
                //    string text = File.ReadAllText(file);
                //    int size = text.Length;
                //}
                //catch (IOException)
                //{
                //}
            }
            return file;

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
                    //string TableRow = Lines[j].Replace("\"", "");
                    //string[] TableData = TableRow.Split(',');
                    string TableRow = Lines[j].Replace("\",\"", "|");
                    TableRow = TableRow.Replace("\"", "");
                    string[] TableData = TableRow.Split('|');

                    table.Rows.Add(TableData);
                }
                return table;
            }
            catch (Exception ex)
            {
                ProcessingLog(ActiveFileName + " Bad File - " + ex.Message.ToString(), "error");
                //FileMove( file,  path+ "BadFiles",ActiveFileName);
                BadFile_Flag = true;
                return null;
            }
        }
        private void POMT10()
        {
            string fields = "";
            DataTable table = null;
            //PO_Number, Line_QTY, Line_Status, Line_Type, Item_Num, Prom_Date, Blanket
            //0        , 1       , 2          , 3        , 4       , 5        , 6

            string PONumber = "", LineQTY = "", Line_Sta = "", Line_Type = "", ItemNum = "", PromDate = "", Blanket = "", ItemUM = "", ItemUnitCost = "";

            string query = "SELECT ItemUM, FS_PONumber, ItemUnitCost, VendorPriority FROM _CAP_Jade_ItemMaster WHERE (VendorID = '@VendorId') AND (ItemNumber = '@PartNum')";
            string Query = "";

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (!error)
                {
                    if (dataGridView1.Rows.Count > 0)
                    {
                        Query = query.Replace("@VendorId", dataGridView1.Rows[i].Cells[7].Value.ToString());
                        Query = Query.Replace("@PartNum", dataGridView1.Rows[i].Cells[8].Value.ToString());

                        table = null;
                        table = DBMNG.Execute_Query(Query);
                        PONumber = table.Rows[0]["FS_PONumber"].ToString();
                        ItemUM = table.Rows[0]["ItemUM"].ToString();
                        ItemUnitCost = table.Rows[0]["ItemUnitCost"].ToString();

                        string[] ItemCost = ItemUnitCost.Split('.');

                        if (ItemUnitCost.Length > 5)
                        {
                            //ItemUnitCost = ItemUnitCost.Remove(5);
                            ItemUnitCost = ItemCost[0] + "." + ItemCost[1].Remove(2);
                        }

                        LineQTY = dataGridView1.Rows[i].Cells[10].Value.ToString();
                        Line_Sta = "4";
                        Line_Type = "S";

                        ItemNum = dataGridView1.Rows[i].Cells[8].Value.ToString();

                        PromDate = DateTime.Now.ToString("MMddyy");
                        Blanket = "";

                        //PO_Number, Line_QTY, Line_Status, Line_Type, Item_Num, Prom_Date, Blanket, UM, Unit_price
                        fields = PONumber + "," + LineQTY + "," + Line_Sta + "," + Line_Type + "," + ItemNum + "," + PromDate + "," + Blanket + "," +
                          ItemUM + "," + ItemUnitCost;
                        process(fields, "POMT");
                    }
                }
                else
                {
                    i = dataGridView1.Rows.Count;
                }
            }
            if (!error)
            {
                //MessageBox.Show("Operation Ended Succesfully");
            }

        }
        private void POMT11()
        {
            string Errormsg = "";
            string PO_LnKey = "";
            string Vendor_ID = "";
            string fields = "";
            DataTable table = null;
            //PO_Number, PO_Line, Line_QTY, Line_Status, Item_Num, StartDate, EndDate, Unit_price, PromDate, Line_Type
            //0        , 1       , 2      , 3          , 4       , 5        , 6      , 7         , 8       , 9

            string PONumber = "", PO_Line = "", LineQTY = "", Line_Sta = "", ItemNum = "", StartDate = "", EndDate = "", ItemUnitCost = "", PromDate = "", Line_Type = "";

            string query = "SELECT ItemUM, FS_PONumber, ItemUnitCost, VendorPriority FROM _CAP_Jade_ItemMaster WITH (NOLOCK) WHERE (VendorID = '@VendorId') AND (ItemNumber = '@PartNum') " +
                " ORDER BY VendorID, ItemNumber, BKPOExpireDate DESC";

            string query2 = "SELECT     FS_POHeader.PONumber, FS_POLine.POLineNumber, FS_POLine.POLineKey " +
                    ", REPLACE (CONVERT(VARCHAR(8), FS_POLine.NeededDate, 1),'/','') AS Needed_Date " +
                    ", REPLACE (CONVERT(VARCHAR(8), FS_POLine.OriginalPromisedDate, 1),'/','') AS OriginalPromised_Date " +
                    ", REPLACE (CONVERT(VARCHAR(8), FS_POLine.StartDate, 1),'/','') AS Start_Date, FS_POHeader.VendorID " +
                    ", FS_Item.ItemNumber, FS_POLine.POLineNumber " +
                    "FROM FS_POHeader WITH (NOLOCK) INNER JOIN FS_POLine WITH (NOLOCK) ON FS_POHeader.POHeaderKey = FS_POLine.POHeaderKey  " +
                    "INNER JOIN FS_Item ON FS_POLine.ItemKey = FS_Item.ItemKey " +
                    "WHERE (FS_POHeader.VendorID = '@VendorId') AND (FS_Item.ItemNumber = '@PartNum') AND (FS_POHeader.PONumber='@PONumber') "+
                    " AND (FS_POLine.POLineStatus < '5') AND (FS_POLine.POLineSubType = 'B')";

            string Query = "";
            string Query2 = "";

            try
            {

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (!error)
                    {
                        if (dataGridView1.Rows.Count > 0)
                        {
                            ItemNum = dataGridView1.Rows[i].Cells[7].Value.ToString();
                            Vendor_ID = dataGridView1.Rows[i].Cells[6].Value.ToString();
                            Query = query.Replace("@VendorId", Vendor_ID);
                            Query = Query.Replace("@PartNum", ItemNum);

                            table = null;
                            
                            table = DBMNG.Execute_Query(Query);

                            if (table.Rows.Count > 0 && table.Rows[0]["FS_PONumber"].ToString() != "")
                            {
                                #region GoodFile
                                #region PO Search
                                PONumber = table.Rows[0]["FS_PONumber"].ToString();
                                ItemUnitCost = table.Rows[0]["ItemUnitCost"].ToString();

                                //ItemNum = dataGridView1.Rows[i].Cells[8].Value.ToString();

                                string[] ItemCost = ItemUnitCost.Split('.');

                                Query2 = query2.Replace("@VendorId", Vendor_ID);
                                Query2 = Query2.Replace("@PartNum", ItemNum);
                                Query2 = Query2.Replace("@PONumber", PONumber);

                                DataTable tabe2 = DBMNG.Execute_Query(Query2);


                                //if (ItemUnitCost.Length > 5)
                                //{
                                //    //ItemUnitCost = ItemUnitCost.Remove(5);

                                //    ItemUnitCost = ItemCost[0] + "." + ItemCost[1].Remove(2);
                                //}

                                //DATES
                                string date = dataGridView1.Rows[i].Cells[5].Value.ToString();
                                date = Convert.ToDateTime(date).ToString("MMddyy");
                                StartDate = "";
                                EndDate = date;
                                PromDate = date;
                                //

                                LineQTY = dataGridView1.Rows[i].Cells[9].Value.ToString();
                                PO_Line = tabe2.Rows[0]["POLineNumber"].ToString();

                                if (PO_Line.Length < 3)
                                {
                                    if (PO_Line.Length == 1)
                                    {
                                        PO_Line = "00" + PO_Line;
                                    }
                                    if (PO_Line.Length == 2)
                                    {
                                        PO_Line = "0" + PO_Line;
                                    }
                                }

                                PO_LnKey = tabe2.Rows[0]["POLineKey"].ToString();
                                Line_Sta = "4";
                                Line_Type = "S";


                                //PromDate = DateTime.Now.ToString("MMddyy");


                                //PO_Number, PO_Line, Line_QTY, Line_Status, Item_Num, StartDate, EndDate, Unit_price, PromDate, Line_Type
                                //0        , 1       , 2      , 3          , 4       , 5        , 6      , 7         , 8       , 9
                                fields = PONumber + "," + PO_Line + "," + LineQTY + "," + Line_Sta + "," + ItemNum + "," + StartDate + "," + EndDate + "," +
                                    ItemUnitCost + "," + PromDate + "," + Line_Type;

                                //FSTI Proccess
                                Errormsg = process(fields, "POMT11");
                                /////////////////

                                //Line key///////////////////////////////
                                PO_LnKey = "";
                                if (Errormsg == "")
                                {
                                    while (PO_LnKey == "")
                                    {
                                        //PO_LnKey = "";
                                        string neededdate = dataGridView1.Rows[i].Cells[5].Value.ToString();
                                        neededdate = Convert.ToDateTime(neededdate).ToString("MM/dd/yy");
                                        string command =
                                            "SELECT FS_POLine.POLineKey, FS_POHeader.PONumber, FS_POLine.POLineNumber, FS_POLine.RequiredDate, FS_POLine.POLineStatus " +
                                            ", FS_POLine.POLineSubType, CONVERT(varchar, FS_POLine.RequiredDate, 101) AS Required, FS_POLine.POLineType " +
                                            "FROM FS_POLine WITH (NOLOCK) INNER JOIN FS_POHeader WITH (NOLOCK) ON FS_POLine.POHeaderKey = FS_POHeader.POHeaderKey " +
                                            "WHERE     (FS_POHeader.PONumber = '" + PONumber + "')  " +
                                            "AND (FS_POLine.POLineNumber = '" + PO_Line + "')  " +
                                            "AND (FS_POLine.RequiredDate = '" + neededdate + "') " +
                                            "AND (FS_POLine.POLineStatus = '4')  " +
                                            "AND (FS_POLine.POLineSubType = 'S')";
                                        PO_LnKey = DBMNG.Execute_Scalar(command);
                                    }
                                }
                                else
                                {
                                    PONumber = "";
                                    PO_Line = "";
                                    PO_LnKey = "";
                                }

                                //line key//////////////////////////////////


                                FileRow = dataGridView1.Rows[i].Cells[0].Value.ToString();

                                for (int c = 1; c < dataGridView1.ColumnCount; c++)
                                {
                                    switch (c)
                                    {
                                        case 3:
                                            {
                                                FileRow += ",\"" + PO_Line + "\"";
                                                break;
                                            }
                                        case 4:
                                            {
                                                FileRow += ",\"" + PONumber + "\"";
                                                break;
                                            }
                                        case 11:
                                            {
                                                FileRow += ",\"" + PO_LnKey + "\"";
                                                break;
                                            }
                                        case 12:
                                            {
                                                FileRow += ",\"" + Errormsg + "\"";
                                                break;
                                            }
                                        case 10:
                                            {
                                                FileRow += ",\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"";
                                                break;
                                            }
                                        default:
                                            {
                                                FileRow += ",\"" + dataGridView1.Rows[i].Cells[c].Value.ToString() + "\"";
                                                break;
                                            }
                                    }
                                }

                                //FileRow = dataGridView1.Rows[i].Cells[0].Value.ToString() + "," + dataGridView1.Rows[i].Cells[1].Value.ToString() + "," + dataGridView1.Rows[i].Cells[2].Value.ToString()
                                //    + "," + dataGridView1.Rows[i].Cells[3].Value.ToString() + "," + PONumber + "," + PO_Line + "," + dataGridView1.Rows[i].Cells[6].Value.ToString() + "," +
                                //    dataGridView1.Rows[i].Cells[7].Value.ToString() + "," + dataGridView1.Rows[i].Cells[8].Value.ToString() + "," + dataGridView1.Rows[i].Cells[9].Value.ToString() + "," +
                                //    dataGridView1.Rows[i].Cells[10].Value.ToString() + "," + Errormsg + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," + dataGridView1.Rows[i].Cells[12].Value.ToString() + "," +
                                //    PO_LnKey;

                                #endregion

                                JadePOREQ_File.Add(FileRow);
                                #endregion
                            }
                            else
                            {
                                #region Bad File
                                ProcessingLog(ActiveFileName + " >> There is no Data for combination Vendor: " + Vendor_ID + " and Item: " + ItemNum, "warning");
                                Errormsg = "There is an error with the Vendor information. There is no Data for combination Vendor: " + Vendor_ID + " and Item: " + ItemNum;
                                FileRow = dataGridView1.Rows[i].Cells[0].Value.ToString();

                                for (int c = 1; c < dataGridView1.ColumnCount; c++)
                                {
                                    switch (c)
                                    {
                                        case 3:
                                            {
                                                FileRow += ",\"" + PO_Line + "\"";
                                                break;
                                            }
                                        case 4:
                                            {
                                                FileRow += ",\"" + PONumber + "\"";
                                                break;
                                            }
                                        case 11:
                                            {
                                                FileRow += ",\"" + PO_LnKey + "\"";
                                                break;
                                            }
                                        case 12:
                                            {
                                                FileRow += ",\"" + Errormsg + "\"";
                                                break;
                                            }
                                        case 10:
                                            {
                                                FileRow += ",\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"";
                                                break;
                                            }
                                        default:
                                            {
                                                FileRow += ",\"" + dataGridView1.Rows[i].Cells[c].Value.ToString() + "\"";
                                                break;
                                            }
                                    }
                                }
                                #endregion
                                JadePOREQ_File.Add(FileRow);
                                BadFile_Flag = true;
                                //FSTI_Error_Flag = true;
                            }
                        }
                    }
                    else
                    {
                        i = dataGridView1.Rows.Count;
                    }
                }
                if (!error)
                {
                    //MessageBox.Show("Operation Ended Succesfully");
                }
            }
            catch (Exception ex)
            {
                FSTI_Error_Flag = true; 
                //MessageBox.Show(ex.Message);
            }


        }
        private void PORV01()
        {
            string respond = "";
            string fields = "";
            string LineTypeQ = "";

            //DataTable table = null;
            //PO_Number, Ln#, Receiving_Type, Quantity_Received, Stk, Bin, Item, Promised_Date, Line Type, Carrier, Traking
            //0        , 1  , 2             , 3                , 4  , 5  , 6   , 7            , 8        , 9      , 10

            string PONumber = "", LineNum = "", Receiving_Type = "", Quantity_Received = "", Stk = "", Bin = "", Item = "", Promised_Date = ""
                , LineType = "", CarrierName = "", Traking = "", ASN = "", Remark = "";


            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (!error)
                {
                    if (dataGridView1.Rows.Count > 0)
                    {
                        PONumber = dataGridView1.Rows[i].Cells[4].Value.ToString();
                        LineNum = dataGridView1.Rows[i].Cells[3].Value.ToString();

                        while (LineNum.Length < 3)
                        {
                            LineNum = "0" + LineNum;

                        }

                        //////Query para sacar el LIne type

                        LineTypeQ = "SELECT FS_POLine.POLineType FROM FS_POHeader INNER JOIN" +
                            " FS_POLine ON FS_POHeader.POHeaderKey = FS_POLine.POHeaderKey" +
                            " WHERE (FS_POHeader.PONumber = '" + PONumber + "') AND (FS_POLine.POLineNumberString = '" + LineNum + "')";

                        LineType = DBMNG.Execute_Scalar(LineTypeQ);

                        Receiving_Type = "R";
                        Quantity_Received = dataGridView1.Rows[i].Cells[13].Value.ToString();
                        switch (dataGridView1.Rows[i].Cells[15].Value.ToString())
                        {
                            case "EPDC":
                                {
                                    Stk = "3";
                                    Bin = "RECV";
                                    break;
                                }
                            case "JZWH":
                                {
                                    Stk = "JZ";
                                    Bin = "RECV";
                                    break;
                                }
                        }

                        Item = dataGridView1.Rows[i].Cells[7].Value.ToString();
                        Promised_Date = DateTime.Parse(dataGridView1.Rows[i].Cells[5].Value.ToString()).ToString("MMddyy");

                        CarrierName = dataGridView1.Rows[i].Cells[16].Value.ToString();
                        Traking = dataGridView1.Rows[i].Cells[17].Value.ToString();
                        ASN = dataGridView1.Rows[i].Cells[2].Value.ToString();

                        //Remark = Traking + " ASN:" + ASN;

                        //+","+
                        //PO_Number, Line_QTY, Line_Status, Line_Type, Item_Num, Prom_Date, Blanket, UM, Unit_price
                        fields = PONumber + "," + LineNum + "," + Receiving_Type + "," + Quantity_Received + "," + Stk + "," + Bin + "," + Item + "," + Promised_Date + ","
                            + LineType + "," + CarrierName + "," + Remark+","+ASN;
                        respond = process(fields, "PORV");

                        respond = respond.Replace(',', ';');

                        FileRow = "";
                        FileRow = dataGridView1.Rows[i].Cells[0].Value.ToString();
                        for (int j = 1; j < dataGridView1.ColumnCount; j++)
                        {
                            switch (j)
                            {
                                case 12:
                                    {
                                        FileRow += ",\"" + respond + "\""; //Error
                                        break;
                                    }
                                case 10:
                                    {
                                        FileRow += ",\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\""; //Capa Hand Shake
                                        break;
                                    }
                                default:
                                    {
                                        FileRow += ",\"" + dataGridView1.Rows[i].Cells[j].Value.ToString() + "\"";
                                        break;
                                    }
                            }

                        }

                        JadeRECV_File.Add(FileRow);

                        //MessageBox.Show(respond);
                    }
                }
                else
                {
                    i = dataGridView1.Rows.Count;
                }
            }
            if (!error)
            {
                //MessageBox.Show("Operation Ended Succesfully");
            }
        }
        private void POMT16()
        {
            //PO_Number, PO_Line, Item_Num, Promissed Date
            //0        , 1      , 2       , 3      

            string respond = "";
            string fields = "";
            string PONumber = "", LineNum = "", Item = "", Promised_Date = "";


            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (!error)
                {
                    if (dataGridView1.Rows.Count > 0)
                    {
                        PONumber = dataGridView1.Rows[i].Cells[4].Value.ToString();
                        LineNum = dataGridView1.Rows[i].Cells[5].Value.ToString();

                        while (LineNum.Length < 3)
                        {
                            LineNum = "0" + LineNum;

                        }
                        Item = dataGridView1.Rows[i].Cells[8].Value.ToString();
                        Promised_Date = dataGridView1.Rows[i].Cells[6].Value.ToString();
                        Promised_Date = Convert.ToDateTime(Promised_Date).ToString("MMddyy");

                        //+","+
                        //PO_Number, LineNum, Item_Num, Prom_Date
                        fields = PONumber.Replace("'", "") + "," + LineNum.Replace("'", "") + "," + Item.Replace("'", "") + "," + Promised_Date.Replace("'", "");
                        respond = process(fields, "POMT16");

                        respond = respond.Replace(',', ';');

                        FileRow = "";
                        FileRow = dataGridView1.Rows[i].Cells[0].Value.ToString();
                        for (int j = 1; j < dataGridView1.ColumnCount; j++)
                        {
                            if (j == 11)
                            {
                                FileRow += "," + respond; //Error
                            }
                            else
                            {
                                if (j == 13)
                                {
                                    FileRow += "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //Capa Hand Shake
                                }
                                else
                                {
                                    FileRow += "," + dataGridView1.Rows[i].Cells[j].Value.ToString();
                                }
                            }
                        }
                        FileRow = "";
                        FileRow = dataGridView1.Rows[i].Cells[0].Value.ToString() + "," + dataGridView1.Rows[i].Cells[1].Value.ToString() + "," + dataGridView1.Rows[i].Cells[2].Value.ToString()
                            + "," + dataGridView1.Rows[i].Cells[3].Value.ToString() + "," + PONumber + "," + LineNum + "," + dataGridView1.Rows[i].Cells[6].Value.ToString() + "," +
                            dataGridView1.Rows[i].Cells[7].Value.ToString() + "," + dataGridView1.Rows[i].Cells[8].Value.ToString() + "," + dataGridView1.Rows[i].Cells[9].Value.ToString() + "," +
                            dataGridView1.Rows[i].Cells[10].Value.ToString() + "," + respond + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," + dataGridView1.Rows[i].Cells[12].Value.ToString() + ",";


                        JadePOCancel_File.Add(FileRow);

                        //MessageBox.Show(respond);
                    }
                }
                else
                {
                    i = dataGridView1.Rows.Count;
                }
            }
            if (!error)
            {
                //MessageBox.Show("Operation Ended Succesfully");
            }


        }
        private void POMT12()
        {
            //PO_Number, PO_Line, Item_Num, Promissed Date,
            //0        , 1      , 2       , 3             ,

            string respond = "";
            string fields = "";
            string PONumber = "", LineNum = "", Item = "", Promised_Date = "";


            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (!error)
                {
                    if (dataGridView1.Rows.Count > 0)
                    {
                        PONumber = dataGridView1.Rows[i].Cells[4].Value.ToString();
                        LineNum = dataGridView1.Rows[i].Cells[3].Value.ToString();

                        while (LineNum.Length < 3)
                        {
                            LineNum = "0" + LineNum;

                        }
                        Item = dataGridView1.Rows[i].Cells[7].Value.ToString();
                        Promised_Date = dataGridView1.Rows[i].Cells[5].Value.ToString();
                        Promised_Date = Convert.ToDateTime(Promised_Date).ToString("MMddyy");

                        //+","+
                        //PO_Number, LineNum, Item_Num, Prom_Date
                        fields = PONumber.Replace("'", "") + "," + LineNum.Replace("'", "") + "," + Item.Replace("'", "") + "," + Promised_Date.Replace("'", "") + "," +
                            "S,5,,";
                        respond = process(fields, "POMT12");

                        respond = respond.Replace(',', ';');

                        FileRow = "";
                        FileRow = dataGridView1.Rows[i].Cells[0].Value.ToString();
                        for (int j = 1; j < dataGridView1.ColumnCount; j++)
                        {
                            switch (j)
                            {
                                case 10:
                                    {
                                        FileRow += ",\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\""; //Capa Hand Shake
                                        break;
                                    }
                                case 12:
                                    {
                                        FileRow += ",\"" + respond + "\""; //Error
                                        break;
                                    }
                                default:
                                    {
                                        FileRow += ",\"" + dataGridView1.Rows[i].Cells[j].Value.ToString() + "\"";
                                        break;
                                    }
                            }
                        }
                        JadePOCancel_File.Add(FileRow);

                        //MessageBox.Show(respond);
                    }
                }
                else
                {
                    i = dataGridView1.Rows.Count;
                }
            }
            if (!error)
            {
                //MessageBox.Show("Operation Ended Succesfully");
            }


        }
        private void POMT12PL()
        {


            //PO_Number, PO_Line, Item_Num, Original Promissed Date, single delivery line, Line Satus, Original Promissed Date, QTY 
            //0        , 1      , 2       , 3                      , 4                   , 5         , 6                      , 7

            //PO_Number, PO_Line, Item_Num, Promissed Date, POQTY
            //0        , 1      , 2       , 3             , 4

            string respond = "";
            string fields = "";
            string PONumber = "", LineNum = "", Item = "", Promised_Date = "";
            string POQTY = "";

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (!error)
                {
                    if (dataGridView1.Rows.Count > 0)
                    {
                        PONumber = dataGridView1.Rows[i].Cells[4].Value.ToString();
                        LineNum = dataGridView1.Rows[i].Cells[3].Value.ToString();

                        while (LineNum.Length < 3)
                        {
                            LineNum = "0" + LineNum;

                        }
                        Item = dataGridView1.Rows[i].Cells[7].Value.ToString();
                        Promised_Date = dataGridView1.Rows[i].Cells[5].Value.ToString();
                        Promised_Date = Convert.ToDateTime(Promised_Date).ToString("MMddyy");
                        POQTY = dataGridView1.Rows[i].Cells[9].Value.ToString();

                        //+","+
                        //PO_Number, LineNum, Item_Num, Prom_Date
                        fields = PONumber.Replace("'", "") + "," + LineNum.Replace("'", "") + "," + Item.Replace("'", "") + "," + Promised_Date.Replace("'", "") + "," +
                            "S,,," + POQTY;
                        respond = process(fields, "POMT12");

                        respond = respond.Replace(',', ';');

                        FileRow = "";
                        FileRow = dataGridView1.Rows[i].Cells[0].Value.ToString();
                        for (int j = 1; j < dataGridView1.ColumnCount; j++)
                        {
                            switch (j)
                            {
                                case 10:
                                    {
                                        FileRow += ",\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\""; //Capa Hand Shake
                                        break;
                                    }
                                case 12:
                                    {
                                        FileRow += ",\"" + respond + "\""; //Error
                                        break;
                                    }
                                default:
                                    {
                                        FileRow += ",\"" + dataGridView1.Rows[i].Cells[j].Value.ToString() + "\"";
                                        break;
                                    }
                            }
                        }
                        JadePOUpQTY_File.Add(FileRow);
                        //JadePOCancel_File.Add(FileRow);

                        //MessageBox.Show(respond);
                    }
                }
                else
                {
                    i = dataGridView1.Rows.Count;
                }
            }
            if (!error)
            {
                //MessageBox.Show("Operation Ended Succesfully");
            }


        }
        private void POMT12UPD()
        {
            //PO_Number, PO_Line, Item_Num, Promissed Date,
            //0        , 1      , 2       , 3             ,

            string respond = "";
            string fields = "";
            string PONumber = "", LineNum = "", Item = "", Promised_Date_Old = "", Promised_Date_New = "";


            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (!error)
                {
                    if (dataGridView1.Rows.Count > 0)
                    {
                        PONumber = dataGridView1.Rows[i].Cells[4].Value.ToString();
                        LineNum = dataGridView1.Rows[i].Cells[3].Value.ToString();

                        while (LineNum.Length < 3)
                        {
                            LineNum = "0" + LineNum;

                        }
                        Item = dataGridView1.Rows[i].Cells[7].Value.ToString();
                        Promised_Date_New = dataGridView1.Rows[i].Cells[5].Value.ToString();
                        Promised_Date_New = Convert.ToDateTime(Promised_Date_New).ToString("MMddyy");


                        //PO LINE KEY

                        string query = "SELECT  convert(varchar, OriginalPromisedDate, 101) as OriginalPromDate FROM FS_POLine WHERE (FS_POLine.POLineKey=" +
                            dataGridView1.Rows[i].Cells["PO LINE KEY"].Value.ToString() + ")";

                        Promised_Date_Old = DBMNG.Execute_Scalar(query);

                        try
                        {
                            Promised_Date_Old = DateTime.ParseExact(Promised_Date_Old, "MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MMddyy");

                            //+","+
                            //PO_Number, LineNum, Item_Num, Prom_Date_New,S,4,Prom_Date_Old
                            fields = PONumber.Replace("'", "") + "," + LineNum.Replace("'", "") + "," + Item.Replace("'", "") + "," + Promised_Date_New.Replace("'", "") + "," +
                                "S,4," + Promised_Date_Old + ",";

                            if (Promised_Date_Old != "")
                            {
                                //POMT12UPD
                                respond = process(fields, "POMT12UPD");
                                //respond = process(fields, "POMT12");

                                respond = respond.Replace(',', ';');
                            }
                            else
                            {
                                FSTI_Error_Flag = true;
                                respond = "There is no PO Line for POLineKey: " + dataGridView1.Rows[i].Cells["PO LINE KEY"].Value.ToString() + " Fields: " + fields;
                            }
                        }
                        catch
                        {
                            respond = "Bad PO Line Key (The PO Line Key Not Found) POLineKey=" + dataGridView1.Rows[i].Cells["PO LINE KEY"].Value.ToString();
                            ProcessingLog(ActiveFileName + " Error During Procesing Transaction - " + respond, "error");
                        }
                        FileRow = "";
                        FileRow = dataGridView1.Rows[i].Cells[0].Value.ToString();
                        for (int j = 1; j < dataGridView1.ColumnCount; j++)
                        {
                            switch (j)
                            {
                                case 10:
                                    {
                                        FileRow += ",\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\""; //Capa Hand Shake
                                        break;
                                    }
                                case 12:
                                    {
                                        FileRow += ",\"" + respond + "\""; //Error
                                        break;
                                    }
                                default:
                                    {
                                        FileRow += ",\"" + dataGridView1.Rows[i].Cells[j].Value.ToString() + "\"";
                                        break;
                                    }
                            }
                        }
                        JadePOUpdate_File.Add(FileRow);

                        //MessageBox.Show(respond);
                    }
                }
                else
                {
                    i = dataGridView1.Rows.Count;
                }
            }
            if (!error)
            {
                //MessageBox.Show("Operation Ended Succesfully");
            }


        }
        private void POMT12PORV01()
        {
            //PO_Number, PO_Line, Item_Num, Promissed Date,
            //0        , 1      , 2       , 3             ,
            float QTY_Num=0;
            string respond = "";
            string fields = "";
            string PONumber = "", LineNum = "", Item = "", Promised_Date_Old = "", Promised_Date_New = "", QTY_String = "";


            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (!error)
                {
                    if (dataGridView1.Rows.Count > 0)
                    {
                        PONumber = dataGridView1.Rows[i].Cells[4].Value.ToString();
                        LineNum = dataGridView1.Rows[i].Cells[3].Value.ToString();

                        while (LineNum.Length < 3)
                        {
                            LineNum = "0" + LineNum;

                        }
                        Item = dataGridView1.Rows[i].Cells[7].Value.ToString();
                        Promised_Date_New = dataGridView1.Rows[i].Cells[5].Value.ToString();
                        Promised_Date_New = Convert.ToDateTime(Promised_Date_New).ToString("MMddyy");


                        //PO LINE KEY

                        string query = "SELECT  convert(varchar, OriginalPromisedDate, 101) as OriginalPromDate FROM FS_POLine WHERE (FS_POLine.POLineKey=" +
                            dataGridView1.Rows[i].Cells["PO LINE KEY"].Value.ToString() + ")";

                        //Promised_Date_Old = DBMNG.Execute_Scalar(query);
                        //Promised_Date_Old = Convert.ToDateTime(Promised_Date_Old).ToString("MMddyy");

                        Promised_Date_Old = Convert.ToDateTime(dataGridView1.Rows[i].Cells["DUE DATE"].Value.ToString()).ToString("MMddyy");

                        query = "SELECT LineItemOrderedQuantity FROM  FS_POLine WHERE (FS_POLine.POLineKey = " +
                            dataGridView1.Rows[i].Cells["PO LINE KEY"].Value.ToString() + ")";
                        QTY_String=DBMNG.Execute_Scalar(query);

                        QTY_Num = float.Parse(QTY_String) + float.Parse(dataGridView1.Rows[i].Cells["RECV QTY"].Value.ToString());

                        //+","+
                        //PO_Number, PO_Line, Item_Num, Promissed Date, single delivery line, Line Satus, Original Promissed Date, QTY 
                        fields = PONumber.Replace("'", "") + "," 
                            + LineNum.Replace("'", "") + "," 
                            + Item.Replace("'", "") + "," 
                            + Promised_Date_Old + "," +
                            "S,4," 
                            + Promised_Date_Old + ","
                            + QTY_Num.ToString();

                        if (Promised_Date_Old != "")
                        {
                            respond = process(fields, "POMT12UPQ");

                            respond = respond.Replace(',', ';');
                            if (respond != "")
                            {
                                respond = "POMT - " + respond;
                            }
                        }
                        else
                        {
                            FSTI_Error_Flag = true;
                            respond = "There is no PO Line for POLineKey: " + dataGridView1.Rows[i].Cells["PO LINE KEY"].Value.ToString() + " Fields: " + fields;
                        }

                        #region PORV
                        if (respond == "")
                        {
                            PORV01();
                            FileRow = "";
                            FileRow = JadeRECV_File[0];
                            JadeRECV_File.Clear();
                        }
                        else
                        {
                            FileRow = "";
                            FileRow = dataGridView1.Rows[i].Cells[0].Value.ToString();
                            for (int j = 1; j < dataGridView1.ColumnCount; j++)
                            {
                                switch (j)
                                {
                                    case 10:
                                        {
                                            FileRow += ",\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\""; //Capa Hand Shake
                                            break;
                                        }
                                    case 12:
                                        {
                                            FileRow += ",\"" + "PORV - " + respond + "\""; //Error
                                            break;
                                        }
                                    default:
                                        {
                                            FileRow += ",\"" + dataGridView1.Rows[i].Cells[j].Value.ToString() + "\"";
                                            break;
                                        }
                                }
                            }
                        }
                        #endregion
                        JadePOUpQTY_File.Add(FileRow);

                        //MessageBox.Show(respond);
                    }
                }
                else
                {
                    i = dataGridView1.Rows.Count;
                }
            }
            if (!error)
            {
                //MessageBox.Show("Operation Ended Succesfully");
            }
        }
        private void FileProcesing()
        {
            string header = "", option = "";
            BadFile_Flag = false;
            JadePOREQ_File.Clear();
            JadeRECV_File.Clear();
            JadePOCancel_File.Clear();
            JadePOUpdate_File.Clear();
            JadePOUpQTY_File.Clear();
            option = dataGridView1.Rows[0].Cells[0].Value.ToString();

            JadeRECV_File.Clear();
            JadePOREQ_File.Clear();
            string CA2File = "";
            //Todas las Funciones 
            switch (option)
            {
                #region RECV
                case "RECV":
                    {
                        PORV01();
                        CA2File = "";

                        if (!FSTI_Error_Flag)
                        {
                                CA2File = "CA2" + DateTime.Now.ToString("MMddyyhhmmss") + ".csv";
                                ProcessingLog(ActiveFileName + " >> " + CA2File, "info");
                                FileStream fileStream = new FileStream(path + "\\Outbox\\" + CA2File, FileMode.Create, FileAccess.Write);
                                StreamWriter writer = new StreamWriter(fileStream);

                                header = "";
                                //header = "type,orders_items.order_item_index,orders.order_index,jade po ln num,FS po num,FS po ln,Due Date,vendors_assoc.vendor_code," +
                                //    "items_assoc.client_item_number,items_assoc.client_item_revision,orders_items.order_qty,Error,time_stamp,CAPA HS,PO Line Key,warehouse";

                                header = "TYPE,JADE ORDER ITEM INDEX,JADE ORDER INDEX,ITEM LINE NUM,FS PO NUMBER,DUE DATE,VENDOR CODE,ITEM NUMBER,ITEM REV,ORDER QTY," +
                                    "TIMESTAMP,PO LINE KEY,ERROR,RECV QTY,LOT NUMBER,WAREHOUSE,SCAC,TRACK";

                                writer.WriteLine(header);
                                for (int i = 0; i < JadeRECV_File.Count; i++)
                                {
                                    writer.WriteLine(JadeRECV_File[i]);
                                }
                                writer.Close();
                        }
                        break;
                    }
                #endregion
                #region POREQ
                case "POREQ":
                    {
                        POMT11();
                        CA2File = "";

                        if (!FSTI_Error_Flag)
                        {
                                CA2File = "CA2" + DateTime.Now.ToString("MMddyyhhmmss") + ".csv";
                                ProcessingLog(ActiveFileName + " >> " + CA2File, "info");
                                FileStream fileStream = new FileStream(path + "\\Outbox\\" + CA2File, FileMode.Create, FileAccess.Write);
                                StreamWriter writer = new StreamWriter(fileStream);

                                header = "";

                                //header = "type,orders_items.order_item_index,orders.order_index,jade po ln num,FS po num,FS po ln,Due Date,vendors_assoc.vendor_code," +
                                //    "items_assoc.client_item_number,items_assoc.client_item_revision,orders_items.order_qty,Error,CAPA HS,time_stamp,PO Line Key";

                                header = "TYPE,JADE ORDER ITEM INDEX,JADE ORDER INDEX,ITEM LINE NUM,FS PO NUMBER,DUE DATE,VENDOR CODE,ITEM NUMBER,ITEM REV," +
                                    "ORDER QTY,TIMESTAMP,PO LINE KEY,ERROR,RECV QTY,LOT NUMBER,WAREHOUSE,SCAC,TRACK";

                                writer.WriteLine(header);
                                for (int i = 0; i < JadePOREQ_File.Count; i++)
                                {
                                    writer.WriteLine(JadePOREQ_File[i]);
                                }

                                writer.Close();                           

                        }
                        

                        break;
                    }
                #endregion
                #region CANCEL
                case "CANCEL":
                    {
                        POMT12();
                        CA2File = "";
                        if (!FSTI_Error_Flag)
                        {
                                CA2File = "CA2" + DateTime.Now.ToString("MMddyyhhmmss") + ".csv";
                                ProcessingLog(ActiveFileName + " >> " + CA2File, "info");
                                FileStream fileStream = new FileStream(path + "\\Outbox\\" + CA2File, FileMode.Create, FileAccess.Write);
                                StreamWriter writer = new StreamWriter(fileStream);

                                header = "";

                                header = "TYPE,JADE ORDER ITEM INDEX,JADE ORDER INDEX,ITEM LINE NUM,FS PO NUMBER,DUE DATE,VENDOR CODE,ITEM NUMBER,ITEM REV," +
                                    "ORDER QTY,TIMESTAMP,PO LINE KEY,ERROR,RECV QTY,LOT NUMBER,WAREHOUSE,SCAC,TRACK";
                                writer.WriteLine(header);

                                //JadePOCancel_File
                                for (int i = 0; i < JadePOCancel_File.Count; i++)
                                {
                                    writer.WriteLine(JadePOCancel_File[i]);
                                }
                                writer.Close();
                        }
                        break;
                    }
                #endregion
                #region POUPD
                case "POUPD":
                    {
                        POMT12UPD();
                        CA2File = "";
                        if (!FSTI_Error_Flag)
                        {
                            CA2File = "CA2" + DateTime.Now.ToString("MMddyyhhmmss") + ".csv";
                            ProcessingLog(ActiveFileName + " >> " + CA2File, "info");
                            FileStream fileStream = new FileStream(path + "\\Outbox\\" + CA2File, FileMode.Create, FileAccess.Write);
                            StreamWriter writer = new StreamWriter(fileStream);

                            header = "";

                            header = "TYPE,JADE ORDER ITEM INDEX,JADE ORDER INDEX,ITEM LINE NUM,FS PO NUMBER,DUE DATE,VENDOR CODE,ITEM NUMBER,ITEM REV," +
                                "ORDER QTY,TIMESTAMP,PO LINE KEY,ERROR,RECV QTY,LOT NUMBER,WAREHOUSE,SCAC,TRACK";
                            writer.WriteLine(header);

                            //JadePOCancel_File
                            for (int i = 0; i < JadePOUpdate_File.Count; i++)
                            {
                                writer.WriteLine(JadePOUpdate_File[i]);
                            }
                            writer.Close();
                        }
                        break;
                    }
                #endregion
                #region POUPQ
                case "POUPQ":
                    {
                        POMT12PORV01();
                        CA2File = "CA2" + DateTime.Now.ToString("MMddyyhhmmss") + ".csv";
                        ProcessingLog(ActiveFileName + " >> " + CA2File, "info");
                        FileStream fileStream = new FileStream(path + "\\Outbox\\" + CA2File, FileMode.Create, FileAccess.Write);
                        StreamWriter writer = new StreamWriter(fileStream);

                        header = "";

                        header = "TYPE,JADE ORDER ITEM INDEX,JADE ORDER INDEX,ITEM LINE NUM,FS PO NUMBER,DUE DATE,VENDOR CODE,ITEM NUMBER,ITEM REV," +
                            "ORDER QTY,TIMESTAMP,PO LINE KEY,ERROR,RECV QTY,LOT NUMBER,WAREHOUSE,SCAC,TRACK";

                        header = "TYPE,JADE ORDER ITEM INDEX,JADE ORDER INDEX,ITEM LINE NUM,FS PO NUMBER,DUE DATE,VENDOR CODE,ITEM NUMBER,ITEM REV,"+
                            "ORDER QTY,TIMESTAMP,PO LINE KEY,ERROR,RECV QTY,LOT NUMBER,WAREHOUSE,SCAC,TRACK";

                        writer.WriteLine(header);

                        //JadePOCancel_File
                        for (int i = 0; i < JadePOUpQTY_File.Count; i++)
                        {
                            writer.WriteLine(JadePOUpQTY_File[i]);
                        }
                        writer.Close();
                        break;
                    }
                #endregion
                #region POUPL
                case "POUPL":
                    {
                        POMT12PL();
                        CA2File = "CA2" + DateTime.Now.ToString("MMddyyhhmmss") + ".csv";
                        ProcessingLog(ActiveFileName + " >> " + CA2File, "info");
                        FileStream fileStream = new FileStream(path + "\\Outbox\\" + CA2File, FileMode.Create, FileAccess.Write);
                        StreamWriter writer = new StreamWriter(fileStream);

                        header = "";

                        header = "TYPE,JADE ORDER ITEM INDEX,JADE ORDER INDEX,ITEM LINE NUM,FS PO NUMBER,DUE DATE,VENDOR CODE,ITEM NUMBER,ITEM REV," +
                            "ORDER QTY,TIMESTAMP,PO LINE KEY,ERROR,RECV QTY,LOT NUMBER,WAREHOUSE,SCAC,TRACK";

                        header = "TYPE,JADE ORDER ITEM INDEX,JADE ORDER INDEX,ITEM LINE NUM,FS PO NUMBER,DUE DATE,VENDOR CODE,ITEM NUMBER,ITEM REV," +
                            "ORDER QTY,TIMESTAMP,PO LINE KEY,ERROR,RECV QTY,LOT NUMBER,WAREHOUSE,SCAC,TRACK";

                        writer.WriteLine(header);

                        //JadePOCancel_File
                        for (int i = 0; i < JadePOUpQTY_File.Count; i++)
                        {
                            writer.WriteLine(JadePOUpQTY_File[i]);
                        }
                        writer.Close();
                        break;
                    }
                #endregion
            }
            FSTI.AmalgammaFSTI_Stop();

        }
        private void FileMove(string file, string toPath, string FileMoveName)
        {
            string path = toPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            try
            {
                System.IO.File.Move(file, toPath + "\\" + FileMoveName);
            }
            catch
            {
                System.IO.File.Move(file, toPath + "\\_" + DateTime.Now.ToString("hhmmss") + "_" + FileMoveName);
            }
        }
        #region Procesing FTP Funcions
        private void CheckWorkingFolders()
        {
            #region FTP Working Folders
            #region InBox
            string[] listInbox = MyFTP.directoryListSimple("inbox");
            FTPinbox.Items.Clear();
            for (int i = 0; i < listInbox.Count(); i++)
            {
                if (listInbox[i].Contains(".csv"))
                {
                    FTPinbox.Items.Add(listInbox[i]);
                }
            }
            //FTPinbox.Items.AddRange(listInbox);
            #endregion
            #region OutBox
            string[] listOutbox = MyFTP.directoryListSimple("outbox");
            FTPoutbox.Items.Clear();
            for (int i = 0; i < listOutbox.Count(); i++)
            {
                if (listOutbox[i].Contains(".csv"))
                {
                    FTPoutbox.Items.Add(listOutbox[i]);
                }
            }
            //FTPoutbox.Items.AddRange(listOutbox);
            #endregion
            #endregion

            #region Local Working Folders
            #region Inbox
            string[] ListWorkingInbox = Directory.GetFiles(path + "\\Inbox\\", "*.csv");
            for (int i = 0; i < ListWorkingInbox.Count(); i++)
            {
                ListWorkingInbox[i] = ListWorkingInbox[i].Replace(path + "\\Inbox\\", "");
            }
            WorkingFolderInbox.Items.Clear();
            WorkingFolderInbox.Items.AddRange(ListWorkingInbox);
            #endregion
            #region Outbox
            string[] ListWorkingOutbox = Directory.GetFiles(path + "\\OutBox\\", "*.csv");
            for (int i = 0; i < ListWorkingOutbox.Count(); i++)
            {
                ListWorkingOutbox[i] = ListWorkingOutbox[i].Replace(path + "\\OutBox\\", "");
            }
            WorkingFolderOutbox.Items.Clear();
            WorkingFolderOutbox.Items.AddRange(ListWorkingOutbox);
            #endregion
            #endregion
        }
        private void DownloadFTPOutBox()
        {
            //string path = Environment.CurrentDirectory.ToString()+"\\working\\";
            string LOG = "";
            for (int i = 0; i < FTPoutbox.Items.Count; i++)
            {
                LOG = "";
                if (FTPoutbox.Items[i].ToString().Contains(".csv"))
                {
                    LOG = FTPoutbox.Items[i].ToString() + " >> " + MyFTP.download("outbox\\" + FTPoutbox.Items[i].ToString(), path + "\\inbox\\" + FTPoutbox.Items[i].ToString());
                    ProcessingLog(LOG, "info");
                    LogListBox.Items.Add(LOG);

                    LOG = "";
                    LOG = FTPoutbox.Items[i].ToString() + " >> " + MyFTP.delete("outbox\\" + FTPoutbox.Items[i].ToString());
                    LogListBox.Items.Add(LOG);
                    ProcessingLog(LOG, "info");
                }
            }
        }
        private void UploadFTPOutBox()
        {
            string ftpfolder = "inbox\\";
            string LOG = "";
            for (int i = 0; i < WorkingFolderOutbox.Items.Count; i++)
            {
                if (WorkingFolderOutbox.Items[i].ToString().Contains(".csv"))
                {
                    LOG = WorkingFolderOutbox.Items[i].ToString() + " >> " + MyFTP.upload(ftpfolder + WorkingFolderOutbox.Items[i].ToString(), path + "OutBox\\" + WorkingFolderOutbox.Items[i].ToString());
                    LogListBox.Items.Add(LOG);
                    ProcessingLog(LOG, "info");
                    FileMove(path + "OutBox\\" + WorkingFolderOutbox.Items[i].ToString(), path + "Archive\\" + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString() + "\\" + DateTime.Now.Day.ToString(), WorkingFolderOutbox.Items[i].ToString());
                    //System.IO.File.Move(path + "OutBox\\" + WorkingFolderOutbox.Items[i].ToString(), path + "Archive\\" + WorkingFolderOutbox.Items[i].ToString());
                }
            }
        }
        #endregion
        private void FullProcess()
        {
            dataGridView1.DataSource = null;
            //Step 1: Se verifican los Folders
            ActiveFileName = "";
            CheckWorkingFolders();

            //Step 2: se bajan los ahchivos de FTP\OutBox y se borran del mismo
            if (DownloadInbox.Checked)
            {
                DownloadFTPOutBox();
            }
            CheckWorkingFolders();

            //Step 3: Se cargan archivos para procecamiento (Procesamiento Individual)
            for (int i = 0; i < WorkingFolderInbox.Items.Count; i++)
            {
                //Step 3.1: Se establece ruta del archivo
                ActiveFileName = WorkingFolderInbox.Items[i].ToString();
                string filename = path + "InBox\\" + ActiveFileName;

                //Step 3.2: Se carga archivo y se convierte a Datatable para navegacion
                dataGridView1.DataSource = CSV2Datatable(filename);

                //Step 3.3: Se procesa el archivo y se crea archivo de respuesta en Working\OutBox
                if (dataGridView1.DataSource != null)
                {
                    FileProcesing();
                }
                //Step 3.4: Mueve el archivo activo de Working\Inbox --> Working\Archive\[Mes]\[Dia]
                if (!FSTI_Error_Flag && !BadFile_Flag)
                {
                    try
                    {
                        FileMove(path + "InBox\\" + ActiveFileName, path + "Archive\\" + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString() + "\\" + DateTime.Now.Day.ToString(), ActiveFileName);
                    }
                    catch (Exception ex)
                    {
                        ProcessingLog(ActiveFileName + " - " + ex.Message, "error");
                    }
                }
                if (BadFile_Flag)
                {
                    try
                    {
                        FileMove(path + "InBox\\" + ActiveFileName, path + "BadFiles\\" + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString() + "\\" + DateTime.Now.Day.ToString(), ActiveFileName);
                        ProcessingLog(ActiveFileName + " was moved to BadFiles Folder", "info");
                    }
                    catch (Exception ex)
                    {
                        ProcessingLog(ActiveFileName + " - " + ex.Message, "error");
                    }
                }
                //System.Threading.Thread.Sleep(1000);
                //Thread.Sleep(1000);
       
            }
            CheckWorkingFolders();

            //Step 4: Se suben archivos de respuesta a FTP\Inbox y se mueven de Working\OutBox a Working\Archive\[Mes]\[Dia]

            if (UploadOutbox.Checked)
            {
                UploadFTPOutBox();
            }
            CheckWorkingFolders();

            //Step 5: Se Limpia el Working\Inbox --> Working\Archive\[Mes]\[Dia]
            //for (int i = 0; i < WorkingFolderInbox.Items.Count; i++)
            //{
            //    if (WorkingFolderInbox.Items[i].ToString().Contains(".csv"))
            //    {
            //        try
            //        {
            //            FileMove(path + "InBox\\" + WorkingFolderInbox.Items[i].ToString(), path + "Archive\\" + DateTime.Now.Year.ToString() + "\\" + DateTime.Now.Month.ToString() + "\\" + DateTime.Now.Day.ToString(), WorkingFolderInbox.Items[i].ToString());
            //            //System.IO.File.Move(path + "InBox\\" + WorkingFolderInbox.Items[i].ToString(), path + "Archive\\" + WorkingFolderInbox.Items[i].ToString());
            //        }
            //        catch(Exception ex)
            //        {
            //            ProcessingLog(ActiveFileName + " - " + ex.Message, "error");
            //        }
            //    }
            //}

        }
        private void ProcessingLog(string Log, string type)
        {
            string DateStamp=DateTime.Now.ToString("MM/dd/yyyy HH:mm");
            LogListBox.Items.Add("");
            LogListBox.Items.Add(DateStamp + " - " + Log);

            switch (type)
            {
                case "error":
                    {
                        LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Error, Log);
                        break;
                    }
                case "info":
                    {
                        LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Info, Log);
                        break;
                    }
                case "warning":
                    {
                        LOGGER.WriteLogLine(TOOLS.Dataloger.Category.Warning, Log);
                        break;
                    }
            }

        }
        private void RefreshFTPSetup()
        {
            string NewFTPAddress = FTP_URL.Text;
            string NewFTPUserID = FTP_User.Text;
            string NewFTPPass = FTP_Pass.Text;
            if ((NewFTPAddress != FTPAddress) || (NewFTPUserID != FTPUserID) || (NewFTPPass != FTPPass))
            {
                FTPAddress = NewFTPAddress;
                FTPUserID = NewFTPUserID;
                FTPPass = NewFTPPass;
                MyFTP = new FTP(FTPAddress, FTPUserID, FTPPass);
                ProcessingLog("FTP Conf. was changed to: FTP: " + FTPAddress + " User: " + FTPUserID + " Pass: " + FTPPass,"info");
            }

        }
        #endregion

        #region Events
        private void OpenFile_Click(object sender, EventArgs e)
        {
            string FileName = OpenFileDialog();

            if (FileName != "")
            {
                dataGridView1.DataSource = CSV2Datatable(FileName);
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {
            FileProcesing();
        }
        private void DownloadFiles_Click(object sender, EventArgs e)
        {
            DownloadFTPOutBox();
            CheckWorkingFolders();
        }
        private void UploadFiles_Click(object sender, EventArgs e)
        {
            UploadFTPOutBox();
            CheckWorkingFolders();
        }
        private void ViewFTP_Click(object sender, EventArgs e)
        {
            CheckWorkingFolders();
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            RefreshFTPSetup();
            FullProcess();
        } 
        private void SQLServer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                DBMNG = null;
                DBMNG = new Data_Base_MNG.SQL(SQLServer.Text, "FSDBMR", "sa", "6rzq4d1");//el paso
                MessageBox.Show("The SQLSERVER Connection was change to: Server Name: " + SQLServer.Text);
            }
        }
        private void RefreshFTP_Click(object sender, EventArgs e)
        {
            RefreshFTPSetup();
        }
        private void AutomaticResponse_CheckedChanged(object sender, EventArgs e)
        {
            if (AutomaticResponse.Checked)
            {
                FileProcess.Enabled = true;
            }
            else
            {
                FileProcess.Enabled = false;
            }
        }

        #endregion

        private void DownloafFunc_Click(object sender, EventArgs e)
        {

            DownloadInbox.Checked = !DownloadInbox.Checked;
            //DoDownload = !DoDownload;

            //if (DoDownload)
            //{
            //    DownloafFunc.BackgroundImage = Image.FromFile("download.png");
            //}
            //else
            //{
            //    DownloafFunc.BackgroundImage = Image.FromFile("NoDownload.png");
            //}
        }

        private void UplloafFunc_Click(object sender, EventArgs e)
        {
            UploadOutbox.Checked = !UploadOutbox.Checked;
            //DoUpload = !DoUpload;

            //if (DoUpload)
            //{
            //    UplloafFunc.BackgroundImage = Image.FromFile("Upload.png");
            //}
            //else
            //{
            //    UplloafFunc.BackgroundImage = Image.FromFile("NoUpload.png");
            //}
        }

        #endregion

        private void FTPTimelapse_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                float Miliseg = 0;
                float horas = float.Parse(FTPTimelapse.Text);

                Miliseg = horas * (360) * (1000);

                DBFiles.Interval = Convert.ToInt32(Miliseg);

                MessageBox.Show("Interval was updated to " + Miliseg+" milisegs");
            }
        }

        private void Extraction_Click(object sender, EventArgs e)
        {
            DataTable table;
            string query = "SELECT _CAP_Jade_ItemMaster_Core.ItemNumber, _CAP_Jade_ItemMaster_Core.ItemDescription, FS_POHeader.PONumber, _CAP_Jade_ItemMaster_Core.DueDate, "+
               " _CAP_Jade_ItemMaster_Core.RequiredQuantity - FS_POLine.ReceiptQuantity AS DUE_DATE, FS_POLine.POLineKey "+
" FROM  _CAP_Jade_ItemMaster_Core INNER JOIN "+
               " FS_POHeader ON _CAP_Jade_ItemMaster_Core.POHeaderKey = FS_POHeader.POHeaderKey INNER JOIN "+
               " FS_POLine ON FS_POHeader.POHeaderKey = FS_POLine.POHeaderKey "+
" WHERE (FS_POLine.POLineStatus < '5') AND (FS_POLine.POLineSubType = 'B') "+
" ORDER BY _CAP_Jade_ItemMaster_Core.ItemKey, FS_POHeader.PONumber, _CAP_Jade_ItemMaster_Core.LineNumber";
            table = DBMNG.Execute_Query(query);
            Table2File(table, "extraction.csv");
        }

        private void DownloadInbox_CheckedChanged(object sender, EventArgs e)
        {
            //DoDownload = !DoDownload;
            //DownloadInbox.Checked = !DownloadInbox.Checked;
            if (DownloadInbox.Checked)
            {
                DownloafFunc.BackgroundImage = Image.FromFile("download.png");
            }
            else
            {
                DownloafFunc.BackgroundImage = Image.FromFile("NoDownload.png");
            }
        }

        private void UploadOutbox_CheckedChanged(object sender, EventArgs e)
        {

            if (UploadOutbox.Checked)
            {
                UplloafFunc.BackgroundImage = Image.FromFile("Upload.png");
            }
            else
            {
                UplloafFunc.BackgroundImage = Image.FromFile("NoUpload.png");
            }
        }

        private void ClearProcessLog_Click(object sender, EventArgs e)
        {
            LogListBox.Items.Clear();
        }

        private void ClearFTPLog_Click(object sender, EventArgs e)
        {
            FTPlistBox2.Items.Clear();
        }

        private void Manual_Process_Click(object sender, EventArgs e)
        {
            FullProcess();
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            LogListBox.Items.Clear();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            FTPlistBox2.Items.Clear();
        }
        
    }
}
