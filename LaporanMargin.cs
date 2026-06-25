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
        private Button btnCetak;
        private Label lblDari;
        private Label lblSampai;
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
            this.lblDari = new System.Windows.Forms.Label();
            this.lblSampai = new System.Windows.Forms.Label();
            this.dtFrom = new System.Windows.Forms.DateTimePicker();
            this.dtTo = new System.Windows.Forms.DateTimePicker();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnCetak = new System.Windows.Forms.Button();
            this.grid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // lblDari
            // 
            this.lblDari.AutoSize = true;
            this.lblDari.Location = new System.Drawing.Point(12, 15);
            this.lblDari.Name = "lblDari";
            this.lblDari.Size = new System.Drawing.Size(32, 16);
            this.lblDari.TabIndex = 0;
            this.lblDari.Text = "Dari";
            // 
            // lblSampai
            // 
            this.lblSampai.AutoSize = true;
            this.lblSampai.Location = new System.Drawing.Point(195, 15);
            this.lblSampai.Name = "lblSampai";
            this.lblSampai.Size = new System.Drawing.Size(54, 16);
            this.lblSampai.TabIndex = 2;
            this.lblSampai.Text = "Sampai";
            // 
            // dtFrom
            // 
            this.dtFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtFrom.Location = new System.Drawing.Point(50, 10);
            this.dtFrom.Name = "dtFrom";
            this.dtFrom.Size = new System.Drawing.Size(130, 22);
            this.dtFrom.TabIndex = 1;
            // 
            // dtTo
            // 
            this.dtTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtTo.Location = new System.Drawing.Point(250, 10);
            this.dtTo.Name = "dtTo";
            this.dtTo.Size = new System.Drawing.Size(130, 22);
            this.dtTo.TabIndex = 3;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(400, 8);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(110, 28);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnCetak
            // 
            this.btnCetak.Location = new System.Drawing.Point(520, 8);
            this.btnCetak.Name = "btnCetak";
            this.btnCetak.Size = new System.Drawing.Size(110, 28);
            this.btnCetak.TabIndex = 6;
            this.btnCetak.Text = "Cetak";
            this.btnCetak.UseVisualStyleBackColor = true;
            this.btnCetak.Click += new System.EventHandler(this.btnCetak_Click);
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.ColumnHeadersHeight = 29;
            this.grid.Location = new System.Drawing.Point(12, 50);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RowHeadersWidth = 51;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(940, 450);
            this.grid.TabIndex = 5;
            // 
            // LaporanMargin
            // 
            this.ClientSize = new System.Drawing.Size(980, 560);
            this.Controls.Add(this.lblDari);
            this.Controls.Add(this.dtFrom);
            this.Controls.Add(this.lblSampai);
            this.Controls.Add(this.dtTo);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnCetak);
            this.Controls.Add(this.grid);
            this.Name = "LaporanMargin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Laporan Margin Laba";
            this.Shown += new System.EventHandler(this.LaporanMargin_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
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

                DataTable dt = GetMarginLabaDataTable(from, to);

                foreach (DataRow row in dt.Rows)
                {
                    grid.Rows.Add(
                        row["Menu"],
                        row["Qty"],
                        row["Omzet"],
                        row["HPP"],
                        row["Laba"],
                        Convert.ToDecimal(row["MarginPersen"]).ToString("0.##")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (reader != null && !reader.IsClosed) reader.Close(); } catch { }
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private DataTable GetMarginLabaDataTable(DateTime from, DateTime to)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Menu", typeof(string));
            dt.Columns.Add("Qty", typeof(int));
            dt.Columns.Add("Omzet", typeof(decimal));
            dt.Columns.Add("HPP", typeof(decimal));
            dt.Columns.Add("Laba", typeof(decimal));
            dt.Columns.Add("MarginPersen", typeof(decimal));

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

            Regex rx = new Regex(@"^Penjualan:\s*(.+?)\s*x(\d+)\b", RegexOptions.IgnoreCase);

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

                dt.Rows.Add(menu, qty, omzet, hpp, laba, margin);
            }

            return dt;
        }

        private void btnCetak_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime from = dtFrom.Value.Date;
                DateTime to = dtTo.Value.Date.AddDays(1);

                DataTable dt = GetMarginLabaDataTable(from, to);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk periode ini.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                FrmReportViewer viewer = new FrmReportViewer();
                viewer.LoadMarginLabaReport(dt, from, to.AddDays(-1));
                viewer.ShowDialog(this);
                viewer.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
