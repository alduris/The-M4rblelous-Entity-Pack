﻿global using static LBMergedMods.Hooks.PlayerHooks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using UnityEngine;

namespace LBMergedMods.Hooks;
//CHK
public static class PlayerHooks
{
    internal static void On_GourmandCombos_InitCraftingLibrary(On.MoreSlugcats.GourmandCombos.orig_InitCraftingLibrary orig)
    {
        orig();
        InitGourmandCombos();
    }

    internal static void On_Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (!PlayerData.TryGetValue(abstractCreature, out _))
            PlayerData.Add(abstractCreature, new() { OriginalBounce = self.bounce });
    }

    internal static bool On_Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
    {
        if (testObj is BouncingMelon or Physalis or SmallPuffBall && (!ModManager.MSC || self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear))
            return true;
        return orig(self, testObj);
    }

    internal static Player.ObjectGrabability On_Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is ThornyStrawberry or BlobPiece or BouncingMelon or LittleBalloon or TintedBeetle or Physalis or LimeMushroom or GummyAnther or MarineEye or SmallPuffBall or DendriticNeuron or MiniFruit)
            return Player.ObjectGrabability.OneHand;
        if (obj is RubberBlossom or MiniFruitSpawner)
            return Player.ObjectGrabability.CantGrab;
        if (obj is StarLemon)
            return Player.ObjectGrabability.TwoHands;
        return orig(self, obj);
    }

    internal static void On_Player_Grabbed(On.Player.orig_Grabbed orig, Player self, Creature.Grasp grasp)
    {
        orig(self, grasp);
        if (grasp.grabber is ThornBug or DivingBeetle or Caterpillar)
        {
            self.dangerGraspTime = 0;
            self.dangerGrasp = grasp;
        }
    }

    internal static void IL_Player_GrabUpdate(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchIsinst_IPlayerEdible))
            c.EmitDelegate((IPlayerEdible obj) => obj is BouncingMelon ? null : obj);
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchLdcI4_M1,
            s_MatchBeq_Any)
         && c.TryGotoNext(
            s_MatchCallOrCallvirt_Creature_get_grasps,
            s_MatchLdloc_InLoc1,
            s_MatchLdelemRef,
            s_MatchLdfld_Creature_Grasp_grabbed,
            s_MatchIsinst_KarmaFlower,
            s_MatchBrtrue_OutLabel))
        {
            var loc1 = il.Body.Variables[s_loc1];
            c.Emit(OpCodes.Ldloc, loc1)
             .EmitDelegate((Player self, int num6) => self.grasps[num6].grabbed is MarineEye or LimeMushroom);
            c.Emit(OpCodes.Brtrue, s_label)
             .Emit(OpCodes.Ldarg_0);
            if (c.TryGotoNext(MoveType.After,
                s_MatchCallOrCallvirt_Player_get_CanPutSpearToBack))
            {
                c.Emit(OpCodes.Ldloc, loc1)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, int num6, Player self) => flag || (num6 >= 0 && self.grasps[num6]?.grabbed is BouncingMelon));
            }
            else
                LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 3)");
            if (c.TryGotoNext(
                s_MatchCallOrCallvirt_Creature_get_grasps,
                s_MatchLdloc_InLoc1,
                s_MatchLdelemRef,
                s_MatchLdfld_Creature_Grasp_grabbed,
                s_MatchIsinst_KarmaFlower,
                s_MatchBrtrue_OutLabel))
            {
                c.Emit(OpCodes.Ldloc, loc1)
                 .EmitDelegate((Player self, int num6) => self.grasps[num6].grabbed is MarineEye or LimeMushroom);
                c.Emit(OpCodes.Brtrue, s_label)
                 .Emit(OpCodes.Ldarg_0);
                if (c.TryGotoNext(MoveType.After,
                    s_MatchLdfld_Player_objectInStomach))
                {
                    c.Emit(OpCodes.Ldarg_0)
                     .EmitDelegate((AbstractPhysicalObject objectInStomach, Player self) =>
                     {
                         var grs = self.grasps;
                         for (var i = 0; i < grs.Length; i++)
                         {
                             if (grs[i]?.grabbed is BouncingMelon mel)
                                 return null;
                         }
                         return objectInStomach;
                     });
                    if (c.TryGotoNext(MoveType.After,
                        s_MatchLdfld_Player_objectInStomach)
                     && c.TryGotoNext(MoveType.After,
                        s_MatchLdfld_Player_objectInStomach))
                    {
                        c.Emit(OpCodes.Ldarg_0)
                         .EmitDelegate((AbstractPhysicalObject objectInStomach, Player self) =>
                         {
                             var grs = self.grasps;
                             for (var i = 0; i < grs.Length; i++)
                             {
                                 if (grs[i]?.grabbed is BouncingMelon mel)
                                     return null;
                             }
                             return objectInStomach;
                         });
                    }
                    else
                        LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 6)");
                }
                else
                    LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 5)");
            }
            else
                LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 4)");
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.GrabUpdate! (part 2)");
    }

    internal static bool On_Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
    {
        var grs = self.grasps;
        for (var i = 0; i < grs.Length; i++)
        {
            if (grs[i]?.grabbed is ThornyStrawberry st && !st.SpikesRemoved())
                return false;
        }
        return orig(self);
    }

    internal static bool On_Player_IsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabCheck)
    {
        if (grabCheck is TintedBeetle or SurfaceSwimmer or BouncingBall)
            return true;
        return orig(self, grabCheck);
    }

    internal static void On_Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
    {
        if (grasp >= 0 && self.grasps[grasp]?.grabbed is BouncingMelon mel)
        {
            if (ModManager.MMF && self.room.game.session is StoryGameSession sess)
                sess.RemovePersistentTracker(mel.abstractPhysicalObject);
            self.mainBodyChunk.vel.y += 2f;
            self.room.PlaySound(SoundID.Slugcat_Swallow_Item, self.mainBodyChunk);
            self.room.PlaySound(SoundID.Slugcat_Eat_Dangle_Fruit, self.mainBodyChunk);
            self.ObjectEaten(mel);
            self.ReleaseGrasp(grasp);
            mel.Destroy();
            if (PlayerData.TryGetValue(self.abstractCreature, out var props))
                props.BounceEffectDuration = 5000;
        }
        else
            orig(self, grasp);
    }

    internal static void IL_Player_TerrainImpact(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_60))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float val, Player self) => PlayerData.TryGetValue(self.abstractCreature, out var props) ? val + props.BounceEffectDuration / 10f : val);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Player.TerrainImpact!");
    }

    internal static void On_Player_ThrowObject(On.Player.orig_ThrowObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp]?.grabbed is ThornyStrawberry st && st.SpikesRemoved())
        {
            self.AerobicIncrease(.75f);
            self.TossObject(grasp, eu);
            self.dontGrabStuff = self.isNPC ? 45 : 15;
            (self.graphicsModule as PlayerGraphics)?.LookAtObject(st);
            st.Forbid();
            self.ReleaseGrasp(grasp);
        }
        else
            orig(self, grasp, eu);
    }

    internal static void On_Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
    {
        if (self.grasps[grasp]?.grabbed is ThornyStrawberry st)
        {
            st.SetRandomSpin();
            st.rotationSpeed = 0f;
        }
        orig(self, grasp, eu);
    }

    internal static void On_Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        var flag = PlayerData.TryGetValue(self.abstractCreature, out var props);
        var chunks = self.bodyChunks;
        if (flag)
        {
            var effect = props.BounceEffectDuration / 5000f;
            if (self.bodyMode != Player.BodyModeIndex.Crawl
                && self.bodyMode != Player.BodyModeIndex.CorridorClimb
                && self.animation != Player.AnimationIndex.CrawlTurn
                && self.animation != Player.AnimationIndex.DownOnFours
                && self.animation != Player.AnimationIndex.LedgeCrawl
                && self.animation != Player.AnimationIndex.LedgeGrab
                && self.animation != Player.AnimationIndex.CorridorTurn
                && self.animation != Player.AnimationIndex.BellySlide)
            {
                if (effect > 0f)
                {
                    for (var i = 0; i < chunks.Length; i++)
                    {
                        var chunk = chunks[i];
                        var contact = chunk.ContactPoint;
                        if (contact.y != 0 && Math.Abs(chunk.vel.y) < 1f)
                            chunk.vel.y = 0f;
                    }
                }
                self.bounce = props.OriginalBounce * (1f + 12f * effect);
            }
            else
                self.bounce = props.OriginalBounce;
            if (!self.inShortcut)
            {
                if (props.BounceEffectDuration > 0)
                    --props.BounceEffectDuration;
                if (props.BlueFaceDuration > 0)
                    --props.BlueFaceDuration;
            }
        }
        orig(self, eu);
        if (flag && props.BounceEffectDuration > 0)
        {
            var velFac = .9f + .1f * (1f - props.BounceEffectDuration / 5000f);
            for (var i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var contact = chunk.ContactPoint;
                if (contact != default)
                    chunk.vel *= velFac;
            }
        }
    }

    internal static void On_PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!rCam.room.game.DEBUGMODE && self.player is Player p && PlayerData.TryGetValue(p.abstractCreature, out var props))
        {
            var num = .5f + .5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * Mathf.PI * 2f);
            Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker),
                vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
            if (p.aerobicLevel > .5f)
                vector += Custom.DirVec(vector2, vector) * Mathf.Lerp(-1f, 1f, num) * Mathf.InverseLerp(.5f, 1f, p.aerobicLevel) * .5f;
            var num2 = Mathf.InverseLerp(.3f, .5f, Math.Abs(Custom.DirVec(vector2, vector).y));
            var ef = props.BounceEffectDuration / 6500f;
            //inverted for pups somehow, bug in msc code?
            FSprite spr;
            if (ModManager.MSC && p.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
            {
                spr = sLeaser.sprites[1];
                spr.scaleX = spr.scaleX * (1f + .5f * ef) - Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-.05f, -.15f, self.malnourished), .05f, num) * num2, .15f, p.sleepCurlUp) * .5f * ef;
                spr = sLeaser.sprites[0];
                spr.scaleX = spr.scaleX * (1f + .5f * ef) - (p.sleepCurlUp * .1f + .025f * num - .025f * self.malnourished) * ef;
            }
            else
            {
                spr = sLeaser.sprites[0];
                spr.scaleX = spr.scaleX * (1f + .5f * ef) - Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-.05f, -.15f, self.malnourished), .05f, num) * num2, .15f, p.sleepCurlUp) * .5f * ef;
                spr = sLeaser.sprites[1];
                spr.scaleX = spr.scaleX * (1f + .5f * ef) - (p.sleepCurlUp * .1f + .025f * num - .025f * self.malnourished) * ef;
            }
            spr = sLeaser.sprites[9];
            spr.color = Color.Lerp(spr.color, MarineCol, props.BlueFaceDuration / 5000f);
        }
    }

    internal static void IL_PlayerGraphics_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdfld_Player_swallowAndRegurgitateCounter)
         && c.TryGotoNext(MoveType.After,
            s_MatchLdfld_Player_swallowAndRegurgitateCounter)
         && c.TryGotoNext(MoveType.After,
            s_MatchLdfld_Player_swallowAndRegurgitateCounter))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((int swallowAndRegurgitateCounter, PlayerGraphics self) =>
             {
                 var grs = self.player.grasps;
                 for (var i = 0; i < grs.Length; i++)
                 {
                     if (grs[i]?.grabbed is BouncingMelon mel)
                         return 0;
                 }
                 return swallowAndRegurgitateCounter;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook PlayerGraphics.Update!");
    }

    internal static void On_PlayerSessionRecord_AddEat(On.PlayerSessionRecord.orig_AddEat orig, PlayerSessionRecord self, PhysicalObject eatenObject)
    {
        orig(self, eatenObject);
        if (eatenObject is BlobPiece)
        {
            self.vegetarian = false;
            self.carnivorous = true;
        }
    }

    internal static void IL_SlugcatHand_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdfld_Player_objectInStomach))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractPhysicalObject objectInStomach, SlugcatHand self) =>
             {
                 var grs = (self.owner.owner as Player)!.grasps;
                 for (var i = 0; i < grs.Length; i++)
                 {
                     if (grs[i]?.grabbed is BouncingMelon mel)
                         return null;
                 }
                 return objectInStomach;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SlugcatHand.Update!");
    }

    internal static int On_SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject)
    {
        var res = orig(slugcatIndex, eatenobject);
        if (eatenobject is Fly b && b.IsSeed() && slugcatIndex != SlugcatStats.Name.Red && (!ModManager.MSC || (slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Spear && slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Saint && slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Artificer)))
            res += 2;
        else if (eatenobject is BouncingMelon or GummyAnther or DendriticNeuron or MiniFruit && slugcatIndex != SlugcatStats.Name.Red && (!ModManager.MSC || (slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Spear && slugcatIndex != MoreSlugcatsEnums.SlugcatStatsName.Artificer)))
            res += 2;
        return res;
    }

    internal static void InitGourmandCombos()
    {
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 0, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 0, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], 0, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], 0, DLCSharedEnums.AbstractObjectType.Seed, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], 0, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], 0, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], 0, null, CreatureTemplate.Type.Fly);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], 0, DLCSharedEnums.AbstractObjectType.LillyPuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.SingularityBomb], 0, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.Seed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GooieDuck], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.LillyPuck], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GlowWeed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.DandelionPeach], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Fly], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallCentipede], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.VultureGrub], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Hazer], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, AbstractPhysicalObject.AbstractObjectType.JellyFish, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, AbstractPhysicalObject.AbstractObjectType.JellyFish, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], 0, DLCSharedEnums.AbstractObjectType.GlowWeed, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], 0, DLCSharedEnums.AbstractObjectType.Seed, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], 0, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], 0, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], 0, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], 0, null, CreatureTemplate.Type.Snail);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], 0, null, CreatureTemplate.Type.Hazer);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], 0, null, CreatureTemplate.Type.Hazer);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], 0, DLCSharedEnums.AbstractObjectType.LillyPuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.SingularityBomb], 0, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.Seed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GooieDuck], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.LillyPuck], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GlowWeed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.DandelionPeach], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Fly], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallCentipede], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.VultureGrub], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Hazer], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], 0, null, CreatureTemplate.Type.SmallCentipede);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], 0, null, CreatureTemplate.Type.SmallNeedleWorm);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], 0, null, CreatureTemplate.Type.Fly);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], 0, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], 0, null, CreatureTemplate.Type.SmallCentipede);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], 0, null, CreatureTemplate.Type.Hazer);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.SingularityBomb], 0, null, CreatureTemplate.Type.SmallCentipede);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], 0, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.Seed], 0, null, CreatureTemplate.Type.SmallCentipede);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GooieDuck], 0, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.LillyPuck], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GlowWeed], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.DandelionPeach], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Fly], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 1, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallCentipede], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 1, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.VultureGrub], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 1, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 1, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Hazer], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 1, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 0, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 0, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], 0, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], 0, DLCSharedEnums.AbstractObjectType.Seed, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], 0, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], 0, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], 0, null, CreatureTemplate.Type.Fly);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], 0, DLCSharedEnums.AbstractObjectType.LillyPuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.SingularityBomb], 0, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.Seed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GooieDuck], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.LillyPuck], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GlowWeed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.DandelionPeach], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Fly], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallCentipede], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.VultureGrub], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Hazer], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], 0, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], 0, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], 0, AbstractPhysicalObject.AbstractObjectType.NeedleEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], 0, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], 0, null, CreatureTemplate.Type.SmallCentipede);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.SingularityBomb], 0, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], 0, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.Seed], 0, null, CreatureTemplate.Type.SmallCentipede);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GooieDuck], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.LillyPuck], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GlowWeed], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.DandelionPeach], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Fly], GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], 1, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallCentipede], GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], 1, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.VultureGrub], GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], 1, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], 1, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Hazer], GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], 1, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 0, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 0, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], 0, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], 0, DLCSharedEnums.AbstractObjectType.Seed, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], 0, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], 0, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], 0, null, CreatureTemplate.Type.Fly);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], 0, DLCSharedEnums.AbstractObjectType.LillyPuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.SingularityBomb], 0, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.Seed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GooieDuck], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.LillyPuck], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GlowWeed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.DandelionPeach], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Fly], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallCentipede], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.VultureGrub], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Hazer], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 0, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 0, AbstractPhysicalObject.AbstractObjectType.SlimeMold, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], 0, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], 0, DLCSharedEnums.AbstractObjectType.Seed, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], 0, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], 0, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], 0, null, CreatureTemplate.Type.Fly);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], 0, DLCSharedEnums.AbstractObjectType.LillyPuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.SingularityBomb], 0, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.Seed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GooieDuck], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.LillyPuck], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GlowWeed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.DandelionPeach], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Fly], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallCentipede], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.VultureGrub], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Hazer], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 0, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 0, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], 0, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], 0, AbstractObjectType.MarineEye, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], 0, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], 0, AbstractObjectType.MarineEye, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], 0, null, CreatureTemplate.Type.SmallCentipede);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], 0, AbstractObjectType.MarineEye, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.SingularityBomb], 0, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], 0, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.Seed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GooieDuck], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.LillyPuck], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GlowWeed], 0, AbstractObjectType.MarineEye, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.DandelionPeach], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Fly], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallCentipede], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 1, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.VultureGrub], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 1, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 1, AbstractObjectType.MarineEye, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Hazer], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 1, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], 0, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], 0, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], 0, AbstractPhysicalObject.AbstractObjectType.OverseerCarcass, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], 0, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], 0, AbstractObjectType.MarineEye, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], 0, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], 0, AbstractObjectType.MarineEye, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], 0, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], 0, null, CreatureTemplate.Type.SmallCentipede);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], 0, AbstractObjectType.MarineEye, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], 0, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.SingularityBomb], 0, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], 0, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.Seed], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GooieDuck], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.LillyPuck], 0, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GlowWeed], 0, AbstractObjectType.MarineEye, null);
        GourmandCombos.SetLibraryData(GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.DandelionPeach], 0, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Fly], GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallCentipede], GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], 1, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.VultureGrub], GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], 1, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallNeedleWorm], GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], 1, AbstractObjectType.MarineEye, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Hazer], GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], 1, AbstractPhysicalObject.AbstractObjectType.BubbleGrass, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Rock], 1, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlareBomb], 1, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.VultureMask], 1, DLCSharedEnums.AbstractObjectType.Seed, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.PuffBall], 1, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DangleFruit], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.DataPearl], 1, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.WaterNut], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.JellyFish], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Lantern], 1, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.KarmaFlower], 1, AbstractPhysicalObject.AbstractObjectType.PuffBall, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.Mushroom], 1, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant], 1, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SlimeMold], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.FlyLure], 1, AbstractPhysicalObject.AbstractObjectType.Mushroom, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.ScavengerBomb], 1, AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.SporePlant], 1, null, CreatureTemplate.Type.TubeWorm);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.EggBugEgg], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.NeedleEgg], 1, DLCSharedEnums.AbstractObjectType.Seed, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.BubbleGrass], 1, AbstractPhysicalObject.AbstractObjectType.JellyFish, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractPhysicalObject.AbstractObjectType.OverseerCarcass], 1, AbstractPhysicalObject.AbstractObjectType.DataPearl, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.SingularityBomb], 1, MoreSlugcatsEnums.AbstractObjectType.FireEgg, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[MoreSlugcatsEnums.AbstractObjectType.FireEgg], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.Seed], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GooieDuck], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.LillyPuck], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.GlowWeed], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[DLCSharedEnums.AbstractObjectType.DandelionPeach], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.Fly], GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], 2, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplate.Type.VultureGrub], GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], 2, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.critsLibrary[CreatureTemplate.Type.SmallCentipede], 2, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], 2, null, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile], 1, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom], 1, DLCSharedEnums.AbstractObjectType.GooieDuck, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractObjectType.Physalis], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther], 1, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon], 1, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
        GourmandCombos.SetLibraryData(GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera], GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit], 1, AbstractPhysicalObject.AbstractObjectType.SporePlant, null);
    }

    internal static void ResizeGourmandCombos()
    {
        _ = GourmandCombos.craftingGrid_CritterObjects;
        var cnt = GourmandCombos.objectsLibrary.Count;
        GourmandCombos.objectsLibrary[AbstractObjectType.ThornyStrawberry] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.BlobPiece] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.LimeMushroom] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.MarineEye] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.SporeProjectile] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.Physalis] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.GummyAnther] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.LittleBalloon] = cnt;
        ++cnt;
        GourmandCombos.objectsLibrary[AbstractObjectType.MiniBlueFruit] = cnt;
        ++cnt;
        var arrayOrig = GourmandCombos.craftingGrid_ObjectsOnly;
        var arrayNew = new GourmandCombos.CraftDat[cnt, cnt];
        int l0 = arrayOrig.GetLength(0), l1 = arrayOrig.GetLength(1), i, j;
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                arrayNew[i, j] = arrayOrig[i, j];
        }
        GourmandCombos.craftingGrid_ObjectsOnly = arrayNew;
        var cnt2 = GourmandCombos.critsLibrary.Count;
        GourmandCombos.critsLibrary[CreatureTemplateType.MiniScutigera] = cnt2;
        ++cnt2;
        arrayOrig = GourmandCombos.craftingGrid_CrittersOnly;
        arrayNew = new GourmandCombos.CraftDat[cnt2, cnt2];
        l0 = arrayOrig.GetLength(0);
        l1 = arrayOrig.GetLength(1);
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                arrayNew[i, j] = arrayOrig[i, j];
        }
        GourmandCombos.craftingGrid_CrittersOnly = arrayNew;
        arrayOrig = GourmandCombos.craftingGrid_CritterObjects;
        l0 = arrayOrig.GetLength(0);
        l1 = arrayOrig.GetLength(1);
        arrayNew = new GourmandCombos.CraftDat[cnt2, cnt];
        for (i = 0; i < l0; i++)
        {
            for (j = 0; j < l1; j++)
                arrayNew[i, j] = arrayOrig[i, j];
        }
        GourmandCombos.craftingGrid_CritterObjects = arrayNew;
    }
}