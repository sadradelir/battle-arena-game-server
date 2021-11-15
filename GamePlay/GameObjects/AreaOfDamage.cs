using System;
using System.Diagnostics;
using MobarezooServer.GamePlay;
using MobarezooServer.GamePlay.Champions;
using MobarezooServer.GamePlay.EventSystem;
using MobarezooServer.Utilities.Geometry;
using Newtonsoft.Json;

namespace MobarezooServer.Gameplay.GameObjects
{
    public class AreaOfDamage : GameObject
    {
        public int maxDiameter;
        public int explosionSpeed;
        public int knockBackSpeed;
        public bool knockBack;
        public int damage;
        public bool damagedOnce;
        public int castTime;
        public bool immediateCast; // no cast time! immediately  
        public bool singleFrame; // you hit if you are in it ... unless no
        public bool explosive; // means the width is increasing over time till reach the max diameter
        public bool perHalfSeconds;
        [JsonIgnore] public Action<Champion> perHalfSecondsAction;
        public int perHalfSecondTimer;
        public bool towardEdge;
        public int knockBackAmount;
        public bool championFollowning;
        [JsonIgnore] public Action<Champion> afterHitEnemyAction;

        
        public AreaOfDamage() : base()
        {
            immediateCast = true;
            singleFrame = false;
        }

        public override void UpdateTransform(long deltaTime)
        {
            if (championFollowning && ownerChampion != null)
            {
                shape.moveTo(ownerChampion.hitCircle.position);
            }
            if (singleFrame)
            {
                return;
            }
            if (explosive)
            {
                // should be circle 
                if (shape is Circle circle)
                {
                    circle.radius += (explosionSpeed * deltaTime / 33f);
                    if (circle.radius > maxDiameter)
                    {
                        deleted = true;
                    }
                }
            }
        }

        public void proceedCastTime(int deltaTime)
        {
            if (!immediateCast)
            {
                castTime -= deltaTime;
            }
            if (castTime <= 0)
            {
                immediateCast = true;
            }
        }
        
        public override void IntersectingWithObstacle(Obstacle thatObs)
        {
            
        }

        public void proceedPerHalfSeconds(int deltaTime)
        {
            if (perHalfSeconds)
            {
                perHalfSecondTimer -= deltaTime;
                if (perHalfSecondTimer <= 0)
                {
                    damagedOnce = false;
                    perHalfSecondTimer = 500;
                }
            }
        }

        public override void IntersectingWithPlayer(Champion enemy)
        {
            if (damagedOnce)
            {
                return;
            }
            if (perHalfSeconds && perHalfSecondTimer >= 500)
            {
                perHalfSecondsAction.Invoke(enemy);
                damagedOnce = true;
            }
            else
            {
                enemy.GetHit(this);
                damagedOnce = true;
                afterHitEnemyAction?.Invoke(enemy);
            }
            ownerChampion.room.AddEvent(GameEvent.GameEventType.HIT, damage, enemy.owner.order);
        }
    }
}