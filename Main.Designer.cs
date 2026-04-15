namespace SaldoGo
{
    partial class Main
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblLoginInfo;
        private System.Windows.Forms.Label lblNote;
        private System.Windows.Forms.Button btnExpense;
        private System.Windows.Forms.Button btnRiwayat;
        private System.Windows.Forms.Button btnMenu;
        private System.Windows.Forms.Button btnStock;
        private System.Windows.Forms.Button btnTarget;
        private System.Windows.Forms.Button btnKas;
        private System.Windows.Forms.Button btnTransfer;
        private System.Windows.Forms.Button btnMargin;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblLoginInfo = new System.Windows.Forms.Label();
            this.lblNote = new System.Windows.Forms.Label();
            this.btnExpense = new System.Windows.Forms.Button();
            this.btnRiwayat = new System.Windows.Forms.Button();
            this.btnMenu = new System.Windows.Forms.Button();
            this.btnStock = new System.Windows.Forms.Button();
            this.btnTarget = new System.Windows.Forms.Button();
            this.btnKas = new System.Windows.Forms.Button();
            this.btnTransfer = new System.Windows.Forms.Button();
            this.btnMargin = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.lblLoginInfo.AutoSize = true;
            this.lblLoginInfo.Location = new System.Drawing.Point(20, 18);
            this.lblLoginInfo.Name = "lblLoginInfo";
            this.lblLoginInfo.Size = new System.Drawing.Size(50, 16);
            this.lblLoginInfo.TabIndex = 0;
            this.lblLoginInfo.Text = "Login: -";
            this.lblNote.AutoSize = true;
            this.lblNote.Location = new System.Drawing.Point(20, 44);
            this.lblNote.Name = "lblNote";
            this.lblNote.Size = new System.Drawing.Size(0, 16);
            this.lblNote.TabIndex = 1;
            this.btnExpense.Location = new System.Drawing.Point(23, 86);
            this.btnExpense.Name = "btnExpense";
            this.btnExpense.Size = new System.Drawing.Size(180, 40);
            this.btnExpense.TabIndex = 2;
            this.btnExpense.Text = "Kasir";
            this.btnExpense.UseVisualStyleBackColor = true;
            this.btnExpense.Click += new System.EventHandler(this.btnExpense_Click);

            this.btnRiwayat.Location = new System.Drawing.Point(23, 140);
            this.btnRiwayat.Name = "btnRiwayat";
            this.btnRiwayat.Size = new System.Drawing.Size(180, 40);
            this.btnRiwayat.TabIndex = 3;
            this.btnRiwayat.Text = "Riwayat Bon & Hutang";
            this.btnRiwayat.UseVisualStyleBackColor = true;
            this.btnRiwayat.Click += new System.EventHandler(this.btnRiwayat_Click);
            this.btnMenu.Location = new System.Drawing.Point(23, 194);
            this.btnMenu.Name = "btnMenu";
            this.btnMenu.Size = new System.Drawing.Size(180, 40);
            this.btnMenu.TabIndex = 4;
            this.btnMenu.Text = "Menu";
            this.btnMenu.UseVisualStyleBackColor = true;
            this.btnMenu.Click += new System.EventHandler(this.btnMenu_Click);

            this.btnStock.Location = new System.Drawing.Point(23, 248);
            this.btnStock.Name = "btnStock";
            this.btnStock.Size = new System.Drawing.Size(180, 40);
            this.btnStock.TabIndex = 5;
            this.btnStock.Text = "Stok & Bahan";
            this.btnStock.UseVisualStyleBackColor = true;
            this.btnStock.Click += new System.EventHandler(this.btnStock_Click);

            this.btnTarget.Location = new System.Drawing.Point(23, 302);
            this.btnTarget.Name = "btnTarget";
            this.btnTarget.Size = new System.Drawing.Size(180, 40);
            this.btnTarget.TabIndex = 6;
            this.btnTarget.Text = "Target Omzet Harian";
            this.btnTarget.UseVisualStyleBackColor = true;
            this.btnTarget.Click += new System.EventHandler(this.btnTarget_Click);
            this.btnKas.Location = new System.Drawing.Point(23, 356);
            this.btnKas.Name = "btnKas";
            this.btnKas.Size = new System.Drawing.Size(180, 40);
            this.btnKas.TabIndex = 7;
            this.btnKas.Text = "Akun Kas";
            this.btnKas.UseVisualStyleBackColor = true;
            this.btnKas.Click += new System.EventHandler(this.btnKas_Click);
            this.btnTransfer.Location = new System.Drawing.Point(23, 410);
            this.btnTransfer.Name = "btnTransfer";
            this.btnTransfer.Size = new System.Drawing.Size(180, 40);
            this.btnTransfer.TabIndex = 8;
            this.btnTransfer.Text = "Transfer";
            this.btnTransfer.UseVisualStyleBackColor = true;
            this.btnTransfer.Click += new System.EventHandler(this.btnTransfer_Click);

            this.btnMargin.Location = new System.Drawing.Point(23, 464);
            this.btnMargin.Name = "btnMargin";
            this.btnMargin.Size = new System.Drawing.Size(180, 40);
            this.btnMargin.TabIndex = 9;
            this.btnMargin.Text = "Laporan Margin";
            this.btnMargin.UseVisualStyleBackColor = true;
            this.btnMargin.Click += new System.EventHandler(this.btnMargin_Click);
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 530);
            this.Controls.Add(this.btnMargin);
            this.Controls.Add(this.btnTransfer);
            this.Controls.Add(this.btnKas);
            this.Controls.Add(this.btnTarget);
            this.Controls.Add(this.btnStock);
            this.Controls.Add(this.btnMenu);
            this.Controls.Add(this.btnRiwayat);
            this.Controls.Add(this.btnExpense);
            this.Controls.Add(this.lblNote);
            this.Controls.Add(this.lblLoginInfo);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Main";
            this.Load += new System.EventHandler(this.Main_Load);
            this.Shown += new System.EventHandler(this.Main_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
