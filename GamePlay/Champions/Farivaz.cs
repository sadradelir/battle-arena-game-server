using System;
using System.Numerics;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities;
using MobarezooServer.Utilities.Geometry;
using Serialization.SyncClasses;

namespace MobarezooServer.GamePlay.Champions
{
    public class Farivaz : Champion
    {
        public Projectile hammer;

        public Farivaz(Room room, int level, int stars) : base(room, "FARIVAZ", ChampionType.FARIVAZ, level, stars)
        {
            // _________________BASE STATS_________________//
            //_____________________________________________
        }

        void CallTheHammerBack()
        {
            hammer.speed = projectileSpeed;
            hammer.enemyHitActing = false;
            hammer.homing = true;
            hammer.homingTarget = new Circle()
            {
                position = new Vector2()
                {
                    X = (hitCircle.position.X + hammer.shape.position.X) / 2,
                    Y = (hitCircle.position.Y + hammer.shape.position.Y) / 2,
                }
            };
            hammer.wallPassing = true;
            hammer.wallStopping = false;
            hammer.onReachTargetAction = () =>
            {
                reloading = false;
                if (level <= 5)
                {
                    hammer.selfHit = true;
                    hammer.ownerIntersectAction = () =>
                    {
                        if (owner.champion.isChanneling)
                        {
                            return;
                        }

                        startChannelTo(() =>
                        {
                            Console.WriteLine("END CHANNEL of farivaz");
                            if (owner.champion.level >= 10)
                            {
                                Heal((int) (shield * myBalance.hL));
                            }

                            shield = (int) (maxHealth / myBalance.sH);
                            reloading = false;
                            hammer.deleted = true;
                            hammer.selfHit = false;
                            hammer.ownerIntersectAction = null;
                        }, attackInterval);
                    };
                }
            };
        }

        void MakeShokwave(Projectile hammer)
        {
            Rectangle wall = (Rectangle) hammer.lastHitedObstacle.shape;
            Vector2 impactPosition = new Vector2(
                hammer.position.X.returnClamp(wall.corner[0].X, wall.corner[1].X),
                hammer.position.Y.returnClamp(wall.corner[0].Y, wall.corner[3].Y)
            );
            var shockwave = new AreaOfDamage()
            {
                type = GameObject.GameObjectType.FARIVAZ_SHOCKWAVE,
                shape = new Circle(impactPosition, myBalance.x1),
                damage = (int) (attackDamage * myBalance.d1M),
                explosionSpeed = 30,
                explosive = true,
                knockBack = true,
                towardEdge = false,
                knockBackSpeed = (int) myBalance.bV1,
                maxDiameter = myBalance.x2,
                ownerChampion = this,
            };
            lock (room.objectsLock)
            {
                room.objects.Add(shockwave);
            }
        }

        public override void StartStop()
        {
        }

        public override void StartMove()
        {
        }

        public override GameObject GetAttackProjectile(ProtoChampion prss)
        {
            owner.details.TotalAttacks++;
            owner.details.CurrentAttackSpree++;
            if (owner.details.CurrentAttackSpree > owner.details.LargestAttackSpree)
            {
                owner.details.LargestAttackSpree = owner.details.CurrentAttackSpree;
            }

            var diam = prss.attackTarget.getVector() - hitCircle.position;
            var attackFacing = (float) (Math.Atan2(diam.Y, diam.X) * (180 / Math.PI));
            if (hammer != null)
            {
                hammer.shape.moveTo(prss.position);
                hammer.shape.rotation = attackFacing;
                hammer.homing = false;
                hammer.speed = projectileSpeed;
                hammer.enemyHitActing = true;
                hammer.deleted = false;
                hammer.wallPassing = false;
                hammer.onHitObstacleCalled = false;
                hammer.wallStopping = true;
                reloading = true;
                hammer.selfHit = false;
                hammer.ownerIntersectAction = null;
                return hammer;
            }

            var newObj = new Projectile()
            {
                type = GameObject.GameObjectType.FARIVAZ_HAMMER,
                shape = new Circle(prss.position, myBalance.x1, attackFacing),
                ownerChampion = this,
                speed = projectileSpeed,
                enemyHitActing = true,
                damage = attackDamage,
                homing = false,
                selfHit = false,
                onHitEffect = false,
                piercing = true,
                wallPassing = false,
                wallStopping = true,
            };
            newObj.onHitObstacleAction = () =>
            {
                MakeShokwave(newObj);
                CallTheHammerBack();
            };

            hammer = newObj;
            reloading = true;
            return newObj;
        }
    }
}