﻿using BAR.Model.product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAR.Model
{
    public class Drink : Product
    {
        public double Alcoholic { get; set; }
        public double Volume { get; set; }
    }

}
