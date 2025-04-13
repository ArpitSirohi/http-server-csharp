using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

//Uncomment this block to pass the first stage

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var client =server.AcceptTcpClient();
//var socket = server.AcceptSocket(); // wait for client
// wait for client
Console.WriteLine("Socket Connected");

string message = "HTTP/1.1 200 OK\r\n\r\n";

byte[] bytes = Encoding.ASCII.GetBytes(message);

NetworkStream stream = client.GetStream();
byte[] receivedDataBuffer = new byte[256];
int bytesRead = stream.Read(receivedDataBuffer, 0, receivedDataBuffer.Length);

Console.WriteLine("Received the Data  : {0}", Encoding.ASCII.GetString(receivedDataBuffer, 0,bytesRead));
var request = Encoding.ASCII.GetString(receivedDataBuffer, 0, bytesRead);
var requestLines = request.Split("\r\n");
var requests = requestLines[0].Split("/");
var linesPart = requestLines[0].Split(' ');
var (requestMethod, path, httpVersion) = (linesPart[0], linesPart[1], linesPart[2]);
var content= linesPart[1].Contains("/echo/") ? linesPart[1].Substring(linesPart[1].LastIndexOf("/")+1) : linesPart[1].Contains("/user-agent")
    ? linesPart[1].Substring(linesPart[1].LastIndexOf("/") + 1) :null;
String response;
if (content.Equals("user-agent", StringComparison.CurrentCultureIgnoreCase))
{
    var useragentIndex = requestLines[3].ToLower().IndexOf("user-agent");
    var useragent = requestLines[3].ToLower().Remove(useragentIndex, 11).Trim();
    var useragentLength = useragent.Length;
    response = path == "/" ? $"{httpVersion} 200 OK\r\n\r\n" :
        path.Contains("/user-agent") ? $"{httpVersion} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {useragentLength}\r\n\r\n{useragent}" : $"{httpVersion} 404 Not Found\r\n\r\n";
}
else
{
    response = path == "/" ? $"{httpVersion} 200 OK\r\n\r\n" :
        path.Contains("/echo/") ? $"{httpVersion} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}" : $"{httpVersion} 404 Not Found\r\n\r\n";
}
byte[] responseBytes = Encoding.ASCII.GetBytes(response);
stream.Write(responseBytes, 0, responseBytes.Length);

client.Close();
stream.Close();
//socket.Send(bytes);
server.Stop();
//socket.Close();

