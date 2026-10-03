namespace Crm.Application.Common.Models;

public class Link
{
    public string Href { get; set; } = string.Empty;
    public string Rel { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
}

public class LinkedResource
{
    public List<Link> Links { get; set; } = new();
}

public class LinkedResource<T> : LinkedResource
{
    public T Data { get; set; } = default!;
}

public static class HateoasExtensions
{
    public static LinkedResource<T> WithSelfLink<T>(this LinkedResource<T> resource, string href, string method = "GET")
    {
        resource.Links.Add(new Link { Href = href, Rel = "self", Method = method });
        return resource;
    }

    public static LinkedResource<T> WithLink<T>(this LinkedResource<T> resource, string href, string rel, string method = "GET")
    {
        resource.Links.Add(new Link { Href = href, Rel = rel, Method = method });
        return resource;
    }
}
