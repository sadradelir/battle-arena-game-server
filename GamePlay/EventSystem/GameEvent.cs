using System;
using System.Data.SqlTypes;
using System.IO;

namespace MobarezooServer.GamePlay.EventSystem
{
    public class GameEvent
    {
        public enum GameEventType
        {
            HIT,SLOW,HEAL,ATTACK_SPEED,END_GAME,IMPACT,BUFF_DAMAGE,IGNITE
        }
        public GameEventType type;
        public uint targetObject;
        public int value;

        public byte[] getSerialized()
        {
            byte[] data, buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                buffer = BitConverter.GetBytes((ushort)type); // 65536
                ms.Write(buffer , 0 , buffer.Length); // 2 
                
                buffer = BitConverter.GetBytes(targetObject); // 65536
                ms.Write(buffer , 0 , buffer.Length); // 4 

                buffer = BitConverter.GetBytes((short)value); // +-32536
                ms.Write(buffer , 0 , buffer.Length); // 2
                
                return ms.ToArray();
            }
        }
    }
}