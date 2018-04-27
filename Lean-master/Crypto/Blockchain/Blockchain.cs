using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Securities;

namespace QuantConnect.Blockchain {

    public abstract class Blockchain {

        public string Name;
        public string Symbol;

        public Blockchain(string Name, string Symbol) {

            this.Name = Name;
            this.Symbol = Symbol;
        }

        public abstract Transaction PublishTransaction(Wallet wallet);

        // for updating transaction state
        public abstract Transaction QueryTransaction(int ID);

        
        public abstract CryptoCash[] QueryHoldings(Wallet wallet);


    }
}
