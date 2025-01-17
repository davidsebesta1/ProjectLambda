namespace ProjectLambda.Database
{
    /// <summary>
    /// Enum representing isolation levels of transactions.
    /// </summary>
    public enum IsolationLevels
    {
        READ_UNCOMMITTED,
        READ_COMMITTED,
        REPEATABLE_READ,
        SERIALIZABLE
    }
}
