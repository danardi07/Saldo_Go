![](ss/Screenshot%202026-04-15%20130527.png)
![](ss/Screenshot%202026-04-15%20130538.png)
![](ss/Screenshot%202026-04-15%20130544.png)
![](ss/Screenshot%202026-04-15%20130550.png)
![](ss/Screenshot%202026-04-15%20130557.png)
![](ss/Screenshot%202026-04-15%20130603.png)
![](ss/Screenshot%202026-04-15%20130609.png)
![](ss/Screenshot%202026-04-15%20130615.png)
![](ss/Screenshot%202026-04-15%20130621.png)
![](ss/Screenshot%202026-04-15%20130632.png)
![](ss/Screenshot%202026-04-15%20130639.png)
![](ss/Screenshot%202026-04-15%20130657.png)
![](ss/Screenshot%202026-04-15%20130702.png)
![](ss/Screenshot%202026-04-15%20130712.png)


Skenario SQL INJECTION :

Form: Login (username/password)

Kondisi rentan 
jika menggunakan :
•	SELECT ... FROM [User] ... WHERE username = '<input>' AND password = '<input>'
Maka input user akan menjadi bagian dari sintaks SQL.
Skenario serangan
1.	Buka form Login
2.	Isi username dengan input yang mengubah kondisi WHERE menjadi selalu benar (tautology).
3.	Password bisa dikosongkan/acak.
4.	Dampak yang diharapkan pada sistem rentan: query mengembalikan baris user pertama yang aktif → attacker bisa “bypass login” dan berpotensi masuk sebagai role PEMILIK/KASIR sesuai data pertama yang terbaca.
Dampak
•	Bypass autentikasi
•	Akses fitur Owner/Kasir tanpa kredensial valid
•	Potensi manipulasi data transaksi/kas

Mitigasi yang dipakai di project ini :
Login.cs sudah memanggil stored procedure dbo.sp_User_Login dengan parameter (SqlParameter) sehingga input berbahaya diperlakukan sebagai nilai, bukan bagian dari query dan menutup celah SQL injection untuk login.
