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
                await SendMessage("Hola :D este es mi primer mensaje");
                await receiveTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to peer: {ex.Message}");
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
                await SendMessage("Hola :D este es mi primer mensaje");
                await receiveTask;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Connection closed :( " + ex.Message);
            }
        }

        public async Task ReceiveMessage()
        {
            try
            {
                var stream = _tcpClient?.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);
                var message = await reader.ReadLineAsync();
                Console.WriteLine($"Peer message: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving message: {ex.Message}");
            }
            finally
            {
                Close();
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
            finally
            {
                Close();
            }
        }

        private void Close()
        {
            _tcpClient?.Close();
            _tcplistener.Stop();
        }
    }
}