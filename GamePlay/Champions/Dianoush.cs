using MobarezooServer.GamePlay.Champions.Modifiers;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities.Geometry;
using Serialization.SyncClasses;

namespace MobarezooServer.GamePlay.Champions
{
    public class Dianoush : Champion
    {
        public bool blocker;
        public int stacks;
        public Dianoush(Room room, int level , int stars) : base(room , "DIANOUSH" , ChampionType.DIANOUSH,level,stars)
        {
            
            var statusEffect = new StatusEffect("dianoush-slow" , StatusEffect.EffectType.SLOW , -1 ,0);
            statusEffect.onProceedUpdate = effect =>
            {
                effect.percent = (int) (stacks * myBalance.bV2);
            };
            AddStatusEffect(statusEffect);
            
        }

        public override void StartStop()
        {
            
        }

        public override void StartMove()
        {
            
        }

        public override void ProcessTheActMessage(ProtoChampion  actMessage)
        {
            stacks = actMessage.attackParameter1;
        }

        public override void OnHitEffect(Champion hitPlayer , Projectile p )
        {
            if (level < 5)
            {
                return;
            }
            if (p is StackingProjectile) // why not ? 
            {
                if (((StackingProjectile) p).stacks >= myBalance.limit1)
                {
                    blocker = true;
                }
            }
        }

        public override GameObject GetAttackProjectile(ProtoChampion  prss)
        {
            int stackPower = prss.attackParameter1;
            var newObj = new StackingProjectile()
            {
                type = GameObject.GameObjectType.DIANOUSH_SPEAR,
                shape = new Rectangle(prss.position , myBalance.x1 , myBalance.y1 , prss.rotation),
                ownerChampion = this,
                speed = (int) (projectileSpeed + myBalance.s1M * stackPower),
                enemyHitActing = true,
                damage = (int) (attackDamage + myBalance.d1M * stackPower) ,
                homing = false,
                onHitEffect = true,
                piercing = true,
                wallPassing = level >= 10 && stackPower >= myBalance.limit1,
                stacks = stackPower
            };
            return newObj;
        }

        public override bool GetHit(Projectile projectile)
        {
            if (blocker)
            {
                blocker = false;
                return true;
            }
            else
            {
                return base.GetHit(projectile);
            }
        }
    }
}