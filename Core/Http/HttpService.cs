using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static spinner.JsonParser;

namespace spinner;

public class InvalidJsonExeption() : Exception($"The returned json is missing or malformed") { }

public class HttpContextOptions
{
    public string BaseUri { get; init; } = "";
    public TimeSpan BaseTimeout { get; init; } = TimeSpan.FromSeconds(5);
}

public class HttpResponse : IDisposable
{
    public JsonDocument? Document { private get; init; }
    public String? Content { get; init; }
    public HttpStatusCode StatusCode { get; init; }

    public void Dispose()
    {
        if (Document is null)
        {
            return;
        }
        Document.Dispose();
    }

    public JsonValue JsonFind(string path)
    {
        if (Document is null)
        {
            return new() { Path = path, Value = "" };
        }

        return JsonParser.Find(path, Document);
    }

    public JsonResponse JsonFind(string path, Scope scope)
    {
        var r = ResponseOperator.Parse(new ParseContext(path));

        if (!r.Success)
        {
            return new()
            {
                Path = path,
                Value = path,
                Type = JsonResponseOperatorTokenType.Key,
            };
        }

        var jsonToken = (JsonResponseOperatorToken)r.Token;
        var key = jsonToken.Key.ToString(path);

        if (Document is null)
        {
            if (jsonToken.Type == JsonResponseOperatorTokenType.Status)
            {
                return new()
                {
                    Found = true,
                    Path = path,
                    Value = $"{((int)StatusCode)}",
                    Key = key,
                    Type = jsonToken.Type,
                };
            }
            return new() { Path = path, Value = path };
        }

        switch (jsonToken.Type)
        {
            case JsonResponseOperatorTokenType.Operator:
                var jsonEl = JsonFind(key);
                return new()
                {
                    Path = path,
                    Found = jsonEl.Found,
                    Key = key,
                    Value = jsonEl.Value,
                    Type = jsonToken.Type,
                };

            case JsonResponseOperatorTokenType.Status:
                return new()
                {
                    Found = true,
                    Path = path,
                    Value = $"{((int)StatusCode)}",
                    Key = key,
                    Type = jsonToken.Type,
                };

            default:
                var val = scope.Get(key);

                return new()
                {
                    Path = path,
                    Key = key,
                    Found = val is not null,
                    Value = val ?? "",
                    Type = jsonToken.Type,
                };
        }
    }

    public override string ToString()
    {
        return $"{StatusCode}\n{Content}\n{Document?.RootElement.ToString()}\n";
    }
}

public partial class HttpContext : IDisposable
{
    protected HttpClient Client;
    public object? Error { get; protected set; }
    public object? ContextResponse { get; protected set; }
    public object? ContextValue { get; protected set; }

    protected JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public HttpContext(HttpContextOptions options)
    {
        AppContext.SetSwitch("System.Net.DisableIPv6", true);
        Client = new() { BaseAddress = new(options.BaseUri), Timeout = options.BaseTimeout };
    }

    public HttpContext()
    {
        HttpContextOptions options = new();
        AppContext.SetSwitch("System.Net.DisableIPv6", true);
        Client = new() { BaseAddress = new(options.BaseUri), Timeout = options.BaseTimeout };
    }

    public async Task<HttpResponse> Get(string path)
    {
        HttpResponseMessage response = await Client.GetAsync(path);
        return await Process(response);
    }

    public async Task<HttpResponse> Post(string path, object? model)
    {
        var jsonContent = JsonSerializer.Serialize(model ?? new { });
        StringContent stringContent = new StringContent(
            jsonContent,
            Encoding.UTF8,
            "application/json"
        );
        HttpResponseMessage response = await Client.PostAsync(path, stringContent);
        return await Process(response);
    }

    public async Task<HttpResponse> Patch(string path, object? model)
    {
        var jsonContent = JsonSerializer.Serialize(model ?? new { });
        StringContent stringContent = new StringContent(
            jsonContent,
            Encoding.UTF8,
            "application/json"
        );
        HttpResponseMessage response = await Client.PatchAsync(path, stringContent);
        return await Process(response);
    }

    public async Task<HttpResponse> Put(string path, object? model)
    {
        var jsonContent = JsonSerializer.Serialize(model ?? new { });
        StringContent stringContent = new StringContent(
            jsonContent,
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage response = await Client.PutAsync(path, stringContent);
        return await Process(response);
    }

    public async Task<HttpResponse> Delete(string path)
    {
        HttpResponseMessage response = await Client.DeleteAsync(path);
        return await Process(response);
    }

    private async Task<HttpResponse> Process(HttpResponseMessage response)
    {
        ContextResponse = response;
        var contentType = response.Content.Headers.ContentType;
        var res = await response.Content.ReadAsStringAsync();
        bool isJsonWithCharset =
            contentType?.ToString() == "application/json"
            || contentType?.ToString() == "application/json; charset=utf-8";

        if (!isJsonWithCharset)
        {
            return new() { Content = res, StatusCode = response.StatusCode };
        }

        try
        {
            JsonDocument doc = JsonDocument.Parse(res);

            return new() { Document = doc, StatusCode = response.StatusCode };
        }
        catch (Exception)
        {
            return new() { StatusCode = response.StatusCode };
        }
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}
