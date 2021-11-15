using System;
using System.IO;
using System.Numerics;
using Serialization.SyncClasses;

namespace MobarezooServer.Utilities.Geometry
{ 
    public class Circle : Shape
    { 
        public float radius;

        public Circle()
        {
            
        }
        
        public Circle(ProtoVector position , float radius , float rotation = 0) :
            this(new Vector2(position.x , position.y), radius )
        {
            this.rotation = rotation;
        }
    
        public Circle(Vector2 position , float radius)
        {
            this.position = position.Clone();
            this.radius = radius;
        }
        
      
        
        public Circle(Vector2 position , float radius , float rotation)
        {
            this.rotation = rotation;
            this.position = position.Clone();
            this.radius = radius;
        }


        public override float dimention()
        {
            return radius;
        }

        // have intersecting point(s);
        public override bool IsIntersecting(Shape shape)
        {
            if (shape is Circle shapeAsCircle)
            {
                return Vector2.Distance(position, shapeAsCircle.position) <=
                       radius + shapeAsCircle.radius;
            }
            if (shape is Rectangle shapeAsRectangle)
            {
                var relX = position.X - shape.position.X;
                var relY = position.Y - shape.position.Y;
                var angle = -shape.rotation;
                var angleCos = Math.Cos(angle * (Math.PI / 180));
                var angleSin = Math.Sin(angle * (Math.PI / 180));
                var localX = angleCos * relX - angleSin * relY;
                var localY = angleSin * relX + angleCos * relY;
                var deltaX = localX - Math.Max(-shapeAsRectangle.width/2, Math.Min(localX, shapeAsRectangle.width/2));
                var deltaY = localY - Math.Max(-shapeAsRectangle.height/2, Math.Min(localY, shapeAsRectangle.height/2));

                var ret =  (deltaX * deltaX) + (deltaY * deltaY) <= (radius * radius);
             
                return ret;
            }
            throw new NotImplementedException();
        }

        public override void KnockOutFromShape(Shape shape)
        {
            
        }

        public override void Rotate(float degree)
        {
            // :(
            rotation = degree;
        }

        public override float GetBoundingRectangle()
        {
            throw new System.NotImplementedException();
        }

        public override float GetBoundingCircle()
        {
            throw new System.NotImplementedException();
        }

        public override byte[] getSerilized()
        {
            byte[] data, buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                var pos = (new ProtoVector(position.X, position.Y)).getSerialized();
                var size = (new ProtoVector(radius, radius)).getSerialized();
                byte type = 1; // circle , rect is 2
                
                
                buffer = pos;
                ms.Write(buffer , 0 , buffer.Length); 
                buffer = size;
                ms.Write(buffer , 0 , buffer.Length); 
                ms.WriteByte(type);
                buffer = BitConverter.GetBytes(rotation);
                ms.Write(buffer , 0 , buffer.Length); 
                
                return ms.ToArray();
            }
        }
    }
}