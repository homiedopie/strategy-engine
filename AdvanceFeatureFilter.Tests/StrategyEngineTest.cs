using Bogus;
using System.Diagnostics;
using System.Security.Principal;
using Xunit.Sdk;

namespace AdvanceFeatureFilter.Tests
{
    [TestClass]
    public class StrategyEngineTest
    {
        [TestMethod]
        public void PrepareFilter_ValidInput_AddsFilterAndRule()
        {
            var engine = makeEngine();
            var filterRow = new Dictionary<string, string> {
                {"RuleId", "1"},
                {"Priority", "80"},
                {"Filter1", "AAA"},
                {"Filter2", "<ANY>"},
                {"Filter3", "CCC"},
                {"Filter4", "DDD"},
                {"OutputValue", "8"},
            };

            engine.PrepareFilter(filterRow);

            Assert.AreEqual(engine.RootFilter.ChildFilters.Count, 1);
            Assert.IsNotNull(engine.Rules);
            Assert.IsTrue(engine.Rules.ContainsKey(Int32.Parse(filterRow["RuleId"])));
            Assert.IsNotNull(engine.RootFilter.ChildFilters["AAA"].ChildFilters["<ANY>"].ChildFilters["CCC"].ChildFilters["DDD"]);
        }

        [TestMethod]
        [DataTestMethod]
        [DataRow("1", "100", "")]
        [DataRow("1", "80", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        public void PrepareFilter_InvalidInput_ShouldThrowException(string RuleId, string Priority, string Filter1)
        {
            var engine = makeEngine();
            var filterRow = new Dictionary<string, string> {
                {"RuleId", RuleId},
                {"Priority", Priority},
                {"Filter1", Filter1},
                {"Filter2", "<ANY>"},
                {"Filter3", "CCC"},
                {"Filter4", "DDD"},
                {"OutputValue", "8"},
            };

            Assert.ThrowsException<System.ArgumentException>(() =>
            {
                engine.PrepareFilter(filterRow);
            });
        }

        [TestMethod]
        public void FindRule_InvalidArgument_ShouldThrowException()
        {
            var engine = makeEngine();
            List<Dictionary<string, string>> filterList = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string> {
                    {"RuleId", "1"},
                    {"Priority", "80"},
                    {"Filter1", "AAA"},
                    {"Filter2", "<ANY>"},
                    {"Filter3", "CCC"},
                    {"Filter4", "DDD"},
                    {"OutputValue", "8"},
                },
                new Dictionary<string, string> {
                    {"RuleId", "4"},
                    {"Priority", "100"},
                    {"Filter1", "AAA"},
                    {"Filter2", "BBB"},
                    {"Filter3", "CCC"},
                    {"Filter4", "<ANY>"},
                    {"OutputValue", "10"},
                }
            };

            foreach (var filterRow in filterList)
            {
                engine.PrepareFilter(filterRow);
            }

            Assert.ThrowsException<System.ArgumentException>(() =>
            {
                var result = engine.FindRule(new { }, "BBB", "CCC", "DDD");
            });
            Assert.ThrowsException<System.ArgumentException>(() =>
            {
                var result = engine.FindRule("AAA", new { }, "CCC", "DDD");
            });
            Assert.ThrowsException<System.ArgumentException>(() =>
            {
                var result = engine.FindRule("AAA", "BBB", new { }, "DDD");
            });
            Assert.ThrowsException<System.ArgumentException>(() =>
            {
                var result = engine.FindRule("AAA", "BBB", "CCC", new { });
            });
        }

        [TestMethod]
        public void FindRule_DifferentPriority_ShouldSelectHighest()
        {
            var engine = makeEngine();
            List<Dictionary<string, string>> filterList = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string> {
                    {"RuleId", "1"},
                    {"Priority", "80"},
                    {"Filter1", "AAA"},
                    {"Filter2", "<ANY>"},
                    {"Filter3", "CCC"},
                    {"Filter4", "DDD"},
                    {"OutputValue", "8"},
                },
                new Dictionary<string, string> {
                    {"RuleId", "4"},
                    {"Priority", "100"},
                    {"Filter1", "AAA"},
                    {"Filter2", "BBB"},
                    {"Filter3", "CCC"},
                    {"Filter4", "<ANY>"},
                    {"OutputValue", "10"},
                }
            };

            foreach (var filterRow in filterList)
            {
                engine.PrepareFilter(filterRow);
            }

            var result = engine.FindRule("AAA", "BBB", "CCC", "DDD");

            Assert.IsNotNull(result);
            Assert.AreEqual(result.RuleId, 4);
            Assert.AreEqual(result.OutputValue, 10);
            Assert.AreEqual(result.Priority, 100);
        }

        [TestMethod]
        public void FindRule_SamePriority_ShouldSelectFirst()
        {
            var engine = makeEngine();
            List<Dictionary<string, string>> filterList = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string> {
                    {"RuleId", "4"},
                    {"Priority", "80"},
                    {"Filter1", "AAA"},
                    {"Filter2", "BBB"},
                    {"Filter3", "CCC"},
                    {"Filter4", "<ANY>"},
                    {"OutputValue", "10"},
                },
                new Dictionary<string, string> {
                    {"RuleId", "1"},
                    {"Priority", "80"},
                    {"Filter1", "AAA"},
                    {"Filter2", "<ANY>"},
                    {"Filter3", "CCC"},
                    {"Filter4", "DDD"},
                    {"OutputValue", "8"},
                }
            };

            foreach (var filterRow in filterList)
            {
                engine.PrepareFilter(filterRow);
            }

            var result = engine.FindRule("AAA", "BBB", "CCC", "DDD");

            Assert.IsNotNull(result);
            Assert.AreEqual(result.RuleId, 1);
            Assert.AreEqual(result.OutputValue, 8);
            Assert.AreEqual(result.Priority, 80);
        }

        [TestMethod]
        public void FindRule_NoMatch_ShouldBeNull()
        {
            var engine = makeEngine();
            List<Dictionary<string, string>> filterList = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string> {
                    {"RuleId", "4"},
                    {"Priority", "80"},
                    {"Filter1", "AAA"},
                    {"Filter2", "BBB"},
                    {"Filter3", "CCC"},
                    {"Filter4", "<ANY>"},
                    {"OutputValue", "10"},
                },
                new Dictionary<string, string> {
                    {"RuleId", "1"},
                    {"Priority", "80"},
                    {"Filter1", "AAA"},
                    {"Filter2", "<ANY>"},
                    {"Filter3", "CCC"},
                    {"Filter4", "DDD"},
                    {"OutputValue", "8"},
                }
            };

            foreach (var filterRow in filterList)
            {
                engine.PrepareFilter(filterRow);
            }

            var result = engine.FindRule("AAA", "BBB", "DDD", "DDD");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void FindRule_IntValue_ShouldMatch()
        {
            var engine = makeEngine();
            List<Dictionary<string, string>> filterList = new List<Dictionary<string, string>>()
            {
                new Dictionary<string, string> {
                    {"RuleId", "4"},
                    {"Priority", "80"},
                    {"Filter1", "AAA"},
                    {"Filter2", "123"},
                    {"Filter3", "CCC"},
                    {"Filter4", "<ANY>"},
                    {"OutputValue", "10"},
                },
                new Dictionary<string, string> {
                    {"RuleId", "1"},
                    {"Priority", "80"},
                    {"Filter1", "AAA"},
                    {"Filter2", "<ANY>"},
                    {"Filter3", "123"},
                    {"Filter4", "DDD"},
                    {"OutputValue", "8"},
                }
            };

            foreach (var filterRow in filterList)
            {
                engine.PrepareFilter(filterRow);
            }

            var result = engine.FindRule("AAA", 123, "CCC", 123);
            Assert.IsNotNull(result);

            result = engine.FindRule("AAA", 123, 123, "DDD");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FindRule_ManyRecords_ShouldBePerformant()
        {
            var engine = makeEngine();
            var Counter = 2;
            
            new Faker<FilterRow>()
                .RuleFor(o => o.Filter1,
                        f => f.System.Random.String(3).ToString())
                .RuleFor(o => o.Filter2,
                        f => f.System.Random.String(3).ToString())
                .RuleFor(o => o.Filter3,
                        f => f.System.Random.String(3).ToString())
                .RuleFor(o => o.Filter4,
                        f => f.System.Random.String(3).ToString())
                .RuleFor(o => o.OutputValue, f => f.System.Random.Number(100))
                .RuleFor(o => o.Priority, f => f.System.Random.Number(100))
                .RuleFor(o => o.RuleId, f => f.System.Random.Number(1000))
                .Generate(50000)
                .ForEach(o =>
                    {
                        engine.PrepareFilter(new Dictionary<string, string> {
                            {"RuleId", (++Counter).ToString()},
                            {"Priority", o.Priority.ToString()},
                            {"Filter1", o.Filter1},
                            {"Filter2", o.Filter1},
                            {"Filter3", o.Filter1},
                            {"Filter4", o.Filter1},
                            {"OutputValue", o.OutputValue.ToString()}
                        });
                    }
                );

            engine.PrepareFilter(new Dictionary<string, string>
                {
                    { "RuleId", "1" },
                    { "Priority", "80" },
                    { "Filter1", "AAA" },
                    { "Filter2", "<ANY>" },
                    { "Filter3", "123" },
                    { "Filter4", "DDD" },
                    { "OutputValue", "8" },
                });

            var stopWatch = Stopwatch.StartNew();
            var result = engine.FindRule("AAA", 123, 123, "DDD");
            stopWatch.Stop();

            Console.WriteLine("Time taken (MS): " + stopWatch.Elapsed.TotalMilliseconds.ToString());
            Assert.IsNotNull(result);
        }


        public class FilterRow
        {
            public int RuleId;
            public int Priority;
            public string Filter1;
            public string Filter2;
            public string Filter3;
            public string Filter4;
            public int OutputValue;
        }

        private StrategyEngine makeEngine()
        {
            var validator = new Validator();
            var utility = new Utility();
            return new StrategyEngine(utility, validator);
        }
    }
}