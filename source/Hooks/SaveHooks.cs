global using static LBMergedMods.Hooks.SaveHooks;
using System;
using UnityEngine;
using System.Text.RegularExpressions;

namespace LBMergedMods.Hooks;

public static class SaveHooks
{
    internal static AbstractPhysicalObject? On_SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
    {
        try
        {
            var array = Regex.Split(objString, "<oA>");
            if (new AbstractPhysicalObject.AbstractObjectType(array[1]) == AbstractObjectType.BlobPiece)
            {
                var apo = new BlobPiece.AbstractBlobPiece(world, null, WorldCoordinate.FromString(array[2]), EntityID.FromString(array[0]), .5f)
                {
                    unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 4)
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
}