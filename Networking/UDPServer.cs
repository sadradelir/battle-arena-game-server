using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace MobarezooServer.Networking
{
    public class UDPServer
    {
        public class MessageToProcess
        {
            public MessageType type;
            public IPEndPoint senderEndPoint;
            public byte[] data;
        }

        public Queue<MessageToProcess> recievedMessages;
        private UdpClient udpClient;
        private Thread[] workers;
        private readonly ManualResetEvent _stop, _ready;
        public event Action<MessageToProcess> ProcessRequest;

        public UDPServer(int maxThreads)
        {
            recievedMessages = new Queue<MessageToProcess>();
            udpClient = new UdpClient(5006);

            _stop = new ManualResetEvent(false);
            _ready = new ManualResetEvent(false);

            workers = new Thread[maxThreads];
            for (int i = 0; i < workers.Length; i++)
            {
                workers[i] = new Thread(processWorker);
            }
        }
    
        public void startRecieveLoop()
        {
            udpClient.BeginReceive(afterRecieve, null);
        }

        private void afterRecieve(IAsyncResult ar)
        {
            try
            {
                IPEndPoint senderEndPoint = new IPEndPoint(0, 0);
                byte[] message = udpClient.EndReceive(ar, ref senderEndPoint);
                var bytesWithoutFirst = new byte[message.Length-1];
                Array.Copy(message, 1,
                    bytesWithoutFirst, 0, 
                    message.Length - 1);
                recievedMessages.Enqueue(new MessageToProcess()
                {
                    data = bytesWithoutFirst, senderEndPoint = senderEndPoint , type = (MessageType)(message[0])
                });
                // Console.WriteLine("Got " + str + " from " + senderEndPoint);
                _ready.Set();
    
                udpClient.BeginReceive(afterRecieve, null);
            }
            catch (Exception e)
            {
                udpClient.BeginReceive(afterRecieve, null);
            }
        }
        
        public void startProcessThread()
        {
            for (int i = 0; i < workers.Length; i++)
            {
                workers[i].Start();
            }
        }
        
        void processWorker()
        {
            WaitHandle[] wait = new[] {_ready, _stop};
            while (0 == WaitHandle.WaitAny(wait))
            {
                MessageToProcess context;
                lock (recievedMessages)
                {
                    if (recievedMessages.Count > 0)
                    {
                        context = recievedMessages.Dequeue();
                    }
                    else
                    {
                        _ready.Reset();
                        continue;
                    }
                }

                try
                {
                    ProcessRequest(context);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
            }
        }

  
        public void sendMessageToEndpoint(byte[] message , IPEndPoint ipEndPoint)
        {
            udpClient.Send(message , message.Length , ipEndPoint);
        }
        
    }
}