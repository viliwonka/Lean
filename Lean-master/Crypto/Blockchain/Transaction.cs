using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Securities;

namespace QuantConnect.Blockchain {

    public class Transaction {

        public int ID;

        public int BlockNumber;

        public byte[] Data;

        public byte[] Signature;
        
        public Transaction() {

        }
    }
}
