using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.DotNet.Cli.Utils;

namespace ZipGit
{
    class Program
    {
        private static string[] Run(string workingDirectory, string commandName, params string[] args)
        {
            var lines = new List<string>();
            var command = Command
                .Create(commandName, args)
                .WorkingDirectory(workingDirectory);
            Console.WriteLine($"{command.CommandName} {command.CommandArgs}");
            var result = command.OnOutputLine(lines.Add).Execute();
            if (result.ExitCode != 0)
            {
                Environment.Exit(result.ExitCode);
            }
            return lines.ToArray();
        }

        static void Main(string[] args)
        {
            string currentPath = Directory.GetCurrentDirectory();
            string path = args.Length > 0 ? Path.GetFullPath(args[0]) : currentPath;
            var lines = Run(path, "git", "ls-files");
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempPath);
            try
            {
                var directory = new DirectoryInfo(path);
                string baseDirectory = Path.Combine(tempPath, directory.Name);
                foreach (var line in lines)
                {
                    string sourceFilePath = Path.Combine(path, line);
                    string tempFilePath = Path.Combine(baseDirectory, line);
                    Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));
                    File.Copy(sourceFilePath, tempFilePath);
                    Console.WriteLine(Path.GetRelativePath(tempPath, tempFilePath));
                }
                string zipFilePath = Path.Combine(currentPath, $"{directory.Name}.zip");
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }
                ZipFile.CreateFromDirectory(
                    baseDirectory,
                    zipFilePath,
                    default,
                    includeBaseDirectory: true);
            }
            finally
            {
                Directory.Delete(tempPath, recursive: true);
            }
        }
    }
}
