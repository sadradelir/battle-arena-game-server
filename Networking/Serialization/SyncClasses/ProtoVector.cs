using System;
using System.IO;
using System.Numerics;

namespace Serialization.SyncClasses
{
    public class ProtoVector
    {
        public float x;
        public float y;


        public Vector2 getVector()
        {
            return  new Vector2(x,y);
        }
        
        public ProtoVector()
        {
            
        }

        public ProtoVector(float x, float y)
        {
            this.x = x;
            this.y = y; // ?
        }
        
        public ProtoVector(byte[] data)
        {
            x = BitConverter.ToSingle(data , 0);
            y = BitConverter.ToSingle(data , 4);
        }

        public byte[] getSerialized()
        {
            byte[] data, buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                buffer = BitConverter.GetBytes(x);
                ms.Write(buffer, 0, buffer.Length);
                
                buffer = BitConverter.GetBytes(y);
                ms.Write(buffer, 0, buffer.Length);
                
                var serialized = ms.ToArray();
                /*string s = "";
                for (int i = 0; i < serialized.Length; i++)
                s += Convert.ToString(serialized[i], 2);
                Debug.Log(s);*/
                return serialized;
            }
        }
    }
}