using System;
using System.IO;
using System.Numerics;
using Serialization.SyncClasses;

namespace MobarezooServer.Utilities.Geometry
{
    public class Rectangle : Shape
    {
        public float width;
        public float height;
        /** Corners of the box, where 0 is the lower left. */
        public Vector2[] corner = new Vector2[4];
    

        public override string ToString()
        {
                var str = position + "{" + width + "," + height + "," + rotation + "}"  ;
                foreach (var point in corner)
                {
                    str += point + "|";
                }
                return str;
        }

        public Rectangle(ProtoVector position, float w, float h, float rotation) :
            this(new Vector2(position.x, position.y), w, h, rotation)
        {
            // :)
        }
        
        public Rectangle(Vector2 position , float w , float h , float rotation)
        {
            this.position = position.Clone(); // u cant make this rect move from outside!
            this.rotation = rotation;
            height = h;
            width = w;
            var cos = (float) Math.Cos(rotation * (Math.PI / 180));
            var sin = (float) Math.Sin(rotation * (Math.PI / 180));
            Vector2 X = new Vector2(cos, sin);
            Vector2 Y = new Vector2(-sin, cos);
            X *= w / 2;
            Y *= h / 2;
           corner[0] = position - X - Y;
           corner[1] = position + X - Y;
           corner[2] = position + X + Y; 
           corner[3] = position - X + Y;
         
        }

        public override bool IsIntersecting(Shape shape)
        {
            if (shape is Circle shapeAsCircle)
            {
                return shapeAsCircle.IsIntersecting(this);
            }
            if (shape is Rectangle shapeAsRectangle)
            {
                return IsPolygonsIntersecting(shapeAsRectangle);
                // return OverLaps(shapeAsRectangle);
            }
            throw new NotImplementedException();
        }

        public override void KnockOutFromShape(Shape shape)
        {
            if (!IsIntersecting(shape))
            {
                return;
            }
        }

        public override void Rotate(float degree)
        {
            this.rotation = degree;
            var cos = (float) Math.Cos(rotation * (Math.PI / 180));
            var sin = (float) Math.Sin(rotation * (Math.PI / 180));
            Vector2 X = new Vector2(cos, sin);
            Vector2 Y = new Vector2(-sin, cos);
            X *= width / 2;
            Y *= height / 2;
            corner[0] = position - X - Y;
            corner[1] = position + X - Y;
            corner[2] = position + X + Y; 
            corner[3] = position - X + Y;
        }

        public override float dimention()
        {
            return width;
        }

        public override void moveTo(Vector2 newPosition)
        {
            var vect = newPosition - position;
            corner[0] += vect;
            corner[1] += vect;
            corner[2] += vect;
            corner[3] += vect;
            base.moveTo(newPosition);
           
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
                var size = (new ProtoVector(width, height)).getSerialized();
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
        
        bool IsPolygonsIntersecting(Rectangle b)
        {
            var a = this;
            foreach (var polygon in new[] { a, b })
            {
                for (int i1 = 0; i1 < polygon.corner.Length; i1++)
                {
                    int i2 = (i1 + 1) % polygon.corner.Length;
                    var p1 = polygon.corner[i1];
                    var p2 = polygon.corner[i2];
 
                    var normal = new Vector2(p2.Y - p1.Y, p1.X - p2.X);
 
                    double? minA = null, maxA = null;
                    foreach (var p in a.corner)
                    {
                        var projected = normal.X * p.X + normal.Y * p.Y;
                        if (minA == null || projected < minA)
                            minA = projected;
                        if (maxA == null || projected > maxA)
                            maxA = projected;
                    }
                    double? minB = null, maxB = null;
                    foreach (var p in b.corner)
                    {
                        var projected = normal.X * p.X + normal.Y * p.Y;
                        if (minB == null || projected < minB)
                            minB = projected;
                        if (maxB == null || projected > maxB)
                            maxB = projected;
                    }
                    if (maxA < minB || maxB < minA)
                        return false;
                }
            }
            return true;
        }
  
    }
}