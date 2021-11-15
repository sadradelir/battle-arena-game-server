using System.Numerics;
using Serialization;
using Serialization.SyncClasses;

namespace MobarezooServer.Utilities.Geometry
{
    public abstract class Shape
    {
        public Vector2 position;
        public float rotation;
        public abstract bool IsIntersecting(Shape shape);
        public virtual void moveTo(Vector2 newPosition){
            position = newPosition;
        }
        public virtual void moveTo(ProtoVector newPosition){
            position.X = newPosition.x;
            position.Y = newPosition.y;
        }
        public abstract float dimention();
        public abstract void KnockOutFromShape(Shape shape);
        public abstract void Rotate(float degree);
        public abstract float GetBoundingRectangle();
        public abstract float GetBoundingCircle();
        public abstract byte[] getSerilized();
        public string Serialize()
        {
            if (this is Circle circle)
            {
                return "";
            }
            return "";
        }
    }
}