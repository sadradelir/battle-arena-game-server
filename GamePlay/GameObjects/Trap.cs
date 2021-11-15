using System;
using System.Numerics;
using MobarezooServer.GamePlay.Champions.Modifiers;
using MobarezooServer.Utilities.Geometry;

namespace MobarezooServer.Gameplay.GameObjects
{
    public class Trap
    {
        public Shape shape;
        public int instanceHeal;
        public int instanceDamage;
        private StatusEffect _effect;

        public StatusEffect OnHitStatusEffect
        {
            set { _effect = value;}
            get
            {
                if (_effect == null)
                {
                    return null;
                }
                return new StatusEffect(_effect.name
                    , _effect.type
                    , _effect.initialDuration
                    , _effect.percent
                    , _effect.value
                    ,_effect.stacking,
                    _effect.stackLimit);
            }
        }

    }
}