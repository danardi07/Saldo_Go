using System;
using System.IO;
using System.Windows.Forms;

namespace SaldoGo
{
    public partial class FrmConfigDatabase : Form
    {
        private TextBox txtServerIP;
        private TextBox txtDatabaseName;
        private Button btnSave;
        private Button btnCancel;
        private Label lblServerIP;
        private Label lblDatabaseName;
        private Label lblTitle;

        public FrmConfigDatabase()
        {
            InitializeComponent();
            LoadConfig();
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lblServerIP = new Label();
            this.lblDatabaseName = new Label();
            this.txtServerIP = new TextBox();
            this.txtDatabaseName = new TextBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(100, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(200, 20);
            this.lblTitle.Text = "Konfigurasi Database";
            // 
            // lblServerIP
            // 
            this.lblServerIP.AutoSize = true;
            this.lblServerIP.Location = new System.Drawing.Point(30, 70);
            this.lblServerIP.Name = "lblServerIP";
            this.lblServerIP.Size = new System.Drawing.Size(80, 13);
            this.lblServerIP.Text = "Server IP:";
            // 
            // lblDatabaseName
            // 
            this.lblDatabaseName.AutoSize = true;
            this.lblDatabaseName.Location = new System.Drawing.Point(30, 110);
            this.lblDatabaseName.Name = "lblDatabaseName";
            this.lblDatabaseName.Size = new System.Drawing.Size(100, 13);
            this.lblDatabaseName.Text = "Database Name:";
            // 
            // txtServerIP
            // 
            this.txtServerIP.Location = new System.Drawing.Point(120, 67);
            this.txtServerIP.Name = "txtServerIP";
            this.txtServerIP.Size = new System.Drawing.Size(200, 20);
            this.txtServerIP.TabIndex = 0;
            // 
            // txtDatabaseName
            // 
            this.txtDatabaseName.Location = new System.Drawing.Point(140, 107);
            this.txtDatabaseName.Name = "txtDatabaseName";
            this.txtDatabaseName.Size = new System.Drawing.Size(180, 20);
            this.txtDatabaseName.TabIndex = 1;
            this.txtDatabaseName.Text = "Saldo_Go";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(80, 160);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Simpan";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(200, 160);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Batal";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // FrmConfigDatabase
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(400, 220);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtDatabaseName);
            this.Controls.Add(this.txtServerIP);
            this.Controls.Add(this.lblDatabaseName);
            this.Controls.Add(this.lblServerIP);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmConfigDatabase";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Konfigurasi Database";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadConfig()
        {
            try
            {
                string configPath = Path.Combine(Application.StartupPath, "database.config");
                if (File.Exists(configPath))
                {
                    string[] lines = File.ReadAllLines(configPath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("ServerIP="))
                        {
                            txtServerIP.Text = line.Substring("ServerIP=".Length);
                        }
                        else if (line.StartsWith("Database="))
                        {
                            txtDatabaseName.Text = line.Substring("Database=".Length);
                        }
                    }
                }
                else
                {
                    txtServerIP.Text = "localhost";
                    txtDatabaseName.Text = "Saldo_Go";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading config: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string serverIP = txtServerIP.Text.Trim();
                string databaseName = txtDatabaseName.Text.Trim();

                if (string.IsNullOrEmpty(serverIP))
                {
                    MessageBox.Show("Server IP tidak boleh kosong", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(databaseName))
                {
                    MessageBox.Show("Database Name tidak boleh kosong", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string configPath = Path.Combine(Application.StartupPath, "database.config");
                string[] configLines = new string[]
                {
                    "ServerIP=" + serverIP,
                    "Database=" + databaseName
                };

                File.WriteAllLines(configPath, configLines);

                MessageBox.Show("Konfigurasi berhasil disimpan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving config: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
