namespace Aelfweard.Dns
{
    public enum Opcode : byte
    {
        Query = 0,
        IQuery = 1,
        Status = 2,
        Notify = 4,
        Update = 5
    }
}
