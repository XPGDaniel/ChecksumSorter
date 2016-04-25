using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using ChecksumSorter.Class;

namespace FVA2MD5
{
    class Program
    {
        static void Main(string[] args)
        {
            int StartingPoint = 0;
            string checksumfile = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory.FullName;
            string fakepath = @"C:\";
            string output = "";
            List<string> CandidateList = new List<string>();
            List<FileStruct> outputlist = null;
            //args = new string[] {@"C:\Users\Dev\Source\Repos\ChecksumSorter\bin\Debug\Work_20160425.md5"};
            if (args.Length > 0 && File.Exists(args[0]))
            {
                foreach (var s in args)
                {
                    CandidateList.Add(s);
                }
                checksumfile = Path.GetDirectoryName(args[0]);
            }
            else
            {
                CandidateList = GetFiles(checksumfile);
            }
            Console.WriteLine("No. of FVAs or md5s : " + CandidateList.Count);
            for (int i = StartingPoint; i < CandidateList.Count; i++)
            {
                Console.WriteLine((CandidateList[i]));
                switch (Path.GetExtension(CandidateList[i]).ToLowerInvariant())
                {
                    case ".fva":
                        XDocument xmlDoc = XDocument.Load(CandidateList[i]);
                        outputlist = xmlDoc.Descendants("fvx")
                            .Elements().Select(entry => new FileStruct
                            {
                                Name = Path.Combine(fakepath, entry.Attribute("name").Value),
                                hash = entry.Element("hash").Value.Trim(),
                            })
                            .OrderBy(r => Path.GetDirectoryName(r.Name))
                            .Select(entry => new FileStruct
                            {
                                Name = entry.Name.Replace(fakepath, ""),
                                hash = entry.hash,
                            }).ToList();
                        output = Path.Combine(checksumfile, Path.GetFileNameWithoutExtension(CandidateList[i]) + ".md5");
                        break;
                    case ".md5":
                        List<string> md5lines = File.ReadAllLines(CandidateList[i]).ToList();
                        outputlist = md5lines.Select(entry => new FileStruct
                        {
                            Name = Path.Combine(fakepath, entry.Trim().Split('*')[1].Trim()),
                            hash = entry.Trim().Split('*')[0].Trim(),
                        })
                        .OrderBy(r => Path.GetDirectoryName(r.Name))
                        .Select(entry => new FileStruct
                        {
                            Name = entry.Name.Replace(fakepath, ""),
                            hash = entry.hash,
                        }).ToList();
                        output = Path.Combine(checksumfile, Path.GetFileNameWithoutExtension(CandidateList[i]) + "-Sorted.md5");
                        break;

                }
                //Create CultureInfo
                //System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("zh-tw");
                //StringComparer cmp = StringComparer.Create(ci, false);
                int subfolderindex = outputlist.FindIndex(a => a.Name.Contains("\\"));
                var firsthalf = outputlist.Take(subfolderindex).OrderBy(x => x.Name).ToList();
                var Secondhalf = outputlist.Skip(subfolderindex).OrderBy(x => x.Name).ToList();
                var newlist = Secondhalf.Concat(firsthalf).ToList();
                if (newlist.Any())
                {
                    StringBuilder builder = new StringBuilder();
                    using (FileStream file = File.Create(output))
                    { }
                    foreach (var fss in newlist)
                    {
                        if (!fss.Name.ToLowerInvariant().Contains("thumbs.db"))
                        {
                            builder.Append(fss.hash + " *" + fss.Name).AppendLine();
                        }
                    }
                    if (builder.Length > 0)
                    {
                        using (TextWriter writer = File.CreateText(output))
                        {
                            writer.Write(builder.ToString());
                        }
                        builder.Clear();
                    }
                }
            }
            //Console.ReadKey();
        }
        static private List<string> GetFiles(string path) //, string pattern
        {
            var files = new List<string>();

            try
            {
                if (!path.Contains("$RECYCLE.BIN") && !path.Contains("#recycle"))
                {
                    //files.AddRange(Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly));
                    files.AddRange(Directory.GetFiles(path).Where(file => file.ToLower().EndsWith("fva") || file.ToLower().EndsWith("md5")).ToList());
                    foreach (var directory in Directory.GetDirectories(path))
                        files.AddRange(GetFiles(directory));
                }
            }
            catch (UnauthorizedAccessException) { }

            return files;
        }
    }
}
