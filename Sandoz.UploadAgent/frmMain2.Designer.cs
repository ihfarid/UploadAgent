namespace Sandoz.UploadAgent
{
    partial class frmMain2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain2));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.txtRemoteFilePath = new System.Windows.Forms.TextBox();
            this.txtLocalFilePath = new System.Windows.Forms.TextBox();
            this.te1 = new DevExpress.XtraEditors.TimeEdit();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.te2 = new DevExpress.XtraEditors.TimeEdit();
            this.te3 = new DevExpress.XtraEditors.TimeEdit();
            this.te4 = new DevExpress.XtraEditors.TimeEdit();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label10 = new System.Windows.Forms.Label();
            this.txtDBPath = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtBkup = new System.Windows.Forms.TextBox();
            this.grpTime = new System.Windows.Forms.GroupBox();
            this.grpLocation = new System.Windows.Forms.GroupBox();
            this.txtSyncFilePath = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.txtDBPathELANCO = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtLocalFilePathELANCO = new System.Windows.Forms.TextBox();
            this.txtRemoteFilePathELANCO = new System.Windows.Forms.TextBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.label12 = new System.Windows.Forms.Label();
            this.txtDuration = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.btnUpload = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.te1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.te2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.te3.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.te4.Properties)).BeginInit();
            this.grpTime.SuspendLayout();
            this.grpLocation.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Host :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "User :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(8, 73);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Password  :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(8, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Remote File Path  :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(8, 125);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Local File Path  :";
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(127, 18);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(237, 20);
            this.txtHost.TabIndex = 0;
            this.txtHost.Text = "edi.zp-bd.com";
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(127, 44);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(237, 20);
            this.txtUser.TabIndex = 1;
            this.txtUser.Text = "prallb001";
            // 
            // txtPass
            // 
            this.txtPass.Location = new System.Drawing.Point(127, 70);
            this.txtPass.Name = "txtPass";
            this.txtPass.PasswordChar = '*';
            this.txtPass.Size = new System.Drawing.Size(237, 20);
            this.txtPass.TabIndex = 2;
            this.txtPass.Text = "pR@11AueZRX";
            // 
            // txtRemoteFilePath
            // 
            this.txtRemoteFilePath.Location = new System.Drawing.Point(127, 96);
            this.txtRemoteFilePath.Name = "txtRemoteFilePath";
            this.txtRemoteFilePath.Size = new System.Drawing.Size(237, 20);
            this.txtRemoteFilePath.TabIndex = 3;
            this.txtRemoteFilePath.Text = "/PROD/OUT_SDS/";
            // 
            // txtLocalFilePath
            // 
            this.txtLocalFilePath.Location = new System.Drawing.Point(127, 122);
            this.txtLocalFilePath.Name = "txtLocalFilePath";
            this.txtLocalFilePath.Size = new System.Drawing.Size(237, 20);
            this.txtLocalFilePath.TabIndex = 4;
            this.txtLocalFilePath.Text = "C:\\ZPBLLocalData";
            // 
            // te1
            // 
            this.te1.EditValue = new System.DateTime(2010, 4, 26, 5, 0, 0, 0);
            this.te1.Location = new System.Drawing.Point(97, 19);
            this.te1.Name = "te1";
            this.te1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.te1.Size = new System.Drawing.Size(100, 20);
            this.te1.TabIndex = 0;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(7, 22);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "1st Access :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(7, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(81, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "2nd Access :";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(7, 74);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(78, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "3rd Access :";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(7, 100);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(78, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "4th Access :";
            // 
            // te2
            // 
            this.te2.EditValue = new System.DateTime(2010, 4, 26, 11, 0, 0, 0);
            this.te2.Location = new System.Drawing.Point(97, 45);
            this.te2.Name = "te2";
            this.te2.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.te2.Size = new System.Drawing.Size(100, 20);
            this.te2.TabIndex = 1;
            // 
            // te3
            // 
            this.te3.EditValue = new System.DateTime(2010, 4, 26, 14, 0, 0, 0);
            this.te3.Location = new System.Drawing.Point(97, 71);
            this.te3.Name = "te3";
            this.te3.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.te3.Size = new System.Drawing.Size(100, 20);
            this.te3.TabIndex = 2;
            // 
            // te4
            // 
            this.te4.EditValue = new System.DateTime(2010, 4, 26, 18, 0, 0, 0);
            this.te4.Location = new System.Drawing.Point(97, 97);
            this.te4.Name = "te4";
            this.te4.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.te4.Size = new System.Drawing.Size(100, 20);
            this.te4.TabIndex = 3;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(392, 168);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(216, 33);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "&Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(392, 204);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(216, 33);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "S&top";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(392, 243);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(216, 33);
            this.btnExit.TabIndex = 5;
            this.btnExit.Text = "E&xit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(8, 151);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(86, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "DB File Path :";
            // 
            // txtDBPath
            // 
            this.txtDBPath.Location = new System.Drawing.Point(127, 148);
            this.txtDBPath.Name = "txtDBPath";
            this.txtDBPath.Size = new System.Drawing.Size(237, 20);
            this.txtDBPath.TabIndex = 5;
            this.txtDBPath.Text = "C:\\ZPBLData";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(8, 258);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(98, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Bkup File Path :";
            // 
            // txtBkup
            // 
            this.txtBkup.Location = new System.Drawing.Point(127, 255);
            this.txtBkup.Name = "txtBkup";
            this.txtBkup.Size = new System.Drawing.Size(237, 20);
            this.txtBkup.TabIndex = 6;
            this.txtBkup.Text = "\\\\gxbdda-s3013\\ZPBLDataBackup$";
            // 
            // grpTime
            // 
            this.grpTime.Controls.Add(this.te1);
            this.grpTime.Controls.Add(this.label7);
            this.grpTime.Controls.Add(this.label6);
            this.grpTime.Controls.Add(this.label8);
            this.grpTime.Controls.Add(this.te4);
            this.grpTime.Controls.Add(this.label9);
            this.grpTime.Controls.Add(this.te3);
            this.grpTime.Controls.Add(this.te2);
            this.grpTime.Location = new System.Drawing.Point(392, 1);
            this.grpTime.Name = "grpTime";
            this.grpTime.Size = new System.Drawing.Size(216, 126);
            this.grpTime.TabIndex = 1;
            this.grpTime.TabStop = false;
            // 
            // grpLocation
            // 
            this.grpLocation.Controls.Add(this.txtSyncFilePath);
            this.grpLocation.Controls.Add(this.label14);
            this.grpLocation.Controls.Add(this.label11);
            this.grpLocation.Controls.Add(this.label1);
            this.grpLocation.Controls.Add(this.label3);
            this.grpLocation.Controls.Add(this.label17);
            this.grpLocation.Controls.Add(this.label4);
            this.grpLocation.Controls.Add(this.label16);
            this.grpLocation.Controls.Add(this.label5);
            this.grpLocation.Controls.Add(this.txtBkup);
            this.grpLocation.Controls.Add(this.txtDBPathELANCO);
            this.grpLocation.Controls.Add(this.label2);
            this.grpLocation.Controls.Add(this.label15);
            this.grpLocation.Controls.Add(this.txtDBPath);
            this.grpLocation.Controls.Add(this.txtLocalFilePathELANCO);
            this.grpLocation.Controls.Add(this.label10);
            this.grpLocation.Controls.Add(this.txtLocalFilePath);
            this.grpLocation.Controls.Add(this.txtHost);
            this.grpLocation.Controls.Add(this.txtRemoteFilePathELANCO);
            this.grpLocation.Controls.Add(this.txtRemoteFilePath);
            this.grpLocation.Controls.Add(this.txtUser);
            this.grpLocation.Controls.Add(this.txtPass);
            this.grpLocation.Location = new System.Drawing.Point(7, 1);
            this.grpLocation.Name = "grpLocation";
            this.grpLocation.Size = new System.Drawing.Size(379, 317);
            this.grpLocation.TabIndex = 0;
            this.grpLocation.TabStop = false;
            // 
            // txtSyncFilePath
            // 
            this.txtSyncFilePath.Location = new System.Drawing.Point(127, 285);
            this.txtSyncFilePath.Name = "txtSyncFilePath";
            this.txtSyncFilePath.Size = new System.Drawing.Size(237, 20);
            this.txtSyncFilePath.TabIndex = 8;
            this.txtSyncFilePath.Text = "D:\\FTP\\Interface";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(8, 288);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(97, 13);
            this.label14.TabIndex = 7;
            this.label14.Text = "Sync File Path :";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(8, 177);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(111, 9);
            this.label17.TabIndex = 0;
            this.label17.Text = "Remote File Path (EL)  :";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(8, 203);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(111, 12);
            this.label16.TabIndex = 0;
            this.label16.Text = "Local File Path (EL) :";
            // 
            // txtDBPathELANCO
            // 
            this.txtDBPathELANCO.Location = new System.Drawing.Point(127, 226);
            this.txtDBPathELANCO.Name = "txtDBPathELANCO";
            this.txtDBPathELANCO.Size = new System.Drawing.Size(237, 20);
            this.txtDBPathELANCO.TabIndex = 5;
            this.txtDBPathELANCO.Text = "C:\\ZPBLData_ELANCO";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(8, 229);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(109, 13);
            this.label15.TabIndex = 0;
            this.label15.Text = "DB File Path (EL):";
            // 
            // txtLocalFilePathELANCO
            // 
            this.txtLocalFilePathELANCO.Location = new System.Drawing.Point(127, 200);
            this.txtLocalFilePathELANCO.Name = "txtLocalFilePathELANCO";
            this.txtLocalFilePathELANCO.Size = new System.Drawing.Size(237, 20);
            this.txtLocalFilePathELANCO.TabIndex = 4;
            this.txtLocalFilePathELANCO.Text = "C:\\ZPBLLocalData_ELANCO";
            // 
            // txtRemoteFilePathELANCO
            // 
            this.txtRemoteFilePathELANCO.Location = new System.Drawing.Point(127, 174);
            this.txtRemoteFilePathELANCO.Name = "txtRemoteFilePathELANCO";
            this.txtRemoteFilePathELANCO.Size = new System.Drawing.Size(237, 20);
            this.txtRemoteFilePathELANCO.TabIndex = 3;
            this.txtRemoteFilePathELANCO.Text = "/EL_INTERFACE/";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Upload Agent";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(399, 136);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(81, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Check after :";
            // 
            // txtDuration
            // 
            this.txtDuration.Location = new System.Drawing.Point(486, 133);
            this.txtDuration.Name = "txtDuration";
            this.txtDuration.Size = new System.Drawing.Size(60, 20);
            this.txtDuration.TabIndex = 2;
            this.txtDuration.Text = "30";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(552, 136);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(26, 13);
            this.label13.TabIndex = 0;
            this.label13.Text = "min";
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(392, 282);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(216, 33);
            this.btnUpload.TabIndex = 3;
            this.btnUpload.Text = "&Upload";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // frmMain2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 321);
            this.Controls.Add(this.grpLocation);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.grpTime);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtDuration);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmMain2";
            this.Text = "Upload Agent";
            this.Load += new System.EventHandler(this.frmMain2_Load);
            this.Resize += new System.EventHandler(this.frmMain2_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.te1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.te2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.te3.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.te4.Properties)).EndInit();
            this.grpTime.ResumeLayout(false);
            this.grpTime.PerformLayout();
            this.grpLocation.ResumeLayout(false);
            this.grpLocation.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.TextBox txtRemoteFilePath;
        private System.Windows.Forms.TextBox txtLocalFilePath;
        private DevExpress.XtraEditors.TimeEdit te1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private DevExpress.XtraEditors.TimeEdit te2;
        private DevExpress.XtraEditors.TimeEdit te3;
        private DevExpress.XtraEditors.TimeEdit te4;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtDBPath;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtBkup;
        private System.Windows.Forms.GroupBox grpTime;
        private System.Windows.Forms.GroupBox grpLocation;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtDuration;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.TextBox txtSyncFilePath;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtDBPathELANCO;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtLocalFilePathELANCO;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtRemoteFilePathELANCO;
    }
}