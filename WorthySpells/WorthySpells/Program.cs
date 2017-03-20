using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;

namespace WorthySpells
{
    class Program
    {
        public static AIHeroClient PPMT { get { return ObjectManager.Player; } }
        public static Spell.Targeted ignt = new Spell.Targeted(PPMT.GetSpellSlotFromName("summonerdot"), 600);
        private static Menu Menu,
            Spell, 
            SpellDraw;
        static void Main(string[] args)
        {
            //Tutorial <3
            Loading.OnLoadingComplete += Game_OnStart;
            Drawing.OnDraw += Game_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            //Toyota 7 Thank you for lending <3
            var target = TargetSelector.GetTarget(700, DamageType.True, Player.Instance.Position);

            float IgniteDMG = 50 + (20 * PPMT.Level);

            if (target != null)
            {
                float HP5 = target.HPRegenRate * 5;

                if (Check(Menu, "Igit") && ignt.IsReady() && target.IsValidTarget(ignt.Range) &&
                    (IgniteDMG > (target.TotalShieldHealth() + HP5)))
                {
                    ignt.Cast(target);
                }
            }
        }

        private static bool Check(Menu menu, string v)
        {
            return menu[v].Cast<CheckBox>().CurrentValue;
        }

        private static void Game_OnDraw(EventArgs args)
        {
            if (Check(Menu, "Draw") && ignt.IsReady())
            {
                Circle.Draw(SharpDX.Color.Green, ignt.Range, PPMT.Position);
            }
        }
        private static void Game_OnStart(EventArgs args)
        {
            Chat.Print("<font color = '#20b2aa'>Welcome to </font><font color = '#ffffff'> WorthySpells  " + Player.Instance.ChampionName + "</font><font color = '#20b2aa'>. Addon in Beta.</font>");
            Menu = MainMenu.AddMenu("MenuSpells", "Spells");
            Menu.AddLabel("By Worthy");

            Spell = Menu.AddSubMenu("Spell");
            Spell.Add("Use Igit", new CheckBox("Use Spell", true));
            Spell.Add("Min Life", new Slider("Use Min <= {0}, 15,25,50"));
            Spell.Add("Min", new Slider("Min.Delay(ms)", 1, 0, 1000));
            Spell.Add("Max", new Slider("Max. Delay (ms)", 249, 0, 1000));
            SpellDraw = Menu.AddSubMenu("Draw", "SpellDraw");
            SpellDraw.Add("Draw", new CheckBox("Draw Spell", true));
        }
    }
}
