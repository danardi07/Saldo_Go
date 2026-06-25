/*
SaldoGo - Tabel LogAktivitas untuk Transaction Management
Database: Saldo_Go

Tujuan: Mencatat aktivitas transaksi untuk audit trail dan logging

Cara pakai:
1) Buka SSMS -> New Query
2) Connect ke database Saldo_Go
3) Jalankan script ini
*/

USE [Saldo_Go];
GO

/* =======================
   CREATE TABLE LogAktivitas
   ======================= */
IF OBJECT_ID(N'dbo.LogAktivitas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LogAktivitas
    (
        id_log BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_LogAktivitas PRIMARY KEY,
        aktivitas NVARCHAR(500) NOT NULL,
        waktu DATETIME2 NOT NULL CONSTRAINT DF_LogAktivitas_waktu DEFAULT(SYSDATETIME())
    );
END
GO

/* =======================
   VERIFIKASI
   ======================= */
SELECT 
    name AS table_name,
    object_id,
    create_date,
    modify_date
FROM sys.tables
WHERE name = N'LogAktivitas';
GO

PRINT 'Tabel LogAktivitas berhasil dibuat.';
GO
