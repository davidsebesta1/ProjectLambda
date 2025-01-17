using ProjectLambda.Database;

namespace ProjectLambda.Configuration
{
    /// <summary>
    /// Config file class.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Database ip to connect to.
        /// </summary>
        public string DatabaseIP { get; set; } = "127.0.0.1";

        /// <summary>
        /// Name of user to login to the db as.
        /// </summary>
        public string LoginUser { get; set; }

        /// <summary>
        /// User password.
        /// </summary>
        public string LoginPassword { get; set; }

        /// <summary>
        /// Initial db to start with instead of having to run "USE dbName;".
        /// </summary>
        public string InitialDatabase { get; set; }

        /// <summary>
        /// Isolation level of transactions.
        /// </summary>
        public IsolationLevels IsolationLevel { get; set; } = IsolationLevels.REPEATABLE_READ;
    }
}
