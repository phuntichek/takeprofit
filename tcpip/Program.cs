using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace tcpip
{
    class Program
    {
        public static void FindMedian(ConcurrentQueue<int> numsCollected)
        {
            List<int> n = new List<int>();
            n = numsCollected.ToList();
            n.Sort();
            foreach (int i in n)
                Console.WriteLine(i);
            float med = (n[1008] + n[1009]) / 2;
            Console.WriteLine("mediana = {0}", med);

        }

        static void Main(string[] args)
        {
            ConcurrentQueue<int> numsToSend = new ConcurrentQueue<int>();
            ConcurrentQueue<int> numsCollected = new ConcurrentQueue<int>();
            GetQueue getQueue = new GetQueue(numsToSend, numsCollected);

            for (int i = 0; i < 2018; i++)
                numsToSend.Enqueue(i);

            while (numsToSend.Count != 0)
            {
                Thread t = new Thread(new ThreadStart(getQueue.GetQueueRandom));
                t.Start();
                t.Join();
            }

            foreach (var n in numsToSend)
                Console.WriteLine(n);
            Console.WriteLine("new queue");
            FindMedian(numsCollected);
        }


        public class GetQueue
        {
            ConcurrentQueue<int> numsToSend;
            ConcurrentQueue<int> numsCollected;

            public GetQueue(ConcurrentQueue<int> _numsToSend, ConcurrentQueue<int> _numsCollected)
            {
                this.numsToSend = _numsToSend;
                this.numsCollected = _numsCollected;
            }

            static int GetInt()
            {
                var random = new Random();
                int i = random.Next(0, 10000000);
                return i;
            }

            public void GetQueueRandom()
            {
                int x = GetInt();
                if (x < 1000000)
                {
                    numsToSend.TryDequeue(out var z);
                    numsCollected.Enqueue(x);
                    return;
                }
                else
                    GetQueueRandom();
            }

            public void GetQueueServ()
            {
                try
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    Int32 port = 2013;
                    String server = "88.212.241.11";
                    TcpClient client = new TcpClient(server, port);

                    numsToSend.TryDequeue(out var sent);
                    Byte[] data = Encoding.ASCII.GetBytes(sent.ToString());


                    NetworkStream stream = client.GetStream();

                    stream.Write(data, 0, data.Length);

                    Console.WriteLine("Sent: {0}", sent);

                    data = new Byte[100];

                    Int32 bytes = stream.Read(data, 0, data.Length);

                    String response = bytes.ToString();
                    String responseNumber = String.Empty;
                    for (int i = 0; i <= response.Length; i++)
                    {
                        if (char.IsDigit(response[i]))
                            responseNumber += response[i];
                    }
                    if (responseNumber == String.Empty)
                        GetQueueServ();

                    numsCollected.Enqueue(Convert.ToInt32(responseNumber));

                    stream.Close();
                    client.Close();
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("ArgumentNullException: {0}", e);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }

                Console.WriteLine("\n Press Enter to continue...");
                Console.Read();
            }
        }
    }
}
