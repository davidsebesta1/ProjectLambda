using ProjectLambda.ConsoleApp.Commands;
using ProjectLambda.Models;

namespace ProjectLambda.ConsoleApp
{
    /// <summary>
    /// Class for showcase console menu.
    /// </summary>
    public static class ConsoleMenu
    {
        public static User LoggedInAs { get; set; }
        public static string Password { get; set; }

        /// <summary>
        /// Inits the console and runs the endless loop.
        /// </summary>
        public static void Init()
        {
            CommandHandler.RegisterAllCommands();

            while (true)
            {
                PrintAllCommands();
                string input = Console.ReadLine();
                string[] arguments = input.Split(' ');

                if (!CommandHandler.TryExecuteCommand(arguments[0], new ArraySegment<string>(arguments, 1, arguments.Length - 1), out string response))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(response);
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(response);
            }
        }

        /// <summary>
        /// Prints all commands.
        /// </summary>
        public static void PrintAllCommands()
        {
            foreach (var commandEntry in CommandHandler.RegisteredCommands)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(commandEntry.Key);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" - ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(commandEntry.Value.Description);
            }
        }
    }
}
