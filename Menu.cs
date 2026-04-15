using System;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SaldoGo
{
    public partial class Menu : Form
    {
        private UserSession session;

        private readonly string connectionString = KoneksiDb.koneksi;

        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader reader;

        public Menu()
        {
            InitializeComponent();
        }

        public Menu(UserSession session) : this()
        {
            this.session = session;
        }

        private void Koneksi()
        {
            conn = new SqlConnection(connectionString);
        }

        private void Menu_Shown(object sender, EventArgs e)
        {
            EnsureDefaultCategories();
            LoadCategories();
            LoadData();
        }

        private void EnsureDefaultCategories()
        {
            try
            {
                Koneksi();
                conn.Open();

                string sql = @"
IF NOT EXISTS (SELECT 1 FROM KategoriMenu WHERE LOWER(LTRIM(RTRIM(nama))) = 'makanan')
    INSERT INTO KategoriMenu(nama) VALUES ('Makanan');
IF NOT EXISTS (SELECT 1 FROM KategoriMenu WHERE LOWER(LTRIM(RTRIM(nama))) = 'minuman')
    INSERT INTO KategoriMenu(nama) VALUES ('Minuman');";

                cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch
            {
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            LoadData();
        }

        private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            PickFromGrid();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            Insert();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            Update();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInput();
        }

        private void LoadCategories()
        {
            try
            {
                string query = @"SELECT MIN(id) AS id, MAX(nama) AS nama
FROM KategoriMenu
WHERE LOWER(nama) IN ('makanan', 'minuman')
GROUP BY LOWER(nama)
ORDER BY MAX(nama)";

                Koneksi();
                conn.Open();

                cmd = new SqlCommand(query, conn);
                reader = cmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Columns.Add("id", typeof(long));
                dt.Columns.Add("nama", typeof(string));

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();
                    row["id"] = Convert.ToInt64(reader["id"]);
                    row["nama"] = reader["nama"].ToString();
                    dt.Rows.Add(row);
                }

                reader.Close();
                conn.Close();

                cmbCategory.DataSource = dt;
                cmbCategory.DisplayMember = "nama";
                cmbCategory.ValueMember = "id";

                bool hasMakanan = false;
                bool hasMinuman = false;
                foreach (DataRow r in dt.Rows)
                {
                    var n = Convert.ToString(r["nama"])?.Trim().ToLowerInvariant();
                    if (n == "makanan") hasMakanan = true;
                    if (n == "minuman") hasMinuman = true;
                }

                if (!hasMakanan || !hasMinuman)
                {
                    string missing = (!hasMakanan && !hasMinuman) ? "Makanan dan Minuman"
                        : (!hasMakanan) ? "Makanan"
                        : "Minuman";

                    MessageBox.Show($"Kategori berikut belum ada di tabel KategoriMenu: {missing}.\n" +
                                    "Silakan tambahkan di database agar dropdown menampilkan lengkap.");
                }
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

        private void LoadData()
        {
            try
            {
                string q = txtSearch.Text;
                if (q == null)
                {
                    q = "";
                }
                q = q.Trim();

                grid.Rows.Clear();
                grid.Columns.Clear();
                grid.Columns.Add("id", "ID");
                grid.Columns.Add("kategori_id", "KategoriId");
                grid.Columns.Add("kategori", "Kategori");
                grid.Columns.Add("nama", "Nama");
                grid.Columns.Add("satuan", "Satuan");
                grid.Columns.Add("harga_jual", "Harga Jual");
                grid.Columns.Add("perkiraan_modal", "Estimasi HPP");
                grid.Columns.Add("aktif", "Aktif");
                grid.Columns["kategori_id"].Visible = false;

                Koneksi();
                conn.Open();

                string query;
                if (q == "")
                {
                    query = @"SELECT mi.id, mi.kategori_id, c.nama AS kategori, mi.nama, mi.satuan, mi.harga_jual, mi.perkiraan_modal, mi.aktif
FROM Menu mi
JOIN KategoriMenu c ON c.id = mi.kategori_id
ORDER BY mi.id DESC";
                    cmd = new SqlCommand(query, conn);
                }
                else
                {
                    query = @"SELECT mi.id, mi.kategori_id, c.nama AS kategori, mi.nama, mi.satuan, mi.harga_jual, mi.perkiraan_modal, mi.aktif
FROM Menu mi
JOIN KategoriMenu c ON c.id = mi.kategori_id
WHERE mi.nama LIKE '%' + @q + '%'
ORDER BY mi.id DESC";
                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@q", q);
                }

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    grid.Rows.Add(
                        reader["id"].ToString(),
                        reader["kategori_id"].ToString(),
                        reader["kategori"].ToString(),
                        reader["nama"].ToString(),
                        reader["satuan"].ToString(),
                        reader["harga_jual"].ToString(),
                        reader["perkiraan_modal"].ToString(),
                        Convert.ToBoolean(reader["aktif"])
                    );
                }
                reader.Close();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Menu", conn);
                int jumlah = (int)cmd.ExecuteScalar();
                lblCount.Text = "Total: " + jumlah.ToString();

                conn.Close();
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
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

        private void PickFromGrid()
        {
            if (grid.CurrentRow == null) return;

            txtId.Text = Convert.ToString(grid.CurrentRow.Cells["id"].Value);
            cmbCategory.SelectedValue = Convert.ToInt64(grid.CurrentRow.Cells["kategori_id"].Value);
            txtName.Text = Convert.ToString(grid.CurrentRow.Cells["nama"].Value);
            txtUnit.Text = Convert.ToString(grid.CurrentRow.Cells["satuan"].Value);
            txtSellingPrice.Text = Convert.ToString(grid.CurrentRow.Cells["harga_jual"].Value);
            txtCost.Text = Convert.ToString(grid.CurrentRow.Cells["perkiraan_modal"].Value);
            chkActive.Checked = Convert.ToBoolean(grid.CurrentRow.Cells["aktif"].Value);
        }

        private bool ValidateInput(out decimal sellingPrice, out decimal? cost)
        {
            sellingPrice = 0m;
            cost = null;

            if (cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Kategori wajib dipilih.");
                cmbCategory.Focus();
                return false;
            }

            if (txtName.Text.Trim() == "")
            {
                MessageBox.Show("Nama menu wajib diisi.");
                txtName.Focus();
                return false;
            }

            if (txtUnit.Text.Trim() == "")
            {
                MessageBox.Show("Satuan wajib diisi.");
                txtUnit.Focus();
                return false;
            }

            if (!decimal.TryParse(txtSellingPrice.Text, out sellingPrice) || sellingPrice <= 0)
            {
                MessageBox.Show("Harga jual harus angka dan > 0.");
                txtSellingPrice.Focus();
                return false;
            }

            if (txtCost.Text.Trim() != "")
            {
                decimal c;
                if (!decimal.TryParse(txtCost.Text, out c) || c < 0)
                {
                    MessageBox.Show("Estimasi HPP harus angka dan >= 0.");
                    txtCost.Focus();
                    return false;
                }
                cost = c;
            }

            return true;
        }

        private void Insert()
        {
            decimal sellingPrice;
            decimal? cost;
            if (!ValidateInput(out sellingPrice, out cost)) return;

            string sql = @"
INSERT INTO Menu(kategori_id, nama, satuan, harga_jual, perkiraan_modal, aktif)
VALUES (@kategori_id, @nama, @satuan, @harga_jual, @perkiraan_modal, @aktif)";

            try
            {
                Koneksi();
                conn.Open();

                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@kategori_id", cmbCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@nama", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@satuan", txtUnit.Text.Trim());
                cmd.Parameters.AddWithValue("@harga_jual", sellingPrice);
                if (cost == null)
                {
                    cmd.Parameters.AddWithValue("@perkiraan_modal", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@perkiraan_modal", cost.Value);
                }
                int aktif = 1;
                if (chkActive.Checked == false) aktif = 0;
                cmd.Parameters.AddWithValue("@aktif", aktif);
                int rows = cmd.ExecuteNonQuery();

                conn.Close();

                MessageBox.Show("Berhasil insert: " + rows + " baris.");
                btnShow.PerformClick();
                ClearInput();
            }
            catch (Exception ex)
            {
                try
                {
                    if (conn != null) conn.Close();
                }
                catch { }

                MessageBox.Show(ex.Message);
            }
        }

        private void Update()
        {
            if (txtId.Text.Trim() == "")
            {
                MessageBox.Show("Pilih data yang akan diupdate.");
                return;
            }

            decimal sellingPrice;
            decimal? cost;
            if (!ValidateInput(out sellingPrice, out cost)) return;

            DialogResult confirm = MessageBox.Show("Yakin update data ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            string sql = @"
UPDATE Menu
SET kategori_id = @kategori_id,
    nama = @nama,
    satuan = @satuan,
    harga_jual = @harga_jual,
    perkiraan_modal = @perkiraan_modal,
    aktif = @aktif
WHERE id = @id";

            try
            {
                Koneksi();
                conn.Open();

                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", txtId.Text);
                cmd.Parameters.AddWithValue("@kategori_id", cmbCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@nama", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@satuan", txtUnit.Text.Trim());
                cmd.Parameters.AddWithValue("@harga_jual", sellingPrice);
                if (cost == null)
                {
                    cmd.Parameters.AddWithValue("@perkiraan_modal", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@perkiraan_modal", cost.Value);
                }
                int aktif = 1;
                if (chkActive.Checked == false) aktif = 0;
                cmd.Parameters.AddWithValue("@aktif", aktif);
                int rows = cmd.ExecuteNonQuery();

                conn.Close();

                MessageBox.Show("Berhasil update: " + rows + " baris.");
                btnShow.PerformClick();
            }
            catch (Exception ex)
            {
                try
                {
                    if (conn != null) conn.Close();
                }
                catch { }

                MessageBox.Show(ex.Message);
            }
        }

        private void Delete()
        {
            if (txtId.Text.Trim() == "")
            {
                MessageBox.Show("Pilih data yang akan dihapus.");
                return;
            }

            DialogResult confirm = MessageBox.Show("Yakin hapus data ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            string sql = "DELETE FROM Menu WHERE id = @id";
            try
            {
                Koneksi();
                conn.Open();

                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", txtId.Text);
                int rows = cmd.ExecuteNonQuery();

                conn.Close();

                MessageBox.Show("Berhasil delete: " + rows + " baris.");
                LoadData();
                ClearInput();
            }
            catch (Exception ex)
            {
                try
                {
                    if (conn != null) conn.Close();
                }
                catch { }

                MessageBox.Show(ex.Message);
            }
        }

        private void ClearInput()
        {
            txtId.Text = "";
            txtName.Text = "";
            txtUnit.Text = "";
            txtSellingPrice.Text = "";
            txtCost.Text = "";
            chkActive.Checked = true;
            txtName.Focus();
        }
    }
}
