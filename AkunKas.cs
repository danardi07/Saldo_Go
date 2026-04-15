using System;
using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SaldoGo
{
    public partial class AkunKas : Form
    {
        private UserSession session;

        private readonly string connectionString = KoneksiDb.koneksi;

        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader reader;

        public AkunKas()
        {
            InitializeComponent();
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
                grid.Columns.Clear();
                grid.Rows.Clear();

                grid.Columns.Add("id", "ID");
                grid.Columns.Add("nama", "Nama");
                grid.Columns.Add("kategori_kas", "Kategori");
                grid.Columns.Add("jenis_kas", "Jenis Kas");
                grid.Columns.Add("saldo", "Saldo");

                DataGridViewCheckBoxColumn colAktif = new DataGridViewCheckBoxColumn();
                colAktif.Name = "aktif";
                colAktif.HeaderText = "Aktif";
                grid.Columns.Add(colAktif);

                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                string sql = "SELECT id, nama, kategori_kas, jenis_kas, saldo, aktif FROM AkunKas ORDER BY id DESC";

                Koneksi();
                conn.Open();
                cmd = new SqlCommand(sql, conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    grid.Rows.Add(reader["id"], reader["nama"], reader["kategori_kas"], reader["jenis_kas"], reader["saldo"], reader["aktif"]);
                }
                reader.Close();

                cmd = new SqlCommand("SELECT COUNT(*) FROM AkunKas", conn);
                int jumlah = (int)cmd.ExecuteScalar();
                lblCount.Text = "Total: " + jumlah.ToString();

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
            DataGridViewRow row = grid.Rows[e.RowIndex];
            txtId.Text = Convert.ToString(row.Cells["id"].Value);
            txtName.Text = Convert.ToString(row.Cells["nama"].Value);
            txtBalance.Text = Convert.ToString(row.Cells["saldo"].Value);
            cmbCategory.SelectedItem = Convert.ToString(row.Cells["kategori_kas"].Value);
            cmbType.SelectedItem = Convert.ToString(row.Cells["jenis_kas"].Value);
            chkActive.Checked = Convert.ToBoolean(row.Cells["aktif"].Value);
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

            string sql = "INSERT INTO AkunKas(nama, kategori_kas, jenis_kas, saldo, aktif) VALUES (@name, @category, @type, @saldo, @active)";
            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureAkunKasSaldoColumn(conn);
                DbSchema.EnsureAkunKasKategoriColumn(conn);
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@category", cmbCategory.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@type", cmbType.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@saldo", saldo);
                int active = 1;
                if (chkActive.Checked == false) active = 0;
                cmd.Parameters.AddWithValue("@active", active);
                int rows = cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Berhasil insert: " + rows + " baris.");
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

            string sql = "UPDATE AkunKas SET nama=@name, kategori_kas=@category, jenis_kas=@type, saldo=@saldo, aktif=@active WHERE id=@id";
            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureAkunKasSaldoColumn(conn);
                DbSchema.EnsureAkunKasKategoriColumn(conn);
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", txtId.Text);
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@category", cmbCategory.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@type", cmbType.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@saldo", saldo);
                int active = 1;
                if (chkActive.Checked == false) active = 0;
                cmd.Parameters.AddWithValue("@active", active);
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

            string sql = "DELETE FROM AkunKas WHERE id=@id";
            try
            {
                Koneksi();
                conn.Open();
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", txtId.Text);
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
