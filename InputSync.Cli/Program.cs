using Mono.Options;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace InputSync.Cli
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var options = GetOptionsFromFile();
            var shouldShowHelp = false;
            var optionsParser = new OptionSet()
            {
                {
                    "p|port=", "The port to run the InputSync server on.", p =>
                    {
                        if(!int.TryParse(p, out var port))
                            throw new ArgumentException("Invalid port value.");

                        options.Port = port;
                    }
                },
                {
                    "m|mouse-sensitivity=", "Change the virtual mouse sensitivity using a scale value.", m =>
                    {
                        if(!double.TryParse(m, out var mouseSensitivity))
                            throw new ArgumentException("Invalid mouse sensitivity value.");

                        options.MouseSensitivity = mouseSensitivity;
                    }
                },
                { "h|help", "Show a help message.", h => shouldShowHelp = h != null }
            };

            try
            {
                optionsParser.Parse(args);
            }
            catch (Exception e)
            {
                Console.Write("InputSync.exe: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `InputSync.exe --help` for more information.");
            }

            if (shouldShowHelp)
            {
                ShowHelp(optionsParser);
                return;
            }

            Console.WriteLine($"Starting InputSync server on port {options.Port}.");

            var server = new InputSyncServer(options);
            server.Start();

            Console.WriteLine("Press enter to stop the program...");
            Console.ReadLine();

            server.Stop();
        }

        private static InputSyncOptions GetOptionsFromFile()
        {
            try
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var config = Path.Combine(path, "config.json");
                if (File.Exists(config))
                {
                    var json = File.ReadAllText(config);
                    return JsonSerializer.Deserialize<InputSyncOptions>(json);
                }

                return new InputSyncOptions();
            }
            catch
            {
                return new InputSyncOptions();
            }
        }

        private static void ShowHelp(OptionSet options)
        {
            Console.WriteLine("Usage: InputSync.exe [option]*");
            Console.WriteLine("Starts an InputSync server used to receive input commands from a mobile device.");
            Console.WriteLine("");
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}
