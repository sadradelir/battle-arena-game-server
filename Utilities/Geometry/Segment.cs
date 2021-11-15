using System;
using System.Numerics;
using MobarezooServer.AI;

namespace MobarezooServer.Utilities.Geometry
{
    public class Segment
    {
        public Vector2 a;
        public Vector2 b;

        public Segment(Vector2 a, Vector2 b)
        {
            this.a = a;
            this.b = b;
        }

        /*public bool isIntersecting(Segment other)
        {
            var r = b - a; // myVector
            var s = other.b - other.a; // other Vector
            var d = r.X * s.Y - r.Y * s.X; // cross two Vectors
            var u = (other.a.X - a.X) * r.Y - (other.a.Y - a.Y) * r.X / d; // myScaler ;
            var t = (other.a.X - a.X) * s.Y - (other.a.Y - a.Y) * s.X / d; // otherScaler ;
            if (a == other.a || a == other.b || b == other.a || b == other.b)
            {
                return true;
            }
            return (0 <= u && u <= 1) && (0 <= t && t <= 1);
        }*/
        public bool isIntersecting(Rectangle other)
        {
            if (isIntersecting(new Segment(other.corner[0] ,other.corner[2])) ||
                isIntersecting(new Segment(other.corner[1] ,other.corner[3]))
                )
            {
                return true;
            }
            return false;
        }
         
        
        static Boolean onSegment(Vector2 p, Vector2 q, Vector2 r) 
        { 
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) && 
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y)) 
                return true; 
  
            return false; 
        } 
  

        static int orientation(Vector2 p, Vector2 q, Vector2 r) 
        { 
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/ 
            // for details of below formula. 
            float val = (q.Y - p.Y) * (r.X - q.X) - 
                      (q.X - p.X) * (r.Y - q.Y); 
  
            if (Math.Abs(val) < 0.0000001f) return 0; // colinear 
  
            return (val > 0)? 1: 2; // clock or counterclock wise 
        } 
  

        public Boolean isIntersecting(Segment other)
        {
            Vector2 p1 = a;
            Vector2 q1 = b;
            Vector2 p2 = other.a;
            Vector2 q2 = other.b; 
            
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = orientation(p1, q1, p2); 
            int o2 = orientation(p1, q1, q2); 
            int o3 = orientation(p2, q2, p1); 
            int o4 = orientation(p2, q2, q1); 
  
            // General case 
            if (o1 != o2 && o3 != o4) 
                return true; 
  
            // Special Cases 
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1 
            if (o1 == 0 && onSegment(p1, p2, q1)) return true; 
  
            // p1, q1 and q2 are colinear and q2 lies on segment p1q1 
            if (o2 == 0 && onSegment(p1, q2, q1)) return true; 
  
            // p2, q2 and p1 are colinear and p1 lies on segment p2q2 
            if (o3 == 0 && onSegment(p2, p1, q2)) return true; 
  
            // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
            if (o4 == 0 && onSegment(p2, q1, q2)) return true; 
  
            return false; // Doesn't fall in any of the above cases 
        } 

        
        
        public Vector2 getProjectedPointOnLine(Vector2 p)
        {
            // get dot product of e1, e2
            Vector2 e1 = b-a;
            Vector2 e2 = p-a;
            double valDp = Vector2.Dot(e1, e2);
            // get length of vectors
            double lenLineE1 = e1.Length();
            double lenLineE2 = e2.Length();
            double cos = valDp / (lenLineE1 * lenLineE2);
            // length of v1P'
            double projLenOfLine = cos * lenLineE2;
            Vector2 p2 = new Vector2((float)(a.X + (projLenOfLine * e1.X) / lenLineE1),
                (float)(a.Y + (projLenOfLine * e1.Y) / lenLineE1));
            return p2;
        }
         
    }
}