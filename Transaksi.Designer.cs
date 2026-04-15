namespace SaldoGo
{
    partial class Transaksi
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.ComboBox cmbSourceCash;
        private System.Windows.Forms.TextBox txtMenu;
        private System.Windows.Forms.NumericUpDown numQty;
        private System.Windows.Forms.TextBox txtAmount;
        private System.Windows.Forms.TextBox txtDesc;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnShow;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;

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
            this.cmbSourceCash = new System.Windows.Forms.ComboBox();
            this.txtMenu = new System.Windows.Forms.TextBox();
            this.numQty = new System.Windows.Forms.NumericUpDown();
            this.txtAmount = new System.Windows.Forms.TextBox();
            this.txtDesc = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnShow = new System.Windows.Forms.Button();
            this.lblCount = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numQty)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            this.cmbSourceCash.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSourceCash.FormattingEnabled = true;
            this.cmbSourceCash.Location = new System.Drawing.Point(15, 32);
            this.cmbSourceCash.Name = "cmbSourceCash";
            this.cmbSourceCash.Size = new System.Drawing.Size(150, 24);
            this.cmbSourceCash.TabIndex = 1;
            this.txtMenu.Location = new System.Drawing.Point(183, 32);
            this.txtMenu.Name = "txtMenu";
            this.txtMenu.ReadOnly = true;
            this.txtMenu.Size = new System.Drawing.Size(260, 22);
            this.txtMenu.TabIndex = 3;
            this.numQty.Location = new System.Drawing.Point(462, 32);
            this.numQty.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numQty.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numQty.Name = "numQty";
            this.numQty.Size = new System.Drawing.Size(70, 22);
            this.numQty.TabIndex = 5;
            this.numQty.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtAmount.Location = new System.Drawing.Point(550, 32);
            this.txtAmount.Name = "txtAmount";
            this.txtAmount.ReadOnly = true;
            this.txtAmount.Size = new System.Drawing.Size(120, 22);
            this.txtAmount.TabIndex = 7;
            this.txtDesc.Location = new System.Drawing.Point(692, 32);
            this.txtDesc.Name = "txtDesc";
            this.txtDesc.Size = new System.Drawing.Size(195, 22);
            this.txtDesc.TabIndex = 9;
            this.btnSave.Location = new System.Drawing.Point(787, 66);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "Bayar";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnShow.Location = new System.Drawing.Point(15, 68);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(100, 28);
            this.btnShow.TabIndex = 10;
            this.btnShow.Text = "Refresh";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(132, 74);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(78, 16);
            this.lblCount.TabIndex = 11;
            this.lblCount.Text = "Total menu: -";
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Location = new System.Drawing.Point(15, 110);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RowHeadersWidth = 51;
            this.grid.RowTemplate.Height = 24;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(872, 340);
            this.grid.TabIndex = 13;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Pembayaran";
            this.label1.Click += new System.EventHandler(this.label_Click);
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(180, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 16);
            this.label4.TabIndex = 2;
            this.label4.Text = "Menu";
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(547, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Total";
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(459, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 16);
            this.label5.TabIndex = 4;
            this.label5.Text = "Qty";
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(689, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 16);
            this.label3.TabIndex = 8;
            this.label3.Text = "Catatan";
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 470);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.btnShow);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtDesc);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtAmount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numQty);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtMenu);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbSourceCash);
            this.Controls.Add(this.label1);
            this.Name = "Transaksi";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kasir";
            this.Shown += new System.EventHandler(this.Transaksi_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.numQty)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
