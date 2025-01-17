namespace Connector.Client;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a paginated response from the Pinecone API.
/// </summary>
/// <typeparam name="TResult">The type of items in the response.</typeparam>
public class PaginatedResponse<TResult>
{
    /// <summary>
    /// Gets or initializes the current page number.
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; init; }

    /// <summary>
    /// Gets or initializes the number of items per page.
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; }

    /// <summary>
    /// Gets or initializes the total number of records available.
    /// </summary>
    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; init; }

    /// <summary>
    /// Gets or initializes the total number of pages available.
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    /// <summary>
    /// Gets or initializes the collection of items for the current page.
    /// </summary>
    [JsonPropertyName("items")]
    public IReadOnlyList<TResult> Items { get; init; } = Array.Empty<TResult>();

    /// <summary>
    /// Gets whether there is a next page available.
    /// </summary>
    [JsonIgnore]
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets whether there is a previous page available.
    /// </summary>
    [JsonIgnore]
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Gets the next page number if available, otherwise null.
    /// </summary>
    [JsonIgnore]
    public int? NextPage => HasNextPage ? Page + 1 : null;

    /// <summary>
    /// Gets the previous page number if available, otherwise null.
    /// </summary>
    [JsonIgnore]
    public int? PreviousPage => HasPreviousPage ? Page - 1 : null;

    /// <summary>
    /// Gets whether the response contains any items.
    /// </summary>
    [JsonIgnore]
    public bool HasItems => Items.Count > 0;
}