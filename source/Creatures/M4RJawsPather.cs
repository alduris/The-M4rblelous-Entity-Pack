using System.Collections.Generic;
using RWCustom;
using System;

namespace LBMergedMods.Creatures;

public class M4RJawsPather : PathFinder
{
	public const int MAX_OFFLIMITS_CONS = 3, SAVED_CONNECTIONS = 100;
    public List<MovementConnection> PastConnections = [];

	public virtual M4RJaws Ow => (creature.realizedCreature as M4RJaws)!;

	public M4RJawsPather(ArtificialIntelligence AI, World world, AbstractCreature creature) : base(AI, world, creature) => stepsPerFrame = 20;

	public override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
	{
		if (connection.type == MovementConnection.MovementType.SideHighway && connection.startCoord.CompareDisregardingTile(creature.pos))
			return base.CheckConnectionCost(start, goal, connection, followingPath) + new PathCost(0f, PathCost.Legality.Unwanted);
		return base.CheckConnectionCost(start, goal, connection, followingPath);
	}

	public override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		if (InThisRealizedRoom(cell.worldCoordinate))
		{
            var crPos = creaturePos;
            if (lookingForImpossiblePath && !cell.reachable)
				return new(costToGoal.resistance + (Math.Abs(cell.worldCoordinate.x - crPos.x) + Math.Abs(cell.worldCoordinate.y - crPos.y)) * 1.5f + (Math.Abs(cell.worldCoordinate.x - destination.x) + Math.Abs(cell.worldCoordinate.y - destination.y)) * .75f, costToGoal.legality);
			return new(costToGoal.resistance + crPos.Tile.FloatDist(cell.worldCoordinate.Tile), costToGoal.legality);
		}
		return costToGoal;
	}

    public virtual MovementConnection FollowPath(WorldCoordinate originPos, bool actuallyFollowingThisPath)
	{
		if (originPos.x > 4 && originPos.x < realizedRoom.TileWidth - 4 && AI.stuckTracker.Utility() < .5f && (currentlyFollowingDestination.room != originPos.room || (currentlyFollowingDestination.NodeDefined && !currentlyFollowingDestination.TileDefined)))
		{
			var movementConnection = PathWithExits(originPos, true);
			if (movementConnection != default)
				return movementConnection;
		}
		var gen1 = int.MinValue;
		var pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
		var dest = originPos;
		if (!originPos.TileDefined && !originPos.NodeDefined)
			return default;
		var coord = new WorldCoordinate(originPos.room, originPos.x, originPos.y, originPos.abstractNode);
		if (originPos.TileDefined)
		{
			coord.Tile = Custom.RestrictInRect(coord.Tile, coveredArea);
			if (coord.TileDefined && (coord.x == 0 || coord.y == 0 || coord.x == realizedRoom.TileWidth - 1 || coord.y == realizedRoom.TileHeight - 1))
			{
				var exitIndex = -1;
				var exitTileDist = int.MaxValue;
				var borderExits = realizedRoom.borderExits;
                for (var i = 0; i < borderExits.Length; i++)
				{
					var border = borderExits[i];
					if (borderExits[i].type != AbstractRoomNode.Type.SideExit)
						continue;
					var tls = border.borderTiles;
                    for (var j = 0; j < tls.Length; j++)
					{
						var tl = tls[j];
						if (Custom.ManhattanDistance(tl, coord.Tile) < exitTileDist)
						{
							exitIndex = i;
							exitTileDist = Custom.ManhattanDistance(tl, coord.Tile);
							if (exitTileDist < 1)
								break;
						}
					}
				}
				if (exitIndex > -1)
				{
					var gen2 = -1;
					var borderTls = borderExits[exitIndex].borderTiles;
                    for (var k = 0; k < borderTls.Length; k++)
					{
						var tl = borderTls[k];
                        if (realizedRoom.aimap.TileAccessibleToCreature(tl, creatureType) && realizedRoom.GetWorldCoordinate(tl) is WorldCoordinate c && PathingCellAtWorldCoordinate(c) is PathingCell cell && cell.generation >= gen2 && !cell.inCheckNextList)
						{
							coord = c;
							gen2 = cell.generation;
						}
					}
				}
			}
		}
		/*if (actuallyFollowingThisPath)
			debugDrawer?.Blink(coord);*/
		var pathingCell = PathingCellAtWorldCoordinate(coord);
		if (pathingCell is not null)
		{
			if (!pathingCell.reachable || !pathingCell.possibleToGetBackFrom)
				OutOfElement(coord);
			MovementConnection moveCon = default;
			var cost = new PathCost(0f, PathCost.Legality.Unallowed);
			var negAge = -acceptablePathAge;
			var legality = PathCost.Legality.Unallowed;
			var negAge2 = -acceptablePathAge;
			var resist = float.MaxValue;
			var conIndex = 0;
			for (; ; )
			{
				var con2 = ConnectionAtCoordinate(true, coord, conIndex);
                ++conIndex;
				if (con2 == default)
					break;
				if (con2.destinationCoord.TileDefined && !Custom.InsideRect(con2.DestTile, coveredArea))
					continue;
				var pathingCell2 = PathingCellAtWorldCoordinate(con2.destinationCoord);
				var cost2 = CheckConnectionCost(pathingCell, pathingCell2, con2, true);
				if (!pathingCell2.possibleToGetBackFrom && !walkPastPointOfNoReturn)
					cost2.legality = PathCost.Legality.Unallowed;
				var cost3 = pathingCell2.costToGoal + cost2;
				if (con2.destinationCoord.TileDefined && destination.TileDefined && con2.destinationCoord.Tile == destination.Tile)
					cost3.resistance = 0f;
				else if (realizedRoom.IsPositionInsideBoundries(creature.pos.Tile) && (!actuallyFollowingThisPath || ConnectionAlreadyFollowedSeveralTimes(con2)))
					cost2 += new PathCost(100f, PathCost.Legality.Unwanted);
				if (con2.type == MovementConnection.MovementType.OutsideRoom && !(AI as M4RJawsAI)!.EnteredRoom)
					cost2 += new PathCost(0f, PathCost.Legality.Unallowed);
				if (pathingCell2.generation > negAge2)
				{
					negAge2 = pathingCell2.generation;
					resist = cost3.resistance;
				}
				else if (pathingCell2.generation == negAge2 && cost3.resistance < resist)
					resist = cost3.resistance;
				if (cost2.legality < legality)
				{
					moveCon = con2;
					legality = cost2.legality;
					negAge = pathingCell2.generation;
					cost = cost3;
				}
				else if (cost2.legality == legality)
				{
					if (pathingCell2.generation > negAge)
					{
						moveCon = con2;
						legality = cost2.legality;
						negAge = pathingCell2.generation;
						cost = cost3;
					}
					else if (pathingCell2.generation == negAge && cost3 <= cost)
					{
						moveCon = con2;
						legality = cost2.legality;
						negAge = pathingCell2.generation;
						cost = cost3;
					}
				}
			}
			/*if (world.game.devToolsActive && Input.GetKey("u") && actuallyFollowingThisPath)
				Custom.Log($"{coord}, chosen move:{moveCon}");*/
			if (legality <= PathCost.Legality.Unwanted)
			{
				if (actuallyFollowingThisPath)
				{
					creatureFollowingGeneration = negAge;
					if (moveCon != default && moveCon.type == MovementConnection.MovementType.ShortCut && realizedRoom.shortcutData(moveCon.StartTile).shortCutType == ShortcutData.Type.RoomExit)
						LeavingRoom();
				}
				if (actuallyFollowingThisPath && moveCon != default && moveCon.type == MovementConnection.MovementType.OutsideRoom && !moveCon.destinationCoord.TileDefined && Ow.shortcutDelay < 1)
				{
					if (!Custom.InsideRect(originPos.Tile, new IntRect(-30, -30, realizedRoom.TileWidth + 30, realizedRoom.TileHeight + 30)))
					{
						var sideAccessNodes = world.sideAccessNodes;
						for (var l = 0; l < sideAccessNodes.Length; l++)
						{
							var crd = sideAccessNodes[l];
							var pathCell = PathingCellAtWorldCoordinate(crd);
							if (pathCell.generation > gen1)
							{
								gen1 = pathCell.generation;
								pathCost = pathCell.costToGoal;
								dest = crd;
							}
							else if (pathCell.generation == gen1 && pathCell.costToGoal < pathCost)
							{
								pathCost = pathCell.costToGoal;
								dest = crd;
							}
							if (crd.CompareDisregardingTile(destination))
							{
								dest = crd;
								break;
							}
						}
						if (!dest.CompareDisregardingTile(moveCon.destinationCoord))
						{
							realizedRoom.game.shortcuts.CreatureTakeFlight(Ow, AbstractRoomNode.Type.SideExit, moveCon.destinationCoord, dest);
							if (dest.room != creaturePos.room)
								LeavingRoom();
						}
						return default;
					}
					var borderDir = new IntVector2(0, 1);
					if (moveCon.startCoord.x == 0)
						borderDir = new(-1, 0);
					else if (moveCon.startCoord.x == realizedRoom.TileWidth - 1)
						borderDir = new(1, 0);
					else if (moveCon.startCoord.y == 0)
						borderDir = new(0, -1);
					return new(MovementConnection.MovementType.Standard, originPos, new(originPos.room, originPos.x + borderDir.x * 10, originPos.y + borderDir.y * 10, originPos.abstractNode), 1);
				}
				var pastCons = PastConnections;
                if (actuallyFollowingThisPath && (pastCons.Count == 0 || moveCon != pastCons[0]))
                    pastCons.Insert(0, moveCon);
				if (pastCons.Count > SAVED_CONNECTIONS)
                    pastCons.RemoveAt(SAVED_CONNECTIONS);
				return moveCon;
			}
		}
		return default;
	}

	public virtual bool ConnectionAlreadyFollowedSeveralTimes(MovementConnection connection)
	{
		var num = 0;
		var pastCons = PastConnections;
		for (var i = 0; i < pastCons.Count; i++)
		{
			if (pastCons[i] == connection)
			{
                ++num;
				if (num >= MAX_OFFLIMITS_CONS)
					return true;
			}
		}
		return false;
	}
}
