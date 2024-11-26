using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAR.Model.User
{
    public abstract class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual bool CanAccessPromotions => false;
    }
}
