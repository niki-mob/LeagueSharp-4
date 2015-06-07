﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using TheBrand.ComboSystem;
using TheBrand.Commons;

namespace TheBrand
{
    class BrandCombo : ComboProvider
    {
        // ReSharper disable once InconsistentNaming
        public bool ForceAutoAttacks;

        public BrandCombo(IEnumerable<Skill> skills, Orbwalking.Orbwalker orbwalker, float range)
            : base(range, skills, orbwalker) { }

        public BrandCombo(float range, Orbwalking.Orbwalker orbwalker, params Skill[] skills)
            : base(range, orbwalker, skills) { }

        public override void Update()
        {
            if (!(ForceAutoAttacks && ObjectManager.Player.IsWindingUp))
                base.Update();


            var passiveBuff = ObjectManager.Player.GetBuff("brandablaze");
            var target = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);

            if (passiveBuff != null)
                IgniteManager.Update(this, GetRemainingPassiveDamage(target, passiveBuff), (int)(passiveBuff.EndTime - Game.Time) + 1); // maybe should use GetTarget!?
            else
                IgniteManager.Update(this); // maybe should use GetTarget!?

        }

        public override bool ShouldBeDead(Obj_AI_Hero target, float additionalSpellDamage = 0f)
        {
            var passive = target.GetBuff("brandablaze");
            return base.ShouldBeDead(target, passive != null ? GetRemainingPassiveDamage(target, passive) : 0f);
        }


        private float GetRemainingPassiveDamage(Obj_AI_Base target, BuffInstance passive)
        {
            return (float)ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, ((int)(passive.EndTime - Game.Time) + 1) * target.MaxHealth * 0.02f);
        }

        public static float GetPassiveDamage(Obj_AI_Hero target)
        {
            return (float)ObjectManager.Player.CalcDamage(target, Damage.DamageType.Magical, target.MaxHealth * 0.08);
        }
    }
}
