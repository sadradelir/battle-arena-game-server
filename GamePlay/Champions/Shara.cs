using System;
using System.Linq;
using MobarezooServer.BalanceData;
using MobarezooServer.GamePlay.Champions.Modifiers;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities;
using MobarezooServer.Utilities.Geometry;
using Serialization.SyncClasses;

namespace MobarezooServer.GamePlay.Champions
{
    public class Shara : Champion
    {
        public int parishCounter;
       
        public Shara(Room room , int level , int stars) : base(room , "SHARA" , ChampionType.SHARA,level,stars)
        {
            // _______CHARACTER SPECIFIED STAT_______/
            parishCounter = 0;
            
        }

        public override void StartStop()
        {
            
        }

        public override void StartMove()
        {
            
        }

        public override void OnHitEffect(Champion hitPlayer , Projectile p)
        {
            attackInterval = (int) (attackInterval * myBalance.bV2);
            attackInterval.clampTo(myBalance.limit2, 20000);
            
            if (level < 5)
            {
                return;
            }
            parishCounter++;
            if (parishCounter == 3)
            {
               
                GameObject parish = new Projectile()
                {
                    type = GameObject.GameObjectType.SHARA_BIRD,
                    shape = new Rectangle(hitCircle.position , myBalance.x2 , myBalance.y2 , 0),
                    damage = (int) (attackDamage * myBalance.d1M),
                    enemyHitActing = true,
                    ownerChampion = this,
                    speed = (int) (projectileSpeed * myBalance.s1M),
                    homing = true,
                    onHitEffect = false,
                    homingTarget = hitPlayer.hitCircle
                };
                lock (room.objectsLock)
                {
                    room.objects.Add(parish);
                }

                parishCounter = 0;
            }
        
          
            if (level >= 10)
            {
                // LEGACY (NOW IS BUFF SYSTEM) : hitPlayer.champion.speed = (int)(hitPlayer.champion.speed * 0.95);
                // new status effect system
                hitPlayer.AddStatusEffect(new StatusEffect("SharaSlow", StatusEffect.EffectType.SLOW ,-1, (int) myBalance.bV1 , 0 , true , 8 ));
            }
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
                type =  GameObject.GameObjectType.SHARA_ARROW ,
                shape = new Rectangle(prss.position , myBalance.x1 , myBalance.y1 , attackFacing),
                ownerChampion = this,
                speed = myBalance.pS,
                enemyHitActing = true,
                damage = attackDamage,
                homing = false,
                onHitEffect = true,
                piercing = false,
            };
            return newObj;
        }
 
    }
}