namespace SPIFFSUploader
{
  partial class frmSPIFFSUploader
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSPIFFSUploader));
      this.btnConnect = new System.Windows.Forms.Button();
      this.SPIFFS = new System.Windows.Forms.TreeView();
      this.imageList = new System.Windows.Forms.ImageList(this.components);
      this.trvDirectory = new System.Windows.Forms.TreeView();
      this.lblHostName = new System.Windows.Forms.Label();
      this.txtHostName = new System.Windows.Forms.TextBox();
      this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.txtPath = new System.Windows.Forms.TextBox();
      this.lblSPIFFS_Info = new System.Windows.Forms.Label();
      this.btnFormat = new System.Windows.Forms.Button();
      this.pbPercentage = new System.Windows.Forms.ProgressBar();
      this.lblStatus = new System.Windows.Forms.Label();
      this.picConnect = new System.Windows.Forms.PictureBox();
      this.cmbdrive = new MCExtensions.DriveCombo();
      ((System.ComponentModel.ISupportInitialize)(this.picConnect)).BeginInit();
      this.SuspendLayout();
      // 
      // btnConnect
      // 
      this.btnConnect.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnConnect.Location = new System.Drawing.Point(263, 14);
      this.btnConnect.Name = "btnConnect";
      this.btnConnect.Size = new System.Drawing.Size(87, 25);
      this.btnConnect.TabIndex = 2;
      this.btnConnect.Text = "Connect";
      this.btnConnect.UseVisualStyleBackColor = true;
      this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
      // 
      // SPIFFS
      // 
      this.SPIFFS.AllowDrop = true;
      this.SPIFFS.Anchor = System.Windows.Forms.AnchorStyles.Top;
      this.SPIFFS.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.SPIFFS.ImageIndex = 0;
      this.SPIFFS.ImageList = this.imageList;
      this.SPIFFS.Location = new System.Drawing.Point(580, 78);
      this.SPIFFS.Name = "SPIFFS";
      this.SPIFFS.SelectedImageIndex = 0;
      this.SPIFFS.Size = new System.Drawing.Size(558, 385);
      this.SPIFFS.TabIndex = 3;
      this.SPIFFS.DragDrop += new System.Windows.Forms.DragEventHandler(this.SPIFFS_DragDrop);
      this.SPIFFS.DragEnter += new System.Windows.Forms.DragEventHandler(this.SPIFFS_DragEnter);
      this.SPIFFS.DragOver += new System.Windows.Forms.DragEventHandler(this.SPIFFS_DragOver);
      this.SPIFFS.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SPIFFS_MouseDown);
      this.SPIFFS.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SPIFFS_MouseMove);
      // 
      // imageList
      // 
      this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
      this.imageList.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList.Images.SetKeyName(0, "Folder_Open.png");
      this.imageList.Images.SetKeyName(1, "Document_Text.png");
      // 
      // trvDirectory
      // 
      this.trvDirectory.AllowDrop = true;
      this.trvDirectory.Anchor = System.Windows.Forms.AnchorStyles.Top;
      this.trvDirectory.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.trvDirectory.ImageIndex = 0;
      this.trvDirectory.ImageList = this.imageList;
      this.trvDirectory.Location = new System.Drawing.Point(8, 78);
      this.trvDirectory.Name = "trvDirectory";
      this.trvDirectory.SelectedImageIndex = 0;
      this.trvDirectory.Size = new System.Drawing.Size(560, 385);
      this.trvDirectory.TabIndex = 4;
      this.trvDirectory.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.trvDirectory_AfterExpand);
      this.trvDirectory.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvDirectory_AfterSelect);
      this.trvDirectory.DragDrop += new System.Windows.Forms.DragEventHandler(this.trvDirectory_DragDrop);
      this.trvDirectory.DragEnter += new System.Windows.Forms.DragEventHandler(this.trvDirectory_DragEnter);
      this.trvDirectory.DragOver += new System.Windows.Forms.DragEventHandler(this.trvDirectory_DragOver);
      this.trvDirectory.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trvDirectory_MouseDown);
      this.trvDirectory.MouseMove += new System.Windows.Forms.MouseEventHandler(this.trvDirectory_MouseMove);
      // 
      // lblHostName
      // 
      this.lblHostName.AutoSize = true;
      this.lblHostName.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblHostName.Location = new System.Drawing.Point(12, 17);
      this.lblHostName.Name = "lblHostName";
      this.lblHostName.Size = new System.Drawing.Size(75, 16);
      this.lblHostName.TabIndex = 5;
      this.lblHostName.Text = "Host Name:";
      // 
      // txtHostName
      // 
      this.txtHostName.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtHostName.Location = new System.Drawing.Point(93, 15);
      this.txtHostName.Name = "txtHostName";
      this.txtHostName.Size = new System.Drawing.Size(145, 23);
      this.txtHostName.TabIndex = 6;
      this.txtHostName.Text = "192.168.0.25";
      this.txtHostName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtHostName_KeyDown);
      // 
      // txtPath
      // 
      this.txtPath.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.txtPath.Location = new System.Drawing.Point(221, 49);
      this.txtPath.Name = "txtPath";
      this.txtPath.Size = new System.Drawing.Size(347, 23);
      this.txtPath.TabIndex = 10;
      this.txtPath.TextChanged += new System.EventHandler(this.txtPath_TextChanged);
      this.txtPath.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPath_KeyPress);
      // 
      // lblSPIFFS_Info
      // 
      this.lblSPIFFS_Info.AutoSize = true;
      this.lblSPIFFS_Info.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblSPIFFS_Info.Location = new System.Drawing.Point(577, 52);
      this.lblSPIFFS_Info.Name = "lblSPIFFS_Info";
      this.lblSPIFFS_Info.Size = new System.Drawing.Size(95, 16);
      this.lblSPIFFS_Info.TabIndex = 11;
      this.lblSPIFFS_Info.Text = "Used Memory: ";
      // 
      // btnFormat
      // 
      this.btnFormat.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.btnFormat.Location = new System.Drawing.Point(1021, 49);
      this.btnFormat.Name = "btnFormat";
      this.btnFormat.Size = new System.Drawing.Size(117, 23);
      this.btnFormat.TabIndex = 12;
      this.btnFormat.Text = "Format SPIFFS";
      this.btnFormat.UseVisualStyleBackColor = true;
      this.btnFormat.Click += new System.EventHandler(this.btnFormat_Click);
      // 
      // pbPercentage
      // 
      this.pbPercentage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pbPercentage.Location = new System.Drawing.Point(8, 513);
      this.pbPercentage.Maximum = 1000;
      this.pbPercentage.Name = "pbPercentage";
      this.pbPercentage.Size = new System.Drawing.Size(1130, 23);
      this.pbPercentage.TabIndex = 13;
      // 
      // lblStatus
      // 
      this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblStatus.AutoSize = true;
      this.lblStatus.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblStatus.Location = new System.Drawing.Point(5, 494);
      this.lblStatus.Name = "lblStatus";
      this.lblStatus.Size = new System.Drawing.Size(44, 16);
      this.lblStatus.TabIndex = 14;
      this.lblStatus.Text = "Status";
      // 
      // picConnect
      // 
      this.picConnect.Location = new System.Drawing.Point(244, 20);
      this.picConnect.Name = "picConnect";
      this.picConnect.Size = new System.Drawing.Size(13, 13);
      this.picConnect.TabIndex = 15;
      this.picConnect.TabStop = false;
      // 
      // cmbdrive
      // 
      this.cmbdrive.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
      this.cmbdrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cmbdrive.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.cmbdrive.FormattingEnabled = true;
      this.cmbdrive.ItemHeight = 22;
      this.cmbdrive.Items.AddRange(new object[] {
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (J:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (J:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (J:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (M:)",
            " (W:)",
            "",
            " (C:)",
            " (D:)",
            " (E:)",
            " (F:)",
            " (G:)",
            " (M:)",
            " (W:)"});
      this.cmbdrive.Location = new System.Drawing.Point(8, 44);
      this.cmbdrive.Name = "cmbdrive";
      this.cmbdrive.Size = new System.Drawing.Size(207, 28);
      this.cmbdrive.TabIndex = 9;
      this.cmbdrive.SelectedIndexChanged += new System.EventHandler(this.cmbdrive_SelectedIndexChanged);
      // 
      // frmSPIFFSUploader
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1150, 537);
      this.Controls.Add(this.picConnect);
      this.Controls.Add(this.lblStatus);
      this.Controls.Add(this.pbPercentage);
      this.Controls.Add(this.btnFormat);
      this.Controls.Add(this.lblSPIFFS_Info);
      this.Controls.Add(this.txtPath);
      this.Controls.Add(this.cmbdrive);
      this.Controls.Add(this.txtHostName);
      this.Controls.Add(this.lblHostName);
      this.Controls.Add(this.trvDirectory);
      this.Controls.Add(this.SPIFFS);
      this.Controls.Add(this.btnConnect);
      this.Name = "frmSPIFFSUploader";
      this.Text = "ESP32/ESP8266 SPIFFS Uploader";
      this.Load += new System.EventHandler(this.frmSPIFFSUploader_Load);
      this.Resize += new System.EventHandler(this.frmSPIFFSUploader_Resize);
      ((System.ComponentModel.ISupportInitialize)(this.picConnect)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

        #endregion
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TreeView SPIFFS;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.TreeView trvDirectory;
        private System.Windows.Forms.Label lblHostName;
        private System.Windows.Forms.TextBox txtHostName;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    private MCExtensions.DriveCombo cmbdrive;
    private System.Windows.Forms.TextBox txtPath;
    private System.Windows.Forms.Label lblSPIFFS_Info;
    private System.Windows.Forms.Button btnFormat;
    private System.Windows.Forms.ProgressBar pbPercentage;
        private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.PictureBox picConnect;
  }
}

