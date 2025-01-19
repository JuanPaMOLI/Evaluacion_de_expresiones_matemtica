using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// IMPORTANTE PARA INICIAR
// Abrir la console y poner: 
//cd ServidorTCP (darle enter)
//dotnet run (darle enter)
//En una consola aparte poner lo mismo pero para el ClientTCP

class TcpServer
{
    //Puerto por default
    private const int Port = 5000;

    //Iniciar el servidor y realizar conexiones
    public static async Task StartServer()
    {
        TcpListener server = new TcpListener(IPAddress.Loopback, Port);

        try
        {
            server.Start();
            Console.WriteLine($"Servidor TCP iniciado en {IPAddress.Loopback}:{Port}");

            while (true)
            {
                Console.WriteLine("Esperando conexiones...");
                TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine("Cliente conectado!");

                _ = Task.Run(() => HandleClient(client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            server.Stop();
            Console.WriteLine("Servidor detenido.");
        }
    }

//Se encarga de manejar las conexiones con el cliente
private static async Task HandleClient(TcpClient client)
{
    using (NetworkStream stream = client.GetStream())
    {
        string ClienteID = Guid.NewGuid().ToString();
        Console.WriteLine($"Cliente conectado: {ClienteID}");

        byte[] buffer = new byte[1024];
        string mode = "aritmético";

        try
        {
            while (true)
            {
                //Envio de datos
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"[{ClienteID}] Mensaje recibido: {message}");

                //El cliente sale del servidor
                if (message.Equals("salir", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Cliente desconectado.");
                    break;
                }

                string response;

                // Configurar modo
                if (message == "1")
                {
                    mode = "lógico";
                    response = "Modo lógico configurado.";
                    Console.WriteLine("Hacer uso de la notacio posfija (1 | 1) -> (1 1 |)");
                }
                else if (message == "2")
                {
                    mode = "aritmético";
                    response = "Modo aritmético configurado.";
                    Console.WriteLine("Hacer uso de la notacio posfija (10 + 10) -> (10 10 +)");
                }
                else
                {
                    // Procesar expresión según el modo
                    try
                    {
                        bool isLogicalMode = mode == "lógico";
                        ExpressionTree tree = new ExpressionTree(isLogicalMode);
                        tree.BuildTree(message);
                        response = $"Resultado: {tree.Evaluate()}";
                    }
                    catch (Exception ex)
                    {
                        response = $"Error: {ex.Message}";
                    }
                }

                byte[] responseData = Encoding.UTF8.GetBytes(response + "\n");
                await stream.WriteAsync(responseData, 0, responseData.Length);
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error manejando cliente: {ex.Message}");
        }
    }
}

    public static async Task Main(string[] args)
    {
        await StartServer();
    }
}