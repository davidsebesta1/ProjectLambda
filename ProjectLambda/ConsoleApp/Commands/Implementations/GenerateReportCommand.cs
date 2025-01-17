using ProjectLambda.ConsoleApp.Commands.Interfaces;
using ProjectLambda.Database;
using ProjectLambda.Models;

namespace ProjectLambda.ConsoleApp.Commands.Implementations
{
    public class GenerateReportCommand : ICommand
    {
        public string Command => "GenerateReport";

        public string Description => "Generates a report about a specific user and his ordered lunches.";

        public bool Execute(ArraySegment<string> args, out string response)
        {
            if (args.Count != 3)
            {
                response = "Please specify target username, year and month.";
                return false;
            }

            string username = args[0];
            if (!int.TryParse(args[1], out int year))
            {
                response = "Unable to parse year (argument 2)";
                return false;
            }

            if (year <= 0 || year > DateTime.Now.Year)
            {
                response = "Please enter valid year";
                return false;
            }

            if (!int.TryParse(args[2], out int month))
            {
                response = "Unable to parse month (argument 3)";
                return false;
            }

            if (month < 1 || month > 12)
            {
                response = "Please enter valid month between 1 and 12";
                return false;
            }

            List<User> list = DataRetriever<User>.Instance.GetAll().GetAwaiter().GetResult();
            foreach (User user in list)
            {
                if (user.Username != username)
                    continue;

                response = user.GenerateMonthlyReportAsync(year, month).GetAwaiter().GetResult().ToString();
                return true;
            }

            response = "Unable to find specified user";
            return false;
        }
    }
}
