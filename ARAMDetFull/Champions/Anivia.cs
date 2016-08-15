﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DetuksSharp;
using LeagueSharp;using DetuksSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ARAMDetFull.Champions
{
    class Anivia : Champion
    {

        public static int FarmId;

        public static GameObject QMissile;
        public static GameObject RMissile;

        public Anivia()
        {
            ARAMSimulator.champBuild = new Build
            {
                coreItems = new List<ConditionalItem>
                {
                    new ConditionalItem(ItemId.Rod_of_Ages),
                    new ConditionalItem(ItemId.Item Archangels_Staff),
                    new ConditionalItem(ItemId.Sorcerers_Shoes),
                    new ConditionalItem(ItemId.Rabadons_Deathcap),
                    new ConditionalItem(ItemId.Void_Staff, ItemId.Liandrys_Torment, ENEMY_MR),
                    new ConditionalItem(ItemId.Zhonyas_Hourglass),
                },
                startingItems = new List<ItemId>
                {
                    ItemId.Tear_of_the_Goddess
                }
            };
            Obj_AI_Base.OnDelete += Obj_AI_Base_OnDelete;
            Obj_AI_Base.OnCreate += Obj_AI_Base_OnCreate;
            DeathWalker.BeforeAttack += Orbwalking_BeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnInterruptableSpell;
        }

        private void OnInterruptableSpell(Obj_AI_Hero unit, InterruptableSpell spell)
        {
            if (W.IsReady() && unit.IsValidTarget(W.Range))
                W.Cast(unit);
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var Target = (Obj_AI_Hero)gapcloser.Sender;
            if (Q.IsReady())
            {
                if (Target.IsValidTarget(Q.Range))
                {
                    Q.Cast(Target);
                }
            }
            else if (W.IsReady())
            {
                if (Target.IsValidTarget(W.Range))
                {
                    W.Cast(Target);
                }
            }
        }

        private void Orbwalking_BeforeAttack(DeathWalker.BeforeAttackEventArgs args)
        {
            if (FarmId != args.Target.NetworkId)
                FarmId = args.Target.NetworkId;
        }

        private void Obj_AI_Base_OnCreate(GameObject obj, EventArgs args)
        {
            if (obj.IsValid)
            {
                if (obj.Name == "cryo_FlashFrost_Player_mis.troy")
                    QMissile = obj;
                if (obj.Name.Contains("cryo_storm"))
                    RMissile = obj;
            }
        }

        private void Obj_AI_Base_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.IsValid)
            {
                if (obj.Name == "cryo_FlashFrost_Player_mis.troy")
                    QMissile = null;
                if (obj.Name.Contains("cryo_storm"))
                    RMissile = null;
            }
        }


        public override void useQ(Obj_AI_Base target)
        {

        }

        public override void useW(Obj_AI_Base target)
        {

        }

        public override void useE(Obj_AI_Base target)
        {
        }


        public override void useR(Obj_AI_Base target)
        {
            if (!R.IsReady() || target == null)
                return;
            if (target.HealthPercent < 35)
                R.CastOnUnit(target);
        }

        public override void useSpells()
        {
            if (Combo)
            {
                DeathWalker.setAttack(!E.IsReady());
            }
            else
                DeathWalker.setAttack(true);

            if (R.IsReady())
            {
                var t = TargetSelector.GetTarget(R.Range + 400, TargetSelector.DamageType.Physical);
                if (RMissile == null && t.IsValidTarget())
                {
                    
                        R.Cast(t, true, true);
                }

                var allMinionsQ = MinionManager.GetMinions(player.ServerPosition, R.Range + 400, MinionTypes.All);
                var Rfarm = R.GetCircularFarmLocation(allMinionsQ, R.Width);

                if (RMissile == null
                    && Farm
                    && Rfarm.MinionsHit > 2)
                {
                    R.Cast(Rfarm.Position);
                }

                if (Combo && RMissile != null && (RMissile.Position.GetAliveEnemiesInRange(450) == 0))
                {
                    R.Cast();
                }
                else if (RMissile != null && Farm && (Rfarm.MinionsHit < 3 || Rfarm.Position.Distance(RMissile.Position) > 400))
                {
                    R.Cast();
                }
                if (!Combo && !Farm && RMissile != null)
                    R.Cast();
            }
            if (W.IsReady())
            {
                var ta = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                if (Combo && ta.IsValidTarget(W.Range) && ta.Path.Count() == 1 && W.GetPrediction(ta).CastPosition.Distance(ta.Position) > 150)
                {
                    if (player.Position.Distance(ta.ServerPosition) > player.Position.Distance(ta.Position))
                    {
                        if (ta.Position.Distance(player.ServerPosition) < ta.Position.Distance(player.Position) && ta.IsValidTarget(W.Range - 200))
                            CastSpell(W, ta, 3);
                    }
                    else
                    {
                        if (ta.Position.Distance(player.ServerPosition) > ta.Position.Distance(player.Position) && ta.IsValidTarget(E.Range) && ta.HasBuffOfType(BuffType.Slow))
                            CastSpell(W, ta, 3);
                    }
                }
            }
            if (Q.IsReady() && QMissile == null)
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    var qDmg = Q.GetDamage(t);

                    if (qDmg > t.Health)
                        Q.Cast(t, true);
                    else if (Combo)
                        CastSpell(Q, t, 3);
                    else if (Farm && !player.UnderTurret(true))
                    {
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(Q.Range)))
                        {
                            CastSpell(Q, enemy, 3);
                        }
                    }

                    else
                    {
                        foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(Q.Range)))
                        {
                            if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                             enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                             enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Slow) || enemy.HasBuff("Recall"))
                            {
                                Q.Cast(enemy, true);
                            }
                        }
                    }
                }
            }

            if (E.IsReady())
            {
                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {

                    var qCd = Q.Instance.CooldownExpires - Game.Time;
                    var rCd = R.Instance.CooldownExpires - Game.Time;
                    if (player.Level < 7)
                        rCd = 10;
                    //debug("Q " + qCd + "R " + rCd + "E now " + E.Instance.Cooldown);
                    var eDmg = E.GetDamage(t);
                    if (t.HasBuff("chilled"))
                    {
                        eDmg = 2 * eDmg;
                    }
                    if (eDmg > t.Health)
                        E.Cast(t, true);
                    else if ((t.HasBuff("chilled")) && Combo && QMissile == null)
                    {
                        if (RMissile == null && R.IsReady())
                            R.Cast(t, true, true);
                        E.Cast(t, true);
                    }
                    else if (t.HasBuff("chilled") && Farm && !player.UnderTurret(true) && QMissile == null)
                    {
                        if (RMissile == null && R.IsReady())
                            R.Cast(t, true, true);
                        E.Cast(t, true);
                    }
                    else if (t.HasBuff("chilled") && Combo)
                    {
                        E.Cast(t, true);
                    }
                }
                farmE();
            }
            if (Q.IsReady() && QMissile != null)
            {
                if (QMissile.Position.GetAliveEnemiesInRange(220) > 0)
                    Q.Cast();
            }
        }

        public void farmE()
        {
            if (Farm && !DeathWalker.canAttack() )
            {

                var mobs = MinionManager.GetMinions(player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0)
                {
                    var mob = mobs[0];
                    E.Cast(mob, true);
                    return;
                }

                var minions = MinionManager.GetMinions(player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);
                foreach (var minion in minions.Where(minion => minion.Health > player.GetAutoAttackDamage(minion) && FarmId != minion.NetworkId))
                {
                    var eDmg = E.GetDamage(minion);
                    if (minion.HasBuff("chilled"))
                        eDmg = 2 * eDmg;

                    if (minion.Health < eDmg * 0.9)
                        E.Cast(minion);
                }
            }
        }

        private void CastSpell(Spell QWER, Obj_AI_Hero target, int HitChanceNum)
        {
            //HitChance 0 - 2
            // example CastSpell(Q, ts, 2);
            var poutput = QWER.GetPrediction(target);
            var col = poutput.CollisionObjects.Count(ColObj => ColObj.IsEnemy && ColObj.IsMinion && !ColObj.IsDead);
            if (target.IsDead || col > 0 || target.Path.Count() > 1)
                return;

            if ((target.Path.Count() == 0 && target.Position == target.ServerPosition) || target.HasBuff("Recall"))
            {
                QWER.Cast(poutput.CastPosition);
                return;
            }

            if (HitChanceNum == 0)
                QWER.Cast(target, true);
            else if (HitChanceNum == 1)
            {
                if ((int)poutput.Hitchance > 4)
                    QWER.Cast(poutput.CastPosition);
            }
            else if (HitChanceNum == 2)
            {
                List<Vector2> waypoints = target.GetWaypoints();
                if (waypoints.Last<Vector2>().To3D().Distance(poutput.CastPosition) > QWER.Width && (int)poutput.Hitchance == 5)
                {
                    if (waypoints.Last<Vector2>().To3D().Distance(player.Position) <= target.Distance(player.Position) || (target.Path.Count() == 0 && target.Position == target.ServerPosition))
                    {
                        if (player.Distance(target.ServerPosition) < QWER.Range - (poutput.CastPosition.Distance(target.ServerPosition) + target.BoundingRadius))
                        {
                            QWER.Cast(poutput.CastPosition);
                        }
                    }
                    else if ((int)poutput.Hitchance == 5)
                    {
                        QWER.Cast(poutput.CastPosition);
                    }
                }
            }
            else if (HitChanceNum == 3)
            {
                List<Vector2> waypoints = target.GetWaypoints();
                float SiteToSite = ((target.MoveSpeed * QWER.Delay) + (player.Distance(target.ServerPosition) / QWER.Speed) - QWER.Width) * 6;
                float BackToFront = ((target.MoveSpeed * QWER.Delay) + (player.Distance(target.ServerPosition) / QWER.Speed));
                if (player.Distance(waypoints.Last<Vector2>().To3D()) < SiteToSite || player.Distance(target.Position) < SiteToSite)
                    QWER.CastIfHitchanceEquals(target, HitChance.High, true);
                else if ((target.ServerPosition.Distance(waypoints.Last<Vector2>().To3D()) > SiteToSite
                    || Math.Abs(player.Distance(waypoints.Last<Vector2>().To3D()) - player.Distance(target.Position)) > BackToFront))
                {
                    if (waypoints.Last<Vector2>().To3D().Distance(player.Position) <= target.Distance(player.Position))
                    {
                        if (player.Distance(target.ServerPosition) < QWER.Range - (poutput.CastPosition.Distance(target.ServerPosition)))
                        {
                            QWER.Cast(poutput.CastPosition);
                        }
                    }
                    else
                    {
                        QWER.Cast(poutput.CastPosition);
                    }
                }
            }
            else if (HitChanceNum == 4 && (int)poutput.Hitchance > 4)
            {
                List<Vector2> waypoints = target.GetWaypoints();
                float SiteToSite = ((target.MoveSpeed * QWER.Delay) + (player.Distance(target.ServerPosition) / QWER.Speed) - QWER.Width) * 6;
                float BackToFront = ((target.MoveSpeed * QWER.Delay) + (player.Distance(target.ServerPosition) / QWER.Speed));

                if (player.Distance(waypoints.Last<Vector2>().To3D()) < SiteToSite || player.Distance(target.Position) < SiteToSite)
                    QWER.CastIfHitchanceEquals(target, HitChance.High, true);
                else if ((target.ServerPosition.Distance(waypoints.Last<Vector2>().To3D()) > SiteToSite
                    || Math.Abs(player.Distance(waypoints.Last<Vector2>().To3D()) - player.Distance(target.Position)) > BackToFront))
                {
                    if (waypoints.Last<Vector2>().To3D().Distance(player.Position) <= target.Distance(player.Position))
                    {
                        if (player.Distance(target.ServerPosition) < QWER.Range - (poutput.CastPosition.Distance(target.ServerPosition)))
                        {
                            QWER.Cast(poutput.CastPosition);
                        }
                    }
                    else
                    {
                        QWER.Cast(poutput.CastPosition);
                    }
                }
            }
        }


        public override void setUpSpells()
        {
            //Create the spells
            Q = new Spell(SpellSlot.Q, 1150);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 650);

            Q.SetSkillshot(.25f, 110f, 850f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(.6f, 1f, float.MaxValue, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(2f, 400f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private static bool Combo
        {
            get { return player.Position.GetAliveEnemiesInRange(1300) != 0; }
        }
        private static bool Farm
        {
            get { return !Combo && player.ManaPercent >79; }
        }
    }
}
