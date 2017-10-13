using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SoftBrands.FourthShift.Transaction;
using System.IO;

namespace FS4Amalgamma
{
    public class AmalgammaFSTI
    {
        #region Private Variables
        private FSTIClient _fstiClient = null;
        private string CFGFile = "";
        private string FSUser = "";
        private string FSPass = "";
        private ITransaction _Trans = null;
        private FSTIError itemError = null;

        #region Public Variables
        public string ErrorMsg = "";
        public string Trans_Error_Msg = "";
        public string CDFResponse = "";
        public List<string> DetailError=new List<string>();
        #endregion

        #region FSTI Transaccions
        #endregion

        #region FSTI Behaving flags
        private bool FSTI_Is_Initialized = false;
        private bool FSTI_Is_Loged = false;
        #endregion

        #endregion

        #region Private Functions
        private string FSTI_Log(string CFGResponse, string User, string Type, string Error)
        {
            Data_Base_MNG.SQL DBMNG = new Data_Base_MNG.SQL("capsp", "AmalgammaDB", "amalgamma", "capsonic");

            Error = Error.Replace("'", "");

            string query = "INSERT INTO Amal_FSTI_Log (Amal_User ,Amal_Trans_Type ,Amal_Trans_Response ,Time_Stamp ,Amal_CFGResponse) " +
                            "VALUES ('" + User + "','" + Type + "','" + Error + "',Getdate(),'" + CFGResponse + "')";

            DBMNG.Execute_Command(query);

            return DBMNG.Error_Mjs;
        }
        private void FSTI_Initialization()
        {
            ErrorMsg = "";
            try
            {
                _fstiClient = new FSTIClient();

                // call InitializeByConfigFile
                // second parameter == true is to participate in unified logon
                // third parameter == false, no support for impersonation is needed

                _fstiClient.InitializeByConfigFile(CFGFile, true, false);

                // Since this program is participating in unified logon, need to
                // check if a logon is required.

                if (_fstiClient.IsLogonRequired)
                {
                    // Logon is required, enable the logon button
                    //FSTI_Login.Enabled = true;
                    //FSTI_Login.Focus();
                    FSTI_Is_Initialized = true;
                    FSTI_Is_Loged = false;
                }
                else
                {
                    // Logon is not required (because of unified logon), enable the SubmitItem button

                    FSTI_Is_Initialized = true;
                    FSTI_Is_Loged = true;
                }
                // Disable the Initialize button


            }
            catch (FSTIApplicationException exception)
            {
                ErrorMsg = exception.Message;
                FSTI_Is_Initialized = false;
                FSTI_Is_Loged = false;
                //MessageBox.Show(exception.Message, "FSTIApplication Exception");
                _fstiClient.Terminate();
                _fstiClient = null;
            }
            //return FSTI_Is_Initialized;
        }
        private void FSTI_Login()
        {
            string message = null;     // used to hold a return message, from the logon

            int status;         // receives the return value from the logon call

            if (!FSTI_Is_Loged)
            {

                status = _fstiClient.Logon(FSUser, FSPass, ref message);
                if (status > 0)
                {
                    ErrorMsg = "Invalid user id or password";
                    FSTI_Is_Loged = false;
                }
                else
                {
                    FSTI_Is_Loged = true;
                }
            }

        }
        private void FSTI_STOP()
        {
            if (_fstiClient != null)
            {
                _fstiClient.Terminate();
                _fstiClient = null;
                FSTI_Is_Initialized = false;
                FSTI_Is_Loged = false;
            }
        }
        private void FSTI_Execute()
        {
            _fstiClient.ProcessId(_Trans, null);
        }
        private bool FSTI_ProcessTransaction(ITransaction Transaction, string TransactionID, string Ammalgamma_User)
        {
            CDFResponse = Transaction.GetString(TransactionStringFormat.fsCDF);

            if (_fstiClient.ProcessId(Transaction, null))
            {
                //
                ErrorMsg = "";
                Trans_Error_Msg = "";

                FSTI_Log(CDFResponse, Ammalgamma_User, TransactionID, "");

                return true;
            }
            else
            {
                // failure, retrieve the error object 
                // and then dump the information in the list box
                itemError = null;
                itemError = _fstiClient.TransactionError;
                Trans_Error_Msg = itemError.Description;
                // DumpErrorObject(myItem, itemError);

                FSTI_Log(CDFResponse, Ammalgamma_User, TransactionID, Trans_Error_Msg);
                return false;
            }
        }
        #endregion

        #region Constructors

        public AmalgammaFSTI(string CFG_File, string FS_User, string FS_Pass)
        {
            CFGFile = CFG_File;
            FSUser = FS_User;
            FSPass = FS_Pass;
        }

        #endregion

        #region Public Function
        public bool AmalgammaFSTI_Initialization()
        {
            FSTI_Initialization();
            return FSTI_Is_Initialized;
        }
        public void AmalgammaFSTI_Stop()
        {
            FSTI_STOP();
        }
        public bool AmalgammaFSTI_Logon()
        {
            FSTI_Login();
            return FSTI_Is_Loged;
        }

        public string[] DumpError(ITransaction transaction, FSTIError fstiErrorObject)
        {
            List<string> errorLog = new List<string>();
            errorLog.Add("Transaction Error:");
            errorLog.Add("");
            errorLog.Add(String.Format("Transaction: {0}", transaction.Name));
            errorLog.Add(String.Format("Description: {0}", fstiErrorObject.Description));
            errorLog.Add(String.Format("MessageFound: {0} ", fstiErrorObject.MessageFound));
            errorLog.Add(String.Format("MessageID: {0} ", fstiErrorObject.MessageID));
            errorLog.Add(String.Format("MessageSource: {0} ", fstiErrorObject.MessageSource));
            errorLog.Add(String.Format("Number: {0} ", fstiErrorObject.Number));
            errorLog.Add(String.Format("Fields in Error: {0} ", fstiErrorObject.NumberOfFieldsInError));
            for (int i = 0; i < fstiErrorObject.NumberOfFieldsInError; i++)
            {
                int field = fstiErrorObject.GetFieldNumber(i);
                errorLog.Add(String.Format("Field[{0}]: {1}", i, field));
                ITransactionField myField = transaction.get_Field(field);
                errorLog.Add(String.Format("Field name: {0}", myField.Name));
            }
            return errorLog.ToArray();
        }

        public bool AmalgammaFSTI_MORV00(string FSTI_fields, string Ammalgamma_User)
        {

            MORV00 myMORV00 = new MORV00();
            string[] Fields_Array = FSTI_fields.Split(',');

            //fields= MO_NO,LN_NO,RECV_QTY,ITEM,STK,BIN

            string MO_NO = Fields_Array[0];//
            string LN_NO = Fields_Array[1];//
            string RECV_TYPE = "R";//
            string RECV_QTY = Fields_Array[2];//
            string LN_TYPE = "M";//
            string ITEM = Fields_Array[3];//

            string MOVE_QTY = RECV_QTY;
            string STK = Fields_Array[4];
            string BIN = Fields_Array[5];
            string INV_CAT = "O";
            string INSP_CODE = "G";

            string NEW_LOT = "";
            string LOT_ASSIGN_POL = "";

            //MO Number
            myMORV00.MONumber.Value = MO_NO;
            //MO Line
            myMORV00.MOLineNumber.Value = LN_NO;
            //MO Line Type
            myMORV00.MOLineType.Value = LN_TYPE;
            //Reciving Type
            myMORV00.ReceivingType.Value = RECV_TYPE;
            //Reciving QTY
            myMORV00.ReceiptQuantity.Value = RECV_QTY;
            //Item Number
            myMORV00.ItemNumber.Value = ITEM;

            //Move QTY
            myMORV00.MoveQuantity1.Value = RECV_QTY;
            //Stock
            myMORV00.Stockroom1.Value = STK;
            //Bin
            myMORV00.Bin1.Value = BIN;
            //Inventory Category
            myMORV00.InventoryCategory1.Value = INV_CAT;
            //Inspection Code
            myMORV00.InspectionCode1.Value = INSP_CODE;

            //myMORV00.

            CDFResponse = myMORV00.GetString(TransactionStringFormat.fsCDF);

            if (_fstiClient.ProcessId(myMORV00, null))
            {
                //
                ErrorMsg = "";
                Trans_Error_Msg = "";

                FSTI_Log(CFGFile, Ammalgamma_User, "MORV00", "");

                return true;
            }
            else
            {
                // failure, retrieve the error object 
                // and then dump the information in the list box
                itemError = null;
                itemError = _fstiClient.TransactionError;
                Trans_Error_Msg = itemError.Description;
                // DumpErrorObject(myItem, itemError);

                FSTI_Log(CFGFile, Ammalgamma_User, "MORV00", Trans_Error_Msg);
                return false;
            }

        }
        public bool AmalgammaFSTI_ITMB03(string FSTI_fields, string Ammalgamma_User)
        {
            ITMB03 MyITMB03 = new ITMB03();

            //IntemNumber,Planer,Buyer
            string[] Fields_Array = FSTI_fields.Split(',');

            //Item Number
            string ItemNo = Fields_Array[0];
            //Item Planer
            string NewPlaner = Fields_Array[1];
            //Item Buyer
            string NewBuyer = Fields_Array[1];

            MyITMB03.ItemNumber.Value = ItemNo;
            MyITMB03.Planner.Value = NewPlaner;
            MyITMB03.Buyer.Value = NewBuyer;

            //myMORV00.

            CDFResponse = MyITMB03.GetString(TransactionStringFormat.fsCDF);

            if (_fstiClient.ProcessId(MyITMB03, null))
            {
                //
                ErrorMsg = "";
                Trans_Error_Msg = "";

                FSTI_Log(CFGFile, Ammalgamma_User, "ITMB03", "");

                return true;
            }
            else
            {
                // failure, retrieve the error object 
                // and then dump the information in the list box
                itemError = null;
                itemError = _fstiClient.TransactionError;
                Trans_Error_Msg = itemError.Description;
                // DumpErrorObject(myItem, itemError);

                FSTI_Log(CFGFile, Ammalgamma_User, "ITMB03", Trans_Error_Msg);
                return false;
            }

        }
        public bool AmalgammaFSTI_IMTR01(string FSTI_fields, string Ammalgamma_User)
        {
            //Transaction Object
            IMTR01 MyIMTR01 = new IMTR01();

            //IntemNumber,STK-BINFrom,InvCatFrom,STK-BINTo,InvCatTo,Qty
            string[] Fields_Array = FSTI_fields.Split(',');

            //Item Number
            MyIMTR01.ItemNumber.Value = Fields_Array[0];

            //from

            string[] LocFrom = Fields_Array[1].Split('-');
            string BinFrom = "";

            if (LocFrom.Count() > 2)
            {
                for (int i = 1; i < LocFrom.Count(); i++)
                {
                    if (i == 1)
                    {
                        BinFrom += LocFrom[i];
                    }
                    else
                    {
                        BinFrom += "-" + LocFrom[i];
                    }
                }
            }
            else
            {
                BinFrom = LocFrom[1];
            }

            MyIMTR01.StockroomFrom.Value = LocFrom[0];
            MyIMTR01.BinFrom.Value = BinFrom;

            MyIMTR01.InventoryCategoryFrom.Value = Fields_Array[2];

            //to

            //IntemNumber,STK-BINFrom,InvCatFrom,STK-BINTo,InvCatTo,Qty

            string[] LocTo = Fields_Array[3].Split('-');
            string BinTo = "";

            if (LocTo.Count() > 2)
            {
                for (int i = 1; i < LocTo.Count(); i++)
                {
                    if (i == 1)
                    {
                        BinTo += LocTo[i];
                    }
                    else
                    {
                        BinTo += "-" + LocTo[i];
                    }
                }
            }
            else
            {
                BinTo = LocTo[1];
            }


            MyIMTR01.StockroomTo.Value = LocTo[0];
            MyIMTR01.BinTo.Value = BinTo;

            MyIMTR01.InventoryCategoryTo.Value = Fields_Array[4];

            //QTY
            MyIMTR01.InventoryQuantity.Value = Fields_Array[5];
            MyIMTR01.LotIdentifier.Value = "N";


            CDFResponse = MyIMTR01.GetString(TransactionStringFormat.fsCDF);

            if (_fstiClient.ProcessId(MyIMTR01, null))
            {
                //
                ErrorMsg = "";
                Trans_Error_Msg = "";

                FSTI_Log(CFGFile, Ammalgamma_User, "ITMB03", "");

                return true;
            }
            else
            {
                // failure, retrieve the error object 
                // and then dump the information in the list box
                itemError = null;
                itemError = _fstiClient.TransactionError;
                Trans_Error_Msg = itemError.Description;
                // DumpErrorObject(myItem, itemError);

                FSTI_Log(CFGFile, Ammalgamma_User, "ITMB03", Trans_Error_Msg);
                return false;
            }
        }
        public bool AmalgammaFSTI_PORV01(string FSTI_fields, string Ammalgamma_User)
        {
            //Transaction Object
            PORV01 MyPORV01 = new PORV01();

            #region Campos
            //PO_Number, Ln#, Receiving_Type, Quantity_Received, Stk, Bin, Item, Promised_Date
            //0        , 1  , 2             , 3                , 4  , 5  , 6   , 7
            string[] Fields_Array = FSTI_fields.Split(',');

            //PO_Number
            MyPORV01.PONumber.Value = Fields_Array[0];

            //PO_Line#
            MyPORV01.POLineNumber.Value = Fields_Array[1];

            //Receiving_Type
            MyPORV01.POReceiptActionType.Value = Fields_Array[2];

            //QTY RECV
            MyPORV01.ReceiptQuantity.Value = Fields_Array[3];//TOTAL
            MyPORV01.ReceiptQuantityMove1.Value = Fields_Array[3];

            //STK
            MyPORV01.Stockroom1.Value = Fields_Array[4];//TOTAL
            
            //BIN
            MyPORV01.Bin1.Value = Fields_Array[5];

            //Item Number
            MyPORV01.ItemNumber.Value = Fields_Array[6];

            //Line Tipe
            MyPORV01.POLineType.Value = "P";

            //Lot assign
            MyPORV01.LotNumberAssignmentPolicy.Value = "";

            //Promised Date
            MyPORV01.PromisedDate.Value = Fields_Array[7];

            //Receipt Date
            MyPORV01.POReceiptDate.Value = DateTime.Now.ToString("MMddyy");
            #endregion

            #region Ejecucion
            CDFResponse = MyPORV01.GetString(TransactionStringFormat.fsCDF);

            if (_fstiClient.ProcessId(MyPORV01, null))
            {
                //
                ErrorMsg = "";
                Trans_Error_Msg = "";

                FSTI_Log(CFGFile, Ammalgamma_User, "PORV01", "");

                return true;
            }
            else
            {
                // failure, retrieve the error object 
                // and then dump the information in the list box
                itemError = null;
                itemError = _fstiClient.TransactionError;
                Trans_Error_Msg = itemError.Description;

                DumpErrorObject(MyPORV01, itemError);

                // DumpErrorObject(myItem, itemError);

                FSTI_Log(CFGFile, Ammalgamma_User, "PORV01", Trans_Error_Msg);
                return false;
            }
            #endregion

        }
        public bool AmalgammaFSTI_POMT00(string FSTI_fields, string Ammalgamma_User)
        {
            POMT00 MyPOMT00 = new POMT00();

            #region Campos
            //PO_Number, Vendor_ID, Terms
            //0        , 1        , 2    
            string[] Fields_Array = FSTI_fields.Split(',');

            //PO_Number
            MyPOMT00.PONumber.Value = Fields_Array[0];

            //Vendor_ID
            MyPOMT00.VendorID.Value = Fields_Array[1];

            //Terms
            MyPOMT00.TermsCode.Value = Fields_Array[2];



            #endregion

            #region Ejecucion
            CDFResponse = MyPOMT00.GetString(TransactionStringFormat.fsCDF);

            if (_fstiClient.ProcessId(MyPOMT00, null))
            {
                //
                ErrorMsg = "";
                Trans_Error_Msg = "";

                FSTI_Log(CFGFile, Ammalgamma_User, "POMT00", "");

                return true;
            }
            else
            {
                // failure, retrieve the error object 
                // and then dump the information in the list box
                itemError = null;
                itemError = _fstiClient.TransactionError;
                Trans_Error_Msg = itemError.Description;
                // DumpErrorObject(myItem, itemError);

                FSTI_Log(CFGFile, Ammalgamma_User, "POMT00", Trans_Error_Msg);
                return false;
            }
            #endregion

        }
        public bool AmalgammaFSTI_POMT10(string FSTI_fields, string Ammalgamma_User)
        {
            POMT10 MyPOMT10 = new POMT10();

            #region Campos
            //PO_Number, Line_QTY, Line_Status, Line_Type, Item_Num, Prom_Date, Blanket, UM, Unit_price
            //0        , 1       , 2          , 3        , 4       , 5        , 6      , 7 , 8
            string[] Fields_Array = FSTI_fields.Split(',');

            //PO_Number
            MyPOMT10.PONumber.Value = Fields_Array[0];

            //Line_QTY
            MyPOMT10.LineItemOrderedQuantity.Value = Fields_Array[1];

            //Line_Type
            MyPOMT10.POLineStatus.Value = Fields_Array[2];

            //Line_Type
            MyPOMT10.POLineType.Value = Fields_Array[3];

            //Item_Num
            MyPOMT10.ItemNumber.Value = Fields_Array[4];

            //Prom_date
            MyPOMT10.PromisedDate.Value = "102013";
            MyPOMT10.NeededDate.Value = "021414";
            //MyPOMT10.PromisedDate.Value = Fields_Array[5];
            //MyPOMT10.NeededDate.Value = Fields_Array[5];

            //Blanket
            MyPOMT10.IsBlanketOrNonBlanket.Value = Fields_Array[6];

            //PO_UM
            MyPOMT10.POLineUM.Value = Fields_Array[7];

            //Unit_price
            MyPOMT10.ItemUnitCost.Value = Fields_Array[8];


            #endregion


            #region Ejecucion
            CDFResponse = MyPOMT10.GetString(TransactionStringFormat.fsCDF);

            if (_fstiClient.ProcessId(MyPOMT10, null))
            {
                //
                ErrorMsg = "";
                Trans_Error_Msg = "";

                FSTI_Log(CFGFile, Ammalgamma_User, "POMT10", "");

                return true;
            }
            else
            {
                // failure, retrieve the error object 
                // and then dump the information in the list box
                itemError = null;
                itemError = _fstiClient.TransactionError;
                Trans_Error_Msg = itemError.Description;
                // DumpErrorObject(myItem, itemError);
                DetailError.Clear();
                DumpErrorObject(MyPOMT10, itemError);

                FSTI_Log(CFGFile, Ammalgamma_User, "POMT10", Trans_Error_Msg);
                return false;
            }
            #endregion

        }
        public bool AmalgammaFSTI_POMT11(string FSTI_fields, string Ammalgamma_User)
        {
            POMT11 MyPOMT11 = new POMT11();

            #region Campos
            //PO_Number, PO_Line, Line_QTY, Line_Status, Item_Num, StartDate, EndDate, Unit_price, PromDate, Line_Type
            //0        , 1       , 2      , 3          , 4       , 5        , 6      , 7         , 8       , 9

            string[] Fields_Array = FSTI_fields.Split(',');

            //PO_Number
            MyPOMT11.PONumber.Value = Fields_Array[0];

            //PO_Line
            MyPOMT11.POLineNumber.Value = Fields_Array[1];

            //Line_QTY
            MyPOMT11.LineItemOrderedQuantity.Value = Fields_Array[2];

            //Line_Status
            MyPOMT11.POLineStatus.Value = Fields_Array[3];

            //Item_Num
            MyPOMT11.ItemNumber.Value = Fields_Array[4];

            //Star Date
            MyPOMT11.NeededDate.Value = Fields_Array[5];

            //End Date
            MyPOMT11.PromisedDate.Value = Fields_Array[6];

            //Unit_price
            MyPOMT11.ItemUnitCost.Value = Fields_Array[7];

            //Prom_date
            MyPOMT11.PromisedDateOld.Value = Fields_Array[8];

            //Line_Type
            MyPOMT11.POLineSubType.Value = Fields_Array[9];


            #endregion


            #region Ejecucion
            CDFResponse = MyPOMT11.GetString(TransactionStringFormat.fsCDF);

            if (_fstiClient.ProcessId(MyPOMT11, null))
            {
                //
                ErrorMsg = "";
                Trans_Error_Msg = "";

                FSTI_Log(CFGFile, Ammalgamma_User, "POMT11", "");

                return true;
            }
            else
            {
                // failure, retrieve the error object 
                // and then dump the information in the list box
                itemError = null;
                itemError = _fstiClient.TransactionError;
                Trans_Error_Msg = itemError.Description;
                // DumpErrorObject(myItem, itemError);
                DetailError.Clear();
                DumpErrorObject(MyPOMT11, itemError);

                FSTI_Log(CFGFile, Ammalgamma_User, "POMT11", Trans_Error_Msg);
                return false;
            }
            #endregion

        }
        private void DumpErrorObject(ITransaction transaction, FSTIError fstiErrorObject)
        {
            DetailError.Add("Transaction Error:");
            DetailError.Add("");
            DetailError.Add("Transaction: "+ transaction.Name);
            DetailError.Add("Description: "+ fstiErrorObject.Description);
            DetailError.Add("MessageFound: "+ fstiErrorObject.MessageFound);
            DetailError.Add("MessageID: "+ fstiErrorObject.MessageID);
            DetailError.Add("MessageSource: "+ fstiErrorObject.MessageSource);
            DetailError.Add("Number: "+ fstiErrorObject.Number);
            DetailError.Add("Fields in Error: "+ fstiErrorObject.NumberOfFieldsInError);
            for (int i = 0; i < fstiErrorObject.NumberOfFieldsInError; i++)
            {
                int field = fstiErrorObject.GetFieldNumber(i);
                DetailError.Add("Field["+i.ToString()+"]: "+ field);
                ITransactionField myField = transaction.get_Field(field);
                DetailError.Add("Field name: "+ myField.Name);
            }
        }

        #endregion

      

    }

    //public class AmalgammaMORV00 : AmalgammaFSTI
    //{
        
    //    public AmalgammaMORV00()

    //    {
    //    }
    //}
}
