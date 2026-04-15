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

                string sql = @"SELECT m.id, c.nama AS kategori, m.nama, m.satuan, m.harga_jual
FROM Menu m
JOIN KategoriMenu c ON c.id = m.kategori_id
WHERE m.aktif = 1
ORDER BY m.nama";

                Koneksi();
                conn.Open();
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

                cmd = new SqlCommand("SELECT COUNT(*) FROM Menu WHERE aktif=1", conn);
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

        private long GetTargetCashAccountId(string paymentType, SqlTransaction tx)
        {
            string sql = "SELECT TOP 1 id FROM AkunKas WHERE aktif=1 AND jenis_kas=@type ORDER BY id";

            cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@type", paymentType);
            object idObj = cmd.ExecuteScalar();
            if (idObj == null || idObj == DBNull.Value) return 0;
            return Convert.ToInt64(idObj);
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

            string sql = @"
INSERT INTO Transaksi(waktu_transaksi, tipe_transaksi, nominal, keterangan, akun_kas_sumber_id, akun_kas_tujuan_id, dibuat_oleh_pengguna_id)
VALUES (SYSDATETIME(), N'PEMASUKAN', @amount, @desc, NULL, @dest, @userId)";

            try
            {
                Koneksi();
                conn.Open();

                SqlTransaction tx = conn.BeginTransaction();
                try
                {
                    DbSchema.EnsureAkunKasSaldoColumn(conn, tx);

                    long destCashId = GetTargetCashAccountId(paymentType, tx);
                    if (destCashId <= 0)
                    {
                        tx.Rollback();
                        conn.Close();
                        MessageBox.Show($"Akun kas untuk pembayaran '{paymentType}' belum ada / belum aktif. Silakan buat di menu Akun Kas.");
                        return;
                    }

                    cmd = new SqlCommand(sql, conn, tx);
                    cmd.Parameters.AddWithValue("@amount", total);
                    cmd.Parameters.AddWithValue("@desc", desc);
                    cmd.Parameters.AddWithValue("@dest", destCashId);
                    cmd.Parameters.AddWithValue("@userId", session.UserId);

                    int rows = cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("UPDATE AkunKas SET saldo = saldo + @amount WHERE id = @id", conn, tx);
                    cmd.Parameters.AddWithValue("@amount", total);
                    cmd.Parameters.AddWithValue("@id", destCashId);
                    cmd.ExecuteNonQuery();

                    tx.Commit();
                    conn.Close();

                    MessageBox.Show("Berhasil simpan pemasukan: " + rows + " baris.");
                    ClearInput();
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
                try
                {
                    if (conn != null) conn.Close();
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
