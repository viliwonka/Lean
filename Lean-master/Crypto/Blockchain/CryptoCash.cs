using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Securities;

namespace QuantConnect.Blockchain {

    public class CryptoCash : Cash {

        public Blockchain Blockchain { get; private set; }

        public CryptoCash(string symbol, decimal amount, decimal conversionRate, Blockchain blockchain) : base(symbol, amount, conversionRate) {

            this.Blockchain = blockchain;
        }

    }
}
