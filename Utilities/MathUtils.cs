using System;
using System.Numerics;
using System.Security.Claims;
using MobarezooServer.GamePlay;
using MobarezooServer.Gameplay.GameObjects;

namespace MobarezooServer.Utilities
{
    public static class MathUtils
    {
        public class RefInteger
        {
            public int value;
        }
        
        public static Vector2 Clone(this Vector2 val)
        {
            return new Vector2(val.X , val.Y);
        } 
        
        public static void clampTo(this ref int val, int min, int max)
        {
            val = val > max ? max : val < min ? min : val;
        }

        public static void clampTo(this ref float val, float min, float max)
        {
            val = val > max ? max : val < min ? min : val;
        }

        public static float returnClamp(this float val, float min, float max)
        {
            return  val > max ? max : val < min ? min : val;
        }

        public static Vector2 Rotate(this ref Vector2 v, float degrees)
        {
            double sin = Math.Sin(degrees * (Math.PI / 180));
            double cos = Math.Cos(degrees * (Math.PI / 180));
            float tx = v.X;
            float ty = v.Y;
            v.X = (float) ((cos * tx) - (sin * ty));
            v.Y = (float) ((sin * tx) + (cos * ty));
            return v;
        }
        
        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
        {
            Vector2 a = target - current;
            float magnitude = a.Length();
            if (magnitude <= maxDistanceDelta || magnitude == 0f)
            {
                return target;
            }
            return current + a / magnitude * maxDistanceDelta;
        }

        public static Vector2 FindMirrorPoint(Vector2 self, Vector2 pivot, float weightOrDistance, bool weighted = false, bool clamped = true , float xMargin = 0 , float yMargin = 0 )
        {
            var deltaVector = pivot - self;
            var norm = Vector2.Normalize(deltaVector);
            norm *= weightOrDistance;
            var tx = pivot.X + norm.X;
            var ty = pivot.Y + norm.Y;
            
            if (clamped)
            {
                // todo read from balance data ... maybe map gets bigger in future 
                tx.clampTo(-600 + xMargin, 600 - xMargin);
                ty.clampTo(-1000 + yMargin, 1000 - yMargin);
            }
            return new Vector2()
            {
                X = tx, 
                Y = ty
            };
        }
        
        public static Vector2 FindTranslationPoint(float degree , Vector2 target , float weightOrDistance , bool weighted = false , bool clamped = true )
        {
            var one = Vector2.UnitX;
            var norm =  one.Rotate(degree);
            norm *= weightOrDistance;
            
            var tx = target.X + norm.X;
            var ty = target.Y + norm.Y;
            
            if (clamped)
            {
                tx.clampTo(-600 , 600);
                ty.clampTo(-1000 , 1000);
            }
            return new Vector2()
            {
                X = tx, 
                Y = ty
            };
        }
    }
}