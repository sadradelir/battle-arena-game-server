using System;
using System.IO;
using System.Linq;
using System.Numerics;
using MobarezooServer.GamePlay.Actors;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities;
using MobarezooServer.Utilities.Geometry;
using Serialization.SyncClasses;

namespace MobarezooServer.GamePlay.Champions
{
    public class Veeman : Champion
    {
        public bool blocker;
        private int blockerTimer;

        public AreaOfDamage afterShockExplosion;
        private int afterShockTimer;
        

        public Veeman(Room room, int level , int stars) : base(room, "VEEMAN" , ChampionType.VEEMAN,level,stars)
        {
            if (level >= 5)
            {
                room.AddTimer(new TimerLoopAction(() =>
                {
                    blockerTimer -= 500;
                    if (blockerTimer <= 0)
                    {
                        blocker = true;
                        shield = 1;
                    }
                }));
            }
            
            if (level == 10)
            {
             //   afterShockTimer = myBalance.limit2;
                afterShockExplosion = new AreaOfDamage()
                {
                    type = GameObject.GameObjectType.VEEMAN_CLOUD,
                    shape = new Circle(hitCircle.position ,(int) ((myBalance.x2 + hitCircle.radius)) ), // move with champion not size !
                    knockBack = true,
                    championFollowning = true,
                    ownerChampion = this,
                    knockBackSpeed = 10 ,
                    damage = (int) (attackDamage * myBalance.d1M),
                    towardEdge = false,
                    knockBackAmount = 300,
                };
                afterShockExplosion.afterHitEnemyAction = champion =>
                {
                    afterShockExplosion.deleted = true;
                    afterShockTimer = myBalance.limit2; 
                };
                // add after shock and its timer
                room.AddTimer( new TimerLoopAction(() =>
                {
                    if (!afterShockExplosion.deleted)
                    {
                        return;
                    }
                    afterShockTimer -= 500;
                    if (afterShockTimer <= 0)
                    {
                        afterShockExplosion.deleted = false;
                        afterShockExplosion.damagedOnce = false;
                        lock (room.objectsLock)
                        {
                            room.objects.Add(afterShockExplosion);
                        }
                    } 
                }));
                afterShockExplosion.deleted = true;
            }
        }

        public override void StartStop()
        {
            
        }

        public override void StartMove()
        {
            
        }

        public override void OnHitEffect(Champion hitPlayer, Projectile p)
        {
            hitPlayer.disabled = true;
            hitPlayer.inDisableMoveSpeed = 20;
            hitPlayer.inDisableMoveTarget = MathUtils.FindTranslationPoint(p.shape.rotation, hitPlayer.hitCircle.position ,100);
        }

        public override GameObject GetAttackProjectile(ProtoChampion prss)
        {
            return makeAttackProjectile();
        }

        private GameObject makeAttackProjectile()
        {
            var enemy = room.FindEnemyChampion(this);
            var mirrorPoint = MathUtils.FindMirrorPoint(hitCircle.position,
                enemy.hitCircle.position,
                enemy.hitCircle.radius + 500 ,
                false,
                true,
                myBalance.x1 + 1,
                myBalance.y1 + 1
                );
            var diam = enemy.hitCircle.position - hitCircle.position;
            var attackFacing = (float) (Math.Atan2(diam.Y, diam.X) * (180 / Math.PI));
            Console.WriteLine("mirror" + mirrorPoint);
            var projectile = new Projectile()
            { 
                type = GameObject.GameObjectType.VEEMAN_VOID,
                shape = new Circle(mirrorPoint , myBalance.x1 , attackFacing),
                ownerChampion = owner.champion,
                speed = 0,
                enemyHitActing = false,
                damage = attackDamage,
                homing = false,
                onHitEffect = true,
                piercing = false,
                wallPassing = false
            };
            projectile.StartWaitForAction(() =>
            {
                projectile.enemyHitActing = true;
                projectile.speed = projectileSpeed;
                projectile.FaceProjectileToEnemy(enemy);
            }, (int) myBalance.s1M);
            return projectile;
        }

        public override bool GetHit(Projectile projectile)
        {
            if (blocker)
            {
                blocker = false;
                var extraAttack = makeAttackProjectile();
                lock (room.objectsLock)
                {
                    room.objects.Add(extraAttack);
                }
                return true;
            }
            else
            {
                blockerTimer = myBalance.limit2;
                return base.GetHit(projectile);
            }
        }

        public override void GetHit(AreaOfDamage areaOfDamage)
        {
            if (blocker)
            {
                var extraAttack = makeAttackProjectile();
                lock (room.objectsLock)
                {
                    room.objects.Add(extraAttack);
                }
                blocker = false;
                return;
            }
            else
            {
                blockerTimer = myBalance.limit2;
                base.GetHit(areaOfDamage);
            }
        }

     
    }
}