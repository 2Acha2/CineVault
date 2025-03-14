namespace CineVault.API.Models.Api;

public class ApiRequest<T>
{

    public T Data { get; set; }

    public Dictionary<string, string> Meta { get; set; }

    public ApiRequest()
    {
        Meta = new Dictionary<string, string>();
    }
}

