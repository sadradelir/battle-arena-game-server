using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using MobarezooServer.GamePlay.Champions.Modifiers;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities;
using MobarezooServer.Utilities.Geometry;
using Serialization.SyncClasses;

namespace MobarezooServer.GamePlay.Champions
{
    public class Langaar : Champion
    {
        public MathUtils.RefInteger anchorReturnSpeed;
        public Projectile lastShootedAnchor;
        public enum AnchorState
        {
            INHAND,SHOOT,ORBIT,PULL
        }

        public AnchorState anchorState;
        public Langaar(Room room, int level , int stars) : base(room,"LANGAAR",ChampionType.LANGAAR,level,stars)
        {
            anchorState = AnchorState.INHAND;
            anchorReturnSpeed = new MathUtils.RefInteger()
            {
                value = projectileSpeed / 2
            };
        }

        public override void StartStop()
        {
            anchorReturnSpeed.value = 0;
            reloading = false;
        }

        public override void StartMove()
        {
            if (lastShootedAnchor == null)
            {
                return;
            }
            // pulling 
            if ( anchorState == AnchorState.PULL )
            {
                lastShootedAnchor.orbital = false;
                lastShootedAnchor.homing = true;
                lastShootedAnchor.enemyHitActing = false;
                lastShootedAnchor.pulling = true;
                lastShootedAnchor.wallStopping = false;
                lastShootedAnchor.remoteSpeedChange = true;
                lastShootedAnchor.orbitalRotation = 0;
            }
            anchorReturnSpeed.value = projectileSpeed / 2;
        }
        
        public override void OnHitEffect(Champion hitPlayer , Projectile projectile )
        {
            if (!hitPlayer.disabled)
            {
                hitPlayer.disabled = true;
                hitPlayer.inDisableMoveSpeed = 10;
                // todo fixme
                var axisY = Vector2.Normalize(((Rectangle) projectile.shape).position);
                var axisX = Vector2.Normalize(((Rectangle) projectile.shape).position);
                Vector2 delta = hitPlayer.hitCircle.position - projectile.shape.position;
                var radian = Math.Acos(Vector2.Dot(delta, axisX) / (delta.LengthSquared() * axisX.LengthSquared()));
                var knockSideTarget = hitPlayer.hitCircle.position.Clone();
                knockSideTarget.X += (radian < 0 ? 1 : -1) * (175) * axisY.X;
                knockSideTarget.Y += (radian < 0 ? 1 : -1) * (175) * axisY.Y;
                hitPlayer.inDisableMoveTarget = knockSideTarget;
            }
        }

        public override GameObject GetAttackProjectile(ProtoChampion  prss)
        {
            
            if (anchorState == AnchorState.SHOOT)
            {
                throw new Exception();
            }
            //swing 
            if (anchorState == AnchorState.PULL)
            {
                var diam1 = prss.attackTarget.getVector() - lastShootedAnchor.position;
                var diam2 = prss.attackTarget.getVector() - hitCircle.position;
                var angle = Math.Atan2(diam1.X*diam2.Y - diam2.X*diam1.Y  ,diam1.X*diam2.X - diam2.Y*diam1.Y );
                anchorState = AnchorState.ORBIT;
                lastShootedAnchor.speed = projectileSpeed * (angle < 0 ? 1 : -1);
                lastShootedAnchor.enemyHitActing = true;
                lastShootedAnchor.orbital = true;
                lastShootedAnchor.orbitalRotation = 0;
                lastShootedAnchor.wallStopping = true;
                lastShootedAnchor.homing = false;
                lastShootedAnchor.onHitObstacleCalled = false;
                lastShootedAnchor.pulling = false;
                lastShootedAnchor.damage = attackDamage;
                lastShootedAnchor.remoteSpeedChange = false;
                lastShootedAnchor.orbitPivot = hitCircle.position;
                reloading = true;
                disabled = true;
                lastShootedAnchor.onHitObstacleAction = () =>
                {
                    Console.WriteLine("HIT WALL ON SWING GET TO STATE PULLING");
                    anchorState = AnchorState.PULL;   
                    attackInterval = myBalance.aI / 4;
                    disabled = false;
                    lastShootedAnchor.homing = true;
                    lastShootedAnchor.FaceToTarget(hitCircle.position);
                    reloading = false;
                    lastShootedAnchor.orbitalRotation = 0;
                    lastShootedAnchor.orbital = false;
                    lastShootedAnchor.StartWaitForAction(MakeSlowingExplosion , myBalance.limit1);
                };
                return lastShootedAnchor;
            }
            
            
            Console.WriteLine("MAKE NEW PROJECTILE");
            reloading = true;
            var diam = prss.attackTarget.getVector() - hitCircle.position;
            var attackFacing =(float) (Math.Atan2(diam.Y, diam.X) * (180 / Math.PI));
            var anchor = new Projectile()
            {
                type = GameObject.GameObjectType.LANGAAR_ANCHOR,
                shape = new Rectangle(prss.position , myBalance.x1 , myBalance.y1 , attackFacing),
                ownerChampion = this,
                speed = projectileSpeed,
                enemyHitActing = true,
                damage = attackDamage,
                homing = false,
                onHitEffect = true,
                piercing = true,
                wallStopping = true,
            };
            anchorState = AnchorState.SHOOT;
            anchor.onHitObstacleAction = () =>
            {
                anchor.enemyHitActing = false;
                anchor.damage = 0;
                anchor.onHitObstacleAction = null;
                anchor.wallPassing = true;
                anchor.wallStopping = false;
                anchor.homing = true;
                anchor.FaceToTarget(hitCircle.position);
                anchor.pulling = true;
                anchor.remoteSpeed = anchorReturnSpeed;
                anchor.remoteSpeedChange = true;
                anchor.homingTarget = hitCircle;
                anchor.selfHit = true;
                anchor.hitActedOnce = false;
                anchorState = AnchorState.PULL;
                attackInterval = myBalance.aI / 4;
                anchor.ownerIntersectAction = () =>
                {
                    Console.WriteLine("REACH OWNER ");
                    anchor.deleted = true;
                    reloading = false;
                    anchorState = AnchorState.INHAND;
                    attackInterval = myBalance.aI;
                };
                anchor.StartWaitForAction(MakeSlowingExplosion, myBalance.limit1);
            };
            lastShootedAnchor = anchor;
            return anchor;
        }

        private void MakeSlowingExplosion()
        {
            var obj = new AreaOfDamage()
            {
                type = GameObject.GameObjectType.LANGAAR_SLOW,
                damage = attackDamage / 2,
                shape = new Rectangle(lastShootedAnchor.position , myBalance.x2 , myBalance.y2 , lastShootedAnchor.shape.rotation),
                ownerChampion = this,
                singleFrame = true,
                immediateCast = true,
                afterHitEnemyAction = enemy =>
                {
                    enemy.AddStatusEffect(new StatusEffect("LangaarSlow" , StatusEffect.EffectType.SLOW , (int) myBalance.bV2 , (int) myBalance.bV1 ));
                }
            };
            lock (room.objectsLock)
            {
                room.objects.Add(obj);
            }
        }

        public override void GetHit(AreaOfDamage areaOfDamage)
        {
            GetDamageInstance();
            base.GetHit(areaOfDamage);
        }
 

        public override bool GetHit(Projectile projectile)
        {
            GetDamageInstance();
            return base.GetHit(projectile);
        }

        private void GetDamageInstance()
        {
            if (level < 5)
            {
                return;
            }
            if (lastShootedAnchor.homing == false)
            {
                attackInterval -= (int)myBalance.s1M;
                attackInterval.clampTo(myBalance.limit2, 2500);
            }
            else
            {
                anchorReturnSpeed.value = (int) (anchorReturnSpeed.value * myBalance.d1M);
                lastShootedAnchor.StartWaitForAction(() => { anchorReturnSpeed.value = projectileSpeed / 2; }, 500);
            }
        }

    }
}