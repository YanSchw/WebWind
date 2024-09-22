using System.Net.WebSockets;

namespace WebWind.Core;

public class ClientInterface
{
    internal static AsyncLocal<ClientInterface> s_LocalClientInterface = new AsyncLocal<ClientInterface>();
    public static ClientInterface Current => s_LocalClientInterface.Value;
    internal Dictionary<Int64, Component> m_ComponentRegistry = new Dictionary<long, Component>();

    internal WebSocket m_WebSocket;
    public WebSocket WebSocket => m_WebSocket;

    internal Int64 m_ViewComponentCounter = 1;

    public Int64 NextComponentId()
    {
        return m_ViewComponentCounter++;
    }

    internal Component m_RootComponent;
    
    public JsSnippet JS => new JsSnippet(m_WebSocket);


    public ClientInterface(WebSocket webSocket)
    {
        m_WebSocket = webSocket;
    }

    public void SetTitle(string title)
    {
        JS.Eval($"document.title = '{title}';");
    }

}