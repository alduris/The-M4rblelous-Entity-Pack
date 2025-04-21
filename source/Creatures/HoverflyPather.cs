using System;
using RWCustom;
using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
public class HoverflyPather(ArtificialIntelligence AI, World world, AbstractCreature creature) : PathFinder(AI, world, creature)
{
    public override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
	{
		var result = base.CheckConnectionCost(start, goal, connection, followingPath);
		if (connection.destinationCoord.TileDefined) {
			if (destination.TileDefined && Custom.ManhattanDistance(connection.destinationCoord, destination) > 6)
				result.resistance += Mathf.Clamp(10f - realizedRoom.aimap.getTerrainProximity(connection.destinationCoord), 0f, 10f) * 10f;
			if (creature is AbstractCreature c && (c.realizedCreature is not Hoverfly f || !f.safariControlled) && c.Room?.realizedRoom?.aimap is AImap mp && mp.getAItile(connection.destinationCoord) is AItile t && (t.narrowSpace || mp.getTerrainProximity(connection.destinationCoord) < 4 || t.AnyWater))
			{
                result.resistance += 100f;
				result.legality = PathCost.Legality.Unwanted;
            }
		}
		return result;
	}

	public override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		if (InThisRealizedRoom(cell.worldCoordinate))
		{
			if (lookingForImpossiblePath && !cell.reachable)
				return costToGoal;
			return new PathCost(Custom.LerpMap(creaturePos.Tile.FloatDist(cell.worldCoordinate.Tile), 20f, 50f, costToGoal.resistance, creaturePos.Tile.FloatDist(cell.worldCoordinate.Tile) * costToGoal.resistance), costToGoal.legality);
		}
		return costToGoal;
	}

	public virtual MovementConnection FollowPath(WorldCoordinate originPos, bool actuallyFollowingThisPath)
	{
		if (originPos.TileDefined)
		{
			originPos.x = Math.Min(Math.Max(originPos.x, 0), realizedRoom.TileWidth - 1);
			originPos.y = Math.Min(Math.Max(originPos.y, 0), realizedRoom.TileHeight - 1);
		}
		if (CoordinateCost(originPos).Allowed)
		{
			var pathingCell = PathingCellAtWorldCoordinate(originPos);
			if (pathingCell is not null)
			{
				if (!pathingCell.reachable || !pathingCell.possibleToGetBackFrom)
					OutOfElement();
				MovementConnection movementConnection = default;
				var pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
				int num = -acceptablePathAge, num2 = 0;
				var legality = PathCost.Legality.Unallowed;
				while (true)
				{
					var movementConnection2 = ConnectionAtCoordinate(true, originPos, num2);
					num2++;
					if (movementConnection2 == default)
						break;
					if (movementConnection2.destinationCoord.TileDefined && !Custom.InsideRect(movementConnection2.DestTile, coveredArea))
						continue;
					var pathingCell2 = PathingCellAtWorldCoordinate(movementConnection2.destinationCoord);
					var pathCost2 = CheckConnectionCost(pathingCell, pathingCell2, movementConnection2, true);
					if (!pathingCell2.possibleToGetBackFrom && !walkPastPointOfNoReturn)
						pathCost2.legality = PathCost.Legality.Unallowed;
					var pathCost3 = pathingCell2.costToGoal + pathCost2;
					if (movementConnection2.destinationCoord.Tile == destination.Tile)
						pathCost3.resistance = 0f;
					if (pathCost2.legality < legality)
					{
						movementConnection = movementConnection2;
						legality = pathCost2.legality;
						num = pathingCell2.generation;
						pathCost = pathCost3;
					}
					else if (pathCost2.legality == legality)
					{
						if (pathingCell2.generation > num)
						{
							movementConnection = movementConnection2;
							legality = pathCost2.legality;
							num = pathingCell2.generation;
							pathCost = pathCost3;
						}
						else if (pathingCell2.generation == num && pathCost3 <= pathCost)
						{
							movementConnection = movementConnection2;
							legality = pathCost2.legality;
							num = pathingCell2.generation;
							pathCost = pathCost3;
						}
					}
				}
				if (legality <= PathCost.Legality.Unwanted)
				{
					if (actuallyFollowingThisPath)
					{
						if (movementConnection != default && !movementConnection.destinationCoord.TileDefined)
							LeavingRoom();
						creatureFollowingGeneration = num;
					}
					return movementConnection;
				}
			}
		}
		return default;
	}
}
