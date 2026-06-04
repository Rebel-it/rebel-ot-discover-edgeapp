namespace Rebelit.OT.Discover.EdgeApp.API.Resolvers;

public interface IDataSourceResolver
{
    /// <summary>
    /// Asynchronously resolves the unique identifier associated with the specified agent and source.
    /// </summary>
    /// <param name="sourceName">The name of the source context in which to resolve the agent. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the resolved unique identifier as a
    /// string</returns>
    Task<string> ResolveAsync(string sourceName);
}