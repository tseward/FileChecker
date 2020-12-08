using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FileChecker
{
    class Program
    {
        private static Regex _pattern = new Regex(@"[\\\|*\:?/<>]+", RegexOptions.Compiled);
        private static string _resultsFileName = "FileCheckerResults.csv";
        private static decimal _maxFileSizeInBytes = 107374182400;
        private static int _maxFileNameLength = 400;
        private static string _supportUrl = 
            "https://support.microsoft.com/office/invalid-file-names-and-file-types-in-onedrive-and-sharepoint-64883a5d-228e-48f5-b3d2-eb39e07630fa";

        static void Main(string[] args)
        {
            var dir = string.Empty;
            var append = false;
            
            var argsList = args.ToList<string>();
            argsList.Sort();

            //add '#' and '%'
            if(argsList.Contains("--legacy"))
            {
                _pattern = new Regex(@"[\\\|*\:?/<>#%]+", RegexOptions.Compiled);
            }

            if(argsList.Contains("--append"))
            {
                append = true;
            }

            if(!argsList.Contains("--path"))
            {
                    Console.WriteLine("Usage: FileChecker --path <path> [--legacy][--append]");
                    Environment.Exit(0);
            }
            else
            {
                //making the wild assumption that the next item in the List will be the directory path
                var idx = argsList.IndexOf("--path") + 1;
                argsList.RemoveRange(0, idx);
                dir = argsList[0];
            }

            Stream stream = null;

            try
            {
                int count = 0;

                if(append)
                {
                    stream = new FileStream(_resultsFileName, FileMode.Append);
                }
                else
                {
                    stream = new FileStream(_resultsFileName, FileMode.Create);
                }

                using (StreamWriter writer = new StreamWriter(stream))
                {
                    stream = null;
                    if(!append)
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

                                var fileName = Path.GetFileName(file.Name.ToLower());
                                var matches = _pattern.Matches(fileName);

                                Console.WriteLine($"{count}. {fileName}");

                                var namespaces = new List<string>() { ".lock", "CON", "PRN", "AUX", "NUL", "COM1", 
                                    "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", 
                                    "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "desktop.ini" };
                                var extensions = new List<string>() { ".tmp", ".ds_store" };
                                var nameContents = new HashSet<string>() { "_vti_" };
                                var extension = Path.GetFileName(file.Extension.ToLower());


                                foreach(var content in nameContents)
                                {
                                    if(fileName.Contains(content))
                                    {
                                        writer.WriteLine($"Error,{fileName},{content},{file.FullName},File cannot contain {content}.");
                                        i++;
                                    }
                                }

                                if (extensions.Contains(extension))
                                {
                                    writer.WriteLine($"Error,{fileName},{extension},{file.FullName},Files cannot be of the following type {extension}. With Microsoft 365 Group-connected Team sites you cannot upload these files.");
                                    i++;
                                }
                                else if (fileName.Equals(namespaces))
                                {
                                    writer.WriteLine($"Error,{fileName},{extension},{file.FullName},Filenames cannot be one of the following type {namespaces}. Also avoid these names followed immediately by an extension; for example NUL.txt is not recommended.");
                                    i++;
                                }
                                else if (matches.Count > 0)
                                {
                                    var matchChars = string.Empty;

                                    foreach(var match in matches)
                                    {
                                        matchChars += $"{match} ";
                                    }
                                    writer.WriteLine($"Error,{fileName},{matchChars},{file.FullName},You cannot use the following character anywhere in a file name {matchChars}.");
                                    i++;
                                }
                                else if (fileName.StartsWith("_", StringComparison.OrdinalIgnoreCase))
                                {
                                    writer.WriteLine($"Warning,{fileName},_,{file.FullName},If you use an underscore character (_) at the beginning of a file name the file will be a hidden file when using Open in Explorer.");
                                    i++;
                                }
                                else if (fileName.Contains(".."))
                                {
                                    writer.WriteLine($"Error,{fileName},..{file.FullName},You cannot use the period character consecutively in the middle of a file name.");
                                    i++;
                                }
                                else if (fileName.EndsWith(".", StringComparison.OrdinalIgnoreCase))
                                {
                                    writer.WriteLine($"Warning,{fileName},.,{file.FullName},Do not end a file or directory name with a period. Although the underlying file system may support such names, the Windows shell and user interface does not.");
                                   i++;
                                }
                                else if (file.Length.Equals(0))
                                {
                                    writer.WriteLine($"Error,{fileName},{string.Empty},{file.FullName},Files cannot be empty.");
                                    i++;
                                }
                                else if (file.Length > _maxFileSizeInBytes)
                                {
                                    writer.WriteLine($"Error,{fileName},{string.Empty},{file.FullName},Files cannot be larger than {_maxFileSizeInBytes / 1024 / 1024 / 1024}GB.");
                                    i++;
                                }
                                else if (fileName.Length > _maxFileNameLength)
                                {
                                    writer.WriteLine($"Error,{fileName},{string.Empty},{file.FullName},File names cannot exceed {_maxFileNameLength} characters.");
                                    i++;
                                }
                                else if (file.FullName.Length > _maxFileNameLength)
                                {
                                    writer.WriteLine($"Warning,{fileName},{string.Empty},{file.FullName},SharePoint Online has a limit of {_maxFileNameLength} characters which includes the parent URL. Consider flattening or reducing the folder structure path length.");
                                    i++;
                                }
                            }
                            break;
                        }
                    }

                    if (i > 0)
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine($"{i} issues discovered parasing {count} files. Refer to {_resultsFileName} for additional details.");
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine($"For additional information on file and folder name restrictions see {_supportUrl}.");
                    }
                    else if (i == 0)
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine($"{i} issues discovered parsing {count} files.");
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine($"For additional information on file and folder name restrictions see {_supportUrl}.");
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