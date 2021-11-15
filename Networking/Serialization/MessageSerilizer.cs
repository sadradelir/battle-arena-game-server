using System.Collections.Generic;
using System.Linq;

namespace MobarezooServer.Networking.Serialization
{
    public static class MessageSerilizer
    {
        public static byte[] SerilizeJoinRoomMessage(int uId , byte[] mapData)
        {
            IEnumerable<byte> rv = 
                new List<byte>(){(byte) MessageType.JOIN_ROOM}.Concat(new []{(byte)uId}).Concat(mapData);
            return rv.ToArray();
        } 
        public static byte[] SerilizeRoomJoinErrorMessage()
        {
            return new[] {(byte) MessageType.ROOM_JOIN_ERROR};
        } 
         
    }
}