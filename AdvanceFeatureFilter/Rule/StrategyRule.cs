using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvanceFeatureFilter.Rule
{
    public class StrategyRule : BaseRule
    {
        public int MaxLevel;
        public List<string> FilterValues = new List<string>();
    }
}
