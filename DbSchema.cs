using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SaldoGo
{
    internal static class DbSchema
    {
        private static void ExecuteBatches(SqlConnection conn, SqlTransaction tx, string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return;

            string[] batches = Regex.Split(sql, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            foreach (string batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;

                using (SqlCommand cmd = new SqlCommand(batch, conn, tx))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void EnsureKategoriMenuViewsAndProcedures(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('dbo.v_KategoriMenu_Default', 'V') IS NULL
    EXEC('CREATE VIEW dbo.v_KategoriMenu_Default AS SELECT 1 AS dummy');

GO

ALTER VIEW dbo.v_KategoriMenu_Default
AS
SELECT MIN(id) AS id,
       MAX(nama) AS nama
FROM dbo.KategoriMenu
WHERE LOWER(LTRIM(RTRIM(nama))) IN ('makanan', 'minuman')
GROUP BY LOWER(LTRIM(RTRIM(nama)));

GO

IF OBJECT_ID('dbo.sp_KategoriMenu_EnsureDefaults', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_KategoriMenu_EnsureDefaults AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_KategoriMenu_EnsureDefaults
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.KategoriMenu WHERE LOWER(LTRIM(RTRIM(nama))) = 'makanan')
        INSERT INTO dbo.KategoriMenu(nama) VALUES (N'Makanan');

    IF NOT EXISTS (SELECT 1 FROM dbo.KategoriMenu WHERE LOWER(LTRIM(RTRIM(nama))) = 'minuman')
        INSERT INTO dbo.KategoriMenu(nama) VALUES (N'Minuman');

    SELECT id, nama
    FROM dbo.v_KategoriMenu_Default
    ORDER BY nama;
END";

            ExecuteBatches(conn, tx, sql);

        }

        public static void EnsurePenjualanProcedures(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('dbo.sp_Penjualan_Save', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Penjualan_Save AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Penjualan_Save
    @paymentType NVARCHAR(20),
    @qty INT,
    @amount DECIMAL(18,2),
    @desc NVARCHAR(255),
    @userId BIGINT,
    @new_transaksi_id BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @paymentType = UPPER(LTRIM(RTRIM(ISNULL(@paymentType, N''))));
    SET @desc = LTRIM(RTRIM(ISNULL(@desc, N'')));

    IF (@paymentType NOT IN (N'CASH', N'QRIS')) THROW 56001, 'Metode pembayaran tidak valid.', 1;
    IF (@qty IS NULL OR @qty <= 0) THROW 56002, 'Qty tidak valid.', 1;
    IF (@amount IS NULL OR @amount <= 0) THROW 56003, 'Nominal tidak valid.', 1;
    IF (@desc = N'') THROW 56004, 'Deskripsi transaksi wajib diisi.', 1;

    DECLARE @destCashId BIGINT;
    SELECT TOP 1 @destCashId = id
    FROM dbo.v_AkunKasActive
    WHERE UPPER(jenis_kas) = @paymentType
    ORDER BY id;

    IF (@destCashId IS NULL OR @destCashId <= 0)
        THROW 56005, 'Akun kas tujuan belum ada / belum aktif.', 1;

    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO dbo.Transaksi(waktu_transaksi, tipe_transaksi, nominal, keterangan, akun_kas_sumber_id, akun_kas_tujuan_id, dibuat_oleh_pengguna_id)
        VALUES (SYSDATETIME(), N'PEMASUKAN', @amount, @desc, NULL, @destCashId, @userId);

        SET @new_transaksi_id = CONVERT(BIGINT, SCOPE_IDENTITY());

        UPDATE dbo.AkunKas SET saldo = saldo + @amount WHERE id = @destCashId;

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END";

            ExecuteBatches(conn, tx, sql);
        }




        public static void EnsureAkunKasViewsAndProcedures(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('dbo.v_AkunKasList', 'V') IS NULL
    EXEC('CREATE VIEW dbo.v_AkunKasList AS SELECT 1 AS dummy');

GO

ALTER VIEW dbo.v_AkunKasList
AS
SELECT a.id,
       a.nama,
       a.kategori_kas,
       a.jenis_kas,
       a.saldo,
       a.aktif,
       (a.nama + N' [' + ISNULL(a.kategori_kas, N'') + N'/' + ISNULL(a.jenis_kas, N'') + N']') AS display_name,
       (a.nama + N' [' + ISNULL(a.kategori_kas, N'') + N'/' + ISNULL(a.jenis_kas, N'') + N'] - Saldo: ' + CAST(a.saldo AS NVARCHAR(50))) AS display_name_with_saldo
FROM dbo.AkunKas a;

GO

IF OBJECT_ID('dbo.v_AkunKasActive', 'V') IS NULL
    EXEC('CREATE VIEW dbo.v_AkunKasActive AS SELECT 1 AS dummy');

GO

ALTER VIEW dbo.v_AkunKasActive
AS
SELECT id, nama, kategori_kas, jenis_kas, saldo, aktif, display_name, display_name_with_saldo
FROM dbo.v_AkunKasList
WHERE aktif = 1;

GO

IF OBJECT_ID('dbo.sp_AkunKas_Search', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_AkunKas_Search AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_AkunKas_Search
    @q NVARCHAR(150) = NULL,
    @kategori_kas NVARCHAR(20) = NULL,
    @jenis_kas NVARCHAR(20) = NULL,
    @aktif BIT = NULL,
    @maxRows INT = 500
AS
BEGIN
    SET NOCOUNT ON;

    SET @q = LTRIM(RTRIM(ISNULL(@q, N'')));
    SET @kategori_kas = NULLIF(LTRIM(RTRIM(ISNULL(@kategori_kas, N''))), N'');
    SET @jenis_kas = NULLIF(LTRIM(RTRIM(ISNULL(@jenis_kas, N''))), N'');
    IF (@maxRows IS NULL OR @maxRows <= 0 OR @maxRows > 2000) SET @maxRows = 500;

    SELECT TOP (@maxRows)
           id, nama, kategori_kas, jenis_kas, saldo, aktif, display_name, display_name_with_saldo
    FROM dbo.v_AkunKasList
    WHERE (@aktif IS NULL OR aktif = @aktif)
      AND (@kategori_kas IS NULL OR kategori_kas = @kategori_kas)
      AND (@jenis_kas IS NULL OR jenis_kas = @jenis_kas)
      AND (@q = N'' OR nama LIKE N'%' + @q + N'%')
    ORDER BY id DESC;

    SELECT COUNT(*) AS total
    FROM dbo.v_AkunKasList
    WHERE (@aktif IS NULL OR aktif = @aktif)
      AND (@kategori_kas IS NULL OR kategori_kas = @kategori_kas)
      AND (@jenis_kas IS NULL OR jenis_kas = @jenis_kas)
      AND (@q = N'' OR nama LIKE N'%' + @q + N'%');
END

GO

IF OBJECT_ID('dbo.sp_AkunKas_Insert', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_AkunKas_Insert AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_AkunKas_Insert
    @nama NVARCHAR(150),
    @kategori_kas NVARCHAR(20),
    @jenis_kas NVARCHAR(20),
    @saldo DECIMAL(18,2) = 0,
    @aktif BIT = 1,
    @new_id BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @nama = LTRIM(RTRIM(ISNULL(@nama, N'')));
    SET @kategori_kas = UPPER(LTRIM(RTRIM(ISNULL(@kategori_kas, N''))));
    SET @jenis_kas = UPPER(LTRIM(RTRIM(ISNULL(@jenis_kas, N''))));

    IF (@nama = N'') THROW 51001, 'Nama akun kas wajib diisi.', 1;
    IF (@kategori_kas NOT IN (N'LACI', N'REKENING', N'EWALLET')) THROW 51002, 'Kategori kas tidak valid.', 1;
    IF (@jenis_kas NOT IN (N'CASH', N'QRIS')) THROW 51003, 'Jenis kas tidak valid.', 1;
    IF (@saldo IS NULL OR @saldo < 0) THROW 51004, 'Saldo harus >= 0.', 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.AkunKas
        WHERE LOWER(LTRIM(RTRIM(nama))) = LOWER(@nama)
          AND UPPER(LTRIM(RTRIM(ISNULL(kategori_kas, N'')))) = @kategori_kas
          AND UPPER(LTRIM(RTRIM(ISNULL(jenis_kas, N'')))) = @jenis_kas
    )
        THROW 51005, 'Akun kas dengan nama/kategori/jenis yang sama sudah ada.', 1;

    INSERT INTO dbo.AkunKas(nama, kategori_kas, jenis_kas, saldo, aktif)
    VALUES (@nama, @kategori_kas, @jenis_kas, @saldo, @aktif);

    SET @new_id = CONVERT(BIGINT, SCOPE_IDENTITY());
END

GO

IF OBJECT_ID('dbo.sp_AkunKas_Update', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_AkunKas_Update AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_AkunKas_Update
    @id BIGINT,
    @nama NVARCHAR(150),
    @kategori_kas NVARCHAR(20),
    @jenis_kas NVARCHAR(20),
    @saldo DECIMAL(18,2) = 0,
    @aktif BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SET @nama = LTRIM(RTRIM(ISNULL(@nama, N'')));
    SET @kategori_kas = UPPER(LTRIM(RTRIM(ISNULL(@kategori_kas, N''))));
    SET @jenis_kas = UPPER(LTRIM(RTRIM(ISNULL(@jenis_kas, N''))));

    IF NOT EXISTS (SELECT 1 FROM dbo.AkunKas WHERE id = @id)
        THROW 51006, 'Data akun kas tidak ditemukan.', 1;

    IF (@nama = N'') THROW 51001, 'Nama akun kas wajib diisi.', 1;
    IF (@kategori_kas NOT IN (N'LACI', N'REKENING', N'EWALLET')) THROW 51002, 'Kategori kas tidak valid.', 1;
    IF (@jenis_kas NOT IN (N'CASH', N'QRIS')) THROW 51003, 'Jenis kas tidak valid.', 1;
    IF (@saldo IS NULL OR @saldo < 0) THROW 51004, 'Saldo harus >= 0.', 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.AkunKas
        WHERE LOWER(LTRIM(RTRIM(nama))) = LOWER(@nama)
          AND UPPER(LTRIM(RTRIM(ISNULL(kategori_kas, N'')))) = @kategori_kas
          AND UPPER(LTRIM(RTRIM(ISNULL(jenis_kas, N'')))) = @jenis_kas
          AND id <> @id
    )
        THROW 51005, 'Akun kas dengan nama/kategori/jenis yang sama sudah ada.', 1;

    UPDATE dbo.AkunKas
    SET nama = @nama,
        kategori_kas = @kategori_kas,
        jenis_kas = @jenis_kas,
        saldo = @saldo,
        aktif = @aktif
    WHERE id = @id;
END

GO

IF OBJECT_ID('dbo.sp_AkunKas_Delete', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_AkunKas_Delete AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_AkunKas_Delete
    @id BIGINT,
    @hardDelete BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.AkunKas WHERE id = @id)
        THROW 51006, 'Data akun kas tidak ditemukan.', 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.Transaksi
        WHERE akun_kas_sumber_id = @id OR akun_kas_tujuan_id = @id
    )
    BEGIN
        IF (@hardDelete = 1)
            THROW 51007, 'Tidak bisa hard delete: akun kas sudah dipakai transaksi.', 1;

        UPDATE dbo.AkunKas SET aktif = 0 WHERE id = @id;
        RETURN;
    END

    IF (@hardDelete = 1)
        DELETE FROM dbo.AkunKas WHERE id = @id;
    ELSE
        UPDATE dbo.AkunKas SET aktif = 0 WHERE id = @id;
END";

            ExecuteBatches(conn, tx, sql);
        }

        public static void EnsureTransferProcedures(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('dbo.sp_Transfer_Save', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Transfer_Save AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Transfer_Save
    @amount DECIMAL(18,2),
    @desc NVARCHAR(255),
    @source_id BIGINT,
    @dest_id BIGINT,
    @userId BIGINT,
    @new_transaksi_id BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @desc = LTRIM(RTRIM(ISNULL(@desc, N'')));
    IF (@amount IS NULL OR @amount <= 0) THROW 52001, 'Nominal transfer harus > 0.', 1;
    IF (@desc = N'') THROW 52002, 'Keterangan wajib diisi.', 1;
    IF (@source_id IS NULL OR @dest_id IS NULL) THROW 52003, 'Kas sumber & tujuan wajib dipilih.', 1;
    IF (@source_id = @dest_id) THROW 52004, 'Kas sumber & tujuan tidak boleh sama.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.AkunKas WHERE id = @source_id AND aktif = 1)
        THROW 52005, 'Kas sumber tidak valid / tidak aktif.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.AkunKas WHERE id = @dest_id AND aktif = 1)
        THROW 52006, 'Kas tujuan tidak valid / tidak aktif.', 1;

    DECLARE @saldo_source DECIMAL(18,2);
    SELECT @saldo_source = saldo FROM dbo.AkunKas WHERE id = @source_id;
    IF (@saldo_source IS NULL OR @saldo_source < @amount)
        THROW 52007, 'Saldo kas sumber tidak mencukupi.', 1;

    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO dbo.Transaksi(waktu_transaksi, tipe_transaksi, nominal, keterangan, akun_kas_sumber_id, akun_kas_tujuan_id, dibuat_oleh_pengguna_id)
        VALUES (SYSDATETIME(), N'TRANSFER', @amount, @desc, @source_id, @dest_id, @userId);

        SET @new_transaksi_id = CONVERT(BIGINT, SCOPE_IDENTITY());

        UPDATE dbo.AkunKas SET saldo = saldo - @amount WHERE id = @source_id;
        UPDATE dbo.AkunKas SET saldo = saldo + @amount WHERE id = @dest_id;

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END";

            ExecuteBatches(conn, tx, sql);
        }



        public static void EnsureTargetOmzetViewsAndProcedures(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('dbo.v_TargetOmzetHistory', 'V') IS NULL
    EXEC('CREATE VIEW dbo.v_TargetOmzetHistory AS SELECT 1 AS dummy');

GO

ALTER VIEW dbo.v_TargetOmzetHistory
AS
WITH s AS (
    SELECT CAST(waktu_transaksi AS DATE) AS tanggal,
           SUM(nominal) AS omzet
    FROM dbo.Transaksi
    WHERE tipe_transaksi = N'PEMASUKAN'
    GROUP BY CAST(waktu_transaksi AS DATE)
)
SELECT t.tanggal,
       t.target_nominal,
       ISNULL(s.omzet, 0) AS omzet,
       (ISNULL(s.omzet, 0) - t.target_nominal) AS selisih
FROM dbo.TargetOmzetHarian t
LEFT JOIN s ON s.tanggal = t.tanggal;

GO

IF OBJECT_ID('dbo.sp_TargetOmzet_Save', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_TargetOmzet_Save AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_TargetOmzet_Save
    @tanggal DATE,
    @target DECIMAL(18,2),
    @userId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    IF (@tanggal IS NULL) THROW 53001, 'Tanggal tidak valid.', 1;
    IF (@target IS NULL OR @target <= 0) THROW 53002, 'Target harus > 0.', 1;

    MERGE dbo.TargetOmzetHarian AS t
    USING (SELECT @tanggal AS tanggal) AS s
    ON t.tanggal = s.tanggal
    WHEN MATCHED THEN
        UPDATE SET target_nominal = @target,
                   dibuat_pada = SYSDATETIME(),
                   dibuat_oleh_pengguna_id = @userId
    WHEN NOT MATCHED THEN
        INSERT (tanggal, target_nominal, dibuat_pada, dibuat_oleh_pengguna_id)
        VALUES (@tanggal, @target, SYSDATETIME(), @userId);
END

GO

IF OBJECT_ID('dbo.sp_TargetOmzet_History', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_TargetOmzet_History AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_TargetOmzet_History
    @maxRows INT = 60
AS
BEGIN
    SET NOCOUNT ON;
    IF (@maxRows IS NULL OR @maxRows <= 0 OR @maxRows > 3650) SET @maxRows = 60;

    SELECT TOP (@maxRows) tanggal, target_nominal, omzet, selisih
    FROM dbo.v_TargetOmzetHistory
    ORDER BY tanggal DESC;
END";

            ExecuteBatches(conn, tx, sql);
        }


        public static void EnsureHutangViewsAndProcedures(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('dbo.v_HutangList', 'V') IS NULL
    EXEC('CREATE VIEW dbo.v_HutangList AS SELECT 1 AS dummy');

GO

ALTER VIEW dbo.v_HutangList
AS
SELECT h.id,
       h.waktu_dibuat,
       h.nama_pelanggan,
       h.nominal,
       h.keterangan,
       h.jatuh_tempo,
       h.status
FROM dbo.HutangPelanggan h;

GO

IF OBJECT_ID('dbo.sp_Hutang_Search', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Hutang_Search AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Hutang_Search
    @status NVARCHAR(20) = N'ALL',
    @q NVARCHAR(150) = NULL,
    @maxRows INT = 500
AS
BEGIN
    SET NOCOUNT ON;

    SET @status = UPPER(LTRIM(RTRIM(ISNULL(@status, N'ALL'))));
    SET @q = LTRIM(RTRIM(ISNULL(@q, N'')));
    IF (@maxRows IS NULL OR @maxRows <= 0 OR @maxRows > 2000) SET @maxRows = 500;

    SELECT TOP (@maxRows)
           id, waktu_dibuat, nama_pelanggan, nominal, keterangan, jatuh_tempo, status
    FROM dbo.v_HutangList
    WHERE (@status = N'ALL' OR UPPER(status) = @status)
      AND (@q = N'' OR nama_pelanggan LIKE N'%' + @q + N'%')
    ORDER BY id DESC;

    SELECT COUNT(*) AS total
    FROM dbo.v_HutangList
    WHERE (@status = N'ALL' OR UPPER(status) = @status)
      AND (@q = N'' OR nama_pelanggan LIKE N'%' + @q + N'%');
END

GO

IF OBJECT_ID('dbo.sp_Hutang_Insert', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Hutang_Insert AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Hutang_Insert
    @nama NVARCHAR(150),
    @nominal DECIMAL(18,2),
    @ket NVARCHAR(255) = NULL,
    @due DATE = NULL,
    @userId BIGINT,
    @new_id BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @nama = LTRIM(RTRIM(ISNULL(@nama, N'')));
    SET @ket = NULLIF(LTRIM(RTRIM(ISNULL(@ket, N''))), N'');

    IF (@nama = N'') THROW 54001, 'Nama pelanggan wajib diisi.', 1;
    IF (@nominal IS NULL OR @nominal <= 0) THROW 54002, 'Nominal hutang harus > 0.', 1;

    INSERT INTO dbo.HutangPelanggan(waktu_dibuat, nama_pelanggan, nominal, keterangan, jatuh_tempo, status, dibuat_oleh_pengguna_id)
    VALUES (SYSDATETIME(), @nama, @nominal, @ket, @due, N'BELUM_LUNAS', @userId);

    SET @new_id = CONVERT(BIGINT, SCOPE_IDENTITY());
END

GO

IF OBJECT_ID('dbo.sp_Hutang_Pay', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Hutang_Pay AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Hutang_Pay
    @hutangId BIGINT,
    @bayar DECIMAL(18,2),
    @statusBayar NVARCHAR(20) = N'LUNAS',
    @payMethod NVARCHAR(20),
    @userId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SET @statusBayar = UPPER(LTRIM(RTRIM(ISNULL(@statusBayar, N'LUNAS'))));
    SET @payMethod = UPPER(LTRIM(RTRIM(ISNULL(@payMethod, N''))));

    IF (@bayar IS NULL OR @bayar <= 0) THROW 54003, 'Nominal bayar harus > 0.', 1;
    IF (@statusBayar NOT IN (N'BELUM_LUNAS', N'LUNAS')) THROW 54004, 'Status bayar tidak valid.', 1;
    IF (@payMethod NOT IN (N'CASH', N'QRIS')) THROW 54005, 'Metode bayar tidak valid.', 1;

    DECLARE @sisa DECIMAL(18,2);
    DECLARE @nama NVARCHAR(150);
    DECLARE @status NVARCHAR(20);
    SELECT @sisa = nominal, @nama = nama_pelanggan, @status = status
    FROM dbo.HutangPelanggan
    WHERE id = @hutangId;

    IF (@sisa IS NULL) THROW 54006, 'Data hutang tidak ditemukan.', 1;
    IF (UPPER(@status) = N'LUNAS') THROW 54007, 'Hutang ini sudah lunas.', 1;
    IF (@bayar > @sisa) THROW 54008, 'Nominal bayar tidak boleh melebihi sisa hutang.', 1;
    IF (@statusBayar = N'LUNAS' AND @bayar <> @sisa) THROW 54009, 'Jika status LUNAS, nominal bayar harus sama dengan sisa hutang.', 1;
    IF (@statusBayar = N'BELUM_LUNAS' AND @bayar >= @sisa) THROW 54010, 'Jika status BELUM_LUNAS, nominal bayar harus lebih kecil dari sisa hutang.', 1;

    DECLARE @destCashId BIGINT;
    SELECT TOP 1 @destCashId = id
    FROM dbo.AkunKas
    WHERE aktif = 1 AND UPPER(jenis_kas) = @payMethod
    ORDER BY id;

    IF (@destCashId IS NULL OR @destCashId <= 0)
        THROW 54011, 'Akun kas tujuan untuk metode bayar belum ada / belum aktif.', 1;

    BEGIN TRY
        BEGIN TRAN;

        IF (@statusBayar = N'LUNAS')
        BEGIN
            UPDATE dbo.HutangPelanggan
            SET nominal = 0,
                status = N'LUNAS',
                dilunasi_pada = SYSDATETIME(),
                dilunasi_oleh_pengguna_id = @userId
            WHERE id = @hutangId;
        END
        ELSE
        BEGIN
            UPDATE dbo.HutangPelanggan
            SET nominal = @sisa - @bayar,
                status = N'BELUM_LUNAS'
            WHERE id = @hutangId;
        END

        INSERT INTO dbo.Transaksi(waktu_transaksi, tipe_transaksi, nominal, keterangan, akun_kas_sumber_id, akun_kas_tujuan_id, dibuat_oleh_pengguna_id)
        VALUES (SYSDATETIME(), N'PEMASUKAN', @bayar,
                (CASE WHEN @statusBayar = N'LUNAS' THEN N'Pelunasan Bon: ' ELSE N'Pembayaran Bon (Sebagian): ' END) + @nama,
                NULL, @destCashId, @userId);

        UPDATE dbo.AkunKas SET saldo = saldo + @bayar WHERE id = @destCashId;

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END";

            ExecuteBatches(conn, tx, sql);
        }

        public static void EnsureStokBelanjaProcedures(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('dbo.sp_Stok_BelanjaSave', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Stok_BelanjaSave AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Stok_BelanjaSave
    @bahanId BIGINT,
    @qty DECIMAL(18,2),
    @total DECIMAL(18,2),
    @kasSumberId BIGINT,
    @ket NVARCHAR(255) = NULL,
    @userId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SET @ket = LTRIM(RTRIM(ISNULL(@ket, N'')));
    IF (@ket = N'') SET @ket = N'Belanja stok';

    IF (@qty IS NULL OR @qty <= 0) THROW 55001, 'Qty belanja tidak valid.', 1;
    IF (@total IS NULL OR @total <= 0) THROW 55002, 'Total belanja harus > 0.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Bahan WHERE id = @bahanId)
        THROW 55003, 'Bahan tidak ditemukan.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.AkunKas WHERE id = @kasSumberId AND aktif = 1)
        THROW 55004, 'Kas sumber tidak valid / tidak aktif.', 1;

    DECLARE @saldo DECIMAL(18,2);
    SELECT @saldo = saldo FROM dbo.AkunKas WHERE id = @kasSumberId;
    IF (@saldo IS NULL OR @saldo < @total)
        THROW 55005, 'Saldo kas sumber tidak mencukupi.', 1;

    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO dbo.MutasiStok(waktu, tipe, bahan_id, qty, total_biaya, keterangan, dibuat_oleh_pengguna_id)
        VALUES (SYSDATETIME(), N'MASUK', @bahanId, @qty, @total, @ket, @userId);

        UPDATE dbo.Bahan SET stok = stok + @qty WHERE id = @bahanId;

        INSERT INTO dbo.Transaksi(waktu_transaksi, tipe_transaksi, nominal, keterangan, akun_kas_sumber_id, akun_kas_tujuan_id, dibuat_oleh_pengguna_id)
        VALUES (SYSDATETIME(), N'PENGELUARAN', @total, N'Belanja Stok: ' + @ket, @kasSumberId, NULL, @userId);

        UPDATE dbo.AkunKas SET saldo = saldo - @total WHERE id = @kasSumberId;

        COMMIT;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        THROW;
    END CATCH
END";

            ExecuteBatches(conn, tx, sql);
        }

        public static void EnsureLaporanViews(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('dbo.v_TransaksiPenjualan', 'V') IS NULL
    EXEC('CREATE VIEW dbo.v_TransaksiPenjualan AS SELECT 1 AS dummy');

GO

ALTER VIEW dbo.v_TransaksiPenjualan
AS
SELECT t.waktu_transaksi,
       t.keterangan,
       t.nominal
FROM dbo.Transaksi t
WHERE t.tipe_transaksi = N'PEMASUKAN'
  AND t.keterangan LIKE N'Penjualan:%';

GO

IF OBJECT_ID('dbo.v_MenuHpp', 'V') IS NULL
    EXEC('CREATE VIEW dbo.v_MenuHpp AS SELECT 1 AS dummy');

GO

ALTER VIEW dbo.v_MenuHpp
AS
SELECT nama,
       perkiraan_modal
FROM dbo.Menu;";

            ExecuteBatches(conn, tx, sql);
        }
        public static void EnsureAuthProcedures(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('dbo.sp_User_Login', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_User_Login AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_User_Login
    @username NVARCHAR(100),
    @password NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SET @username = LTRIM(RTRIM(ISNULL(@username, N'')));
    SET @password = LTRIM(RTRIM(ISNULL(@password, N'')));

    SELECT TOP 1 u.id,
                 u.username,
                 u.full_name,
                 r.name AS role_name
    FROM [User] u
    JOIN UserRole ur ON ur.user_id = u.id
    JOIN Role r ON r.id = ur.role_id
    WHERE u.username = @username
      AND u.[Password] = @password
      AND u.is_active = 1;
END";

            ExecuteBatches(conn, tx, sql);
        }

        public static void EnsureStokViewsAndProcedures(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('dbo.v_BahanList', 'V') IS NULL
    EXEC('CREATE VIEW dbo.v_BahanList AS SELECT 1 AS dummy');

GO

ALTER VIEW dbo.v_BahanList
AS
SELECT b.id,
       b.nama,
       b.satuan,
       b.stok,
       b.aktif
FROM dbo.Bahan b;

GO

IF OBJECT_ID('dbo.sp_Bahan_Search', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Bahan_Search AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Bahan_Search
    @q NVARCHAR(150) = NULL,
    @aktif BIT = NULL,
    @maxRows INT = 500
AS
BEGIN
    SET NOCOUNT ON;
    SET @q = LTRIM(RTRIM(ISNULL(@q, N'')));
    IF (@maxRows IS NULL OR @maxRows <= 0 OR @maxRows > 2000) SET @maxRows = 500;

    SELECT TOP (@maxRows) id, nama, satuan, stok, aktif
    FROM dbo.v_BahanList
    WHERE (@aktif IS NULL OR aktif = @aktif)
      AND (@q = N'' OR nama LIKE N'%' + @q + N'%')
    ORDER BY id DESC;

    SELECT COUNT(*) AS total
    FROM dbo.v_BahanList
    WHERE (@aktif IS NULL OR aktif = @aktif)
      AND (@q = N'' OR nama LIKE N'%' + @q + N'%');
END

GO

IF OBJECT_ID('dbo.sp_Bahan_Insert', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Bahan_Insert AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Bahan_Insert
    @nama NVARCHAR(150),
    @satuan NVARCHAR(50),
    @aktif BIT = 1,
    @new_id BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @nama = LTRIM(RTRIM(ISNULL(@nama, N'')));
    SET @satuan = LTRIM(RTRIM(ISNULL(@satuan, N'')));

    IF (@nama = N'') THROW 50101, 'Nama bahan wajib diisi.', 1;
    IF (@satuan = N'') THROW 50102, 'Satuan wajib diisi.', 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.Bahan
        WHERE LOWER(LTRIM(RTRIM(nama))) = LOWER(@nama)
          AND LOWER(LTRIM(RTRIM(satuan))) = LOWER(@satuan)
    )
        THROW 50103, 'Bahan dengan nama & satuan yang sama sudah ada.', 1;

    INSERT INTO dbo.Bahan(nama, satuan, stok, aktif)
    VALUES (@nama, @satuan, 0, @aktif);

    SET @new_id = CONVERT(BIGINT, SCOPE_IDENTITY());
END

GO

IF OBJECT_ID('dbo.sp_Bahan_Update', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Bahan_Update AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Bahan_Update
    @id BIGINT,
    @nama NVARCHAR(150),
    @satuan NVARCHAR(50),
    @aktif BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET @nama = LTRIM(RTRIM(ISNULL(@nama, N'')));
    SET @satuan = LTRIM(RTRIM(ISNULL(@satuan, N'')));

    IF NOT EXISTS (SELECT 1 FROM dbo.Bahan WHERE id = @id)
        THROW 50104, 'Data bahan tidak ditemukan.', 1;

    IF (@nama = N'') THROW 50101, 'Nama bahan wajib diisi.', 1;
    IF (@satuan = N'') THROW 50102, 'Satuan wajib diisi.', 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.Bahan
        WHERE LOWER(LTRIM(RTRIM(nama))) = LOWER(@nama)
          AND LOWER(LTRIM(RTRIM(satuan))) = LOWER(@satuan)
          AND id <> @id
    )
        THROW 50103, 'Bahan dengan nama & satuan yang sama sudah ada.', 1;

    UPDATE dbo.Bahan
    SET nama = @nama,
        satuan = @satuan,
        aktif = @aktif
    WHERE id = @id;
END

GO

IF OBJECT_ID('dbo.sp_Bahan_Delete', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Bahan_Delete AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Bahan_Delete
    @id BIGINT,
    @hardDelete BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Bahan WHERE id = @id)
        THROW 50104, 'Data bahan tidak ditemukan.', 1;

    IF EXISTS (SELECT 1 FROM dbo.MutasiStok WHERE bahan_id = @id)
        THROW 50105, 'Tidak bisa menghapus: bahan sudah dipakai di mutasi stok.', 1;

    IF (@hardDelete = 1)
    BEGIN
        DELETE FROM dbo.Bahan WHERE id = @id;
    END
    ELSE
    BEGIN
        UPDATE dbo.Bahan SET aktif = 0 WHERE id = @id;
    END
END";

            ExecuteBatches(conn, tx, sql);
        }

        public static void EnsureMenuViewsAndProcedures(SqlConnection conn, SqlTransaction tx = null)
        {
            string sql = @"
IF OBJECT_ID('dbo.v_MenuList', 'V') IS NULL
    EXEC('CREATE VIEW dbo.v_MenuList AS SELECT 1 AS dummy');

GO

ALTER VIEW dbo.v_MenuList
AS
SELECT m.id,
       m.kategori_id,
       c.nama AS kategori,
       m.nama,
       m.satuan,
       m.harga_jual,
       m.perkiraan_modal,
       m.aktif,
       (m.harga_jual - ISNULL(m.perkiraan_modal, 0)) AS margin
FROM dbo.Menu m
JOIN dbo.KategoriMenu c ON c.id = m.kategori_id;

GO

IF OBJECT_ID('dbo.v_MenuActive', 'V') IS NULL
    EXEC('CREATE VIEW dbo.v_MenuActive AS SELECT 1 AS dummy');

GO

ALTER VIEW dbo.v_MenuActive
AS
SELECT id, kategori_id, kategori, nama, satuan, harga_jual, perkiraan_modal, aktif, margin
FROM dbo.v_MenuList
WHERE aktif = 1;

GO

IF OBJECT_ID('dbo.sp_Menu_Search', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Menu_Search AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Menu_Search
    @q NVARCHAR(150) = NULL,
    @kategori_id BIGINT = NULL,
    @aktif BIT = NULL,
    @maxRows INT = 500
AS
BEGIN
    SET NOCOUNT ON;

    SET @q = LTRIM(RTRIM(ISNULL(@q, N'')));
    IF (@maxRows IS NULL OR @maxRows <= 0 OR @maxRows > 2000) SET @maxRows = 500;

    SELECT TOP (@maxRows)
           id, kategori_id, kategori, nama, satuan, harga_jual, perkiraan_modal, aktif, margin
    FROM dbo.v_MenuList
    WHERE (@kategori_id IS NULL OR kategori_id = @kategori_id)
      AND (@aktif IS NULL OR aktif = @aktif)
      AND (@q = N'' OR nama LIKE N'%' + @q + N'%')
    ORDER BY id DESC;

    SELECT COUNT(*) AS total
    FROM dbo.v_MenuList
    WHERE (@kategori_id IS NULL OR kategori_id = @kategori_id)
      AND (@aktif IS NULL OR aktif = @aktif)
      AND (@q = N'' OR nama LIKE N'%' + @q + N'%');
END

GO

IF OBJECT_ID('dbo.sp_Menu_Insert', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Menu_Insert AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Menu_Insert
    @kategori_id BIGINT,
    @nama NVARCHAR(150),
    @satuan NVARCHAR(50),
    @harga_jual DECIMAL(18,2),
    @perkiraan_modal DECIMAL(18,2) = NULL,
    @aktif BIT = 1,
    @new_id BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @nama = LTRIM(RTRIM(ISNULL(@nama, N'')));
    SET @satuan = LTRIM(RTRIM(ISNULL(@satuan, N'')));

    IF (@nama = N'') THROW 50001, 'Nama menu wajib diisi.', 1;
    IF (@satuan = N'') THROW 50002, 'Satuan wajib diisi.', 1;
    IF (@harga_jual IS NULL OR @harga_jual <= 0) THROW 50003, 'Harga jual harus > 0.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.KategoriMenu WHERE id = @kategori_id)
        THROW 50004, 'Kategori tidak valid.', 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.Menu
        WHERE kategori_id = @kategori_id
          AND LOWER(LTRIM(RTRIM(nama))) = LOWER(@nama)
    )
        THROW 50005, 'Nama menu sudah ada di kategori yang sama.', 1;

    INSERT INTO dbo.Menu(kategori_id, nama, satuan, harga_jual, perkiraan_modal, aktif)
    VALUES (@kategori_id, @nama, @satuan, @harga_jual, @perkiraan_modal, @aktif);

    SET @new_id = CONVERT(BIGINT, SCOPE_IDENTITY());
END

GO

IF OBJECT_ID('dbo.sp_Menu_Update', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Menu_Update AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Menu_Update
    @id BIGINT,
    @kategori_id BIGINT,
    @nama NVARCHAR(150),
    @satuan NVARCHAR(50),
    @harga_jual DECIMAL(18,2),
    @perkiraan_modal DECIMAL(18,2) = NULL,
    @aktif BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SET @nama = LTRIM(RTRIM(ISNULL(@nama, N'')));
    SET @satuan = LTRIM(RTRIM(ISNULL(@satuan, N'')));

    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE id = @id)
        THROW 50006, 'Data menu tidak ditemukan.', 1;

    IF (@nama = N'') THROW 50001, 'Nama menu wajib diisi.', 1;
    IF (@satuan = N'') THROW 50002, 'Satuan wajib diisi.', 1;
    IF (@harga_jual IS NULL OR @harga_jual <= 0) THROW 50003, 'Harga jual harus > 0.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.KategoriMenu WHERE id = @kategori_id)
        THROW 50004, 'Kategori tidak valid.', 1;

    IF EXISTS (
        SELECT 1
        FROM dbo.Menu
        WHERE kategori_id = @kategori_id
          AND LOWER(LTRIM(RTRIM(nama))) = LOWER(@nama)
          AND id <> @id
    )
        THROW 50005, 'Nama menu sudah ada di kategori yang sama.', 1;

    UPDATE dbo.Menu
    SET kategori_id = @kategori_id,
        nama = @nama,
        satuan = @satuan,
        harga_jual = @harga_jual,
        perkiraan_modal = @perkiraan_modal,
        aktif = @aktif
    WHERE id = @id;
END

GO

IF OBJECT_ID('dbo.sp_Menu_Delete', 'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.sp_Menu_Delete AS BEGIN SET NOCOUNT ON; END');

GO

ALTER PROCEDURE dbo.sp_Menu_Delete
    @id BIGINT,
    @hardDelete BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Menu WHERE id = @id)
        THROW 50006, 'Data menu tidak ditemukan.', 1;

    IF (@hardDelete = 1)
    BEGIN
        DELETE FROM dbo.Menu WHERE id = @id;
    END
    ELSE
    BEGIN
        UPDATE dbo.Menu SET aktif = 0 WHERE id = @id;
    END
END";

            ExecuteBatches(conn, tx, sql);
        }

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
