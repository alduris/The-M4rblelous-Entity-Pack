global using static LBMergedMods.Hooks.RoomHooks;
using System.IO;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using RWCustom;
using System.Collections.Generic;

namespace LBMergedMods.Hooks;

public static class RoomHooks
{
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

    internal static void On_Room_SpawnMultiplayerItem(On.Room.orig_SpawnMultiplayerItem orig, Room self, PlacedObject placedObj)
    {
        var data = (placedObj.data as PlacedObject.MultiplayerItemData)!;
        if (data.type == MultiplayerItemType.ThornyStrawberry || data.type == MultiplayerItemType.LittleBalloon || data.type == MultiplayerItemType.BouncingMelon || data.type == MultiplayerItemType.Physalis || data.type == MultiplayerItemType.LimeMushroom || data.type == MultiplayerItemType.MarineEye)
        {
            var tp = new AbstractPhysicalObject.AbstractObjectType(data.type.value);
            if (tp.Index >= 0 && Random.value <= data.chance)
                self.abstractRoom.entities.Add(new AbstractConsumable(self.world, tp, null, self.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), -2, -2, null));
        }
        else
            orig(self, placedObj);
    }

    internal static void On_Water_DrawSprites(On.Water.orig_DrawSprites orig, Water self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.room?.game?.Players is List<AbstractCreature> clist)
        {
            for (var i = 0; i < clist.Count; i++)
            {
                if (clist[i] is AbstractCreature cr && PlayerData.TryGetValue(cr, out var props) && props.BlueFaceDuration > 10 && cr.realizedCreature is Player p && !p.isNPC && p.Submersion >= 1f)
                {
                    sLeaser.sprites[1].isVisible = false;
                    break;
                }
            }
        }
    }

    [SuppressMessage(null, "IDE0060"), MethodImpl(MethodImplOptions.NoInlining)]
    public static int DefaultWaterLevel(this Room self, IntVector2 pos) => self.defaultWaterLevel;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float FloatWaterLevel(this Room self, Vector2 pos) => self.FloatWaterLevel(pos.x);
}