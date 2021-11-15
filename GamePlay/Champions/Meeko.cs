using System;
using System.Linq;
using MobarezooServer.GamePlay.Champions.Modifiers;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities;
using MobarezooServer.Utilities.Geometry;
using Serialization.SyncClasses;

namespace MobarezooServer.GamePlay.Champions
{
    public class Meeko : Champion
    {
        private int stackingDamage;
        private int speedBuffPercent;
        public Meeko(Room room, int level , int stars) : base(room,"MEEKO",ChampionType.MEEKO,level,stars)
        {
            if (level >= 5)
            {
                var effect = new StatusEffect("meeko-speed", StatusEffect.EffectType.SPEED, -1, speedBuffPercent);
                effect.onProceedUpdate = (percent) =>
                {
                    effect.percent = (int) (100 * myBalance.s1M * ((maxHealth - health) / (float)maxHealth));;
                };
                AddStatusEffect(effect);
            }
        }

        public override void StartStop()
        {
            
        }

        public override void StartMove()
        {
            
        }

        public override void OnHitEffect(Champion hitPlayer , Projectile p)
        {
            // lvl 1 
            stackingDamage += (int)myBalance.d1M;
            
            if (level >= 10)
            {
                Heal((int) (p.damage * myBalance.hL));
            }
        }

        public override GameObject GetAttackProjectile(ProtoChampion  prss)
        {
            var diam = prss.attackTarget.getVector() - hitCircle.position;
            var attackFacing =(float) (Math.Atan2(diam.Y, diam.X) * (180 / Math.PI));
            
            var newObj = new Projectile()
            {
                shape = new Rectangle(prss.position , myBalance.x1 , myBalance.y1 , attackFacing),
                type = GameObject.GameObjectType.MEEKO_KONAI,
                ownerChampion = this,
                speed = projectileSpeed,
                enemyHitActing = true,
                damage = attackDamage + stackingDamage,
                homing = false,
                onHitEffect = true,
                piercing = false,
            };
            return newObj;
        }
 
    }
}