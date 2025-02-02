global using static LBMergedMods.Hooks.CollectiblesTrackerHooks;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using MonoMod.Cil;
using MoreSlugcats;
using Mono.Cecil.Cil;
using System;

namespace LBMergedMods.Hooks;

public static class CollectiblesTrackerHooks
{
    public static ConditionalWeakTable<CollectiblesTracker.SaveGameData, HashSet<string>> TrackerScoreData = new();

    internal static void IL_CollectiblesTracker_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (!c.TryGotoNext(MoveType.After,
            s_MatchLdfld_CollectiblesTracker_sprites,
            s_MatchLdarg_0,
            s_MatchLdfld_CollectiblesTracker_displayRegions,
            s_MatchLdloc_OutLoc1))
        {
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CollectiblesTracker.ctor (part 1)!");
            return;
        }
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_ModManager_MSC))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_S, il.Method.Parameters[5])
             .Emit(OpCodes.Ldloc, il.Body.Variables[s_loc1])
             .EmitDelegate((bool flag, CollectiblesTracker self, SlugcatStats.Name saveSlot, int l) =>
             {
                 var rw = self.menu.manager.rainWorld;
                 if (RegionScoreData.TryGetValue(rw, out var data) && TrackerScoreData.TryGetValue(self.collectionData, out var list))
                 {
                     var rg = self.displayRegions[l];
                     var tks = data.RegionScoreTokens[rg];
                     var access = data.RegionScoreTokensInaccessibility[rg];
                     var spriteColors = self.spriteColors[rg];
                     var sprites = self.sprites[rg];
                     for (var n = 0; n < tks.Count; n++)
                     {
                         if (!access[n].Contains(saveSlot))
                         {
                             spriteColors.Add(ScoreToken.TokenColor);
                             sprites.Add(new(list.Contains(tks[n]) ? "ctm4rOn" : "ctm4rOff"));
                         }
                     }
                 }
                 return flag;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CollectiblesTracker.ctor (part 2)!");
    }

    internal static CollectiblesTracker.SaveGameData? On_CollectiblesTracker_MineForSaveData(On.MoreSlugcats.CollectiblesTracker.orig_MineForSaveData orig, CollectiblesTracker self, ProcessManager manager, SlugcatStats.Name slugcat)
    {
        var res = orig(self, manager, slugcat);
        if (res is not null)
        {
            var regionsFile = AssetManager.ResolveFilePath(Path.Combine("world", "regions.txt"));
            if (File.Exists(regionsFile))
            {
                var rw = manager.rainWorld;
                var regions = File.ReadAllLines(regionsFile);
                var prog = rw.progression;
                var saveState = prog.currentSaveState;
                saveState ??= prog.starvedSaveState;
                var deathData = saveState?.deathPersistentSaveData;
                var rightData = deathData is not null && saveState!.saveStateNumber == slugcat;
                HashSet<string>? data = null;
                if (rightData)
                {
                    if (!TrackerScoreData.TryGetValue(res, out data))
                        TrackerScoreData.Add(res, data = []);
                    else
                        data.Clear();
                }
                if (ScoreTokenData.DataChanged && RegionScoreData.TryGetValue(rw, out _))
                {
                    RegionScoreData.Remove(rw);
                    ScoreTokenData.DataChanged = false;
                }
                if (!RegionScoreData.TryGetValue(rw, out var tksData))
                {
                    RegionScoreData.Add(rw, tksData = new());
                    for (var i = 0; i < regions.Length; i++)
                    {
                        var acronym = regions[i].ToLowerInvariant();
                        var tks = tksData.RegionScoreTokens[acronym] = [];
                        var access = tksData.RegionScoreTokensInaccessibility[acronym] = [];
                        var path = AssetManager.ResolveFilePath(Path.Combine("world", acronym, "mpkscoretokens.txt"));
                        if (File.Exists(path))
                        {
                            var ar1 = File.ReadAllLines(path);
                            for (var k = 0; k < ar1.Length; k++)
                            {
                                var ar2 = ar1[k].Split('~');
                                if (ar2.Length > 0)
                                {
                                    var ar20 = ar2[0];
                                    tks.Add(ar20);
                                    if (ar2.Length > 1)
                                    {
                                        var list = new List<SlugcatStats.Name>();
                                        var ar3 = ar2[1].Split(["<m4r>"], StringSplitOptions.RemoveEmptyEntries);
                                        for (var l = 0; l < ar3.Length; l++)
                                            list.Add(new(ar3[l]));
                                        access.Add(list);
                                    }
                                    else
                                        access.Add([]);
                                    if (rightData && deathData!.GetScoreTokenCollected(ar20))
                                        data!.Add(ar20);
                                }
                            }
                        }
                    }
                }
                else if (rightData)
                {
                    var entries = tksData.RegionScoreTokens;
                    for (var k = 0; k < regions.Length; k++)
                    {
                        if (entries.TryGetValue(regions[k].ToLowerInvariant(), out var list))
                        {
                            for (var i = 0; i < list.Count; i++)
                            {
                                var entry = list[i];
                                if (deathData!.GetScoreTokenCollected(entry))
                                    data!.Add(entry);
                            }
                        }
                    }
                }
            }
        }
        return res;
    }
}