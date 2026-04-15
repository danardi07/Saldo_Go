using System;

namespace SaldoGo
{
    public class UserSession
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; } 

        public bool IsOwner
        {
            get { return RoleName == "PEMILIK"; }
        }

        public bool IsCashier
        {
            get { return RoleName == "KASIR"; }
        }
    }
}
