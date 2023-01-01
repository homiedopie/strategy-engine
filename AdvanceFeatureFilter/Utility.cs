using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvanceFeatureFilter
{
    public class Utility
    {
        public string? ConvertToString<T1>(T1 param)
        {
            if (param == null)
            {
                return string.Empty;
            }

            return param.ToString();
        }
    }
}
