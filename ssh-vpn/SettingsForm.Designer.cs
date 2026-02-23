namespace ssh_vpn
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.txt_ip = new System.Windows.Forms.TextBox();
            this.lblIp = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.txt_username = new System.Windows.Forms.TextBox();
            this.lblPass = new System.Windows.Forms.Label();
            this.txt_password = new System.Windows.Forms.TextBox();
            this.btn_save = new System.Windows.Forms.Button();
            this.txt_port = new System.Windows.Forms.NumericUpDown();
            this.lblPort = new System.Windows.Forms.Label();
            this.cmbLang = new System.Windows.Forms.ComboBox();
            this.lblLang = new System.Windows.Forms.Label();
            this.cmbTheme = new System.Windows.Forms.ComboBox();
            this.lblTheme = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.txt_port)).BeginInit();
            this.SuspendLayout();
            // 
            // txt_ip
            // 
            this.txt_ip.Location = new System.Drawing.Point(85, 12);
            this.txt_ip.Name = "txt_ip";
            this.txt_ip.Size = new System.Drawing.Size(187, 20);
            this.txt_ip.TabIndex = 0;
            // 
            // lblIp
            // 
            this.lblIp.AutoSize = true;
            this.lblIp.Location = new System.Drawing.Point(12, 15);
            this.lblIp.Name = "lblIp";
            this.lblIp.Size = new System.Drawing.Size(67, 13);
            this.lblIp.TabIndex = 1;
            this.lblIp.Text = "IP Address : ";
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(12, 68);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(64, 13);
            this.lblUser.TabIndex = 3;
            this.lblUser.Text = "Username : ";
            // 
            // txt_username
            // 
            this.txt_username.Location = new System.Drawing.Point(85, 65);
            this.txt_username.Name = "txt_username";
            this.txt_username.Size = new System.Drawing.Size(187, 20);
            this.txt_username.TabIndex = 2;
            // 
            // lblPass
            // 
            this.lblPass.AutoSize = true;
            this.lblPass.Location = new System.Drawing.Point(12, 94);
            this.lblPass.Name = "lblPass";
            this.lblPass.Size = new System.Drawing.Size(62, 13);
            this.lblPass.TabIndex = 5;
            this.lblPass.Text = "Password : ";
            // 
            // txt_password
            // 
            this.txt_password.Location = new System.Drawing.Point(85, 91);
            this.txt_password.Name = "txt_password";
            this.txt_password.Size = new System.Drawing.Size(187, 20);
            this.txt_password.TabIndex = 4;
            // 
            // btn_save
            // 
            this.btn_save.Location = new System.Drawing.Point(12, 185);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(260, 39);
            this.btn_save.TabIndex = 6;
            this.btn_save.Text = "Save";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // txt_port
            // 
            this.txt_port.Location = new System.Drawing.Point(85, 38);
            this.txt_port.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.txt_port.Name = "txt_port";
            this.txt_port.Size = new System.Drawing.Size(187, 20);
            this.txt_port.TabIndex = 9;
            this.txt_port.Value = new decimal(new int[] {
            22,
            0,
            0,
            0});
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(12, 42);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(65, 13);
            this.lblPort.TabIndex = 8;
            this.lblPort.Text = "Server port :";
            //
            // cmbLang
            //
            this.cmbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLang.FormattingEnabled = true;
            this.cmbLang.Items.AddRange(new object[] {
            "English",
            "Farsi"});
            this.cmbLang.Location = new System.Drawing.Point(85, 120);
            this.cmbLang.Name = "cmbLang";
            this.cmbLang.Size = new System.Drawing.Size(187, 21);
            this.cmbLang.TabIndex = 10;
            this.cmbLang.SelectedIndexChanged += new System.EventHandler(this.cmbLang_SelectedIndexChanged);
            //
            // lblLang
            //
            this.lblLang.AutoSize = true;
            this.lblLang.Location = new System.Drawing.Point(12, 123);
            this.lblLang.Name = "lblLang";
            this.lblLang.Size = new System.Drawing.Size(61, 13);
            this.lblLang.TabIndex = 11;
            this.lblLang.Text = "Language :";
            //
            // cmbTheme
            //
            this.cmbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTheme.FormattingEnabled = true;
            this.cmbTheme.Items.AddRange(new object[] {
            "Light",
            "Dark"});
            this.cmbTheme.Location = new System.Drawing.Point(85, 147);
            this.cmbTheme.Name = "cmbTheme";
            this.cmbTheme.Size = new System.Drawing.Size(187, 21);
            this.cmbTheme.TabIndex = 12;
            this.cmbTheme.SelectedIndexChanged += new System.EventHandler(this.cmbTheme_SelectedIndexChanged);
            //
            // lblTheme
            //
            this.lblTheme.AutoSize = true;
            this.lblTheme.Location = new System.Drawing.Point(12, 150);
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Size = new System.Drawing.Size(46, 13);
            this.lblTheme.TabIndex = 13;
            this.lblTheme.Text = "Theme :";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(286, 235);
            this.Controls.Add(this.lblTheme);
            this.Controls.Add(this.cmbTheme);
            this.Controls.Add(this.lblLang);
            this.Controls.Add(this.cmbLang);
            this.Controls.Add(this.txt_port);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.lblPass);
            this.Controls.Add(this.txt_password);
            this.Controls.Add(this.lblUser);
            this.Controls.Add(this.txt_username);
            this.Controls.Add(this.lblIp);
            this.Controls.Add(this.txt_ip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject(".Icon")));
            this.MaximizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txt_port)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_ip;
        private System.Windows.Forms.Label lblIp;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.TextBox txt_username;
        private System.Windows.Forms.Label lblPass;
        private System.Windows.Forms.TextBox txt_password;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.NumericUpDown txt_port;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.ComboBox cmbLang;
        private System.Windows.Forms.Label lblLang;
        private System.Windows.Forms.ComboBox cmbTheme;
        private System.Windows.Forms.Label lblTheme;
    }
}
