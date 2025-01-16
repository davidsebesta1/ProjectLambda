using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ProjectLambda.ConsoleApp.Commands.Interfaces;

namespace ProjectLambda.ConsoleApp.Commands
{
    public static class CommandHandler
    {
        public static Dictionary<string, ICommand> RegisteredCommands = new Dictionary<string, ICommand>();

        public static bool TryExecuteCommand(string commandName, ArraySegment<string> args, out string response)
        {
            if (RegisteredCommands == null)
            {
                response = "Command not found";
                return false;
            }

            if (RegisteredCommands.TryGetValue(commandName, out ICommand command))
            {
                return command.Execute(args, out response);
            }

            response = "Command not found";
            return false;
        }

        public static void RegisterAllCommands()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.GetInterfaces().Contains(typeof(ICommand)) || type.IsAbstract || type.IsInterface)
                    continue;

                TryRegisterCommand(type);
            }
        }

        private static bool TryRegisterCommand(Type type)
        {
            if (Activator.CreateInstance(type) is not ICommand command)
            {
                return false;
            }

            RegisteredCommands.Add(command.Command, command);
            return true;
        }

        public static bool TryRegisterCommand<T>() where T : ICommand
        {
            return TryRegisterCommand(typeof(T));
        }
    }
}
