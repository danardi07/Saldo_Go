CREATE TRIGGER trg_Transaksi_Final
ON dbo.Transaksi
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    /* =========================
       PEMASUKAN
    ========================= */
    UPDATE ak
    SET ak.saldo = ak.saldo + i.nominal
    FROM dbo.AkunKas ak
    INNER JOIN inserted i
        ON ak.id = i.akun_kas_tujuan_id
    WHERE i.tipe_transaksi = 'PEMASUKAN';

    INSERT INTO LogAktivitas (aktivitas, waktu)
    SELECT 
        'PEMASUKAN: ' + CAST(nominal AS VARCHAR),
        GETDATE()
    FROM inserted
    WHERE tipe_transaksi = 'PEMASUKAN';

    /* =========================
       PENGELUARAN
    ========================= */
    UPDATE ak
    SET ak.saldo = ak.saldo - i.nominal
    FROM dbo.AkunKas ak
    INNER JOIN inserted i
        ON ak.id = i.akun_kas_sumber_id
    WHERE i.tipe_transaksi = 'PENGELUARAN';

    INSERT INTO LogAktivitas (aktivitas, waktu)
    SELECT 
        'PENGELUARAN: ' + CAST(nominal AS VARCHAR),
        GETDATE()
    FROM inserted
    WHERE tipe_transaksi = 'PENGELUARAN';

    /* =========================
       TRANSFER
    ========================= */
    -- kurangi sumber
    UPDATE ak
    SET ak.saldo = ak.saldo - i.nominal
    FROM dbo.AkunKas ak
    INNER JOIN inserted i
        ON ak.id = i.akun_kas_sumber_id
    WHERE i.tipe_transaksi = 'TRANSFER';

    -- tambah tujuan
    UPDATE ak
    SET ak.saldo = ak.saldo + i.nominal
    FROM dbo.AkunKas ak
    INNER JOIN inserted i
        ON ak.id = i.akun_kas_tujuan_id
    WHERE i.tipe_transaksi = 'TRANSFER';

    INSERT INTO LogAktivitas (aktivitas, waktu)
    SELECT 
        'TRANSFER: ' + CAST(nominal AS VARCHAR),
        GETDATE()
    FROM inserted
    WHERE tipe_transaksi = 'TRANSFER';

END;

