﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TheCassiopeia.Commons;
using TheCassiopeia.Commons.ComboSystem;
using TheCassiopeia.Commons.Debug;
using Color = System.Drawing.Color;

namespace TheCassiopeia
{
    class Cassiopeia
    {
        public void Load(EventArgs eArgs)
        {
            if (ObjectManager.Player.ChampionName != "Cassiopeia") return;

            //ItemSeraphsEmbrace
            var mainMenu = new Menu("The Cassiopeia", "TheCassiopeia", true);
            var orbwalkerMenu = mainMenu.CreateSubmenu("Orbwalker");
            var targetselectorMenu = mainMenu.CreateSubmenu("Target selector");
            var comboMenu = mainMenu.CreateSubmenu("Combo");
            var ultMenu = mainMenu.CreateSubmenu("Ultimate Settings");
            var harassMenu = mainMenu.CreateSubmenu("Harass");
            var laneclearMenu = mainMenu.CreateSubmenu("Laneclear");
            var lasthitMenu = mainMenu.CreateSubmenu("Lasthit");
            var burstmodeMenu = mainMenu.CreateSubmenu("Special mode: Burstmode");
            var lanepressureMenu = mainMenu.CreateSubmenu("Special mode: Lanepressure");

            var gapcloserMenu = mainMenu.CreateSubmenu("Gapcloser");
            // var interrupterMenu = mainMenu.CreateSubmenu("Interrupter");
            var manamanagerMenu = mainMenu.CreateSubmenu("Manamanager");
            var miscMenu = mainMenu.CreateSubmenu("Misc");
            var summonerMenu = mainMenu.CreateSubmenu("Summoners");
            var itemMenu = mainMenu.CreateSubmenu("Items");
            var drawingMenu = mainMenu.CreateSubmenu("Drawing");
            var autolevelMenu = mainMenu.CreateSubmenu("Auto level spells");
            var infoMenu = mainMenu.CreateSubmenu("Info");

            var orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            TargetSelector.AddToMenu(targetselectorMenu);

            var provider = new CassioCombo(1000, orbwalker, new CassQ(SpellSlot.Q), new CassW(SpellSlot.W), new CassE(SpellSlot.E), new CassR(SpellSlot.R));

            provider.CreateBasicMenu(comboMenu, harassMenu, laneclearMenu, gapcloserMenu, null, manamanagerMenu, summonerMenu, itemMenu, drawingMenu, false);
            provider.CreateAutoLevelMenu(autolevelMenu, ComboProvider.SpellOrder.RQEEW, ComboProvider.SpellOrder.REQW);

            ultMenu.AddMItem("(Will ult if one condition is met)");
            ultMenu.AddMItem("Min Enemies (facing)", new Slider(2, 1, HeroManager.Enemies.Count), (sender, args) => provider.GetSkill<CassR>().MinTargetsFacing = args.GetNewValue<Slider>().Value);
            ultMenu.AddMItem("Min Enemies (not facing)", new Slider(HeroManager.Enemies.Count - 1, 1, HeroManager.Enemies.Count), (sender, args) => provider.GetSkill<CassR>().MinTargetsNotFacing = args.GetNewValue<Slider>().Value);
            ultMenu.AddMItem("Do above only in combo", true, (sender, args) => provider.GetSkill<CassR>().MinEnemiesOnlyInCombo = args.GetNewValue<bool>());
            ultMenu.AddMItem("Ult if target killable with combo", true, (sender, args) => provider.GetSkill<CassR>().UltOnKillable = args.GetNewValue<bool>());
            ultMenu.AddMItem("Only ult if target has more health % than", new Slider(30), (sender, args) => provider.GetSkill<CassR>().MinHealth = args.GetNewValue<Slider>().Value);
            ultMenu.AddMItem("Block ults that wouldn't hit", false, (sender, args) => provider.BlockBadUlts = args.GetNewValue<bool>());
            ultMenu.AddMItem("Range", new Slider(700, 400, 825), (sender, args) => provider.GetSkill<CassR>().Range = args.GetNewValue<Slider>().Value);
            provider.AssistedUltMenu = ultMenu.AddMItem("Assisted Ult", new KeyBind(82, KeyBindType.Press));

            ultMenu.ProcStoredValueChanged<Slider>();
            ultMenu.ProcStoredValueChanged<bool>();

            harassMenu.AddMItem("Auto harass", false, (sender, args) => provider.GetSkill<CassQ>().AutoHarass = args.GetNewValue<bool>());
            harassMenu.AddMItem("Min mana %", new Slider(60), (sender, args) => provider.GetSkill<CassQ>().AutoHarassMana = args.GetNewValue<Slider>().Value);
            harassMenu.ProcStoredValueChanged<bool>();
            harassMenu.ProcStoredValueChanged<Slider>();


            burstmodeMenu.AddMItem("(Burstmode = going full ham, replaces combo when enabled)");
            var burstMode = provider.BurstMode = burstmodeMenu.AddMItem("Burst mode enabled", new KeyBind(78, KeyBindType.Toggle));
            provider.GetSkill<CassR>().BurstMode = burstMode;
            burstMode.Permashow(customdisplayname: "Burst mode");
            burstmodeMenu.AddMItem("Automatically go burst mode if my health % < ", new Slider(25), (sender, args) => provider.GetSkill<CassR>().PanicModeHealth = args.GetNewValue<Slider>().Value);
            burstmodeMenu.AddMItem("Use ignite in burst mode", false, (sender, args) => provider.IgniteInBurstMode = args.GetNewValue<bool>());
            burstmodeMenu.AddMItem("Ignite only when E on cooldown", false, (sender, args) => provider.OnlyIgniteWhenNoE = args.GetNewValue<bool>());
            burstmodeMenu.ProcStoredValueChanged<Slider>();
            burstmodeMenu.ProcStoredValueChanged<bool>();

            comboMenu.AddMItem("Only kill non-poisoned with E if no other enemies near", false, (sender, args) => provider.GetSkill<CassE>().OnlyKillNonPIn1V1 = args.GetNewValue<bool>());
            comboMenu.AddMItem("Fast combo (small chance to E non-poisoned)", true, (sender, args) => provider.GetSkill<CassQ>().FastCombo = args.GetNewValue<bool>());
            //comboMenu.AddMItem("Risky mode (uses fast combo often, but more fails)", false, (sender, args) => provider.GetSkill<CassQ>().RiskyCombo = args.GetNewValue<bool>());
            comboMenu.AddMItem("AA in combo (disable for better kiting!)", true, (sender, args) => provider.AutoInCombo = args.GetNewValue<bool>());
            comboMenu.ProcStoredValueChanged<bool>();

            var stackTearItem = miscMenu.AddMItem("Stack tear", new KeyBind(77, KeyBindType.Toggle, true));
            miscMenu.AddMItem("(Will only stack when no enemies near, ...)");
            provider.GetSkill<CassQ>().StackTear = stackTearItem;
            stackTearItem.Permashow();
            miscMenu.AddMItem("Min mana % for tear stack", new Slider(90), (sender, args) => provider.GetSkill<CassQ>().MinTearStackMana = args.GetNewValue<Slider>().Value);
            miscMenu.AddMItem("Make poison influence target selection", true, (sender, args) => provider.EnablePoisonTargetSelection = args.GetNewValue<bool>());
            miscMenu.ProcStoredValueChanged<Slider>();
            miscMenu.ProcStoredValueChanged<bool>();

            miscMenu.AddMItem("Enable this if you are Hawk", false, (sender, args) =>
            {
                ObjectManager.Player.SetSkin(args.GetNewValue<bool>() ? "Trundle" : "Cassiopeia", 0);
                Utility.DelayAction.Add(1000, () => Game.Say("/laugh"));
            });

            Circle q = new Circle(true, Color.GreenYellow), e = new Circle(false, Color.Red);

            drawingMenu.AddMItem("Q Range", q, (sender, args) => q = args.GetNewValue<Circle>());
            drawingMenu.AddMItem("E Range", e, (sender, args) => e = args.GetNewValue<Circle>());
            drawingMenu.ProcStoredValueChanged<Circle>();
            drawingMenu.ProcStoredValueChanged<bool>();

            gapcloserMenu.AddMItem("Ult if my HP % <", new Slider(40), (sender, args) => provider.GetSkill<CassR>().GapcloserUltHp = args.GetNewValue<Slider>().Value);
            gapcloserMenu.AddMItem("Otherwise use W", true, (sender, args) => provider.GetSkill<CassW>().UseOnGapcloser = args.GetNewValue<bool>());

            lasthitMenu.AddMItem("Use E on poisoned", true, (sender, args) => provider.GetSkill<CassE>().Farm = args.GetNewValue<bool>());
            //lasthitMenu.AddMItem("Lasthit assist", true, (sender, args) => provider.GetSkill<CassE>().FarmAssist = args.GetNewValue<bool>());
            lasthitMenu.AddMItem("Use E on non-poisoned if mana % <", new Slider(50), (sender, args) => provider.GetSkill<CassE>().FarmNonPoisonedPercent = args.GetNewValue<Slider>().Value);
            lasthitMenu.ProcStoredValueChanged<bool>();
            lasthitMenu.ProcStoredValueChanged<Slider>();


            var lanepressureEnabled = lanepressureMenu.AddMItem("Enabled", new KeyBind(84, KeyBindType.Toggle));
            provider.LanepressureMenu = lanepressureEnabled;
            provider.GetSkill<CassQ>().LanepressureMenu = lanepressureEnabled;
            lanepressureEnabled.Permashow(customdisplayname: "Lanepressure mode");
            lanepressureMenu.AddMItem("Override Laneclear when active");
            lanepressureMenu.AddMItem("It uses Harass + Lasthit while pushing with AA");
            lanepressureMenu.AddMItem("All Harass + Lasthit settings apply to it!");
            lanepressureMenu.AddMItem("Use Q on minions if E up", true, (sender, args) => provider.GetSkill<CassQ>().Farm = args.GetNewValue<bool>());
            lanepressureMenu.AddMItem("Only Q if mana % > ", new Slider(60), (sender, args) => provider.GetSkill<CassQ>().FarmIfHigherThan = args.GetNewValue<Slider>().Value);
            lanepressureMenu.AddMItem("Only Q if min. minions: ", new Slider(3, maxValue: 6), (sender, args) => provider.GetSkill<CassQ>().FarmIfMoreOrEqual = args.GetNewValue<Slider>().Value);


            infoMenu.AddMItem("TheCassiopeia - by TheNinow");
            infoMenu.AddMItem("Please give me feedback (on joduska.me) so I can improve this assembly!");
            infoMenu.AddMItem("Also, if you like this assembly, feel free to reward me with an upvote :)");

            mainMenu.AddToMainMenu();
            provider.Initialize();
            provider.GetSkill<CassQ>().GetPrediction(ObjectManager.Player); // Initializing the new prediction settings

            //DevAssistant.Init();

            Game.OnUpdate += (args) => provider.Update();

            Drawing.OnDraw += (args) =>
            {
                if (q.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 850, q.Color);
                if (e.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 700, e.Color);

                //Drawing.DrawText(200, 200, Color.Red, Game.CursorPos.ToString());
            };
        }



    }
}
