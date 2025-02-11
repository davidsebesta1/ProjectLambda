﻿using ProjectLambda.ConsoleApp.Commands.Interfaces;
using ProjectLambda.Database;
using ProjectLambda.Models;
using ProjectLambda.Pooling;
using System.Text;

namespace ProjectLambda.ConsoleApp.Commands.Implementations
{
    public class PrintLunchWeekCommand : ICommand
    {
        public string Command => "PrintWeek";

        public string Description => "Prints lunch that is available this week.";

        public bool Execute(ArraySegment<string> args, out string response)
        {
            StringBuilder sb = StringBuilderPool.Get();
            foreach (Lunch lunch in DataRetriever<Lunch>.Instance.GetAllBy(n => n.Date.IsInSameWeek(DateTime.Now)).GetAwaiter().GetResult())
            {
                sb.Append(lunch.Date.Date.ToString("dd/MM/yyyy"));
                sb.Append(" - ");
                sb.Append(lunch.Price);
                sb.AppendLine(" credits");
                sb.Append("Soup: ");
                sb.AppendLine(lunch.ReferencedSoup.Name);
                sb.Append("Main course: ");
                sb.AppendLine(lunch.ReferencedMainCourse.Name);
                sb.Append("Dessert: ");
                sb.AppendLine(lunch.ReferencedDessert.Name);
            }

            response = StringBuilderPool.ReturnToString(sb);
            return true;
        }
    }
}
