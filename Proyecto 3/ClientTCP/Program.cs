using System;
using System.Net.Sockets;
using System.Text;

class ClienteTCP
{
    private const int Port = 5000;

    public static void Main(string[] args)
    {
        try
        {
            using (TcpClient client = new TcpClient("127.0.0.1", Port))
            using (NetworkStream stream = client.GetStream())
            {
                Console.WriteLine("Conectado al servidor.");
                Console.WriteLine("Elige el modo:");
                Console.WriteLine("1 - Modo Lógico");
                Console.WriteLine("2 - Modo Aritmético");

                while (true)
                {
                    string input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input)) continue;

                    // Enviar mensaje al servidor
                    byte[] data = Encoding.UTF8.GetBytes(input + "\n");
                    stream.Write(data, 0, data.Length);

                    if (input.Trim().Equals("salir", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Saliendo del servidor...");
                        break;
                    }

                    // Leer respuesta del servidor
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine($"Respuesta del servidor: {response}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}