using System.Net.WebSockets;
using System.Reflection;

namespace WebWind.Core;

public static class ViewCache
{

    private static Dictionary<string, Type> PathToType = new Dictionary<string, Type>();
    internal static Dictionary<WebSocket, ClientInterface> Views = new Dictionary<WebSocket, ClientInterface>();
    
    static ViewCache()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach(Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(RouteAttribute), true).Length > 0)
                {
                    string path = type.GetCustomAttribute<RouteAttribute>().Path;
                    Log.Info($"ViewCache: {type} available at '{path}'");
                    PathToType.Add(path, type);
                }
            }
        }
    }

    internal static void CreateView(WebSocket webSocket, string path)
    {
        if (!PathToType.ContainsKey(path))
        {
            return;
        }

        Task.Run(() =>
        {
            ClientInterface clientInterface = new ClientInterface(webSocket);
            ClientInterface.s_LocalClientInterface.Value = clientInterface;
            Views[webSocket] = clientInterface;
            SubmitCssAndJs();
            clientInterface.m_RootComponent = Activator.CreateInstance(PathToType[path]) as Component;
        });
        
    }

    private static void SubmitCssAndJs()
    {
        // Get all loaded assemblies
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            string[] resourceNames = assembly.GetManifestResourceNames();

            foreach (string resourceName in resourceNames)
            {
                // Check if the resource is a CSS file
                if (resourceName.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                {
                    // Read the CSS content as a string
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string cssContent = reader.ReadToEnd();

                        // Send the CSS content to the frontend
                        ClientInterface.Current.JS.Eval($"{{const styleElement = document.createElement('style');" +
                                                        $"styleElement.textContent = `{cssContent}`;" +
                                                        $"document.head.appendChild(styleElement);}}");
                    }
                }
                else if (resourceName.EndsWith(".js", StringComparison.OrdinalIgnoreCase) && !resourceName.EndsWith("index.js"))
                {
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string jsSource = reader.ReadToEnd();

                        // Send the JS src to the frontend
                        ClientInterface.Current.JS.Eval(jsSource);
                    }
                }
            }
        }
    }
    internal static void DisposeView(WebSocket webSocket)
    {
        if (Views.ContainsKey(webSocket))
        {
            Views.Remove(webSocket);
        }
    }
}