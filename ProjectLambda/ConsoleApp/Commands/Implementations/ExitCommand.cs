using ProjectLambda.ConsoleApp.Commands.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectLambda.ConsoleApp.Commands.Implementations
{
    public class ExitCommand : ICommand
    {
        public string Command => "exit";

        public string Description => "Exits the program";

        public bool Execute(ArraySegment<string> args, out string response)
        {
            Environment.Exit(0);

            response = "Goodbye";
            return true;
        }
    }
}
