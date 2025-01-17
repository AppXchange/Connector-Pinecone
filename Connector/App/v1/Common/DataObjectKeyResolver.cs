namespace Connector.App.v1.Common;

using System;

/// <summary>
/// Resolves unique keys for data objects based on their Id property.
/// </summary>
/// <typeparam name="T">The type of data object to resolve keys for.</typeparam>
public class DataObjectKeyResolver<T> : IChangeResolver<T> where T : class
{
    /// <summary>
    /// Resolves a unique key for the given data object.
    /// </summary>
    /// <param name="dataObject">The data object to resolve a key for.</param>
    /// <returns>A string representing the unique key for the data object.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Id property is missing or empty.</exception>
    public string ResolveKey(T dataObject)
    {
        if (dataObject == null)
        {
            throw new ArgumentNullException(nameof(dataObject));
        }

        // Get the Id property using reflection
        var idProperty = typeof(T).GetProperty("Id") 
            ?? throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");

        var id = idProperty.GetValue(dataObject)?.ToString();
        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidOperationException($"Id property of {typeof(T).Name} is null or empty");
        }

        return id;
    }
}

public interface IChangeResolver<T> where T : class
{
}