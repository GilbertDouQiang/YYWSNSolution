namespace PictureTransfer
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.lbStatus = new System.Windows.Forms.Label();
            this.lbTime = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbFileSize = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.pic1 = new System.Windows.Forms.PictureBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.txtOpenFile = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.cbCOMPort = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lbPacketCount = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnCheck = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnFormat = new System.Windows.Forms.Button();
            this.ExeStatus = new System.Windows.Forms.Label();
            this.btnPing = new System.Windows.Forms.Button();
            this.btnSleep = new System.Windows.Forms.Button();
            this.btnStatus = new System.Windows.Forms.Button();
            this.tbxBaudRate = new System.Windows.Forms.TextBox();
            this.labError = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(727, 379);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(128, 27);
            this.button1.TabIndex = 52;
            this.button1.Text = "测试";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDown2.Location = new System.Drawing.Point(314, 67);
            this.numericUpDown2.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown2.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(68, 21);
            this.numericUpDown2.TabIndex = 51;
            this.numericUpDown2.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(238, 71);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 12);
            this.label5.TabIndex = 50;
            this.label5.Text = "Timeout(ms)";
            // 
            // lbStatus
            // 
            this.lbStatus.AutoSize = true;
            this.lbStatus.Location = new System.Drawing.Point(725, 508);
            this.lbStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(77, 12);
            this.lbStatus.TabIndex = 49;
            this.lbStatus.Text = "Status: Idle";
            // 
            // lbTime
            // 
            this.lbTime.AutoSize = true;
            this.lbTime.Location = new System.Drawing.Point(725, 533);
            this.lbTime.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbTime.Name = "lbTime";
            this.lbTime.Size = new System.Drawing.Size(83, 12);
            this.lbTime.TabIndex = 48;
            this.lbTime.Text = "Time Span(s):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(68, 71);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 47;
            this.label3.Text = "BaudRate";
            // 
            // lbFileSize
            // 
            this.lbFileSize.AutoSize = true;
            this.lbFileSize.Location = new System.Drawing.Point(197, 137);
            this.lbFileSize.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbFileSize.Name = "lbFileSize";
            this.lbFileSize.Size = new System.Drawing.Size(71, 12);
            this.lbFileSize.TabIndex = 45;
            this.lbFileSize.Text = "File Size: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 134);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 44;
            this.label2.Text = "Packet Size";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericUpDown1.Location = new System.Drawing.Point(115, 133);
            this.numericUpDown1.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(68, 21);
            this.numericUpDown1.TabIndex = 43;
            this.numericUpDown1.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            // 
            // pic1
            // 
            this.pic1.Location = new System.Drawing.Point(57, 158);
            this.pic1.Margin = new System.Windows.Forms.Padding(2);
            this.pic1.Name = "pic1";
            this.pic1.Size = new System.Drawing.Size(388, 291);
            this.pic1.TabIndex = 42;
            this.pic1.TabStop = false;
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(530, 233);
            this.btnSend.Margin = new System.Windows.Forms.Padding(2);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(128, 27);
            this.btnSend.TabIndex = 41;
            this.btnSend.Text = "发送图片";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Location = new System.Drawing.Point(530, 99);
            this.btnOpenFile.Margin = new System.Windows.Forms.Padding(2);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(128, 27);
            this.btnOpenFile.TabIndex = 40;
            this.btnOpenFile.Text = "选择图片";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // txtOpenFile
            // 
            this.txtOpenFile.Location = new System.Drawing.Point(125, 99);
            this.txtOpenFile.Margin = new System.Windows.Forms.Padding(2);
            this.txtOpenFile.Name = "txtOpenFile";
            this.txtOpenFile.ReadOnly = true;
            this.txtOpenFile.Size = new System.Drawing.Size(356, 21);
            this.txtOpenFile.TabIndex = 39;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(62, 102);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 38;
            this.label4.Text = "File Name";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(530, 41);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(2);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(128, 28);
            this.btnRefresh.TabIndex = 37;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // cbCOMPort
            // 
            this.cbCOMPort.FormattingEnabled = true;
            this.cbCOMPort.Location = new System.Drawing.Point(125, 41);
            this.cbCOMPort.Margin = new System.Windows.Forms.Padding(2);
            this.cbCOMPort.Name = "cbCOMPort";
            this.cbCOMPort.Size = new System.Drawing.Size(356, 20);
            this.cbCOMPort.TabIndex = 35;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 43);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 34;
            this.label1.Text = "Serial Port";
            // 
            // lbPacketCount
            // 
            this.lbPacketCount.AutoSize = true;
            this.lbPacketCount.Location = new System.Drawing.Point(347, 137);
            this.lbPacketCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbPacketCount.Name = "lbPacketCount";
            this.lbPacketCount.Size = new System.Drawing.Size(89, 12);
            this.lbPacketCount.TabIndex = 53;
            this.lbPacketCount.Text = "Packet Count: ";
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(530, 158);
            this.btnApply.Margin = new System.Windows.Forms.Padding(2);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(128, 27);
            this.btnApply.TabIndex = 54;
            this.btnApply.Text = "申请发送";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnCheck
            // 
            this.btnCheck.Location = new System.Drawing.Point(530, 319);
            this.btnCheck.Margin = new System.Windows.Forms.Padding(2);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(128, 27);
            this.btnCheck.TabIndex = 55;
            this.btnCheck.Text = "校验图片";
            this.btnCheck.UseVisualStyleBackColor = true;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(530, 422);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(128, 27);
            this.btnCancel.TabIndex = 56;
            this.btnCancel.Text = "取消发送";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnFormat
            // 
            this.btnFormat.Location = new System.Drawing.Point(727, 422);
            this.btnFormat.Margin = new System.Windows.Forms.Padding(2);
            this.btnFormat.Name = "btnFormat";
            this.btnFormat.Size = new System.Drawing.Size(128, 27);
            this.btnFormat.TabIndex = 57;
            this.btnFormat.Text = "格式化";
            this.btnFormat.UseVisualStyleBackColor = true;
            this.btnFormat.Click += new System.EventHandler(this.btnFormat_Click);
            // 
            // ExeStatus
            // 
            this.ExeStatus.AutoSize = true;
            this.ExeStatus.Location = new System.Drawing.Point(55, 483);
            this.ExeStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ExeStatus.Name = "ExeStatus";
            this.ExeStatus.Size = new System.Drawing.Size(53, 12);
            this.ExeStatus.TabIndex = 58;
            this.ExeStatus.Text = "Status: ";
            // 
            // btnPing
            // 
            this.btnPing.Location = new System.Drawing.Point(727, 158);
            this.btnPing.Margin = new System.Windows.Forms.Padding(2);
            this.btnPing.Name = "btnPing";
            this.btnPing.Size = new System.Drawing.Size(128, 27);
            this.btnPing.TabIndex = 59;
            this.btnPing.Text = "Ping";
            this.btnPing.UseVisualStyleBackColor = true;
            this.btnPing.Click += new System.EventHandler(this.btnPing_Click);
            // 
            // btnSleep
            // 
            this.btnSleep.Location = new System.Drawing.Point(727, 215);
            this.btnSleep.Margin = new System.Windows.Forms.Padding(2);
            this.btnSleep.Name = "btnSleep";
            this.btnSleep.Size = new System.Drawing.Size(128, 27);
            this.btnSleep.TabIndex = 60;
            this.btnSleep.Text = "睡眠";
            this.btnSleep.UseVisualStyleBackColor = true;
            this.btnSleep.Click += new System.EventHandler(this.btnSleep_Click);
            // 
            // btnStatus
            // 
            this.btnStatus.Location = new System.Drawing.Point(727, 272);
            this.btnStatus.Margin = new System.Windows.Forms.Padding(2);
            this.btnStatus.Name = "btnStatus";
            this.btnStatus.Size = new System.Drawing.Size(128, 27);
            this.btnStatus.TabIndex = 61;
            this.btnStatus.Text = "状态";
            this.btnStatus.UseVisualStyleBackColor = true;
            this.btnStatus.Click += new System.EventHandler(this.btnStatus_Click);
            // 
            // tbxBaudRate
            // 
            this.tbxBaudRate.Location = new System.Drawing.Point(125, 68);
            this.tbxBaudRate.Margin = new System.Windows.Forms.Padding(2);
            this.tbxBaudRate.Name = "tbxBaudRate";
            this.tbxBaudRate.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.tbxBaudRate.Size = new System.Drawing.Size(98, 21);
            this.tbxBaudRate.TabIndex = 62;
            this.tbxBaudRate.Text = "115200";
            // 
            // labError
            // 
            this.labError.AutoSize = true;
            this.labError.Location = new System.Drawing.Point(55, 533);
            this.labError.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labError.Name = "labError";
            this.labError.Size = new System.Drawing.Size(47, 12);
            this.labError.TabIndex = 63;
            this.labError.Text = "Error: ";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(947, 585);
            this.Controls.Add(this.labError);
            this.Controls.Add(this.tbxBaudRate);
            this.Controls.Add(this.btnStatus);
            this.Controls.Add(this.btnSleep);
            this.Controls.Add(this.btnPing);
            this.Controls.Add(this.ExeStatus);
            this.Controls.Add(this.btnFormat);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCheck);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.lbPacketCount);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.numericUpDown2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lbStatus);
            this.Controls.Add(this.lbTime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lbFileSize);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.pic1);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnOpenFile);
            this.Controls.Add(this.txtOpenFile);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.cbCOMPort);
            this.Controls.Add(this.label1);
            this.Name = "FormMain";
            this.Text = "图片发送客户端";
            this.Load += new System.EventHandler(this.FormMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Label lbTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbFileSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.PictureBox pic1;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.TextBox txtOpenFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ComboBox cbCOMPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbPacketCount;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnFormat;
        private System.Windows.Forms.Label ExeStatus;
        private System.Windows.Forms.Button btnPing;
        private System.Windows.Forms.Button btnSleep;
        private System.Windows.Forms.Button btnStatus;
        private System.Windows.Forms.TextBox tbxBaudRate;
        private System.Windows.Forms.Label labError;
    }
}

