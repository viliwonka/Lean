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
        public string Name { get; }
        
        public Blockchain Blockchain { get; }

        public Secret PrivateKey { get; private set; }

        public Wallet(string name, Blockchain blockchain, Secret privateKey) {
            
            this.PrivateKey = privateKey;
            this.Name = name;
            this.Blockchain = blockchain;
        }

        bool Supports(CryptoCash cash) {
            return cash.Blockchain.Symbol == this.Blockchain.Symbol;
        }

        //public abstract byte[] Sign(byte[] data);
    }
}
