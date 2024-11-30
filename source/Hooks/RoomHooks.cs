global using static LBMergedMods.Hooks.RoomHooks;
using System.IO;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using RWCustom;
using System.Collections.Generic;
using CoralBrain;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class RoomHooks
{
    internal static int On_AImap_ExitDistanceForCreatureAndCheckNeighbours(On.AImap.orig_ExitDistanceForCreatureAndCheckNeighbours orig, AImap self, IntVector2 pos, int creatureSpecificExitIndex, CreatureTemplate crit)
    {
        if (crit.PreBakedPathingIndex < 0 || crit.PreBakedPathingIndex >= self.creatureSpecificAImaps?.Length)
            return -1;
        return orig(self, pos, creatureSpecificExitIndex, crit);
    }

    internal static void On_LightSource_InitiateSprites(On.LightSource.orig_InitiateSprites orig, LightSource self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.tiedToObject is StarLemon && self.flat)
            sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["FlatLightBehindTerrain"];
    }

    internal static void IL_MeltLights_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchIsinst_Fly))
            c.EmitDelegate((Fly fly) => (fly?.room?.world?.region?.name == "NP" && fly.IsSeed()) ? null : fly);
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook MeltLights.Update!");
    }

    internal static void On_Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        if (LBMergedModsPlugin.Bundle is null && self.game is RainWorldGame g)
        {
            try
            {
                LBMergedModsPlugin.Bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles" + Path.DirectorySeparatorChar + "lbmodpack_shaders"));
                g.rainWorld.Shaders["MiniLeviEelBody"] = FShader.CreateShader("MiniLeviEelBody", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "MiniLeviEelBody.shader"));
                g.rainWorld.Shaders["MiniLeviEelFin"] = FShader.CreateShader("MiniLeviEelFin", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "MiniLeviEelFin.shader"));
                g.rainWorld.Shaders["AMiniLeviEelBody"] = FShader.CreateShader("AMiniLeviEelBody", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "AMiniLeviEelBody.shader"));
                g.rainWorld.Shaders["AMiniLeviEelFin"] = FShader.CreateShader("AMiniLeviEelFin", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "AMiniLeviEelFin.shader"));
                g.rainWorld.Shaders["GRJEelBody"] = FShader.CreateShader("GRJEelBody", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "GRJEelBody.shader"));
                g.rainWorld.Shaders["GRJMiniEelBody"] = FShader.CreateShader("GRJMiniEelBody", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "GRJMiniEelBody.shader"));
                g.rainWorld.Shaders["StarLemonBloom"] = FShader.CreateShader("StarLemonBloom", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "StarLemonBloom.shader"));
                _MiniLeviColorA = Shader.PropertyToID("_MiniLeviColorA");
                _MiniLeviColorB = Shader.PropertyToID("_MiniLeviColorB");
                _MiniLeviColorHead = Shader.PropertyToID("_MiniLeviColorHead");
                _AMiniLeviColorA = Shader.PropertyToID("_AMiniLeviColorA");
                _AMiniLeviColorB = Shader.PropertyToID("_AMiniLeviColorB");
                _AMiniLeviColorHead = Shader.PropertyToID("_AMiniLeviColorHead");
                _GRJLeviathanColorA = Shader.PropertyToID("_GRJLeviathanColorA");
                _GRJLeviathanColorB = Shader.PropertyToID("_GRJLeviathanColorB");
                _GRJLeviathanColorHead = Shader.PropertyToID("_GRJLeviathanColorHead");
                _GRJMiniLeviathanColorA = Shader.PropertyToID("_GRJMiniLeviathanColorA");
                _GRJMiniLeviathanColorB = Shader.PropertyToID("_GRJMiniLeviathanColorB");
                _GRJMiniLeviathanColorHead = Shader.PropertyToID("_GRJMiniLeviathanColorHead");
            }
            catch (Exception e)
            {
                LBMergedModsPlugin.s_logger.LogError("Issue with shader loading: " + e.ToString());
            }
        }
        var firstTimeRealized = self.abstractRoom.firstTimeRealized;
        orig(self);
        if (self.game is RainWorldGame game)
        {
            var objs = self.roomSettings.placedObjects;
            for (var i = 0; i < objs.Count; i++)
            {
                var pObj = objs[i];
                if (pObj.active)
                {
                    if (firstTimeRealized && pObj.type == PlacedObjectType.ThornyStrawberry && (game.session is not StoryGameSession session || !session.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i)))
                        self.abstractRoom.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.ThornyStrawberry, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), self.abstractRoom.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.LittleBalloon && (game.session is not StoryGameSession session2 || !session2.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i)))
                        self.abstractRoom.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.LittleBalloon, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), self.abstractRoom.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.BouncingMelon && (game.session is not StoryGameSession session3 || !session3.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i)))
                        self.abstractRoom.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.BouncingMelon, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), self.abstractRoom.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.LimeMushroom && (game.session is not StoryGameSession session13 || !session13.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i)))
                        self.abstractRoom.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.LimeMushroom, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), self.abstractRoom.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.MarineEye && (game.session is not StoryGameSession session14 || !session14.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i)))
                        self.abstractRoom.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.MarineEye, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), self.abstractRoom.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.StarLemon && (game.session is not StoryGameSession session15 || !session15.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i)))
                        self.abstractRoom.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.StarLemon, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), self.abstractRoom.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.DendriticNeuron && (game.session is not StoryGameSession session16 || !session16.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i)))
                    {
                        var flag = true;
                        var uads = self.updateList;
                        for (var t = 0; t < uads.Count; t++)
                        {
                            if (uads[t] is CoralNeuronSystem)
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                            self.AddObject(new CoralNeuronSystem());
                        self.abstractRoom.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.DendriticNeuron, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), self.abstractRoom.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false });
                        self.waitToEnterAfterFullyLoaded = Math.Max(self.waitToEnterAfterFullyLoaded, 80);
                    }
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.RubberBlossom)
                    {
                        AbstractConsumable plant;
                        self.abstractRoom.entities.Add(plant = new AbstractConsumable(self.world, AbstractObjectType.RubberBlossom, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), self.abstractRoom.index, i, (pObj.data as PlacedObject.ConsumableObjectData)!)
                        {
                            isConsumed = false
                        });
                        if (StationPlant.TryGetValue(plant, out var props))
                        {
                            if (game.session is StoryGameSession ses)
                            {
                                var (consumed, waitCycles) = ses.saveState.PlantConsumed(self.world, self.abstractRoom.index, i);
                                if (consumed)
                                {
                                    if (waitCycles - 100 <= 0)
                                    {
                                        if (props.StartsOpen && !props.AlwaysOpen)
                                            props.NumberOfFruits = 0;
                                        props.RemainingOpenCycles = 0;
                                    }
                                    else
                                    {
                                        if (!props.StartsOpen && !props.AlwaysOpen)
                                            props.NumberOfFruits = 0;
                                        props.RemainingOpenCycles = (int)Math.Floor(waitCycles * .01f);
                                    }
                                }
                                else if (!props.StartsOpen && !props.AlwaysOpen)
                                    props.NumberOfFruits = 0;
                            }
                            else if (!props.StartsOpen && !props.AlwaysOpen)
                                props.NumberOfFruits = 0;
                            if (props.AlwaysClosed)
                            {
                                props.NumberOfFruits = 0;
                                props.RemainingOpenCycles = 0;
                            }
                            plant.isConsumed = props.RemainingOpenCycles == 0;
                            var state = Random.state;
                            Random.InitState(plant.ID.RandomSeed);
                            AbstractConsumable fruit;
                            for (var j = 1; j <= props.NumberOfFruits; j++)
                            {
                                self.abstractRoom.entities.Add(fruit = new AbstractConsumable(self.world, AbstractObjectType.GummyAnther, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), self.abstractRoom.index, -1, null)
                                {
                                    isConsumed = false
                                });
                                if (StationFruit.TryGetValue(fruit, out var fprops))
                                    fprops.Plant = plant;
                            }
                            Random.state = state;
                        }
                    }
                    else if (pObj.type == PlacedObjectType.Physalis)
                    {
                        if (game.session is not StoryGameSession session7 || !session7.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i))
                        {
                            if (firstTimeRealized)
                                self.abstractRoom.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.Physalis, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), self.abstractRoom.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false });
                            else
                                self.AddObject(new Physalis.Stem(pObj.pos, self, false));
                        }
                        else
                            self.AddObject(new Physalis.Stem(pObj.pos, self, true));
                    }
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.HazerMom || pObj.type == PlacedObjectType.DeadHazerMom || pObj.type == PlacedObjectType.AlbinoHazerMom || pObj.type == PlacedObjectType.DeadAlbinoHazerMom)
                    {
                        if (game.session is not StoryGameSession sess || !sess.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i))
                        {
                            var abstractCreature = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate(CreatureTemplateType.HazerMom), null, self.GetWorldCoordinate(pObj.pos), game.GetNewID())
                            {
                                superSizeMe = pObj.type == PlacedObjectType.HazerMom || pObj.type == PlacedObjectType.DeadHazerMom,
                                spawnData = "{AlternateForm}"
                            };
                            var state = (abstractCreature.state as HazerMomState)!;
                            state.OrigRoom = self.abstractRoom.index;
                            state.PlacedObjectIndex = i;
                            self.abstractRoom.AddEntity(abstractCreature);
                            if (pObj.type == PlacedObjectType.DeadHazerMom || pObj.type == PlacedObjectType.DeadAlbinoHazerMom)
                                state.Die();
                        }
                    }
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.AlbinoFormHazer || pObj.type == PlacedObjectType.DeadAlbinoFormHazer)
                    {
                        if (game.session is not StoryGameSession sess || !sess.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i))
                        {
                            var abstractCreature = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Hazer), null, self.GetWorldCoordinate(pObj.pos), game.GetNewID())
                            {
                                spawnData = "{albinoform}"
                            };
                            if (Albino.TryGetValue(abstractCreature, out var props))
                                props.Value = true;
                            var state = (abstractCreature.state as VultureGrub.VultureGrubState)!;
                            state.origRoom = self.abstractRoom.index;
                            state.placedObjectIndex = i;
                            self.abstractRoom.AddEntity(abstractCreature);
                            if (pObj.type == PlacedObjectType.DeadAlbinoFormHazer)
                                state.Die();
                        }
                    }
                }
            }
        }
    }

    internal static float On_Room_PlaceQCScore(On.Room.orig_PlaceQCScore orig, Room self, IntVector2 pos, CreatureSpecificAImap critMap, int node, CreatureTemplate.Type critType, List<IntVector2> QCPositions)
    {
        var num = orig(self, pos, critMap, node, critType, QCPositions);
        if (critType == CreatureTemplateType.MiniBlackLeech)
        {
            num += self.aimap.getAItile(pos).visibility;
            num += self.aimap.getTerrainProximity(pos) * .2f;
        }
        return num;
    }

    internal static void On_Room_SpawnMultiplayerItem(On.Room.orig_SpawnMultiplayerItem orig, Room self, PlacedObject placedObj)
    {
        var data = (placedObj.data as PlacedObject.MultiplayerItemData)!;
        if (data.type == MultiplayerItemType.ThornyStrawberry || data.type == MultiplayerItemType.LittleBalloon || data.type == MultiplayerItemType.BouncingMelon || data.type == MultiplayerItemType.Physalis || data.type == MultiplayerItemType.LimeMushroom || data.type == MultiplayerItemType.MarineEye || data.type == MultiplayerItemType.StarLemon)
        {
            var tp = new AbstractPhysicalObject.AbstractObjectType(data.type.value);
            if (tp.Index >= 0 && Random.value <= data.chance)
                self.abstractRoom.entities.Add(new AbstractConsumable(self.world, tp, null, self.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), -2, -2, null));
        }
        else
            orig(self, placedObj);
    }

    internal static AbstractRoomNode On_World_GetNode(On.World.orig_GetNode orig, World self, WorldCoordinate c)
    {
        if (c.abstractNode < 0 || self.GetAbstractRoom(c.room)?.nodes is not AbstractRoomNode[] nds || c.abstractNode >= nds.Length)
            return new(new("UnregisteredNodeType"), 0, 0, false, 0, 0);
        return orig(self, c);
    }

    internal static int On_World_TotalShortCutLengthBetweenTwoConnectedRooms_AbstractRoom_AbstractRoom(On.World.orig_TotalShortCutLengthBetweenTwoConnectedRooms_AbstractRoom_AbstractRoom orig, World self, AbstractRoom room1, AbstractRoom room2)
    {
        if (room1?.nodes is AbstractRoomNode[] nds1 && room2?.nodes is AbstractRoomNode[] nds2 && (room1.ExitIndex(room2.index) >= nds1.Length || room2.ExitIndex(room1.index) >= nds2.Length))
            return -1;
        return orig(self, room1, room2);
    }

    [SuppressMessage(null, "IDE0060"), MethodImpl(MethodImplOptions.NoInlining)]
    public static int DefaultWaterLevel(this Room self, IntVector2 pos) => self.defaultWaterLevel;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float FloatWaterLevel(this Room self, Vector2 pos) => self.FloatWaterLevel(pos.x);
}