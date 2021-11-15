using System;
using System.Linq;
using System.Numerics;
using MobarezooServer.GamePlay.Actors;
using MobarezooServer.GamePlay.Champions.Modifiers;
using MobarezooServer.GamePlay.EventSystem;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities;
using MobarezooServer.Utilities.Geometry;
using Serialization.SyncClasses;

namespace MobarezooServer.GamePlay.Champions
{
    public class Arita : Champion
    {
        public int stacks;
        public int stackTimer;
        public Arita(Room room, int level , int stars) : base(room , "ARITA" , ChampionType.ARITA,level,stars)
        {

            var timer = new TimerLoopAction(() =>
            {
                stackTimer++;
                if (stackTimer >= myBalance.limit2)
                {
                    stacks--;
                    stackTimer = 0;
                    stacks.clampTo(0,myBalance.limit1);
                }
            });
            room.AddTimer(timer);
        }

        public override void StartStop()
        {
            
        }

        public override void StartMove()
        {
            
        }
        public override GameObject GetAttackProjectile(ProtoChampion  prss)
        {
             stacks++;
             stacks.clampTo(0,myBalance.limit1);
             stackTimer = 0;
             // it is not projectile! or is it ? it is ! 
             var newObj = new AreaOfDamage()
             {
                 shape = new Circle(prss.attackTarget, myBalance.x1 + myBalance.x2 * stacks),
                 ownerChampion = this,
                 damage = attackDamage,
                 castTime = projectileSpeed * 20,
                 singleFrame = true,
                 immediateCast = false,
                 type = GameObject.GameObjectType.ARITA_FIRE_BALL
             };
             newObj.StartWaitForAction(() =>
             {
                 var aoe = new AreaOfDamage()
                 {
                     type = GameObject.GameObjectType.ARITA_FIRE_GROUND,
                     ownerChampion = this,
                     shape = newObj.shape, // i should clone it !
                     damage = (int) (attackDamage * myBalance.bV1),
                     perHalfSeconds = true,
                     perHalfSecondsAction = (enemy) =>
                     {
                         var statusEffect = new StatusEffect("arita-damage" , 
                             StatusEffect.EffectType.DAMAGE ,
                             (int) myBalance.bV2,
                             0 , (int) (attackDamage * myBalance.bV1) , level > 10 , 100);
                         statusEffect.perStackValue = (int) (myBalance.d1M * attackDamage);
                         enemy.AddStatusEffect(statusEffect);
                         room.AddEvent(GameEvent.GameEventType.IGNITE , 0 , enemy.owner.order );
                     }
                 };
                 aoe.StartWaitForAction(() =>
                 {
                     aoe.deleted = true;
                 },(int) myBalance.s1M);
                 lock (room.objectsLock)
                 {
                    room.objects.Add(aoe);
                 }
             },projectileSpeed * 20);
             return newObj;
        }

    }
}