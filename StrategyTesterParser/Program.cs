using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReadWriteCsv;
using System.IO;
using HtmlAgilityPack;
using System.Data;

namespace StrategyTesterParser
{
    class Program
    {
        static void Main(string[] args)
        {


            //getXMLfilesConvert(@"C:\Users\Home\Documents\2016-laptop-backups\documents\_trading\bookerreport\backtesting\mt5\", "10StratOpt.csv");

            //ReplaceIniFileNames("gbpaud", @"C:\Users\Home\Documents\2016-laptop-backups\documents\_trading\bookerreport\backtesting\mt5\template");
            //ReplaceIniFileNames("GBPAUD", @"C:\Users\Home\AppData\Roaming\MetaQuotes\Terminal\3212703ED955F10C7534BE8497B221F4\template-bb-pips");

            //ReplaceIniFileNames("AUDCAD-bb", "AUDCHF-bb-pips", "txt", @"AUD");

            CreateBatchFile(@"C:\Users\Home\Documents\2016-laptop-backups\documents\_trading\bookerreport\backtesting\mt5\ini-files");

            CsvRow row = new CsvRow();
            string folder = @"C:\Users\Home\AppData\Roaming\MetaQuotes\Terminal\3212703ED955F10C7534BE8497B221F4\Opt\EURCAD\2pass";
            string[] fileEntries = Directory.GetFiles(folder, "*.htm");

            using (CsvFileWriter writer = new CsvFileWriter(Path.Combine(folder, "StrategyTesterParser-EURCAD-2pass.csv")))
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
                                    if (i > 10)
                                    {
                                        break;
                                    }
                                    IEnumerable<HtmlAgilityPack.HtmlNode> td = tr.Descendants();
                                    if (td.Count() > 4)
                                    {
                                        string inputs = td.First().Attributes["title"] != null ? td.First().Attributes["title"].Value : string.Empty;
                                        string profit = td.Skip(2).Take(1).First().InnerText;
                                        string dd = td.Skip(10).Take(1).First().InnerText;
                                        row.Add(profit);
                                        row.Add(td.Skip(4).Take(1).First().InnerText);
                                        row.Add(dd);
                                        if (!dd.Contains("Drawdown") && Convert.ToDecimal(profit) > 0 && Convert.ToDecimal(dd) > 0)
                                        {
                                            row.Add(String.Format("profit/dd: {0:P2}", (Convert.ToDecimal(profit) / Convert.ToDecimal(dd))));
                                        }
                                        else
                                        {
                                            row.Add(string.Empty);
                                        }
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
                        row.Add(trades.InnerText);
                        row.Add(dd.InnerText);
                        if (Convert.ToDecimal(profit.InnerText) > 0 && Convert.ToDecimal(dd.InnerText.Substring(0, dd.InnerText.IndexOf("("))) > 0)
                        {
                            row.Add(String.Format("profit/dd: {0:P2}", (Convert.ToDecimal(profit.InnerText) / Convert.ToDecimal(dd.InnerText.Substring(0, dd.InnerText.IndexOf("("))))));
                        }
                        else
                        {
                            row.Add(string.Empty);
                        }
                        row.Add(parameters.InnerText);
                        writer.WriteRow(row);
                        row = new CsvRow();
                    }
                }
            }

        }

        private static void getXMLfilesConvert(string folder, string outputFile)
        {
            CsvRow row = null;
            string[] fileEntries = Directory.GetFiles(folder, "*.xml");

            using (CsvFileWriter writer = new CsvFileWriter(Path.Combine(folder, outputFile)))
            {
                foreach (string fileName in fileEntries)
                {
                    int i = 0;
                    row = new CsvRow();
                    row.Add(Path.GetFileName(fileName));
                    writer.WriteRow(row);

                    DataSet dt = null;
                    try
                    { //sometimes empty xml
                        dt = XMLtoDataTable.ImportExcelXML(fileName, true, true);
                    }
                    catch
                    {
                    }

                    if (dt != null)
                    {
                        row = new CsvRow();
                        foreach (DataColumn c in dt.Tables[0].Columns)
                        {
                            row.Add(c.ColumnName);
                        }
                        writer.WriteRow(row);


                        foreach (DataRow r in dt.Tables[0].Rows)
                        {
                            row = new CsvRow();
                            foreach (DataColumn c in dt.Tables[0].Columns)
                            {
                                row.Add(r[c.Ordinal].ToString());
                            }
                            writer.WriteRow(row);
                        }
                    }

                }
            }
        }

        private static void ReplaceIniFileNames(string newPair, string folder)
        {

            string[] fileEntries = Directory.GetFiles(folder, "*.*");

            foreach (string fileNameFull in fileEntries)
            {
                string fileName = Path.GetFileName(fileNameFull);
                string pairName = fileName.Substring(0, fileName.IndexOf("-"));
                File.Move(Path.Combine(folder, fileName), Path.Combine(folder, fileName.Replace(pairName, newPair)));

            }
        }

        private static void ReplaceIniFileNames(string searchString, string replaceString, string extension, string folder)
        {

            string[] fileEntries = Directory.GetFiles(folder, "*." + extension);

            foreach (string fileNameFull in fileEntries)
            {
                string fileName = Path.GetFileName(fileNameFull);
                File.Move(Path.Combine(folder, fileName), Path.Combine(folder, fileName.Replace(searchString, replaceString)));

            }
        }

        private static void CreateBatchFile(string folder)
        {
            string fileName = "opt.bat";

            try
            {
                if (File.Exists(Path.Combine(folder, fileName)))
                {
                    File.Delete(Path.Combine(folder, fileName));
                }

                using (StreamWriter sw = File.CreateText(Path.Combine(folder, fileName)))
                {
                    string[] fileEntries = Directory.GetFiles(folder, "*.*");
                    foreach (string fileNameFull in fileEntries)
                    {
                        string fileNameShort = Path.GetFileName(fileNameFull);
                        //sw.WriteLine("START /wait terminal.exe {0}", fileNameShort);
                        sw.WriteLine(@"START /wait terminal64.exe /config:""C:\Users\u92062794\AppData\Roaming\MetaQuotes\Terminal\FB9A56D617EDDDFE29EE54EBEFFE96C1\{0}""", fileNameShort);
                        ///
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
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
