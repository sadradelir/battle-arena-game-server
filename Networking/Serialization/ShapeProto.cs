using MobarezooServer.Utilities.Geometry;

namespace MobarezooServer.Networking.Serialization
{ 
    public class ShapeProto
    {       
        public float x;       
        public float y;       
        public float w;       
        public float h;       
        public float d; // rotation angle in degrees 

        public ShapeProto()
        {
        }
        
        public ShapeProto(Utilities.Geometry.Shape shape)
        {
            x = shape.position.X;
            y = shape.position.Y;
            d = shape.rotation;
            if (shape is Circle c)
            {
                w = c.radius;
                h = c.radius;
            }else if (shape is Rectangle r)
            {
                w = r.width;
                h = r.height;
            }
        }
    }
}