using System.Reflection;

namespace WebWind.Core;

[AttributeUsage(AttributeTargets.Class)]
public class HtmlTemplateAttribute : Attribute
{
    
    public string Path { get; set; }
    
    public HtmlTemplateAttribute(string path)
    {
        Path = path;
    }

    internal void Inject(Component component, HtmlTagAttribute tagAttribute)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string resourceName in resourceNames)
            {
                if (resourceName.EndsWith(Path))
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string html = reader.ReadToEnd();

                        ClientInterface.Current.JS.Eval($"{component.Id}.innerHTML = `{html}`;");
                    }
                }
            }
        }
    }
}