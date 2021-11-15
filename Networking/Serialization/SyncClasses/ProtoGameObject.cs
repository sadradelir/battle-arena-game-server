
using System;
using System.IO;

namespace Serialization.SyncClasses
{
    public class ProtoGameObject
    {
       public ProtoVector position;
       public float dimension; // not necessary 
       public float rotation;
       public uint uId; // :)
       public ushort gameObjectType; // i discuss it later ...
       public short speed;
       public bool homing;
       public bool orbital;
       public ProtoVector target;
       public byte ownerOrder;
       
       public static int getSize()
       {
           return 8 + 4 + 4 + 4 + 2 + 2 + 1 + 8 + 1 + 1;
       }
       
       public byte[] getSerialized()
       {
           byte[] data, buffer;
           using (MemoryStream ms = new MemoryStream())
           {
               
               buffer = position.getSerialized();
               ms.Write(buffer , 0 , buffer.Length); // 9
                
               buffer = BitConverter.GetBytes(dimension);
               ms.Write(buffer , 0 , buffer.Length); // 13

               buffer = BitConverter.GetBytes(rotation);
               ms.Write(buffer , 0 , buffer.Length); // 13

               buffer = BitConverter.GetBytes(uId);
               ms.Write(buffer , 0 , buffer.Length); // 17

               buffer = BitConverter.GetBytes(gameObjectType);
               ms.Write(buffer , 0 , buffer.Length); 
               
               buffer = BitConverter.GetBytes(speed);
               ms.Write(buffer , 0 , buffer.Length);
               
               buffer = BitConverter.GetBytes(homing);
               ms.Write(buffer , 0 , buffer.Length);
               
               buffer = BitConverter.GetBytes(orbital);
               ms.Write(buffer , 0 , buffer.Length);
               
               buffer = target.getSerialized();
               ms.Write(buffer , 0 , buffer.Length); 
               
               ms.WriteByte(ownerOrder);
               
               return ms.ToArray();
           }
       }
        
    }
}