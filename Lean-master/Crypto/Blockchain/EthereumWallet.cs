using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Securities;

namespace QuantConnect.Blockchain {

    public class EthereumWallet : Wallet {

        public EthereumWallet(string Name, Secret privateKey, Blockchain blockchain) {

            

            this.Name = Name;

            this.cashList = new List<CryptoCash>(cashList);

        }

        public CryptoCash[] Holdings { 

            get { 
        
            }       
        }

        bool Supports(CryptoCash cash) {

            if (cash.Blockchain)
                return true;

            return false;
        }

        public byte[] Sign(byte[] data) {
            
        }
    }
}
