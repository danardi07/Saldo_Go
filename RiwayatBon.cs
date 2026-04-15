using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SaldoGo
{
    public class RiwayatBon : Form
    {
        private readonly UserSession session;
        private readonly string connectionString = KoneksiDb.koneksi;

        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataReader reader;

        private ComboBox cmbStatus;
        private TextBox txtCari;
        private Button btnRefresh;

        private DataGridView gridBon;

        private TextBox txtCustomer;
        private TextBox txtHutangAmount;
        private TextBox txtHutangNote;
        private DateTimePicker dtDue;
        private CheckBox chkDue;
        private Button btnAddHutang;

        private TextBox txtBayar;
        private ComboBox cmbPayMethod;
        private ComboBox cmbStatusBayar;
        private Button btnBayar;

        public RiwayatBon() : this(null)
        {
        }

        public RiwayatBon(UserSession session)
        {
            this.session = session;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Label lblStatus;
            Label lblCari;
            GroupBox grpAdd;
            Label lblPelanggan;
            Label lblNominal;
            Label lblKetAdd;
            GroupBox grpPay;
            Label lblBayarNominal;
            Label lblBayarVia;
            Label lblStatusBayar;

            this.SuspendLayout();

            this.Text = "Bon (Hutang Pelanggan)";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(1060, 640);
            this.Shown += new EventHandler(this.RiwayatBon_Shown);

            lblStatus = new Label();
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 16);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(45, 16);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "Status";
            this.Controls.Add(lblStatus);

            this.cmbStatus = new ComboBox();
            this.cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbStatus.Location = new Point(60, 12);
            this.cmbStatus.Name = "cmbStatus";
            this.cmbStatus.Size = new Size(140, 24);
            this.cmbStatus.TabIndex = 1;
            this.cmbStatus.Items.AddRange(new object[] { "ALL", "BELUM_LUNAS", "LUNAS" });
            this.cmbStatus.SelectedIndex = 1;
            this.Controls.Add(this.cmbStatus);

            lblCari = new Label();
            lblCari.AutoSize = true;
            lblCari.Location = new Point(220, 16);
            lblCari.Name = "lblCari";
            lblCari.Size = new Size(30, 16);
            lblCari.TabIndex = 2;
            lblCari.Text = "Cari";
            this.Controls.Add(lblCari);

            this.txtCari = new TextBox();
            this.txtCari.Location = new Point(260, 12);
            this.txtCari.Name = "txtCari";
            this.txtCari.Size = new Size(240, 22);
            this.txtCari.TabIndex = 3;
            this.Controls.Add(this.txtCari);

            this.btnRefresh = new Button();
            this.btnRefresh.Location = new Point(515, 10);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(110, 28);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new EventHandler(this.btnRefresh_Click);
            this.Controls.Add(this.btnRefresh);

            this.gridBon = new DataGridView();
            this.gridBon.AllowUserToAddRows = false;
            this.gridBon.AllowUserToDeleteRows = false;
            this.gridBon.Location = new Point(12, 50);
            this.gridBon.MultiSelect = false;
            this.gridBon.Name = "gridBon";
            this.gridBon.ReadOnly = true;
            this.gridBon.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.gridBon.Size = new Size(1015, 360);
            this.gridBon.TabIndex = 5;
            this.gridBon.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.gridBon.CellClick += new DataGridViewCellEventHandler(this.gridBon_CellClick);
            this.Controls.Add(this.gridBon);

            grpAdd = new GroupBox();
            grpAdd.Location = new Point(12, 420);
            grpAdd.Name = "grpAdd";
            grpAdd.Size = new Size(1015, 90);
            grpAdd.TabIndex = 6;
            grpAdd.TabStop = false;
            grpAdd.Text = "Tambah Bon/Hutang";
            this.Controls.Add(grpAdd);

            lblPelanggan = new Label();
            lblPelanggan.AutoSize = true;
            lblPelanggan.Location = new Point(12, 25);
            lblPelanggan.Name = "lblPelanggan";
            lblPelanggan.Size = new Size(70, 16);
            lblPelanggan.TabIndex = 0;
            lblPelanggan.Text = "Pelanggan";
            grpAdd.Controls.Add(lblPelanggan);

            this.txtCustomer = new TextBox();
            this.txtCustomer.Location = new Point(90, 22);
            this.txtCustomer.Name = "txtCustomer";
            this.txtCustomer.Size = new Size(200, 22);
            this.txtCustomer.TabIndex = 1;
            grpAdd.Controls.Add(this.txtCustomer);

            lblNominal = new Label();
            lblNominal.AutoSize = true;
            lblNominal.Location = new Point(305, 25);
            lblNominal.Name = "lblNominal";
            lblNominal.Size = new Size(55, 16);
            lblNominal.TabIndex = 2;
            lblNominal.Text = "Nominal";
            grpAdd.Controls.Add(lblNominal);

            this.txtHutangAmount = new TextBox();
            this.txtHutangAmount.Location = new Point(365, 22);
            this.txtHutangAmount.Name = "txtHutangAmount";
            this.txtHutangAmount.Size = new Size(120, 22);
            this.txtHutangAmount.TabIndex = 3;
            grpAdd.Controls.Add(this.txtHutangAmount);

            lblKetAdd = new Label();
            lblKetAdd.AutoSize = true;
            lblKetAdd.Location = new Point(500, 25);
            lblKetAdd.Name = "lblKetAdd";
            lblKetAdd.Size = new Size(75, 16);
            lblKetAdd.TabIndex = 4;
            lblKetAdd.Text = "Keterangan";
            grpAdd.Controls.Add(lblKetAdd);

            this.txtHutangNote = new TextBox();
            this.txtHutangNote.Location = new Point(575, 22);
            this.txtHutangNote.Name = "txtHutangNote";
            this.txtHutangNote.Size = new Size(280, 22);
            this.txtHutangNote.TabIndex = 5;
            grpAdd.Controls.Add(this.txtHutangNote);

            this.chkDue = new CheckBox();
            this.chkDue.AutoSize = true;
            this.chkDue.Location = new Point(12, 55);
            this.chkDue.Name = "chkDue";
            this.chkDue.Size = new Size(96, 20);
            this.chkDue.TabIndex = 6;
            this.chkDue.Text = "Jatuh Tempo";
            this.chkDue.UseVisualStyleBackColor = true;
            this.chkDue.CheckedChanged += new EventHandler(this.chkDue_CheckedChanged);
            grpAdd.Controls.Add(this.chkDue);

            this.dtDue = new DateTimePicker();
            this.dtDue.Enabled = false;
            this.dtDue.Format = DateTimePickerFormat.Short;
            this.dtDue.Location = new Point(130, 52);
            this.dtDue.Name = "dtDue";
            this.dtDue.Size = new Size(130, 22);
            this.dtDue.TabIndex = 7;
            grpAdd.Controls.Add(this.dtDue);

            this.btnAddHutang = new Button();
            this.btnAddHutang.Location = new Point(870, 22);
            this.btnAddHutang.Name = "btnAddHutang";
            this.btnAddHutang.Size = new Size(120, 28);
            this.btnAddHutang.TabIndex = 8;
            this.btnAddHutang.Text = "Tambah";
            this.btnAddHutang.UseVisualStyleBackColor = true;
            this.btnAddHutang.Click += new EventHandler(this.btnAddHutang_Click);
            grpAdd.Controls.Add(this.btnAddHutang);

            grpPay = new GroupBox();
            grpPay.Location = new Point(12, 515);
            grpPay.Name = "grpPay";
            grpPay.Size = new Size(1015, 80);
            grpPay.TabIndex = 7;
            grpPay.TabStop = false;
            grpPay.Text = "Pembayaran Bon (Bisa Sebagian)";
            this.Controls.Add(grpPay);

            lblBayarNominal = new Label();
            lblBayarNominal.AutoSize = true;
            lblBayarNominal.Location = new Point(12, 32);
            lblBayarNominal.Name = "lblBayarNominal";
            lblBayarNominal.Size = new Size(95, 16);
            lblBayarNominal.TabIndex = 0;
            lblBayarNominal.Text = "Bayar Nominal";
            grpPay.Controls.Add(lblBayarNominal);

            this.txtBayar = new TextBox();
            this.txtBayar.Location = new Point(120, 28);
            this.txtBayar.Name = "txtBayar";
            this.txtBayar.Size = new Size(120, 22);
            this.txtBayar.TabIndex = 1;
            grpPay.Controls.Add(this.txtBayar);

            lblBayarVia = new Label();
            lblBayarVia.AutoSize = true;
            lblBayarVia.Location = new Point(260, 32);
            lblBayarVia.Name = "lblBayarVia";
            lblBayarVia.Size = new Size(60, 16);
            lblBayarVia.TabIndex = 2;
            lblBayarVia.Text = "Bayar via";
            grpPay.Controls.Add(lblBayarVia);

            this.cmbPayMethod = new ComboBox();
            this.cmbPayMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbPayMethod.Location = new Point(330, 28);
            this.cmbPayMethod.Name = "cmbPayMethod";
            this.cmbPayMethod.Size = new Size(110, 24);
            this.cmbPayMethod.TabIndex = 3;
            this.cmbPayMethod.Items.AddRange(new object[] { "CASH", "QRIS" });
            this.cmbPayMethod.SelectedIndex = 0;
            grpPay.Controls.Add(this.cmbPayMethod);

            lblStatusBayar = new Label();
            lblStatusBayar.AutoSize = true;
            lblStatusBayar.Location = new Point(460, 32);
            lblStatusBayar.Name = "lblStatusBayar";
            lblStatusBayar.Size = new Size(45, 16);
            lblStatusBayar.TabIndex = 4;
            lblStatusBayar.Text = "Status";
            grpPay.Controls.Add(lblStatusBayar);

            this.cmbStatusBayar = new ComboBox();
            this.cmbStatusBayar.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbStatusBayar.Location = new Point(510, 28);
            this.cmbStatusBayar.Name = "cmbStatusBayar";
            this.cmbStatusBayar.Size = new Size(140, 24);
            this.cmbStatusBayar.TabIndex = 5;
            this.cmbStatusBayar.Items.AddRange(new object[] { "BELUM_LUNAS", "LUNAS" });
            this.cmbStatusBayar.SelectedIndex = 1;
            grpPay.Controls.Add(this.cmbStatusBayar);

            this.btnBayar = new Button();
            this.btnBayar.Location = new Point(670, 26);
            this.btnBayar.Name = "btnBayar";
            this.btnBayar.Size = new Size(240, 28);
            this.btnBayar.TabIndex = 6;
            this.btnBayar.Text = "Simpan Pembayaran Bon";
            this.btnBayar.UseVisualStyleBackColor = true;
            this.btnBayar.Click += new EventHandler(this.btnBayar_Click);
            grpPay.Controls.Add(this.btnBayar);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadBon();
        }

        private void gridBon_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            PickSelectedBon();
        }

        private void chkDue_CheckedChanged(object sender, EventArgs e)
        {
            dtDue.Enabled = chkDue.Checked;
        }

        private void btnAddHutang_Click(object sender, EventArgs e)
        {
            AddHutang();
        }

        private void btnBayar_Click(object sender, EventArgs e)
        {
            PayBon();
        }

        private void Koneksi()
        {
            conn = new SqlConnection(connectionString);
        }

        private void RiwayatBon_Shown(object sender, EventArgs e)
        {
            if (session == null)
            {
                MessageBox.Show("Session kosong.");
                Close();
                return;
            }

            EnsureSchema();

            LoadBon();
        }

        private void EnsureSchema()
        {
            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureAkunKasSaldoColumn(conn);
                DbSchema.EnsureAkunKasKategoriColumn(conn);
                DbSchema.EnsureHutangTable(conn);
                conn.Close();
            }
            catch
            {
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void LoadBon()
        {
            try
            {
                gridBon.Columns.Clear();
                gridBon.Rows.Clear();

                gridBon.Columns.Add("id", "ID");
                gridBon.Columns.Add("waktu", "Waktu");
                gridBon.Columns.Add("nama", "Pelanggan");
                gridBon.Columns.Add("nominal", "Sisa Hutang");
                gridBon.Columns.Add("ket", "Keterangan");
                gridBon.Columns.Add("due", "Jatuh Tempo");
                gridBon.Columns.Add("status", "Status");
                gridBon.Columns["id"].Visible = false;

                string status = cmbStatus.SelectedItem?.ToString() ?? "ALL";
                string q = (txtCari.Text ?? "").Trim();

                Koneksi();
                conn.Open();
                DbSchema.EnsureHutangTable(conn);

                string sql = @"
SELECT TOP 500 id, waktu_dibuat, nama_pelanggan, nominal, keterangan, jatuh_tempo, status
FROM HutangPelanggan
WHERE (@status='ALL' OR status=@status)
  AND (@q='' OR nama_pelanggan LIKE '%' + @q + '%')
ORDER BY id DESC";

                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@q", q);

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    gridBon.Rows.Add(
                        reader["id"],
                        reader["waktu_dibuat"],
                        reader["nama_pelanggan"],
                        reader["nominal"],
                        reader["keterangan"],
                        reader["jatuh_tempo"],
                        reader["status"]
                    );
                }
                reader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (reader != null && !reader.IsClosed) reader.Close(); } catch { }
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void PickSelectedBon()
        {
            if (gridBon.CurrentRow == null) return;

            string status = Convert.ToString(gridBon.CurrentRow.Cells["status"].Value);
            if (status == "LUNAS")
            {
                txtBayar.Text = "";
                return;
            }

            txtBayar.Text = Convert.ToString(gridBon.CurrentRow.Cells["nominal"].Value);
            if (cmbStatusBayar != null) cmbStatusBayar.SelectedItem = "LUNAS";
        }

        private bool ValidateHutang(out decimal nominal)
        {
            nominal = 0m;

            if ((txtCustomer.Text ?? "").Trim() == "")
            {
                MessageBox.Show("Nama pelanggan wajib diisi.");
                txtCustomer.Focus();
                return false;
            }

            if (!decimal.TryParse(txtHutangAmount.Text, out nominal) || nominal <= 0)
            {
                MessageBox.Show("Nominal hutang harus angka dan > 0.");
                txtHutangAmount.Focus();
                return false;
            }

            return true;
        }

        private void AddHutang()
        {
            decimal nominal;
            if (!ValidateHutang(out nominal)) return;

            string customer = txtCustomer.Text.Trim();
            string note = (txtHutangNote.Text ?? "").Trim();
            object due = DBNull.Value;
            if (chkDue.Checked) due = dtDue.Value.Date;

            string sql = @"
INSERT INTO HutangPelanggan(waktu_dibuat, nama_pelanggan, nominal, keterangan, jatuh_tempo, status, dibuat_oleh_pengguna_id)
VALUES (SYSDATETIME(), @nama, @nominal, @ket, @due, N'BELUM_LUNAS', @userId)";

            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureHutangTable(conn);

                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nama", customer);
                cmd.Parameters.AddWithValue("@nominal", nominal);
                cmd.Parameters.AddWithValue("@ket", note == "" ? (object)DBNull.Value : note);
                cmd.Parameters.AddWithValue("@due", due);
                cmd.Parameters.AddWithValue("@userId", session.UserId);

                cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Hutang tersimpan.");

                txtCustomer.Text = "";
                txtHutangAmount.Text = "";
                txtHutangNote.Text = "";
                chkDue.Checked = false;

                LoadBon();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private long GetTargetCashAccountId(string paymentType, SqlTransaction tx)
        {
            string sql = "SELECT TOP 1 id FROM AkunKas WHERE aktif=1 AND jenis_kas=@type ORDER BY id";
            cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@type", paymentType);
            object idObj = cmd.ExecuteScalar();
            if (idObj == null || idObj == DBNull.Value) return 0;
            return Convert.ToInt64(idObj);
        }

        private void PayBon()
        {
            if (gridBon.CurrentRow == null)
            {
                MessageBox.Show("Pilih data hutang dulu.");
                return;
            }

            long hutangId = Convert.ToInt64(gridBon.CurrentRow.Cells["id"].Value);
            string status = Convert.ToString(gridBon.CurrentRow.Cells["status"].Value);
            if (status == "LUNAS")
            {
                MessageBox.Show("Hutang ini sudah lunas.");
                return;
            }

            decimal sisa = Convert.ToDecimal(gridBon.CurrentRow.Cells["nominal"].Value);
            string customer = Convert.ToString(gridBon.CurrentRow.Cells["nama"].Value);

            if (cmbPayMethod.SelectedItem == null)
            {
                MessageBox.Show("Pilih metode pembayaran (CASH/QRIS).");
                return;
            }

            string pay = cmbPayMethod.SelectedItem.ToString();

            string statusBayar = cmbStatusBayar?.SelectedItem?.ToString() ?? "LUNAS";

            decimal bayar;
            if (txtBayar.Text.Trim() == "")
            {
                if (statusBayar == "LUNAS")
                {
                    bayar = sisa;
                }
                else
                {
                    MessageBox.Show("Isi nominal bayar (karena status BELUM_LUNAS berarti bayar sebagian).");
                    txtBayar.Focus();
                    return;
                }
            }
            else if (!decimal.TryParse(txtBayar.Text, out bayar) || bayar <= 0)
            {
                MessageBox.Show("Nominal bayar harus angka dan > 0.");
                txtBayar.Focus();
                return;
            }

            if (bayar > sisa)
            {
                MessageBox.Show("Nominal bayar tidak boleh melebihi sisa hutang.");
                return;
            }

            if (statusBayar == "LUNAS" && bayar != sisa)
            {
                MessageBox.Show("Jika status LUNAS, nominal bayar harus sama dengan sisa hutang.");
                return;
            }
            if (statusBayar == "BELUM_LUNAS" && bayar >= sisa)
            {
                MessageBox.Show("Jika status BELUM_LUNAS, nominal bayar harus lebih kecil dari sisa hutang.");
                return;
            }

            DialogResult confirm = MessageBox.Show("Simpan pembayaran bon ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                Koneksi();
                conn.Open();

                SqlTransaction tx = conn.BeginTransaction();
                try
                {
                    DbSchema.EnsureAkunKasSaldoColumn(conn, tx);
                    DbSchema.EnsureAkunKasKategoriColumn(conn, tx);
                    DbSchema.EnsureHutangTable(conn, tx);

                    long destCashId = GetTargetCashAccountId(pay, tx);
                    if (destCashId <= 0)
                    {
                        tx.Rollback();
                        conn.Close();
                        MessageBox.Show($"Akun kas untuk pembayaran '{pay}' belum ada / belum aktif.");
                        return;
                    }

                    decimal sisaBaru = sisa - bayar;
                    bool lunas = (statusBayar == "LUNAS");

                    if (lunas)
                    {
                        cmd = new SqlCommand(@"
UPDATE HutangPelanggan
SET nominal = 0,
    status = N'LUNAS',
    dilunasi_pada = SYSDATETIME(),
    dilunasi_oleh_pengguna_id = @userId
WHERE id = @id", conn, tx);
                        cmd.Parameters.AddWithValue("@id", hutangId);
                        cmd.Parameters.AddWithValue("@userId", session.UserId);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        cmd = new SqlCommand(@"
UPDATE HutangPelanggan
SET nominal = @sisaBaru,
    status = N'BELUM_LUNAS'
WHERE id = @id", conn, tx);
                        cmd.Parameters.AddWithValue("@id", hutangId);
                        cmd.Parameters.AddWithValue("@sisaBaru", sisaBaru);
                        cmd.ExecuteNonQuery();
                    }

                    cmd = new SqlCommand(@"
INSERT INTO Transaksi(waktu_transaksi, tipe_transaksi, nominal, keterangan, akun_kas_sumber_id, akun_kas_tujuan_id, dibuat_oleh_pengguna_id)
VALUES (SYSDATETIME(), N'PEMASUKAN', @amount, @ket, NULL, @dest, @userId)", conn, tx);
                    cmd.Parameters.AddWithValue("@amount", bayar);
                    cmd.Parameters.AddWithValue("@ket", (lunas ? "Pelunasan Bon: " : "Pembayaran Bon (Sebagian): ") + customer);
                    cmd.Parameters.AddWithValue("@dest", destCashId);
                    cmd.Parameters.AddWithValue("@userId", session.UserId);
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("UPDATE AkunKas SET saldo = saldo + @amount WHERE id = @id", conn, tx);
                    cmd.Parameters.AddWithValue("@amount", bayar);
                    cmd.Parameters.AddWithValue("@id", destCashId);
                    cmd.ExecuteNonQuery();

                    tx.Commit();
                    conn.Close();

                    MessageBox.Show(lunas ? "Bon ditandai LUNAS." : "Pembayaran tersimpan. Sisa hutang berkurang.");
                    txtBayar.Text = "";
                    LoadBon();
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
