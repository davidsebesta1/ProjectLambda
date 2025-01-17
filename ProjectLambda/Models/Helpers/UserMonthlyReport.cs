using ProjectLambda.Pooling;
using System.Text;

namespace ProjectLambda.Models.Helpers
{
    /// <summary>
    /// Class representing a single monthly report for a user.
    /// </summary>
    public class UserMonthlyReport : IDisposable
    {
        /// <summary>
        /// Target user.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// User's orders.
        /// </summary>
        public List<LunchOrder> LunchOrders { get; private set; }

        /// <summary>
        /// Total lunches this month.
        /// </summary>
        public int TotalLunchesMonth { get; private set; }


        public UserMonthlyReport(User user, List<LunchOrder> lunchOrders, int totalLunches)
        {
            User = user;
            LunchOrders = lunchOrders;
            TotalLunchesMonth = totalLunches;
        }

        public void Dispose()
        {
            User = null;
            LunchOrders.Clear();
            LunchOrders = null;

            GC.SuppressFinalize(this);
        }

        public override string ToString()
        {
            StringBuilder sb = StringBuilderPool.Get();
            sb.Append(User.Username);
            sb.Append('(');
            sb.Append(User.FirstName);
            sb.Append(' ');
            sb.Append(User.LastName);
            sb.AppendLine(")");

            int len = Math.Max(User.Username.Length, User.FirstName.Length + User.LastName.Length + 1);
            for (int i = 0; i < len; i++)
            {
                sb.Append('-');
            }

            sb.AppendLine();

            double totalCost = 0;
            foreach (LunchOrder lunchOrder in LunchOrders)
            {
                sb.AppendLine(lunchOrder.ReferencedLunch.Date.ToString("dd/MM/yyyy"));
                sb.Append(lunchOrder.ReferencedLunch.ReferencedSoup);
                sb.Append(',');
                sb.Append(lunchOrder.ReferencedLunch.ReferencedMainCourse);
                sb.Append(',');
                sb.AppendLine(lunchOrder.ReferencedLunch.ReferencedDessert);

                totalCost += lunchOrder.ReferencedLunch.Price;
            }

            for (int i = 0; i < len; i++)
            {
                sb.Append('-');
            }

            sb.AppendLine();

            sb.AppendLine("Summary: ");
            sb.Append("Bought ");
            sb.Append(LunchOrders.Count);
            sb.Append('/');
            sb.Append(TotalLunchesMonth);
            sb.AppendLine(" lunches this month");
            sb.Append("For the price of ");
            sb.AppendLine(totalCost.ToString("0.##"));

            return StringBuilderPool.ReturnToString(sb);
        }
    }
}
