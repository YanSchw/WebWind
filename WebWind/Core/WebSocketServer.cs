using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Reflection;

namespace WebWind.Core
{
    public class WebSocketServer
    {
        public void Host(int port)
        {
            Log.Trace(" _ _ _       _    _ _ _  _         _ ");
            Log.Trace("| | | | ___ | |_ | | | |<_>._ _  _| |");
            Log.Trace("| | | |/ ._>| . \\| | | || || ' |/ . |");
            Log.Trace("|__/_/ \\___.|___/|__/_/ |_||_|_|\\___|");
            Log.Trace("                                     ");
            
            Task listenTask = Listen(port);
            listenTask.GetAwaiter().GetResult();
        }

        public async Task Listen(int port)
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            Log.Info($"Server running on ws://localhost:{port} and http://localhost:{port}");

            while (true)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClient(tcpClient)); // Handle each client in a separate task
            }
        }

        private async Task HandleClient(TcpClient tcpClient)
        {
            NetworkStream networkStream = tcpClient.GetStream();

            var buffer = new byte[1024];
            int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if (IsWebSocketHandshake(request))
            {
                await PerformWebSocketHandshakeAsync(networkStream, request);
                WebSocket webSocket = WebSocket.CreateFromStream(networkStream, isServer: true, subProtocol: null, keepAliveInterval: TimeSpan.FromMinutes(2));

                Log.Info("WebSocket connection accepted.");
                await HandleWebSocket(webSocket);
            }
            else if (IsHttpRequest(request))
            {
                await ServeHttpRequest(networkStream, request);
            }
            else
            {
                // Invalid request, close the connection
                tcpClient.Close();
            }
        }

        private bool IsWebSocketHandshake(string request)
        {
            return request.Contains("Upgrade: websocket");
        }

        private bool IsHttpRequest(string request)
        {
            return request.StartsWith("GET");
        }

        private async Task PerformWebSocketHandshakeAsync(NetworkStream stream, string request)
        {
            // Extract the WebSocket key from the handshake request
            string webSocketKey = request.Split(new[] { "Sec-WebSocket-Key: " }, StringSplitOptions.None)[1]
                                        .Split(new[] { "\r\n" }, StringSplitOptions.None)[0]
                                        .Trim();

            // Compute the accept key
            string acceptKey = ComputeWebSocketAcceptKey(webSocketKey);

            // Create the handshake response
            string response = "HTTP/1.1 101 Switching Protocols\r\n" +
                              "Upgrade: websocket\r\n" +
                              "Connection: Upgrade\r\n" +
                              $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n";

            // Send the handshake response
            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }

        private string ComputeWebSocketAcceptKey(string webSocketKey)
        {
            string concatenatedKey = webSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            byte[] concatenatedKeyBytes = Encoding.UTF8.GetBytes(concatenatedKey);
            byte[] hashBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(concatenatedKeyBytes);
            return Convert.ToBase64String(hashBytes);
        }

        private string GetMimeTypeFromFileEnding(string ending)
        {
            switch (ending)
            {
                case "html": return "text/html; charset=UTF-8";
                case "json": return "application/json; charset=UTF-8";
                case "ico": return "image/x-icon";
            }
            return "text/html";
        }
        private bool IsBinaryFile(string ending)
        {
            switch (ending)
            {
                case "html": return false;
                case "json": return false;
                case "ico": return true;
            }
            return false;
        }
        private async Task ServeHttpRequest(NetworkStream stream, string request)
        {
            try
            {
                // Extract the requested resource name from the HTTP request (for simplicity, assume it's the first line and the path is in the format "GET /resource HTTP/1.1")
                string resourceName = "index.html"; // Default to index.html
                string[] requestLines = request.Split('\n');
                if (requestLines.Length > 0)
                {
                    string[] requestParts = requestLines[0].Split(' ');
                    if (requestParts.Length > 1)
                    {
                        string requestedPath = requestParts[1].Trim('/');
                        if (!string.IsNullOrEmpty(requestedPath))
                        {
                            resourceName = requestedPath; // Use the requested resource if provided
                        }
                    }
                }
        
                // Use the Assembly to get the embedded resource stream
                var assembly = Assembly.GetExecutingAssembly();
                string resourcePath = $"WebWind.Resources.{resourceName}"; // Adjust namespace and resource folder
                using (Stream resourceStream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (resourceStream != null)
                    {
                        string fileExtension = resourcePath.Split('.').Last().ToLower();
                        string mimeType = GetMimeTypeFromFileEnding(fileExtension);
                        byte[] contentBytes;

                        // Check if the resource is a binary file (like an image) or text-based
                        if (IsBinaryFile(fileExtension))
                        {
                            // Read binary data directly from the stream
                            using (MemoryStream ms = new MemoryStream())
                            {
                                await resourceStream.CopyToAsync(ms);
                                contentBytes = ms.ToArray();
                            }
                        }
                        else
                        {
                            // Handle text-based content (HTML, CSS, JS, etc.)
                            using (StreamReader reader = new StreamReader(resourceStream))
                            {
                                string content = await reader.ReadToEndAsync();
                                contentBytes = Encoding.UTF8.GetBytes(content);
                            }
                        }

                        // Build the HTTP response header
                        string responseHeader = "HTTP/1.1 200 OK\r\n" +
                                                $"Content-Type: {mimeType}\r\n" +
                                                $"Content-Length: {contentBytes.Length}\r\n" +
                                                "Connection: close\r\n\r\n";

                        byte[] headerBytes = Encoding.UTF8.GetBytes(responseHeader);

                        // Send the header and the content
                        await stream.WriteAsync(headerBytes, 0, headerBytes.Length);
                        await stream.WriteAsync(contentBytes, 0, contentBytes.Length);
                    }
                    else
                    {
                        // Send a 404 Not Found response if the resource doesn't exist
                        string responseHeader = "HTTP/1.1 404 Not Found\r\n" +
                                                "Content-Type: text/plain\r\n" +
                                                "Connection: close\r\n\r\n" +
                                                "404 - File Not Found";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(responseHeader);
                        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error serving HTTP request: {ex.Message}");
            }
            finally
            {
                stream.Close(); // Close the connection after serving the file
            }
        }

        private async Task HandleWebSocket(WebSocket webSocket)
        {
            byte[] receiveBuffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, receiveResult.Count);
                    Log.Debug("Received message from client: " + receivedMessage);

                    if (receivedMessage.StartsWith("view"))
                    {
                        ViewCache.CreateView(webSocket, receivedMessage.Substring(5));
                    }
                    else if (receivedMessage.StartsWith("event"))
                    {
                        ProcessEvent(webSocket, receivedMessage);
                    }
                    else
                    {
                        Log.Error("Illegal Message");
                    }
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    Log.Info("WebSocket connection closed.");
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
            }
        }

        public static void SendJsToClient(WebSocket webSocket, string js)
        {
            webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(js)), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        static void ProcessEvent(WebSocket webSocket, string input)
        {
            Task.Run(() =>
            {
                // Step 1: Remove the "event" prefix and trim the string
                string trimmedInput = input.Substring(6).Trim(); // Remove "event" (5 chars + 1 space)

                // Step 2: Split the string by space but keep JSON part intact
                int firstSpaceIndex = trimmedInput.IndexOf(' ');
                int secondSpaceIndex = trimmedInput.IndexOf(' ', firstSpaceIndex + 1);

                string id = trimmedInput.Substring(0, firstSpaceIndex);
                string shortString = trimmedInput.Substring(firstSpaceIndex + 1, secondSpaceIndex - firstSpaceIndex - 1);
                string jsonPart = trimmedInput.Substring(secondSpaceIndex + 1);

                ClientInterface.s_LocalClientInterface.Value = ViewCache.Views[webSocket];
                Component component = ClientInterface.Current.m_ComponentRegistry[long.Parse(id)];
                component.m_RegisteredEvents[shortString](jsonPart);
            });
        }
    }
}
