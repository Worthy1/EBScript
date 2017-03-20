using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace Fiddlesticks
{
    public static class SpellManager
    {
        public static Spell.Targeted Ignite { get; private set; }
        public static Spell.Active Q { get; private set; }
        public static Spell.Targeted W { get; private set; }
        public static Spell.Targeted E { get; private set; }
        public static Spell.Targeted R { get; private set; }

        static SpellManager()
        {
            if (Player.Instance.GetSpellSlotFromName("summonerdot") != SpellSlot.Unknown)
            {
                Ignite = new Spell.Targeted(Player.Instance.GetSpellSlotFromName("summonerdot"), 600);
            }

            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Targeted(SpellSlot.W, 900);
            E = new Spell.Targeted(SpellSlot.E, 575);
            R = new Spell.Targeted(SpellSlot.R, 575);
        }
    }
}