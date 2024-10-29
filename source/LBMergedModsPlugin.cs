﻿global using LBMergedMods.Items;
global using LBMergedMods.Creatures;
using BepInEx;
using System.Security.Permissions;
using System.Security;
using UnityEngine;
using BepInEx.Logging;
using Fisobs.Core;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MonoMod.RuntimeDetour;
using Fisobs.Sandbox;

#pragma warning disable CS0618 // ignore false message
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace LBMergedMods;

[BepInPlugin("lb-fgf-m4r-ik.modpack", "LB Merged Mods", "1.0.8"), BepInDependency("io.github.dual.fisobs")]
public sealed class LBMergedModsPlugin : BaseUnityPlugin
{
    public static AssetBundle? Bundle;
    public const BindingFlags ALL_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
    [AllowNull] internal static ManualLogSource s_logger;

    public void OnEnable()
    {
        s_logger = Logger;
        On.RainWorld.OnModsInit += On_RainWorld_OnModsInit;
        On.RainWorld.UnloadResources += On_RainWorld_UnloadResources;
        On.RainWorld.PostModsInit += On_RainWorld_PostModsInit;
        On.RainWorld.OnModsDisabled += On_RainWorld_OnModsDisabled;
        On.SoundLoader.LoadSounds += On_SoundLoader_LoadSounds;
        On.Player.ctor += On_Player_ctor;
        On.Player.CanBeSwallowed += On_Player_CanBeSwallowed;
        On.Player.Update += On_Player_Update;
        IL.Player.TerrainImpact += IL_Player_TerrainImpact;
        On.Player.SwallowObject += On_Player_SwallowObject;
        IL.Player.GrabUpdate += IL_Player_GrabUpdate;
        IL.PlayerGraphics.Update += IL_PlayerGraphics_Update;
        //On.PlayerGraphics.DrawSprites += On_PlayerGraphics_DrawSprites; it should be applied later
        IL.SlugcatHand.Update += IL_SlugcatHand_Update;
        On.ArenaBehaviors.SandboxEditor.GetPerformanceEstimate += On_SandboxEditor_GetPerformanceEstimate;
        On.VultureGrub.RayTraceSky += On_VultureGrub_RayTraceSky;
        IL.BigEel.ctor += IL_BigEel_ctor;
        IL.BigEel.AccessSwimSpace += IL_BigEel_AccessSwimSpace;
        IL.BigEel.Swim += IL_BigEel_Swim;
        IL.BigEel.JawsSnap += IL_BigEel_JawsSnap;
        IL.BigEelPather.FollowPath += IL_BigEelPather_FollowPath;
        IL.BigEelGraphics.ctor += IL_BigEelGraphics_ctor;
        IL.BigEelGraphics.Update += IL_BigEelGraphics_Update;
        On.BigEelGraphics.Reset += On_BigEelGraphics_Reset;
        On.BigEelGraphics.ApplyPalette += On_BigEelGraphics_ApplyPalette;
        IL.BigEelAbstractAI.AbstractBehavior += IL_BigEelAbstractAI_AbstractBehavior;
        IL.BigEelAbstractAI.AddRandomCheckRoom += IL_BigEelAbstractAI_AddRandomCheckRoom;
        IL.BigEelAI.IUseARelationshipTracker_UpdateDynamicRelationship += IL_BigEelAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        IL.BigEelAI.Update += IL_BigEelAI_Update;
        IL.GarbageWormAI.Update += IL_GarbageWormAI_Update;
        On.PathFinder.CoordinateReachableAndGetbackable += On_PathFinder_CoordinateReachableAndGetbackable;
        On.LizardCosmetics.BumpHawk.DrawSprites += On_BumpHawk_DrawSprites;
        On.LizardCosmetics.JumpRings.DrawSprites += On_JumpRings_DrawSprites;
        On.LizardCosmetics.LongHeadScales.DrawSprites += On_LongHeadScales_DrawSprites;
        On.LizardCosmetics.LongBodyScales.DrawSprites += On_LongBodyScales_DrawSprites;
        On.LizardGraphics.GenerateIvars += On_LizardGraphics_GenerateIvars;
        new Hook(typeof(LizardGraphics).GetMethod("get_HeadColor1", ALL_FLAGS), On_LizardGraphics_get_HeadColor1);
        new Hook(typeof(LizardGraphics).GetMethod("get_HeadColor2", ALL_FLAGS), On_LizardGraphics_get_HeadColor2);
        On.LizardGraphics.ApplyPalette += On_LizardGraphics_ApplyPalette;
        On.LizardGraphics.BodyColor += On_LizardGraphics_BodyColor;
        On.LizardCosmetics.SpineSpikes.DrawSprites += On_SpineSpikes_DrawSprites;
        On.LizardCosmetics.TailFin.DrawSprites += On_TailFin_DrawSprites;
        On.LizardCosmetics.WingScales.DrawSprites += On_WingScales_DrawSprites;
        IL.LizardCosmetics.TailGeckoScales.DrawSprites += IL_TailGeckoScales_DrawSprites;
        new Hook(typeof(Lizard).GetMethod("get_VisibilityBonus", ALL_FLAGS), On_Lizard_get_VisibilityBonus);
        new Hook(typeof(LizardJumpModule).GetMethod("get_canChainJump", ALL_FLAGS), On_LizardJumpModule_get_canChainJump);
        On.Lizard.SpearStick += On_Lizard_SpearStick;
        IL.Lizard.Act += IL_Lizard_Act;
        On.JetFishAI.WantToEatObject += On_JetFishAI_WantToEatObject;
        On.PlayerSessionRecord.AddEat += On_PlayerSessionRecord_AddEat;
        new Hook(typeof(LizardGraphics).GetMethod("get_effectColor", ALL_FLAGS), On_LizardGraphics_get_effectColor);
        On.LizardAI.Update += On_LizardAI_Update;
        On.LizardCosmetics.Whiskers.ctor += On_Whiskers_ctor;
        On.LizardCosmetics.Whiskers.AnchorPoint += On_Whiskers_AnchorPoint;
        On.ShortcutGraphics.ShortCutColor += On_ShortcutGraphics_ShortCutColor;
        IL.Lizard.ActAnimation += IL_Lizard_ActAnimation;
        On.LizardSpit.ApplyPalette += On_LizardSpit_ApplyPalette;
        On.LizardSpit.AddToContainer += On_LizardSpit_AddToContainer;
        IL.LizardSpit.Update += IL_LizardSpit_Update;
        IL.LizardAI.LizardSpitTracker.Update += IL_LizardSpitTracker_Update;
        IL.BigSpider.Update += IL_BigSpider_Update;
        IL.BigSpiderAI.Update += IL_BigSpiderAI_Update;
        IL.BigSpiderAI.SpiderSpitModule.SpitPosScore += IL_SpiderSpitModule_SpitPosScore;
        On.BigSpiderAI.SpiderSpitModule.Update += On_SpiderSpitModule_Update;
        new Hook(typeof(BigSpider).GetMethod("get_CanJump", ALL_FLAGS), On_BigSpider_get_CanJump);
        new Hook(typeof(BigSpider).GetMethod("get_CanIBeRevived", ALL_FLAGS), On_BigSpider_get_CanIBeRevived);
        new Hook(typeof(BigSpiderAI).GetMethod("get_ShyFromLight", ALL_FLAGS), On_BigSpiderAI_get_ShyFromLight);
        IL.BigSpider.FlyingWeapon += IL_BigSpider_FlyingWeapon;
        On.BodyPart.ConnectToPoint += On_BodyPart_ConnectToPoint;
        IL.BigSpider.Spit += IL_BigSpider_Spit;
        On.BigSpider.TryInitiateSpit += On_BigSpider_TryInitiateSpit;
        On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_BigSpiderAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        IL.BigSpider.Collide += IL_BigSpider_Collide;
        On.BigSpiderAI.ReactToNoise += On_BigSpiderAI_ReactToNoise;
        IL.SporeCloud.Update += IL_SporeCloud_Update;
        On.BigSpider.Revive += On_BigSpider_Revive;
        On.BigSpiderAI.SpiderSpitModule.CanSpit += On_SpiderSpitModule_CanSpit;
        IL.BigSpider.Act += IL_BigSpider_Act;
        On.BigSpider.InitiateJump += On_BigSpider_InitiateJump;
        On.Creature.Abstractize += On_Creature_Abstractize;
        On.CollectToken.AvailableToPlayer += On_CollectToken_AvailableToPlayer;
        IL.Leech.Swim += IL_Leech_Swim;
        IL.SnailAI.TileIdleScore += IL_SnailAI_TileIdleScore;
        On.Snail.Click += On_Snail_Click;
        On.SnailAI.TileIdleScore += On_SnailAI_TileIdleScore;
        On.Vulture.VultureThruster.Update += On_VultureThruster_Update;
        On.Vulture.AirBrake += On_Vulture_AirBrake;
        On.OverseerAbstractAI.HowInterestingIsCreature += On_OverseerAbstractAI_HowInterestingIsCreature;
        On.VultureAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_VultureAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        new Hook(typeof(CreatureTemplate).GetMethod("get_IsVulture", ALL_FLAGS), On_CreatureTemplate_get_IsVulture);
        IL.VultureGrub.AttemptCallVulture += IL_VultureGrub_AttemptCallVulture;
        On.MirosBirdAI.DoIWantToBiteCreature += On_MirosBirdAI_DoIWantToBiteCreature;
        On.VultureAI.DoIWantToBiteCreature += On_VultureAI_DoIWantToBiteCreature;
        On.Lizard.DamageAttackClosestChunk += On_Lizard_DamageAttackClosestChunk;
        On.Scavenger.MeleeGetFree += On_Scavenger_MeleeGetFree;
        IL.ShortcutHandler.FlyingCreatureArrivedInRealizedRoom += IL_ShortcutHandler_FlyingCreatureArrivedInRealizedRoom;
        On.AbstractCreature.Update += On_AbstractCreature_Update;
        On.AbstractCreature.IsEnteringDen += On_AbstractCreature_IsEnteringDen;
        IL.BigEelAbstractAI.AddRoomClusterToCheckList += IL_BigEelAbstractAI_AddRoomClusterToCheckList;
        On.BigEelAI.ctor += On_BigEelAI_ctor;
        On.BigEelAI.Update += On_BigEelAI_Update;
        IL.BigEelGraphics.ApplyPalette += IL_BigEelGraphics_ApplyPalette;
        On.BigEel.ctor += On_BigEel_ctor;
        On.BigEelGraphics.ctor += On_BigEelGraphics_ctor;
        On.BigEelGraphics.Update += On_BigEelGraphics_Update;
        On.BigEelGraphics.InitiateSprites += On_BigEelGraphics_InitiateSprites;
        On.BigEelGraphics.DrawSprites += On_BigEelGraphics_DrawSprites;
        On.BigEelAI.WantToChargeJaw += On_BigEelAI_WantToChargeJaw;
        On.BigEel.InBiteArea += On_BigEel_InBiteArea;
        On.BigEel.ShortCutColor += On_BigEel_ShortCutColor;
        On.BigEel.Crush += On_BigEel_Crush;
        On.YellowAI.YellowPack.RemoveLizard_int += On_YellowPack_RemoveLizard_int;
        On.YellowAI.YellowPack.RemoveLizard_AbstractCreature += On_YellowPack_RemoveLizard_AbstractCreature;
        On.YellowAI.YellowPack.FindLeader += On_YellowPack_FindLeader;
        On.LizardAI.NewRoom += On_LizardAI_NewRoom;
        IL.YellowAI.Update += IL_YellowAI_Update;
        IL.Snail.Click += IL_Snail_Click;
        On.LizardCosmetics.AxolotlGills.DrawSprites += On_AxolotlGills_DrawSprites;
        On.LizardAI.LurkTracker.Utility += On_LurkTracker_Utility;
        IL.LizardAI.LurkTracker.LurkPosScore += IL_LurkTracker_LurkPosScore;
        On.LizardAI.ComfortableIdlePosition += On_LizardAI_ComfortableIdlePosition;
        On.LizardAI.IdleSpotScore += On_LizardAI_IdleSpotScore;
        On.LizardAI.TravelPreference += On_LizardAI_TravelPreference;
        On.Lizard.Update += On_Lizard_Update;
        IL.Lizard.SwimBehavior += IL_Lizard_SwimBehavior;
        On.LizardAI.ctor += On_LizardAI_ctor;
        On.LizardPather.HeuristicForCell += On_LizardPather_HeuristicForCell;
        IL.Lizard.EnterAnimation += IL_Lizard_EnterAnimation;
        IL.LizardGraphics.UpdateTailSegment += IL_LizardGraphics_UpdateTailSegment;
        IL.LizardGraphics.Update += IL_LizardGraphics_Update;
        IL.Menu.MultiplayerMenu.ctor += IL_MultiplayerMenu_ctor;
        IL.DaddyLongLegs.ctor += IL_DaddyLongLegs_ctor;
        On.DaddyLongLegs.InitiateGraphicsModule += On_DaddyLongLegs_InitiateGraphicsModule;
        On.DaddyGraphics.RenderSlits += On_DaddyGraphics_RenderSlits;
        IL.DaddyGraphics.ReactToNoise += IL_DaddyGraphics_ReactToNoise;
        On.DaddyAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_DaddyAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.DaddyTentacle.ctor += On_DaddyTentacle_ctor;
        On.DaddyTentacle.CollideWithCreature += On_DaddyTentacle_CollideWithCreature;
        On.DaddyLongLegs.ShortCutColor += On_DaddyLongLegs_ShortCutColor;
        On.Menu.SandboxSettingsInterface.DefaultKillScores += On_SandboxSettingsInterface_DefaultKillScores;
        On.TubeWorm.NewRoom += On_TubeWorm_NewRoom;
        On.TubeWorm.Tongue.Shoot += On_Tongue_Shoot;
        On.TubeWorm.Update += On_TubeWorm_Update;
        On.TubeWormGraphics.Update += On_TubeWormGraphics_Update;
        On.TubeWormGraphics.DrawSprites += On_TubeWormGraphics_DrawSprites;
        On.CreatureSymbol.ColorOfCreature += On_CreatureSymbol_ColorOfCreature;
        On.DevInterface.MapPage.CreatureVis.CritCol += On_CreatureVis_CritCol;
        On.TubeWormGraphics.Reset += On_TubeWormGraphics_Reset;
        On.TubeWormGraphics.ApplyPalette += On_TubeWormGraphics_ApplyPalette;
        On.SLOracleBehaviorHasMark.CreatureJokeDialog += On_SLOracleBehaviorHasMark_CreatureJokeDialog;
        On.SSOracleBehavior.CreatureJokeDialog += On_SSOracleBehavior_CreatureJokeDialog;
        On.ArenaCreatureSpawner.IsMajorCreature += On_ArenaCreatureSpawner_IsMajorCreature;
        new Hook(typeof(Centipede).GetMethod("get_Centiwing", ALL_FLAGS), On_Centipede_get_Centiwing);
        new Hook(typeof(Centipede).GetMethod("get_Red", ALL_FLAGS), On_Centipede_get_Red);
        IL.CentipedeGraphics.DrawSprites += IL_CentipedeGraphics_DrawSprites;
        IL.CentipedeAI.ctor += IL_CentipedeAI_ctor;
        On.Centipede.Update += On_Centipede_Update;
        IL.Centipede.ShortCutColor += IL_Centipede_ShortCutColor;
        IL.Centipede.Stun += IL_Centipede_Stun;
        IL.ScavengerAI.IUseARelationshipTracker_UpdateDynamicRelationship += IL_ScavengerAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        IL.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += IL_BigSpiderAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.CentipedeGraphics.ctor += On_CentipedeGraphics_ctor;
        IL.CentipedeGraphics.InitiateSprites += IL_CentipedeGraphics_InitiateSprites;
        On.CentipedeGraphics.DrawSprites += On_CentipedeGraphics_DrawSprites;
        On.CentipedeGraphics.WhiskerLength += On_CentipedeGraphics_WhiskerLength;
        IL.CentipedeGraphics.Update += IL_CentipedeGraphics_Update;
        new Hook(typeof(CentipedeGraphics).GetMethod("get_SecondaryShellColor", ALL_FLAGS), On_CentipedeGraphics_get_SecondaryShellColor);
        On.CentipedeAI.ctor += On_CentipedeAI_ctor;
        On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_CentipedeAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.CentipedeAI.DoIWantToShockCreature += On_CentipedeAI_DoIWantToShockCreature;
        IL.CentipedeAI.VisualScore += IL_CentipedeAI_VisualScore;
        IL.CentipedeAI.Update += IL_CentipedeAI_Update;
        On.CentipedeAI.CreatureSpotted += On_CentipedeAI_CreatureSpotted;
        On.Centipede.GenerateSize += On_Centipede_GenerateSize;
        IL.Centipede.ctor += IL_Centipede_ctor;
        On.Centipede.ctor += On_Centipede_ctor;
        On.Centipede.SpearStick += On_Centipede_SpearStick;
        IL.Centipede.Violence += IL_Centipede_Violence;
        IL.Centipede.Crawl += IL_Centipede_Crawl;
        On.Centipede.ShortCutColor += On_Centipede_ShortCutColor;
        IL.Centipede.Shock += IL_Centipede_Shock;
        On.SlugcatStats.NourishmentOfObjectEaten += On_SlugcatStats_NourishmentOfObjectEaten;
        On.AbstractCreature.ctor += On_AbstractCreature_ctor;
        On.Fly.NewRoom += On_Fly_NewRoom;
        On.Fly.Act += On_Fly_Act;
        On.Fly.BatFlight += On_Fly_BatFlight;
        On.FlyAI.Update += On_FlyAI_Update;
        On.FlyGraphics.InitiateSprites += On_FlyGraphics_InitiateSprites;
        On.FlyGraphics.DrawSprites += On_FlyGraphics_DrawSprites;
        On.CreatureSymbol.SpriteNameOfCreature += On_CreatureSymbol_SpriteNameOfCreature;
        On.DevInterface.MapPage.CreatureVis.CritString += On_CreatureVis_CritString;
        On.CreatureSymbol.SymbolDataFromCreature += On_CreatureSymbol_SymbolDataFromCreature;
        On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += On_MultiplayerUnlocks_SymbolDataForSandboxUnlock;
        On.MultiplayerUnlocks.SandboxUnlockForSymbolData += On_MultiplayerUnlocks_SandboxUnlockForSymbolData;
        IL.SandboxGameSession.SpawnEntity += IL_SandboxGameSession_SpawnEntity;
        On.Menu.SandboxSettingsInterface.IsThisSandboxUnlockVisible += On_SandboxSettingsInterface_IsThisSandboxUnlockVisible;
        On.Limb.FindGrip += On_Limb_FindGrip;
        On.BodyPart.OnOtherSideOfTerrain += On_BodyPart_OnOtherSideOfTerrain;
        On.BodyPart.PushOutOfTerrain += On_BodyPart_PushOutOfTerrain;
        On.EggBugAI.UnpleasantFallRisk += On_EggBugAI_UnpleasantFallRisk;
        On.EggBug.DropEggs += On_EggBug_DropEggs;
        On.EggBugAI.IdleScore += On_EggBugAI_IdleScore;
        IL.EggBug.Act += IL_EggBug_Act;
        IL.EggBugGraphics.Update += IL_EggBugGraphics_Update;
        On.EggBug.Run += On_EggBug_Run;
        On.EggBug.MoveTowards += On_EggBug_MoveTowards;
        new Hook(typeof(EggBugGraphics).GetMethod("get_ShowEggs", ALL_FLAGS), On_EggBugGraphics_get_ShowEggs);
        On.LizardVoice.ctor += On_LizardVoice_ctor;
        On.LizardTongue.ctor += On_LizardTongue_ctor;
        new Hook(typeof(LizardBreedParams).GetMethod("get_WallClimber", ALL_FLAGS), On_LizardBreedParams_get_WallClimber);
        On.LizardGraphics.Update += On_LizardGraphics_Update;
        On.LizardGraphics.DynamicBodyColor += On_LizardGraphics_DynamicBodyColor;
        On.LizardGraphics.HeadColor += On_LizardGraphics_HeadColor;
        On.LizardGraphics.WhiteFlicker += On_LizardGraphics_WhiteFlicker;
        On.LizardGraphics.InitiateSprites += On_LizardGraphics_InitiateSprites;
        On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.LizardGraphics.DrawSprites += On_LizardGraphics_DrawSprites;
        On.Lizard.HitHeadShield += On_Lizard_HitHeadShield;
        On.LizardBubble.ctor += On_LizardBubble_ctor;
        On.LizardBubble.DrawSprites += On_LizardBubble_DrawSprites;
        On.LizardGraphics.AddCosmetic += On_LizardGraphics_AddCosmetic;
        On.Player.Grabbed += On_Player_Grabbed;
        IL.HUD.Map.Draw += IL_Map_Draw;
        IL.OverseerAbstractAI.HowInterestingIsCreature += IL_OverseerAbstractAI_HowInterestingIsCreature;
        On.LizardLimb.ctor += On_LizardLimb_ctor;
        On.LizardVoice.GetMyVoiceTrigger += On_LizardVoice_GetMyVoiceTrigger;
        On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += On_LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate;
        On.Lizard.ctor += On_Lizard_ctor;
        On.LizardGraphics.ctor += On_LizardGraphics_ctor;
        On.AbstractConsumable.ctor += On_AbstractConsumable_ctor;
        On.AbstractConsumable.IsTypeConsumable += On_AbstractConsumable_IsTypeConsumable;
        On.SaveState.SetCustomData_AbstractPhysicalObject_string += On_SaveState_SetCustomData_AbstractPhysicalObject_string;
        On.SaveState.AbstractPhysicalObjectFromString += On_SaveState_AbstractPhysicalObjectFromString;
        On.AbstractPhysicalObject.Realize += On_AbstractPhysicalObject_Realize;
        On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += On_ObjectsPage_DevObjectGetCategoryFromPlacedType;
        On.DevInterface.ObjectsPage.CreateObjRep += On_ObjectsPage_CreateObjRep;
        On.PlacedObject.GenerateEmptyData += On_PlacedObject_GenerateEmptyData;
        On.Room.SpawnMultiplayerItem += On_Room_SpawnMultiplayerItem;
        On.ArenaGameSession.SpawnItem += On_ArenaGameSession_SpawnItem;
        On.ItemSymbol.ColorForItem += On_ItemSymbol_ColorForItem;
        On.ItemSymbol.SpriteNameForItem += On_ItemSymbol_SpriteNameForItem;
        On.SLOracleBehaviorHasMark.TypeOfMiscItem += On_SLOracleBehaviorHasMark_TypeOfMiscItem;
        On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += On_MoonConversation_AddEvents;
        On.Player.Grabability += On_Player_Grabability;
        On.ScavengerAI.CollectScore_PhysicalObject_bool += On_ScavengerAI_CollectScore_PhysicalObject_bool;
        On.ScavengerAI.WeaponScore += On_ScavengerAI_WeaponScore;
        On.OverseerCommunicationModule.FoodDelicousScore += On_OverseerCommunicationModule_FoodDelicousScore;
        On.Player.GraspsCanBeCrafted += On_Player_GraspsCanBeCrafted;
        On.Room.Loaded += On_Room_Loaded;
        On.Player.ThrowObject += On_Player_ThrowObject;
        On.Player.TossObject += On_Player_TossObject;
        On.AbstractCreature.setCustomFlags += On_AbstractCreature_setCustomFlags;
        On.PlacedObject.ConsumableObjectData.ctor += On_ConsumableObjectData_ctor;
        On.ShortcutHelper.PopsOutOfDeadShortcuts += On_ShortcutHelper_PopsOutOfDeadShortcuts;
        On.Hazer.Collide += On_Hazer_Collide;
        IL.AbstractCreature.InitiateAI += IL_AbstractCreature_InitiateAI;
        On.AbstractCreature.MSCInitiateAI += IL_AbstractCreature_MSCInitiateAI;
        On.Player.IsCreatureLegalToHoldWithoutStun += On_Player_IsCreatureLegalToHoldWithoutStun;
        On.HazerGraphics.ctor += On_HazerGraphics_ctor;
        IL.HazerGraphics.ApplyPalette += IL_HazerGraphics_ApplyPalette;
        On.Hazer.Update += On_Hazer_Update;
        IL.DevInterface.ObjectsPage.AssembleObjectPages += IL_ObjectsPage_AssembleObjectPages;
        On.JetFish.ctor += On_JetFish_ctor;
        On.EggBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_EggBugAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.BigNeedleWormAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_BigNeedleWormAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.SmallNeedleWormAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_SmallNeedleWormAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.DropBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += On_DropBugAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.AbstractConsumable.Consume += On_AbstractConsumable_Consume;
        IL.OverseerCommunicationModule.FoodDelicousScore += IL_OverseerCommunicationModule_FoodDelicousScore;
        On.Lizard.ShortCutColor += On_Lizard_ShortCutColor;
        On.LizardAI.ReactToNoise += On_LizardAI_ReactToNoise;
        IL.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += IL_LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        On.LizardGraphics.CreatureSpotted += On_LizardGraphics_CreatureSpotted;
        On.LizardCosmetics.LongHeadScales.ctor += On_LongHeadScales_ctor;
        On.ArtificialIntelligence.VisualScore += On_ArtificialIntelligence_VisualScore;
        On.MirosBird.ctor += On_MirosBird_ctor;
        On.MirosBird.BirdLeg.RunMode += On_BirdLeg_RunMode;
        On.MirosBird.Act += On_MirosBird_Act;
        On.Creature.Blind += On_Creature_Blind;
        On.MirosBirdGraphics.ctor += On_MirosBirdGraphics_ctor;
        On.MirosBirdGraphics.InitiateSprites += On_MirosBirdGraphics_InitiateSprites;
        On.MirosBirdGraphics.ApplyPalette += On_MirosBirdGraphics_ApplyPalette;
        On.MirosBirdAbstractAI.ctor += On_MirosBirdAbstractAI_ctor;
        IL.MirosBirdAbstractAI.Raid += IL_MirosBirdAbstractAI_Raid;
        On.JetFishAI.SocialEvent += On_JetFishAI_SocialEvent;
        On.Water.DrawSprites += On_Water_DrawSprites;
        IL.Leech.Swim += IL_Leech_Swim;
        On.AbstractCreature.WantToStayInDenUntilEndOfCycle += On_AbstractCreature_WantToStayInDenUntilEndOfCycle;
        IL.ArenaBehaviors.ExitManager.Update += IL_ExitManager_Update;
        IL.SnailAI.CreatureUnease += IL_SnailAI_CreatureUnease;
        IL.SnailAI.TileIdleScore += IL_SnailAI_TileIdleScore;
        On.Snail.VibrateLeeches += On_Snail_VibrateLeeches;
        On.OverseerAbstractAI.AllowSwarmTarget += On_OverseerAbstractAI_AllowSwarmTarget;
        On.GarbageWormAI.CreateTrackerRepresentationForCreature += On_GarbageWormAI_CreateTrackerRepresentationForCreature;
        On.Room.PlaceQCScore += On_Room_PlaceQCScore;
        new Hook(typeof(SandboxRegistry).GetMethod("DoSpawn", ALL_FLAGS), On_SandboxRegistry_DoSpawn);
        On.FlareBomb.Update += On_FlareBomb_Update;
        Content.Register(new WaterBlobCritob(),
                        new BouncingBallCritob(),
                        new HazerMomCritob(),
                        new MiniLeechCritob(),
                        new NoodleEaterCritob(),
                        new PolliwogCritob(),
                        new WaterSpitterCritob(),
                        new MoleSalamanderCritob(),
                        new SilverLizardCritob(),
                        new HunterSeekerCritob(),
                        new HoverflyCritob(),
                        new FatFireFlyCritob(),
                        new BlizzorCritob(),
                        new SurfaceSwimmerCritob(),
                        new ThornBugCritob(),
                        new TintedBeetleCritob(),
                        new ScutigeraCritob(),
                        new RedHorrorCritob(),
                        new SporantulaCritob(),
                        new MiniLeviathanCritob(),
                        new MiniFlyingBigEelCritob(),
                        new FlyingBigEelCritob());
    }

    public void OnDisable()
    {
        StrawberryData = null!;
        s_DrainMite = null;
        s_SnootShootNoot = null;
        Seed = null!;
        Big = null!;
        Albino = null!;
        Jelly = null!;
        HoverflyData = null!;
        SporeMemory = null!;
        StationFruit = null!;
        StationPlant = null!;
        PlayerData = null!;
        OnResultAction = null;
        OnFoodItemSpotted = null;
        s_logger = null;
    }
}