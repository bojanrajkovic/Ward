namespace Ward.Dns
{
    /// <summary>
    /// A DNS RR CLASS/QCLASS.
    /// </summary>
    /// <remarks>
    /// The first three values appear only in responses. <see cref="Any" />
    /// and <see cref="None"/> are QCLASS values and only appears in questions,
    /// but are included here for convenience.
    /// </remarks>
    public enum Class : ushort
    {
        /// <summary>
        /// Internet
        /// </summary>
        Internet = 1,
        /// <summary>
        /// ChaosNet
        /// </summary>
        /// <remarks>
        /// See D. Moon, "Chaosnet", A.I. Memo 628, Massachusetts Institute of Technology Artificial Intelligence Laboratory, June 1981.
        /// </remarks>
        Chaos = 3,
        /// <summary>
        /// Hesiod 
        /// </summary>
        /// <remarks>
        /// See Dyer, S., and F. Hsu, "Hesiod", Project Athena Technical Plan - Name Service, April 1987.
        /// </remarks>
        Hesiod = 4,
        /// <summary>
        /// A QCLASS value, indicating no class specified.
        /// </summary>
        /// <remarks>
        /// For use with queries only.
        /// </remarks>
        None = 254,
        /// <summary>
        /// A QCLASS value, indicating any class is acceptable.
        /// </summary>
        /// <remarks>
        /// For use with queries only.</remarks>
        Any = 255
    }
}
