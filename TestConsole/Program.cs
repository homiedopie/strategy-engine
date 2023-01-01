// See https://aka.ms/new-console-template for more information
using AdvanceFeatureFilter;
using CsvHelper;
using System.Globalization;
using TestConsole;

var engine = new StrategyEngine(new Utility(), new Validator());
var inputFile = args.Length > 0 ? args[0] : "SampleData.csv";
var resultFile = args.Length > 1 ? args[1] : "ResultData.csv";

if (!File.Exists(inputFile))
{
    throw new ArgumentException($"Input file does not exist - File: {inputFile}");
}

if (!File.Exists(resultFile))
{
    throw new ArgumentException($"Result file does not exist - File: {resultFile}");
}

using (var reader = new StreamReader(inputFile))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var records = csv.GetRecords<FilterRecord>();
    foreach (var record in records)
    {
        engine.PrepareFilter(new Dictionary<string, string>()
        {
            { "RuleId", record.RuleId.ToString() },
            { "Priority", record.Priority.ToString() },
            { "Filter1", record.Filter1 },
            { "Filter2", record.Filter2 },
            { "Filter3", record.Filter3 },
            { "Filter4", record.Filter4 },
            { "OutputValue", record.OutputValue.ToString() },
        });
    }
}

using (var reader = new StreamReader(resultFile))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var records = csv.GetRecords<ResultRecord>();
    Console.WriteLine($"Filter1\tFilter2\tFilter3\tFilter4\tResultId\tOutputValue");
    foreach (var record in records)
    {
        var result = engine.FindRule(record.Filter1, record.Filter2, record.Filter3, record.Filter4);
        Console.Write($"{record.Filter1}\t{record.Filter2}\t{record.Filter3}\t{record.Filter4}\t");
        if (result != null)
        {
            Console.Write($"{result.RuleId}\t{result.OutputValue}\n");
        }
        else
        {
            Console.Write("0\t0\n");
        }
    }
}
