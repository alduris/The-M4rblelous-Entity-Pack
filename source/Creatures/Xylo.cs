/*using Noise;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;*/
/*TODO:
 * NVst port + finish
 * 
 * SDv make
 * 
 * MDLz port
 * 
 * Entity Pack:
 * Merge Aluris PR
 * 
 * Xylo + Xylo Worm:
 * relationships
 * add black teetch to worms
 * add colored smoke
 * rework root sprite
 * change container after that
 * fix some items not being swallowed
 * make worms spawnable
 * make living worms attach the player or other creatures and slow them down slightly
 * make icons (reuse black leech icon for worm)
 * implement non-trypo mode
 * prevent some creatures from being swallowed
 */
/*
public class Xylo : Creature
{
    public const float BASE_RAD = 70f;
    public PlacedObject? PlacedObj;
    public Color EffectColor;
    public Vector2 RootPos;
    public float Lerper;
    public int Worms;
    public bool LerpUp = true;

    public new HazerMomState State => (base.State as HazerMomState)!;

    public override bool SandstormImmune => true;

    public Xylo(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        bodyChunks = [new(this, 0, default, BASE_RAD, .5f)];
        bodyChunkConnections = [];
        abstractCreature.tentacleImmune = true;
        abstractCreature.HypothermiaImmune = true;
        abstractCreature.lavaImmune = true;
        GoThroughFloors = true;
        airFriction = .99f;
        gravity = 0f;
        bounce = .01f;
        surfaceFriction = .47f;
        collisionLayer = 1;
        waterFriction = .94f;
        buoyancy = .01f;
        var state = Random.state;
        Random.InitState(abstractPhysicalObject.ID.RandomSeed);
        Worms = Random.Range(3, 6);
        EffectColor = new(Random.Range(82f / 255f, 119f / 255f), 3f / 255f, 252f / 255f);
        Random.state = state;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new XyloGraphics(this);

    public override void Update(bool eu)
    {
        base.Update(eu);
        var speed = .1f * (2.5f - State.ClampedHealth * 1.5f);
        Lerper = Mathf.Clamp(Lerper + (LerpUp ? speed * 2.5f : -speed), -2f, 1f);
        if (Lerper >= 1f)
        {
            room?.PlaySound(NewSoundID.M4R_Xylo_Swell, firstChunk);
            LerpUp = false;
        }
        else if (Lerper <= -2f)
            LerpUp = true;
        firstChunk.rad = BASE_RAD * Mathf.Clamp(1f + Lerper * .1f, 1f, 1.25f);
        firstChunk.HardSetPosition(RootPos);
    }

    public override void Stun(int st) { }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        if (otherObject is not null)
        {
            var radSum = 0f;
            var chs = otherObject.bodyChunks;
            for (var i = 0; i < chs.Length; i++)
                radSum += chs[i].rad;
            if (radSum <= 10f)
            {
                room?.PlaySound(NewSoundID.M4R_Xylo_Swallow, firstChunk);
                if (otherObject is Creature cr)
                {
                    cr.killTag = abstractCreature;
                    cr.Die();
                }
                if (otherObject is Weapon w)
                {
                    killTag = w.thrownBy?.abstractCreature;
                    State.health -= .275f;
                }
                otherObject.Destroy();
                if (graphicsModule is XyloGraphics gr)
                    gr.LightUp = .5f;
            }
        }
    }

    public override void HitByWeapon(Weapon weapon)
    {
        if (weapon is not null)
        {
            var radSum = 0f;
            var chs = weapon.bodyChunks;
            for (var i = 0; i < chs.Length; i++)
                radSum += chs[i].rad;
            if (radSum <= 10f)
            {
                room?.PlaySound(NewSoundID.M4R_Xylo_Swallow, firstChunk);
                killTag = weapon.thrownBy?.abstractCreature;
                State.health -= .2f;
                weapon.Destroy();
                if (graphicsModule is XyloGraphics gr)
                    gr.LightUp = .5f;
            }
        }
    }

    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk) { }

    public override void HeardNoise(InGameNoise noise) { }

    public override bool AllowableControlledAIOverride(MovementConnection.MovementType movementType) => false;

    public override void Blind(int blnd) { }

    public override bool CanBeGrabbed(Creature grabber) => false;

    public override void Deafen(int df) { }

    public override void Die()
    {
        base.Die();
        if (PlacedObj?.data is PlacedObject.ConsumableObjectData dt && grabbedBy.Count > 0)
        {
            if (room?.game?.session is StoryGameSession sess)
                sess.saveState.ReportConsumedItem(room.world, false, State.OrigRoom, State.PlacedObjectIndex, Random.Range(dt.minRegen, dt.maxRegen));
            PlacedObj = null;
        }
        if (room is Room rm)
        {
            var vector = firstChunk.pos;
            rm.AddObject(new SootMark(rm, vector, 80f, true));
            rm.AddObject(new Explosion(rm, this, vector, 5, 250f, 14f, 1.6f, 80f, .6f, this, .8f, 0f, .8f));
            for (var i = 0; i < 14; i++)
                rm.AddObject(new Explosion.ExplosionSmoke(vector, Custom.RNV() * 5f * Random.value, 1f));
            rm.AddObject(new Explosion.ExplosionLight(vector, 250f, 1f, 3, EffectColor));
            rm.AddObject(new ExplosionSpikes(rm, vector, 9, 4f, 7f, 12f, 150f, EffectColor));
            rm.AddObject(new ShockWave(vector, 150f, .25f, 5));
            for (var j = 0; j < 25; j++)
            {
                var vector2 = Custom.RNV();
                rm.AddObject(new Spark(vector + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), EffectColor, null, 4, 18));
            }
            rm.ScreenMovement(vector, default, 1f);
            rm.PlaySound(SoundID.Bomb_Explode, firstChunk);
            AbstractCreature abstractWorm;
            var wormTpl = StaticWorld.GetCreatureTemplate(CreatureTemplateType.XyloWorm);
            XyloWorm worm;
            for (; Worms > 0; Worms--)
            {
                rm.abstractRoom.AddEntity(abstractWorm = new(rm.world, wormTpl, null, abstractPhysicalObject.pos, rm.game.GetNewID()));
                abstractWorm.RealizeInRoom();
                worm = (abstractWorm.realizedObject as XyloWorm)!;
                worm.firstChunk.HardSetPosition(rm.MiddleOfTile(firstChunk.pos));
            }
            rm.abstractRoom.AddEntity(abstractWorm = new(rm.world, wormTpl, null, abstractPhysicalObject.pos, rm.game.GetNewID())
            {
                superSizeMe = true
            });
            abstractWorm.RealizeInRoom();
            worm = (abstractWorm.realizedObject as XyloWorm)!;
            worm.firstChunk.HardSetPosition(rm.MiddleOfTile(firstChunk.pos));
        }
        Destroy();
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus) { }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact) { }

    public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction) => false;

    public override bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying) => false;

    public override void Grabbed(Grasp grasp) => grasp?.Release();

    public override void GrabbedObjectSnatched(PhysicalObject grabbedObject, Creature thief) { }

    public override void LoseAllGrasps() { }

    public override Color ShortCutColor() => default;

    public override void ReleaseGrasp(int grasp) { }

    public override void PlaceInRoom(Room placeRoom)
    {
        placeRoom.AddObject(this);
        NewRoom(placeRoom);
        var state = State;
        if (state.OrigRoom > -1 && state.OrigRoom == placeRoom.abstractRoom.index && state.PlacedObjectIndex >= 0 && state.PlacedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
        {
            PlacedObj = placeRoom.roomSettings.placedObjects[state.PlacedObjectIndex];
            firstChunk.HardSetPosition(RootPos = PlacedObj.pos);
        }
        else
            firstChunk.HardSetPosition(RootPos = placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
    }

    public override void PushOutOf(Vector2 pos, float rad, int exceptedChunk) { }
}*/