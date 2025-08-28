using RWCustom;
using System.Collections.Generic;
using UnityEngine;

namespace LBMergedMods.Creatures;

public class FoodItemTracker(ArtificialIntelligence AI, int maxRememberedItems, float persistanceBias, float sureToGetItemDistance, float sureToLoseItemDistance) : AIModule(AI)
{
    public class TrackedItem(FoodItemTracker owner, FoodItemRepresentation itemRep)
    {
        public FoodItemTracker Owner = owner;
        public FoodItemRepresentation ItemRep = itemRep;
        public int UnreachableCounter, AtPositionButCantSeeCounter;
        public WorldCoordinate LastBestGuessPos;

        public virtual float Reachable
        {
            get
            {
                var guessPos = ItemRep.BestGuessForPosition();
                var crit = Owner.AI.creature;
                var critPos = crit.pos;
                if (guessPos.room == critPos.room && crit.Room.realizedRoom is Room rm && rm.GetTile(guessPos).Solid)
                    return 0f;
                var giveUp = Owner.GiveUpOnUnreachableItem;
                var num = giveUp >= 0 ? Mathf.InverseLerp(giveUp, 0f, UnreachableCounter) * Mathf.InverseLerp(200f, 100f, AtPositionButCantSeeCounter) : 1f;
                var world = crit.world;
                var num2 = world.GetAbstractRoom(guessPos).AttractionValueForCreature(crit);
                if (num2 < world.GetAbstractRoom(critPos).AttractionValueForCreature(crit))
                    num *= .5f;
                return num;
            }
        }

        public virtual void Update()
        {
            var coord = ItemRep.BestGuessForPosition();
            var AI = Owner.AI;
            if (AI.pathFinder is PathFinder pf && pf.DoneMappingAccessibility)
            {
                var flag = pf.CoordinateReachableAndGetbackable(coord);
                if (!flag)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        if (flag)
                            break;
                        flag = pf.CoordinateReachableAndGetbackable(coord + Custom.fourDirections[i]);
                    }
                }
                if (flag)
                    UnreachableCounter = 0;
                else
                    ++UnreachableCounter;
            }
            var crit = AI.creature;
            var critPos = crit.pos;
            var critRoom = coord.room;
            var coordTile = coord.Tile;
            if (LastBestGuessPos == coord && critPos.room == critRoom && critPos.Tile.FloatDist(coordTile) < 5f && AI.pathFinder is PathFinder p && p.GetDestination is WorldCoordinate wc && wc.room == critRoom && wc.Tile.FloatDist(coordTile) < 5f && crit.Room.realizedRoom is Room rm && rm.VisualContact(critPos, coord))
                AtPositionButCantSeeCounter += 5;
            else
                --AtPositionButCantSeeCounter;
            AtPositionButCantSeeCounter = Custom.IntClamp(AtPositionButCantSeeCounter, 0, 200);
            LastBestGuessPos = coord;
        }

        public virtual bool PathFinderCanGetToPrey()
        {
            var guessPos = ItemRep.BestGuessForPosition();
            var pf = Owner.AI.pathFinder;
            WorldCoordinate cusDir;
            int i;
            for (i = 0; i < 9; i++)
            {
                cusDir = WorldCoordinate.AddIntVector(guessPos, Custom.eightDirectionsAndZero[i]);
                if (pf.CoordinateReachable(cusDir) && pf.CoordinatePossibleToGetBackFrom(cusDir))
                    return true;
            }
            for (i = 0; i < 4; i++)
            {
                cusDir = WorldCoordinate.AddIntVector(guessPos, Custom.fourDirections[i] * 2);
                if (pf.CoordinateReachable(cusDir) && pf.CoordinatePossibleToGetBackFrom(cusDir))
                    return true;
            }
            return false;
        }

        public virtual float Attractiveness()
        {
            var AI = Owner.AI;
            var crit = AI.creature;
            var critPos = crit.pos;
            var num = 1f;
            var guessPos = ItemRep.BestGuessForPosition();
            var f = Mathf.Lerp(Mathf.Pow(Owner.DistanceEstimation(critPos, guessPos), 1.5f), 1f, .5f);
            if (AI.pathFinder is PathFinder pf)
            {
                if (!pf.CoordinateReachable(guessPos))
                    num /= 2f;
                if (!pf.CoordinatePossibleToGetBackFrom(guessPos))
                    num /= 2f;
                if (!PathFinderCanGetToPrey())
                    num /= 2f;
            }
            num *= ItemRep.EstimatedChanceOfFinding;
            num *= Reachable;
            if (guessPos.room != critPos.room)
                num *= Mathf.InverseLerp(0f, .5f, crit.world.GetAbstractRoom(guessPos).AttractionValueForCreature(crit));
            num /= f;
            var giveUpGh = Owner.GiveUpOnGhostGeneration;
            return num * Mathf.InverseLerp(giveUpGh, giveUpGh / 2, ItemRep.LowestGenerationAvailable);
        }
    }

    public int MaxRememberedItems = maxRememberedItems;
    public float PersistanceBias = persistanceBias, SureToGetItemDistance = sureToGetItemDistance, SureToLoseItemDistance = sureToLoseItemDistance, FrustrationSpeed = .0125f;
    public List<TrackedItem> Items = [];
    public TrackedItem? CurrentItem;
    public float Frustration;
    public int GiveUpOnUnreachableItem = 400, GiveUpOnGhostGeneration = 50;

    public virtual AImap? AIMap => AI.creature.realizedCreature.room?.aimap;

    public virtual int TotalTrackedItems => Items.Count;

    public virtual FoodItemRepresentation? MostAttractiveItem => CurrentItem?.ItemRep;

    public virtual FoodItemRepresentation GetTrackedItem(int index) => Items[index].ItemRep;

    public override float Utility()
    {
        if (CurrentItem is not TrackedItem item)
            return 0f;
        var crit = AI.creature;
        var absAI = crit.abstractAI;
        var guessPos = item.ItemRep.BestGuessForPosition();
        if (absAI.WantToMigrate && guessPos.room != absAI.MigrationDestination.room && guessPos.room != crit.pos.room)
            return 0f;
        var num = DistanceEstimation(crit.pos, guessPos);
        num = Mathf.Lerp(1f - Mathf.InverseLerp(SureToGetItemDistance, SureToLoseItemDistance, num), Mathf.Lerp(SureToGetItemDistance, SureToLoseItemDistance, .25f) / num, .5f);
        var rep = item.ItemRep;
        return Mathf.Min(num, rep.EstimatedChanceOfFinding * Mathf.InverseLerp(GiveUpOnGhostGeneration, GiveUpOnGhostGeneration / 2, rep.LowestGenerationAvailable)) * item.Reachable;
    }

    public virtual void AddItem(FoodItemRepresentation obj)
    {
        var items = Items;
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i].ItemRep == obj)
                return;
        }
        items.Add(new(this, obj));
        if (items.Count > MaxRememberedItems)
        {
            var num = float.MaxValue;
            TrackedItem? trackedPrey = null;
            for (var j = 0; j < items.Count; j++)
            {
                var item = items[j];
                if (item.Attractiveness() < num)
                {
                    num = item.Attractiveness();
                    trackedPrey = item;
                }
            }
            if (trackedPrey is not null)
            {
                trackedPrey.ItemRep.Destroy();
                items.Remove(trackedPrey);
            }
        }
        Update();
    }

    public virtual void ForgetItem(AbstractPhysicalObject item)
    {
        var items = Items;
        for (var num = items.Count - 1; num >= 0; num--)
        {
            if (items[num].ItemRep.RepresentedItem == item)
                items.RemoveAt(num);
        }
    }

    public override void Update()
    {
        var num = float.MinValue;
        TrackedItem? trackedPrey = null;
        var items = Items;
        for (var num2 = items.Count - 1; num2 >= 0; num2--)
        {
            var item = items[num2];
            var itemRep = item.ItemRep;
            item.Update();
            var num3 = item.Attractiveness();
            itemRep.ForgetCounter = 0;
            if (item == CurrentItem)
                num3 *= PersistanceBias;
            if (itemRep.DeleteMeNextFrame)
                items.RemoveAt(num2);
            else if (num3 > num)
            {
                num = num3;
                trackedPrey = items[num2];
            }
        }
        CurrentItem = trackedPrey;
        if (CurrentItem is not null && AI.pathFinder is not null && AI.creature.pos.room == CurrentItem.ItemRep.BestGuessForPosition().room && !CurrentItem.PathFinderCanGetToPrey())
            Frustration = Mathf.Clamp(Frustration + FrustrationSpeed, 0f, 1f);
        else
            Frustration = Mathf.Clamp(Frustration - FrustrationSpeed * 4f, 0f, 1f);
    }

    public virtual float DistanceEstimation(WorldCoordinate from, WorldCoordinate to) => from.room != to.room ? 50f : Vector2.Distance(IntVector2.ToVector2(from.Tile), IntVector2.ToVector2(to.Tile));
}