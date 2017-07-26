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
            string folder = @"C:\Users\bholland\Documents\finch\opts-lots";
            string[] fileEntries = Directory.GetFiles(folder, "*.htm");

            using (CsvFileWriter writer = new CsvFileWriter(Path.Combine(folder, "StrategyTesterParser-lotsNZD.csv")))
            {
                foreach (string fileName in fileEntries)
                {
                    int i = 0;

                    row.Add(Path.GetFileName(fileName));
                    writer.WriteRow(row);
                    row = new CsvRow();
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(fileName);

                    if (doc.DocumentNode.Descendants("div").First().Descendants().Skip(1).First().InnerText.Contains("Optimization"))
                    {
                        var tablenodes = doc.DocumentNode.Descendants("table").Skip(1).Take(1);

                        if (tablenodes != null)
                        {
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
                    else
                    {
                        row = new CsvRow();
                        HtmlAgilityPack.HtmlNode profit = doc.DocumentNode.Descendants("table").First().Descendants("td").Where(x => x.InnerText == "Total net profit").First().NextSibling;
                        HtmlAgilityPack.HtmlNode dd = doc.DocumentNode.Descendants("table").First().Descendants("td").Where(x => x.InnerText == "Maximal drawdown").First().NextSibling;
                        HtmlAgilityPack.HtmlNode trades = doc.DocumentNode.Descendants("table").First().Descendants("td").Where(x => x.InnerText == "Total trades").First().NextSibling;
                        HtmlAgilityPack.HtmlNode parameters = doc.DocumentNode.Descendants("table").First().Descendants("td").Where(x => x.InnerText == "Parameters").First().NextSibling;
                        row.Add(profit.InnerText);
                        row.Add(dd.InnerText);
                        row.Add(trades.InnerText);
                        row.Add(parameters.InnerText);
                        writer.WriteRow(row);
                        row = new CsvRow();
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
