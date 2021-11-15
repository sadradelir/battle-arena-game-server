 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Windows.Forms.VisualStyles;
using MobarezooServer.GamePlay.EventSystem;

namespace Serialization.SyncClasses
{
    //------- TESTAMENT -------//
    // hello my dear descendants
    // this is your lovely grandpa! and this class is room snapshot
    // we are going to sync server state with players in each frame
    // i am not pretty sure at the moment should i sync all of the room 
    // every frame or just changed objects, but i code the class in a way 
    // that u can do both , just don't mention unchanged objects!
    //------------------------//
     
    public class ProtoRoomSnapshot
    { 
       public List<ProtoChampion> champions;
       public ProtoGameObject[] gameObjects;
       public List<GameEvent> gameEvents; // cuz of simplicity game events has not proto version it is proto already // 
       public int frameNumber; 
       
       public byte[] getSerialized()
       {
           using (var ms = new MemoryStream())
           {
               ms.WriteByte((byte) champions.Count);
               foreach (var t in champions)
               {
                   var buffer = t.getSerialized();
                   ms.Write(buffer,0,buffer.Length);
               }
               ms.WriteByte((byte) gameObjects.Length);
               foreach (var t in gameObjects)
               {
                   var buffer = t.getSerialized();
                   ms.Write(buffer,0,buffer.Length);
               }
               ms.WriteByte((byte) gameEvents.Count);
               foreach (var t in gameEvents)
               {
                   var buffer = t.getSerialized();
                   ms.Write(buffer,0,buffer.Length);
               }
               
               {
                   var buffer = BitConverter.GetBytes(frameNumber);
                   ms.Write(buffer , 0 , buffer.Length); 
               }
               
               //Console.WriteLine("number of inserted events in ss" + gameEvents.Count);
               return ms.ToArray();
           }
       }
    }
}