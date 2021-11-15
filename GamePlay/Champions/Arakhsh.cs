using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities.Geometry;
using Serialization.SyncClasses;

namespace MobarezooServer.GamePlay.Champions
{
    public class Arakhsh : Champion
    {
        public Arakhsh(Room room, int level , int stars) : base(room , "ARAKHSH" , ChampionType.ARAKHSH,level,stars)
        {
            
        }
        public void ShootAgain(StackingProjectile projectile)
        {
            var enemy = room.FindEnemyChampion(this);
            if (enemy != null)
            {
                float beforeCheckRotation = projectile.shape.rotation;
                Vector2 beforeCheckPostion = projectile.shape.position;
                projectile.FaceProjectileToEnemy(enemy);
                projectile.speed = projectileSpeed;
                projectile.UpdateTransform(60);
                if (projectile.lastHitedObstacle.shape.IsIntersecting(projectile.shape))
                {
                    projectile.shape.moveTo(beforeCheckPostion);
                    projectile.speed = 0;
                    projectile.shape.Rotate(beforeCheckRotation);
                    projectile.stacks--;
                    if (projectile.stacks <= 0)
                    {
                        projectile.deleted = true;
                        projectile.onHitObstacleAction = null;
                        return;
                    }
                    else
                    {
                        projectile.StartWaitForAction(() =>
                        {
                            ShootAgain(projectile);
                        }, attackInterval);
                        return;
                    }
                }
                projectile.onHitObstacleCalled = false;
                projectile.speed = projectileSpeed;
                projectile.wallStopping = true;
                projectile.wallPassing = false;
                projectile.stacks--;
            }
        }

        public override void StartStop()
        {
            
        }

        public override void StartMove()
        {
            
        }

        public override GameObject GetAttackProjectile(ProtoChampion  prss)
        {
            var diam = prss.attackTarget.getVector() - hitCircle.position;
            var attackFacing =(float) (Math.Atan2(diam.Y, diam.X) * (180 / Math.PI));
            var newObj = new StackingProjectile()
            {
                type = GameObject.GameObjectType.ARAKHSH_ATTACK,
                shape = new Rectangle(prss.position , myBalance.x1 , myBalance.y1 , attackFacing),
                ownerChampion = this,
                speed = projectileSpeed,
                enemyHitActing = true,
                damage = attackDamage,
                homing = false,
                onHitEffect = false,
                piercing = true,
                wallStopping = true,
                stacks = (int) (level < 5 ? 1 : (level < 10 ? myBalance.bV1 : myBalance.bV2 ))
            };
            newObj.onHitObstacleAction = () =>
            {
                if (newObj.stacks <= 0)
                {
                    newObj.deleted = true;
                    newObj.wallStopping = false;
                    newObj.wallPassing = false;
                }
                else
                {
                    newObj.StartWaitForAction(() =>
                    {
                        ShootAgain(newObj);
                    }, attackInterval);
                }
            };
            return newObj;
        }

       
        
 
    }
}