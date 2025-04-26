using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

//Uncomment this block to pass the first stage

TcpListener server = new TcpListener(IPAddress.Any, 4221);
var connectionisOn = true;
server.Start();
while (connectionisOn)
{
    TcpClient client = server.AcceptTcpClient();
    _ = Task.Run(() => HandleClient(client));
}
    //var socket = server.AcceptSocket(); // wait for client
    // wait for client
    Console.WriteLine("Socket Connected");

    string message = "HTTP/1.1 200 OK\r\n\r\n";

    byte[] bytes = Encoding.ASCII.GetBytes(message);

    Task HandleClient(TcpClient client) {
        NetworkStream stream = client.GetStream();
        byte[] receivedDataBuffer = new byte[256];
        int bytesRead = stream.Read(receivedDataBuffer, 0, receivedDataBuffer.Length);

        Console.WriteLine("Received the Data  : {0}", Encoding.ASCII.GetString(receivedDataBuffer, 0, bytesRead));

        var request = Encoding.ASCII.GetString(receivedDataBuffer, 0, bytesRead);
        var requestLines = request.Split("\r\n");
        var requests = requestLines[0].Split("/");
        var linesPart = requestLines[0].Split(' ');
        var (requestMethod, path, httpVersion) = (linesPart[0], linesPart[1], linesPart[2]);
        var content = linesPart[1].Contains("/echo/") ? linesPart[1].Substring(linesPart[1].LastIndexOf("/") + 1) : linesPart[1].Contains("/user-agent", StringComparison.CurrentCultureIgnoreCase)
            ? linesPart[1].Substring(linesPart[1].LastIndexOf("/") + 1) : null;
        String response;
        if (!string.IsNullOrWhiteSpace(content) && content.Equals("user-agent", StringComparison.CurrentCultureIgnoreCase))
        {

            var useragent = requestLines[2].ToLower().Remove(0, 11).Trim();
            int useragentLength = useragent.Length;
            response = path == "/" ? $"{httpVersion} 200 OK\r\n\r\n" :
                path.Contains("/user-agent", StringComparison.CurrentCultureIgnoreCase) ? $"{httpVersion} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {useragentLength}\r\n\r\n{useragent}" : $"{httpVersion} 404 Not Found\r\n\r\n";
        }
        else if (!string.IsNullOrWhiteSpace(linesPart[1]) && linesPart[1].Contains("/files", StringComparison.CurrentCultureIgnoreCase))
        {
        var currentdirectory = Environment.CurrentDirectory;
        var filePath = string.Format("tmp/{0}", linesPart[1].Substring(linesPart[1].LastIndexOf("/") + 1));
        var newfilePath = Path.Join(args[1],"foo");
        bool isFileExists = false;
        string text = string.Empty;
        if (File.Exists(newfilePath))
        {
            text = File.ReadAllText(newfilePath);
            isFileExists = true;
        }

        response = isFileExists ? $"{httpVersion} 200 OK\r\nContent-Type:application/octet-stream\r\nContent-Length: {text.Length}\r\n\r\n{text}" : $"{httpVersion} 404 Not Found\r\n\r\n";
    }
        else
        {
            response = path == "/" ? $"{httpVersion} 200 OK\r\n\r\n" :
                path.Contains("/echo/") ? $"{httpVersion} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}" : $"{httpVersion} 404 Not Found\r\n\r\n";
        }
        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
        stream.Write(responseBytes, 0, responseBytes.Length);

        stream.Close();
        client.Close();
        return Task.CompletedTask;
    }
//socket.Send(bytes);
server.Stop();
//socket.Close();

