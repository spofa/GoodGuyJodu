﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;using DetuksSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ARAMDetFull.Champions
{
    class Talon : Champion
    {


        public Talon()
        {
            //Interrupter.OnPossibleToInterrupt += InterrupterOnOnPossibleToInterrupt;

            ARAMSimulator.champBuild = new Build
            {
                coreItems = new List<ConditionalItem>
                        {
                            new ConditionalItem(ItemId.Youmuus_Ghostblade),
                            new ConditionalItem(ItemId.Boots_of_Mobility),
                            new ConditionalItem(ItemId.Ravenous_Hydra_Melee_Only),
                            new ConditionalItem(ItemId.The_Black_Cleaver),
                            new ConditionalItem(ItemId.Maw_of_Malmortius),
                            new ConditionalItem(ItemId.Lord_Dominiks_Regards),
                        },
                startingItems = new List<ItemId>
                        {
                            ItemId.Vampiric_Scepter,ItemId.Boots_of_Speed
                        }
            };
        }

        public override void useQ(Obj_AI_Base target)
        {
            if (!Q.IsReady())
                return;

            Q.Cast();
        }

        public override void useW(Obj_AI_Base target)
        {
            if (!W.IsReady())
                return;
            W.Cast(target);
        }

        public override void useE(Obj_AI_Base target)
        {
            if (!E.IsReady())
                return;
            if (!Sector.inTowerRange(target.Position.To2D()) &&
                (MapControl.balanceAroundPoint(target.Position.To2D(), 700) >= -1 ||
                 (MapControl.fightIsOn() != null && MapControl.fightIsOn().NetworkId == target.NetworkId)))
                E.Cast(target);
        }

        public override void useR(Obj_AI_Base target)
        {
            if (!R.IsReady())
                return;
            if (player.CountEnemiesInRange(400) > 1)
                R.Cast();
        }

        public override void setUpSpells()
        {
            Q = new Spell(SpellSlot.Q, 200);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 0);

            W.SetSkillshot(0.0f, 400f, 1700f, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.0f, 400f, 1700f, false, SkillshotType.SkillshotCircle);
        }

        public override void useSpells()
        {
            var tar = ARAMTargetSelector.getBestTarget(Q.Range);
            if (tar != null) useQ(tar);
            tar = ARAMTargetSelector.getBestTarget(W.Range);
            if (tar != null) useW(tar);
            tar = ARAMTargetSelector.getBestTarget(E.Range);
            if (tar != null) useE(tar);
            tar = ARAMTargetSelector.getBestTarget(R.Range);
            if (tar != null) useR(tar);
        }
    }
}
