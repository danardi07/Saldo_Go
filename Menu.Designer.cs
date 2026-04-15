namespace SaldoGo
{
    partial class Menu
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnShow;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtUnit;
        private System.Windows.Forms.TextBox txtSellingPrice;
        private System.Windows.Forms.TextBox txtCost;
        private System.Windows.Forms.CheckBox chkActive;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;

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
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnShow = new System.Windows.Forms.Button();
            this.lblCount = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            this.txtId = new System.Windows.Forms.TextBox();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtUnit = new System.Windows.Forms.TextBox();
            this.txtSellingPrice = new System.Windows.Forms.TextBox();
            this.txtCost = new System.Windows.Forms.TextBox();
            this.chkActive = new System.Windows.Forms.CheckBox();
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
          
            this.txtSearch.Location = new System.Drawing.Point(12, 12);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(252, 22);
            this.txtSearch.TabIndex = 0;
            
            this.btnSearch.Location = new System.Drawing.Point(270, 10);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(90, 26);
            this.btnSearch.TabIndex = 1;
            this.btnSearch.Text = "Cari";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
          
            this.btnShow.Location = new System.Drawing.Point(366, 10);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(90, 26);
            this.btnShow.TabIndex = 2;
            this.btnShow.Text = "Tampilkan";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
            
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(470, 15);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(46, 16);
            this.lblCount.TabIndex = 3;
            this.lblCount.Text = "Total: -";
          
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Location = new System.Drawing.Point(12, 46);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RowHeadersWidth = 51;
            this.grid.RowTemplate.Height = 24;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(888, 280);
            this.grid.TabIndex = 4;
            this.grid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellClick);
            
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 344);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(22, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "ID";
           
            this.txtId.Location = new System.Drawing.Point(12, 363);
            this.txtId.Name = "txtId";
            this.txtId.ReadOnly = true;
            this.txtId.Size = new System.Drawing.Size(120, 22);
            this.txtId.TabIndex = 6;
            
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(156, 344);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 16);
            this.label2.TabIndex = 7;
            this.label2.Text = "Kategori";
            
            this.cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCategory.FormattingEnabled = true;
            this.cmbCategory.Location = new System.Drawing.Point(156, 362);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(220, 24);
            this.cmbCategory.TabIndex = 8;
           
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(396, 344);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 16);
            this.label3.TabIndex = 9;
            this.label3.Text = "Nama";
            
            this.txtName.Location = new System.Drawing.Point(396, 363);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(240, 22);
            this.txtName.TabIndex = 10;
            
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 399);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 16);
            this.label4.TabIndex = 11;
            this.label4.Text = "Satuan";
            
            this.txtUnit.Location = new System.Drawing.Point(12, 418);
            this.txtUnit.Name = "txtUnit";
            this.txtUnit.Size = new System.Drawing.Size(120, 22);
            this.txtUnit.TabIndex = 12;
            
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(156, 399);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 16);
            this.label5.TabIndex = 13;
            this.label5.Text = "Harga Jual";
           
            this.txtSellingPrice.Location = new System.Drawing.Point(156, 418);
            this.txtSellingPrice.Name = "txtSellingPrice";
            this.txtSellingPrice.Size = new System.Drawing.Size(220, 22);
            this.txtSellingPrice.TabIndex = 14;
            
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(396, 399);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 16);
            this.label6.TabIndex = 15;
            this.label6.Text = "Estimasi HPP";
            
            this.txtCost.Location = new System.Drawing.Point(396, 418);
            this.txtCost.Name = "txtCost";
            this.txtCost.Size = new System.Drawing.Size(240, 22);
            this.txtCost.TabIndex = 16;
            
            this.chkActive.AutoSize = true;
            this.chkActive.Checked = true;
            this.chkActive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkActive.Location = new System.Drawing.Point(660, 364);
            this.chkActive.Name = "chkActive";
            this.chkActive.Size = new System.Drawing.Size(57, 20);
            this.chkActive.TabIndex = 17;
            this.chkActive.Text = "Aktif";
            this.chkActive.UseVisualStyleBackColor = true;
            
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(657, 344);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 16);
            this.label7.TabIndex = 18;
            
            this.btnInsert.Location = new System.Drawing.Point(660, 408);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(100, 32);
            this.btnInsert.TabIndex = 19;
            this.btnInsert.Text = "Insert";
            this.btnInsert.UseVisualStyleBackColor = true;
            this.btnInsert.Click += new System.EventHandler(this.btnInsert_Click);
            
            this.btnUpdate.Location = new System.Drawing.Point(766, 408);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(100, 32);
            this.btnUpdate.TabIndex = 20;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            
            this.btnDelete.Location = new System.Drawing.Point(660, 446);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(100, 32);
            this.btnDelete.TabIndex = 21;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            
            this.btnClear.Location = new System.Drawing.Point(766, 446);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(100, 32);
            this.btnClear.TabIndex = 22;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(912, 495);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnInsert);
            this.Controls.Add(this.chkActive);
            this.Controls.Add(this.txtCost);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtSellingPrice);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtUnit);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtId);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.btnShow);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtSearch);
            this.Name = "Menu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Menu";
            this.Shown += new System.EventHandler(this.Menu_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
