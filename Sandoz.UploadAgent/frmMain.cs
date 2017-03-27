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
    public partial class frmMain : Form
    {
        bool _bTimer1 = true, _bTimer2 = true, _bTimer3 = true, _bTimer4 = true;
        string _sMaxSerial = "";
        
        public frmMain()
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
                UpdateLog_ELANCO(DateTime.Now, DateTime.Now.Hour.ToString() + " - " + te1.Time.Hour.ToString()
                    + " - " + te2.Time.Hour.ToString() + " - " + te3.Time.Hour.ToString()
                    + " - " + te4.Time.Hour.ToString());
                if (DateTime.Now.Hour >= te1.Time.Hour && DateTime.Now.Hour<te2.Time.Hour)
                {
                    _bTimer1 = true;
                    UpdateLog(DateTime.Now, "Timer 1 Started");
                    UpdateLog_ELANCO(DateTime.Now, "Timer 1 Started");
                }
                if (DateTime.Now.Hour >= te2.Time.Hour && DateTime.Now.Hour < te3.Time.Hour)
                {
                    _bTimer2 = true;
                    UpdateLog(DateTime.Now, "Timer 2 Started");
                    UpdateLog_ELANCO(DateTime.Now, "Timer 2 Started");
                }
                if (DateTime.Now.Hour >= te3.Time.Hour && DateTime.Now.Hour < te4.Time.Hour)
                {
                    _bTimer3 = true;
                    UpdateLog(DateTime.Now, "Timer 3 Started");
                    UpdateLog_ELANCO(DateTime.Now, "Timer 3 Started");
                }
                if (DateTime.Now.Hour >= te4.Time.Hour)
                {
                    _bTimer4 = true;
                    UpdateLog(DateTime.Now, "Timer 4 Started");
                    UpdateLog_ELANCO(DateTime.Now, "Timer 3 Started");
                }
                if (bFire)
                {
                    FireProtocol();                    
                    //FireProtocol_ELANCO();
                }
            }
            catch (Exception ex)
            {
                UpdateLog(DateTime.Now, "TimerTick Error");
                UpdateLog_ELANCO(DateTime.Now, "TimerTick Error");
                //MessageBox.Show("Exception: " + ex.Message);
            }
            timer1.Start();
        }

        public void FireProtocol()
        {
            bool bContinueFurther = false;
            try
            {

                #region Protocol

                #region Connect
                SshTransferProtocolBase oProtocol;

                oProtocol = new Sftp(txtHost.Text, txtUser.Text,txtPass.Text);
                
                oProtocol.OnTransferStart += new FileTransferEvent(sshCp_OnTransferStart);
                oProtocol.OnTransferProgress += new FileTransferEvent(sshCp_OnTransferProgress);
                oProtocol.OnTransferEnd += new FileTransferEvent(sshCp_OnTransferEnd);

                
                try
                {
                    oProtocol.Connect();
                    UpdateLog(DateTime.Now, "Connected to SFTP.");
                }
                catch (Exception  ex)
                {
                    //MessageBox.Show("Unable to connect. Detail Message: " + ex.Message+" - "+ex.Source);
                    if (oProtocol.Connected) oProtocol.Close();
                    UpdateLog(DateTime.Now, "Unable to connect to SFTP. ");
                    return;
                }
                #endregion

                #region Copy 2 Local
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
                    UpdateLog(DateTime.Now, "Unable to copy file to Read Directory.");
                }
                try
                {
                    oProtocol.Get(sRemoteFilePath+"/*.txt", sLocalFilePath);
                    UpdateLog(DateTime.Now, "Successfully read file from SFTP Sever.");
                }
                catch (SftpException ex)
                {
                    UpdateLog(DateTime.Now, "Unable to get file from SFTP.");
                    if(oProtocol.Connected) oProtocol.Close();
                    return;
                    //MessageBox.Show("Unable to get file from SFTP. Detail Message: " + ex.Message);
                }
                #endregion

                oProtocol.Close();

                #endregion
                
                # region File Serial
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
                    _sMaxSerial= oTable.Rows[oTable.Rows.Count - 1]["SerialNo"].ToString();
                }
                catch (Exception ex)
                {
                    UpdateLog(DateTime.Now, "Serial Copy Error.");
                }
                #endregion

                int nCount = 0;
                bool bDuplicate = false;
                
                foreach (DataRow oFileRow in oTable.Rows)
                {
                    //Update Successfully Receive Log
                    oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                    nCount = 0;

                    #region check duplicate
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
                                    UpdateLog(DateTime.Now, "Duplicate file: " + oFile.Name + ", File already uploaded.");
                                    MoveToBackupFolderFinal(oFile.Name);
                                    return;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateLog(DateTime.Now, "Unable to copy file to Read Directory.");
                    }
                    #endregion

                    if (!bDuplicate)
                    {
                        bContinueFurther = false;
                        //Copy to Backup folder
                        #region copy to folder

                        oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                        try
                        {
                            oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                            foreach (FileInfo oFile in oDirectory.GetFiles())
                            {
                                if (oFile.Name.Contains(oFileRow["SerialNo"].ToString()))
                                {
                                    File.Copy(oFile.FullName, txtBkup.Text + "\\" + oFile.Name, true);
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            UpdateLog(DateTime.Now, "Unable to copy file to Bkup Directory.");

                        }
                        UpdateLog(DateTime.Now, "File copied to backup folder.");

                  

                        //Clear DB Folder
                        oDirectory = new DirectoryInfo(txtDBPath.Text);
                        try
                        {
                            oDirectory = new DirectoryInfo(txtDBPath.Text);
                            foreach (FileInfo oFile in oDirectory.GetFiles())
                            {
                                if (oFile.Name.Contains(oFileRow["SerialNo"].ToString()))
                                {
                                    oFile.Delete();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UpdateLog(DateTime.Now, "Unable to copy file to Read Directory.");
                        }
                        //Copy to DB folder
                        try
                        {
                            oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
                            foreach (FileInfo oFile in oDirectory.GetFiles())
                            {
                                if (oFile.Name.Contains(oFileRow["SerialNo"].ToString()))
                                {
                                    File.Copy(oFile.FullName, txtDBPath.Text + "\\" + oFile.Name, true);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UpdateLog(DateTime.Now, "Unable to copy file to DB Directory.");
                        }

                        UpdateLog(DateTime.Now, "File copied to DB folder.");
                     

                        #endregion
                        //Fire Stored Procedure
                        try
                        {
                            ExecuteSP(oFileRow["SerialNo"].ToString());                            
                            bContinueFurther = true;
                        }
                        catch (Exception ex)
                        {
                            bContinueFurther = false;
                            return;
                        }
                        oDirectory = new DirectoryInfo(txtDBPath.Text);
                        try
                        {
                            oDirectory = new DirectoryInfo(txtDBPath.Text);
                            foreach (FileInfo oFile in oDirectory.GetFiles())
                            {
                                if (oFile.Name.Contains(oFileRow["SerialNo"].ToString()))
                                {
                                    oFile.Delete();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UpdateLog(DateTime.Now, "Unable to copy file to Read Directory.");
                        }
                    }
                }

                #region Copy 2 Backup folder
                //Now the Transfer to backup folder will run from ExecuteSP()
                //if (bContinueFurther)
                //{
                //    UpdateLog(DateTime.Now, "Files to be removed: " + bContinueFurther.ToString());
                //    MoveToBackupFolder();
                //}
                #endregion
             
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                return;
            }
        }

        public void ExecuteSP(string sFileSerial)
        {
            SqlConnection oConnection = new SqlConnection();
            DirectoryInfo oDirectory = new DirectoryInfo(txtLocalFilePath.Text);
            bool bTransfer2Backup = false;
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


            #region Get Files
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
                if (sAdjustFileName != "" && sAdjustmentDetails != ""
                    && sBranchInventory != "" && sCustomer != "" && sFileSummary != ""
                    && sProduct != "" && sRoute != "" && sTransfer != "" && sTransferDetails != ""
                    && sDeliveryStatus != "" && sInvoice != "" && sInvoiceDetail != "" && sInvoiceBonus != ""
                    && sCancelInvoice != "" && sCancelInvoiceDetails != "" && sCustomerCollection != "" && sDeletedOrders != "")
                {
                    #region spUploadData
                    oCommand = new SqlCommand("spUploadData", oConnection);
                    oCommand.CommandType = CommandType.StoredProcedure;
                    oCommand.CommandTimeout = 0;
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
                    oCommand.Parameters.AddWithValue("@FileDateString", DateTime.Now.ToString("dd MMM yyyy"));
                    oCommand.Parameters.AddWithValue("@DeletedOrders", sDeletedOrders);

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


                    if (_sMaxSerial.ToString() == sFileSerial.ToString())
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
                //MoveToBackupFolderFinal();
                foreach (FileInfo oFile in oDirectory.GetFiles())
                {
                    if (oFile.Name.Contains(sFileSerial))
                    {
                        //MoveToBackupFolderFileWise(oFile.Name);
                        //MoveToBackupFolder(oFile.Name);
                        MoveToBackupFolderFinal(oFile.Name);
                    }
                }
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

        public void FireProtocol_ELANCO()
        {
            bool bContinueFurther = false;
            try
            {

                #region Protocol

                #region Connect
                SshTransferProtocolBase oProtocol;

                oProtocol = new Sftp(txtHost.Text, txtUser.Text, txtPass.Text);

                oProtocol.OnTransferStart += new FileTransferEvent(sshCp_OnTransferStart);
                oProtocol.OnTransferProgress += new FileTransferEvent(sshCp_OnTransferProgress);
                oProtocol.OnTransferEnd += new FileTransferEvent(sshCp_OnTransferEnd);


                try
                {
                    oProtocol.Connect();
                    UpdateLog_ELANCO(DateTime.Now, "Connected to SFTP.");
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Unable to connect. Detail Message: " + ex.Message+" - "+ex.Source);
                    if (oProtocol.Connected) oProtocol.Close();
                    UpdateLog_ELANCO(DateTime.Now, "Unable to connect to SFTP. ");
                    return;
                }
                #endregion

                #region Copy 2 Local
                string sRemoteFilePath = txtRemoteFilePathELANCO.Text;
                string sLocalFilePath = txtLocalFilePathELANCO.Text;
                DirectoryInfo oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
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
                    UpdateLog_ELANCO(DateTime.Now, "Unable to copy file to Read Directory.");
                }
                try
                {
                    oProtocol.Get(sRemoteFilePath + "/*.txt", sLocalFilePath);
                    UpdateLog_ELANCO(DateTime.Now, "Successfully read file from SFTP Sever.");
                }
                catch (SftpException ex)
                {
                    UpdateLog_ELANCO(DateTime.Now, "Unable to get file from SFTP.");
                    if (oProtocol.Connected) oProtocol.Close();
                    return;
                    //MessageBox.Show("Unable to get file from SFTP. Detail Message: " + ex.Message);
                }
                #endregion

                oProtocol.Close();

                #endregion

                # region File Serial
                DataTable oTable = new DataTable();
                DataRow oRow;

                try
                {
                    oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);

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
                    UpdateLog_ELANCO(DateTime.Now, "Serial Copy Error.");
                }
                #endregion

                int nCount = 0;
                bool bDuplicate = false;

                foreach (DataRow oFileRow in oTable.Rows)
                {
                    //Update Successfully Receive Log
                    oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
                    nCount = 0;

                    #region check duplicate
                    try
                    {
                        oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
                        bDuplicate = false;
                        foreach (FileInfo oFile in oDirectory.GetFiles())
                        {
                            if (oFile.Name.Contains(oFileRow["SerialNo"].ToString()))
                            {
                                string sFName = oFile.Name.Remove(oFile.Name.Length - 4);
                                if (IsDuplicateFile_ELANCO(sFName))
                                {
                                    bDuplicate = true;
                                    UpdateLog_ELANCO(DateTime.Now, "Duplicate file: " + oFile.Name + ", File already uploaded.");

                                    TimeSpan dtEndTime = new TimeSpan(11, 0, 0); //11 o'clock
                                    TimeSpan dtNowTime = DateTime.Now.TimeOfDay;
                                    if (dtNowTime > dtEndTime)
                                    {
                                        //MoveToBackupFolderFinal_ELANCO();
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        UpdateLog_ELANCO(DateTime.Now, "Unable to copy file to Read Directory.");
                    }
                    #endregion

                    if (!bDuplicate)
                    {
                        bContinueFurther = false;
                        //Copy to Backup folder
                        #region copy to folder

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
                                if (oFile.Name.Contains(oFileRow["SerialNo"].ToString()))
                                {
                                    oFile.Delete();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UpdateLog_ELANCO(DateTime.Now, "Unable to copy file to Read Directory.");
                        }
                        //Copy to DB folder
                        try
                        {
                            oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
                            foreach (FileInfo oFile in oDirectory.GetFiles())
                            {
                                if (oFile.Name.Contains(oFileRow["SerialNo"].ToString()))
                                {
                                    File.Copy(oFile.FullName, txtDBPathELANCO.Text + "\\" + oFile.Name, true);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UpdateLog_ELANCO(DateTime.Now, "Unable to copy file to DB Directory.");
                        }

                        UpdateLog_ELANCO(DateTime.Now, "File copied to DB folder.");


                        #endregion
                        //Fire Stored Procedure
                        try
                        {
                            ExecuteSP_ELANCO(oFileRow["SerialNo"].ToString());
                            bContinueFurther = true;
                        }
                        catch (Exception ex)
                        {
                            bContinueFurther = false;
                            return;
                        }
                        oDirectory = new DirectoryInfo(txtDBPathELANCO.Text);
                        try
                        {
                            oDirectory = new DirectoryInfo(txtDBPathELANCO.Text);
                            foreach (FileInfo oFile in oDirectory.GetFiles())
                            {
                                if (oFile.Name.Contains(oFileRow["SerialNo"].ToString()))
                                {
                                    oFile.Delete();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            UpdateLog_ELANCO(DateTime.Now, "Unable to copy file to Read Directory.");
                        }
                    }
                }

                #region Copy 2 Backup folder
                //Now the Transfer to backup folder will run from ExecuteSP()
                //if (bContinueFurther)
                //{
                //    UpdateLog(DateTime.Now, "Files to be removed: " + bContinueFurther.ToString());
                //    MoveToBackupFolder();
                //}
                #endregion

            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                return;
            }
        }
        
        public void ExecuteSP_ELANCO(string sFileSerial)
        {
            SqlConnection oConnection = new SqlConnection();
            DirectoryInfo oDirectory = new DirectoryInfo(txtLocalFilePathELANCO.Text);
            bool bTransfer2Backup = false;
            try
            {
                NameValueCollection oValues = (NameValueCollection)ConfigurationSettings.GetConfig("appParams");
                oConnection.ConnectionString = oValues["ConnectionString"].ToString();
            }
            catch
            {
                UpdateLog_ELANCO(DateTime.Now, "Connection String Missing");
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


            #region Get Files
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
            #endregion

            if (_bTimer2 || _bTimer3 || _bTimer4)
            {
                UpdateLog_ELANCO(DateTime.Now, "Timer 234 Executing");
                #region for Timer234

                #region For Customer Only
                if (sBranchInventory == "" && sFileSummary != "" && sFileSerial != "" && sCustomer != "")
                {
                    oCommand = new SqlCommand("spUploadCustomerOnly_ELANCO", oConnection);                                               
                    oCommand.CommandType = CommandType.StoredProcedure;
                    oCommand.CommandTimeout = 0;

                    #region Add Stored Proc Parameter
                    oCommand.Parameters.AddWithValue("@FilesSummaryFileName", sFileSummary);
                    oCommand.Parameters.AddWithValue("@CustomerFileName", sCustomer);
                    #endregion

                    #region upload Customer File
                    try
                    {
                        UpdateLog_ELANCO(DateTime.Now, "Executing SP 4 Customer Only");
                        oCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        UpdateLog_ELANCO(DateTime.Now, "Stored Procedure Fail 4 Customer Update." + ex.Source.Substring(0, 500));
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

                        //try
                        //{
                        //    oDirectory = new DirectoryInfo(txtDBPathELANCO.Text);
                        //    foreach (FileInfo oFile in oDirectory.GetFiles())
                        //    {
                        //        File.Copy(oFile.FullName, txtSyncFilePath.Text + "\\" + oFile.Name, true);

                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    UpdateLog_ELANCO(DateTime.Now, "Unable to copy Customer file to Sync Directory.");
                        //}

                        //UpdateLog_ELANCO(DateTime.Now, "Customer File copied to Sync folder.");


                        oCommand = new SqlCommand("spUploadFASTCustomerOnly_ELANCO", oConnection);                                                   
                        oCommand.CommandType = CommandType.StoredProcedure;
                        oCommand.CommandTimeout = 0;

                        UpdateLog_ELANCO(DateTime.Now, "Executing [spUploadFASTCustomerOnly_ELANCO] Only");
                        oCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        UpdateLog_ELANCO(DateTime.Now, "Stored Procedure Fail [spUploadFASTCustomerOnly_ELANCO]." + ex.Source.Substring(0, 500));
                        return;
                    }
                    #endregion

                    UpdateLog_ELANCO(DateTime.Now, "Data uploaded successfully.");
                    bTransfer2Backup = true;
                }
                else
                {
                    UpdateLog_ELANCO(DateTime.Now, "Customer ONLY Timer 234 Not all file received");
                }
                #endregion

                #region Branch Inventory ONLY
                UpdateLog_ELANCO(DateTime.Now, "Checking Branch Inventory File");
                if (sBranchInventory != "" && sFileSummary != "" && sFileSerial != "" && sCustomer == "")
                {   
                    oCommand = new SqlCommand("spUploadBranchInventoryOnly_ELANCO", oConnection);
                    oCommand.CommandType = CommandType.StoredProcedure;
                    oCommand.CommandTimeout = 0;

                    #region Add Stored Proc Parameter
                    oCommand.Parameters.AddWithValue("@BranchInventoryFileName", sBranchInventory);
                    oCommand.Parameters.AddWithValue("@FilesSummaryFileName", sFileSummary);
                    #endregion

                    try
                    {
                        UpdateLog_ELANCO(DateTime.Now, "Executing SP 4 Branch Inventory Only");
                        oCommand.ExecuteNonQuery();
                        UpdateLog_ELANCO(DateTime.Now, "Data uploaded successfully.");
                        bTransfer2Backup = true;
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message + " --- " + ex.Source);
                        UpdateLog_ELANCO(DateTime.Now, "Stored Procedure Fail 4 Branch Inventory." + ex.Source);
                        return;
                    }
                }
                else
                {
                    UpdateLog_ELANCO(DateTime.Now, "Branch Inventory Timer 234 Not all file received");
                }
                #endregion

                #endregion
            }
            if (_bTimer1)
            {


                //Copy to Sync folder
                //try
                //{
                //    oDirectory = new DirectoryInfo(txtDBPathELANCO.Text);
                //    foreach (FileInfo oFile in oDirectory.GetFiles())
                //    {
                //        File.Copy(oFile.FullName, txtSyncFilePath.Text + "\\" + oFile.Name, true);

                //    }
                //}
                //catch (Exception ex)
                //{
                //    UpdateLog_ELANCO(DateTime.Now, "Unable to copy file to Sync Directory.");
                //}

                //UpdateLog_ELANCO(DateTime.Now, "File copied to Sync folder.");



                UpdateLog_ELANCO(DateTime.Now, "Timer 1 Executing");
                #region for Timer1
                if (sAdjustFileName != "" && sAdjustmentDetails != ""
                    && sBranchInventory != "" && sCustomer != "" && sFileSummary != ""
                    && sProduct != "" && sRoute != "" && sTransfer != "" && sTransferDetails != ""
                    && sDeliveryStatus != "" && sInvoice != "" && sInvoiceDetail != "" && sInvoiceBonus != ""
                    && sCancelInvoice != "" && sCancelInvoiceDetails != "" && sCustomerCollection != "" && sDeletedOrders != "")
                {
                    #region spUploadData
                    oCommand = new SqlCommand("spUploadData_ELANCO", oConnection);
                    oCommand.CommandType = CommandType.StoredProcedure;
                    oCommand.CommandTimeout = 0;
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
                    oCommand.Parameters.AddWithValue("@FileDateString", DateTime.Now.ToString("dd MMM yyyy"));
                    oCommand.Parameters.AddWithValue("@DeletedOrders", sDeletedOrders);

                    try
                    {
                        oCommand.ExecuteNonQuery();
                        UpdateLog_ELANCO(DateTime.Now, "Stored Procedure Success.");
                        bTransfer2Backup = true;
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message + " --- " + ex.Source + " ********* " + ex.InnerException.Message);

                        UpdateLog_ELANCO(DateTime.Now, "Stored Procedure Fail." + ex.Source);

                        return;
                    }

                    #endregion

                    #region Update FAST


                    try
                    {
                        UpdateLog_ELANCO(DateTime.Now, "Updating FAST");
                        oCommand = new SqlCommand("spUpdateFAST_ELANCO", oConnection);
                        oCommand.CommandType = CommandType.StoredProcedure;
                        oCommand.CommandTimeout = 0;
                        oCommand.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        UpdateLog_ELANCO(DateTime.Now, "FAST Update Fail" + ex.Source);
                        return;
                    }
                    #endregion


                    if (_sMaxSerial.ToString() == sFileSerial.ToString())
                    {
                        #region spCalculateData
                        oCommand = new SqlCommand("spCalculateData_ELANCO", oConnection);
                        oCommand.CommandType = CommandType.StoredProcedure;
                        oCommand.CommandTimeout = 0;


                        try
                        {
                            oCommand.ExecuteNonQuery();
                            UpdateLog_ELANCO(DateTime.Now, "spCalculateData_ELANCO Success.");
                        }
                        catch (Exception ex)
                        {
                            UpdateLog_ELANCO(DateTime.Now, "spCalculateData_ELANCO Fail." + ex.Source);
                            if (ex.Source != "mscorlib")
                                return;
                        }

                        #endregion

                        #region spMobileReportData
                        UpdateLog_ELANCO(DateTime.Now, "Starting Mobile Update.");
                        oCommand = new SqlCommand("spMobileReportData_ELANCO", oConnection);
                        oCommand.CommandType = CommandType.StoredProcedure;
                        oCommand.CommandTimeout = 0;


                        try
                        {
                            oCommand.ExecuteNonQuery();
                            UpdateLog_ELANCO(DateTime.Now, "spMobileReportData_ELANCO Success.");
                            //UpdateDARTStatus();
                            //UpdateLog_ELANCO(DateTime.Now, "Updated DART Status Successfully.");
                            UpdateELReportGenerateStatus();
                            UpdateLog_ELANCO(DateTime.Now, "Updated EL Report Generate Status Successfully.");
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(ex.Message + " --- " + ex.Source);
                            UpdateLog_ELANCO(DateTime.Now, "spMobileReportData_ELANCO Fail." + ex.Source);
                            return;
                        }
                    }
                        #endregion

                }
                else
                {
                    UpdateLog_ELANCO(DateTime.Now, "Timer 234 Not all file received");
                }
                #endregion
            }

            #region Moving to backup folder

            if (bTransfer2Backup)
            {
                //MoveToBackupFolderFinal_ELANCO();
                foreach (FileInfo oFile in oDirectory.GetFiles())
                {
                    if (oFile.Name.Contains(sFileSerial))
                    {
                        //MoveToBackupFolderFileWise(oFile.Name);
                        //MoveToBackupFolder(oFile.Name);
                        //MoveToBackupFolderFinal_ELANCO(oFile.Name);
                    }
                }
            }
            #endregion
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
                
        public bool UpdateLog_ELANCO(DateTime dLogTime, string sLogComment)
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
            oCommand.CommandText = "INSERT INTO UploadLog_ELANCO(LogDateTime,LogComment) VALUES('" + dLogTime.ToString("dd MMM yyyy HH:mm") + "','" + sLogComment + "')";
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

        //public bool UpdateDARTStatus()
        //{
        //    SqlConnection oConnection = new SqlConnection();

        //    try
        //    {
        //        NameValueCollection oValues = (NameValueCollection)ConfigurationSettings.GetConfig("appParams");
        //        oConnection.ConnectionString = oValues["ConnectionString"].ToString();
        //    }
        //    catch
        //    {
        //        MessageBox.Show("Connection String Missing. ");
        //    }

        //    SqlCommand oCommand = new SqlCommand();
        //    try
        //    {
        //        oConnection.Open();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Unable to open connection. Detail message: " + ex.Message);
        //    }
        //    oCommand.CommandType = CommandType.Text;
        //    oCommand.Connection = oConnection;
        //    oCommand.CommandText = "Update [LS_SRS_HO].SRS_HO.dbo.CheckStatusDART SET [Status] = 1";
        //    try
        //    {
        //        oCommand.ExecuteNonQuery();
        //        oConnection.Close();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        oConnection.Close();
        //        //MessageBox.Show("Unable to update log file. Detail Message: "+ex.Message);
        //        return false;
        //    }
        //}

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

        private void frmMain_Resize(object sender, EventArgs e)
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

        private void frmMain_Load(object sender, EventArgs e)
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
                MessageBox.Show(ex.ToString());
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

        private void MoveToBackupFolderFinal(string sFileName)
        {
            #region Copy 2 Backup folder
            //SshStream oSSH = new SshStream(txtHost.Text, txtUser.Text, txtPass.Text);

            try
            {
                Tamir.SharpSsh.Sftp client = new Tamir.SharpSsh.Sftp(txtHost.Text, txtUser.Text, txtPass.Text);
                client.Connect();
                if (client.Connected)
                {
                    //ArrayList list;
                    //list = client.GetFileList(txtRemoteFilePath.Text + "*.txt");
                    //string[] myArray = (string[])list.ToArray(typeof(string));
                    //for (int i = 0; i < myArray.Length; i++)
                    //{
                        //string sFile = myArray[i];

                    MoveFiles(txtHost.Text, txtUser.Text, txtPass.Text, txtRemoteFilePath.Text, txtRemoteFilePath.Text + "Backup", sFileName);                       
                    UpdateLog(DateTime.Now, "Files Transfered to Backup Folder");
                    //}

                }

                //UpdateLog(DateTime.Now, "Files Transfered to Backup Folder");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                UpdateLog(DateTime.Now, "Unable to delete file from SFTP.");
            }
            #endregion
        }

        private void MoveToBackupFolderFinal_ELANCO(string sFileName)
        {
            #region Copy 2 Backup folder
            //SshStream oSSH = new SshStream(txtHost.Text, txtUser.Text, txtPass.Text);

            try
            {
                Tamir.SharpSsh.Sftp client = new Tamir.SharpSsh.Sftp(txtHost.Text, txtUser.Text, txtPass.Text);
                client.Connect();
                if (client.Connected)
                {
                    //ArrayList list;
                    //list = client.GetFileList(txtRemoteFilePathELANCO.Text + "*.txt");
                    //string[] myArray = (string[])list.ToArray(typeof(string));
                    //for (int i = 0; i < myArray.Length; i++)
                    //{
                        //string sFile = myArray[i];

                    MoveFiles(txtHost.Text, txtUser.Text, txtPass.Text, txtRemoteFilePathELANCO.Text, txtRemoteFilePathELANCO.Text + "Backup", sFileName);
                        UpdateLog_ELANCO(DateTime.Now, "Files Transfered to Backup Folder");
                    //}
                }

                //UpdateLog_ELANCO(DateTime.Now, "Files Transfered to Backup Folder");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                UpdateLog_ELANCO(DateTime.Now, "Unable to delete file from SFTP.");
            }
            #endregion
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
