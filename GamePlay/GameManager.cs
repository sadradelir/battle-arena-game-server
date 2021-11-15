using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using MobarezooServer.AI;
using MobarezooServer.BalanceData;
using MobarezooServer.Networking;
using MobarezooServer.Networking.Serialization;
using Newtonsoft.Json;

namespace MobarezooServer.GamePlay
{
    public class GameManager
    {
        private UDPServer udpServer;
        public Dictionary<IPEndPoint , Room> playerRooms;
        public List<Room> rooms;
        public GameBalance balanceData;
        public ConcurrentDictionary<string, Room> deviceIdsToRooms;
        
        public void start()
        {
            deviceIdsToRooms = new ConcurrentDictionary<string, Room>();
            playerRooms = new Dictionary<IPEndPoint, Room>();
            balanceData = new GameBalance();
            udpServer = new UDPServer(5);
            rooms = new List<Room>();
            udpServer.ProcessRequest += messageToProcess =>
            {
                if (messageToProcess.type == MessageType.ENTER_ROOM_REQUEST)
                {
                    Console.Write("a player wants to enter his room ... :" );
                    // i shall find his room ! so where is the unique id ??
                    var deviceId = Encoding.UTF8.GetString(messageToProcess.data);
                    AddPlayerToRoom(deviceId, messageToProcess);
                    Console.WriteLine("ok");
                }
                else if (messageToProcess.type == MessageType.GAME_ACT)
                {
                    // inshala u have room ha ? if not 
                    // find the room 
                    var room = playerRooms[messageToProcess.senderEndPoint];
                    room.enqueueActToProcess(messageToProcess.senderEndPoint , messageToProcess.data);
                }
            };
            udpServer.startRecieveLoop();
            udpServer.startProcessThread();
            Thread tickThread = new Thread(startTicking);
            tickThread.Start();
        }
        
        public void startTicking()
        {    
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                if (stopwatch.Elapsed.Milliseconds > 1000 / 16)
                {
                    // Console.WriteLine(stopwatch.Elapsed.Milliseconds);
                    updateTick((int) stopwatch.ElapsedMilliseconds);    
                    stopwatch.Restart();
                }    
                Thread.Sleep(1);
            }
        }
        
        public void updateTick(int deltaTime)
        {
            /*if (deltaTime > 35)
            {
                Console.WriteLine("Tick behind:" + (deltaTime - 33) + " ms");
            }*/
            
            foreach (Room room in rooms)
            {
                if (!room.gameEnd)
                {
                    AISystem.enqueueInputs(room , deltaTime);
                    room.processInputs(deltaTime);
                    room.UpdateObjects(deltaTime);
                    room.UpdateTimers(deltaTime);
                    room.UpdatePlayers(deltaTime);
                }
                else
                {
                    
                }
                room.SyncDataToRoomPlayers(udpServer);
            }
            rooms = rooms.Where(t => (!t.gameEnd) || t.afterEndTimer < 5000).ToList();
        }
         
        private void AddPlayerToRoom(string deviceId, UDPServer.MessageToProcess s)
        {
            Console.WriteLine("Adding Player to room");
            if (deviceIdsToRooms.ContainsKey(deviceId))
            {
                // ok find the room ... 
                // lets register the endpoint there
                Console.WriteLine("player room found");
                var room = deviceIdsToRooms[deviceId];
                var order = room.bindPlayerToHisChampion(deviceId, s.senderEndPoint);
                Console.WriteLine("bound and gets order // " + order);
                if (order > 0)
                {                    
                    udpServer.sendMessageToEndpoint( MessageSerilizer.SerilizeJoinRoomMessage(order , room.mapData)
                        , s.senderEndPoint);
                    if (playerRooms.ContainsKey(s.senderEndPoint))
                    {
                        playerRooms[s.senderEndPoint] = room;
                    }
                    else
                    {
                        playerRooms.Add(s.senderEndPoint , room);
                    }
                }
                else
                {
                    //ERROR
                    //inGamePlayersEndPoints.Add(deviceId, s.senderEndPoint);
                    udpServer.sendMessageToEndpoint( MessageSerilizer.SerilizeRoomJoinErrorMessage() 
                        , s.senderEndPoint);
                    return ;
                }
            }
            else
            {
                //ERROR
                //inGamePlayersEndPoints.Add(deviceId, s.senderEndPoint);
                udpServer.sendMessageToEndpoint( MessageSerilizer.SerilizeRoomJoinErrorMessage() 
                    , s.senderEndPoint);
                return ;
            }
        }
    }
}