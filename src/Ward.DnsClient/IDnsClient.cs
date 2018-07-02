using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Ward.Dns;

namespace Ward.DnsClient
{
    /// <summary>
    /// Represents a generic DNS client interface.
    /// </summary>
    public interface IDnsClient
    {
        /// <summary>
        /// Asynchronously resolves the given question.
        /// </summary>
        /// <param name="question">The question to resolve.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A DNS resolve result.</returns>
        Task<IResolveResult> ResolveAsync(Question question, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously looks up the <paramref name="type"/> record in the
        /// given <paramref name="class"/> for the <paramref name="host"/>.
        /// </summary>
        /// <param name="host">The host to resolve.</param>
        /// <param name="type">The record type to resolve.</param>
        /// <param name="class">The record class to resolve.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A DNS resolve result.</returns>
        Task<IResolveResult> ResolveAsync(string host, Type type, Class @class, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously resolves all of the given questions.
        /// </summary>
        /// <param name="questions">The list of questions.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A DNS resolve result for all the questions.</returns>
        Task<IResolveResult> ResolveAsync(IEnumerable<Question> questions, CancellationToken cancellationToken = default);
    }
}
