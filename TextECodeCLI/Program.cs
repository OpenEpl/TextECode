using CommandLine;
using Microsoft.Extensions.Logging;
using OpenEpl.TextECode;
using QIQI.EProjectFile;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace OpenEpl.TextECodeCLI
{
    internal interface ICmd
    {
        public int Run(ILoggerFactory loggerFactory);
    }

    [Verb("generate", HelpText = "Generate text ecode files from *.e file")]
    internal class GenerateCmd: ICmd
    {
        [Value(0, Required = true, HelpText = "Set the input file (*.e)")]
        public string Input { get; set; }

        [Value(1, Required = false, HelpText = "Set the output project file path (*.eproject)")]
        public string Output { get; set; }

        [Option("source-set", HelpText = "Set the relative path of source set")]
        public string SourceSet { get; set; }

        [Option("program-out-file", HelpText = "Set the out file of the program")]
        public string ProgramOutFile { get; set; }

        [Option('c', "allow-cleanup", Default = true, HelpText = "Delete non-generated files", Required = false)]
        public bool DeleteNonGeneratedFiles { get; set; }

        public int Run(ILoggerFactory loggerFactory)
        {
            var doc = new EplDocument();
            using (var file = File.OpenRead(Input))
            {
                doc.Load(file);
            }
            if (Output is null)
            {
                Output = Path.ChangeExtension(Input, ".eproject");
            }
            var originDir = Path.GetDirectoryName(Path.GetFullPath(Input));
            var generator = new TextECodeGenerator(loggerFactory, doc, Output, new EComSearcher(new string[] { originDir }), originDir);
            if (!string.IsNullOrEmpty(SourceSet))
            {
                generator.SetSourceSet(SourceSet);
            }
            if (!string.IsNullOrEmpty(ProgramOutFile))
            {
                generator.SetProgramOutFile(ProgramOutFile);
            }
            generator.Generate();
            if (DeleteNonGeneratedFiles)
            {
                generator.DeleteNonGeneratedFiles();
            }
            return 0;
        }
    }

    [Verb("restore", HelpText = "Restore text ecode files to *.e file")]
    internal class RestoreCmd : ICmd
    {
        [Value(0, Required = true, HelpText = "Set the input project file path (*.eproject)")]
        public string Input { get; set; }

        [Value(1, Required = false, HelpText = "Set the output file (*.e)")]
        public string Output { get; set; }

        public int Run(ILoggerFactory loggerFactory)
        {
            if (Output is null)
            {
                Output = Path.ChangeExtension(Input, ".e");
            }
            var doc = new TextECodeRestorer(loggerFactory, Input).Restore();
            using (var file = File.Open(Output, FileMode.Create))
            {
                doc.Save(file);
            }
            return 0;
        }
    }

    [Verb("view", HelpText = "View a text ecode project (restore it to a temporary file and auto sync all changes)")]
    internal class ViewCmd : ICmd
    {
        [Value(0, Required = true, HelpText = "Set the input project file path (*.eproject)")]
        public string ProjectFile { get; set; }

        public int Run(ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<ViewCmd>();
            var fullProjectFilePath = Path.GetFullPath(ProjectFile);
            if (!File.Exists(ProjectFile))
            {
                logger.LogError("The input file \"{Path}\" is not found.", fullProjectFilePath);
                return 1;
            }
            logger.LogInformation("Restoring \"{Source}\"", fullProjectFilePath);
            var binProjectPath = Path.ChangeExtension(fullProjectFilePath, ".tmp.e");
            var stopWatchForRestore = new Stopwatch();
            {
                stopWatchForRestore.Restart();
                var doc = new TextECodeRestorer(loggerFactory, fullProjectFilePath).Restore();
                using var file = File.Open(binProjectPath, FileMode.Create);
                doc.Save(file);
                stopWatchForRestore.Stop();
            }
            logger.LogInformation("Restored to the temporary file \"{Path}\" in {ElapsedSeconds}s", binProjectPath, stopWatchForRestore.Elapsed.TotalSeconds);
            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(binProjectPath),
                Filter = Path.GetFileName(binProjectPath),
                NotifyFilter = NotifyFilters.CreationTime
                | NotifyFilters.LastAccess
                | NotifyFilters.LastWrite
                | NotifyFilters.Size
                | NotifyFilters.FileName
            };
            void sync()
            {
                try
                {
                    var doc = new EplDocument();
                    Stream file;
                    while (true)
                    {
                        try
                        {
                            file = File.OpenRead(binProjectPath);
                            break;
                        }
                        catch (IOException e)
                        {
                            if (e.HResult == unchecked((int)0x80070020))
                            {
                                logger.LogError(e, "The file is being used, retrying in 1s");
                                Thread.Sleep(1000);
                                continue;
                            }
                            throw;
                        }
                    }
                    var stopWatchForSync = new Stopwatch();
                    logger.LogInformation("Syncing...");
                    using (file)
                    {
                        stopWatchForSync.Restart();
                        doc.Load(file);
                        var originDir = Path.GetDirectoryName(binProjectPath);
                        var generator = new TextECodeGenerator(loggerFactory, doc, ProjectFile, new EComSearcher(new string[] { originDir }), originDir);
                        generator.Generate();
                        generator.DeleteNonGeneratedFiles();
                        stopWatchForSync.Stop();
                    }
                    logger.LogInformation("Synced in {ElapsedSeconds}s", stopWatchForSync.Elapsed.TotalSeconds);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to sync files");
                }
            }
            watcher.Changed += (sender, e) =>
            {
                sync();
            };
            watcher.Renamed += (sender, e) =>
            {
                if (e.FullPath == binProjectPath)
                {
                    sync();
                }
            };
            watcher.EnableRaisingEvents = true;
            using var process = Process.Start(new ProcessStartInfo()
            {
                FileName = Path.GetFullPath(binProjectPath),
                WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(binProjectPath)),
                UseShellExecute = true
            });
            process.WaitForExit();
            watcher.Dispose();
            if (process.ExitCode != 0)
            {
                logger.LogWarning("The viewer process exits with the code {Code}.", process.ExitCode);
            }
            else
            {
                logger.LogInformation("The viewer process exits with the code {Code}.", process.ExitCode);
            }
            sync();
            try
            {
                File.Delete(binProjectPath);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete the temporary file \"{Path}\"", binProjectPath);
            }
            return 0;
        }

    }

    internal class Program
    {
        private static int Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
            });

            return new Parser(x =>
            {
                x.CaseSensitive = false;
                x.CaseInsensitiveEnumValues = true;
                x.HelpWriter = Console.Error;
            }).ParseArguments<GenerateCmd, RestoreCmd, ViewCmd>(args)
              .MapResult(
                options => (options as ICmd)?.Run(loggerFactory) ?? 1,
                _ => 1);
        }
    }
}
