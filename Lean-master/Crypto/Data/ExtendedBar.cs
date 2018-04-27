using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect;

namespace QuantConnect.Data.Market {

    class ExtendedBar : Bar {
        
        /// <summary>
        /// Arithmetic return
        /// </summary>
        public decimal Delta {
            get { return Close - Open; }
        }

        public decimal DeltaShadow {
            get { return High - Low; }
        }

        public decimal LogReturn {
            get { return (decimal) Math.Log( (double) Delta); }
        }

        public ExtendedBar(Bar bar) {
            
        }
    }
}
