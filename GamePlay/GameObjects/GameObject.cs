using System;
using System.Numerics;
using MobarezooServer.GamePlay;
using MobarezooServer.GamePlay.Champions;
using MobarezooServer.Utilities;
using MobarezooServer.Utilities.Geometry;
using Newtonsoft.Json;
using shortid;

namespace MobarezooServer.Gameplay.GameObjects
{
    public abstract class GameObject
    {
            public enum GameObjectType
            {
                SHARA_ARROW,  // 0
                SIVAN_AXE,  // 1
                SHARA_BIRD,  // 2
                ARITA_FIRE_BALL,  // 3
                ARITA_FIRE_GROUND,  // 4
                ARAKHSH_ATTACK,  // 5
                FARIVAZ_HAMMER,  // 6
                FARIVAZ_SHOCKWAVE, // 7
                MEEKO_KONAI, // 8
                VEEMAN_CLOUD, // 9
                VEEMAN_VOID, // 10
                LANGAAR_ANCHOR, // 11
                LANGAAR_SLOW, // 12
                DIANOUSH_SPEAR
                //...
            }
            // ----------- IDENTITY -------------- // 
            public uint id;
            //public Room.SummonerData owner;
            public Champion ownerChampion;
            public bool deleted;
            public GameObjectType type;
            
            // ----------- GEOMETRY -------------- //
            public Shape shape;
            public Vector2 position => shape.position;
            
            // ----------- BEHAVIOUR -------------- //
            public bool selfHit;
            public Action ownerIntersectAction;

            public bool isWaiting => waiting;
            private bool waiting;
            private int waitTime;
            private Action waitEndAction;
            
            protected GameObject()
            {
                id = IdGenerator.Generate();
            }

            public void IntersectingWithOwner()
            {
                if (!selfHit)
                { 
                    throw new InvalidOperationException();
                    return;
                }
                if (ownerIntersectAction == null)
                {
                    throw new NullReferenceException();
                }
                ownerIntersectAction.Invoke();
            }

            public void StartWaitForAction(Action waitEndAction , int waitTime)
            {
                if (waiting)
                {
                    throw new InvalidOperationException();
                }
                waiting = true;
                this.waitTime = waitTime;
                this.waitEndAction = waitEndAction;
            }
            
            
            public void ProceedWait(int deltaTime)
            {
                if (!waiting)
                {
                    throw new InvalidOperationException();
                    return;
                }
                waitTime -= deltaTime;
                if (waitTime > 0) return;
                waitTime = -1;
                waiting = false;
                if (waitEndAction == null)
                {
                    throw new NullReferenceException();
                }
                waitEndAction.Invoke();
            }

            public abstract void UpdateTransform(long deltaTime);
            public abstract void IntersectingWithObstacle(Obstacle thatObs);
            public abstract void IntersectingWithPlayer(Champion enemy);


            public short getSpeed()
            {
                if (this is Projectile p)
                {
                    return (short) p.speed;
                }
                else
                {
                    return 0;
                }
            }
            
    }
}