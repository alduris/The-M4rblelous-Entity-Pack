global using static LBMergedMods.Hooks.SaveHooks;
using MonoMod.Cil;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace LBMergedMods.Hooks;

public static class SaveHooks
{
    public static ConditionalWeakTable<DeathPersistentSaveData, DeathPersistentScoreData> ScoreData = new();

    internal static void On_DeathPersistentSaveData_FromString(On.DeathPersistentSaveData.orig_FromString orig, DeathPersistentSaveData self, string s)
    {
        orig(self, s);
        if (ScoreData.TryGetValue(self, out var data))
        {
            var unrec = self.unrecognizedSaveStrings;
            bool flag1 = false, flag2 = false, flag3 = false;
            for (var i = 0; i < unrec.Count; i++)
            {
                var str = unrec[i];
                if (str.StartsWith("M4R_CollectedScoreTokens<scm4r>", StringComparison.OrdinalIgnoreCase))
                {
                    data.CollectedTokens = [.. str.Remove(0, 31).Split(["<scm4r>"], StringSplitOptions.RemoveEmptyEntries)];
                    flag1 = true;
                }
                else if (str.StartsWith("M4R_ScoreTokensBonus<scm4r>", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(str.Remove(0, 27), out data.Score);
                    flag2 = true;
                }
                else if (str.Equals("M4R_LimeMushroomMessage", StringComparison.OrdinalIgnoreCase))
                {
                    data.LimeMushroomMessage = true;
                    flag3 = true;
                }
                if (flag1 && flag2 && flag3)
                    return;
            }
        }
    }

    internal static void On_DeathPersistentSaveData_ctor(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, SlugcatStats.Name slugcat)
    {
        orig(self, slugcat);
        if (!ScoreData.TryGetValue(self, out _))
            ScoreData.Add(self, new());
    }

    internal static string On_DeathPersistentSaveData_SaveToString(On.DeathPersistentSaveData.orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
    {
        if (ScoreData.TryGetValue(self, out var data))
        {
            var strs = self.unrecognizedSaveStrings;
            int i = -1, k = -1;
            var limeFlag = false;
            for (var j = 0; j < strs.Count; j++)
            {
                var str = strs[j];
                if (str.StartsWith("M4R_CollectedScoreTokens<scm4r>", StringComparison.OrdinalIgnoreCase))
                    i = j;
                else if (str.StartsWith("M4R_ScoreTokensBonus<scm4r>", StringComparison.OrdinalIgnoreCase))
                    k = j;
                else if (str.Equals("M4R_LimeMushroomMessage", StringComparison.OrdinalIgnoreCase))
                    limeFlag = true;
            }
            var sb = new StringBuilder("M4R_CollectedScoreTokens");
            foreach (var id in data.CollectedTokens)
                sb.Append("<scm4r>").Append(id);
            if (i != -1)
                strs[i] = sb.ToString();
            else
                strs.Add(sb.ToString());
            if (k != -1)
                strs[k] = "M4R_ScoreTokensBonus<scm4r>" + data.Score;
            else
                strs.Add("M4R_ScoreTokensBonus<scm4r>" + data.Score);
            if (!limeFlag && data.LimeMushroomMessage)
                strs.Add("M4R_LimeMushroomMessage");
        }
        return orig(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);
    }

    internal static void IL_SaveState_AbstractPhysicalObjectFromString(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<AbstractPhysicalObject.AbstractObjectType>("VultureMask"))
         && c.TryGotoNext(MoveType.After,
            x => x.MatchCall<ModManager>("get_DLCShared")))
            c.EmitDelegate((bool _) => true);
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SaveState.AbstractPhysicalObjectFromString!");
    }

    internal static AbstractPhysicalObject? On_SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
    {
        try
        {
            var array = objString.Split(["<oA>"], StringSplitOptions.None);
            if (new AbstractPhysicalObject.AbstractObjectType(array[1]) == AbstractObjectType.BlobPiece)
            {
                var rippleLayer = 0;
                EntityID iD;
                var ar0 = array[0];
                if (ar0.Contains("<oB>"))
                {
                    var array2 = ar0.Split(["<oB>"], StringSplitOptions.None);
                    iD = EntityID.FromString(array2[0]);
                    int.TryParse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture, out rippleLayer);
                }
                else
                    iD = EntityID.FromString(ar0);
                var apo = new BlobPiece.AbstractBlobPiece(world, null, WorldCoordinate.FromString(array[2]), iD, .5f)
                {
                    unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4),
                    rippleLayer = rippleLayer
                };
                float.TryParse(array[3], out apo.Color);
                return apo;
            }
            var res = orig(world, objString);
            if (res is AbstractConsumable cons && StrawberryData.TryGetValue(cons, out var data) && cons.unrecognizedAttributes is string[] unrec)
            {
                var index = Array.IndexOf(unrec, "M4R_ThStr");
                if (index != -1 && index < unrec.Length - 1)
                    data.SpikesRemoved = unrec[index + 1] == "Y";
            }
            return res;
        }
        catch (Exception ex)
        {
            if (RainWorld.ShowLogs)
                Debug.Log("[EXCEPTION] AbstractPhysicalObjectFromString: " + objString + " -- " + ex.Message + " -- " + ex.StackTrace);
            return null;
        }
    }

    internal static string On_SaveState_SetCustomData_AbstractPhysicalObject_string(On.SaveState.orig_SetCustomData_AbstractPhysicalObject_string orig, AbstractPhysicalObject apo, string baseString)
    {
        if (apo is AbstractConsumable cons && StrawberryData.TryGetValue(cons, out var data))
        {
            if (cons.unrecognizedAttributes is not string[] unrec)
                cons.unrecognizedAttributes = ["M4R_ThStr", data.SpikesRemoved ? "Y" : "N"];
            else
            {
                var index = Array.IndexOf(unrec, "M4R_ThStr");
                if (index == -1)
                {
                    Array.Resize(ref unrec, unrec.Length + 2);
                    unrec[unrec.Length - 2] = "M4R_ThStr";
                    unrec[unrec.Length - 1] = data.SpikesRemoved ? "Y" : "N";
                }
                else if (index == unrec.Length - 1)
                {
                    Array.Resize(ref unrec, unrec.Length + 1);
                    unrec[unrec.Length - 1] = data.SpikesRemoved ? "Y" : "N";
                }
                else
                    unrec[index + 1] = data.SpikesRemoved ? "Y" : "N";
            }
        }
        return orig(apo, baseString);
    }

    public static bool GetScoreTokenCollected(this DeathPersistentSaveData self, string id) => ScoreData.TryGetValue(self, out var data) && data.CollectedTokens.Contains(id);

    public static bool SetScoreTokenCollected(this DeathPersistentSaveData self, string id)
    {
        if (ScoreData.TryGetValue(self, out var data))
        {
            if (data.CollectedTokens.Contains(id))
                return false;
            data.CollectedTokens.Add(id);
            return true;
        }
        return false;
    }
}