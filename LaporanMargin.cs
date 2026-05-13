using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SaldoGo
{
    public class LaporanMargin : Form
    {
        private readonly UserSession session;
        private readonly string connectionString = KoneksiDb.koneksi;

        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataReader reader;

        private DateTimePicker dtFrom;
        private DateTimePicker dtTo;
        private Button btnRefresh;
        private DataGridView grid;

        public LaporanMargin() : this(null)
        {
        }

        public LaporanMargin(UserSession session)
        {
            this.session = session;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Label lblDari;
            Label lblSampai;

            this.SuspendLayout();

            this.Text = "Laporan Margin Laba";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(980, 560);
            this.Shown += new EventHandler(this.LaporanMargin_Shown);

            lblDari = new Label();
            lblDari.AutoSize = true;
            lblDari.Location = new Point(12, 15);
            lblDari.Name = "lblDari";
            lblDari.Size = new Size(30, 16);
            lblDari.TabIndex = 0;
            lblDari.Text = "Dari";
            this.Controls.Add(lblDari);

            this.dtFrom = new DateTimePicker();
            this.dtFrom.Format = DateTimePickerFormat.Short;
            this.dtFrom.Location = new Point(50, 10);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new Size(130, 22);
            this.dtFrom.TabIndex = 1;
            this.Controls.Add(this.dtFrom);

            lblSampai = new Label();
            lblSampai.AutoSize = true;
            lblSampai.Location = new Point(195, 15);
            lblSampai.Name = "lblSampai";
            lblSampai.Size = new Size(45, 16);
            lblSampai.TabIndex = 2;
            lblSampai.Text = "Sampai";
            this.Controls.Add(lblSampai);

            this.dtTo = new DateTimePicker();
            this.dtTo.Format = DateTimePickerFormat.Short;
            this.dtTo.Location = new Point(250, 10);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new Size(130, 22);
            this.dtTo.TabIndex = 3;
            this.Controls.Add(this.dtTo);

            this.btnRefresh = new Button();
            this.btnRefresh.Location = new Point(400, 8);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(110, 28);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new EventHandler(this.btnRefresh_Click);
            this.Controls.Add(this.btnRefresh);

            this.grid = new DataGridView();
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.Location = new Point(12, 50);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new Size(940, 450);
            this.grid.TabIndex = 5;
            this.grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(this.grid);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void Koneksi()
        {
            conn = new SqlConnection(connectionString);
        }

        private void LaporanMargin_Shown(object sender, EventArgs e)
        {
            if (session == null)
            {
                MessageBox.Show("Session kosong.");
                Close();
                return;
            }

            if (!session.IsOwner)
            {
                MessageBox.Show("Akses ditolak: hanya Pemilik yang boleh melihat laporan margin.");
                Close();
                return;
            }

            dtFrom.Value = DateTime.Today;
            dtTo.Value = DateTime.Today;

            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                grid.Columns.Clear();
                grid.Rows.Clear();

                grid.Columns.Add("menu", "Menu");
                grid.Columns.Add("qty", "Qty");
                grid.Columns.Add("omzet", "Omzet");
                grid.Columns.Add("hpp", "Estimasi HPP");
                grid.Columns.Add("laba", "Laba");
                grid.Columns.Add("margin", "Margin %");

                DateTime from = dtFrom.Value.Date;
                DateTime to = dtTo.Value.Date.AddDays(1);

                List<(string ket, decimal nominal)> sales = new List<(string ket, decimal nominal)>();

                Koneksi();
                conn.Open();

                DbSchema.EnsureLaporanViews(conn);

                string sqlSales = @"
 SELECT keterangan, nominal
 FROM dbo.v_TransaksiPenjualan
 WHERE waktu_transaksi >= @from
   AND waktu_transaksi < @to";

                cmd = new SqlCommand(sqlSales, conn);
                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@to", to);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sales.Add((Convert.ToString(reader["keterangan"]), Convert.ToDecimal(reader["nominal"])));
                }
                reader.Close();

                Dictionary<string, decimal?> hppByMenu = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);

                cmd = new SqlCommand("SELECT nama, perkiraan_modal FROM dbo.v_MenuHpp", conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string nama = Convert.ToString(reader["nama"]);
                    decimal? hpp = reader["perkiraan_modal"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["perkiraan_modal"]);
                    if (!hppByMenu.ContainsKey(nama))
                        hppByMenu.Add(nama, hpp);
                }
                reader.Close();

                conn.Close();

                Regex rx = new Regex(@"^Penjualan:\s*(.+?)\s+x(\d+)\b", RegexOptions.IgnoreCase);

                var aggQty = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var aggOmzet = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                foreach (var s in sales)
                {
                    if (s.ket == null) continue;
                    Match m = rx.Match(s.ket);
                    if (!m.Success) continue;

                    string menu = m.Groups[1].Value.Trim();
                    int qty = 0;
                    int.TryParse(m.Groups[2].Value, out qty);
                    if (qty <= 0) qty = 1;

                    if (!aggQty.ContainsKey(menu)) aggQty[menu] = 0;
                    if (!aggOmzet.ContainsKey(menu)) aggOmzet[menu] = 0m;

                    aggQty[menu] += qty;
                    aggOmzet[menu] += s.nominal;
                }

                foreach (var kv in aggQty)
                {
                    string menu = kv.Key;
                    int qty = kv.Value;
                    decimal omzet = aggOmzet.ContainsKey(menu) ? aggOmzet[menu] : 0m;

                    decimal hppUnit = 0m;
                    if (hppByMenu.ContainsKey(menu) && hppByMenu[menu].HasValue)
                        hppUnit = hppByMenu[menu].Value;

                    decimal hpp = hppUnit * qty;
                    decimal laba = omzet - hpp;
                    decimal margin = omzet == 0 ? 0 : (laba / omzet) * 100m;

                    grid.Rows.Add(menu, qty, omzet, hpp, laba, margin.ToString("0.##"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (reader != null && !reader.IsClosed) reader.Close(); } catch { }
                try { if (conn != null) conn.Close(); } catch { }
            }
        }
    }
}
