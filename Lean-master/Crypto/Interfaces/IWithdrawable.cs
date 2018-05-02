using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Securities;

namespace QuantConnect.Crypto.Interfaces {


    /// <summary>
    /// Interfaces that describes object, from which we can withdraw specific asset
    /// </summary>
    public interface IWithdrawable {

        bool CanWithdraw(Cash cash);

        List<Cash> Balance { get; }
    }
}
