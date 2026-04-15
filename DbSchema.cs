using System.Data.SqlClient;

namespace SaldoGo
{
    internal static class DbSchema
    {
        public static void EnsureAkunKasSaldoColumn(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF COL_LENGTH('AkunKas', 'saldo') IS NULL
BEGIN
    ALTER TABLE AkunKas
    ADD saldo DECIMAL(18,2) NOT NULL
        CONSTRAINT DF_AkunKas_saldo DEFAULT(0);
END";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void EnsureAkunKasKategoriColumn(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF COL_LENGTH('AkunKas', 'kategori_kas') IS NULL
BEGIN
    ALTER TABLE AkunKas
    ADD kategori_kas NVARCHAR(20) NOT NULL
        CONSTRAINT DF_AkunKas_kategori_kas DEFAULT(N'LACI');
END";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void EnsureStokTables(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('Bahan', 'U') IS NULL
BEGIN
    CREATE TABLE Bahan (
        id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bahan PRIMARY KEY,
        nama NVARCHAR(150) NOT NULL,
        satuan NVARCHAR(50) NOT NULL,
        stok DECIMAL(18,2) NOT NULL CONSTRAINT DF_Bahan_stok DEFAULT(0),
        aktif BIT NOT NULL CONSTRAINT DF_Bahan_aktif DEFAULT(1)
    );
END

IF OBJECT_ID('MutasiStok', 'U') IS NULL
BEGIN
    CREATE TABLE MutasiStok (
        id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MutasiStok PRIMARY KEY,
        waktu DATETIME2 NOT NULL,
        tipe NVARCHAR(20) NOT NULL, /* MASUK / KELUAR */
        bahan_id BIGINT NOT NULL,
        qty DECIMAL(18,2) NOT NULL,
        total_biaya DECIMAL(18,2) NULL,
        keterangan NVARCHAR(255) NULL,
        dibuat_oleh_pengguna_id BIGINT NOT NULL,
        CONSTRAINT FK_MutasiStok_Bahan FOREIGN KEY (bahan_id) REFERENCES Bahan(id),
        CONSTRAINT FK_MutasiStok_User FOREIGN KEY (dibuat_oleh_pengguna_id) REFERENCES [User](id)
    );

    CREATE INDEX IX_MutasiStok_waktu ON MutasiStok(waktu DESC);
    CREATE INDEX IX_MutasiStok_bahan ON MutasiStok(bahan_id);
END";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void EnsureTargetOmzetTable(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('TargetOmzetHarian', 'U') IS NULL
BEGIN
    CREATE TABLE TargetOmzetHarian (
        tanggal DATE NOT NULL CONSTRAINT PK_TargetOmzetHarian PRIMARY KEY,
        target_nominal DECIMAL(18,2) NOT NULL,
        dibuat_pada DATETIME2 NOT NULL,
        dibuat_oleh_pengguna_id BIGINT NOT NULL,
        CONSTRAINT FK_TargetOmzet_User FOREIGN KEY (dibuat_oleh_pengguna_id) REFERENCES [User](id)
    );
END";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public static void EnsureHutangTable(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('HutangPelanggan', 'U') IS NULL
BEGIN
    CREATE TABLE HutangPelanggan (
        id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HutangPelanggan PRIMARY KEY,
        waktu_dibuat DATETIME2 NOT NULL,
        nama_pelanggan NVARCHAR(150) NOT NULL,
        nominal DECIMAL(18,2) NOT NULL,
        keterangan NVARCHAR(255) NULL,
        jatuh_tempo DATE NULL,
        status NVARCHAR(20) NOT NULL CONSTRAINT DF_Hutang_status DEFAULT(N'BELUM_LUNAS'),
        dilunasi_pada DATETIME2 NULL,
        dibuat_oleh_pengguna_id BIGINT NOT NULL,
        dilunasi_oleh_pengguna_id BIGINT NULL,
        CONSTRAINT FK_Hutang_User FOREIGN KEY (dibuat_oleh_pengguna_id) REFERENCES [User](id),
        CONSTRAINT FK_Hutang_LunasUser FOREIGN KEY (dilunasi_oleh_pengguna_id) REFERENCES [User](id)
    );

    CREATE INDEX IX_Hutang_status ON HutangPelanggan(status);
    CREATE INDEX IX_Hutang_waktu ON HutangPelanggan(waktu_dibuat DESC);
END";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
