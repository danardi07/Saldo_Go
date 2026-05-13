using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SaldoGo
{
    public class TargetOmzet : Form
    {
        private readonly UserSession session;
        private readonly string connectionString = KoneksiDb.koneksi;

        private readonly BindingSource historyBindingSource = new BindingSource();
        private DataTable historyTable;
        private BindingNavigator historyNavigator;

        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataReader reader;

        private DateTimePicker dtTanggal;
        private TextBox txtTarget;
        private Button btnSave;
        private Button btnRefresh;
        private DataGridView grid;

        public TargetOmzet() : this(null)
        {
        }

        public TargetOmzet(UserSession session)
        {
            this.session = session;
            InitializeComponent();

            SetupHistoryBinding();
            InputValidation.AttachDecimalOnly(txtTarget, "Target");
        }

        private void SetupHistoryBinding()
        {
            grid.AutoGenerateColumns = true;
            grid.DataSource = historyBindingSource;
            historyNavigator = new BindingNavigator(true);
            historyNavigator.BindingSource = historyBindingSource;
            historyNavigator.Location = new Point(12, 470);
            historyNavigator.Size = new Size(810, 27);
            historyNavigator.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.Controls.Add(historyNavigator);
        }

        private void InitializeComponent()
        {
            Label lblTanggal;
            Label lblTarget;

            this.SuspendLayout();

            this.Text = "Target Omzet Harian";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(860, 520);
            this.Shown += new EventHandler(this.TargetOmzet_Shown);

            lblTanggal = new Label();
            lblTanggal.AutoSize = true;
            lblTanggal.Location = new Point(12, 15);
            lblTanggal.Name = "lblTanggal";
            lblTanggal.Size = new Size(55, 16);
            lblTanggal.TabIndex = 0;
            lblTanggal.Text = "Tanggal";
            this.Controls.Add(lblTanggal);

            this.dtTanggal = new DateTimePicker();
            this.dtTanggal.Format = DateTimePickerFormat.Short;
            this.dtTanggal.Location = new Point(70, 10);
            this.dtTanggal.Name = "dtTanggal";
            this.dtTanggal.Size = new Size(160, 22);
            this.dtTanggal.TabIndex = 1;
            this.Controls.Add(this.dtTanggal);

            lblTarget = new Label();
            lblTarget.AutoSize = true;
            lblTarget.Location = new Point(250, 15);
            lblTarget.Name = "lblTarget";
            lblTarget.Size = new Size(44, 16);
            lblTarget.TabIndex = 2;
            lblTarget.Text = "Target";
            this.Controls.Add(lblTarget);

            this.txtTarget = new TextBox();
            this.txtTarget.Location = new Point(305, 10);
            this.txtTarget.Name = "txtTarget";
            this.txtTarget.Size = new Size(140, 22);
            this.txtTarget.TabIndex = 3;
            this.Controls.Add(this.txtTarget);

            this.btnSave = new Button();
            this.btnSave.Location = new Point(460, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(110, 28);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Simpan";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            this.Controls.Add(this.btnSave);

            this.btnRefresh = new Button();
            this.btnRefresh.Location = new Point(580, 8);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(110, 28);
            this.btnRefresh.TabIndex = 5;
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
            this.grid.Size = new Size(810, 410);
            this.grid.TabIndex = 6;
            this.grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(this.grid);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveTarget();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadHistory();
        }

        private void Koneksi()
        {
            conn = new SqlConnection(connectionString);
        }

        private void TargetOmzet_Shown(object sender, EventArgs e)
        {
            if (session == null)
            {
                MessageBox.Show("Session kosong.");
                Close();
                return;
            }

            if (!session.IsOwner)
            {
                MessageBox.Show("Akses ditolak: hanya Pemilik yang boleh mengatur target omzet.");
                Close();
                return;
            }

            EnsureSchema();
            LoadHistory();
        }

        private void EnsureSchema()
        {
            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureTargetOmzetTable(conn);
                DbSchema.EnsureTargetOmzetViewsAndProcedures(conn);
                conn.Close();
            }
            catch
            {
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private bool ValidateInput(out decimal target)
        {
            target = 0m;
            if (!decimal.TryParse(txtTarget.Text, out target) || target <= 0)
            {
                MessageBox.Show("Target harus angka dan > 0.");
                txtTarget.Focus();
                return false;
            }
            return true;
        }

        private void SaveTarget()
        {
            decimal target;
            if (!ValidateInput(out target)) return;

            DateTime tanggal = dtTanggal.Value.Date;

            try
            {
                Koneksi();
                conn.Open();
                DbSchema.EnsureTargetOmzetTable(conn);

                DbSchema.EnsureTargetOmzetViewsAndProcedures(conn);

                cmd = new SqlCommand("dbo.sp_TargetOmzet_Save", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tanggal", tanggal);
                cmd.Parameters.AddWithValue("@target", target);
                cmd.Parameters.AddWithValue("@userId", session.UserId);
                cmd.ExecuteNonQuery();

                conn.Close();

                MessageBox.Show("Target omzet tersimpan.");
                LoadHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try { if (conn != null) conn.Close(); } catch { }
            }
        }

        private void LoadHistory()
        {
            try
            {
                Koneksi();
                conn.Open();

                DbSchema.EnsureTargetOmzetTable(conn);

                DbSchema.EnsureTargetOmzetViewsAndProcedures(conn);

                cmd = new SqlCommand("dbo.sp_TargetOmzet_History", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@maxRows", 60);

                historyTable = new DataTable();
                reader = cmd.ExecuteReader();
                historyTable.Load(reader);
                reader.Close();

                historyBindingSource.DataSource = historyTable;

                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                conn.Close();
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
