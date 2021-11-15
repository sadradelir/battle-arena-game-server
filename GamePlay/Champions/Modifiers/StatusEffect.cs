using System;
using MobarezooServer.Utilities;

namespace MobarezooServer.GamePlay.Champions.Modifiers
{
    public class StatusEffect
    {
        public enum EffectType
        {
            SLOW,
            SPEED,
            HEAL,
            DAMAGE,
            ATTACK_SPEED,
            SHIELD,
            // ... 
        }

        public readonly EffectType type;
        public readonly string name; // important
        public readonly int initialDuration; // -1 for infinity
        public readonly bool stacking;
        public readonly int stackLimit;
        public readonly int value;
        public int perStackValue;
        public int percent; // 60 means 60%
        public int timer; // in millis
        public int currentStack;
        public int tickRate; // in millis
        public int timeToTick; // in millis
        public Action<StatusEffect> onProceedUpdate;

        public StatusEffect(string name,EffectType type, int duration, int percent,  int value = 0, bool stacking = false, int stackLimit = 1)
        {
            this.percent = percent;
            this.name = name;
            this.stacking = stacking;
            this.stackLimit = stackLimit;
            this.value = value;
            this.initialDuration = duration;
            tickRate = 500;
            this.type = type;
            currentStack = 1;
            timer = 0;
        }

        public bool Proceed(int deltaTime)
        {
            onProceedUpdate?.Invoke(this);
            timeToTick += deltaTime;
            var actAfterProceed = false;
            if (timeToTick >= tickRate)
            {
                actAfterProceed = true;
                timeToTick = 0;
            }
            
            if (initialDuration < 0) // infinity
            {
                return actAfterProceed;
            }

            timer += deltaTime;
            if (timer >= initialDuration)
            {
                // delete ? no ! it will be deleted .. 
            }
            return actAfterProceed;
        }

        public void Reset()
        {
            timer = 0;
        }
        
        public bool isAlive()
        {
            return (initialDuration < 0 ) || (timer < initialDuration);
        }
    }
}