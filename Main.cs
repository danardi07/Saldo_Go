using System;
using System.Windows.Forms;

namespace SaldoGo
{
    public partial class Main : Form
    {
        private UserSession session;

        public Main()
        {
            InitializeComponent();
        }

        public Main(UserSession session) : this()
        {
            this.session = session;
        }

        private void Main_Load(object sender, EventArgs e)
        {
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            if (session == null)
            {
                MessageBox.Show("Session kosong.");
                Close();
                return;
            }

            lblLoginInfo.Text = "Login: " + session.FullName;

            btnRiwayat.Enabled = true;

            btnMenu.Enabled = session.IsOwner;
            btnStock.Enabled = session.IsOwner;
            btnImportBahan.Enabled = session.IsOwner;
            btnTarget.Enabled = session.IsOwner;
            btnKas.Enabled = session.IsOwner;
            btnTransfer.Enabled = session.IsOwner;
            btnMargin.Enabled = session.IsOwner;

            lblNote.Visible = false;
        }

        private void btnExpense_Click(object sender, EventArgs e)
        {
            Transaksi f = new Transaksi(session);
            f.ShowDialog(this);
            f.Dispose();
        }

        private void btnRiwayat_Click(object sender, EventArgs e)
        {
            RiwayatBon f = new RiwayatBon(session);
            f.ShowDialog(this);
            f.Dispose();
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            Menu f = new Menu(session);
            f.ShowDialog(this);
            f.Dispose();
        }

        private void btnKas_Click(object sender, EventArgs e)
        {
            AkunKas f = new AkunKas(session);
            f.ShowDialog(this);
            f.Dispose();
        }

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            Transfer f = new Transfer(session);
            f.ShowDialog(this);
            f.Dispose();
        }

        private void btnStock_Click(object sender, EventArgs e)
        {
            StokBahan f = new StokBahan(session);
            f.ShowDialog(this);
            f.Dispose();
        }

        private void btnImportBahan_Click(object sender, EventArgs e)
        {
            FrmImportBahan f = new FrmImportBahan();
            f.ShowDialog(this);
            f.Dispose();
        }

        private void btnTarget_Click(object sender, EventArgs e)
        {
            TargetOmzet f = new TargetOmzet(session);
            f.ShowDialog(this);
            f.Dispose();
        }

        private void btnMargin_Click(object sender, EventArgs e)
        {
            LaporanMargin f = new LaporanMargin(session);
            f.ShowDialog(this);
            f.Dispose();
        }

        private void btnConfigDatabase_Click(object sender, EventArgs e)
        {
            FrmConfigDatabase f = new FrmConfigDatabase();
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                MessageBox.Show("Konfigurasi database berhasil disimpan. Silakan restart aplikasi untuk menerapkan perubahan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            f.Dispose();
        }
    }
}
