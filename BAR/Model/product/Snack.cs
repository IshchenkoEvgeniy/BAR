using BAR.Model.product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAR.Model
{
    public class Snack : Product
    {
        public double Weight { get; set; }
        public bool IsVegetarian { get; set; }
    }
}
