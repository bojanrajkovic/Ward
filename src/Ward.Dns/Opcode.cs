namespace Ward.Dns
{
    /// <summary>
    /// The DNS header opcode.
    /// </summary>
    public enum Opcode : byte
    {
        Query = 0,
        IQuery = 1,
        Status = 2,
        Notify = 4,
        Update = 5
    }
}
