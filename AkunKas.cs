using System;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SaldoGo
{
    public partial class AkunKas : Form
    {
        private UserSession session;

        private readonly BindingSource akunKasBindingSource = new BindingSource();
        private DataTable akunKasTable;
        private BindingNavigator akunKasNavigator;

        private readonly string connectionString = KoneksiDb.koneksi;

        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader reader;

        public AkunKas()
        {
            InitializeComponent();

            SetupGridBinding();

            InputValidation.AttachDecimalOnly(txtBalance, "Saldo");
        }

        private void SetupGridBinding()
        {
            grid.AutoGenerateColumns = true;
            grid.DataSource = akunKasBindingSource;
            akunKasBindingSource.CurrentChanged += (s, e) => PickFromBinding();

            akunKasNavigator = new BindingNavigator(true);
            akunKasNavigator.BindingSource = akunKasBindingSource;
            akunKasNavigator.Location = new System.Drawing.Point(12, 304);
            akunKasNavigator.Size = new System.Drawing.Size(620, 27);
            akunKasNavigator.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            this.Controls.Add(akunKasNavigator);
        }

        public AkunKas(UserSession session) : this()
        {
            this.session = session;
        }

        private void Koneksi()
        {
            conn = new SqlConnection(connectionString);
        }

        private void AkunKas_Shown(object sender, EventArgs e)
        {
            EnsureSaldoColumn();
            LoadData();
        }

        private void EnsureSaldoColumn()
        {
            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureAkunKasSaldoColumn(conn);
                DbSchema.EnsureAkunKasKategoriColumn(conn);
                DbSchema.EnsureAkunKasViewsAndProcedures(conn);
                conn.Close();
            }
            catch
            {
                try
                {
                    if (conn != null) conn.Close();
                }
                catch { }
            }
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            PickFromGrid(e);
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

        private void LoadData()
        {
            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureAkunKasSaldoColumn(conn);
                DbSchema.EnsureAkunKasKategoriColumn(conn);
                DbSchema.EnsureAkunKasViewsAndProcedures(conn);

                cmd = new SqlCommand("dbo.sp_AkunKas_Search", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@q", DBNull.Value);
                cmd.Parameters.AddWithValue("@kategori_kas", DBNull.Value);
                cmd.Parameters.AddWithValue("@jenis_kas", DBNull.Value);
                cmd.Parameters.AddWithValue("@aktif", DBNull.Value);
                cmd.Parameters.AddWithValue("@maxRows", 1000);

                akunKasTable = new DataTable();
                reader = cmd.ExecuteReader();
                akunKasTable.Load(reader);
                int total = akunKasTable.Rows.Count;
                if (reader.NextResult() && reader.Read())
                {
                    total = Convert.ToInt32(reader["total"]);
                }
                reader.Close();

                akunKasBindingSource.DataSource = akunKasTable;
                lblCount.Text = "Total: " + total.ToString();

                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                if (grid.Columns.Contains("id")) grid.Columns["id"].Visible = false;

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

        private void PickFromGrid(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (akunKasBindingSource != null) akunKasBindingSource.Position = e.RowIndex;
        }

        private void PickFromBinding()
        {
            if (!(akunKasBindingSource.Current is DataRowView drv)) return;

            txtId.Text = Convert.ToString(drv["id"]);
            txtName.Text = Convert.ToString(drv["nama"]);
            txtBalance.Text = Convert.ToString(drv["saldo"]);
            cmbCategory.SelectedItem = Convert.ToString(drv["kategori_kas"]);
            cmbType.SelectedItem = Convert.ToString(drv["jenis_kas"]);
            chkActive.Checked = Convert.ToBoolean(drv["aktif"]);
        }

        private bool ValidateInput(out decimal saldo)
        {
            saldo = 0m;

            if (txtName.Text.Trim() == "")
            {
                MessageBox.Show("Nama akun kas wajib diisi.");
                txtName.Focus();
                return false;
            }

            if (cmbType.SelectedItem == null)
            {
                MessageBox.Show("Tipe akun kas wajib dipilih.");
                cmbType.Focus();
                return false;
            }

            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("Kategori kas wajib dipilih (LACI/REKENING/EWALLET). ");
                cmbCategory.Focus();
                return false;
            }

            if (txtBalance.Text.Trim() == "")
            {
                txtBalance.Text = "0";
            }

            if (!decimal.TryParse(txtBalance.Text, out saldo) || saldo < 0)
            {
                MessageBox.Show("Saldo harus angka dan >= 0.");
                txtBalance.Focus();
                return false;
            }

            return true;
        }

        private void Insert()
        {
            decimal saldo;
            if (!ValidateInput(out saldo)) return;
            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureAkunKasSaldoColumn(conn);
                DbSchema.EnsureAkunKasKategoriColumn(conn);
                DbSchema.EnsureAkunKasViewsAndProcedures(conn);

                cmd = new SqlCommand("dbo.sp_AkunKas_Insert", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nama", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@kategori_kas", cmbCategory.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@jenis_kas", cmbType.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@saldo", saldo);
                cmd.Parameters.AddWithValue("@aktif", chkActive.Checked);
                SqlParameter outId = new SqlParameter("@new_id", SqlDbType.BigInt);
                outId.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outId);

                int rows = cmd.ExecuteNonQuery();
                long newId = 0;
                if (outId.Value != null && outId.Value != DBNull.Value)
                {
                    newId = Convert.ToInt64(outId.Value);
                }
                conn.Close();

                MessageBox.Show("Berhasil insert: " + rows + " baris. ID: " + newId);
                btnShow.PerformClick();
                ClearInput();
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

        private void Update()
        {
            if (txtId.Text.Trim() == "")
            {
                MessageBox.Show("Pilih data yang akan diupdate.");
                grid.Focus();
                return;
            }
            decimal saldo;
            if (!ValidateInput(out saldo)) return;

            DialogResult confirm = MessageBox.Show("Yakin update data ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureAkunKasSaldoColumn(conn);
                DbSchema.EnsureAkunKasKategoriColumn(conn);
                DbSchema.EnsureAkunKasViewsAndProcedures(conn);

                cmd = new SqlCommand("dbo.sp_AkunKas_Update", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Convert.ToInt64(txtId.Text.Trim()));
                cmd.Parameters.AddWithValue("@nama", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@kategori_kas", cmbCategory.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@jenis_kas", cmbType.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@saldo", saldo);
                cmd.Parameters.AddWithValue("@aktif", chkActive.Checked);
                int rows = cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Berhasil update: " + rows + " baris.");
                btnShow.PerformClick();
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

        private void Delete()
        {
            if (txtId.Text.Trim() == "")
            {
                MessageBox.Show("Pilih data yang akan dihapus.");
                grid.Focus();
                return;
            }

            DialogResult confirm = MessageBox.Show("Yakin hapus data ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureAkunKasViewsAndProcedures(conn);

                cmd = new SqlCommand("dbo.sp_AkunKas_Delete", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", Convert.ToInt64(txtId.Text.Trim()));
                cmd.Parameters.AddWithValue("@hardDelete", 1);
                int rows = cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Berhasil delete: " + rows + " baris.");
                btnShow.PerformClick();
                ClearInput();
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
            txtId.Text = "";
            txtName.Text = "";
            txtBalance.Text = "0";
            cmbCategory.SelectedIndex = -1;
            cmbType.SelectedIndex = -1;
            chkActive.Checked = true;
            txtName.Focus();
        }
    }
}
