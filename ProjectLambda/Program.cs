using ProjectLambda.Configuration;
using ProjectLambda.ConsoleApp;
using ProjectLambda.Logging;
using Config = ProjectLambda.Configuration.Config;

namespace ProjectLambda
{
    public class Program
    {
        /// <summary>
        /// Instance of the config.
        /// </summary>
        public static Config Config { get; private set; }

        /// <summary>
        /// Entry point.
        /// </summary>
        public static async Task Main(string[] args)
        {
            await Logger.Log("Loading config");

            await LoadConfigAsync();

            await Logger.Log("Done!");

            ConsoleMenu.Init();
        }

        /// <summary>
        /// Loads and validates the config file.
        /// </summary>
        public static async Task LoadConfigAsync()
        {
            Config = await ConfigManager.LoadConfigAsync(ValidateConfig);
        }

        public static bool ValidateConfig(Config config)
        {
            return true;
        }
    }
}
