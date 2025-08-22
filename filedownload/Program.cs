namespace FileDownload;

class Program
{
    public static async Task Main(string[] args)
    {
        var cancellationTokenSource = new CancellationTokenSource().Token;
        var peer = new Peer();
        var task = peer.Start(cancellationTokenSource);
        if (args.Length > 0 && args[0] == "download")
        {
            // Espera: args[1]=IP, args[2]=puerto, args[3]=nombre de archivo, args[4]=ruta de guardado
            await peer.DownloadFileAsync(args[1], int.Parse(args[2]), args[3], args[4], cancellationTokenSource);
        }
        else
        {
            Console.WriteLine("waiting for other peers to connect...");
        }
        await task;
    }
}