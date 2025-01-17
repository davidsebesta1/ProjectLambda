using ProjectLambda.ConsoleApp.Commands.Interfaces;
using System.Reflection;

namespace ProjectLambda.ConsoleApp.Commands
{
    /// <summary>
    /// A static class for registering and executing commands.
    /// </summary>
    public static class CommandHandler
    {
        /// <summary>
        /// All registered commands.
        /// </summary>
        public static Dictionary<string, ICommand> RegisteredCommands = new Dictionary<string, ICommand>();

        /// <summary>
        /// Attempts to execute a command.
        /// </summary>
        /// <param name="commandName">Target command name.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="response">Response from the command.</param>
        /// <returns>Whether the command has successfully executed.</returns>
        public static bool TryExecuteCommand(string commandName, ArraySegment<string> args, out string response)
        {
            commandName = commandName.ToLower();
            if (RegisteredCommands == null)
            {
                response = "Command not found";
                return false;
            }

            if (RegisteredCommands.TryGetValue(commandName, out ICommand command))
            {
                try
                {
                    return command.Execute(args, out response);

                }
                catch (Exception ex)
                {
                    response = ex.Message;
                    Logging.Logger.LogError(ex);
                    return false;
                }
            }

            response = "Command not found";
            return false;
        }
        
        /// <summary>
        /// Registers all commands from executing assembly.s
        /// </summary>
        public static void RegisterAllCommands()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (!type.GetInterfaces().Contains(typeof(ICommand)) || type.IsAbstract || type.IsInterface)
                    continue;

                TryRegisterCommand(type);
            }
        }

        /// <summary>
        /// Attempts to register a command.
        /// </summary>
        /// <typeparam name="T">Target command type.</typeparam>
        /// <returns>Whether it was successfully registered.</returns>
        public static bool TryRegisterCommand<T>() where T : ICommand
        {
            return TryRegisterCommand(typeof(T));
        }

        private static bool TryRegisterCommand(Type type)
        {
            if (Activator.CreateInstance(type) is not ICommand command)
            {
                return false;
            }

            RegisteredCommands.Add(command.Command.ToLower(), command);
            return true;
        }
    }
}
