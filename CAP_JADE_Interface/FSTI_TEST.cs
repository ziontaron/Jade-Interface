using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CAP_JADE_Interface
{
    public partial class FSTI_TEST : Form
    {
        FS4Amalgamma.AmalgammaFSTI FSTI;
        string CFG_File = @"M:\Mfgsys\fs.cfg";
        string User = "IMPT";
        string Pass = "fstiapp";

        public FSTI_TEST()
        {
            InitializeComponent();
            FSTI = new FS4Amalgamma.AmalgammaFSTI(CFG_File, User, Pass);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string transaction = TransactionName.Text;
            try
            {
                if (FSTI.AmalgammaFSTI_Initialization())
                {
                    if (FSTI.AmalgammaFSTI_Logon())
                    {
                        switch (transaction)
                        {
                            // POMT11
                            case "POMT12":
                                {

                                    if (FSTI.AmalgammaFSTI_POMT10(Fields.Text, "JadeInterface"))
                                    {
                                    }
                                    else
                                    {
                                        string[] error = FSTI.DetailError.ToArray();
                                        LogListBox.Items.AddRange(error);
                                                                                
                                    }
                                    break;
                                }
                            //POMT16
                            //PORV00
                            //PORV01
                            //MORV00
                            //ITMB03
                            //IMTR01
                        }
                    }
                }
            }
            catch
            { }

        }

    }
}
