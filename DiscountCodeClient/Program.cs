using System;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main()
    {
        try
        {
            using var client = new TcpClient("127.0.0.1", 5000);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            Console.WriteLine("Connected to server.");
            Console.WriteLine("Enter command (e.g., GEN 3 or USE ABC1234):");

            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) break;

                writer.WriteLine(input);

                string? response;
                while (!string.IsNullOrEmpty(response = reader.ReadLine()))
                {
                    Console.WriteLine("SERVER: " + response);
                    if (!response.StartsWith("CODE ")) break;
                }
            }
        }
        catch (Exception ex) { 
            Console.WriteLine(ex.Message.ToString());
        }
    }
}
