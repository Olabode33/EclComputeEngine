using CsvHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFRS9_ECL.Util
{
    public static class FileSystemStorage<T>
    {
        public static List<T> ReadCsvData(Guid eclId, string fileName)
        {
            fileName = $"{eclId}-{fileName}";
            var lst = new List<T>();
            var eCLProcessingData= Path.Combine(ConfigurationManager.AppSettings["ECLProcessingData"], eclId.ToString());
            if (!Directory.Exists(eCLProcessingData))
                Directory.CreateDirectory(eCLProcessingData);

            using (TextReader reader = File.OpenText(Path.Combine(eCLProcessingData, $"{fileName}.csv")))
            {
                CsvReader csv = new CsvReader(reader, new System.Globalization.CultureInfo("en"));
                csv.Configuration.Delimiter = ";";
                
                csv.Configuration.MissingFieldFound = null;
                csv.Configuration.HasHeaderRecord = false;
                var _lst=csv.GetRecords<T>();
                lst = _lst.ToList();
                //while (csv.Read())
                //{
                //    T Record = csv.GetRecord<T>();
                //    lst.Add(Record);
                //}
            }
            return lst;
        }
        public static List<T> ReadCsvData(string fileName)
        {
            var lst = new List<T>();
            using (TextReader reader = File.OpenText(fileName))
            {
                CsvReader csv = new CsvReader(reader, new System.Globalization.CultureInfo("en"));
                csv.Configuration.Delimiter = ";";

                csv.Configuration.MissingFieldFound = null;
                csv.Configuration.HasHeaderRecord = false;
                var _lst = csv.GetRecords<T>();
                lst = _lst.ToList();
            }
            return lst;
        }

        public static bool WriteCsvData(string fileName, List<T> data)
        {
            using (TextWriter writer = File.CreateText(fileName))
            {
                CsvWriter csv = new CsvWriter(writer, new System.Globalization.CultureInfo("en"));
                csv.Configuration.Delimiter = ";";
                csv.Configuration.HasHeaderRecord = false;

                csv.WriteRecords(data);
            }
            return true;
        }

        public static bool WriteCsvData(Guid eclId, string fileName, List<T> data)
        {
            var fileNameTemp = $"{eclId}-{Guid.NewGuid()}-{fileName}";
            fileName = $"{eclId}-{fileName}";
            
            var lst = new List<T>();
            var eCLProcessingData = Path.Combine(ConfigurationManager.AppSettings["ECLProcessingData"], eclId.ToString());
            if (!Directory.Exists(eCLProcessingData))
                Directory.CreateDirectory(eCLProcessingData);

            using (TextWriter writer = File.CreateText(Path.Combine(eCLProcessingData, $"{fileNameTemp}.csv")))
            {
                CsvWriter csv = new CsvWriter(writer, new System.Globalization.CultureInfo("en"));
                csv.Configuration.Delimiter = ";";
                csv.Configuration.HasHeaderRecord = false;
                
                csv.WriteRecords(data);
            }

            var hasAppendedData = false;

            var contentTemp=File.ReadAllText(Path.Combine(eCLProcessingData, $"{fileNameTemp}.csv"));
            while (!hasAppendedData)
            {
                try
                {
                    File.AppendAllText(Path.Combine(eCLProcessingData, $"{fileName}.csv"), contentTemp);
                    hasAppendedData = true;
                    File.Delete(Path.Combine(eCLProcessingData, $"{fileNameTemp}.csv"));
                }
                catch { };
            }

            return true;
        }

       
    }
}
