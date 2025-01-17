using MySql.Data.MySqlClient;
using ProjectLambda.ConsoleApp.Commands.Interfaces;
using ProjectLambda.Database;
using ProjectLambda.Models;
using System.Globalization;

namespace ProjectLambda.ConsoleApp.Commands.Implementations
{
    public class OrderLunchCommand : ICommand
    {
        public string Command => "OrderLunch";

        public string Description => "Orders specified lunch at specified date. (Number can also be specified if multiple exists at same date)";

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

            if (orderedLunch != null)
            {
                response = $"You already have ordered lunch for date {orderedLunch.ReferencedLunch.Date.ToString("dd/MM/yyyy")}";
                return false;
            }

            int index = 0;
            if (args.Count == 2)
            {
                if (!int.TryParse(args[1], out index))
                {
                    response = "Specified index is not valid";
                    return false;
                }

            }
            var lunches = DataRetriever<Lunch>.Instance.GetAllBy(n => n.Date == dateTime).GetAwaiter().GetResult();

            if (index == lunches.Count + 1)
            {
                index--;
            }

            Lunch lunch = lunches[index];
            LunchOrder order = new LunchOrder(lunch.ID, ConsoleMenu.LoggedInAs.ID, false);

            List<(string, MySqlParameter[])> list = new List<(string, MySqlParameter[])>(2)
            {
                (LunchOrder.InsertQueryNoId, order.GetMySqlParameters(false)),
                (User.AddCreditQuery, ConsoleMenu.LoggedInAs.GetMySqlParametersEditCredit(-lunch.Price))
            };

            MySqlDatabaseConnection.Instance.ExecuteTransactionAsync(list);

            response = $"Ordered lunch {index + 1} from date {dateTime.ToString("dd/MM/yyyy")}";
            return true;
        }
    }
}
