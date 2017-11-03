using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    class Return : Exception
    {
        public readonly object Value;

        public Return(object value) : base(null, null)
        {
            Value = value;
        }
    }
}
