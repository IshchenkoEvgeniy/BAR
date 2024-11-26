using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAR.Model.User
{
    public class Admin : AccountUser
    {
        public bool CanManageProducts => true;
        public bool CanManageUsers => true;
    }
}
