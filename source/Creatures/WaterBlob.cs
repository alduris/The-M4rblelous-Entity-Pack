using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class WaterBlob : Creature, Weapon.INotifyOfFlyingWeapons
{
    public class BodyFragment : CosmeticSprite
    {
        public float Darkness, LastDarkness, Size, Ratio;

        public BodyFragment(Vector2 pos, Vector2 vel, float size, float color)
        {
            base.pos = pos + vel * 2f;
            lastPos = pos;
            base.vel = vel;
            Size = size;
            Ratio = color;
        }

        public override void Update(bool eu)
        {
            vel *= .999f;
            vel.y -= room.gravity * .9f;
            Size -= .02f;
            if (room is not Room rm)
                return;
            if (Vector2.Distance(lastPos, pos) > 9f && rm.GetTile(pos).Solid && !rm.GetTile(lastPos).Solid)
            {
                var intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(rm, Room.StaticGetTilePosition(lastPos), Room.StaticGetTilePosition(pos));
                var floatRect = intVector.HasValue ? Custom.RectCollision(pos, lastPos, rm.TileRect(intVector.Value).Grow(2f)) : new();
                pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
                var flag = false;
                if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
                {
                    vel.x = Math.Abs(vel.x) * .25f;
                    flag = true;
                }
                else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
                {
                    vel.x = Math.Abs(vel.x) * -.25f;
                    flag = true;
                }
                else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
                {
                    vel.y = Math.Abs(vel.y) * .25f;
                    flag = true;
                }
                else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
                {
                    vel.y = Math.Abs(vel.y) * -.25f;
                    flag = true;
                }
                if (flag)
                    rm.PlaySound(SoundID.Swollen_Water_Nut_Terrain_Impact, pos, .6f, 1f);
            }
            base.Update(eu);
            if (rm.GetTile(pos).Solid && rm.GetTile(lastPos).Solid || pos.x < -100f || Size <= 0f)
                Destroy();
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var vector = Vector2.Lerp(lastPos, pos, timeStacker);
            if (room is not Room rm)
                return;
            LastDarkness = Darkness;
            Darkness = rm.Darkness(vector) * (1f - rm.LightSourceExposure(vector));
            if (Darkness != LastDarkness)
                ApplyPalette(sLeaser, rCam, rCam.currentPalette);
            var s0 = sLeaser.sprites[0];
            s0.x = vector.x - camPos.x;
            s0.y = vector.y - camPos.y;
            s0.alpha = (1f - Darkness * .25f) * (1f - Mathf.InverseLerp(vector.y - Size, vector.y + Size, rm.FloatWaterLevel(vector)) * .25f);
            s0.scale = Size;
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [new("Futile_White") { shader = rCam.game.rainWorld.Shaders["WaterNut"] }];
            AddToContainer(sLeaser, rCam, null);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => sLeaser.sprites[0].color = Color.Lerp(palette.waterColor1, palette.waterColor2, Ratio);

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("GrabShaders");
            base.AddToContainer(sLeaser, rCam, newContatiner);
        }
    }

    public class WaterBlobState(AbstractCreature creature) : HealthState(creature)
    {
        public float Saturated;

        public override string ToString()
        {
            var text = HealthBaseSaveString() + "<cB>SATURATED<cC>" + Saturated;
            foreach (var unrecognizedSaveString in unrecognizedSaveStrings)
                text = text + "<cB>" + unrecognizedSaveString.Key + "<cC>" + unrecognizedSaveString.Value;
            return text;
        }

        public override void LoadFromString(string[] s)
        {
            base.LoadFromString(s);
            for (var i = 0; i < s.Length; i++)
            {
                var ccAr = s[i].Split(["<cC>"], StringSplitOptions.None);
                if (ccAr[0] == "SATURATED")
                    float.TryParse(ccAr[1], out Saturated);
            }
            unrecognizedSaveStrings.Remove("SATURATED");
        }

        public override void CycleTick()
        {
            base.CycleTick();
            Saturated = 0f;
        }
    }

    public List<MovementConnection> UpcomingConnections;
    public PhysicalObject? EatObject;
    public float Size, WaterRatio, EatProgression, Distance;
    public int JumpCounter, DodgeCooldown;
    public bool Popped, ClimbingWall, ClimbUpcoming, JumpOver, NarrowUpcoming;

    public virtual WaterBlobAI? AI => abstractCreature.abstractAI.RealAI as WaterBlobAI;

    public virtual bool PreyInRange => AI?.Prey is AbstractCreature crit && crit.realizedCreature is Creature c && room?.VisualContact(abstractCreature.pos, crit.pos) is true && Vector2.Distance(firstChunk.pos, c.firstChunk.pos) < 250f;

    public virtual bool Panic => AI?.Threat is AbstractCreature crit && crit.realizedCreature is Creature c && room?.VisualContact(abstractCreature.pos, crit.pos) is true && Vector2.Distance(firstChunk.pos, c.firstChunk.pos) < 100f;

    public virtual WaterBlobState WState => (State as WaterBlobState)!;


    public virtual float Saturated
    {
        get => WState.Saturated;
        set => WState.Saturated = value;
    }

    public WaterBlob(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        Size = Mathf.Pow(Random.value, 2f);
        var num = Mathf.Pow(Mathf.Lerp(.9f, 1.1f, Size), 2f);
        WaterRatio = Mathf.Lerp(Random.Range(0f, 1f), .5f, Mathf.Pow(Random.value, 2f));
        bodyChunks = [new(this, 0, default, 10f * num, .5f)];
        Random.state = state;
        bodyChunkConnections = [];
        airFriction = .999f;
        gravity = .9f;
        bounce = .4f;
        surfaceFriction = 0.4f;
        collisionLayer = 1;
        waterFriction = .95f;
        buoyancy = 1.2f;
        UpcomingConnections = [];
    }

    public virtual void Pop()
    {
        if (Popped)
            return;
        Popped = true;
        var num = 1 + Mathf.FloorToInt(Size / .4f);
        for (var i = 0; i < num; i++)
        {
            for (var j = 0; j < Random.Range(5, 9); j++)
                room.AddObject(new WaterDrip(firstChunk.pos, Custom.RNV() * Random.Range(4f, 8f), true));
            for (var k = 0; k < Random.Range(3, 8); k++)
                room.AddObject(new BodyFragment(firstChunk.pos, Custom.RNV() * Random.Range(6f, 12f), Random.Range(.5f, .9f), WaterRatio));
            var abstractBlobPiece = new BlobPiece.AbstractBlobPiece(room.world, null, abstractCreature.pos, room.game.GetNewID(), WaterRatio);
            room.abstractRoom.AddEntity(abstractBlobPiece);
            abstractBlobPiece.RealizeInRoom();
        }
        room.PlaySound(SoundID.Egg_Bug_Drop_Eggs, firstChunk, false, 1f, .9f);
        Destroy();
    }

    public override void Die()
    {
        base.Die();
        if (room is not null)
            Pop();
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new WaterBlobGraphics(this);

    public virtual void EatSomething(PhysicalObject otherObject)
    {
        if (EatObject is not null)
            return;
        if (graphicsModule is WaterBlobGraphics gr)
        {
            if (otherObject is IDrawable dr)
                gr.AddObjectToInternalContainer(dr, 0);
            else if (otherObject.graphicsModule is GraphicsModule g)
                gr.AddObjectToInternalContainer(g, 0);
        }
        EatObject = otherObject;
        if (EatObject is Creature c)
            c.Die();
        EatProgression = 0f;
        Distance = Vector2.Distance(firstChunk.pos, otherObject.bodyChunks[0]?.pos is Vector2 v ? v : default);
        room?.PlaySound(SoundID.Bro_Digestion_Init, firstChunk.pos, 1f, 2f);
    }

    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        var vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
        firstChunk.pos = newRoom.MiddleOfTile(pos) - vector * -1.5f * 15f;
        firstChunk.lastPos = newRoom.MiddleOfTile(pos);
        Jump(Random.Range(5f, 8f), vector);
    }

    public virtual void Jump(float strength, Vector2 direction)
    {
        if (Consious && (AI?.pathFinder.CoordinateViable(abstractCreature.pos) is true || abstractCreature.pos.TileDefined && room?.GetTile(abstractCreature.pos.Tile).Terrain == Room.Tile.TerrainType.Slope))
        {
            if (graphicsModule is WaterBlobGraphics g)
                g.Impact(new(0, -1), strength);
            for (var i = 0; i < Random.Range(1, 4); i++)
                room?.AddObject(new WaterDrip(firstChunk.pos, (direction * Random.Range(4f, 8f) + Custom.RNV() * Random.Range(2f, 4f)) * (strength / 12f), true));
            room?.PlaySound(SoundID.Swollen_Water_Nut_Terrain_Impact, firstChunk);
            firstChunk.vel += direction * strength;
        }
    }

    public virtual void FlyingWeapon(Weapon weapon)
    {
        if (AI?.VisualContact(weapon.firstChunk.pos, 1f) is true && DodgeCooldown <= 0)
        {
            var vector = bodyChunks[0].pos - (weapon.firstChunk.pos + weapon.firstChunk.vel.normalized * 200f);
            vector.y *= 2f;
            if (vector.magnitude <= 200f)
            {
                Jump(Random.Range(8f, 16f), new(0f, 1f));
                DodgeCooldown = Random.Range(15, 30);
            }
        }
    }

    public virtual void Eat(bool eu)
    {
        if (EatObject?.firstChunk is not BodyChunk ch)
            return;
        var pos = firstChunk.pos;
        if (EatProgression > 1f)
        {
            for (var i = 0; i < Random.Range(5, 9); i++)
                room?.AddObject(new WaterDrip(firstChunk.pos, Custom.DirVec(firstChunk.pos + Custom.RNV() * Random.Range(0f, 5f), ch.pos) * Random.Range(4f, 8f) + Custom.RNV() * Random.Range(4f, 8f), true));
            if (EatObject is Creature c)
                AI?.tracker.ForgetCreature(c.abstractCreature);
            EatObject.Destroy();
            EatObject = null;
            return;
        }
        for (var j = 0; j < Random.Range(0, 2); j++)
            room?.AddObject(new WaterDrip(firstChunk.pos, Custom.DirVec(firstChunk.pos + Custom.RNV() * Random.Range(0f, 5f), ch.pos) * Random.Range(2f, 4f) + Custom.RNV() * Random.Range(2f, 4f), true));
        if (EatObject.collisionLayer != 0)
            EatObject.ChangeCollisionLayer(0);
        var num = EatProgression;
        EatProgression += .0125f;
        Saturated = Mathf.Min(Saturated + .00625f * (1f - Size * .75f), 1f);
        if (num <= .5f && EatProgression > .5f)
        {
            var cons = EatObject.bodyChunkConnections;
            for (var k = 0; k < cons.Length; k++)
                cons[k].type = BodyChunkConnection.Type.Pull;
        }
        var num2 = Distance * (1f - EatProgression);
        ch.vel *= 0f;
        ch.MoveFromOutsideMyUpdate(eu, pos + Custom.DirVec(pos, ch.pos) * num2);
        var chunks = EatObject.bodyChunks;
        for (var l = 0; l < chunks.Length; l++)
        {
            var chl = chunks[l];
            chl.vel *= 1f - EatProgression;
            chl.MoveFromOutsideMyUpdate(eu, Vector2.Lerp(chl.pos, pos + Custom.DirVec(pos, chl.pos) * num2, EatProgression));
        }
        if (EatObject.graphicsModule?.bodyParts is BodyPart[] parts)
        {
            for (var m = 0; m < parts.Length; m++)
            {
                var part = parts[m];
                part.vel *= 1f - EatProgression;
                part.pos = Vector2.Lerp(part.pos, pos, EatProgression);
            }
        }
    }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        if (DodgeCooldown <= 0 && otherObject is Creature c && AI?.DynamicRelationship(c.abstractCreature).type == CreatureTemplate.Relationship.Type.Afraid)
        {
            Jump(Random.Range(10f, 20f) * AI.DynamicRelationship(c.abstractCreature).intensity, Vector2.Lerp(Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, firstChunk.pos), new(0f, 1f), .5f));
            DodgeCooldown = Random.Range(15, 30);
        }
    }

    public virtual void Act()
    {
        if (room is not Room rm || AI is not WaterBlobAI ai || ai.pathFinder is not StandardPather p)
            return;
        ai.Update();
        if (safariControlled)
        {
            firstChunk.goThroughFloors = true;
            if (inputWithDiagonals is Player.InputPackage inpm)
            {
                if (inpm.jmp && (lastInputWithDiagonals is not Player.InputPackage inp || !inp.jmp))
                    Jump(Random.Range(15f, 10f) * Random.Range(.75f, 1f), new(inpm.x / 2f, inpm.y / 4f + 1f));
                if (abstractCreature.pos.TileDefined)
                {
                    int x = abstractCreature.pos.Tile.x, y = abstractCreature.pos.Tile.y;
                    if (x >= 0 && y >= 0 && x < rm.TileWidth && y < rm.TileHeight)
                    {
                        var tl = rm.Tiles[x, y];
                        var ps = new IntVector2(x, y);
                        if (tl.Terrain == Room.Tile.TerrainType.ShortcutEntrance && rm.ShorcutEntranceHoleDirection(ps) == new IntVector2(-inpm.x, -inpm.y))
                            enteringShortCut = ps;
                    }
                }
                firstChunk.vel.x += inpm.x / 5f;
            }
            return;
        }
        var movementConnection = p.FollowPath(abstractCreature.pos, false);
        NarrowUpcoming = false;
        ClimbUpcoming = false;
        JumpOver = false;
        var flag = false;
        UpcomingConnections.Clear();
        if (movementConnection != default)
        {
            for (var i = 0; i < 5; i++)
            {
                if (movementConnection.type == MovementConnection.MovementType.ReachUp)
                {
                    flag = true;
                    ClimbUpcoming = false;
                    NarrowUpcoming = false;
                }
                if (!NarrowUpcoming && room.aimap.getAItile(movementConnection.destinationCoord).narrowSpace && !flag)
                    NarrowUpcoming = true;
                if (!ClimbUpcoming && i < 2 && movementConnection.startCoord.y < movementConnection.destinationCoord.y && (room.aimap.getAItile(movementConnection.startCoord).floorAltitude > 0 || room.aimap.getAItile(movementConnection.destinationCoord).floorAltitude > 0) && !room.aimap.getAItile(movementConnection.startCoord).AnyWater && !flag)
                    ClimbUpcoming = true;
                if (!JumpOver && (movementConnection.destinationCoord.y > movementConnection.startCoord.y || movementConnection.type > MovementConnection.MovementType.Standard && movementConnection.type < MovementConnection.MovementType.LizardTurn) && !ClimbUpcoming)
                    JumpOver = true;
                UpcomingConnections.Add(movementConnection);
                movementConnection = p.FollowPath(movementConnection.destinationCoord, false);
                var ucons = UpcomingConnections;
                for (var j = 0; j < ucons.Count; j++)
                {
                    if (movementConnection == default)
                        break;
                    if (ucons[j].destinationCoord == movementConnection.destinationCoord)
                        movementConnection = default;
                }
                if (movementConnection == default)
                    break;
            }
        }
        movementConnection = p.FollowPath(abstractCreature.pos, true);
        if (movementConnection != default)
        {
            if ((NarrowUpcoming || ClimbUpcoming) && ai.Behav != WaterBlobAI.Behavior.Idle && (ai.Behav != WaterBlobAI.Behavior.Hunting || !PreyInRange))
            {
                firstChunk.terrainSqueeze = Mathf.Lerp(firstChunk.terrainSqueeze, .4f, .1f);
                ClimbingWall = true;
                gravity = 0f;
            }
            else
            {
                if (ClimbingWall && (ai.Behav == WaterBlobAI.Behavior.Hunting && PreyInRange || movementConnection.type > MovementConnection.MovementType.Standard && movementConnection.type < MovementConnection.MovementType.DropToFloor))
                    JumpCounter = 0;
                ClimbingWall = false;
                gravity = 1f;
                firstChunk.terrainSqueeze = Mathf.Lerp(firstChunk.terrainSqueeze, 1f, .1f);
            }
            if (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation)
            {
                enteringShortCut = movementConnection.StartTile;
                if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
                    NPCTransportationDestination = movementConnection.destinationCoord;
            }
            firstChunk.goThroughFloors = movementConnection.startCoord.y > movementConnection.destinationCoord.y;
        }
        else
        {
            ClimbingWall = false;
            gravity = 1f;
        }
        if (room.aimap.TileAccessibleToCreature(firstChunk.pos, abstractCreature.creatureTemplate))
        {
            if (ClimbingWall)
            {
                var to = default(Vector2);
                if (ai.Behav != WaterBlobAI.Behavior.Idle && p.FollowPath(abstractCreature.pos, false) != default && movementConnection != default)
                    to = Custom.DirVec(firstChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord)) * Custom.LerpMap(ai.JumpUrgency, 35f, 5f, 2f, 5f);
                firstChunk.vel = Vector2.Lerp(firstChunk.vel, to, room.aimap.getAItile(firstChunk.pos).AnyWater ? .5f : .2f);
                for (var k = 0; k < Random.Range(0, 3); k++)
                    room.AddObject(new WaterDrip(firstChunk.pos + Custom.RNV() * Random.Range(1f, 3f), Custom.RNV() * Random.Range(1f, 3f), true));
                return;
            }
            if (JumpCounter > 0)
            {
                --JumpCounter;
                return;
            }
            var direction = Custom.RNV();
            var num = Random.Range(6f, 8f);
            if (ai.Behav == WaterBlobAI.Behavior.Hunting && PreyInRange && ai.Prey?.realizedCreature is Creature cr)
            {
                direction = Vector2.Lerp(Custom.DirVec(firstChunk.pos, cr.firstChunk?.pos is Vector2 v ? v : default), new(0f, 1f), .5f);
                num *= Random.Range(2f, 3f);
            }
            else if (ai.Behav == WaterBlobAI.Behavior.Fleeing && ai.Threat?.realizedCreature is Creature c && Panic)
            {
                direction = Vector2.Lerp(Custom.DirVec(c.firstChunk?.pos is Vector2 v ? v : default, firstChunk.pos), new(0f, 1f), .5f);
                num *= Random.Range(2f, 3f);
            }
            else if (ai.Behav != WaterBlobAI.Behavior.Idle && movementConnection != default)
            {
                direction = Custom.DirVec(room.MiddleOfTile(movementConnection.startCoord), room.MiddleOfTile(movementConnection.destinationCoord) + new Vector2(0f, 20f));
                if (JumpOver)
                    num *= Random.Range(1.5f, 2f);
            }
            else
                direction.y = Math.Abs(direction.y);
            if (movementConnection != default && movementConnection.type > MovementConnection.MovementType.Standard && movementConnection.type < MovementConnection.MovementType.DropToFloor || ai.Behav == WaterBlobAI.Behavior.Fleeing)
                num *= 1.5f;
            Jump(num, direction);
            JumpCounter = (int)(ai.JumpUrgency * Random.Range(.75f, 1f));
        }
        else
        {
            ClimbingWall = false;
            gravity = 1f;
        }
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room is not Room rm)
            return;
        if (rm.game.devToolsActive && Input.GetKey("b") && rm.game.cameras[0].room == rm)
        {
            bodyChunks[0].vel += Custom.DirVec(bodyChunks[0].pos, (Vector2)Input.mousePosition + rm.game.cameras[0].pos) * 14f;
            Stun(12);
        }
        DodgeCooldown = Mathf.Max(DodgeCooldown - 1, 0);
        if (EatObject is not null)
        {
            if (EatObject.room != rm)
                EatObject = null;
            else
            {
                Stun(20);
                Eat(eu);
            }
        }
        if (Consious)
        {
            Act();
            return;
        }
        ClimbingWall = false;
        gravity = 1f;
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
        if (firstContact && graphicsModule is WaterBlobGraphics g)
        {
            g.Impact(direction, speed);
            room?.PlaySound(SoundID.Swollen_Water_Nut_Terrain_Impact, firstChunk);
        }
    }

    public override void LoseAllGrasps() { }
}