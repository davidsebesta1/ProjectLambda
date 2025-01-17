using ProjectLambda.ConsoleApp.Commands.Interfaces;
using ProjectLambda.Models;

namespace ProjectLambda.ConsoleApp.Commands.Implementations
{
    public class ImportCsvCommand : ICommand
    {
        public string Command => "ImportCsv";

        public string Description => "Imports a specified csv file as lunches. Creates new if it doesn't exists.";

        public bool Execute(ArraySegment<string> args, out string response)
        {
            if (args.Count == 0)
            {
                response = "Please specify file";
                return false;
            }

            if (!File.Exists(args[0]))
            {
                response = "Specified file doesn't exists, please specify absolute path to the file (drag and drop is possible).";
                return false;
            }

            using (StreamReader reader = new StreamReader(File.OpenRead(args[0])))
            {
                string firstLine = reader.ReadLine();

                if (firstLine != Lunch.CsvRowHeader)
                {
                    response = "Specified file doesn't have proper csv header or is missing.";
                    return false;
                }

                string line = null;

                List<string> list = new List<string>();
                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(line);
                }

                bool success = Lunch.LoadFromCsvRow(list).GetAwaiter().GetResult();

                response = success ? $"Successfully imported {list.Count} lunches." : "Failed to import csv";
                return success;
            }
        }
    }
}
