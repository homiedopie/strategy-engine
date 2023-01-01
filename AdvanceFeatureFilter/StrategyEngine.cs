using AdvanceFeatureFilter.Rule;
using System.Linq;
using System.Net.Security;

namespace AdvanceFeatureFilter
{
    public class StrategyEngine
    {
        public Filter RootFilter;
        public Utility Utility;
        public Validator Validator;
        public Dictionary<int, BaseRule> Rules = new Dictionary<int, BaseRule>();

        public StrategyEngine(Utility utility, Validator validator)
        {
            RootFilter = new Filter()
            {
                Value = "Root"
            };

            Utility = utility;
            Validator = validator;
        }

        public void PrepareFilter(Dictionary<string, string> filterRow)
        {
            var maxPriority = 100;
            var rule = new StrategyRule(); // Assume
            rule.RuleId = Int32.Parse(filterRow["RuleId"]);
            rule.Priority = Int32.Parse(filterRow["Priority"]);
            rule.OutputValue = Int32.Parse(filterRow["OutputValue"]);

            if (
                !Validator.ValidInput(rule.RuleId) ||
                !Validator.ValidMax(rule.Priority, maxPriority) ||
                !Validator.ValidInput(rule.OutputValue)
            )
            {
                throw new ArgumentException();
            }

            // Check filter key
            var filterCounter = 1;
            var filterKey = $"Filter{filterCounter}";
            var maxLevel = 0;
            Filter? filter = null;

            // We will enumerate all Filter# columns dynamically
            while (filterRow.ContainsKey(filterKey))
            {
                var filterValue = filterRow[filterKey];
                if (!Validator.ValidInput(filterValue))
                {
                    throw new ArgumentException();
                }

                var oldFilter = filter;
                rule.FilterValues.Add(filterValue);

                Dictionary<string, Filter>? ChildFilters = null;
                bool IsAdd = false;

                if (filterCounter == 1)
                {
                    ChildFilters = RootFilter.ChildFilters;
                }
                else
                {
                    ChildFilters = filter?.ChildFilters;

                }

                if (ChildFilters?.ContainsKey(filterValue) == false)
                {
                    filter = new Filter();
                    filter.Value = filterValue;
                    filter.IsRootLevel = oldFilter == null;
                    filter.Level = 1;
                    IsAdd = true;

                    if (oldFilter != null)
                    {
                        filter.ParentFilter = oldFilter;
                        filter.Level = oldFilter.Level + 1;
                    }
                }

                if (ChildFilters != null)
                {
                    if (IsAdd && filter != null)
                    {
                        ChildFilters.Add(filterValue, filter);
                    }
                    else
                    {
                        filter = ChildFilters[filterValue];
                    }
                }

                filterCounter++;
                filterKey = $"Filter{filterCounter}";
                maxLevel++;
            }

            rule.MaxLevel = maxLevel;
            Rules.Add(rule.RuleId, rule);

            if (filter != null)
            {
                filter.Rules.Add(rule.RuleId);
                while (filter.ParentFilter != null && filter.ParentFilter != RootFilter)
                {
                    filter.MaxLevel = maxLevel;
                    filter = filter.ParentFilter;
                }
            }
        }

        public BaseRule? FindRule<T1, T2, T3, T4>(T1 Filter1, T2 Filter2, T3 Filter3, T4 Filter4)
        {

            if (
                !Validator.ValidInput(Filter1) ||
                !Validator.ValidInput(Filter2) ||
                !Validator.ValidInput(Filter3) ||
                !Validator.ValidInput(Filter4)
            )
            {
                throw new ArgumentException();
            }

            var stringFilter1 = Utility.ConvertToString(Filter1);
            var stringFilter2 = Utility.ConvertToString(Filter2);
            var stringFilter3 = Utility.ConvertToString(Filter3);
            var stringFilter4 = Utility.ConvertToString(Filter4);

            bool isFound = false;
            List<Filter> filterList = new List<Filter>()
            {
                RootFilter
            };

            List<int> selectedRules = new List<int>();
            var anyKey = "<ANY>";
            var filterKey = stringFilter1;
            var level = 1;

            while (isFound == false)
            {
                List<Filter> selectedFilters = new List<Filter>();
                var counter = 0;
                foreach (var filter in filterList)
                {
                    // To keep track of the inner list count
                    counter++;

                    // Root level has max level of 0
                    // We check the Max Level and level to determine we are
                    // at the bottom of the filter
                    if (filter.MaxLevel != 0 && filter.MaxLevel == filter.Level)
                    {
                        selectedRules.AddRange(filter.Rules);
                        isFound = true;
                        continue;
                    }

                    // We check against the exact filter key
                    if (filter.ChildFilters.ContainsKey(filterKey))
                    {
                        selectedFilters.Add(filter.ChildFilters[filterKey]);
                    }

                    // We check against the any key
                    if (filter.ChildFilters.ContainsKey(anyKey))
                    {
                        selectedFilters.Add(filter.ChildFilters[anyKey]);
                    }

                    // If we dont see anything then we terminate
                    if (counter == filterList.Count && selectedFilters.Count == 0)
                    {
                        // No matches
                        isFound = true;
                    }
                }

                // If we found something or not we terminate
                if (isFound)
                {
                    break;
                }

                // We increase the level of the filter until we reach
                // the max level
                level++;
                switch(level)
                {
                    case 2:
                        filterKey = stringFilter2;
                        break;
                    case 3: filterKey = stringFilter3;
                        break;
                    case 4: filterKey = stringFilter4;
                        break;
                }

                // We clear the filter list and replace it with the selected filters
                filterList.Clear();
                filterList.AddRange(selectedFilters);

                // We clear the selected filters for the next iteration
                selectedFilters.Clear();
            }

            // If we have selected rules, we now want to sort it by highest priority
            if (selectedRules.Count > 0)
            {
                BaseRule? lastRule = null;
                foreach (var ruleId in selectedRules.Distinct())
                {
                    var rule = Rules[ruleId];
                    if (lastRule == null)
                    {
                        lastRule = rule;
                        continue;
                    }

                    if (
                        // If the priority is greater than the old one
                        rule.Priority > lastRule.Priority ||
                        // If the priority is the same, select the first one
                        (rule.Priority == lastRule.Priority && rule.RuleId < lastRule.RuleId)
                    )
                    {
                        lastRule = rule;
                    }
                }

                return lastRule;
            }

            return null;
        }
    }
}