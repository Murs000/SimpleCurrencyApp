using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCurrencyApp.Models
{
    public class BanknoteInfo
    {
        public int Denomination { get; set; }
        public int Count { get; set; }
        public string Currency { get; set; }

        public int Total => Denomination * Count;
    }
}
