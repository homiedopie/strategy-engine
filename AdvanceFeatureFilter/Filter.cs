using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvanceFeatureFilter
{
    public class Filter
    {
        public string? Value;
        public int Level;
        public int MaxLevel;
        public bool IsRootLevel;
        public Dictionary<string, Filter> ChildFilters = new Dictionary<string, Filter>();
        public Filter? ParentFilter = null;

        public List<int> Rules = new List<int>();
    }
}
