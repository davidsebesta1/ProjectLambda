using ProjectLambda.ConsoleApp.Commands.Interfaces;
using ProjectLambda.Database;
using ProjectLambda.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLambda.ConsoleApp.Commands.Implementations
{
    public class OrderLunchCommand : ICommand
    {
        public string Command => "orderLunch";

        public string Description => "Orders specified lunch at specified date. (wNumber can also be specified if multiple exists at same date)";

        public bool Execute(ArraySegment<string> args, out string response)
        {
            if(args.Count == 0)
            {
                response = "Not enough arguments.";
                return false;
            }

            if (!DateTime.TryParseExact(args[0].Trim(), "dd/MM/yyyy", null, DateTimeStyles.None, out DateTime dateTime))
            {
                response = "Specified date is not in format dd/MM/yyyy.";
                return false;
            }

            int index = 0;
            if(args.Count == 2)
            {
                if (!int.TryParse(args[1], out index))
                {
                    response = "Specified index is not valid";
                    return false;
                }
            }

            var lunches = DataCacher<Lunch>.Instance.GetAllBy(n => n.Date == dateTime).GetAwaiter().GetResult();

            Lunch lunch = lunches[index];
            //LunchOrder order = new LunchOrder(lunch.ID, );

            response = "Ok";
            return true;
        }
    }
}
