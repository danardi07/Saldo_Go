using System;
using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SaldoGo
{
    public partial class StatusKoneksi : Form
    {
        private readonly string connectionString = KoneksiDb.koneksi;

        SqlConnection conn;
        SqlCommand cmd;

        public StatusKoneksi()
        {
            InitializeComponent();
        }

        private void Koneksi()
        {
            conn = new SqlConnection(connectionString);
        }

        private void StatusKoneksi_Shown(object sender, EventArgs e)
        {
            CheckConnection();
        }

        private void btnRetry_Click(object sender, EventArgs e)
        {
            CheckConnection();
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            Hide();
            Login f = new Login();
            f.ShowDialog(this);
            f.Dispose();
            Close();
        }

        private void CheckConnection()
        {
            btnContinue.Enabled = false;

            bool ok;

            try
            {
                Koneksi();
                conn.Open();
                ok = true;
                conn.Close();
            }
            catch (Exception ex)
            {
                ok = false;

                try
                {
                    if (conn != null) conn.Close();
                }
                catch { }
            }

            if (ok)
            {
                lblStatus.Text = "berhasil terhubung";
            }
            else
            {
                lblStatus.Text = "gagal terhubung";
            }

            btnContinue.Enabled = ok;
        }
    }
}
