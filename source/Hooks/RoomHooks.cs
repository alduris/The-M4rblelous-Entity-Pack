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

namespace LBMergedMods.Hooks;
//CHK
public static class RoomHooks
{
    internal static void On_Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        if (LBMergedModsPlugin.Bundle is null && self.game is RainWorldGame g)
        {
            try
            {
                LBMergedModsPlugin.Bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles" + Path.DirectorySeparatorChar + "lbmodpack_shaders"));
                var shaders = g.rainWorld.Shaders;
                shaders["MiniLeviEelBody"] = FShader.CreateShader("MiniLeviEelBody", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "MiniLeviEelBody.shader"));
                shaders["MiniLeviEelFin"] = FShader.CreateShader("MiniLeviEelFin", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "MiniLeviEelFin.shader"));
                shaders["AMiniLeviEelBody"] = FShader.CreateShader("AMiniLeviEelBody", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "AMiniLeviEelBody.shader"));
                shaders["AMiniLeviEelFin"] = FShader.CreateShader("AMiniLeviEelFin", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "AMiniLeviEelFin.shader"));
                shaders["GRJEelBody"] = FShader.CreateShader("GRJEelBody", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "GRJEelBody.shader"));
                shaders["GRJMiniEelBody"] = FShader.CreateShader("GRJMiniEelBody", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "GRJMiniEelBody.shader"));
                shaders["StarLemonBloom"] = FShader.CreateShader("StarLemonBloom", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "StarLemonBloom.shader"));
                shaders["DivingBeetleFin"] = FShader.CreateShader("DivingBeetleFin", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "DivingBeetleFin.shader"));
                shaders["DivingBeetleFin2"] = FShader.CreateShader("DivingBeetleFin2", LBMergedModsPlugin.Bundle.LoadAsset<Shader>("Assets" + Path.DirectorySeparatorChar + "DivingBeetleFin2.shader"));
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
        var arm = self.abstractRoom;
        var firstTimeRealized = arm.firstTimeRealized;
        orig(self);
        if (self.game is RainWorldGame game)
        {
            var exped = ModManager.Expedition && game.rainWorld.ExpeditionMode;
            var objs = self.roomSettings.placedObjects;
            for (var i = 0; i < objs.Count; i++)
            {
                var pObj = objs[i];
                if (pObj.active)
                {
                    if (pObj.type == PlacedObjectType.BonusScoreToken && !exped)
                    {
                        var data = (pObj.data as ScoreTokenData)!;
                        self.AddObject(game.session is StoryGameSession session && session.saveState?.deathPersistentSaveData?.GetScoreTokenCollected(data.ID) is false ? new ScoreToken(self, pObj) : new ScoreToken.TokenStalk(self, pObj.pos, pObj.pos + data.handlePos, null));
                    }
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.ThornyStrawberry && (game.session is not StoryGameSession session || !session.saveState.ItemConsumed(self.world, false, arm.index, i)))
                        arm.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.ThornyStrawberry, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false, placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i) });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.LittleBalloon && (game.session is not StoryGameSession session2 || !session2.saveState.ItemConsumed(self.world, false, arm.index, i)))
                        arm.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.LittleBalloon, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false, placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i) });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.BouncingMelon && (game.session is not StoryGameSession session3 || !session3.saveState.ItemConsumed(self.world, false, arm.index, i)))
                        arm.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.BouncingMelon, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false, placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i) });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.LimeMushroom && (game.session is not StoryGameSession session13 || !session13.saveState.ItemConsumed(self.world, false, arm.index, i)))
                        arm.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.LimeMushroom, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false, placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i) });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.MarineEye && (game.session is not StoryGameSession session14 || !session14.saveState.ItemConsumed(self.world, false, arm.index, i)))
                        arm.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.MarineEye, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false, placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i) });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.StarLemon && (game.session is not StoryGameSession session15 || !session15.saveState.ItemConsumed(self.world, false, arm.index, i)))
                        arm.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.StarLemon, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false, placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i) });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.SporeProjectile && (game.session is not StoryGameSession session133 || !session133.saveState.ItemConsumed(self.world, false, arm.index, i)))
                        arm.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.SporeProjectile, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false, placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i) });
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.DendriticNeuron && (game.session is not StoryGameSession session16 || !session16.saveState.ItemConsumed(self.world, false, arm.index, i)))
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
                        arm.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.DendriticNeuron, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false, placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i) });
                        self.waitToEnterAfterFullyLoaded = Math.Max(self.waitToEnterAfterFullyLoaded, 80);
                    }
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.MiniFruitBranch && (game.session is not StoryGameSession session23 || !session23.saveState.ItemConsumed(self.world, false, arm.index, i)))
                    {
                        var data = (pObj.data as MiniFruitSpawnerData)!;
                        AbstractConsumable spawner;
                        arm.entities.Add(spawner = new AbstractConsumable(self.world, AbstractObjectType.MiniFruitSpawner, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, i, data)
                        {
                            isConsumed = false,
                            placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i)
                        });
                        if (MiniFruitSpawners.TryGetValue(spawner, out var props))
                        {
                            props.RootPos = data.RootHandlePos + pObj.pos;
                            var state = Random.state;
                            Random.InitState(spawner.ID.RandomSeed);
                            AbstractConsumable fruit;
                            List<Vector2> vecs = [];
                            props.NumberOfFruits = game.session is StoryGameSession ses ? data.FoodAmount - ses.saveState.ConsumedFruits(self.world, arm.index, i) : data.FoodAmount;
                            for (var j = 1; j <= props.NumberOfFruits; j++)
                            {
                                arm.entities.Add(fruit = new AbstractConsumable(self.world, AbstractObjectType.MiniBlueFruit, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, -10 - i, null)
                                {
                                    isConsumed = false,
                                    placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i)
                                });
                                if (MiniFruits.TryGetValue(fruit, out var fprops))
                                {
                                    fprops.Spawner = spawner;
                                    var pos = Random.insideUnitCircle * .95f * data.Rad + pObj.pos;
                                    var cntUnstuck = 0;
                                    while (vecs.ContainsClosePosition(pos, 10f) && cntUnstuck < 25)
                                    {
                                        pos = Random.insideUnitCircle * .95f * data.Rad + pObj.pos;
                                        ++cntUnstuck;
                                    }
                                    fprops.FruitPos = pos;
                                }
                            }
                            Random.state = state;
                        }
                    }
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.RubberBlossom)
                    {
                        AbstractConsumable plant;
                        arm.entities.Add(plant = new AbstractConsumable(self.world, AbstractObjectType.RubberBlossom, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, i, (pObj.data as PlacedObject.ConsumableObjectData)!)
                        {
                            isConsumed = false,
                            placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i)
                        });
                        if (StationPlant.TryGetValue(plant, out var props))
                        {
                            //var consumedFruits = 0;
                            if (game.session is StoryGameSession ses)
                            {
                                //consumedFruits = ses.saveState.ConsumedFruits(self.world, arm.index, i); // removal intended
                                var (consumed, waitCycles) = ses.saveState.PlantConsumed(self.world, arm.index, i);
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
                            for (var j = 1; j <= props.NumberOfFruits /* - consumedFruits*/; j++)
                            {
                                arm.entities.Add(fruit = new AbstractConsumable(self.world, AbstractObjectType.GummyAnther, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, -10 - i, null)
                                {
                                    isConsumed = false,
                                    placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i)
                                });
                                if (StationFruit.TryGetValue(fruit, out var fprops))
                                    fprops.Plant = plant;
                            }
                            Random.state = state;
                        }
                    }
                    else if (pObj.type == PlacedObjectType.Physalis)
                    {
                        if (game.session is not StoryGameSession session7 || !session7.saveState.ItemConsumed(self.world, false, arm.index, i))
                        {
                            if (firstTimeRealized)
                                arm.AddEntity(new AbstractConsumable(self.world, AbstractObjectType.Physalis, null, self.GetWorldCoordinate(pObj.pos), game.GetNewID(), arm.index, i, pObj.data as PlacedObject.ConsumableObjectData) { isConsumed = false, placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i) });
                            else
                                self.AddObject(new Physalis.Stem(pObj.pos, self, false));
                        }
                        else
                            self.AddObject(new Physalis.Stem(pObj.pos, self, true));
                    }
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.HazerMom || pObj.type == PlacedObjectType.DeadHazerMom || pObj.type == PlacedObjectType.AlbinoHazerMom || pObj.type == PlacedObjectType.DeadAlbinoHazerMom)
                    {
                        if (game.session is not StoryGameSession sess || !sess.saveState.ItemConsumed(self.world, false, arm.index, i))
                        {
                            var abstractCreature = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate(CreatureTemplateType.HazerMom), null, self.GetWorldCoordinate(pObj.pos), game.GetNewID())
                            {
                                superSizeMe = pObj.type == PlacedObjectType.HazerMom || pObj.type == PlacedObjectType.DeadHazerMom,
                                spawnData = "{AlternateForm}",
                                placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i)
                            };
                            var state = (abstractCreature.state as HazerMomState)!;
                            state.OrigRoom = arm.index;
                            state.PlacedObjectIndex = i;
                            arm.AddEntity(abstractCreature);
                            if (pObj.type == PlacedObjectType.DeadHazerMom || pObj.type == PlacedObjectType.DeadAlbinoHazerMom)
                                state.Die();
                        }
                    }
                    else if (firstTimeRealized && pObj.type == PlacedObjectType.AlbinoFormHazer || pObj.type == PlacedObjectType.DeadAlbinoFormHazer)
                    {
                        if (game.session is not StoryGameSession sess || !sess.saveState.ItemConsumed(self.world, false, arm.index, i))
                        {
                            var abstractCreature = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Hazer), null, self.GetWorldCoordinate(pObj.pos), game.GetNewID())
                            {
                                spawnData = "{albinoform}",
                                placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i)
                            };
                            if (Albino.TryGetValue(abstractCreature, out var props))
                                props.Value = true;
                            var state = (abstractCreature.state as VultureGrub.VultureGrubState)!;
                            state.origRoom = arm.index;
                            state.placedObjectIndex = i;
                            arm.AddEntity(abstractCreature);
                            if (pObj.type == PlacedObjectType.DeadAlbinoFormHazer)
                                state.Die();
                        }
                    }
                    /*else if (firstTimeRealized && pObj.type == PlacedObjectType.PlacedXylo)
                    {
                        if (game.session is not StoryGameSession sess || !sess.saveState.ItemConsumed(self.world, false, arm.index, i))
                        {
                            var abstractCreature = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate(CreatureTemplateType.Xylo), null, self.GetWorldCoordinate(pObj.pos), game.GetNewID())
                            {
                                placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(arm.name, i)
                            };
                            var state = (abstractCreature.state as HazerMomState)!;
                            state.OrigRoom = arm.index;
                            state.PlacedObjectIndex = i;
                            arm.AddEntity(abstractCreature);
                        }
                    }*/
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
        if (data.type == MultiplayerItemType.ThornyStrawberry || data.type == MultiplayerItemType.LittleBalloon || data.type == MultiplayerItemType.BouncingMelon || data.type == MultiplayerItemType.Physalis || data.type == MultiplayerItemType.LimeMushroom || data.type == MultiplayerItemType.MarineEye || data.type == MultiplayerItemType.StarLemon || data.type == MultiplayerItemType.SporeProjectile)
        {
            var tp = new AbstractPhysicalObject.AbstractObjectType(data.type.value);
            if (tp.Index >= 0 && Random.value <= data.chance)
                self.abstractRoom.entities.Add(new AbstractConsumable(self.world, tp, null, self.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), -2, -2, null));
        }
        else
            orig(self, placedObj);
    }

    [SuppressMessage(null, "IDE0060"), MethodImpl(MethodImplOptions.NoInlining)]
    public static int DefaultWaterLevel(this Room self, IntVector2 pos) => self.defaultWaterLevel;

    public static bool ContainsClosePosition(this List<Vector2> self, Vector2 pos, float dist)
    {
        for (var i = 0; i < self.Count; i++)
        {
            var vec = self[i];
            if (Math.Abs(vec.y - pos.y) < dist || Math.Abs(vec.x - pos.x) < dist)
                return true;
        }
        return false;
    }
}