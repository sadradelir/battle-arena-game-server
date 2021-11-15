using System;
using System.Collections.Generic;
using System.Linq;
using MobarezooServer.GamePlay.Champions.Modifiers;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities.Geometry;
using Serialization.SyncClasses;

namespace MobarezooServer.GamePlay.Champions
{
    public class Sivan : Champion
    {
        public int reloadTimer;
        public List<Projectile> shottedProjectiles;
        public int attacksShot;
        
        public Sivan(Room room, int level , int stars) : base(room , "SIVAN" , ChampionType.SIVAN,level,stars)
        {
            shottedProjectiles = new List<Projectile>();
            attacksShot = 0;
        }

        public override void StartStop()
        {
            if (reloading)
            {
                var timeTochanel = myBalance.aI * myBalance.bV2 * 2 ;
                if (level >= 10)
                {    
                    timeTochanel = (int)(2 * myBalance.aI * myBalance.bV2 * ((maxHealth - health) / (float)maxHealth));
                }
                startChannelTo(Reload, (int)timeTochanel);
            }
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
            var attackFacing =(float) (Math.Atan2(diam.Y, diam.X) * (180 / Math.PI));
            var newObj = new Projectile()
            {
                type = GameObject.GameObjectType.SIVAN_AXE,
                shape = new Rectangle(prss.position , myBalance.x1 , myBalance.y1 , attackFacing),
                ownerChampion = this,
                speed = projectileSpeed,
                enemyHitActing = true,
                damage = attackDamage,
                homing = false,
                onHitEffect = false,
                piercing = true,
                wallStopping = true,
            };
            newObj.onHitObstacleAction = () =>
            {
                newObj.enemyHitActing = false;
                newObj.homing = true;
                newObj.homingTarget = hitCircle;
            };
            shottedProjectiles.Add(newObj);
            
            if (level >= 5)
            {
                attackInterval = myBalance.aI - 20 * attacksShot;
            }

            attacksShot++;
            if (attacksShot >= myBalance.bV1)
            {
                reloading = true;
                attacksShot = 0;
                StartStop();
            }
            return newObj;
        }

        public void AxeBack(Projectile projectile)
        {
            shottedProjectiles.Remove(projectile);
            if (shottedProjectiles.Count == 0 || shottedProjectiles.All(t=>t.deleted))
            {
                reloading = false;
            }
        }

        public override void Reload()
        {
            attackInterval = myBalance.aI;
            if (shottedProjectiles.Count == 0 || shottedProjectiles.All(t => t.deleted))
            {
                reloading = false;
                return;
            }
 
            // legacy
            /*if (level  <= 5)
            {
                foreach (Projectile projectile in shottedProjectiles)
                {
                    projectile.deleted = true;
                }
                reloading = false;
                return;
            }*/
            foreach (Projectile projectile in shottedProjectiles)
            {
                projectile.enemyHitActing = true;
                projectile.speed = projectileSpeed;
                projectile.homing = true;
                projectile.piercing = true;
                projectile.damage = (int) (attackDamage * myBalance.d1M);
                projectile.wallStopping = false;
                projectile.wallPassing = true;
                projectile.homingTarget = hitCircle;
                projectile.onReachTargetAction = () =>
                {
                    AxeBack(projectile);
                    projectile.deleted = true;
                };
            }
        }
        
        
 
    }
}