using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DevConsole;
using DevConsole.Commands;
using RWCustom;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using CritType = CreatureTemplate.Type;
using ObjType = AbstractPhysicalObject.AbstractObjectType;
using Random = UnityEngine.Random;
using SandboxUnlock = MultiplayerUnlocks.SandboxUnlockID;

namespace LBMergedMods;

static class DevConsoleCommands
{
    const string K_HINT_PREFIX = "help-";
    static readonly Regex s_entityID = new(@"^ID\.-?\d+\.-?\d+(\.-?\d+)?$");
    static readonly string[] s_allTags = ["Voidsea", "Winter", "Ignorecycle", "TentacleImmune", "Lavasafe", "AlternateForm", "PreCycle", "Night", "Ripple", "AlbinoForm", "DestroyOnAbstract", "DontSave", "RottenMode"];
    static HashSet<ObjType> s_objectTypes = [];
    static HashSet<CritType> s_critterTypes = [];
    static HashSet<SandboxUnlock> s_sandboxUnlocks = [];

    public static WorldCoordinate GetWorldCoordinate(this AbstractRoom self, Vector2 pos) => Custom.MakeWorldCoordinate(Room.StaticGetTilePosition(pos), self.index);

    public static void RegisterDevConsole()
    {
        try
        {
            // Collect copies of types so we can add our own stuff if wanted
            s_objectTypes = [.. AbstractObjectType.M4RItemList];
            s_objectTypes.Remove(AbstractObjectType.MiniFruitSpawner);
            s_critterTypes = [.. CreatureTemplateType.M4RCreatureList.Union([new("Bigrub"), new("Bigworm"), new("SeedBat"), new("JellyLongLegs"), new("AlbinoHazer")])];
            s_sandboxUnlocks = [.. SandboxUnlockID.M4RUnlockList]; // creating a copy
            // Shrembly
            if (ModManager.ActiveMods.Any(x => x.id == "com.rainworldgame.shroudedassembly.plugin"))
            {
                s_critterTypes.Add(new("BabyCroaker"));
                s_critterTypes.Add(new("Gecko"));
                s_critterTypes.Add(new("MaracaSpider"));
                s_objectTypes.Add(new("PureCrystal"));
                s_objectTypes.Add(new("RockFruit"));
                s_objectTypes.Add(new("BigGoldenPearl"));
                s_objectTypes.Add(new("CustomBigPearl"));
                s_sandboxUnlocks.Add(new("BabyCroaker"));
                s_sandboxUnlocks.Add(new("Gecko"));
                s_sandboxUnlocks.Add(new("MaracaSpider"));
                s_sandboxUnlocks.Add(new("PureCrystal"));
                s_sandboxUnlocks.Add(new("RockFruit"));
                s_sandboxUnlocks.Add(new("CustomBigPearl"));
            }
            // Actually register commands
            new CommandBuilder("spawn_lb")
                .Help("spawn_lb [type] [ID?] [args...]")
                .RunGame(SpawnLB)
                .AutoComplete(SpawnLBAutoComplete)
                .Register();
            new CommandBuilder("unlock_lb")
                .Help("unlock_lb enable|disable all|[tokens...]")
                .Run(SandboxLB)
                .AutoComplete(SandboxLBAutoComplete)
                .Register();
            new CommandBuilder("effect_lb")
                .Help("effect_lb [type] [args...]")
                .RunGame(PlayerEffectLB)
                .AutoComplete(PlayerEffectLBAutoComplete)
                .Register();
        }
        catch (Exception e)
        {
            LBMergedModsPlugin.s_logger.LogError(e);
            GameConsole.WriteLine(e.ToString(), Color.red);
        }
    }

    static EntityID ParseExtendedID(string id)
    {
        // From Dev Console
        var outID = EntityID.FromString(id);
        var split = id.Split('.');
        if (split.Length > 3 && int.TryParse(split[3], out var altSeed))
            outID.setAltSeed(altSeed);
        return outID;
    }

    static void SpawnLB(RainWorldGame game, string[] args)
    {
        if (args.Length == 0)
        {
            GameConsole.WriteLine("No object type specified!", Color.red);
            return;
        }
        try
        {
            var arg0 = args[0];
            var objType = new ObjType(arg0);
            var critType = new CritType(arg0);
            var startIndex = 1;
            EntityID ID;
            if (args.Length > 1 && s_entityID.IsMatch(args[1]))
            {
                startIndex = 2;
                ID = ParseExtendedID(args[1]);
            }
            else
                ID = game.GetNewID();
            List<float> foundFloats = [];
            List<bool> foundBools = [];
            List<string> foundStrings = [];
            for (var i = startIndex; i < args.Length; i++)
            {
                var arg = args[i];
                if (float.TryParse(arg, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var f))
                    foundFloats.Add(f);
                else if (bool.TryParse(arg, out var b))
                    foundBools.Add(b);
                else if (!string.IsNullOrEmpty(arg))
                    foundStrings.Add(arg);
            }
            AbstractPhysicalObject entity;
            var pos = GameConsole.TargetPos.Room.GetWorldCoordinate(GameConsole.TargetPos.Pos);
            var spawnDentureFlag = false;
            // Generic cases
            if (s_objectTypes.Contains(objType))
            {
                // Special cases
                if (objType == AbstractObjectType.BlobPiece)
                {
                    var c = Mathf.Lerp(Random.Range(0f, 1f), .5f, Mathf.Pow(Random.value, 2f));
                    if (foundFloats.Count != 0)
                        c = foundFloats[0];
                    entity = new BlobPiece.AbstractBlobPiece(game.world, null, pos, ID, c);
                }
                else if (objType == AbstractObjectType.ThornyStrawberry)
                {
                    var strawb = new AbstractConsumable(game.world, objType, null, pos, ID, -1, -1, null);
                    entity = strawb;
                    if (foundBools.Count > 0 && StrawberryData.TryGetValue(strawb, out var data))
                        data.SpikesRemoved = !foundBools[0];
                }
                else if (objType == AbstractObjectType.RubberBlossom)
                {
                    if (foundBools.Count == 0 || (foundFloats.Count != 1 && foundFloats.Count != 4))
                    {
                        GameConsole.WriteLine("Invalid spawn line for rubber blossom!", Color.red);
                        return;
                    }
                    var open = foundBools[0];
                    var color = StationPlantCol;
                    if (foundFloats.Count > 1)
                        color = new(foundFloats[1], foundFloats[2], foundFloats[3]);
                    var flower = new AbstractConsumable(game.world, objType, null, pos, ID, -1, -1, null);
                    StationPlant.Remove(flower);
                    StationPlant.Add(flower, new(open, (int)foundFloats[0], 999, open, !open)
                    {
                        Open = open,
                        DevSpawn = true,
                        ForceMaxVel = foundFloats[0],
                        ForceColor = color
                    });
                    entity = flower;
                }
                else if (objType.value == "BigGoldenPearl")
                    entity = new DataPearl.AbstractDataPearl(game.world, objType, null, pos, ID, -1, -1, null, new("BigGoldenPearl"));
                else if (objType.value == "CustomBigPearl")
                {
                    var dataPearlType = foundStrings.Count == 0 ? DataPearl.AbstractDataPearl.DataPearlType.Misc : new(foundStrings[0]);
                    if ((int)dataPearlType < 0)
                    {
                        GameConsole.WriteLine("Invalid pearl type!", Color.red);
                        dataPearlType = DataPearl.AbstractDataPearl.DataPearlType.Misc;
                    }
                    entity = new DataPearl.AbstractDataPearl(game.world, objType, null, pos, ID, -1, -1, null, dataPearlType);
                }
                // Generic cases
                else if (AbstractConsumable.IsTypeConsumable(objType))
                    entity = new AbstractConsumable(game.world, objType, null, pos, ID, -1, -1, null);
                else
                    entity = new(game.world, objType, null, pos, ID);
            }
            else if (s_critterTypes.Contains(critType))
            {
                var actualCritType = critType;
                if (critType.value == "Bigrub" || critType.value == "Bigworm")
                    actualCritType = CritType.TubeWorm;
                else if (critType.value == "AlbinoHazer")
                    actualCritType = CritType.Hazer;
                else if (critType.value == "SeedBat")
                    actualCritType = CritType.Fly;
                else if (critType.value == "JellyLongLegs")
                    actualCritType = CritType.BrotherLongLegs;
                var template = StaticWorld.GetCreatureTemplate(actualCritType);
                if (actualCritType == CreatureTemplateType.Denture && GameConsole.TargetPos.Room.realizedRoom is Room rm)
                {
                    var nds = GameConsole.TargetPos.Room.nodes;
                    var basePos = Room.StaticGetTilePosition(GameConsole.TargetPos.Pos);
                    var dist = float.PositiveInfinity;
                    for (var i = 0; i < nds.Length; i++)
                    {
                        var nd = nds[i];
                        var coord = rm.LocalCoordinateOfNode(i) with { abstractNode = i };
                        var tempDist = coord.Tile.FloatDist(basePos);
                        if (tempDist < dist && nd.type == AbstractRoomNode.Type.Den)
                        {
                            dist = tempDist;
                            spawnDentureFlag = true;
                            pos = coord;
                        }
                    }
                }
                var crit = new AbstractCreature(game.world, template, null, pos, ID);
                entity = crit;
                crit.Move(pos);
                // Get tags to apply from arguments
                HashSet<string> tagSet = [];
                var allTags = s_allTags;
                for (var i = 0; i < allTags.Length; i++)
                {
                    var tag = allTags[i];
                    if (args.Any(x => x.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                        tagSet.Add(tag);
                }
                tagSet.UnionWith(args.Where(x => s_allTags.FirstOrDefault(y => y.Equals(x, StringComparison.OrdinalIgnoreCase)) is null));
                // Special tags
                if (tagSet.Remove("DestroyOnAbstract"))
                    crit.destroyOnAbstraction = true;
                if (tagSet.Remove("DontSave"))
                    crit.saveCreature = false;
                // use AlternateForm instead, this is just a shortcut for world files
                /*if (critType.value == "Bigrub")
                    tagSet.Add(foundBools.Count > 0 && foundBools[0] ? "altbigrub" : "bigrub");
                else */
                JellyProperties? jellyProps = null;
                if (critType.value == "Bigworm")
                    tagSet.Add("bigworm");
                else if (critType.value == "AlbinoHazer")
                    tagSet.Add("albinoform");
                else if (critType.value == "SeedBat")
                    tagSet.Add("seedbat");
                else if (critType.value == "JellyLongLegs")
                    Jelly.Add(crit, jellyProps = new() { IsJelly = true, DevSpawnColor = new(146f / 255f, 33f / 255f, 191f / 255f) });// Born is false by default
                var tags = tagSet.ToList();
                // Apply creature-specific tags
                if (critType.value == "JellyLongLegs")
                {
                    if (foundStrings.Count > 0)
                    {
                        // Will work as they all use the same creature classes and aren't realized yet
                        var tp = foundStrings[0];
                        if (tp.StartsWith("Daddy"))
                        {
                            jellyProps!.DevSpawnColor = Color.Lerp(new(146f / 255f, 33f / 255f, 191f / 255f), Color.blue, .15f);
                            crit.creatureTemplate = StaticWorld.GetCreatureTemplate(CritType.DaddyLongLegs);
                        }
                        else if (tp.StartsWith("Terror") && ModManager.DLCShared)
                        {
                            jellyProps!.DevSpawnColor = Color.Lerp(new(146f / 255f, 33f / 255f, 191f / 255f), Color.blue, .3f);
                            crit.creatureTemplate = StaticWorld.GetCreatureTemplate(DLCSharedEnums.CreatureTemplateType.TerrorLongLegs);
                        }
                        if (foundFloats.Count == 3)
                            jellyProps!.DevSpawnColor = new(foundFloats[0], foundFloats[1], foundFloats[2]);
                        else if (foundFloats.Count > 0)
                            GameConsole.WriteLine("Invalid spawn line for jelly long legs!", Color.red);
                    }
                }
                else if (critType.Index >= 0 && StaticWorld.GetCreatureTemplate(critType).TopAncestor().type == CritType.LizardTemplate)
                {
                    // Lizard meanness
                    if (foundFloats.Count > 0)
                        tags.Add("Mean:" + foundFloats[0].ToString(NumberFormatInfo.InvariantInfo));
                    if (foundStrings.Count > 0 && critType != CreatureTemplateType.CommonEel)
                    {
                        var rotStr = foundStrings[0];
                        if (rotStr.StartsWith("RotType:"))
                        {
                            var strInt = rotStr.Replace("RotType:", string.Empty);
                            if (int.TryParse(strInt, out var intVal) && intVal >= 0 && intVal <= 3)
                                tags.Add(rotStr);
                        }
                    }
                }
                // Actually apply the tags
                crit.spawnData = $"{{{string.Join(",", tags)}}}";
                crit.setCustomFlags();
            }
            else if (objType.index >= 0 || critType.index >= 0)
            {
                GameConsole.WriteLine("Use spawn or spawn_raw for non-M4rblelous object types!", Color.red);
                return;
            }
            else
            {
                GameConsole.WriteLine("Invalid object type!", Color.red);
                return;
            }
            // Realize created object, if there is one
            if (entity is not null)
            {
                if (entity is AbstractCreature cr && cr.creatureTemplate.type == CreatureTemplateType.Denture && spawnDentureFlag)
                    GameConsole.TargetPos.Room.entitiesInDens.Add(cr);
                else
                {
                    GameConsole.TargetPos.Room.AddEntity(entity);
                    entity.RealizeInRoom();
                }
            }
        }
        catch (Exception ex)
        {
            LBMergedModsPlugin.s_logger.LogError(ex);
            GameConsole.WriteLine(ex.ToString(), Color.red);
        }
    }

    static IEnumerable<string> SpawnLBAutoComplete(string[] args)
    {
        if (args.Length == 0)
        {
            // All objects and creatures
            foreach (var key in s_objectTypes)
                yield return key.ToString();
            foreach (var key in s_critterTypes)
                yield return key.ToString();
        }
        else
        {
            var arg0 = args[0];
            var objType = new ObjType(arg0);
            var critType = new CritType(arg0);
            var startIndex = 1;
            if (args.Length > 1 && s_entityID.IsMatch(args[1]))
                startIndex = 2;
            // Search for ints and floats
            int foundFloats = 0,
                foundBools = 0;
            for (var i = startIndex; i < args.Length; i++)
            {
                var arg = args[i];
                if (float.TryParse(arg, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out _))
                    ++foundFloats;
                else if (bool.TryParse(arg, out _))
                    ++foundBools;
            }
            if (s_objectTypes.Contains(objType))
            {
                // Show arguments specific to object
                if (objType == AbstractObjectType.BlobPiece && foundFloats == 0)
                    yield return K_HINT_PREFIX + "color: float";
                else if (objType == AbstractObjectType.ThornyStrawberry && foundBools == 0)
                    yield return K_HINT_PREFIX + "thorns: bool";
                else if (objType.value == "CustomBigPearl" && args.Length == 1)
                {
                    var entries = DataPearl.AbstractDataPearl.DataPearlType.values.entries;
                    for (var i = 0; i < entries.Count; i++)
                        yield return entries[i];
                }
                else if (objType == AbstractObjectType.RubberBlossom)
                {
                    if (foundBools == 0)
                        yield return K_HINT_PREFIX + "open: bool";
                    else
                    {
                        var open = false;
                        args.FirstOrDefault(x => bool.TryParse(x, out open));
                        var hint = foundFloats switch
                        {
                            0 when open => K_HINT_PREFIX + "food: int",
                            0 when !open => K_HINT_PREFIX + "boost: float",
                            1 => K_HINT_PREFIX + "r: float",
                            2 => K_HINT_PREFIX + "g: float",
                            3 => K_HINT_PREFIX + "b: float",
                            _ => null
                        };
                        if (hint is not null)
                            yield return hint;
                    }
                }
            }
            else if (s_critterTypes.Contains(critType))
            {
                // Show generic creature tags
                var tags = s_allTags.ToHashSet();
                for (var i = startIndex; i < args.Length; i++)
                    tags.Remove(args[i]);
                foreach (var tag in tags)
                    yield return tag;
                // Show arguments specific to creature
                if (critType.value == "JellyLongLegs")
                {
                    if (!args.Any(x => x.StartsWith("Brother") || x.StartsWith("Daddy") || (ModManager.DLCShared && x.StartsWith("Terror"))))
                    {
                        yield return "Brother";
                        yield return "Daddy";
                        if (ModManager.DLCShared)
                            yield return "Terror";
                    }
                    var hint = foundFloats switch
                    {
                        0 => K_HINT_PREFIX + "r: float",
                        1 => K_HINT_PREFIX + "g: float",
                        2 => K_HINT_PREFIX + "b: float",
                        _ => null
                    };
                    if (hint is not null)
                        yield return hint;
                }
                else if (critType.Index >= 0 && StaticWorld.GetCreatureTemplate(critType).TopAncestor().type == CritType.LizardTemplate)
                {
                    // Lizard-specific
                    if (foundFloats == 0)
                        yield return K_HINT_PREFIX + "mean: float";
                    if (critType != CreatureTemplateType.CommonEel && !args.Any(x => x.StartsWith("RotType:")))
                    {
                        yield return "RotType:1";
                        yield return "RotType:2";
                        yield return "RotType:3";
                    }
                }
                // use AlternateForm instead, this is just a shortcut for world files
                /*else if (critType.value == "Bigrub" && foundBools == 0)
                    yield return K_HINT_PREFIX + "alt: bool";*/
            }
        }
    }

    static void SandboxLB(string[] args)
    {
        if (args.Length == 0)
            GameConsole.WriteLine("Missing arguments!", Color.red);
        else if (args[0] is "enable" or "disable")
        {
            var enable = args[0] == "enable";
            for (var i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg == "ALL")
                {
                    // Unlock all
                    foreach (var unlock in s_sandboxUnlocks)
                    {
                        if (enable)
                        {
                            if (Custom.rainWorld.progression.miscProgressionData.SetTokenCollected(unlock))
                                GameConsole.WriteLine("Unlocked " + unlock.ToString(), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey));
                        }
                        else
                        {
                            if (Custom.rainWorld.progression.miscProgressionData.sandboxTokens.Remove(unlock))
                                GameConsole.WriteLine("Relocked " + unlock.ToString(), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey));
                        }
                    }
                    return;
                }
                else
                {
                    var unlock = new SandboxUnlock(arg);
                    if (s_sandboxUnlocks.Contains(unlock))
                    {
                        if (enable)
                        {
                            if (Custom.rainWorld.progression.miscProgressionData.SetTokenCollected(unlock))
                                GameConsole.WriteLine("Unlocked " + unlock.ToString(), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey));
                        }
                        else
                        {
                            if (Custom.rainWorld.progression.miscProgressionData.sandboxTokens.Remove(unlock))
                                GameConsole.WriteLine("Relocked " + unlock.ToString(), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey));
                        }
                    }
                    else if (unlock.index >= 0)
                        GameConsole.WriteLine("Cannot use unlock '" + unlock.ToString() + "' with unlock_lb!", Color.red);
                    else
                        GameConsole.WriteLine("Invalid unlock token '" + unlock.ToString() + "!", Color.red);
                }
            }
        }
        else
            GameConsole.WriteLine("Invalid argument! Must have 'enable' or 'disable'.", Color.red);
    }

    static IEnumerable<string> SandboxLBAutoComplete(string[] args)
    {
        if (args.Length == 0)
        {
            yield return "enable";
            yield return "disable";
        }
        else
        {
            if (args[0] is "enable" or "disable")
            {
                yield return "ALL";
                foreach (var unlock in s_sandboxUnlocks)
                    yield return unlock.ToString();
            }
        }
        yield break;
    }

    static void PlayerEffectLB(RainWorldGame game, string[] args)
    {
        if (args.Length == 0 || (args[0] != "Reset" && args.Length == 1))
        {
            GameConsole.WriteLine("Missing arguments!", Color.red);
            return;
        }
        PlayerCustomData props;
        switch (args[0])
        {
            case "MarineEye":
                {
                    if (!int.TryParse(args[1], NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var val) || val < 0f)
                        GameConsole.WriteLine("Invalid duration!", Color.red);
                    if (game.Players is List<AbstractCreature> players)
                    {
                        for (var i = 0; i < players.Count; i++)
                        {
                            if (players[i] is AbstractCreature cr && PlayerData.TryGetValue(cr, out props))
                                props.BlueFaceDuration = val;
                        }
                    }
                }
                break;
            case "BouncingMelon":
                {
                    if (!int.TryParse(args[1], NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var val) || val < 0f)
                        GameConsole.WriteLine("Invalid duration!", Color.red);
                    if (game.Players is List<AbstractCreature> players)
                    {
                        for (var i = 0; i < players.Count; i++)
                        {
                            if (players[i] is AbstractCreature cr && PlayerData.TryGetValue(cr, out props))
                                props.BounceEffectDuration = val;
                        }
                    }
                }
                break;
            case "DarkGrub":
                {
                    if (!int.TryParse(args[1], NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var val) || val < 0f)
                        GameConsole.WriteLine("Invalid duration!", Color.red);
                    if (game.Players is List<AbstractCreature> players)
                    {
                        for (var i = 0; i < players.Count; i++)
                        {
                            if (players[i] is AbstractCreature cr && PlayerData.TryGetValue(cr, out props))
                                props.GrubVisionDuration = val;
                        }
                    }
                }
                break;
            case "Reset":
                {
                    if (game.Players is List<AbstractCreature> players)
                    {
                        for (var i = 0; i < players.Count; i++)
                        {
                            if (players[i] is AbstractCreature cr && PlayerData.TryGetValue(cr, out props))
                            {
                                props.GrubVisionDuration = 0;
                                props.BounceEffectDuration = 0;
                                props.BlueFaceDuration = 0;
                            }
                        }
                    }
                }
                break;
        }
    }

    static IEnumerable<string> PlayerEffectLBAutoComplete(string[] args)
    {
        if (args.Length == 0)
        {
            yield return "MarineEye";
            yield return "BouncingMelon";
            yield return "DarkGrub";
            yield return "Reset";
        }
        else if (args[0] != "Reset")
            yield return K_HINT_PREFIX + "duration: int";
        yield break;
    }
}