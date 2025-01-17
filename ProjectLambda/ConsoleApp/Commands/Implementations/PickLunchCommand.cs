using ProjectLambda.ConsoleApp.Commands.Interfaces;
using ProjectLambda.Database;
using ProjectLambda.Models;
using System.Globalization;

namespace ProjectLambda.ConsoleApp.Commands.Implementations
{
    public class PickLunchCommand : ICommand
    {
        public string Command => "PickLunch";

        public string Description => "Marks launch at specified date as picked";

        public bool Execute(ArraySegment<string> args, out string response)
        {
            if (args.Count == 0)
            {
                response = "Not enough arguments.";
                return false;
            }

            if (ConsoleMenu.LoggedInAs == null)
            {
                response = "You are not logged in.";
                return false;
            }

            if (!DateTime.TryParseExact(args[0].Trim(), "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime dateTime))
            {
                response = "Specified date is not in format dd/MM/yyyy.";
                return false;
            }

            LunchOrder orderedLunch = DataRetriever<LunchOrder>.Instance.GetFirstBy(n => n.ReferencedUser == ConsoleMenu.LoggedInAs && n.ReferencedLunch.Date == dateTime).GetAwaiter().GetResult();

            if (orderedLunch == null)
            {
                response = $"You dont have lunch ordered for {dateTime.ToString("dd/MM/yyyy")}";
                return false;
            }

            if (orderedLunch.Picked.HasValue && orderedLunch.Picked.Value)
            {
                response = $"Lunch for {orderedLunch.ReferencedLunch.Date.ToString("dd/MM/yyyy")} is already picked";
                return false;
            }

            orderedLunch.Picked = true;
            orderedLunch.SaveAsync();


            response = $"Lunch for {orderedLunch.ReferencedLunch.Date.ToString("dd/MM/yyyy")} is now picked";
            return true;
        }
    }
}
