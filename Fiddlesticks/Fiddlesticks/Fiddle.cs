using System;
using SharpDX;
using EloBuddy;
using System.Linq;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Spells;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Enumerations;
using System.Collections.Generic;
using Color = System.Drawing.Color;

namespace Fiddlesticks
{
    class Fiddle
    {
        public static Menu Menu,
            ComboMenu,
            DrawMenu,
            HarassMenu,
            JungleMenu,
            LaneMenu;

        public static Spell.Active Q;
        public static Spell.Targeted W;
        public static Spell.Skillshot E;
        public static Spell.Targeted R;
        private static Vector3 Position;
        private static bool IsPreAa;
        private static int ManaPercent;

        public static PredictionResult HarrasWPred(Obj_AI_Base harrasW)
        {
            var coneW = new Geometry.Polygon.Sector(Fiddle.Position, Game.CursorPos, (float)(Math.PI / 180 * 40), 1250, 9).Points.ToArray();
            for (var x = 1; x < 10; x++)
            {
                var prophecyW = Prediction.Position.PredictLinearMissile(harrasW, 1250, 20, 250, 1500, 0, Fiddle.Position.Extend(coneW[x], 20).To3D());
                if (prophecyW.CollisionObjects.Any() || (prophecyW.HitChance < HitChance.High)) continue;
                return prophecyW;
            }
            return null;
        }

        public static Vector3? ServerPosition { get; private set; }
        public static GameObject[] Caoujter { get; private set; }

        private static bool SpellShield(Obj_AI_Base shield) { return shield.HasBuffOfType(BuffType.SpellShield) || shield.HasBuffOfType(BuffType.SpellImmunity); }

        public static void Load()
        {
            Chat.Print("<font color = '#cfa9a'>Welcome to </font><font color = '#ffffff'> Worthy " + Player.Instance.ChampionName + "</font><font color = '#cfa9a'>. Addon in Beta.</font>");
            Menu = MainMenu.AddMenu("Fiddlesticks", "ConFiddlesticks");
            Menu.AddSeparator();
            Menu.AddLabel("Addon Fiddlesticks");

            DrawMenu = Menu.AddSubMenu("Draw", "DrawFiddles");
            DrawMenu.Add("drawDisable", new CheckBox("Disable all Draws", true));
            DrawMenu.Add("Draw Q", new CheckBox("Active Draw Q", true));
            DrawMenu.Add("Draw W", new CheckBox("Active Draw W", true));
            DrawMenu.Add("Draw E", new CheckBox("Active Draw E", true));
            DrawMenu.Add("Draw R", new CheckBox("Active Draw R", true));
            ////////////////////////////////////////////////////////////////
            ComboMenu = Menu.AddSubMenu("Combo", "ComboFiddles");
            ComboMenu.Add("comboQ", new CheckBox("Use Q in combo", true));
            ComboMenu.Add("comboW", new CheckBox("Use W in combo", true));
            ComboMenu.Add("comboE", new CheckBox("Use E in combo", true));
            ComboMenu.Add("comboR", new CheckBox("Use R in combo", false));
            ///////////////////////////////////////////////////////////////
            HarassMenu = Menu.AddSubMenu("Harass", "HarassFiddles");
            HarassMenu.Add("Harass Q", new CheckBox("Use Q in Harass", true));
            HarassMenu.Add("Harass W", new CheckBox("Use W in Harass", false));
            HarassMenu.Add("Harass E", new CheckBox("Use E In Harass", true));
            HarassMenu.Add("Harass HitChance", new Slider("Min >= {0}", 25, 50, 75));
            HarassMenu.Add("Min%", new Slider("Min Mana >= {0}", 25, 50, 75));
            ///////////////////////////////////////////////////////////////////
            JungleMenu = Menu.AddSubMenu("JungleClear", "JungleFiddles");
            JungleMenu.Add("Jungle Q", new CheckBox("Use Q in Jungle", false));
            JungleMenu.Add("Jungle W", new CheckBox("Use W in Jungle", true));
            JungleMenu.Add("Jungle E", new CheckBox("Use E in Jungle", true));
            JungleMenu.Add("Min%", new Slider("Min Mana >= {0}", 25, 50, 75));
            ///////////////////////////////////////////////////////////////////
            LaneMenu = Menu.AddSubMenu("LaneClear", "LaneFiddles");
            LaneMenu.Add("Lane Q", new CheckBox("Use Q in Jungle", false));
            LaneMenu.Add("Lane W", new CheckBox("Use W in Jungle", true));
            LaneMenu.Add("Lane E", new CheckBox("Use E in Jungle", true));
            LaneMenu.Add("Min%", new Slider("Min Mana >= {0}", 25, 50, 75));
            //////////////////////////////////////////////////////////////////

            Gapcloser.OnGapcloser += OnGapcloser;
            Interrupter.OnInterruptableSpell += OnInterruptableSpell;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            { Combo(); }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            { LanuClear(); }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            { JunuClear(); }
            Harras();
        }

        private static void LanuClear()
        {
            var farmClear = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Fiddle.ServerPosition).Where(x => x.IsValidTarget(W.Range - 100)).ToList();
            if (!farmClear.Any()) return;
            if ((Q.IsReady() && LaneMenu["E"].Cast<CheckBox>().CurrentValue && Fiddle.ManaPercent >= LaneMenu["LmanaP"].Cast<Slider>().CurrentValue && IsPreAa && (Fiddle.CountEnemyChampionsInRange(590) >= 1)))
            {
                Q.Cast();
            }
        }

        private static int CountEnemyChampionsInRange(int v)
        {
            throw new NotImplementedException();
        }

        private static void JunuClear()
        {
            var farmjungclear = EntityManager.MinionsAndMonsters.GetJungleMonsters().Where(x => x.IsValidTarget(Fiddle.GetAutoAttackRange())).ToList();
            if (!farmjungclear.Any()) return;
            string[] monsters = { "SRU_Gromp", "SRU_Blue", "SRU_Red", "SRU_Razorbeak", "SRU_Krug", "SRU_Murkwolf", "Sru_Crab", "SRU_RiftHerald", "SRU_Dragon", "SRU_Baron" };
            if (Q.IsReady() && JungleMenu["Q"].Cast<CheckBox>().CurrentValue && Fiddle.ManaPercent >= JungleMenu["JmanaP"].Cast<Slider>().CurrentValue && farmjungclear.Count(x => monsters.Contains(x.BaseSkinName, StringComparer.CurrentCultureIgnoreCase)) >= 1)
            {
                Q.Cast();
            }
            if (!W.IsReady() || !JungleMenu["W"].Cast<CheckBox>().CurrentValue || !(Fiddle.ManaPercent >= JungleMenu["JmanaP"].Cast<Slider>().CurrentValue)) return;
            {
                var farmjungclearW = farmjungclear.FirstOrDefault(x => monsters.Contains(x.BaseSkinName, StringComparer.CurrentCultureIgnoreCase));
                if (farmjungclearW == null || !(farmjungclearW.Health > Fiddle.GetAutoAttackDamage(farmjungclearW, true) * 2)) return;
                var pred = W.GetHealthPrediction(farmjungclearW);
            }
        }

        private static int GetAutoAttackDamage(Obj_AI_Minion farmjungclearW, bool v)
        {
            throw new NotImplementedException();
        }

        private static void Harras()
        {
            if (!E.IsReady() || !HarassMenu["E"].Cast<CheckBox>().CurrentValue || !(Fiddle.ManaPercent >= HarassMenu["HmanaP"].Cast<Slider>().CurrentValue) || Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo) || Fiddle.IsUnderEnemyturret()) return;
            var harrasWtarget = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (harrasWtarget == null) return;
            var harrasWprophecy = HarrasWPred(harrasWtarget);
        }

        private static bool IsUnderEnemyturret()
        {
            throw new NotImplementedException();
        }

        private static object HarrasWPred(AIHeroClient harrasWtarget)
        {
            throw new NotImplementedException();
        }

        public static PredictionResult ComboWPred(Obj_AI_Base comW)
        {
            var comboconeW = new Geometry.Polygon.Sector(Fiddle.Position, Game.CursorPos, (float)(Math.PI / 180 * 40), 1250, 9).Points.ToArray();
            for (var x = 1; x < 10; x++)
            {
                var comboprophecyW = Prediction.Position.PredictLinearMissile(comW, 1250, 20, 250, 1500, 0, Fiddle.Position.Extend(comboconeW[x], 20).To3D());
                if (comboprophecyW.CollisionObjects.Any() || (comboprophecyW.HitChancePercent < ComboMenu["WHitChance"].Cast<Slider>().CurrentValue)) continue;
                return comboprophecyW;
            }
            return null;
        }
        private static void Combo()
        {
            if (Q.IsReady() && ComboMenu["Q"].Cast<CheckBox>().CurrentValue && EntityManager.Heroes.Enemies.Any(x => x.IsValidTarget(Fiddle.GetAutoAttackRange() - 50) && !IsPreAa))
            { Q.Cast(); }

            if (!W.IsReady() || !ComboMenu["E"].Cast<CheckBox>().CurrentValue) return;
            {
                var prophecyW = EntityManager.Heroes.Enemies.Where(x => { if (!x.IsValidTarget(W.Range)) return false; var wPred = ComboWPred(x); if (wPred == null) return false; return !SpellShield(x) && (wPred.HitChancePercent >= ComboMenu["WHitChance"].Cast<Slider>().CurrentValue); }).ToList();
                if (!prophecyW.Any() || IsPreAa) return;
                var targetW = TargetSelector.GetTarget(prophecyW, DamageType.Physical);
                if (targetW == null) return;
                var wPred2 = ComboWPred(targetW);
                if (wPred2 != null && wPred2.HitChancePercent >= ComboMenu["WHitChance"].Cast<Slider>().CurrentValue)
                { W.Cast(wPred2.CastPosition); }
            }
        }

        private static int GetAutoAttackRange()
        {
            throw new NotImplementedException();
        }

        private static void OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static bool Status_CheckBox(Menu sub, string str)
        {
            return sub[str].Cast<CheckBox>().CurrentValue;
        }

        public static int Status_Slider(Menu sub, string str)
        {
            return sub[str].Cast<Slider>().CurrentValue;
        }

        public static int Status_ComboBox(Menu sub, string str)
        {
            return sub[str].Cast<ComboBox>().CurrentValue;
        }

        public static bool Status_KeyBind(Menu sub, string str)
        {
            return sub[str].Cast<KeyBind>().CurrentValue;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (DrawMenu["Q"].Cast<CheckBox>().CurrentValue) { Circle.Draw(SharpDX.Color.Green, ComboMenu["Draw Q"].Cast<Slider>().CurrentValue, Caoujter); }
            if (DrawMenu["W"].Cast<CheckBox>().CurrentValue) { W.DrawRange(Color.FromArgb(130, Color.Green)); }
            if (DrawMenu["E"].Cast<CheckBox>().CurrentValue) { Circle.Draw(SharpDX.Color.Green, ComboMenu["Draw E"].Cast<Slider>().CurrentValue, Caoujter); }
            if (DrawMenu["R"].Cast<CheckBox>().CurrentValue) { R.DrawRange(Color.FromArgb(130, Color.Green)); }
        }
    }
}
  
 
    


