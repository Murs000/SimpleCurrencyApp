using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCurrencyApp.Models
{
    public class BanknoteInfo
    {
        public uint Denomination { get; set; }
        public uint Count { get; set; }
        public string Currency { get; set; }

        public uint Total => Denomination * Count;
    }
}
