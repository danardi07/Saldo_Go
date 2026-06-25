/*
SaldoGo - Trigger untuk Auto Update Saldo AkunKas
Database: Saldo_Go

Trigger: tr_Transaksi_AfterInsert
Tujuan: Otomatis update saldo pada dbo.AkunKas setelah insert ke dbo.Transaksi
         Otomatis mencatat aktivitas ke dbo.LogAktivitas

Logic:
- PEMASUKAN: tambah saldo ke akun_kas_tujuan_id
- PENGELUARAN: kurangi saldo dari akun_kas_sumber_id
- TRANSFER: kurangi dari sumber dan tambah ke tujuan
- Log aktivitas ke tabel LogAktivitas

Cara pakai:
1) Buka SSMS -> New Query
2) Connect ke database Saldo_Go
3) Jalankan script ini
*/

USE [Saldo_Go];
GO

/* =======================
   DROP TRIGGER JIKA SUDAH ADA
   ======================= */
IF OBJECT_ID(N'dbo.tr_Transaksi_AfterInsert', N'TR') IS NOT NULL
BEGIN
    DROP TRIGGER dbo.tr_Transaksi_AfterInsert;
END
GO

/* =======================
   CREATE TRIGGER
   ======================= */
CREATE TRIGGER dbo.tr_Transaksi_AfterInsert
ON dbo.Transaksi
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @tipe_transaksi NVARCHAR(20);
    DECLARE @nominal DECIMAL(18,2);
    DECLARE @akun_sumber_id BIGINT;
    DECLARE @akun_tujuan_id BIGINT;
    DECLARE @keterangan NVARCHAR(255);

    -- Ambil data dari tabel inserted (support multiple rows)
    SELECT 
        @tipe_transaksi = tipe_transaksi,
        @nominal = nominal,
        @akun_sumber_id = akun_kas_sumber_id,
        @akun_tujuan_id = akun_kas_tujuan_id,
        @keterangan = keterangan
    FROM inserted;

    -- Logic berdasarkan tipe transaksi
    IF @tipe_transaksi = N'PEMASUKAN'
    BEGIN
        -- Tambah saldo ke akun tujuan
        IF @akun_tujuan_id IS NOT NULL
        BEGIN
            UPDATE dbo.AkunKas
            SET saldo = saldo + @nominal
            WHERE id = @akun_tujuan_id;
        END
    END
    ELSE IF @tipe_transaksi = N'PENGELUARAN'
    BEGIN
        -- Kurangi saldo dari akun sumber
        IF @akun_sumber_id IS NOT NULL
        BEGIN
            UPDATE dbo.AkunKas
            SET saldo = saldo - @nominal
            WHERE id = @akun_sumber_id;
        END
    END
    ELSE IF @tipe_transaksi = N'TRANSFER'
    BEGIN
        -- Kurangi saldo dari akun sumber
        IF @akun_sumber_id IS NOT NULL
        BEGIN
            UPDATE dbo.AkunKas
            SET saldo = saldo - @nominal
            WHERE id = @akun_sumber_id;
        END

        -- Tambah saldo ke akun tujuan
        IF @akun_tujuan_id IS NOT NULL
        BEGIN
            UPDATE dbo.AkunKas
            SET saldo = saldo + @nominal
            WHERE id = @akun_tujuan_id;
        END
    END

    -- Catat aktivitas ke LogAktivitas (jika tabel ada)
    IF OBJECT_ID(N'dbo.LogAktivitas', N'U') IS NOT NULL
    BEGIN
        DECLARE @log_aktivitas NVARCHAR(500);
        SET @log_aktivitas = 'TRIGGER UPDATE SALDO: ' + @tipe_transaksi + ' - ' + CAST(@nominal AS NVARCHAR(50));
        
        INSERT INTO dbo.LogAktivitas (aktivitas, waktu)
        VALUES (@log_aktivitas, SYSDATETIME());
    END
END
GO

/* =======================
   VERIFIKASI TRIGGER
   ======================= */
-- Cek apakah trigger sudah terbuat
SELECT 
    name AS trigger_name,
    object_id,
    parent_class_desc AS table_name,
    create_date,
    modify_date
FROM sys.triggers
WHERE name = N'tr_Transaksi_AfterInsert';
GO

PRINT 'Trigger tr_Transaksi_AfterInsert berhasil dibuat.';
GO
