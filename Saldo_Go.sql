/*
SaldoGo - SQL Server schema (SSMS)
Database: Saldo_Go

Cara pakai:
1) Buka SSMS -> New Query
2) Jalankan script ini (boleh sekaligus)
3) (Opsional) ubah seed user/password di bagian SEED USER
*/

/* =======================
   CREATE DATABASE
   ======================= */
IF DB_ID(N'Saldo_Go') IS NULL
BEGIN
    CREATE DATABASE [Saldo_Go];
END
GO

USE [Saldo_Go];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =======================
   TABLE: [User]
   ======================= */
IF OBJECT_ID(N'dbo.[User]', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[User]
    (
        id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_User PRIMARY KEY,
        username NVARCHAR(50) NOT NULL,
        [Password] NVARCHAR(255) NOT NULL,
        full_name NVARCHAR(150) NOT NULL,
        is_active BIT NOT NULL CONSTRAINT DF_User_is_active DEFAULT(1)
    );

    CREATE UNIQUE INDEX UX_User_username ON dbo.[User](username);
END
GO

/* =======================
   TABLE: Role
   ======================= */
IF OBJECT_ID(N'dbo.Role', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Role
    (
        id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Role PRIMARY KEY,
        name NVARCHAR(50) NOT NULL
    );

    CREATE UNIQUE INDEX UX_Role_name ON dbo.Role(name);
END
GO

/* =======================
   TABLE: UserRole
   ======================= */
IF OBJECT_ID(N'dbo.UserRole', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRole
    (
        user_id BIGINT NOT NULL,
        role_id BIGINT NOT NULL,
        CONSTRAINT PK_UserRole PRIMARY KEY (user_id, role_id),
        CONSTRAINT FK_UserRole_User FOREIGN KEY (user_id) REFERENCES dbo.[User](id),
        CONSTRAINT FK_UserRole_Role FOREIGN KEY (role_id) REFERENCES dbo.Role(id)
    );

    CREATE INDEX IX_UserRole_role ON dbo.UserRole(role_id);
END
GO

/* =======================
   TABLE: AkunKas
   ======================= */
IF OBJECT_ID(N'dbo.AkunKas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AkunKas
    (
        id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_AkunKas PRIMARY KEY,
        nama NVARCHAR(150) NOT NULL,
        kategori_kas NVARCHAR(20) NOT NULL CONSTRAINT DF_AkunKas_kategori_kas DEFAULT(N'LACI'),
        jenis_kas NVARCHAR(20) NOT NULL,
        saldo DECIMAL(18,2) NOT NULL CONSTRAINT DF_AkunKas_saldo DEFAULT(0),
        aktif BIT NOT NULL CONSTRAINT DF_AkunKas_aktif DEFAULT(1)
    );

    ALTER TABLE dbo.AkunKas WITH CHECK
        ADD CONSTRAINT CK_AkunKas_kategori_kas
        CHECK (kategori_kas IN (N'LACI', N'REKENING', N'EWALLET'));

    ALTER TABLE dbo.AkunKas WITH CHECK
        ADD CONSTRAINT CK_AkunKas_jenis_kas
        CHECK (jenis_kas IN (N'CASH', N'QRIS'));
END
GO

/* =======================
   TABLE: KategoriMenu
   ======================= */
IF OBJECT_ID(N'dbo.KategoriMenu', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KategoriMenu
    (
        id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_KategoriMenu PRIMARY KEY,
        nama NVARCHAR(100) NOT NULL
    );
END
GO

/* =======================
   TABLE: Menu
   ======================= */
IF OBJECT_ID(N'dbo.Menu', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Menu
    (
        id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Menu PRIMARY KEY,
        kategori_id BIGINT NOT NULL,
        nama NVARCHAR(150) NOT NULL,
        satuan NVARCHAR(50) NOT NULL,
        harga_jual DECIMAL(18,2) NOT NULL,
        perkiraan_modal DECIMAL(18,2) NULL,
        aktif BIT NOT NULL CONSTRAINT DF_Menu_aktif DEFAULT(1),
        CONSTRAINT FK_Menu_KategoriMenu FOREIGN KEY (kategori_id) REFERENCES dbo.KategoriMenu(id)
    );

    CREATE INDEX IX_Menu_kategori ON dbo.Menu(kategori_id);
    CREATE INDEX IX_Menu_aktif ON dbo.Menu(aktif);
END
GO

/* =======================
   TABLE: Transaksi
   ======================= */
IF OBJECT_ID(N'dbo.Transaksi', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Transaksi
    (
        id BIGINT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Transaksi PRIMARY KEY,
        waktu_transaksi DATETIME2 NOT NULL,
        tipe_transaksi NVARCHAR(20) NOT NULL,
        nominal DECIMAL(18,2) NOT NULL,
        keterangan NVARCHAR(255) NOT NULL,
        akun_kas_sumber_id BIGINT NULL,
        akun_kas_tujuan_id BIGINT NULL,
        dibuat_oleh_pengguna_id BIGINT NOT NULL,
        CONSTRAINT FK_Transaksi_AkunKas_Sumber FOREIGN KEY (akun_kas_sumber_id) REFERENCES dbo.AkunKas(id),
        CONSTRAINT FK_Transaksi_AkunKas_Tujuan FOREIGN KEY (akun_kas_tujuan_id) REFERENCES dbo.AkunKas(id),
        CONSTRAINT FK_Transaksi_User FOREIGN KEY (dibuat_oleh_pengguna_id) REFERENCES dbo.[User](id)
    );

    ALTER TABLE dbo.Transaksi WITH CHECK
        ADD CONSTRAINT CK_Transaksi_tipe
        CHECK (tipe_transaksi IN (N'PEMASUKAN', N'PENGELUARAN', N'TRANSFER'));

    CREATE INDEX IX_Transaksi_waktu ON dbo.Transaksi(waktu_transaksi DESC);
    CREATE INDEX IX_Transaksi_tipe ON dbo.Transaksi(tipe_transaksi);
    CREATE INDEX IX_Transaksi_kas_sumber ON dbo.Transaksi(akun_kas_sumber_id);
    CREATE INDEX IX_Transaksi_kas_tujuan ON dbo.Transaksi(akun_kas_tujuan_id);
END
GO

/* =======================
   TABLE: Bahan
   ======================= */
IF OBJECT_ID(N'dbo.Bahan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bahan
    (
        id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bahan PRIMARY KEY,
        nama NVARCHAR(150) NOT NULL,
        satuan NVARCHAR(50) NOT NULL,
        stok DECIMAL(18,2) NOT NULL CONSTRAINT DF_Bahan_stok DEFAULT(0),
        aktif BIT NOT NULL CONSTRAINT DF_Bahan_aktif DEFAULT(1)
    );
END
GO

/* =======================
   TABLE: MutasiStok
   ======================= */
IF OBJECT_ID(N'dbo.MutasiStok', N'U') IS NULL
BEGIN
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
        CONSTRAINT FK_MutasiStok_User FOREIGN KEY (dibuat_oleh_pengguna_id) REFERENCES dbo.[User](id)
    );

    ALTER TABLE dbo.MutasiStok WITH CHECK
        ADD CONSTRAINT CK_MutasiStok_tipe
        CHECK (tipe IN (N'MASUK', N'KELUAR'));

    CREATE INDEX IX_MutasiStok_waktu ON dbo.MutasiStok(waktu DESC);
    CREATE INDEX IX_MutasiStok_bahan ON dbo.MutasiStok(bahan_id);
END
GO

/* =======================
   TABLE: TargetOmzetHarian
   ======================= */
IF OBJECT_ID(N'dbo.TargetOmzetHarian', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TargetOmzetHarian
    (
        tanggal DATE NOT NULL CONSTRAINT PK_TargetOmzetHarian PRIMARY KEY,
        target_nominal DECIMAL(18,2) NOT NULL,
        dibuat_pada DATETIME2 NOT NULL,
        dibuat_oleh_pengguna_id BIGINT NOT NULL,
        CONSTRAINT FK_TargetOmzet_User FOREIGN KEY (dibuat_oleh_pengguna_id) REFERENCES dbo.[User](id)
    );
END
GO

/* =======================
   TABLE: HutangPelanggan
   ======================= */
IF OBJECT_ID(N'dbo.HutangPelanggan', N'U') IS NULL
BEGIN
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
        CONSTRAINT FK_Hutang_LunasUser FOREIGN KEY (dilunasi_oleh_pengguna_id) REFERENCES dbo.[User](id)
    );

    ALTER TABLE dbo.HutangPelanggan WITH CHECK
        ADD CONSTRAINT CK_Hutang_status
        CHECK (status IN (N'BELUM_LUNAS', N'LUNAS'));

    CREATE INDEX IX_Hutang_status ON dbo.HutangPelanggan(status);
    CREATE INDEX IX_Hutang_waktu ON dbo.HutangPelanggan(waktu_dibuat DESC);
END
GO

/* =======================
   SEED: Role
   ======================= */
IF NOT EXISTS (SELECT 1 FROM dbo.Role WHERE name = N'PEMILIK')
    INSERT INTO dbo.Role(name) VALUES (N'PEMILIK');
IF NOT EXISTS (SELECT 1 FROM dbo.Role WHERE name = N'KASIR')
    INSERT INTO dbo.Role(name) VALUES (N'KASIR');
GO

/* =======================
   SEED: KategoriMenu (default)
   ======================= */
IF NOT EXISTS (SELECT 1 FROM dbo.KategoriMenu WHERE LOWER(LTRIM(RTRIM(nama))) = 'makanan')
    INSERT INTO dbo.KategoriMenu(nama) VALUES (N'Makanan');
IF NOT EXISTS (SELECT 1 FROM dbo.KategoriMenu WHERE LOWER(LTRIM(RTRIM(nama))) = 'minuman')
    INSERT INTO dbo.KategoriMenu(nama) VALUES (N'Minuman');
GO

/* =======================
   SEED: AkunKas minimal (agar transaksi CASH/QRIS jalan)
   ======================= */
IF NOT EXISTS (SELECT 1 FROM dbo.AkunKas WHERE jenis_kas = N'CASH')
    INSERT INTO dbo.AkunKas(nama, kategori_kas, jenis_kas, saldo, aktif)
    VALUES (N'Kas Cash', N'LACI', N'CASH', 0, 1);

IF NOT EXISTS (SELECT 1 FROM dbo.AkunKas WHERE jenis_kas = N'QRIS')
    INSERT INTO dbo.AkunKas(nama, kategori_kas, jenis_kas, saldo, aktif)
    VALUES (N'Kas QRIS', N'LACI', N'QRIS', 0, 1);
GO

/* =======================
   SEED USER (OPSIONAL)
   - Aplikasi belum punya form kelola user, jadi login perlu data awal.
   - Silakan ganti username/password/full_name sesuai kebutuhan.
   ======================= */
/*
IF NOT EXISTS (SELECT 1 FROM dbo.[User] WHERE username = N'owner')
BEGIN
    INSERT INTO dbo.[User](username, [Password], full_name, is_active)
    VALUES (N'owner', N'owner123', N'Pemilik', 1);

    DECLARE @ownerId BIGINT = SCOPE_IDENTITY();
    DECLARE @roleOwnerId BIGINT = (SELECT TOP 1 id FROM dbo.Role WHERE name = N'PEMILIK');

    INSERT INTO dbo.UserRole(user_id, role_id)
    VALUES (@ownerId, @roleOwnerId);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.[User] WHERE username = N'kasir')
BEGIN
    INSERT INTO dbo.[User](username, [Password], full_name, is_active)
    VALUES (N'kasir', N'kasir123', N'Kasir', 1);

    DECLARE @kasirUserId BIGINT = SCOPE_IDENTITY();
    DECLARE @roleKasirId BIGINT = (SELECT TOP 1 id FROM dbo.Role WHERE name = N'KASIR');

    INSERT INTO dbo.UserRole(user_id, role_id)
    VALUES (@kasirUserId, @roleKasirId);
END
GO
*/
