using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAR.Model.User
{
    public class AccountUser : User
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int BonusPoints { get; set; }
        public override bool CanAccessPromotions => true;
    }
}
