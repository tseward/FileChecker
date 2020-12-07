using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace FileChecker
{
    class Program
    {
        /// <summary>
        /// Implements a new Regex Class for the specified regular expression.
        /// </summary>
        private static Regex _pattern = new Regex(@"[\\\|~#%*\:{}?/]+", RegexOptions.Compiled);
        private static decimal _maxFileSizeInBytes = 107374182400;
        private static int _maxFileNameLength = 400;
        private static string _supportUrl = 
            "https://support.microsoft.com/office/invalid-file-names-and-file-types-in-onedrive-and-sharepoint-64883a5d-228e-48f5-b3d2-eb39e07630fa";
        private static string _debugDir = ""; //enter full path for testing
        static void Main(string[] args)
        {
            var dir = string.Empty;

            if(Debugger.IsAttached && !string.IsNullOrEmpty(_debugDir))
            {
                dir = _debugDir;
            }
            else if (Debugger.IsAttached && string.IsNullOrEmpty(_debugDir))
            {
                Console.WriteLine("Add a path to _debugDir prior to debugging.");
                Environment.Exit(0);
            }
            else
            {
                if (args.Length < 2 || args.Length > 2)
                {
                    Console.WriteLine("Usage: FileChecker -d <path>.");
                    Environment.Exit(0);
                }
                else if (args[0] == "-d".ToLower())
                {
                    dir = args[1].Trim();
                }
            }

            Stream stream = null;

            try
            {
                int count = 0;

                stream = new FileStream("FileCheckerResults.csv", FileMode.OpenOrCreate);

                using (StreamWriter writer = new StreamWriter(stream))
                {
                    stream = null;

                    writer.WriteLine("Condition,File Name,Invalid Character,Path,Rule Violation");

                    int i = 0;

                    DirectoryInfo source = new DirectoryInfo(dir);

                    foreach (DirectoryInfo di in source.GetDirectories())
                    {
                        if (di != null)
                        {
                            FileInfo[] files = source.GetFiles("*.*", SearchOption.AllDirectories);

                            foreach (FileInfo file in files)
                            {
                                count++;

                                var name = Path.GetFileName(file.Name);
                                var match = _pattern.Match(name);

                                Console.WriteLine(count + ".  " + name);

                                var namespaces = new List<string>() { "Icon", ".lock", "CON", "PRN", "AUX", "NUL", "COM1", 
                                    "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", 
                                    "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "desktop.ini", "thumbs.db", "ehtumbs.db" };
                                var extensions = new List<string>() { ".aspx", ".asmx", ".ascx", ".master", ".xap", ".swf", 
                                    ".jar", ".xsf", ".htc", ".tmp", ".ds_store" };

                                var extension = Path.GetFileName(file.Extension);

                                if (extensions.Contains(extension))
                                {
                                    writer.WriteLine($@"Error,{name},{extension},{file.FullName},Files cannot be of the following type {extension}. With Microsoft 365 Group-connected Team sites you cannot upload these files.");
                                    i++;
                                }
                                else if (name.Equals(namespaces))
                                {
                                    writer.WriteLine($@"Error,{name},{extension},{file.FullName},Filenames cannot be one of the following type {namespaces}. Also avoid these names followed immediately by an extension; for example NUL.txt is not recommended.");
                                    i++;
                                }
                                else if (match.Success)
                                {
                                    writer.WriteLine($@"Error,{name},{match} {match.NextMatch()},{file.FullName},You cannot use the following character anywhere in a file name {match} {match.NextMatch()}.");
                                    i++;
                                }
                                else if (name.StartsWith("_", StringComparison.OrdinalIgnoreCase))
                                {
                                    writer.WriteLine($@"Warning,{name},_,{file.FullName},If you use an underscore character (_) at the beginning of a file name the file will be a hidden file when using Open in Explorer.");
                                    i++;
                                }
                                else if (name.Contains(".."))
                                {
                                    writer.WriteLine($@"Error,{name},..{file.FullName},You cannot use the period character consecutively in the middle of a file name.");
                                    i++;
                                }
                                else if (name.EndsWith(".", StringComparison.OrdinalIgnoreCase))
                                {
                                    writer.WriteLine($@"Warning,{name},.,{file.FullName},Do not end a file or directory name with a period. Although the underlying file system may support such names, the Windows shell and user interface does not.");
                                   i++;
                                }
                                else if (file.Length.Equals(0))
                                {
                                    writer.WriteLine($"Error,{name},{string.Empty},{file.FullName},Files cannot be empty.");
                                    i++;
                                }
                                else if (file.Length > _maxFileSizeInBytes)
                                {
                                    writer.WriteLine($"Error,{name},{string.Empty},{file.FullName},Files cannot be larger than {_maxFileSizeInBytes / 1024 / 1024 / 1024}GB.");
                                    i++;
                                }
                                else if (name.Length > _maxFileNameLength)
                                {
                                    writer.WriteLine($"Error,{name},{string.Empty},{file.FullName},File names cannot exceed {_maxFileNameLength} characters.");
                                    i++;
                                }
                                else if (file.FullName.Length > _maxFileNameLength)
                                {
                                    writer.WriteLine($"Warning,{name},{string.Empty},{file.FullName},SharePoint Online has a limit of {_maxFileNameLength} which includes the parent URL. Consider flattening or reducing the folder structure path length.");
                                    i++;
                                }
                            }
                            break;
                        }
                    }

                    if (i > 0)
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine(i + " issues discovered parsing " + count + " files.  Refer to FileCheckerResults.csv for additional details.");
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine($"For additional information on file and folder name restrictions see also {_supportUrl}.");
                    }
                    else if (i == 0)
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine(i + " issues discovered parsing " + count + " files.");
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine($"For additional information on file and folder name restrictions see also {_supportUrl}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception Occurred: " + ex.Message);
                Console.Error.Close();
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }
    }
}