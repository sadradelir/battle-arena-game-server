using System.Numerics;
using Serialization.SyncClasses;

namespace MobarezooServer.Networking.Serialization.SyncClasses
{
    public static class ProtoConvertors
    {
        public static ProtoVector ToProto(this Vector2 val)
        {
            return new ProtoVector()
            {
                x = val.X,
                y = val.Y,
            };
        }
    }
}