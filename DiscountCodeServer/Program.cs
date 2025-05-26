using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class Program
{
    static void Main()
    {
        var listener = new TcpListener(IPAddress.Any, 5000);
        var storage = new CodeStorage();
        var service = new DiscountCodeService(storage);
        
        listener.Start();
        Console.WriteLine("Server listening on port 5000...");


        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected.");

            Thread thread = new(() =>
            {
                var handler = new ClientHandler(client, service);
                handler.Handle();
            });

            thread.Start(); // Start client handler on a new thread
        }

       
    }
}
