using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SaldoGo
{
    public class StokBahan : Form
    {
        private readonly UserSession session;
        private readonly string connectionString = KoneksiDb.koneksi;

        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataReader reader;

        private DataGridView grid;
        private Label lblCount;

        private TextBox txtId;
        private TextBox txtName;
        private TextBox txtUnit;
        private TextBox txtStock;
        private CheckBox chkActive;
        private Button btnInsert;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnRefresh;

        private GroupBox grpBelanja;
        private ComboBox cmbBahan;
        private NumericUpDown numQty;
        private TextBox txtTotal;
        private ComboBox cmbKasSumber;
        private TextBox txtKet;
        private Button btnBelanja;

        public StokBahan() : this(null)
        {
        }

        public StokBahan(UserSession session)
        {
            this.session = session;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Label lblId;
            Label lblNama;
            Label lblSatuan;
            Label lblStok;
            Label lblBahan;
            Label lblQty;
            Label lblTotal;
            Label lblKasSumber;
            Label lblKet;

            this.SuspendLayout();

            this.Text = "Stok & Bahan";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(980, 560);
            this.Shown += new EventHandler(this.StokBahan_Shown);

            this.btnRefresh = new Button();
            this.btnRefresh.Location = new Point(12, 12);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(110, 28);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new EventHandler(this.btnRefresh_Click);
            this.Controls.Add(this.btnRefresh);

            this.lblCount = new Label();
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new Point(130, 17);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new Size(60, 16);
            this.lblCount.TabIndex = 1;
            this.lblCount.Text = "Total: -";
            this.Controls.Add(this.lblCount);

            this.grid = new DataGridView();
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.Location = new Point(12, 50);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new Size(940, 280);
            this.grid.TabIndex = 2;
            this.grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.CellClick += new DataGridViewCellEventHandler(this.Grid_CellClick);
            this.Controls.Add(this.grid);

            int y = 345;

            lblId = new Label();
            lblId.AutoSize = true;
            lblId.Location = new Point(12, y);
            lblId.Name = "lblId";
            lblId.Size = new Size(20, 16);
            lblId.TabIndex = 3;
            lblId.Text = "ID";
            this.Controls.Add(lblId);

            this.txtId = new TextBox();
            this.txtId.Location = new Point(12, y + 18);
            this.txtId.Name = "txtId";
            this.txtId.ReadOnly = true;
            this.txtId.Size = new Size(100, 22);
            this.txtId.TabIndex = 4;
            this.Controls.Add(this.txtId);

            lblNama = new Label();
            lblNama.AutoSize = true;
            lblNama.Location = new Point(125, y);
            lblNama.Name = "lblNama";
            lblNama.Size = new Size(44, 16);
            lblNama.TabIndex = 5;
            lblNama.Text = "Nama";
            this.Controls.Add(lblNama);

            this.txtName = new TextBox();
            this.txtName.Location = new Point(125, y + 18);
            this.txtName.Name = "txtName";
            this.txtName.Size = new Size(250, 22);
            this.txtName.TabIndex = 6;
            this.Controls.Add(this.txtName);

            lblSatuan = new Label();
            lblSatuan.AutoSize = true;
            lblSatuan.Location = new Point(390, y);
            lblSatuan.Name = "lblSatuan";
            lblSatuan.Size = new Size(50, 16);
            lblSatuan.TabIndex = 7;
            lblSatuan.Text = "Satuan";
            this.Controls.Add(lblSatuan);

            this.txtUnit = new TextBox();
            this.txtUnit.Location = new Point(390, y + 18);
            this.txtUnit.Name = "txtUnit";
            this.txtUnit.Size = new Size(120, 22);
            this.txtUnit.TabIndex = 8;
            this.Controls.Add(this.txtUnit);

            lblStok = new Label();
            lblStok.AutoSize = true;
            lblStok.Location = new Point(525, y);
            lblStok.Name = "lblStok";
            lblStok.Size = new Size(35, 16);
            lblStok.TabIndex = 9;
            lblStok.Text = "Stok";
            this.Controls.Add(lblStok);

            this.txtStock = new TextBox();
            this.txtStock.Location = new Point(525, y + 18);
            this.txtStock.Name = "txtStock";
            this.txtStock.ReadOnly = true;
            this.txtStock.Size = new Size(90, 22);
            this.txtStock.TabIndex = 10;
            this.txtStock.Text = "0";
            this.Controls.Add(this.txtStock);

            this.chkActive = new CheckBox();
            this.chkActive.AutoSize = true;
            this.chkActive.Location = new Point(630, y + 20);
            this.chkActive.Name = "chkActive";
            this.chkActive.Size = new Size(54, 20);
            this.chkActive.TabIndex = 11;
            this.chkActive.Text = "Aktif";
            this.chkActive.UseVisualStyleBackColor = true;
            this.chkActive.Checked = true;
            this.Controls.Add(this.chkActive);

            this.btnInsert = new Button();
            this.btnInsert.Location = new Point(700, y + 14);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new Size(80, 30);
            this.btnInsert.TabIndex = 12;
            this.btnInsert.Text = "Insert";
            this.btnInsert.UseVisualStyleBackColor = true;
            this.btnInsert.Click += new EventHandler(this.btnInsert_Click);
            this.Controls.Add(this.btnInsert);

            this.btnUpdate = new Button();
            this.btnUpdate.Location = new Point(785, y + 14);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new Size(80, 30);
            this.btnUpdate.TabIndex = 13;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new EventHandler(this.btnUpdate_Click);
            this.Controls.Add(this.btnUpdate);

            this.btnDelete = new Button();
            this.btnDelete.Location = new Point(870, y + 14);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new Size(80, 30);
            this.btnDelete.TabIndex = 14;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);
            this.Controls.Add(this.btnDelete);

            this.grpBelanja = new GroupBox();
            this.grpBelanja.Location = new Point(12, 405);
            this.grpBelanja.Name = "grpBelanja";
            this.grpBelanja.Size = new Size(940, 110);
            this.grpBelanja.TabIndex = 15;
            this.grpBelanja.TabStop = false;
            this.grpBelanja.Text = "Belanja Stok (otomatis PENGELUARAN)";
            this.Controls.Add(this.grpBelanja);

            lblBahan = new Label();
            lblBahan.AutoSize = true;
            lblBahan.Location = new Point(12, 25);
            lblBahan.Name = "lblBahan";
            lblBahan.Size = new Size(50, 16);
            lblBahan.TabIndex = 0;
            lblBahan.Text = "Bahan";
            this.grpBelanja.Controls.Add(lblBahan);

            this.cmbBahan = new ComboBox();
            this.cmbBahan.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbBahan.Location = new Point(12, 45);
            this.cmbBahan.Name = "cmbBahan";
            this.cmbBahan.Size = new Size(250, 24);
            this.cmbBahan.TabIndex = 1;
            this.grpBelanja.Controls.Add(this.cmbBahan);

            lblQty = new Label();
            lblQty.AutoSize = true;
            lblQty.Location = new Point(280, 25);
            lblQty.Name = "lblQty";
            lblQty.Size = new Size(28, 16);
            lblQty.TabIndex = 2;
            lblQty.Text = "Qty";
            this.grpBelanja.Controls.Add(lblQty);

            this.numQty = new NumericUpDown();
            this.numQty.DecimalPlaces = 2;
            this.numQty.Location = new Point(280, 45);
            this.numQty.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.numQty.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numQty.Name = "numQty";
            this.numQty.Size = new Size(90, 22);
            this.numQty.TabIndex = 3;
            this.numQty.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.grpBelanja.Controls.Add(this.numQty);

            lblTotal = new Label();
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(390, 25);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(40, 16);
            lblTotal.TabIndex = 4;
            lblTotal.Text = "Total";
            this.grpBelanja.Controls.Add(lblTotal);

            this.txtTotal = new TextBox();
            this.txtTotal.Location = new Point(390, 45);
            this.txtTotal.Name = "txtTotal";
            this.txtTotal.Size = new Size(140, 22);
            this.txtTotal.TabIndex = 5;
            this.grpBelanja.Controls.Add(this.txtTotal);

            lblKasSumber = new Label();
            lblKasSumber.AutoSize = true;
            lblKasSumber.Location = new Point(550, 25);
            lblKasSumber.Name = "lblKasSumber";
            lblKasSumber.Size = new Size(80, 16);
            lblKasSumber.TabIndex = 6;
            lblKasSumber.Text = "Kas Sumber";
            this.grpBelanja.Controls.Add(lblKasSumber);

            this.cmbKasSumber = new ComboBox();
            this.cmbKasSumber.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbKasSumber.Location = new Point(550, 45);
            this.cmbKasSumber.Name = "cmbKasSumber";
            this.cmbKasSumber.Size = new Size(220, 24);
            this.cmbKasSumber.TabIndex = 7;
            this.grpBelanja.Controls.Add(this.cmbKasSumber);

            lblKet = new Label();
            lblKet.AutoSize = true;
            lblKet.Location = new Point(12, 73);
            lblKet.Name = "lblKet";
            lblKet.Size = new Size(74, 16);
            lblKet.TabIndex = 8;
            lblKet.Text = "Keterangan";
            this.grpBelanja.Controls.Add(lblKet);

            this.txtKet = new TextBox();
            this.txtKet.Location = new Point(95, 70);
            this.txtKet.Name = "txtKet";
            this.txtKet.Size = new Size(675, 22);
            this.txtKet.TabIndex = 9;
            this.grpBelanja.Controls.Add(this.txtKet);

            this.btnBelanja = new Button();
            this.btnBelanja.Location = new Point(785, 43);
            this.btnBelanja.Name = "btnBelanja";
            this.btnBelanja.Size = new Size(140, 30);
            this.btnBelanja.TabIndex = 10;
            this.btnBelanja.Text = "Simpan Belanja";
            this.btnBelanja.UseVisualStyleBackColor = true;
            this.btnBelanja.Click += new EventHandler(this.btnBelanja_Click);
            this.grpBelanja.Controls.Add(this.btnBelanja);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadBahan();
            LoadBahanCombo();
            LoadKasSumber();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            InsertBahan();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateBahan();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteBahan();
        }

        private void btnBelanja_Click(object sender, EventArgs e)
        {
            SaveBelanja();
        }

        private void Koneksi()
        {
            conn = new SqlConnection(connectionString);
        }

        private void StokBahan_Shown(object sender, EventArgs e)
        {
            if (session == null)
            {
                MessageBox.Show("Session kosong.");
                Close();
                return;
            }

            if (!session.IsOwner)
            {
                MessageBox.Show("Akses ditolak: hanya Pemilik yang boleh mengelola stok.");
                Close();
                return;
            }

            EnsureSchema();
            LoadBahan();
            LoadBahanCombo();
            LoadKasSumber();
        }

        private void EnsureSchema()
        {
            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureAkunKasSaldoColumn(conn);
                DbSchema.EnsureAkunKasKategoriColumn(conn);
                DbSchema.EnsureStokTables(conn);
                conn.Close();
            }
            catch
            {
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void LoadKasSumber()
        {
            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureAkunKasSaldoColumn(conn);
                DbSchema.EnsureAkunKasKategoriColumn(conn);

                string sql = "SELECT id, nama + ' [' + ISNULL(kategori_kas,'') + '/' + jenis_kas + '] - Saldo: ' + CAST(saldo AS NVARCHAR(50)) AS display_name FROM AkunKas WHERE aktif=1 ORDER BY nama";
                cmd = new SqlCommand(sql, conn);
                reader = cmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Columns.Add("id", typeof(long));
                dt.Columns.Add("display_name", typeof(string));

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();
                    row["id"] = Convert.ToInt64(reader["id"]);
                    row["display_name"] = Convert.ToString(reader["display_name"]);
                    dt.Rows.Add(row);
                }

                reader.Close();
                conn.Close();

                cmbKasSumber.DataSource = dt;
                cmbKasSumber.DisplayMember = "display_name";
                cmbKasSumber.ValueMember = "id";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (reader != null && !reader.IsClosed) reader.Close(); } catch { }
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void LoadBahanCombo()
        {
            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureStokTables(conn);

                string sql = "SELECT id, nama + ' (' + satuan + ')' AS display_name FROM Bahan WHERE aktif=1 ORDER BY nama";
                cmd = new SqlCommand(sql, conn);
                reader = cmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Columns.Add("id", typeof(long));
                dt.Columns.Add("display_name", typeof(string));

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();
                    row["id"] = Convert.ToInt64(reader["id"]);
                    row["display_name"] = Convert.ToString(reader["display_name"]);
                    dt.Rows.Add(row);
                }

                reader.Close();
                conn.Close();

                cmbBahan.DataSource = dt;
                cmbBahan.DisplayMember = "display_name";
                cmbBahan.ValueMember = "id";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (reader != null && !reader.IsClosed) reader.Close(); } catch { }
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void LoadBahan()
        {
            try
            {
                grid.Columns.Clear();
                grid.Rows.Clear();

                grid.Columns.Add("id", "ID");
                grid.Columns.Add("nama", "Nama");
                grid.Columns.Add("satuan", "Satuan");
                grid.Columns.Add("stok", "Stok");

                DataGridViewCheckBoxColumn colAktif = new DataGridViewCheckBoxColumn();
                colAktif.Name = "aktif";
                colAktif.HeaderText = "Aktif";
                grid.Columns.Add(colAktif);

                grid.Columns["id"].Visible = false;

                Koneksi();
                conn.Open();

                DbSchema.EnsureStokTables(conn);

                string sql = "SELECT id, nama, satuan, stok, aktif FROM Bahan ORDER BY id DESC";
                cmd = new SqlCommand(sql, conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    grid.Rows.Add(reader["id"], reader["nama"], reader["satuan"], reader["stok"], reader["aktif"]);
                }
                reader.Close();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Bahan", conn);
                lblCount.Text = "Total: " + Convert.ToInt32(cmd.ExecuteScalar());

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (reader != null && !reader.IsClosed) reader.Close(); } catch { }
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = grid.Rows[e.RowIndex];

            txtId.Text = Convert.ToString(row.Cells["id"].Value);
            txtName.Text = Convert.ToString(row.Cells["nama"].Value);
            txtUnit.Text = Convert.ToString(row.Cells["satuan"].Value);
            txtStock.Text = Convert.ToString(row.Cells["stok"].Value);
            chkActive.Checked = Convert.ToBoolean(row.Cells["aktif"].Value);
        }

        private bool ValidateBahanInput()
        {
            if (txtName.Text.Trim() == "")
            {
                MessageBox.Show("Nama bahan wajib diisi.");
                txtName.Focus();
                return false;
            }

            if (txtUnit.Text.Trim() == "")
            {
                MessageBox.Show("Satuan wajib diisi.");
                txtUnit.Focus();
                return false;
            }

            return true;
        }

        private void InsertBahan()
        {
            if (!ValidateBahanInput()) return;

            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureStokTables(conn);

                string sql = "INSERT INTO Bahan(nama, satuan, stok, aktif) VALUES (@nama, @satuan, 0, @aktif)";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nama", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@satuan", txtUnit.Text.Trim());
                cmd.Parameters.AddWithValue("@aktif", chkActive.Checked ? 1 : 0);

                int rows = cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Berhasil insert bahan: " + rows + " baris.");
                ClearBahanInput();
                LoadBahan();
                LoadBahanCombo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void UpdateBahan()
        {
            if (txtId.Text.Trim() == "")
            {
                MessageBox.Show("Pilih bahan yang akan diupdate.");
                return;
            }

            if (!ValidateBahanInput()) return;

            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureStokTables(conn);

                string sql = "UPDATE Bahan SET nama=@nama, satuan=@satuan, aktif=@aktif WHERE id=@id";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", txtId.Text.Trim());
                cmd.Parameters.AddWithValue("@nama", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@satuan", txtUnit.Text.Trim());
                cmd.Parameters.AddWithValue("@aktif", chkActive.Checked ? 1 : 0);

                int rows = cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Berhasil update bahan: " + rows + " baris.");
                LoadBahan();
                LoadBahanCombo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void DeleteBahan()
        {
            if (txtId.Text.Trim() == "")
            {
                MessageBox.Show("Pilih bahan yang akan dihapus.");
                return;
            }

            DialogResult confirm = MessageBox.Show("Yakin hapus bahan ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureStokTables(conn);


                cmd = new SqlCommand("SELECT COUNT(*) FROM MutasiStok WHERE bahan_id=@id", conn);
                cmd.Parameters.AddWithValue("@id", txtId.Text.Trim());
                int cnt = Convert.ToInt32(cmd.ExecuteScalar());
                if (cnt > 0)
                {
                    conn.Close();
                    MessageBox.Show("Tidak bisa menghapus: bahan sudah dipakai di mutasi stok.");
                    return;
                }

                cmd = new SqlCommand("DELETE FROM Bahan WHERE id=@id", conn);
                cmd.Parameters.AddWithValue("@id", txtId.Text.Trim());
                int rows = cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Berhasil delete bahan: " + rows + " baris.");
                ClearBahanInput();
                LoadBahan();
                LoadBahanCombo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void ClearBahanInput()
        {
            txtId.Text = "";
            txtName.Text = "";
            txtUnit.Text = "";
            txtStock.Text = "0";
            chkActive.Checked = true;
            txtName.Focus();
        }

        private bool ValidateBelanja(out decimal total)
        {
            total = 0m;

            if (cmbBahan.SelectedValue == null)
            {
                MessageBox.Show("Pilih bahan yang dibeli.");
                return false;
            }

            if (cmbKasSumber.SelectedValue == null)
            {
                MessageBox.Show("Pilih kas sumber untuk belanja.");
                return false;
            }

            if (!decimal.TryParse(txtTotal.Text, out total) || total <= 0)
            {
                MessageBox.Show("Total belanja harus angka dan > 0.");
                txtTotal.Focus();
                return false;
            }

            return true;
        }

        private void SaveBelanja()
        {
            decimal total;
            if (!ValidateBelanja(out total)) return;

            long bahanId = Convert.ToInt64(cmbBahan.SelectedValue);
            decimal qty = Convert.ToDecimal(numQty.Value);
            long kasSumberId = Convert.ToInt64(cmbKasSumber.SelectedValue);

            string ket = txtKet.Text.Trim();
            if (ket == "") ket = "Belanja stok";

            try
            {
                Koneksi();
                conn.Open();

                SqlTransaction tx = conn.BeginTransaction();
                try
                {
                    DbSchema.EnsureStokTables(conn, tx);
                    DbSchema.EnsureAkunKasSaldoColumn(conn, tx);
                    DbSchema.EnsureAkunKasKategoriColumn(conn, tx);


                    string sqlMutasi = @"
INSERT INTO MutasiStok(waktu, tipe, bahan_id, qty, total_biaya, keterangan, dibuat_oleh_pengguna_id)
VALUES (SYSDATETIME(), N'MASUK', @bahan, @qty, @total, @ket, @userId)";
                    cmd = new SqlCommand(sqlMutasi, conn, tx);
                    cmd.Parameters.AddWithValue("@bahan", bahanId);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@total", total);
                    cmd.Parameters.AddWithValue("@ket", ket);
                    cmd.Parameters.AddWithValue("@userId", session.UserId);
                    cmd.ExecuteNonQuery();


                    cmd = new SqlCommand("UPDATE Bahan SET stok = stok + @qty WHERE id=@id", conn, tx);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@id", bahanId);
                    cmd.ExecuteNonQuery();


                    string sqlTrx = @"
INSERT INTO Transaksi(waktu_transaksi, tipe_transaksi, nominal, keterangan, akun_kas_sumber_id, akun_kas_tujuan_id, dibuat_oleh_pengguna_id)
VALUES (SYSDATETIME(), N'PENGELUARAN', @amount, @desc, @source, NULL, @userId)";
                    cmd = new SqlCommand(sqlTrx, conn, tx);
                    cmd.Parameters.AddWithValue("@amount", total);
                    cmd.Parameters.AddWithValue("@desc", "Belanja Stok: " + ket);
                    cmd.Parameters.AddWithValue("@source", kasSumberId);
                    cmd.Parameters.AddWithValue("@userId", session.UserId);
                    cmd.ExecuteNonQuery();


                    cmd = new SqlCommand("UPDATE AkunKas SET saldo = saldo - @amount WHERE id=@id", conn, tx);
                    cmd.Parameters.AddWithValue("@amount", total);
                    cmd.Parameters.AddWithValue("@id", kasSumberId);
                    cmd.ExecuteNonQuery();

                    tx.Commit();
                    conn.Close();

                    MessageBox.Show("Belanja stok berhasil disimpan.");
                    txtTotal.Text = "";
                    txtKet.Text = "";
                    numQty.Value = 1;

                    LoadBahan();
                    LoadKasSumber();
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (conn != null) conn.Close(); } catch { }
            }
        }
    }
}
