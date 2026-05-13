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

        private readonly BindingSource menuBindingSource = new BindingSource();
        private DataTable menuTable;
        private BindingNavigator menuNavigator;

        private readonly string connectionString = KoneksiDb.koneksi;

        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader reader;

        public Menu()
        {
            InitializeComponent();

            SetupMenuGridBinding();

            InputValidation.AttachDecimalOnly(txtSellingPrice, "Harga jual");
            InputValidation.AttachDecimalOnly(txtCost, "Estimasi HPP");
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
            EnsureMenuDbObjects();
            EnsureDefaultCategories();
            LoadCategories();
            LoadData();
        }

        private void EnsureMenuDbObjects()
        {
            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureMenuViewsAndProcedures(conn);
                conn.Close();
            }
            catch
            {
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void SetupMenuGridBinding()
        {
            grid.AutoGenerateColumns = false;
            grid.Columns.Clear();

            DataGridViewTextBoxColumn colId = new DataGridViewTextBoxColumn();
            colId.Name = "id";
            colId.HeaderText = "ID";
            colId.DataPropertyName = "id";
            colId.Visible = false;
            grid.Columns.Add(colId);

            DataGridViewTextBoxColumn colKategoriId = new DataGridViewTextBoxColumn();
            colKategoriId.Name = "kategori_id";
            colKategoriId.HeaderText = "KategoriId";
            colKategoriId.DataPropertyName = "kategori_id";
            colKategoriId.Visible = false;
            grid.Columns.Add(colKategoriId);

            DataGridViewTextBoxColumn colKategori = new DataGridViewTextBoxColumn();
            colKategori.Name = "kategori";
            colKategori.HeaderText = "Kategori";
            colKategori.DataPropertyName = "kategori";
            grid.Columns.Add(colKategori);

            DataGridViewTextBoxColumn colNama = new DataGridViewTextBoxColumn();
            colNama.Name = "nama";
            colNama.HeaderText = "Nama";
            colNama.DataPropertyName = "nama";
            grid.Columns.Add(colNama);

            DataGridViewTextBoxColumn colSatuan = new DataGridViewTextBoxColumn();
            colSatuan.Name = "satuan";
            colSatuan.HeaderText = "Satuan";
            colSatuan.DataPropertyName = "satuan";
            grid.Columns.Add(colSatuan);

            DataGridViewTextBoxColumn colHargaJual = new DataGridViewTextBoxColumn();
            colHargaJual.Name = "harga_jual";
            colHargaJual.HeaderText = "Harga Jual";
            colHargaJual.DataPropertyName = "harga_jual";
            grid.Columns.Add(colHargaJual);

            DataGridViewTextBoxColumn colModal = new DataGridViewTextBoxColumn();
            colModal.Name = "perkiraan_modal";
            colModal.HeaderText = "Estimasi HPP";
            colModal.DataPropertyName = "perkiraan_modal";
            grid.Columns.Add(colModal);

            DataGridViewTextBoxColumn colMargin = new DataGridViewTextBoxColumn();
            colMargin.Name = "margin";
            colMargin.HeaderText = "Margin";
            colMargin.DataPropertyName = "margin";
            grid.Columns.Add(colMargin);

            DataGridViewCheckBoxColumn colAktif = new DataGridViewCheckBoxColumn();
            colAktif.Name = "aktif";
            colAktif.HeaderText = "Aktif";
            colAktif.DataPropertyName = "aktif";
            grid.Columns.Add(colAktif);

            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            menuBindingSource.CurrentChanged += (s, e) => PickFromBinding();
            grid.DataSource = menuBindingSource;

            menuNavigator = new BindingNavigator(true);
            menuNavigator.BindingSource = menuBindingSource;
            menuNavigator.Location = new System.Drawing.Point(12, 330);
            menuNavigator.Size = new System.Drawing.Size(888, 27);
            menuNavigator.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            this.Controls.Add(menuNavigator);
        }

        private void EnsureDefaultCategories()
        {
            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureKategoriMenuViewsAndProcedures(conn);

                cmd = new SqlCommand("dbo.sp_KategoriMenu_EnsureDefaults", conn);
                cmd.CommandType = CommandType.StoredProcedure;
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
                string query = @"SELECT id, nama
 FROM dbo.v_KategoriMenu_Default
 ORDER BY nama";

                Koneksi();
                conn.Open();

                DbSchema.EnsureKategoriMenuViewsAndProcedures(conn);

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

                Koneksi();
                conn.Open();
                DbSchema.EnsureMenuViewsAndProcedures(conn);

                cmd = new SqlCommand("dbo.sp_Menu_Search", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@q", q);
                cmd.Parameters.Add("@kategori_id", SqlDbType.BigInt).Value = DBNull.Value;
                cmd.Parameters.Add("@aktif", SqlDbType.Bit).Value = DBNull.Value;
                cmd.Parameters.Add("@maxRows", SqlDbType.Int).Value = 500;

                menuTable = new DataTable();

                int total = 0;
                reader = cmd.ExecuteReader();
                menuTable.Load(reader);
                if (reader.NextResult() && reader.Read())
                {
                    total = Convert.ToInt32(reader["total"]);
                }
                reader.Close();

                menuBindingSource.DataSource = menuTable;
                lblCount.Text = "Total: " + total.ToString();

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

        private void PickFromBinding()
        {
            if (!(menuBindingSource.Current is DataRowView drv)) return;

            txtId.Text = Convert.ToString(drv["id"]);
            cmbCategory.SelectedValue = Convert.ToInt64(drv["kategori_id"]);
            txtName.Text = Convert.ToString(drv["nama"]);
            txtUnit.Text = Convert.ToString(drv["satuan"]);
            txtSellingPrice.Text = Convert.ToString(drv["harga_jual"]);
            txtCost.Text = Convert.ToString(drv["perkiraan_modal"]);
            chkActive.Checked = Convert.ToBoolean(drv["aktif"]);
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

            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureMenuViewsAndProcedures(conn);

                cmd = new SqlCommand("dbo.sp_Menu_Insert", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@kategori_id", cmbCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@nama", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@satuan", txtUnit.Text.Trim());
                cmd.Parameters.AddWithValue("@harga_jual", sellingPrice);
                cmd.Parameters.AddWithValue("@perkiraan_modal", (object)cost ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@aktif", chkActive.Checked);
                SqlParameter outId = new SqlParameter("@new_id", SqlDbType.BigInt);
                outId.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outId);

                cmd.ExecuteNonQuery();
                long newId = 0;
                if (outId.Value != null && outId.Value != DBNull.Value)
                {
                    newId = Convert.ToInt64(outId.Value);
                }

                conn.Close();

                MessageBox.Show("Berhasil insert. ID: " + newId.ToString());
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

            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureMenuViewsAndProcedures(conn);

                cmd = new SqlCommand("dbo.sp_Menu_Update", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Convert.ToInt64(txtId.Text));
                cmd.Parameters.AddWithValue("@kategori_id", cmbCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@nama", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@satuan", txtUnit.Text.Trim());
                cmd.Parameters.AddWithValue("@harga_jual", sellingPrice);
                cmd.Parameters.AddWithValue("@perkiraan_modal", (object)cost ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@aktif", chkActive.Checked);

                int rows = cmd.ExecuteNonQuery();

                conn.Close();

                MessageBox.Show("Berhasil update: " + rows + " baris.");
                LoadData();
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

            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureMenuViewsAndProcedures(conn);

                cmd = new SqlCommand("dbo.sp_Menu_Delete", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Convert.ToInt64(txtId.Text));
                cmd.Parameters.AddWithValue("@hardDelete", 1);
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
