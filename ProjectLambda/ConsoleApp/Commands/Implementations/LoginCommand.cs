using ProjectLambda.ConsoleApp.Commands.Interfaces;
using ProjectLambda.Database;
using ProjectLambda.Models;

namespace ProjectLambda.ConsoleApp.Commands.Implementations
{
    public class LoginCommand : ICommand
    {
        public string Command => "login";

        public string Description => "Logins you as specified user with specified password";

        public bool Execute(ArraySegment<string> args, out string response)
        {
            if (args.Count != 2)
            {
                response = "Please enter username and password";
                return false;
            }

            string username = args[0];
            string password = args[1];

            foreach (User user in DataCacher<User>.Instance.GetAll().GetAwaiter().GetResult())
            {
                if (user.Username != username)
                {
                    continue;
                }

                if (!user.CheckPassword(password))
                {
                    response = "Incorrent password!";
                    return false;
                }

                ConsoleMenu.LoggedInAs = user;
                ConsoleMenu.Password = password;

                response = "Logged in!";
                return true;
            }

            response = "User not found";
            return false;
        }
    }
}
