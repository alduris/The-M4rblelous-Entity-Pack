using System;

namespace LBMergedMods.Creatures;

public class BlizzorAbstractAI : MirosBirdAbstractAI
{
    public BlizzorAbstractAI(World world, AbstractCreature parent) : base(world, parent)
    {
        var allowNds = allowedNodes;
        allowNds.Clear();
        var mytp = parent.creatureTemplate.type;
        var firstRoomIndex = world.firstRoomIndex;
        var totRooms = firstRoomIndex + (world.NumberOfRooms - 1);
        for (var i = firstRoomIndex; i < totRooms; i++)
        {
            var rm = world.GetAbstractRoom(i);
            var attr = rm.AttractionForCreature(mytp);
            if (attr == AbstractRoom.CreatureRoomAttraction.Like || attr == AbstractRoom.CreatureRoomAttraction.Stay || Array.IndexOf(UGLYHARDCODEDALLOWEDROOMS, rm.name) >= 0)
            {
                var nodes = rm.nodes;
                for (var k = 0; k < nodes.Length; k++)
                {
                    ref readonly var node = ref nodes[k];
                    if (node.type == AbstractRoomNode.Type.SideExit && node.entranceWidth >= 5)
                    {
                        var nd = new WorldCoordinate(rm.index, -1, -1, k);
                        if (!allowNds.Contains(nd))
                            allowNds.Add(nd);
                    }
                }
            }
        }
    }
}