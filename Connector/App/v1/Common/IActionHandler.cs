namespace Connector.App.v1.Common;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines the contract for handling actions on data objects.
/// </summary>
/// <typeparam name="T">The type of data object this handler processes.</typeparam>
public interface IActionHandler<T>
{
    /// <summary>
    /// Handles an action on the specified data object.
    /// </summary>
    /// <param name="action">The name of the action to perform.</param>
    /// <param name="data">The data object to perform the action on.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleActionAsync(string action, T data, CancellationToken cancellationToken);
} 