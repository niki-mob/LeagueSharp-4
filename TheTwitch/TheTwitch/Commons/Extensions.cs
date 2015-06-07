﻿using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace TheTwitch.Commons
{
    public static class Extensions
    {
        public static SpellState GetState(this SpellDataInst spellData)
        {
            switch((int)spellData.State)
            {
                case 0:
                    return SpellState.Ready;
                case 2:
                    return SpellState.NotLearned;
                case 4:
                    return SpellState.Surpressed;
                case 5:
                    return SpellState.Cooldown;
                case 6:
                    return SpellState.NoMana;
                default:
                    return SpellState.Unknown;
            }
        }

        public static SpellState GetState(this Spell spellData)
        {
            return spellData.Instance.GetState();
        }

        public static T ToEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str);
        }
    }
}
