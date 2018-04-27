using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Securities;

namespace QuantConnect.Blockchain
{
    public abstract class Wallet
    {
        string Name { get; }
        
        Blockchain Blockchain { get; }

        private Secret privateKey;

        public Wallet(string name, Blockchain blockchain) {
            this.Name = name;
            this.Blockchain = blockchain;
        }

        bool Supports(CryptoCash cash) {
            return cash.Blockchain.Symbol == this.Blockchain.Symbol;
        }

        public abstract byte[] Sign(byte[] data);
    }
}
