using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadWriteCsv;
using System.IO;
using HtmlAgilityPack;

namespace StrategyTesterParser
{
    class Program
    {
        static void Main(string[] args)
        {
            CsvRow row = new CsvRow();
            //string[] fileEntries = Directory.GetFiles(@"C:\Users\encino\AppData\Roaming\MetaQuotes\Terminal\3212703ED955F10C7534BE8497B221F4\opt","*.htm");
            string[] fileEntries = Directory.GetFiles(@"C:\Users\bholland\Documents\finch\opts", "*.htm");

            using (CsvFileWriter writer = new CsvFileWriter("StrategyTesterParser.csv"))
            {
                foreach (string fileName in fileEntries)
                {
                    int i = 0;

                    row.Add(Path.GetFileName(fileName));
                    writer.WriteRow(row);
                    row = new CsvRow();
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(fileName);
                    var tablenodes = doc.DocumentNode.Descendants("table").Skip(1).Take(1);

                    if (tablenodes != null)
                    {

                        int j = 0;
                        row = new CsvRow();
                        foreach (HtmlAgilityPack.HtmlNode node in tablenodes)
                        {
                            foreach (HtmlAgilityPack.HtmlNode tr in node.Descendants())
                            {
                                if (i > 6)
                                {
                                    break;
                                } 
                                IEnumerable<HtmlAgilityPack.HtmlNode> td = tr.Descendants();
                                if (td.Count() > 4)
                                {
                                    string inputs = td.First().Attributes["title"] != null ? td.First().Attributes["title"].Value : string.Empty;
                                    row.Add(td.Skip(2).Take(1).First().InnerText);
                                    row.Add(td.Skip(4).Take(1).First().InnerText);
                                    row.Add(td.Skip(10).Take(1).First().InnerText);
                                    row.Add(inputs);
                                    writer.WriteRow(row);
                                    row = new CsvRow();
                                    i++;
                                }
                            }

                        }

                    }
                }
            }

        }

        static void WriteTest()
        {
            // Write sample data to CSV file
            using (CsvFileWriter writer = new CsvFileWriter("WriteTest.csv"))
            {
                for (int i = 0; i < 100; i++)
                {
                    CsvRow row = new CsvRow();
                    for (int j = 0; j < 5; j++)
                        row.Add(String.Format("Column{0}", j));
                    writer.WriteRow(row);
                }
            }
        }

        static void ReadTest()
        {
            // Read sample data from CSV file
            using (CsvFileReader reader = new CsvFileReader("ReadTest.csv"))
            {
                CsvRow row = new CsvRow();
                while (reader.ReadRow(row))
                {
                    foreach (string s in row)
                    {
                        Console.Write(s);
                        Console.Write(" ");
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
