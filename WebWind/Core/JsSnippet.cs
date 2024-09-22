using System.Net.WebSockets;
using Microsoft.VisualBasic;

namespace WebWind.Core;

public class JsSnippet
{
    internal WebSocket _webSocket;

    public JsSnippet(WebSocket webSocket)
    {
        _webSocket = webSocket;
    }
    

    public void CreateDocumentElement(string type, string id)
    {
        Eval($"var {id} = document.createElement('{type}');\n{id}.setAttribute('id', '{id}');\ndocument.querySelector('#root').appendChild({id});\n");
    }

    public void AppendChild(string parent, string child)
    {
        Eval($"{parent}.appendChild({child});\n");
    }

    public void Eval(string js)
    {
        WebSocketServer.SendJsToClient(_webSocket, js);
    }
    
}