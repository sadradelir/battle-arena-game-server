using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using MobarezooServer.BalanceData;
using MobarezooServer.GamePlay;
using MobarezooServer.GamePlay.Champions.Modifiers;
using MobarezooServer.GamePlay.EventSystem;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities;
using MobarezooServer.Utilities.Geometry;
using Newtonsoft.Json;
using Serialization.SyncClasses;

namespace MobarezooServer.GamePlay.Champions
{
    public abstract class Champion
    {
        public enum ChampionType
        {
            SHARA = 1, //
            FARIVAZ = 2, // 
            SIVAN = 3, // 
            ARITA = 4, // 
            DIANOUSH = 5,
            MEEKO = 6,
            VEEMAN = 7,
            ARAKHSH = 8, // 
            LANGAAR = 9
        }

        public ChampionType type;

        // ----------- IDENTITY -------------- // 
        [JsonIgnore] public Room.SummonerData owner;

        [JsonIgnore] public Room room;

        [JsonIgnore] public string name;


        // ----------- GEOMETRY -------------- //
        public Circle hitCircle; // it has rotation also!

        // ----------- BASE STAT -------------- //    
        [JsonProperty("mhp")] public int maxHealth;
        [JsonProperty("ai")] public int attackInterval;
        [JsonIgnore] public int baseMovementSpeed;
        [JsonProperty("ps")] public int projectileSpeed;
        [JsonProperty("dmg")] public int attackDamage;


        // ----------- DYNAMIC STAT -------------- //    
        [JsonProperty("bs")] public List<StatusEffect> statusEffects;
        [JsonProperty("h")] public int health;
        [JsonProperty("l")] public int level;
        [JsonProperty("st")] public int stars;

        [JsonProperty("s")]
        public int speed
        {
            get
            {
                // should i do it in getter ? is it good ? 
                var toIssueBuffs = statusEffects.ToList(); // for safe iteration ... 

                var totalSpeedPercent = 0;
                var totalSlowPercent = 0;
                var totalSlowValue = 0;
                var totalSpeedValue = 0;

                totalSpeedPercent += toIssueBuffs.Where(t => t.type == StatusEffect.EffectType.SPEED).Sum(t => t.percent * t.currentStack);
                totalSlowPercent += toIssueBuffs.Where(t => t.type == StatusEffect.EffectType.SLOW).Sum(t => t.percent * t.currentStack);
                totalSpeedValue += toIssueBuffs.Where(t => t.type == StatusEffect.EffectType.SPEED).Sum(t => t.value * t.currentStack);
                totalSlowValue += toIssueBuffs.Where(t => t.type == StatusEffect.EffectType.SLOW).Sum(t => t.value * t.currentStack);

                return totalSpeedValue - totalSlowValue + (int) (baseMovementSpeed * (100 - totalSlowPercent + totalSpeedPercent) / 100f);
            }
        }

        [JsonProperty("sh")] public int shield;
        [JsonProperty("r")] public bool reloading;
        private bool lastMoveStat;

        public bool moving
        {
            get => lastMoveStat;
            set
            {
                if (lastMoveStat && value == false)
                {
                    StartStop();
                }

                if (lastMoveStat && value == true)
                {
                    StartMove();
                }

                if (value == true)
                {
                    stopChanel();
                }

                lastMoveStat = value;
            }
        }

        // ------------ CHANNELING -------------- //    
        [JsonProperty("isC")]
        public bool isChanneling
        {
            get { return channeling; }
        }

        [JsonIgnore] private bool channeling;
        [JsonIgnore] private int channelingTime;
        [JsonIgnore] private int channelEndTime;
        [JsonIgnore] public Action channelEndAction;
        [JsonIgnore] public Action channelCancelAction;


        // ------------ CC -------------- //    
        [JsonProperty("d")] public bool disabled;
        [JsonIgnore] public int inDisableMoveSpeed;
        [JsonIgnore] public Vector2 inDisableMoveTarget;
        [JsonIgnore] public GameBalance balanceData;
        [JsonIgnore] protected ChampionBalanceData myBalance;


        public abstract void StartStop();
        public abstract void StartMove();

        public virtual void OnHitEffect(Champion hitPlayer, Projectile projectile)
        {
        }

        public abstract GameObject GetAttackProjectile(ProtoChampion actMessage);

        public virtual void Reload()
        {
        }

        public void stopChanel()
        {
            if (!channeling)
            {
                return;
            }
            Console.WriteLine("channel canceled");
            channelCancelAction?.Invoke();
            channeling = false;
            channelEndAction = null;
            channelingTime = 0;
        }

        public bool startChannelTo(Action onEnd, int forMillis, Action cancelAction = null)
        {
            if (isChanneling || disabled || moving)
            {
                Console.WriteLine("WE ARE ALREADY ON CHANNEL OR DISABLED");
                return false;
            }
            Console.WriteLine("CHANNEL STARTS ");
            channeling = true;
            channelingTime = 0;
            channelEndAction = onEnd;
            this.channelCancelAction = cancelAction;
            channelEndTime = forMillis;
            return true;
        }

        public void proceedChannel(int deltaTime)
        {
            //Console.WriteLine("proceed chanel");
            channelingTime += deltaTime;
            if (channelingTime >= channelEndTime)
            {
                //Console.WriteLine("CHANEL TIME ENDS GOING TO INVOKE");
                channelEndAction?.Invoke();
                stopChanel();
            }
        }

        public virtual void Heal(int hp)
        {
            hp = Math.Min(hp, maxHealth - health);
            if (hp > 0)
            {
                owner.details.TotalHealing += hp;
                room.AddEvent(GameEvent.GameEventType.HEAL , hp , owner.order );
                health += hp;
            }
        }

        public bool getDamage(int amount)
        {
            owner.details.TotalDamageGet += Math.Max(0 , amount - shield);
            owner.details.TotalDamageNeglected += Math.Min(amount, shield);
            var totalHealthAndShiled = health + shield;
            totalHealthAndShiled -= amount;
            if (totalHealthAndShiled >= health)
            {
                shield = totalHealthAndShiled - health;
            }
            else
            {
                shield = 0;
                health = totalHealthAndShiled;
            }
            if (health <= 0)
            {
                health = 0;
                room.AddEvent(GameEvent.GameEventType.END_GAME, health, owner.order);
                owner.details.Win = false;
                room.FindEnemyChampion(this).owner.details.Win = true;
                return true;
            }
            return false;
        }

        public virtual bool GetHit(Projectile projectile)
        {
            projectile.ownerChampion.owner.details.AttacksHit++;
            owner.details.TotalAttacksGet++;
            getDamage(projectile.damage);
            return true;
        }

        public virtual void GetHit(AreaOfDamage areaOfDamage)
        {
            areaOfDamage.ownerChampion.owner.details.AttacksHit++;
            owner.details.TotalAttacksGet++;
            getDamage(areaOfDamage.damage);
            // margin 

            if (areaOfDamage.knockBack)
            {
                disabled = true;
                inDisableMoveSpeed = areaOfDamage.knockBackSpeed;
                if (areaOfDamage.towardEdge)
                {
                    var deltaFromCenter = hitCircle.position - areaOfDamage.shape.position;
                    var point = areaOfDamage.shape.position + ((areaOfDamage.maxDiameter / 2f + hitCircle.radius) * Vector2.Normalize(deltaFromCenter));
                    inDisableMoveTarget = point;
                }
                else
                {
                    Console.WriteLine("HIR TO KNOCK BACK");
                    var point = MathUtils.FindMirrorPoint(areaOfDamage.position, hitCircle.position, areaOfDamage.knockBackAmount);
                    inDisableMoveTarget = point;
                }
            }
        }

        protected Champion(Room room,string name,ChampionType type,int level,int stars)
        {
            hitCircle = new Circle(Vector2.Zero, 75);
            statusEffects = new List<StatusEffect>();
            balanceData = room.balanceData;
            this.room = room;
            this.name = name;
            this.level = level;
            this.stars = stars;
            this.type = type;
            //--
            myBalance = balanceData.championsBalance[name];
            
            attackDamage = myBalance.aD +
                           (level - 1) * myBalance.adpl +
                           (stars * myBalance.adstar) +
                           ((level - 1) * stars * myBalance.adplstar);
            
            maxHealth = myBalance.hP +
                        (level - 1) * myBalance.hppl +
                        (stars * myBalance.hpstar) +
                        ((level - 1) * stars * myBalance.hpplstar);
            
            health = maxHealth;
            attackInterval = myBalance.aI;
            baseMovementSpeed = myBalance.mS;
            projectileSpeed = myBalance.pS;
        }

        public virtual void ProcessTheActMessage(ProtoChampion actMessage)
        {
            // just some champions
        }

        public bool AddStatusEffect(StatusEffect statusEffect)
        {
            var se = statusEffects.FirstOrDefault(t => t.name == statusEffect.name);
            if (se != null)
            {
                if (se.stacking)
                {
                    se.currentStack++;
                    se.currentStack.clampTo(0, se.stackLimit);
                }

                // reset
                se.Reset();
            }
            else
            {
                statusEffects.Add(statusEffect);
                return true;
            }    
            return false;
        }


        public void UpdateBuffs(int deltaTime)
        {
            foreach (StatusEffect buff in statusEffects)
            {
                var actAfterProceed = buff.Proceed((int) deltaTime);
                if (actAfterProceed)
                {
                    // act is for damage and heal ... slow and speed will trigger eventually 
                    var buffPerStackValue = buff.value + (buff.stacking ? ((buff.currentStack - 1) * buff.perStackValue) : 0);
                    if (buff.type == StatusEffect.EffectType.DAMAGE)
                    {
                        getDamage(buffPerStackValue);
                        room.AddEvent(GameEvent.GameEventType.BUFF_DAMAGE , buffPerStackValue , owner.order );
                    }
                    else if (buff.type == StatusEffect.EffectType.HEAL)
                    {
                        Heal(buffPerStackValue);
                    }
                }
            }

            statusEffects = statusEffects.Where(t => t.isAlive()).ToList();
        }

        private List<Trap> lastFrameTraps;

        public void GetTrapsEffect(List<Trap> traps)
        {
            foreach (var trap in traps)
            {
                if (trap.OnHitStatusEffect != null)
                {
                    AddStatusEffect(trap.OnHitStatusEffect);
                }

                if (lastFrameTraps.Contains(trap))
                {
                    continue;
                }

                if (trap.instanceDamage != 0)
                {
                    getDamage(trap.instanceDamage);
                }

                if (trap.instanceHeal != 0)
                {
                    Heal(trap.instanceHeal);
                }

                lastFrameTraps.Add(trap);
            }

            lastFrameTraps = traps;
        }

        public bool IsIntersecting(Shape otherShape)
        {
            return hitCircle.IsIntersecting(otherShape);
        }
    }
}