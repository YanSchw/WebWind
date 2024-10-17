namespace WebWind.Core;

[AttributeUsage(AttributeTargets.Class)]
public class HtmlTagAttribute : Attribute
{
    
    public string Tag { get; set; }
    
    public HtmlTagAttribute(string tag)
    {
        Tag = tag;
    }
}