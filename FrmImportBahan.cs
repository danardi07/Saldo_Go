using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using ExcelDataReader;

namespace SaldoGo
{
    public partial class FrmImportBahan : Form
    {
        private readonly string connectionString = KoneksiDb.koneksi;
        private string excelFilePath = "";
        private DataSet excelDataSet = null;

        SqlConnection conn;
        SqlCommand cmd;
        SqlTransaction trans;

        public FrmImportBahan()
        {
            InitializeComponent();
        }

        private void FrmImportBahan_Load(object sender, EventArgs e)
        {
            // Initialize DataGridView
            gridExcel.Columns.Clear();
            gridExcel.Columns.Add("Nama", "Nama");
            gridExcel.Columns.Add("Satuan", "Satuan");
            gridExcel.Columns.Add("Stok", "Stok");
            gridExcel.Columns.Add("Aktif", "Aktif");
            gridExcel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridExcel.ReadOnly = true;

            txtFilePath.Text = "";
            btnImport.Enabled = false;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls|All Files|*.*";
            openFileDialog.Title = "Pilih File Excel Bahan";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                excelFilePath = openFileDialog.FileName;
                txtFilePath.Text = excelFilePath;

                try
                {
                    // Baca file Excel menggunakan ExcelDataReader
                    using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet();
                            excelDataSet = result;

                            // Ambil sheet pertama
                            DataTable sheet = result.Tables[0];

                            // Tampilkan ke DataGridView
                            gridExcel.Rows.Clear();
                            int rowCount = 0;

                            foreach (DataRow row in sheet.Rows)
                            {
                                // Skip header (baris pertama)
                                if (rowCount == 0)
                                {
                                    rowCount++;
                                    continue;
                                }

                                // Skip baris kosong
                                if (IsRowEmpty(row))
                                {
                                    rowCount++;
                                    continue;
                                }

                                string nama = GetCellValue(row, 0);
                                string satuan = GetCellValue(row, 1);
                                string stokStr = GetCellValue(row, 2);
                                string aktifStr = GetCellValue(row, 3);

                                // Validasi dasar
                                if (string.IsNullOrWhiteSpace(nama) || string.IsNullOrWhiteSpace(satuan))
                                {
                                    rowCount++;
                                    continue;
                                }

                                gridExcel.Rows.Add(nama, satuan, stokStr, aktifStr);
                                rowCount++;
                            }
                        }
                    }

                    if (gridExcel.Rows.Count > 0)
                    {
                        btnImport.Enabled = true;
                        MessageBox.Show($"Berhasil membaca {gridExcel.Rows.Count} baris data dari Excel.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        btnImport.Enabled = false;
                        MessageBox.Show("Tidak ada data valid yang ditemukan di Excel.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error membaca file Excel: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnImport.Enabled = false;
                }
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (gridExcel.Rows.Count == 0)
            {
                MessageBox.Show("Tidak ada data untuk di-import.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(excelFilePath))
            {
                MessageBox.Show("Pilih file Excel terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int totalRows = gridExcel.Rows.Count;
            int successCount = 0;
            int skipCount = 0;

            trans = null;

            try
            {
                Koneksi();
                conn.Open();

                // Mulai transaction
                trans = conn.BeginTransaction();

                // Pastikan schema tersedia
                DbSchema.EnsureStokViewsAndProcedures(conn, trans);

                // Loop setiap baris di DataGridView
                foreach (DataGridViewRow row in gridExcel.Rows)
                {
                    string nama = Convert.ToString(row.Cells["Nama"].Value);
                    string satuan = Convert.ToString(row.Cells["Satuan"].Value);
                    string stokStr = Convert.ToString(row.Cells["Stok"].Value);
                    string aktifStr = Convert.ToString(row.Cells["Aktif"].Value);

                    // Validasi
                    if (string.IsNullOrWhiteSpace(nama) || string.IsNullOrWhiteSpace(satuan))
                    {
                        skipCount++;
                        continue;
                    }

                    // Parse stok
                    decimal stok = 0;
                    if (!string.IsNullOrWhiteSpace(stokStr) && decimal.TryParse(stokStr, out decimal parsedStok))
                    {
                        stok = parsedStok;
                    }
                    if (stok < 0) stok = 0;

                    // Parse aktif
                    bool aktif = true;
                    if (!string.IsNullOrWhiteSpace(aktifStr))
                    {
                        if (aktifStr.ToLower() == "0" || aktifStr.ToLower() == "false")
                        {
                            aktif = false;
                        }
                    }

                    // Cek duplikat (nama + satuan)
                    string checkDupSql = @"SELECT COUNT(*) FROM dbo.Bahan 
                                          WHERE LOWER(LTRIM(RTRIM(nama))) = LOWER(@nama) 
                                          AND LOWER(LTRIM(RTRIM(satuan))) = LOWER(@satuan)";
                    cmd = new SqlCommand(checkDupSql, conn, trans);
                    cmd.Parameters.AddWithValue("@nama", nama.Trim());
                    cmd.Parameters.AddWithValue("@satuan", satuan.Trim());
                    object countObj = cmd.ExecuteScalar();
                    int existingCount = 0;
                    if (countObj != null && countObj != DBNull.Value)
                    {
                        existingCount = Convert.ToInt32(countObj);
                    }

                    if (existingCount > 0)
                    {
                        // Skip jika sudah ada
                        skipCount++;
                        continue;
                    }

                    // Insert ke dbo.Bahan
                    string insertSql = @"INSERT INTO dbo.Bahan (nama, satuan, stok, aktif)
                                        VALUES (@nama, @satuan, @stok, @aktif)";
                    cmd = new SqlCommand(insertSql, conn, trans);
                    cmd.Parameters.AddWithValue("@nama", nama.Trim());
                    cmd.Parameters.AddWithValue("@satuan", satuan.Trim());
                    cmd.Parameters.AddWithValue("@stok", stok);
                    cmd.Parameters.AddWithValue("@aktif", aktif);
                    cmd.ExecuteNonQuery();

                    successCount++;
                }

                // Log aktivitas ke LogAktivitas
                if (successCount > 0)
                {
                    string logAktivitas = $"IMPORT EXCEL BAHAN : {successCount} DATA";
                    string sqlInsertLog = @"INSERT INTO dbo.LogAktivitas (aktivitas, waktu)
                                            VALUES (@aktivitas, @waktu)";
                    cmd = new SqlCommand(sqlInsertLog, conn, trans);
                    cmd.Parameters.AddWithValue("@aktivitas", logAktivitas);
                    cmd.Parameters.AddWithValue("@waktu", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }

                // Commit transaction
                trans.Commit();
                trans = null;

                conn.Close();

                string message = $"Import selesai.\n\nBerhasil: {successCount} data\nDilewati (duplikat/invalid): {skipCount} data";
                MessageBox.Show(message, "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear form
                gridExcel.Rows.Clear();
                txtFilePath.Text = "";
                excelFilePath = "";
                excelDataSet = null;
                btnImport.Enabled = false;
            }
            catch (SqlException ex)
            {
                // Rollback jika error SQL
                if (trans != null)
                {
                    try
                    {
                        trans.Rollback();
                    }
                    catch { }
                }

                MessageBox.Show("Error SQL: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try { if (conn != null && conn.State == ConnectionState.Open) conn.Close(); } catch { }
            }
            catch (Exception ex)
            {
                // Rollback jika error umum
                if (trans != null)
                {
                    try
                    {
                        trans.Rollback();
                    }
                    catch { }
                }

                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try { if (conn != null && conn.State == ConnectionState.Open) conn.Close(); } catch { }
            }
            finally
            {
                // Pastikan connection ditutup
                try
                {
                    if (conn != null && conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
                catch { }
            }
        }

        private void Koneksi()
        {
            conn = new SqlConnection(connectionString);
        }

        private bool IsRowEmpty(DataRow row)
        {
            foreach (var item in row.ItemArray)
            {
                if (item != null && !string.IsNullOrWhiteSpace(Convert.ToString(item)))
                {
                    return false;
                }
            }
            return true;
        }

        private string GetCellValue(DataRow row, int columnIndex)
        {
            try
            {
                if (columnIndex < row.ItemArray.Length)
                {
                    object value = row.ItemArray[columnIndex];
                    if (value != null && value != DBNull.Value)
                    {
                        return Convert.ToString(value);
                    }
                }
            }
            catch { }
            return "";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
