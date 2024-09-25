﻿using System.Net;
using System.Text.Json;

namespace Money.ApiClient;

public class ApiClientResponse(HttpStatusCode code, string content)
{
    /// <summary>
    ///     HTTP код ответа.
    /// </summary>
    public HttpStatusCode Code { get; } = code;

    /// <summary>
    ///     Содержимое ответа в строковом представлении.
    /// </summary>
    public string StringContent { get; } = content;
}

public class ApiClientResponse<T>(HttpStatusCode code, string content) : ApiClientResponse(code, content)
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    ///     Содержимое ответа.
    /// </summary>
    public T? Content => DeserializeContent();

    private T? DeserializeContent()
    {
        if (typeof(T) == typeof(string))
        {
            return (T)Convert.ChangeType(StringContent, typeof(T));
        }

        return JsonSerializer.Deserialize<T>(StringContent, _serializerOptions);
    }
}