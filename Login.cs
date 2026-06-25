using System;
using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SaldoGo
{
    public partial class Login : Form
    {
        private readonly string connectionString = KoneksiDb.koneksi;

        SqlConnection conn;
        SqlCommand cmd;
        SqlDataReader reader;

        public Login()
        {
            InitializeComponent();
            AcceptButton = btnLogin;
        }

        private void Koneksi()
        {
            conn = new SqlConnection(connectionString);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            DoLogin();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DoLogin()
        {
            lblInfo.Text = "";

            string username = txtUsername.Text;
            string password = txtPassword.Text;

            username = username.Trim();
            password = password.Trim();

            if (username == "" || password == "")
            {
                lblInfo.Text = "Nama pengguna dan kata sandi wajib diisi.";
                return;
            }

            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureAuthProcedures(conn);

                cmd = new SqlCommand("dbo.sp_User_Login", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    lblInfo.Text = "Login gagal. Cek nama pengguna/kata sandi.";
                    reader.Close();
                    conn.Close();
                    return;
                }

                UserSession session = new UserSession();
                session.UserId = Convert.ToInt64(reader["id"]);
                session.Username = Convert.ToString(reader["username"]);
                session.FullName = Convert.ToString(reader["full_name"]);
                session.RoleName = Convert.ToString(reader["role_name"]);

                reader.Close();
                conn.Close();

                Hide();
                Main f = new Main(session);
                f.ShowDialog(this);
                f.Dispose();
                Close();
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
    }
}
