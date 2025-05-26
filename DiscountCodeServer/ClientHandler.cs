using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

public class ClientHandler
{
    private readonly TcpClient _client;
    private readonly DiscountCodeService _service;

    public ClientHandler(TcpClient client, DiscountCodeService service)
    {
        _client = client;
        _service = service;
    }

    public void Handle()
    {
        using var stream = _client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                writer.WriteLine("ERR Invalid command");
                continue;
            }

            string cmd = parts[0];
            string arg = parts[1];

            try
            {
                if (cmd == Protocol.GenerateCommand)
                {
                    int count = int.Parse(arg);
                    var codes = _service.GenerateCodes(count);
                    foreach (var code in codes)
                        writer.WriteLine($"CODE {code}");
                }
                else if (cmd == Protocol.UseCommand)
                {
                    bool success = _service.UseCode(arg);
                    writer.WriteLine(success ? "OK" : "INVALID");
                }
                else
                {
                    writer.WriteLine("ERR Unknown command");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"ERR {ex.Message}");
            }
        }
    }
}
