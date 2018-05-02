using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Securities;
using Nethereum.Signer.Crypto;

namespace QuantConnect.Blockchain.Ethereum {

    public class EthereumWallet : Wallet {

        public EthereumWallet(string name, Blockchain blockchain, Secret privateKey) : base(name, blockchain, privateKey) {

            var acc = new Nethereum.Web3.Accounts.Account(new byte[0], Nethereum.Signer.Chain.MainNet);
            
        }

        public CryptoCash[] Holdings { 

            get {

                return this.Blockchain.QueryHoldings(this);
            }       
        }

        bool Supports(CryptoCash cash) {

            if (cash.Blockchain.Symbol == Blockchain.Symbol)
                return true;

            return false;
        }

        public byte[] GetPublicKey() {

            ECKey privKey = null;

            PrivateKey.Read(bytes  => {

                privKey = new ECKey(bytes, true);
            });
            
            return privKey.GetPubKey(false);
        }
    }
}
