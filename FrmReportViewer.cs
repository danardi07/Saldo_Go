using System;
using System.Data;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Windows.Forms;

namespace SaldoGo
{
    public partial class FrmReportViewer : Form
    {
        private CrystalReportViewer crystalReportViewer;
        private ReportDocument reportDocument;

        public FrmReportViewer()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.crystalReportViewer = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
            this.SuspendLayout();
            // 
            // crystalReportViewer
            // 
            this.crystalReportViewer.ActiveViewIndex = -1;
            this.crystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crystalReportViewer.Cursor = System.Windows.Forms.Cursors.Default;
            this.crystalReportViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.crystalReportViewer.Location = new System.Drawing.Point(0, 0);
            this.crystalReportViewer.Name = "crystalReportViewer";
            this.crystalReportViewer.Size = new System.Drawing.Size(876, 535);
            this.crystalReportViewer.TabIndex = 0;
            // 
            // FrmReportViewer
            // 
            this.ClientSize = new System.Drawing.Size(876, 535);
            this.Controls.Add(this.crystalReportViewer);
            this.Name = "FrmReportViewer";
            this.Text = "Report Viewer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmReportViewer_FormClosing);
            this.ResumeLayout(false);

        }

        private void FrmReportViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (reportDocument != null)
            {
                reportDocument.Close();
                reportDocument.Dispose();
            }
        }

        public void LoadMarginLabaReport(DataTable dt, DateTime tanggalMulai, DateTime tanggalSelesai)
        {
            try
            {
                reportDocument = new ReportDocument();
                string reportPath = System.IO.Path.Combine(Application.StartupPath, "rptMarginLaba.rpt");
                
                if (!System.IO.File.Exists(reportPath))
                {
                    reportPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "rptMarginLaba.rpt");
                }
                
                if (!System.IO.File.Exists(reportPath))
                {
                    reportPath = System.IO.Path.Combine(System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName, "rptMarginLaba.rpt");
                }
                
                if (!System.IO.File.Exists(reportPath))
                {
                    MessageBox.Show("File report tidak ditemukan. Pastikan:\n1. rptMarginLaba.rpt ada di project folder\n2. Properties rptMarginLaba.rpt: Copy to Output Directory = Copy always\n\nPath yang dicari:\n" + reportPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                reportDocument.Load(reportPath);
                reportDocument.SetDataSource(dt);

                try
                {
                    reportDocument.SetParameterValue("tanggal_mulai", tanggalMulai);
                    reportDocument.SetParameterValue("tanggal_selesai", tanggalSelesai);
                    reportDocument.SetParameterValue("tanggal_cetak", DateTime.Now);
                }
                catch
                {
                    // Parameter belum dibuat di Crystal Report, abaikan sementara
                }

                crystalReportViewer.ReportSource = reportDocument;
            }
            catch (Exception ex)
            {
                string msg = ex.ToString();

                if (ex.InnerException != null)
                {
                    msg += "\n\nINNER:\n" + ex.InnerException.ToString();
                }

                MessageBox.Show(msg);
            }
        }
    }
}
