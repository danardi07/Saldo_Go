using System;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SaldoGo
{
    public partial class Transaksi : Form
    {
        private UserSession session;

        private long selectedMenuId = 0;
        private string selectedMenuName = "";
        private decimal selectedMenuPrice = 0m;

        private readonly string connectionString = KoneksiDb.koneksi;

        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader reader;

        public Transaksi()
        {
            InitializeComponent();
        }

        public Transaksi(UserSession session) : this()
        {
            this.session = session;
        }

        private void Koneksi()
        {
            conn = new SqlConnection(connectionString);
        }

        private void Transaksi_Shown(object sender, EventArgs e)
        {
            if (session == null)
            {
                MessageBox.Show("Session kosong.");
                Close();
                return;
            }

            LoadPaymentMethods();

            LoadMenuList();

            grid.CellClick -= grid_CellClick;
            grid.CellClick += grid_CellClick;

            numQty.ValueChanged -= numQty_ValueChanged;
            numQty.ValueChanged += numQty_ValueChanged;

            ClearInput();
        }

        private void label_Click(object sender, EventArgs e)
        {
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveSale();
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            LoadMenuList();
        }

        private void LoadPaymentMethods()
        {
            try
            {
                cmbSourceCash.DataSource = null;
                cmbSourceCash.Items.Clear();
                cmbSourceCash.Items.Add("CASH");
                cmbSourceCash.Items.Add("QRIS");
                cmbSourceCash.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadMenuList()
        {
            try
            {
                grid.Columns.Clear();
                grid.Rows.Clear();

                grid.Columns.Add("id", "ID");
                grid.Columns.Add("kategori", "Kategori");
                grid.Columns.Add("nama", "Nama");
                grid.Columns.Add("satuan", "Satuan");
                grid.Columns.Add("harga_jual", "Harga Jual");
                grid.Columns["id"].Visible = false;

                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                string sql = @"SELECT m.id, m.kategori, m.nama, m.satuan, m.harga_jual
 FROM dbo.v_MenuActive m
 ORDER BY m.nama";

                Koneksi();
                conn.Open();
                DbSchema.EnsureMenuViewsAndProcedures(conn);
                cmd = new SqlCommand(sql, conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    grid.Rows.Add(
                        reader["id"],
                        reader["kategori"],
                        reader["nama"],
                        reader["satuan"],
                        reader["harga_jual"]
                    );
                }
                reader.Close();

                cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.v_MenuActive", conn);
                object totalObj = cmd.ExecuteScalar();
                lblCount.Text = "Total menu: " + Convert.ToInt32(totalObj);

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                try
                {
                    if (reader != null && !reader.IsClosed) reader.Close();
                }
                catch { }

                try
                {
                    if (conn != null) conn.Close();
                }
                catch { }
            }
        }

        private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = grid.Rows[e.RowIndex];

            selectedMenuId = Convert.ToInt64(row.Cells["id"].Value);
            selectedMenuName = Convert.ToString(row.Cells["nama"].Value);
            selectedMenuPrice = Convert.ToDecimal(row.Cells["harga_jual"].Value);

            txtMenu.Text = selectedMenuName;
            RecalcTotal();
        }

        private void numQty_ValueChanged(object sender, EventArgs e)
        {
            RecalcTotal();
        }

        private void RecalcTotal()
        {
            if (selectedMenuId <= 0)
            {
                txtAmount.Text = "";
                return;
            }

            decimal total = selectedMenuPrice * Convert.ToDecimal(numQty.Value);
            txtAmount.Text = total.ToString("0.##");
        }

        private void SaveSale()
        {
            if (selectedMenuId <= 0)
            {
                MessageBox.Show("Pilih menu dulu dari daftar.");
                return;
            }

            if (cmbSourceCash.SelectedItem == null)
            {
                MessageBox.Show("Pilih metode pembayaran (CASH/QRIS).");
                return;
            }

            string paymentType = cmbSourceCash.SelectedItem.ToString();
            int qty = Convert.ToInt32(numQty.Value);
            decimal total = selectedMenuPrice * qty;
            if (total <= 0)
            {
                MessageBox.Show("Total tidak valid.");
                return;
            }

            string note = txtDesc.Text.Trim();
            string desc = $"Penjualan: {selectedMenuName} x{qty}";
            if (note != "") desc += " | " + note;

            SqlTransaction trans = null;

            try
            {
                Koneksi();
                conn.Open();

                // Mulai transaction
                trans = conn.BeginTransaction();

                // Pastikan schema tersedia
                DbSchema.EnsureAkunKasSaldoColumn(conn, trans);
                DbSchema.EnsureAkunKasKategoriColumn(conn, trans);
                DbSchema.EnsureAkunKasViewsAndProcedures(conn, trans);

                // Ambil ID akun kas tujuan berdasarkan payment type
                long destCashId = 0;
                string sqlCash = @"SELECT TOP 1 id FROM dbo.v_AkunKasActive 
                                 WHERE UPPER(jenis_kas) = @paymentType 
                                 ORDER BY id";
                cmd = new SqlCommand(sqlCash, conn, trans);
                cmd.Parameters.AddWithValue("@paymentType", paymentType.ToUpper());
                object cashResult = cmd.ExecuteScalar();
                if (cashResult != null && cashResult != DBNull.Value)
                {
                    destCashId = Convert.ToInt64(cashResult);
                }

                if (destCashId <= 0)
                {
                    throw new Exception("Akun kas tujuan belum ada / belum aktif.");
                }

                // INSERT ke dbo.Transaksi
                string sqlInsertTrx = @"INSERT INTO dbo.Transaksi 
                                       (waktu_transaksi, tipe_transaksi, nominal, keterangan, akun_kas_sumber_id, akun_kas_tujuan_id, dibuat_oleh_pengguna_id)
                                       VALUES (@waktu, @tipe, @nominal, @keterangan, @sumber_id, @tujuan_id, @user_id);
                                       SELECT SCOPE_IDENTITY();";
                cmd = new SqlCommand(sqlInsertTrx, conn, trans);
                cmd.Parameters.AddWithValue("@waktu", DateTime.Now);
                cmd.Parameters.AddWithValue("@tipe", "PEMASUKAN");
                cmd.Parameters.AddWithValue("@nominal", total);
                cmd.Parameters.AddWithValue("@keterangan", desc);
                cmd.Parameters.AddWithValue("@sumber_id", DBNull.Value);
                cmd.Parameters.AddWithValue("@tujuan_id", destCashId);
                cmd.Parameters.AddWithValue("@user_id", session.UserId);

                object trxResult = cmd.ExecuteScalar();
                long trxId = 0;
                if (trxResult != null && trxResult != DBNull.Value)
                {
                    trxId = Convert.ToInt64(trxResult);
                }

                // INSERT ke dbo.LogAktivitas
                string logAktivitas = $"INSERT TRANSAKSI : PEMASUKAN - {total}";
                string sqlInsertLog = @"INSERT INTO dbo.LogAktivitas (aktivitas, waktu)
                                        VALUES (@aktivitas, @waktu)";
                cmd = new SqlCommand(sqlInsertLog, conn, trans);
                cmd.Parameters.AddWithValue("@aktivitas", logAktivitas);
                cmd.Parameters.AddWithValue("@waktu", DateTime.Now);
                cmd.ExecuteNonQuery();

                // Commit transaction jika semua berhasil
                trans.Commit();
                trans = null;

                conn.Close();

                MessageBox.Show("Transaksi berhasil. ID Transaksi: " + trxId);
                ClearInput();
            }
            catch (SqlException ex)
            {
                // Rollback transaction jika terjadi error SQL
                if (trans != null)
                {
                    try
                    {
                        trans.Rollback();
                    }
                    catch { }
                }

                MessageBox.Show("Error SQL: " + ex.Message);

                try { if (conn != null && conn.State == ConnectionState.Open) conn.Close(); } catch { }
            }
            catch (Exception ex)
            {
                // Rollback transaction jika terjadi error umum
                if (trans != null)
                {
                    try
                    {
                        trans.Rollback();
                    }
                    catch { }
                }

                MessageBox.Show("Error: " + ex.Message);

                try { if (conn != null && conn.State == ConnectionState.Open) conn.Close(); } catch { }
            }
            finally
            {
                // Pastikan connection ditutup
                try
                {
                    if (conn != null && conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
                catch { }
            }
        }

        private void ClearInput()
        {
            selectedMenuId = 0;
            selectedMenuName = "";
            selectedMenuPrice = 0m;

            txtMenu.Text = "";
            numQty.Value = 1;
            txtAmount.Text = "";
            txtDesc.Text = "";

            grid.ClearSelection();
            grid.Focus();
        }

    }
}
