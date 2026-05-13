using System;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SaldoGo
{
    public partial class Transfer : Form
    {
        private UserSession session;

        private readonly string connectionString = KoneksiDb.koneksi;

        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader reader;

        public Transfer()
        {
            InitializeComponent();

            InputValidation.AttachDecimalOnly(txtAmount, "Nominal");
        }

        public Transfer(UserSession session) : this()
        {
            this.session = session;
        }

        private void Koneksi()
        {
            conn = new SqlConnection(connectionString);
        }

        private void Transfer_Shown(object sender, EventArgs e)
        {
            if (session == null)
            {
                MessageBox.Show("Session kosong.");
                Close();
                return;
            }

            if (!session.IsOwner)
            {
                MessageBox.Show("Akses ditolak: hanya Pemilik yang boleh transfer antar kas.");
                Close();
                return;
            }

            LoadCash();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveTransfer();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LoadCash()
        {
            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureAkunKasSaldoColumn(conn);
                DbSchema.EnsureAkunKasKategoriColumn(conn);

                DbSchema.EnsureAkunKasViewsAndProcedures(conn);

                string sql = "SELECT id, display_name FROM dbo.v_AkunKasActive ORDER BY nama";

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

                cmbSource.DataSource = dt.Copy();
                cmbSource.DisplayMember = "display_name";
                cmbSource.ValueMember = "id";

                cmbDest.DataSource = dt;
                cmbDest.DisplayMember = "display_name";
                cmbDest.ValueMember = "id";

                reader.Close();
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

        private bool ValidateInput(out decimal amount)
        {
            amount = 0m;

            if (cmbSource.SelectedValue == null || cmbDest.SelectedValue == null)
            {
                MessageBox.Show("Kas sumber dan tujuan wajib dipilih.");
                return false;
            }

            if (Convert.ToInt64(cmbSource.SelectedValue) == Convert.ToInt64(cmbDest.SelectedValue))
            {
                MessageBox.Show("Kas sumber dan tujuan tidak boleh sama.");
                return false;
            }

            if (!decimal.TryParse(txtAmount.Text, out amount) || amount <= 0)
            {
                MessageBox.Show("Nominal harus angka dan > 0.");
                return false;
            }

            if (txtDesc.Text.Trim() == "")
            {
                MessageBox.Show("Keterangan wajib diisi.");
                return false;
            }

            return true;
        }

        private void SaveTransfer()
        {
            if (session == null)
            {
                MessageBox.Show("Session kosong.");
                return;
            }

            decimal amount;
            if (!ValidateInput(out amount)) return;

            DialogResult confirm = MessageBox.Show("Yakin simpan transfer ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureAkunKasSaldoColumn(conn);
                DbSchema.EnsureAkunKasKategoriColumn(conn);
                DbSchema.EnsureAkunKasViewsAndProcedures(conn);
                DbSchema.EnsureTransferProcedures(conn);

                cmd = new SqlCommand("dbo.sp_Transfer_Save", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@desc", txtDesc.Text.Trim());
                cmd.Parameters.AddWithValue("@source_id", Convert.ToInt64(cmbSource.SelectedValue));
                cmd.Parameters.AddWithValue("@dest_id", Convert.ToInt64(cmbDest.SelectedValue));
                cmd.Parameters.AddWithValue("@userId", session.UserId);

                SqlParameter outTrxId = new SqlParameter("@new_transaksi_id", SqlDbType.BigInt);
                outTrxId.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outTrxId);

                int rows = cmd.ExecuteNonQuery();
                long trxId = 0;
                if (outTrxId.Value != null && outTrxId.Value != DBNull.Value)
                {
                    trxId = Convert.ToInt64(outTrxId.Value);
                }

                conn.Close();

                MessageBox.Show("Berhasil simpan transfer: " + rows + " baris. ID Transaksi: " + trxId);
                txtAmount.Text = "";
                txtDesc.Text = "";
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
    }
}
