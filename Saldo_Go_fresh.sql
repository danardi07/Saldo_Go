/*
SaldoGo - SQL Server schema (SSMS)
MODE: FRESH INSTALL (tanpa IF OBJECT_ID / IF NOT EXISTS)
Database: Saldo_Go

Catatan:
- Script ini diasumsikan dijalankan pada SQL Server yang BELUM punya DB Saldo_Go.
- Kalau DB/tabel sudah ada, script ini akan error.
*/

/* =======================
   CREATE DATABASE
   ======================= */
CREATE DATABASE [Saldo_Go];
GO

USE [Saldo_Go];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =======================
   TABLE: [User]
   ======================= */
CREATE TABLE dbo.[User]
(
    id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_User PRIMARY KEY,
    username NVARCHAR(50) NOT NULL,
    [Password] NVARCHAR(255) NOT NULL,
    full_name NVARCHAR(150) NOT NULL,
    is_active BIT NOT NULL CONSTRAINT DF_User_is_active DEFAULT(1)
);
GO

CREATE UNIQUE INDEX UX_User_username ON dbo.[User](username);
GO

/* =======================
   TABLE: Role
   ======================= */
CREATE TABLE dbo.Role
(
    id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Role PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);
GO

CREATE UNIQUE INDEX UX_Role_name ON dbo.Role(name);
GO

/* =======================
   TABLE: UserRole
   ======================= */
CREATE TABLE dbo.UserRole
(
    user_id BIGINT NOT NULL,
    role_id BIGINT NOT NULL,
    CONSTRAINT PK_UserRole PRIMARY KEY (user_id, role_id),
    CONSTRAINT FK_UserRole_User FOREIGN KEY (user_id) REFERENCES dbo.[User](id),
    CONSTRAINT FK_UserRole_Role FOREIGN KEY (role_id) REFERENCES dbo.Role(id)
);
GO

CREATE INDEX IX_UserRole_role ON dbo.UserRole(role_id);
GO

/* =======================
   TABLE: AkunKas
   ======================= */
CREATE TABLE dbo.AkunKas
(
    id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AkunKas PRIMARY KEY,
    nama NVARCHAR(150) NOT NULL,
    kategori_kas NVARCHAR(20) NOT NULL CONSTRAINT DF_AkunKas_kategori_kas DEFAULT(N'LACI'),
    jenis_kas NVARCHAR(20) NOT NULL,
    saldo DECIMAL(18,2) NOT NULL CONSTRAINT DF_AkunKas_saldo DEFAULT(0),
    aktif BIT NOT NULL CONSTRAINT DF_AkunKas_aktif DEFAULT(1),

    CONSTRAINT CK_AkunKas_kategori_kas CHECK (kategori_kas IN (N'LACI', N'REKENING', N'EWALLET')),
    CONSTRAINT CK_AkunKas_jenis_kas CHECK (jenis_kas IN (N'CASH', N'QRIS'))
);
GO

/* =======================
   TABLE: KategoriMenu
   ======================= */
CREATE TABLE dbo.KategoriMenu
(
    id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_KategoriMenu PRIMARY KEY,
    nama NVARCHAR(100) NOT NULL
);
GO

/* =======================
   TABLE: Menu
   ======================= */
CREATE TABLE dbo.Menu
(
    id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Menu PRIMARY KEY,
    kategori_id BIGINT NOT NULL,
    nama NVARCHAR(150) NOT NULL,
    satuan NVARCHAR(50) NOT NULL,
    harga_jual DECIMAL(18,2) NOT NULL,
    perkiraan_modal DECIMAL(18,2) NULL,
    aktif BIT NOT NULL CONSTRAINT DF_Menu_aktif DEFAULT(1),
    CONSTRAINT FK_Menu_KategoriMenu FOREIGN KEY (kategori_id) REFERENCES dbo.KategoriMenu(id)
);
GO

CREATE INDEX IX_Menu_kategori ON dbo.Menu(kategori_id);
CREATE INDEX IX_Menu_aktif ON dbo.Menu(aktif);
GO

/* =======================
   TABLE: Transaksi
   ======================= */
CREATE TABLE dbo.Transaksi
(
    id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Transaksi PRIMARY KEY,
    waktu_transaksi DATETIME2 NOT NULL,
    tipe_transaksi NVARCHAR(20) NOT NULL,
    nominal DECIMAL(18,2) NOT NULL,
    keterangan NVARCHAR(255) NOT NULL,
    akun_kas_sumber_id BIGINT NULL,
    akun_kas_tujuan_id BIGINT NULL,
    dibuat_oleh_pengguna_id BIGINT NOT NULL,

    CONSTRAINT FK_Transaksi_AkunKas_Sumber FOREIGN KEY (akun_kas_sumber_id) REFERENCES dbo.AkunKas(id),
    CONSTRAINT FK_Transaksi_AkunKas_Tujuan FOREIGN KEY (akun_kas_tujuan_id) REFERENCES dbo.AkunKas(id),
    CONSTRAINT FK_Transaksi_User FOREIGN KEY (dibuat_oleh_pengguna_id) REFERENCES dbo.[User](id),
    CONSTRAINT CK_Transaksi_tipe CHECK (tipe_transaksi IN (N'PEMASUKAN', N'PENGELUARAN', N'TRANSFER'))
);
GO

CREATE INDEX IX_Transaksi_waktu ON dbo.Transaksi(waktu_transaksi DESC);
CREATE INDEX IX_Transaksi_tipe ON dbo.Transaksi(tipe_transaksi);
CREATE INDEX IX_Transaksi_kas_sumber ON dbo.Transaksi(akun_kas_sumber_id);
CREATE INDEX IX_Transaksi_kas_tujuan ON dbo.Transaksi(akun_kas_tujuan_id);
GO

/* =======================
   TABLE: Bahan
   ======================= */
CREATE TABLE dbo.Bahan
(
    id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bahan PRIMARY KEY,
    nama NVARCHAR(150) NOT NULL,
    satuan NVARCHAR(50) NOT NULL,
    stok DECIMAL(18,2) NOT NULL CONSTRAINT DF_Bahan_stok DEFAULT(0),
    aktif BIT NOT NULL CONSTRAINT DF_Bahan_aktif DEFAULT(1)
);
GO

/* =======================
   TABLE: MutasiStok
   ======================= */
CREATE TABLE dbo.MutasiStok
(
    id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MutasiStok PRIMARY KEY,
    waktu DATETIME2 NOT NULL,
    tipe NVARCHAR(20) NOT NULL, /* MASUK / KELUAR */
    bahan_id BIGINT NOT NULL,
    qty DECIMAL(18,2) NOT NULL,
    total_biaya DECIMAL(18,2) NULL,
    keterangan NVARCHAR(255) NULL,
    dibuat_oleh_pengguna_id BIGINT NOT NULL,

    CONSTRAINT FK_MutasiStok_Bahan FOREIGN KEY (bahan_id) REFERENCES dbo.Bahan(id),
    CONSTRAINT FK_MutasiStok_User FOREIGN KEY (dibuat_oleh_pengguna_id) REFERENCES dbo.[User](id),
    CONSTRAINT CK_MutasiStok_tipe CHECK (tipe IN (N'MASUK', N'KELUAR'))
);
GO

CREATE INDEX IX_MutasiStok_waktu ON dbo.MutasiStok(waktu DESC);
CREATE INDEX IX_MutasiStok_bahan ON dbo.MutasiStok(bahan_id);
GO

/* =======================
   TABLE: TargetOmzetHarian
   ======================= */
CREATE TABLE dbo.TargetOmzetHarian
(
    tanggal DATE NOT NULL CONSTRAINT PK_TargetOmzetHarian PRIMARY KEY,
    target_nominal DECIMAL(18,2) NOT NULL,
    dibuat_pada DATETIME2 NOT NULL,
    dibuat_oleh_pengguna_id BIGINT NOT NULL,
    CONSTRAINT FK_TargetOmzet_User FOREIGN KEY (dibuat_oleh_pengguna_id) REFERENCES dbo.[User](id)
);
GO

/* =======================
   TABLE: HutangPelanggan
   ======================= */
CREATE TABLE dbo.HutangPelanggan
(
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

    CONSTRAINT FK_Hutang_User FOREIGN KEY (dibuat_oleh_pengguna_id) REFERENCES dbo.[User](id),
    CONSTRAINT FK_Hutang_LunasUser FOREIGN KEY (dilunasi_oleh_pengguna_id) REFERENCES dbo.[User](id),
    CONSTRAINT CK_Hutang_status CHECK (status IN (N'BELUM_LUNAS', N'LUNAS'))
);
GO

CREATE INDEX IX_Hutang_status ON dbo.HutangPelanggan(status);
CREATE INDEX IX_Hutang_waktu ON dbo.HutangPelanggan(waktu_dibuat DESC);
GO

/* =======================
   SEED
   ======================= */
INSERT INTO dbo.Role(name) VALUES (N'PEMILIK');
INSERT INTO dbo.Role(name) VALUES (N'KASIR');
GO

INSERT INTO dbo.KategoriMenu(nama) VALUES (N'Makanan');
INSERT INTO dbo.KategoriMenu(nama) VALUES (N'Minuman');
GO

INSERT INTO dbo.AkunKas(nama, kategori_kas, jenis_kas, saldo, aktif)
VALUES (N'Kas Cash', N'LACI', N'CASH', 0, 1);

INSERT INTO dbo.AkunKas(nama, kategori_kas, jenis_kas, saldo, aktif)
VALUES (N'Kas QRIS', N'LACI', N'QRIS', 0, 1);
GO

/* =======================
   SEED USER (OPSIONAL)
   ======================= */
/*
DECLARE @roleOwnerId BIGINT = (SELECT TOP 1 id FROM dbo.Role WHERE name = N'PEMILIK');
DECLARE @roleKasirId BIGINT = (SELECT TOP 1 id FROM dbo.Role WHERE name = N'KASIR');

-- Owner
INSERT INTO dbo.[User](username, [Password], full_name, is_active)
VALUES (N'owner', N'owner123', N'Pemilik', 1);

DECLARE @ownerId BIGINT = SCOPE_IDENTITY();
INSERT INTO dbo.UserRole(user_id, role_id)
VALUES (@ownerId, @roleOwnerId);

-- Kasir
INSERT INTO dbo.[User](username, [Password], full_name, is_active)
VALUES (N'kasir', N'kasir123', N'Kasir', 1);

DECLARE @kasirUserId BIGINT = SCOPE_IDENTITY();
INSERT INTO dbo.UserRole(user_id, role_id)
VALUES (@kasirUserId, @roleKasirId);

GO
*/
