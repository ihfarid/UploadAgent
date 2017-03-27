using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.IO.Compression;

namespace Sandoz.UploadAgent
{
    public partial class frmMain2 : Form
    {
        bool _bTimer1 = true, _bTimer2 = true, _bTimer3 = true, _bTimer4 = true;
        string _sMaxSerial = "";
        string _sMaxSerial2 = "";
        string _sFileSerial = "";
        string _sFileSerial2 = "";
        int nFileCount = 0;
        int nFileCount2 = 0;
        
        public frmMain2()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            bool bFire = true;
            _bTimer1 = false;
            _bTimer2 = false;
            _bTimer3 = false;
            _bTimer4 = false;
            try
            {
                UpdateLog(DateTime.Now, DateTime.Now.Hour.ToString() + " - " + te1.Time.Hour.ToString()
                    + " - " + te2.Time.Hour.ToString() + " - " + te3.Time.Hour.ToString()
                    + " - " + te4.Time.Hour.ToString());
                if (DateTime.Now.Hour >= te1.Time.Hour && DateTime.Now.Hour<te2.Time.Hour)
                {
                    _bTimer1 = true;
                    UpdateLog(DateTime.Now, "Timer 1 Started");
                }
                if (DateTime.Now.Hour >= te2.Time.Hour && DateTime.Now.Hour < te3.Time.Hour)
                {
                    _bTimer2 = true;
                    UpdateLog(DateTime.Now, "Timer 2 Started");
                }
                if (DateTime.Now.Hour >= te3.Time.Hour && DateTime.Now.Hour < te4.Time.Hour)
                {
                    _bTimer3 = true;
                    UpdateLog(DateTime.Now, "Timer 3 Started");
                }
                if (DateTime.Now.Hour >= te4.Time.Hour)
                {
                    _bTimer4 = true;
                    UpdateLog(DateTime.Now, "Timer 4 Started");
                }
                if (bFire)
                {
                    FireProtocol();
                }
            }
            catch (Exception ex)
            {
                UpdateLog(DateTime.Now, "TimerTick Error");
                //MessageBox.Show("Exception: " + ex.Message);
            }
            timer1.Start();
        }

        public void FireProtocol()
        {
            bool bContinueFurther = false;
            bool bContinueFurther2 = false;

            try
            {

                #region Protocol

                #region Connect to SFTP
                SshTransferProtocolBase oProtocol;

                oProtocol = new Sftp(txtHost.Text, txtUser.Text, txtPass.Text);

                oProtocol.OnTransferStart += new FileTransferEvent(sshCp_OnTransferStart);
                oProtocol.OnTransferProgress += new FileTransferEvent(sshCp_OnTransferProgress);
                oProtocol.OnTransferEnd += new FileTransferEvent(sshCp_OnTransferEnd);


                try
                {
                    oProtocol.Connect();
                    UpdateLog(DateTime.Now, "Connected to SFTP.");
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Unable to connect. Detail Message: " + ex.Message+" - "+ex.Source);
                    if (oProtocol.Connected) oProtocol.Close();
                    UpdateLog(DateTime.Now, "Unable to connect to SFTP. ");
                    return;
                }
                #endregion

                #region Copy 2 Local for NBL
                string sRemoteFilePath = txtRemoteFilePath.Text;
                string sLocalFilePath = txtLocalFilePath.Text;
                DirectoryInfo oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                try
                {
                    oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                    foreach (FileInfo oFile in oDirectory.GetFiles())
                    {
                        oFile.Delete();
                    }
                }
                catch (Exception ex)
                {
                    UpdateLog(DateTime.Now, "NBL : Unable to copy file to Read Directory.");
                }
                try
                {
                    oProtocol.Get(sRemoteFilePath + "/*.txt", sLocalFilePath);
                    UpdateLog(DateTime.Now, "NBL : Successfully read file from SFTP Sever.");
                }
                catch (SftpException ex)
                {
                    UpdateLog(DateTime.Now, "NBL : Unable to get file from SFTP.");
                    //if (oProtocol.Connected) oProtocol.Close();
                    //return;
                    //MessageBox.Show("Unable to get file from SFTP. Detail Message: " + ex.Message);
                }
                #endregion

                #region Copy 2 Local for ELANCO
                string sRemoteFilePathELanco = txtRemoteFilePathELANCO.Text;
                string sLocalFilePathELanco = txtLocalFilePathELANCO.Text;
                oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
                try
                {
                    oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
                    foreach (FileInfo oFile in oDirectory.GetFiles())
                    {
                        oFile.Delete();
                    }
                }
                catch (Exception ex)
                {
                    UpdateLog(DateTime.Now, "Elanco : Unable to copy file to Read Directory.");
                }
                try
                {
                    oProtocol.Get(sRemoteFilePathELanco + "/*.txt", sLocalFilePathELanco);
                    UpdateLog(DateTime.Now, "Elanco : Successfully read file from SFTP Sever.");
                }
                catch (SftpException ex)
                {
                    UpdateLog(DateTime.Now, "Elanco : Unable to get file from SFTP.");
                    //if (oProtocol.Connected) oProtocol.Close();
                    //return;
                    //MessageBox.Show("Unable to get file from SFTP. Detail Message: " + ex.Message);
                }
                #endregion

                oProtocol.Close();

                #endregion

                # region File Serial for NBL
                DataTable oTable = new DataTable();
                DataRow oRow;

                try
                {
                    oDirectory = new DirectoryInfo(txtLocalFilePath.Text);

                    oTable.Columns.Add("SerialNo", Type.GetType("System.String"));

                    foreach (FileInfo oFile in oDirectory.GetFiles())
                    {
                        oRow = oTable.NewRow();
                        oRow["SerialNo"] = oFile.Name.Split('-')[1].ToString();
                        oTable.Rows.Add(oRow);
                    }
                    DataView oV = oTable.DefaultView;
                    oV.Sort = "SerialNo ASC";
                    oTable = oV.ToTable(true, "SerialNo");
                    _sMaxSerial = oTable.Rows[oTable.Rows.Count - 1]["SerialNo"].ToString();
                }
                catch (Exception ex)
                {
                    UpdateLog(DateTime.Now, "NBL : Serial Copy Error.");
                }
                #endregion

                int nCount = 0;
                bool bDuplicate = false;

                foreach (DataRow oFileRow in oTable.Rows)
                {
                    //Update Successfully Receive Log
                    oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                    nCount = 0;
                    nFileCount = 0;
                    _sFileSerial = "";

                    #region check duplicate for NBL
                    try
                    {
                        oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                        bDuplicate = false;
                        foreach (FileInfo oFile in oDirectory.GetFiles())
                        {
                            if (oFile.Name.Contains(oFileRow["SerialNo"].ToString()))
                            {
                                string sFName = oFile.Name.Remove(oFile.Name.Length - 4);
                                if (IsDuplicateFile(sFName))
                                {
                                    bDuplicate = true;
                                    UpdateLog(DateTime.Now, "NBL : Duplicate file: " + oFile.Name + ", File already uploaded.");
                                    MoveToBackupFolderFinal(oFileRow["SerialNo"].ToString());
                                    break;
                                }
                                else
                                {
                                    nFileCount = 1;
                                    _sFileSerial = oFileRow["SerialNo"].ToString();
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "NBL : Unable to copy file to Read Directory.");
                    }
                    #endregion
                }

                # region File Serial for Elanco
                DataTable oTable2 = new DataTable();
                DataRow oRow2;

                try
                {
                    oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);

                    oTable2.Columns.Add("SerialNo", Type.GetType("System.String"));

                    foreach (FileInfo oFile in oDirectory.GetFiles())
                    {
                        oRow2 = oTable2.NewRow();
                        oRow2["SerialNo"] = oFile.Name.Split('-')[1].ToString();
                        oTable2.Rows.Add(oRow2);
                    }
                    DataView oV2 = oTable2.DefaultView;
                    oV2.Sort = "SerialNo ASC";
                    oTable2 = oV2.ToTable(true, "SerialNo");
                    _sMaxSerial2 = oTable2.Rows[oTable2.Rows.Count - 1]["SerialNo"].ToString();
                }
                catch (Exception ex)
                {
                    UpdateLog(DateTime.Now, "Elanco : Serial Copy Error.");
                }
                #endregion

                int nCount2 = 0;
                bool bDuplicate2 = false;

                foreach (DataRow oFileRow in oTable2.Rows)
                {
                    //Update Successfully Receive Log
                    oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
                    nCount2 = 0;
                    nFileCount2 = 0;
                    _sFileSerial2 = "";

                    #region check duplicate for Elanco
                    try
                    {
                        oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
                        bDuplicate2 = false;
                        foreach (FileInfo oFile in oDirectory.GetFiles())
                        {
                            if (oFile.Name.Contains(oFileRow["SerialNo"].ToString()))
                            {
                                string sFName = oFile.Name.Remove(oFile.Name.Length - 4);
                                if (IsDuplicateFile_ELANCO(sFName))
                                {
                                    bDuplicate2 = true;
                                    UpdateLog(DateTime.Now, "Elanco : Duplicate file: " + oFile.Name + ", File already uploaded.");

                                    TimeSpan dtEndTime = new TimeSpan(11, 0, 0); //11 o'clock
                                    TimeSpan dtNowTime = DateTime.Now.TimeOfDay;
                                    if (dtNowTime > dtEndTime)
                                    {
                                        MoveToBackupFolderFinal_ELANCO(oFileRow["SerialNo"].ToString());
                                        MoveToBackupElancoEcommerceFiles();
                                    }
                                    break;
                                }
                                else
                                {
                                    nFileCount2 = 1;
                                    _sFileSerial2 = oFileRow["SerialNo"].ToString();
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "Elanco : Unable to copy file to Read Directory.");
                    }
                    #endregion
                }

                if (!bDuplicate || !bDuplicate2)
                {
                    bContinueFurther = false;

                    //Copy to Backup folder
                    #region copy to folder for NBL

                    oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                    try
                    {
                        oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                        foreach (FileInfo oFile in oDirectory.GetFiles())
                        {
                            if (oFile.Name.Contains(_sFileSerial))
                            {
                                //File.Copy(oFile.FullName, txtBkup.Text + "\\" + oFile.Name, true);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "NBL : Unable to copy file to Bkup Directory.");

                    }
                    UpdateLog(DateTime.Now, "NBL : File copied to backup folder.");



                    //Clear DB Folder
                    oDirectory = new DirectoryInfo(txtDBPath.Text);
                    try
                    {
                        oDirectory = new DirectoryInfo(txtDBPath.Text);
                        foreach (FileInfo oFile in oDirectory.GetFiles())
                        {
                            if (oFile.Name.Contains(_sFileSerial))
                            {
                                oFile.Delete();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "NBL : Unable to copy file to Read Directory.");
                    }
                    //Copy to DB folder
                    try
                    {
                        oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                        foreach (FileInfo oFile in oDirectory.GetFiles())
                        {
                            if (oFile.Name.Contains(_sFileSerial))
                            {
                                File.Copy(oFile.FullName, txtDBPath.Text + "\\" + oFile.Name, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "NBL : Unable to copy file to DB Directory.");
                    }

                    UpdateLog(DateTime.Now, "NBL : File copied to DB folder.");


                    #endregion
                    //Fire Stored Procedure

                    //Copy to Backup folder
                    #region copy to folder for Elanco

                    //oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
                    //try
                    //{
                    //    oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
                    //    foreach (FileInfo oFile in oDirectory.GetFiles())
                    //    {
                    //        if (oFile.Name.Contains(oFileRow["SerialNo"].ToString()))
                    //        {
                    //            //File.Copy(oFile.FullName, txtBkup.Text + "\\" + oFile.Name, true);
                    //        }
                    //    }

                    //}
                    //catch (Exception ex)
                    //{
                    //    UpdateLog_ELANCO(DateTime.Now, "Unable to copy file to Bkup Directory.");

                    //}
                    //UpdateLog_ELANCO(DateTime.Now, "File copied to backup folder.");



                    //Clear DB Folder
                    oDirectory = new DirectoryInfo(txtDBPathELANCO.Text);
                    try
                    {
                        oDirectory = new DirectoryInfo(txtDBPathELANCO.Text);
                        foreach (FileInfo oFile in oDirectory.GetFiles())
                        {
                            if (oFile.Name.Contains(_sFileSerial2))
                            {
                                oFile.Delete();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "Elanco : Unable to copy file to Read Directory.");
                    }
                    //Copy to DB folder
                    try
                    {
                        oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
                        foreach (FileInfo oFile in oDirectory.GetFiles())
                        {
                            if (oFile.Name.Contains(_sFileSerial2))
                            {
                                File.Copy(oFile.FullName, txtDBPathELANCO.Text + "\\" + oFile.Name, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "Elanco : Unable to copy file to DB Directory.");
                    }

                    UpdateLog(DateTime.Now, "Elanco : File copied to DB folder.");


                    #endregion
                    //Fire Stored Procedure

                    try
                    {
                        ExecuteSP(_sFileSerial, _sFileSerial2);
                        bContinueFurther = true;
                    }
                    catch (Exception ex)
                    {
                        bContinueFurther = false;
                        return;
                    }

                    #region delete from DBPath for NBL
                    oDirectory = new DirectoryInfo(txtDBPath.Text);
                    try
                    {
                        oDirectory = new DirectoryInfo(txtDBPath.Text);
                        foreach (FileInfo oFile in oDirectory.GetFiles())
                        {
                            if (oFile.Name.Contains(_sFileSerial))
                            {
                                oFile.Delete();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "Unable to copy file to Read Directory.");
                    }
                    #endregion

                    #region delete from DBPath for Elanco
                    oDirectory = new DirectoryInfo(txtDBPathELANCO.Text);
                    try
                    {
                        oDirectory = new DirectoryInfo(txtDBPathELANCO.Text);
                        foreach (FileInfo oFile in oDirectory.GetFiles())
                        {
                            if (oFile.Name.Contains(_sFileSerial2))
                            {
                                oFile.Delete();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "Elanco : Unable to copy file to Read Directory.");
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                return;
            }
        }

        public void ExecuteSP(string sFileSerial, string sFileSerial2)
        {
            SqlConnection oConnection = new SqlConnection();            
            bool bTransfer2Backup = false;
            DirectoryInfo oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                             
            try
            {
                NameValueCollection oValues = (NameValueCollection)ConfigurationSettings.GetConfig("appParams");
                oConnection.ConnectionString = oValues["ConnectionString"].ToString();
            }
            catch
            {
                UpdateLog(DateTime.Now, "Connection String Missing");
                return;
            }

            try
            {
                oConnection.Open();
            }
            catch (Exception ex)
            {
                throw new Exception();
            }

            SqlCommand oCommand = new SqlCommand();
            oCommand.CommandType = CommandType.StoredProcedure;

            string sAdjustFileName = "", sAdjustmentDetails = ""
                , sBranchInventory = "", sCancelInvoice = "", sCancelInvoiceDetails = ""
                , sCustomer = "", sCustomerCollection = "", sFileSummary = ""
                , sInvoice = "", sInvoiceDetail = "", sInvoiceBonus = "", sProduct = ""
                , sRoute = "", sTransfer = "", sTransferDetails = "", sDeliveryStatus = "", sDeletedOrders = "";

            string sAdjustFileName2 = "", sAdjustmentDetails2 = ""
                , sBranchInventory2 = "", sCancelInvoice2 = "", sCancelInvoiceDetails2 = ""
                , sCustomer2 = "", sCustomerCollection2 = "", sFileSummary2 = ""
                , sInvoice2 = "", sInvoiceDetail2 = "", sInvoiceBonus2 = "", sProduct2 = ""
                , sRoute2 = "", sTransfer2 = "", sTransferDetails2 = "", sDeliveryStatus2 = "", sDeletedOrders2 = "";


            #region Get Files for NBL
            if (nFileCount == 1)
            {
                foreach (FileInfo oFile in oDirectory.GetFiles())
                {
                    if (oFile.Name.Contains(sFileSerial))
                    {
                        if (oFile.Name.Contains("tblAdjustment-"))
                        {
                            sAdjustFileName = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblAdjustmentDetails-"))
                        {
                            sAdjustmentDetails = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblBranchInventory-"))
                        {
                            sBranchInventory = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblCancelInvoice-"))
                        {
                            sCancelInvoice = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblCanInvDetails-"))
                        {
                            sCancelInvoiceDetails = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblCustomer-"))
                        {
                            sCustomer = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblCustomerCollection-"))
                        {
                            sCustomerCollection = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblDeliveryStatus-"))
                        {
                            sDeliveryStatus = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblFilesSummary-"))
                        {
                            sFileSummary = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblInvBonsDetl-"))
                        {
                            sInvoiceBonus = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblInvoice-"))
                        {
                            sInvoice = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblInvoiceDetails-"))
                        {
                            sInvoiceDetail = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblProduct-"))
                        {
                            sProduct = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblRoute-"))
                        {
                            sRoute = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblTransferReceive-"))
                        {
                            sTransfer = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblTrnsRevDetails-"))
                        {
                            sTransferDetails = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblDeletedOrders-"))
                        {
                            sDeletedOrders = oFile.Name;
                        }
                    }

                }
            }
            #endregion

            #region Get Files for Elanco
            if (nFileCount2 == 1)
            {
                oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
                foreach (FileInfo oFile in oDirectory.GetFiles())
                {
                    if (oFile.Name.Contains(sFileSerial2))
                    {
                        if (oFile.Name.Contains("tblAdjustment-"))
                        {
                            sAdjustFileName2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblAdjustmentDetails-"))
                        {
                            sAdjustmentDetails2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblBranchInventory-"))
                        {
                            sBranchInventory2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblCancelInvoice-"))
                        {
                            sCancelInvoice2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblCanInvDetails-"))
                        {
                            sCancelInvoiceDetails2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblCustomer-"))
                        {
                            sCustomer2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblCustomerCollection-"))
                        {
                            sCustomerCollection2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblDeliveryStatus-"))
                        {
                            sDeliveryStatus2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblFilesSummary-"))
                        {
                            sFileSummary2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblInvBonsDetl-"))
                        {
                            sInvoiceBonus2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblInvoice-"))
                        {
                            sInvoice2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblInvoiceDetails-"))
                        {
                            sInvoiceDetail2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblProduct-"))
                        {
                            sProduct2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblRoute-"))
                        {
                            sRoute2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblTransferReceive-"))
                        {
                            sTransfer2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblTrnsRevDetails-"))
                        {
                            sTransferDetails2 = oFile.Name;
                        }
                        if (oFile.Name.Contains("tblDeletedOrders-"))
                        {
                            sDeletedOrders2 = oFile.Name;
                        }
                    }

                }
            }
            #endregion
            
            if (_bTimer2 || _bTimer3 || _bTimer4)
            {
                UpdateLog(DateTime.Now, "Timer 234 Executing");
                #region for Timer234

                #region For Customer Only
                if (sBranchInventory == "" && sFileSummary != "" && sFileSerial != "" && sCustomer != "")
                {
                    oCommand = new SqlCommand("spUploadCustomerOnly", oConnection);
                    oCommand.CommandType = CommandType.StoredProcedure;
                    oCommand.CommandTimeout = 0;

                    #region Add Stored Proc Parameter
                    oCommand.Parameters.AddWithValue("@FilesSummaryFileName", sFileSummary);
                    oCommand.Parameters.AddWithValue("@CustomerFileName", sCustomer);
                    #endregion

                    #region upload Customer File
                    try
                    {
                        UpdateLog(DateTime.Now, "Executing SP 4 Customer Only");
                        oCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "Stored Procedure Fail 4 Customer Update." + ex.Source.Substring(0, 500));
                        return;
                    }
                    #endregion

                    #region Create File 4 FAST
                    //try
                    //{
                    //    UpdateLog(DateTime.Now, "Creating File for FAST");
                    //    CreateFile4FAST();
                    //}
                    //catch (Exception ex)
                    //{
                    //    UpdateLog(DateTime.Now, "FAST Customer File Creation Fail" + ex.Source);
                    //    return;
                    //}
                    #endregion

                    #region spUploadFASTCustomerOnly
                    try
                    {
                        try
                        {
                            oDirectory = new DirectoryInfo(txtDBPath.Text);
                            foreach (FileInfo oFile in oDirectory.GetFiles())
                            {
                                File.Copy(oFile.FullName, txtSyncFilePath.Text + "\\" + oFile.Name, true);
                            }
                        }
                        catch (Exception ex)
                        {
                            UpdateLog(DateTime.Now, "Unable to copy Customer file to Sync Directory.");
                        }

                        UpdateLog(DateTime.Now, "Customer File copied to Sync folder.");


                        oCommand = new SqlCommand("spUploadFASTCustomerOnly", oConnection);
                        oCommand.CommandType = CommandType.StoredProcedure;
                        oCommand.CommandTimeout = 0;

                        UpdateLog(DateTime.Now, "Executing [spUploadFASTCustomerOnly] Only");
                        oCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "Stored Procedure Fail [spUploadFASTCustomerOnly]." + ex.Source.Substring(0, 500));
                        return;
                    }
                    #endregion

                    UpdateLog(DateTime.Now, "Data uploaded successfully.");
                    bTransfer2Backup = true;
                }
                else
                {
                    UpdateLog(DateTime.Now, "Customer ONLY Timer 234 Not all file received");
                }
                #endregion

                #region Branch Inventory ONLY
                UpdateLog(DateTime.Now, "Checking Branch Inventory File");
                if (sBranchInventory != "" && sFileSummary != "" && sFileSerial != "" && sCustomer == "")
                {
                    oCommand = new SqlCommand("spUploadBranchInventoryOnly", oConnection);
                    oCommand.CommandType = CommandType.StoredProcedure;
                    oCommand.CommandTimeout = 0;

                    #region Add Stored Proc Parameter
                    oCommand.Parameters.AddWithValue("@BranchInventoryFileName", sBranchInventory);
                    oCommand.Parameters.AddWithValue("@FilesSummaryFileName", sFileSummary);
                    #endregion

                    try
                    {
                        UpdateLog(DateTime.Now, "Executing SP 4 Branch Inventory Only");
                        oCommand.ExecuteNonQuery();
                        UpdateLog(DateTime.Now, "Data uploaded successfully.");
                        bTransfer2Backup = true;
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message + " --- " + ex.Source);
                        UpdateLog(DateTime.Now, "Stored Procedure Fail 4 Branch Inventory." + ex.Source);
                        return;
                    }
                }
                else
                {
                    UpdateLog(DateTime.Now, "Branch Inventory Timer 234 Not all file received");
                }
                #endregion

                #endregion
            }
            if (_bTimer1)
            {
                //Copy to Sync folder
                try
                {
                    oDirectory = new DirectoryInfo(txtDBPath.Text);
                    foreach (FileInfo oFile in oDirectory.GetFiles())
                    {
                        File.Copy(oFile.FullName, txtSyncFilePath.Text + "\\" + oFile.Name, true);

                    }
                }
                catch (Exception ex)
                {
                    UpdateLog(DateTime.Now, "Unable to copy file to Sync Directory.");
                }

                UpdateLog(DateTime.Now, "File copied to Sync folder.");
                

                UpdateLog(DateTime.Now, "Timer 1 Executing");
                #region for Timer1
                if ((sAdjustFileName != "" && sAdjustmentDetails != ""
                    && sBranchInventory != "" && sCustomer != "" && sFileSummary != ""
                    && sProduct != "" && sRoute != "" && sTransfer != "" && sTransferDetails != ""
                    && sDeliveryStatus != "" && sInvoice != "" && sInvoiceDetail != "" && sInvoiceBonus != ""
                    && sCancelInvoice != "" && sCancelInvoiceDetails != "" && sCustomerCollection != "" && sDeletedOrders != "") ||
                    (sAdjustFileName2 != "" && sAdjustmentDetails2 != ""
                    && sBranchInventory2 != "" && sCustomer2 != "" && sFileSummary2 != ""
                    && sProduct2 != "" && sRoute2 != "" && sTransfer2 != "" && sTransferDetails2 != ""
                    && sDeliveryStatus2 != "" && sInvoice2 != "" && sInvoiceDetail2 != "" && sInvoiceBonus2 != ""
                    && sCancelInvoice2 != "" && sCancelInvoiceDetails2 != "" && sCustomerCollection2 != "" && sDeletedOrders2 != ""))
                {
                    #region spUploadData
                    oCommand = new SqlCommand("spUploadData", oConnection);
                    oCommand.Parameters.Clear();
                    oCommand.CommandType = CommandType.StoredProcedure;
                    oCommand.CommandTimeout = 0;
                    if (nFileCount == 1)
                    {
                        oCommand.Parameters.AddWithValue("@AdjustmentFileName", sAdjustFileName);
                        oCommand.Parameters.AddWithValue("@AdjustmentDetailsFileName", sAdjustmentDetails);
                        oCommand.Parameters.AddWithValue("@BranchInventoryFileName", sBranchInventory);
                        oCommand.Parameters.AddWithValue("@CancelInvoiceFileName", sCancelInvoice);
                        oCommand.Parameters.AddWithValue("@CancelInvoiceDetailsFileName", sCancelInvoiceDetails);
                        oCommand.Parameters.AddWithValue("@CustomerFileName", sCustomer);
                        oCommand.Parameters.AddWithValue("@CustomerCollectionHistoryFileName", sCustomerCollection);
                        oCommand.Parameters.AddWithValue("@DeliveryStatusFileName", sDeliveryStatus);
                        oCommand.Parameters.AddWithValue("@FilesSummaryFileName", sFileSummary);
                        oCommand.Parameters.AddWithValue("@InvoiceBonusDetailsFileName", sInvoiceBonus);
                        oCommand.Parameters.AddWithValue("@InvoiceFileName", sInvoice);
                        oCommand.Parameters.AddWithValue("@InvoiceDetailsFileName", sInvoiceDetail);
                        oCommand.Parameters.AddWithValue("@ProductFileName", sProduct);
                        oCommand.Parameters.AddWithValue("@RouteFileName", sRoute);
                        oCommand.Parameters.AddWithValue("@TransferReceiveFileName", sTransfer);
                        oCommand.Parameters.AddWithValue("@TransferReceiveDetailsFileName", sTransferDetails);                        
                        oCommand.Parameters.AddWithValue("@DeletedOrders", sDeletedOrders);
                    }
                    oCommand.Parameters.AddWithValue("@FileCountNBL", nFileCount);
                    if (nFileCount2 == 1)
                    {
                        oCommand.Parameters.AddWithValue("@AdjustmentFileName2", sAdjustFileName2);
                        oCommand.Parameters.AddWithValue("@AdjustmentDetailsFileName2", sAdjustmentDetails2);
                        oCommand.Parameters.AddWithValue("@BranchInventoryFileName2", sBranchInventory2);
                        oCommand.Parameters.AddWithValue("@CancelInvoiceFileName2", sCancelInvoice2);
                        oCommand.Parameters.AddWithValue("@CancelInvoiceDetailsFileName2", sCancelInvoiceDetails2);
                        oCommand.Parameters.AddWithValue("@CustomerFileName2", sCustomer2);
                        oCommand.Parameters.AddWithValue("@CustomerCollectionHistoryFileName2", sCustomerCollection2);
                        oCommand.Parameters.AddWithValue("@DeliveryStatusFileName2", sDeliveryStatus2);
                        oCommand.Parameters.AddWithValue("@FilesSummaryFileName2", sFileSummary2);
                        oCommand.Parameters.AddWithValue("@InvoiceBonusDetailsFileName2", sInvoiceBonus2);
                        oCommand.Parameters.AddWithValue("@InvoiceFileName2", sInvoice2);
                        oCommand.Parameters.AddWithValue("@InvoiceDetailsFileName2", sInvoiceDetail2);
                        oCommand.Parameters.AddWithValue("@ProductFileName2", sProduct2);
                        oCommand.Parameters.AddWithValue("@RouteFileName2", sRoute2);
                        oCommand.Parameters.AddWithValue("@TransferReceiveFileName2", sTransfer2);
                        oCommand.Parameters.AddWithValue("@TransferReceiveDetailsFileName2", sTransferDetails2);
                        oCommand.Parameters.AddWithValue("@DeletedOrders2", sDeletedOrders2);
                    }
                    oCommand.Parameters.AddWithValue("@FileCountEL", nFileCount2);
                    oCommand.Parameters.AddWithValue("@FileDateString", DateTime.Now.ToString("dd MMM yyyy"));

                    try
                    {
                        oCommand.ExecuteNonQuery();
                        UpdateLog(DateTime.Now, "Stored Procedure Success.");
                        bTransfer2Backup = true;
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message + " --- " + ex.Source + " ********* " + ex.InnerException.Message);

                        UpdateLog(DateTime.Now, "Stored Procedure Fail." + ex.Source);

                        return;
                    }

                    #endregion

                    #region Update FAST


                    try
                    {
                        UpdateLog(DateTime.Now, "Updating FAST");
                        oCommand = new SqlCommand("spUpdateFAST", oConnection);
                        oCommand.CommandType = CommandType.StoredProcedure;
                        oCommand.CommandTimeout = 0;
                        oCommand.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "FAST Update Fail" + ex.Source);
                        return;
                    }
                    #endregion


                    if ((_sMaxSerial.ToString() == sFileSerial.ToString()) || (_sMaxSerial2.ToString() == sFileSerial2.ToString()))
                    {
                        #region spCalculateData
                        oCommand = new SqlCommand("spCalculateData", oConnection);
                        oCommand.CommandType = CommandType.StoredProcedure;
                        oCommand.CommandTimeout = 0;


                        try
                        {
                            oCommand.ExecuteNonQuery();
                            UpdateLog(DateTime.Now, "spCalculateData Success.");
                        }
                        catch (Exception ex)
                        {
                            UpdateLog(DateTime.Now, "spCalculateData Fail." + ex.Source);
                            if (ex.Source != "mscorlib")
                                return;
                        }

                        #endregion

                        #region spMobileReportData
                        UpdateLog(DateTime.Now, "Starting Mobile Update.");
                        oCommand = new SqlCommand("spMobileReportData", oConnection);
                        oCommand.CommandType = CommandType.StoredProcedure;
                        oCommand.CommandTimeout = 0;


                        try
                        {
                            oCommand.ExecuteNonQuery();
                            UpdateLog(DateTime.Now, "spMobileReportData Success.");
                            UpdateELReportGenerateStatus();
                            UpdateLog(DateTime.Now, "Elanco Report Generate Status Updated.");
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(ex.Message + " --- " + ex.Source);
                            UpdateLog(DateTime.Now, "spMobileReportData Fail." + ex.Source);
                            return;
                        }
                    }
                        #endregion

                }
                else
                {
                    UpdateLog(DateTime.Now, "Timer 234 Not all file received");
                }
                #endregion
            }

            #region Moving to backup folder

            if (bTransfer2Backup)
            {
                MoveToBackupFolderFinal(_sFileSerial);
                //foreach (FileInfo oFile in oDirectory.GetFiles())
                //{
                //    if (oFile.Name.Contains(sFileSerial))
                //    {
                //        //MoveToBackupFolderFileWise(oFile.Name);
                //        //MoveToBackupFolder(oFile.Name);
                //        MoveToBackupFolderFinal(oFile.Name);
                //    }
                //}
            }
            #endregion
        }

        public bool IsDuplicateFile(string sFilename)
        {
            SqlConnection oConnection = new SqlConnection();

            try
            {
                NameValueCollection oValues = (NameValueCollection)ConfigurationSettings.GetConfig("appParams");
                oConnection.ConnectionString = oValues["ConnectionString"].ToString();
            }
            catch
            {
                throw new Exception("Connection String Missing. ");
            }


            SqlCommand oCommand = new SqlCommand();
            oConnection.Open();
            oCommand.CommandType = CommandType.Text;
            oCommand.Connection = oConnection;
            oCommand.CommandText = "SELECT COUNT(*) FROM tblFilesSummary WHERE FileName='" + sFilename + "'";
            if (oCommand.ExecuteScalar().ToString() == "0")
            {
                oConnection.Close();
                return false;

            }
            else
            {
                oConnection.Close();
                return true;
            }
        }

        public bool UpdateLog(DateTime dLogTime, string sLogComment)
        {
            SqlConnection oConnection = new SqlConnection();

            try
            {
                NameValueCollection oValues = (NameValueCollection)ConfigurationSettings.GetConfig("appParams");
                oConnection.ConnectionString = oValues["ConnectionString"].ToString();
            }
            catch
            {
                MessageBox.Show("Connection String Missing. ");
            }


            SqlCommand oCommand = new SqlCommand();
            try
            {
                oConnection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open connection. Detail message: " + ex.Message + " - " + sLogComment);
            }
            oCommand.CommandType = CommandType.Text;
            oCommand.Connection = oConnection;
            oCommand.CommandText = "INSERT INTO UploadLog(LogDateTime,LogComment) VALUES('" + dLogTime.ToString("dd MMM yyyy HH:mm") + "','" + sLogComment + "')";
            try
            {
                oCommand.ExecuteNonQuery();
                oConnection.Close();
                return true;
            }
            catch (Exception ex)
            {
                oConnection.Close();
                //MessageBox.Show("Unable to update log file. Detail Message: "+ex.Message);
                return false;
            }
        }

        public bool IsDuplicateFile_ELANCO(string sFilename)
        {
            SqlConnection oConnection = new SqlConnection();

            try
            {
                NameValueCollection oValues = (NameValueCollection)ConfigurationSettings.GetConfig("appParams");
                oConnection.ConnectionString = oValues["ConnectionString"].ToString();
            }
            catch
            {
                throw new Exception("Connection String Missing. ");
            }

            SqlCommand oCommand = new SqlCommand();
            oConnection.Open();
            oCommand.CommandType = CommandType.Text;
            oCommand.Connection = oConnection;
            oCommand.CommandText = "SELECT COUNT(*) FROM tblFilesSummary_ELANCO WHERE FileName='" + sFilename + "'";
            if (oCommand.ExecuteScalar().ToString() == "0")
            {
                oConnection.Close();
                return false;

            }
            else
            {
                oConnection.Close();
                return true;
            }
        }
                
        public bool UpdateELReportGenerateStatus()
        {
            SqlConnection oConnection = new SqlConnection();

            try
            {
                NameValueCollection oValues = (NameValueCollection)ConfigurationSettings.GetConfig("appParams");
                oConnection.ConnectionString = oValues["ConnectionString"].ToString();
            }
            catch
            {
                MessageBox.Show("Connection String Missing. ");
            }

            SqlCommand oCommand = new SqlCommand();
            try
            {
                oConnection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open connection. Detail message: " + ex.Message);
            }
            oCommand.CommandType = CommandType.Text;
            oCommand.Connection = oConnection;
            oCommand.CommandText = "Update [ZPBLData].[dbo].[GenerateReport_ELANCO] SET [CreateReport] = 1";
            try
            {
                oCommand.ExecuteNonQuery();
                oConnection.Close();
                return true;
            }
            catch (Exception ex)
            {
                oConnection.Close();
                //MessageBox.Show("Unable to update log file. Detail Message: "+ex.Message);
                return false;
            }
        }

        public bool GenerateReport()
        {
            SqlConnection oConnection = new SqlConnection();

            try
            {
                NameValueCollection oValues = (NameValueCollection)ConfigurationSettings.GetConfig("appParams");
                oConnection.ConnectionString = oValues["ConnectionString"].ToString();
            }
            catch
            {
                MessageBox.Show("Connection String Missing. ");
            }


            SqlCommand oCommand = new SqlCommand();
            try
            {
                oConnection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open connection. Detail message: " + ex.Message );
            }
            oCommand.CommandType = CommandType.Text;
            oCommand.Connection = oConnection;
            oCommand.CommandText = "SELECT CreateReport FROM GenerateReport";
            bool bGenerateReport = false;
            SqlDataReader oReader;
            try
            {
                oReader =oCommand.ExecuteReader();
                while (oReader.Read())
                {
                    if (oReader.GetInt16(0) == 0)
                    {
                        bGenerateReport = false;
                    }
                    else
                    {
                        bGenerateReport = true;
                    }
                }
                oReader.Close();
                oConnection.Close();
            }
            catch (Exception ex)
            {
                oConnection.Close();
                MessageBox.Show("Unable Get Data From GenerateReport");
                return false;
            }
            return bGenerateReport;
        }

        public bool UpdateGenerateReportFlag()
        {
            SqlConnection oConnection = new SqlConnection();

            try
            {
                NameValueCollection oValues = (NameValueCollection)ConfigurationSettings.GetConfig("appParams");
                oConnection.ConnectionString = oValues["ConnectionString"].ToString();
            }
            catch
            {
                MessageBox.Show("Connection String Missing. ");
            }


            SqlCommand oCommand = new SqlCommand();
            try
            {
                oConnection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open connection. Detail message: " + ex.Message );
            }
            oCommand.CommandType = CommandType.Text;
            oCommand.Connection = oConnection;
            oCommand.CommandText = "UPDATE GenerateReport SET CreateReport=0";
            try
            {
                oCommand.ExecuteNonQuery();
                oConnection.Close();
                return true;
            }
            catch (Exception ex)
            {
                oConnection.Close();
                MessageBox.Show("Unable to update GenerateReport");
                return false;
            }
        }

        private static void sshCp_OnTransferStart(string src, string dst, int transferredBytes, int totalBytes, string message)
        {

        }

        private static void sshCp_OnTransferProgress(string src, string dst, int transferredBytes, int totalBytes, string message)
        {

        }

        private static void sshCp_OnTransferEnd(string src, string dst, int transferredBytes, int totalBytes, string message)
        {

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                timer1.Interval = Convert.ToInt32(txtDuration.Text)*60000;

                timer1.Start();
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                grpLocation.Enabled = false;
                grpTime.Enabled = false;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Timer Start Error. Detail Message: "+ex.Message);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            grpLocation.Enabled = true;
            grpTime.Enabled = true;
        }

        private void frmMain2_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SshShell oShell = new SshShell(txtHost.Text, txtUser.Text, txtPass.Text);
            try
            {
                oShell.Connect();
                MessageBox.Show("1.1");
            }
            catch (Exception ex)
            {
                MessageBox.Show("1.2");
            }
            try
            {
                oShell.WriteLine("ls" );
            }
            catch (Exception ex)
            {
                MessageBox.Show("2.2");
            }
            try
            {
                oShell.Close();
                MessageBox.Show("3.1");
            }
            catch (Exception ex)
            {
                MessageBox.Show("3.2");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            #region Copy 2 Backup folder
            
            SshShell oShell = new SshShell(txtHost.Text, txtUser.Text, txtPass.Text);
            try
            {
                oShell.Connect();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to copy file to SFTP Shell.");
            }
            try
            {
                oShell.WriteLine("mv " + txtRemoteFilePath.Text + "/*.txt " + txtRemoteFilePath.Text + "/Backup/");
                MessageBox.Show("Files Transfered to Backup Folder");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to delete file from SFTP.");
            }
            try
            {
                oShell.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to close Shell.");
            }
            #endregion
        }

        private void MoveToBackupFolder1()
        {
            #region Copy 2 Backup folder
            SshShell oShell = new SshShell(txtHost.Text, txtUser.Text, txtPass.Text);
            try
            {
                oShell.Connect();

            }
            catch (Exception ex)
            {
                UpdateLog(DateTime.Now, "Unable to copy file to SFTP Shell.");
            }
            try
            {
                oShell.WriteLine("mv " + txtRemoteFilePath.Text + "/*.txt " + txtRemoteFilePath.Text + "/Backup/");
                UpdateLog(DateTime.Now, "Files Transfered to Backup Folder");
            }
            catch (Exception ex)
            {
                UpdateLog(DateTime.Now, "Unable to delete file from SFTP.");
            }
            try
            {
                oShell.Close();
            }
            catch (Exception ex)
            {
                UpdateLog(DateTime.Now, "Unable to close Shell.");
            }
            #endregion
        }

        private void MoveToBackupFolder()
        {
            #region Copy 2 Backup folder
            SshStream oSSH = new SshStream(txtHost.Text, txtUser.Text, txtPass.Text);
            try
            {
                oSSH.Write("mv " + txtRemoteFilePath.Text + "/*.txt " + txtRemoteFilePath.Text + "/Backup/");
                UpdateLog(DateTime.Now, "Files Transfered to Backup Folder");
            }
            catch (Exception ex)
            {
                UpdateLog(DateTime.Now, "Unable to delete file from SFTP.");
            }
            #endregion
        }

        private void MoveToBackupFolder(string sFileName)
        {
            #region Copy 2 Backup folder
            SshStream oSSH = new SshStream(txtHost.Text, txtUser.Text, txtPass.Text);
            try
            {
                oSSH.Write("mv " + txtRemoteFilePath.Text + "/"+sFileName + " "+ txtRemoteFilePath.Text + "/Backup/");
                UpdateLog(DateTime.Now, "Files Transfered to Backup Folder");
            }
            catch (Exception ex)
            {
                UpdateLog(DateTime.Now, "Unable to delete file from SFTP.");
            }
            #endregion
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            bool bFire = true;
            _bTimer1 = false;
            _bTimer2 = false;
            _bTimer3 = false;
            _bTimer4 = false;
            try
            {
                UpdateLog(DateTime.Now, DateTime.Now.Hour.ToString() + " - " + te1.Time.Hour.ToString()
                    + " - " + te2.Time.Hour.ToString() + " - " + te3.Time.Hour.ToString()
                    + " - " + te4.Time.Hour.ToString());
                if (DateTime.Now.Hour >= te1.Time.Hour && DateTime.Now.Hour < te2.Time.Hour)
                {
                    _bTimer1 = true;
                    UpdateLog(DateTime.Now, "Timer 1 Started");
                }
                if (DateTime.Now.Hour >= te2.Time.Hour && DateTime.Now.Hour < te3.Time.Hour)
                {
                    _bTimer2 = true;
                    UpdateLog(DateTime.Now, "Timer 2 Started");
                }
                if (DateTime.Now.Hour >= te3.Time.Hour && DateTime.Now.Hour < te4.Time.Hour)
                {
                    _bTimer3 = true;
                    UpdateLog(DateTime.Now, "Timer 3 Started");
                }
                if (DateTime.Now.Hour >= te4.Time.Hour)
                {
                    _bTimer4 = true;
                    UpdateLog(DateTime.Now, "Timer 4 Started");
                }
                if (bFire)
                {
                    FireProtocol();
                }
            }
            catch (Exception ex)
            {
                UpdateLog(DateTime.Now, "TimerTick Error");
                //MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private void CreateFile4FAST()
        {
            Process oProcess = new Process();
            string sCommand = "ZPBLDATA.dbo.Temp_Customer4Fast out \\\\GXBDDA-WFAST01\\$ZPBLCustomer\\chemist.txt -c -t\"|\" -T";
            ProcessStartInfo oInfo = new ProcessStartInfo("bcp", sCommand);
            
            try
            {
                oProcess = System.Diagnostics.Process.Start(oInfo);
            }
            catch (Exception ex)
            {
                UpdateLog(DateTime.Now, "BCP Error" + ex.Source);
            }

        }

        private void frmMain2_Load(object sender, EventArgs e)
        {
            btnStart_Click(sender, e);            
        }

        private void MoveToBackupFolderFileWise(string sFileName)
        {


            #region Copy 2 Backup folder
            SshStream oSSH = new SshStream(txtHost.Text, txtUser.Text, txtPass.Text);

            try
            {
                Tamir.SharpSsh.Sftp client = new Tamir.SharpSsh.Sftp(txtHost.Text, txtUser.Text, txtPass.Text);
                client.Connect();
                if (client.Connected)
                {


                    MoveFiles(txtHost.Text, txtUser.Text, txtPass.Text, txtRemoteFilePath.Text, txtRemoteFilePath.Text + "Backup", sFileName);
                    //MessageBox.Show("Files Transfered to Backup Folder");
                    UpdateLog(DateTime.Now, "Files Transfered to Backup Folder");
                }


            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                UpdateLog(DateTime.Now, "Unable to delete file from SFTP.");
            }
            #endregion
        }

        private void MoveToBackupFolderFinal_Test()
        {


            #region Copy 2 Backup folder
            SshStream oSSH = new SshStream(txtHost.Text, txtUser.Text, txtPass.Text);

            try
            {
                Tamir.SharpSsh.Sftp client = new Tamir.SharpSsh.Sftp(txtHost.Text, txtUser.Text, txtPass.Text);
                client.Connect();
                if (client.Connected)
                {


                    ArrayList list;
                    list = client.GetFileList(txtRemoteFilePath.Text + "*.txt");
                    string[] myArray = (string[])list.ToArray(typeof(string));
                    for (int i = 0; i < myArray.Length; i++)
                    {
                        string sFile = myArray[i];

                        MoveFiles(txtHost.Text, txtUser.Text, txtPass.Text, txtRemoteFilePath.Text, txtRemoteFilePath.Text + "Backup", sFile);
                        MessageBox.Show("Files Transfered to Backup Folder");
                        // UpdateLog(DateTime.Now, "Files Transfered to Backup Folder");
                    }
                    // DeleteFile(txtHost.Text, txtUser.Text, txtPass.Text, txtRemoteFilePath.Text, "tblBranchInventory-412-2016.11.09.txt");



                }


                //MessageBox.Show("Files Transfered to Backup Folder");

                //UpdateLog(DateTime.Now, "Files Transfered to Backup Folder");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show("Unable to delete file from SFTP.");
            }
            #endregion
        }

        private void MoveToBackupFolderFinal(string sFileSerialNo)
        {
            #region Copy 2 Backup folder
            //SshStream oSSH = new SshStream(txtHost.Text, txtUser.Text, txtPass.Text);
            try
            {
                Tamir.SharpSsh.Sftp client = new Tamir.SharpSsh.Sftp(txtHost.Text, txtUser.Text, txtPass.Text);
                client.Connect();
                if (client.Connected)
                {
                    ArrayList list;
                    list = client.GetFileList(txtRemoteFilePath.Text + "*.txt");
                    string[] myArray = (string[])list.ToArray(typeof(string));
                    for (int i = 0; i < myArray.Length; i++)
                    {
                        string sFile = myArray[i];
                        if (sFile.Contains(sFileSerialNo))
                        {
                            MoveFiles(txtHost.Text, txtUser.Text, txtPass.Text, txtRemoteFilePath.Text, txtRemoteFilePath.Text + "Backup", sFile);
                            UpdateLog(DateTime.Now, "NBL : Files Transfered to Backup Folder");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                UpdateLog(DateTime.Now, "NBL : Unable to delete file from SFTP.");
            }
            #endregion
        }

        private void MoveToBackupFolderFinal_ELANCO(string sFileSerialNo)
        {
            #region Copy 2 Backup folder
            //SshStream oSSH = new SshStream(txtHost.Text, txtUser.Text, txtPass.Text);

            try
            {
                Tamir.SharpSsh.Sftp client = new Tamir.SharpSsh.Sftp(txtHost.Text, txtUser.Text, txtPass.Text);
                client.Connect();
                if (client.Connected)
                {
                    ArrayList list;
                    list = client.GetFileList(txtRemoteFilePathELANCO.Text + "*.txt");
                    string[] myArray = (string[])list.ToArray(typeof(string));
                    for (int i = 0; i < myArray.Length; i++)
                    {
                        string sFile = myArray[i];
                        if (sFile.Contains(sFileSerialNo))
                        {
                            MoveFiles(txtHost.Text, txtUser.Text, txtPass.Text, txtRemoteFilePathELANCO.Text, txtRemoteFilePathELANCO.Text + "Backup", sFile);
                            UpdateLog(DateTime.Now, "Elanco : Files Transfered to Backup Folder");
                        }
                    }
                }

                //UpdateLog_ELANCO(DateTime.Now, "Files Transfered to Backup Folder");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                UpdateLog(DateTime.Now, "Elanco : Unable to delete file from SFTP.");
            }
            #endregion
        }

        private void MoveToBackupElancoEcommerceFiles()
        {
            #region Copy 2 Backup folder
            //SshStream oSSH = new SshStream(txtHost.Text, txtUser.Text, txtPass.Text);

            try
            {
                Tamir.SharpSsh.Sftp client = new Tamir.SharpSsh.Sftp(txtHost.Text, txtUser.Text, txtPass.Text);
                client.Connect();
                if (client.Connected)
                {
                    ArrayList list;
                    list = client.GetFileList("/ELECOMB/" + "*.txt");
                    string[] myArray = (string[])list.ToArray(typeof(string));
                    for (int i = 0; i < myArray.Length; i++)
                    {
                        string sFile = myArray[i];
                        MoveFiles(txtHost.Text, txtUser.Text, txtPass.Text, "/ELECOMB/", "/ELECOMB/Backup", sFile);
                        UpdateLog(DateTime.Now, "Elanco : ECommerce Files Transfered to Backup Folder");
                    }
                }

                //UpdateLog_ELANCO(DateTime.Now, "Files Transfered to Backup Folder");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                UpdateLog(DateTime.Now, "Elanco : Unable to delete Ecommerce file from SFTP.");
            }
            #endregion
        }

        private void uploadFile(string filePath)
        {
            //Create FTP request
            FtpWebRequest request = (FtpWebRequest)FtpWebRequest.Create("ftp://edi.zp-bd.com/Backup/" + Path.GetFileName(filePath));

            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential("prelb003", "Prbecdm23");
            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = false;

            //Load the file
            FileStream stream = File.OpenRead(filePath);
            byte[] buffer = new byte[stream.Length];

            stream.Read(buffer, 0, buffer.Length);
            stream.Close();

            //Upload file
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(buffer, 0, buffer.Length);
            reqStream.Close();

            MessageBox.Show("Uploaded Successfully");
        }
        
        public static string GetRelativePath(string ftpBasePath, string ftpToPath)
        {

            if (!ftpBasePath.StartsWith("/"))
            {
                throw new Exception("Base path is not absolute");
            }
            else
            {
                ftpBasePath = ftpBasePath.Substring(1);
            }
            if (ftpBasePath.EndsWith("/"))
            {
                ftpBasePath = ftpBasePath.Substring(0, ftpBasePath.Length - 1);
            }

            if (!ftpToPath.StartsWith("/"))
            {
                throw new Exception("Base path is not absolute");
            }
            else
            {
                ftpToPath = ftpToPath.Substring(1);
            }
            if (ftpToPath.EndsWith("/"))
            {
                ftpToPath = ftpToPath.Substring(0, ftpToPath.Length - 1);
            }
            string[] arrBasePath = ftpBasePath.Split("/".ToCharArray());
            string[] arrToPath = ftpToPath.Split("/".ToCharArray());

            int basePathCount = arrBasePath.Count();
            int levelChanged = basePathCount;
            for (int iIndex = 0; iIndex < basePathCount; iIndex++)
            {
                if (arrToPath.Count() > iIndex)
                {
                    if (arrBasePath[iIndex] != arrToPath[iIndex])
                    {
                        levelChanged = iIndex;
                        break;
                    }
                }
            }
            int HowManyBack = basePathCount - levelChanged;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < HowManyBack; i++)
            {
                sb.Append("../");
            }
            for (int i = levelChanged; i < arrToPath.Count(); i++)
            {
                sb.Append(arrToPath[i]);
                sb.Append("/");
            }

            return sb.ToString();
        }

        public static string DeleteFile(string ftpuri, string username, string password, string ftppath, string filename)
        {

            FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create("ftp://" + ftpuri + ftppath + filename);
            ftp.Method = WebRequestMethods.Ftp.DeleteFile;
            ftp.Credentials = new NetworkCredential(username, password);
            ftp.UsePassive = true;

            FtpWebResponse response = (FtpWebResponse)ftp.GetResponse();

            Stream responseStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(responseStream);

            return reader.ReadToEnd();
        }

        public static string MoveFiles(string ftpuri, string username, string password, string ftpfrompath, string ftptopath, string filename)
        {
            string retval = string.Empty;

            

            FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create("ftp://" + ftpuri + ftpfrompath + filename);
            ftp.Method = WebRequestMethods.Ftp.Rename;
            ftp.Credentials = new NetworkCredential(username, password);
            //ftp.Credentials = new NetworkCredential(username.Normalize(), password.Normalize(), ftpuri.Normalize());
            ftp.UsePassive = true;
            ftp.RenameTo = GetRelativePath(ftpfrompath, ftptopath) + filename;           


            FtpWebResponse ftpresponse = (FtpWebResponse)ftp.GetResponse();

            Stream responseStream = ftpresponse.GetResponseStream();

            StreamReader reader = new StreamReader(responseStream);

            return reader.ReadToEnd();
        }

    }
}
