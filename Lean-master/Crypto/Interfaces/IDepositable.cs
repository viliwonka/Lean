using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Securities;

namespace QuantConnect.Crypto.Interfaces {

    /// <summary>
    /// Interfaces that describes object, to which we can deposit specific assets to
    /// </summary>
    public interface IDepositable {

        bool CanDeposit(Cash cash, decimal amount = 0);

    }
}
