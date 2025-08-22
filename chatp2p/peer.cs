using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatP2P
{
    public class Peer
    {
        private readonly TcpListener _tcplistener;
        private TcpClient? _tcpClient;
        private const int Port = 8080;

        public Peer() => _tcplistener = new TcpListener(IPAddress.Any, Port);

        public async Task ConnectToPeer(string ipAddress, string port)
        {
            try
            {
                _tcpClient = new TcpClient(ipAddress, Convert.ToInt32(port));
                Console.WriteLine($"Connected to peer at {ipAddress}:{port}");

                var receiveTask = ReceiveMessage();

                Console.WriteLine("Type your message and press Enter to send. Type '/exit' to close.");
                while (true)
                {
                    var messageToSend = Console.ReadLine();
                    if (string.IsNullOrEmpty(messageToSend) || messageToSend.ToLower() == "/exit")
                    {
                        break; 
                    }
                    await SendMessage(messageToSend);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to peer: {ex.Message}");
            }
            finally
            {
                Close(); 
            }
        }

        public async Task StartListening()
        {
            try
            {
                _tcplistener.Start();
                Console.WriteLine("Listening for incoming connections...");

                _tcpClient = await _tcplistener.AcceptTcpClientAsync();
                Console.WriteLine("Connection established with a client.");

                var receiveTask = ReceiveMessage();
                
                Console.WriteLine("Type your message and press Enter to send. Type '/exit' to close.");
                while (true)
                {
                    var messageToSend = Console.ReadLine();
                    if (string.IsNullOrEmpty(messageToSend) || messageToSend.ToLower() == "/exit")
                    {
                        break; 
                    }
                    await SendMessage(messageToSend);
                }

            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Connection closed :( " + ex.Message);
            }
            finally
            {
                Close();
            }
        }

        public async Task ReceiveMessage()
        {
            try
            {
                var stream = _tcpClient?.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);

                while (_tcpClient is { Connected: true })
                {
                    var message = await reader.ReadLineAsync();
                    if (message == null)
                    {
                        Console.WriteLine("Peer has disconnected.");
                        break; 
                    }
                    Console.WriteLine($"Peer message: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection lost while receiving messages.");
            }
        }

        public async Task SendMessage(string message)
        {
            try
            {
                var stream = _tcpClient?.GetStream();
                var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                await writer.WriteLineAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        private void Close()
        {
            _tcpClient?.Close();
            _tcplistener.Stop();
        }
    }
}