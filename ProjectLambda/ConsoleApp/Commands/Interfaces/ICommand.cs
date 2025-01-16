
namespace ProjectLambda.ConsoleApp.Commands.Interfaces
{
    /// <summary>
    /// Base interface for all command clases.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Command name.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Description of the command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Executes the command with specified arguments.
        /// </summary>
        /// <param name="args">Arguments to pass in.</param>
        /// <param name="response">Text response of the command.</param>
        /// <returns>Whether the execution was successful.</returns>
        public abstract bool Execute(ArraySegment<string> args, out string response);
    }
}
