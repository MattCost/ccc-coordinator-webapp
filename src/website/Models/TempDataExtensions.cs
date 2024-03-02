using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CCC.website.Models;

public static class TempDataExtensions
{
    private static JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString | System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters = {
            new JsonStringEnumConverter()
        }
    };

    public static void Set<T>(this ITempDataDictionary tempData, string key, T value, JsonSerializerOptions? options = null) where T : class
    {
        tempData[key] = JsonSerializer.Serialize(value, options ?? _options);
    }

    public static T? Get<T>(this ITempDataDictionary tempData, string key, JsonSerializerOptions? options = null) where T : class
    {
        tempData.TryGetValue(key, out object? o);
        return o == null ? null : JsonSerializer.Deserialize<T>((string)o, options ?? _options);
    }

    public static bool TryGet<T>(this ITempDataDictionary tempData, string key, out T? value, JsonSerializerOptions? options = null) where T : class
    {
        value = null;
        if (tempData.TryGetValue(key, out object? o))
        {
            if (o is null) return false;

            value = JsonSerializer.Deserialize<T>((string)o, options ?? _options);
            return true;
        }
        return false;
    }

    public static T? Peek<T>(this ITempDataDictionary tempData, string key, JsonSerializerOptions? options = null) where T : class
    {
        var o = tempData.Peek(key);
        return o == null ? null : JsonSerializer.Deserialize<T>((string)o, options ?? _options);
    }

    public static bool TryPeek<T>(this ITempDataDictionary tempData, string key, out T? value, JsonSerializerOptions? options = null) where T : class
    {
        value = null;

        var o = tempData.Peek(key);
        if (o is null) return false;

        value = JsonSerializer.Deserialize<T>((string)o, options ?? _options);
        return value != null;
    }

}