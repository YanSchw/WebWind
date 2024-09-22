namespace WebWind.Core;

[AttributeUsage(AttributeTargets.Class)]
public class RouteAttribute : Attribute
{
    
    public string Path { get; set; }
    
    public RouteAttribute(string path)
    {
        Path = path;
    }
}