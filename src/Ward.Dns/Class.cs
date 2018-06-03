namespace Ward.Dns
{
    /// <summary>
    /// A DNS RR CLASS/QCLASS.
    /// </summary>
    /// <remarks>
    /// The first four values appear only in responses. <see cref="Class.Any">
    /// is a QCLASS value and only appears in questions, but is included
    /// here for convenience.
    /// </remarks>
    public enum Class : ushort
    {
        Internet = 1,
        Chaos = 3,
        Hesiod = 4,
        None = 254,
        Any = 255
    }
}
