using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.DotNet.Cli.Utils;

namespace ZipGit
{
    class Program
    {
        private static string[] Run(string commandName, params string[] args)
        {
            var lines = new List<string>();
            var command = Command.Create(commandName, args);
            Console.WriteLine($"{command.CommandName} {command.CommandArgs}");
            var result = command.OnOutputLine(lines.Add).Execute();
            if (result.ExitCode != 0)
            {
                Environment.Exit(result.ExitCode);
            }
            return lines.ToArray();
        }

        static async Task Main()
        {
            var lines = Run("git", "ls-files");
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var line in lines)
                {
                    string entryName = Path.Combine(directory.Name, line);
                    Console.WriteLine(entryName);
                    archive.CreateEntryFromFile(line, entryName);
                }
            }
            string zipFilePath = Path.Combine(directory.FullName, $"{directory.Name}.zip");
            using var zipFile = File.OpenWrite(zipFilePath);
            stream.Seek(0, SeekOrigin.Begin);
            await stream.CopyToAsync(zipFile);
        }
    }
}
