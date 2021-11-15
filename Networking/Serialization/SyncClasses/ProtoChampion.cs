using System;
using System.IO;

namespace Serialization.SyncClasses
{
    // a brief of a champion in server ... u can send this to sync and receive 
    // a list from these to sync 
    
    public class ProtoChampion
    {
        public byte championIndex; // SHARA , DIANOUSH , ...  
        public ProtoVector position;
        public float rotation;
        public byte uId; // room based unique Id 1 or 2 ! 
        
        //______________ACTS______________//
        public bool attacking; // yes ! u can do all together ... 
        public bool moving;
        public bool reloading;
        public bool disabled;
        public bool channeling;
        
         //____EXTRA PARAMS____//
        public ProtoVector attackTarget; // if u need ...
        public byte attackParameter1; // 0 to 255 each for extra parameters
        public byte attackParameter2; // 0 to 255 each for extra parameters
        public byte attackParameter3; // 0 to 255 each for extra parameters
         
         //______________STATS_____________//
        public ushort movementSpeed; // 0 to 65536
        public ushort attackInterval;
        public ushort health;
        public ushort maxHealth;
        public ushort shield;
        
        public static int getSize()
        {
            return 31 + 8 + 1;
        }
        
        public byte[] getSerialized()
        {
            byte[] data, buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte(championIndex); // 1
                
                buffer = position.getSerialized();
                ms.Write(buffer , 0 , buffer.Length); // 9
                
                buffer = BitConverter.GetBytes(rotation);
                ms.Write(buffer , 0 , buffer.Length); // 13
                
                ms.WriteByte(uId); // 14

                //todo ... do it better 
                buffer = BitConverter.GetBytes(attacking);
                ms.Write(buffer , 0 , buffer.Length);

                buffer = BitConverter.GetBytes(moving);
                ms.Write(buffer , 0 , buffer.Length);

                buffer = BitConverter.GetBytes(reloading);
                ms.Write(buffer , 0 , buffer.Length);

                buffer = BitConverter.GetBytes(disabled);
                ms.Write(buffer , 0 , buffer.Length); // 18
                
                buffer = BitConverter.GetBytes(channeling);
                ms.Write(buffer , 0 , buffer.Length); // 19

                buffer = attackTarget.getSerialized();
                ms.Write(buffer , 0 , buffer.Length); 
                
                ms.WriteByte(attackParameter1);
                ms.WriteByte(attackParameter2);
                ms.WriteByte(attackParameter3); // 21
                
                buffer = BitConverter.GetBytes(movementSpeed);
                ms.Write(buffer , 0 , buffer.Length);
                
                buffer = BitConverter.GetBytes(attackInterval);
                ms.Write(buffer , 0 , buffer.Length);

                buffer = BitConverter.GetBytes(health);
                ms.Write(buffer , 0 , buffer.Length);
                
                buffer = BitConverter.GetBytes(maxHealth);
                ms.Write(buffer , 0 , buffer.Length);
                
                buffer = BitConverter.GetBytes(shield);
                ms.Write(buffer , 0 , buffer.Length); // 28 bytes 
                
                return ms.ToArray();
            }
        }

        public static ProtoChampion getFromBytes(byte[] data)
        {
            byte[] buffer = new byte[1024];
            var champion = new ProtoChampion();
            using (MemoryStream ms = new MemoryStream(data))
            {
                champion.championIndex = (byte) ms.ReadByte();
                
                ms.Read(buffer , 0 , 8); // 9
                champion.position = new ProtoVector(buffer);
                
                ms.Read(buffer , 0 , 4); // 13
                champion.rotation = BitConverter.ToSingle(buffer,0);

                champion.uId = (byte) ms.ReadByte();

                //todo ... do it better 
                ms.Read(buffer , 0 , 1);
                champion.attacking = BitConverter.ToBoolean(buffer , 0);

                ms.Read(buffer , 0 , 1);
                champion.moving = BitConverter.ToBoolean(buffer , 0);

                ms.Read(buffer , 0 , 1);
                champion.reloading = BitConverter.ToBoolean(buffer , 0);

                ms.Read(buffer , 0 , 1); // 18
                champion.disabled = BitConverter.ToBoolean(buffer , 0);
                
                ms.Read(buffer , 0 , 1); // 18
                champion.channeling = BitConverter.ToBoolean(buffer , 0);
                
                
                ms.Read(buffer , 0 , 8); 
                champion.attackTarget = new ProtoVector(buffer);
                
                champion.attackParameter1 = (byte)ms.ReadByte();
                champion.attackParameter2 = (byte)ms.ReadByte();
                champion.attackParameter3 = (byte)ms.ReadByte();
                
                ms.Read(buffer , 0 , 2);
                champion.movementSpeed = BitConverter.ToUInt16(buffer,0);
                
                ms.Read(buffer , 0 , 2);
                champion.attackInterval = BitConverter.ToUInt16(buffer,0);

                ms.Read(buffer , 0 , 2);
                champion.health = BitConverter.ToUInt16(buffer,0);
                
                ms.Read(buffer , 0 , 2);
                champion.maxHealth = BitConverter.ToUInt16(buffer,0);
                
                ms.Read(buffer , 0 , 2); // 28 bytes 
                champion.shield = BitConverter.ToUInt16(buffer,0);
            }
            return champion;
        }
        
    }
}