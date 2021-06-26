using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ticker
{
    class TickerUpdateEventArgs : EventArgs
    {
        public DTO.Symbol Symbol { get; }

        public TickerUpdateEventArgs(DTO.Symbol symbol)
        {
            Symbol = symbol;
        }
    }
}
