using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MobarezooServer.AI;
using MobarezooServer.GamePlay;
using MobarezooServer.Networking;
using MobarezooServer.Networking.Serialization;
using MobarezooServer.Utilities.Geometry;
using Newtonsoft.Json;
using UdpServer_001.HttpServer;

namespace MobarezooServer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("__________________________");
            Console.WriteLine("======  MOBAREZOO  =======");
            Console.WriteLine("**************************");
  
            var gm = new GameManager();
            gm.start();
            Console.WriteLine("Game Manager Started ...OK!");
            StartHttpServer(gm);
            Console.WriteLine("HTTP Server Started ...OK!");
            
        //  var aiRoom = new AISystem.ArtificalRoom();
        //  Application.Run(new AIForm(aiRoom));
   
          Application.Run(new MainForm(gm));
             
            Console.ReadKey();
        }
        
        
        // THIS IS FOR MATCH MAKE REQUEST ... 
        private static void StartHttpServer(GameManager gm)
        {
            var httpServer = new HttpAsyncServer(10);
            httpServer.ProcessRequest += context =>
            {
                var stringmsg = "";
                using (StreamReader sr = new StreamReader(context.Request.InputStream))
                {
                    stringmsg = sr.ReadToEnd();
                }
                //Console.WriteLine(stringmsg);
                var msg = JsonConvert.DeserializeObject<Battle>(stringmsg);
                var stringResp = "{}";
                if (true) // message type check ... 
                {
                    Console.WriteLine(stringmsg);
                    Console.WriteLine("New room requested for these players");
                    Console.WriteLine(msg.Player1Details.PlayerRef  + " <<VS>> " + msg.Player2Details.PlayerRef);
                    // alright // 
                    Room r = new Room(gm.balanceData ,
                        msg.Player1Details,
                        msg.Player2Details,msg,msg.Map);
                    gm.deviceIdsToRooms.AddOrUpdate(msg.Player1Details.PlayerRef, r, (s, room) => r);
                    gm.deviceIdsToRooms.AddOrUpdate(msg.Player1Details.PlayerRef, r, (s, room) => r);
                    var resp = new
                    {
                        Status = 0,
                        Data = r.id,
                    };
                    gm.rooms.Add(r);
                    Console.WriteLine("... ok! : new RoomId:" + r.id);
                    stringResp = JsonConvert.SerializeObject(resp);
                }
                 
                var responseOutputStream = context.Response.OutputStream;
                var bytesToSend = Encoding.UTF8.GetBytes(stringResp);
                responseOutputStream.Write(
                    bytesToSend, 0, bytesToSend.Length);
                responseOutputStream.Close();
            };
            httpServer.Start(5005);
        }


        private static void test()
        { 
             var gShape = new Circle( new Vector2(10,12), 15 );
             gShape.rotation = 12;
          
             Stopwatch s = new Stopwatch();
             s.Start();
           
             s.Stop();
             Console.WriteLine(s.ElapsedMilliseconds);
             
        }
    }
}