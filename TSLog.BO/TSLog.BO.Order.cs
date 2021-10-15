using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.BO
{
    public class Order
    {
        public class Line 
        { 
            public int Number { get; set; }
            public Product Product { get; set; }
        }
        public string Id { get; set; }
        public string Number { get; set; }
        public double Quantity { get; set; }
    }
}
