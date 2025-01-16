using ProjectLambda.Logging;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProjectLambda.Configuration
{
    /// <summary>
    /// Class made for loading and parsing of a config file in yaml.
    /// </summary>
    public static class ConfigManager
    {
        public static readonly IDeserializer Deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
        public static readonly ISerializer Serializer = new SerializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();

        /// <summary>
        /// Loads and validates the config file.
        /// </summary>
        /// <param name="validationFunction">Function to check the config file after it is loaded. An exception is thrown if <see langword="false"/> is returned.</param>
        /// <returns>A new config file instance.</returns>
        public static async Task<Config> LoadConfigAsync(Func<Config, bool> validationFunction)
        {
            string dllLocation;
            string configPath;
            string workingDir = string.Empty;

            Config config = null;

            try
            {
                dllLocation = Extensions.GetExecutablePath();
                workingDir = Path.GetDirectoryName(dllLocation);
                configPath = Path.Combine(workingDir, "config.yaml");
                config = await TryReadConfig<Config>(configPath);
            }
            catch (YamlException yamlException)
            {
                await Logger.Log($"Error at reading config, fix any errors and try again!\nMessage: {yamlException.Message}");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                await Logger.Log($"Fatal at reading config: {ex.Message}");
                Environment.Exit(1);
            }

            await Logger.Log("Checking config integrity...");

            if (!validationFunction(config))
            {
                throw new ConfigurationException();
            }

            return config;
        }

        private static async Task<T> TryReadConfig<T>(string path) where T : class
        {
            try
            {
                T obj = await ReadAndCreate<T>(path);
                return obj;
            }
            catch (FileNotFoundException)
            {
                T obj = (T)Activator.CreateInstance(typeof(T))!;
                await Save(obj, path);

                await Logger.Log("Warning, a new config instance has been created. You might want to stop the program and configure it?", ConsoleColor.Yellow);
                return obj;
            }
        }

        private static async Task<T> ReadAndCreate<T>(string path) where T : class
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("Path must not be null");

            if (!Path.Exists(path))
                throw new FileNotFoundException("Specified file not found");

            using (StreamReader reader = new StreamReader(path))
            {
                string content = await reader.ReadToEndAsync();
                return Deserializer.Deserialize<T>(content);
            }
        }

        private static async Task Save<T>(T conf, string path) where T : class
        {
            string serialized = Serializer.Serialize(conf);
            using (StreamWriter writer = new StreamWriter(File.OpenWrite(path)))
            {
                await writer.WriteAsync(serialized);
            }
        }
    }

    /// <summary>
    /// Error that is thrown when the config file doesn't pass the validation function.
    /// </summary>
    public class ConfigurationException : Exception
    {

    }
}
