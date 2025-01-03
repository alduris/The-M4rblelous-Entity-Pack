using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace LBMergedMods.Items;

public class MiniFruit : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	//public Stalk Stalk;
    public float Darkness, LastDarkness;
    public Vector2 Rotation, LastRotation;
	public Vector2? SetRotation;

	public virtual AbstractConsumable AbstrCons => (abstractPhysicalObject as AbstractConsumable)!;

	public virtual int BitesLeft => 1;

	public virtual int FoodPoints => 0;

	public virtual bool Edible => true;

	public virtual bool AutomaticPickUp => true;

	public MiniFruit(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
	{
		bodyChunks = [new(this, 0, default, 4f, .1f)];
		bodyChunkConnections = [];
		airFriction = .999f;
		gravity = .9f;
		bounce = .2f;
		surfaceFriction = .7f;
		collisionLayer = 1;
		waterFriction = .95f;
		buoyancy = 1.1f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		var fc = firstChunk;
		if (room.game.devToolsActive && Input.GetKey("b"))
			fc.vel += Custom.DirVec(fc.pos, Futile.mousePosition) * 3f;
		LastRotation = Rotation;
		if (grabbedBy.Count > 0)
		{
			Rotation = Custom.PerpendicularVector(Custom.DirVec(fc.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			Rotation.y = Mathf.Abs(Rotation.y);
		}
		if (SetRotation is Vector2 v)
		{
			Rotation = v;
			SetRotation = null;
		}
		if (fc.ContactPoint.y < 0)
		{
			Rotation = (Rotation - Custom.PerpendicularVector(Rotation) * .1f * fc.vel.x).normalized;
			fc.vel.x *= .8f;
		}
		var crits = room.abstractRoom.creatures;
        if (fc.submersion > .5f && crits.Count > 0 && grabbedBy.Count == 0)
		{
			var crit = crits[Random.Range(0, crits.Count)];
			if (crit.creatureTemplate.type == CreatureTemplate.Type.JetFish && crit.realizedCreature is JetFish j && !j.dead && j.AI.goToFood is null && j.AI.WantToEatObject(this))
				j.AI.goToFood = this;
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta is not null) && room.game.GetArenaGameSession.counter < 10)
		{
			firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			//Stalk = new(this, placeRoom, firstChunk.pos);
			//placeRoom.AddObject(Stalk);
		}
		else if (!AbstrCons.isConsumed && AbstrCons.placedObjectIndex >= 0 && AbstrCons.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrCons.placedObjectIndex].pos);
			//Stalk = new(this, placeRoom, firstChunk.pos);
			//placeRoom.AddObject(Stalk);
		}
		else
		{
			firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			Rotation = Custom.RNV();
			LastRotation = Rotation;
		}
	}

	/*public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (Stalk is Stalk st && st.ReleaseCounter == 0)
			st.ReleaseCounter = Random.Range(30, 50);
	}*/

	public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = [new("DangleFruit0A") { scale = .7071f }, new("DangleFruit0B") { scale = .7071f }];
		AddToContainer(sLeaser, rCam, null);
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker),
			rot = Vector3.Slerp(LastRotation, Rotation, timeStacker);
		LastDarkness = Darkness;
		Darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
		if (Darkness != LastDarkness)
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		var sprs = sLeaser.sprites;
		for (var i = 0; i < sprs.Length; i++)
		{
			var sprite = sprs[i];
			sprite.SetPosition(pos - camPos);
            sprite.rotation = Custom.VecToDeg(rot);
			if (i == 1)
                sprite.color = blink > 0 && Random.value < .5f ? blinkColor : color;
        }
		if (slatedForDeletetion || room != rCam.room)
			sLeaser.CleanSpritesAndRemove();
	}

	public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = palette.blackColor;
        color = Color.Lerp(ModManager.MSC && rCam.room.game.session is StoryGameSession && rCam.room.world.name == "HR" ? RainWorld.SaturatedGold : Color.blue, palette.blackColor, Darkness);
    }

	public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
	{
		newContainer ??= rCam.ReturnFContainer("Items");
		var sprs = sLeaser.sprites;
		for (var i = 0; i < sprs.Length; i++)
		{
			var sprite = sprs[i];
			sprite.RemoveFromContainer();
			newContainer.AddChild(sprite);
        }
	}

	public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		room.PlaySound(SoundID.Slugcat_Eat_Dangle_Fruit, firstChunk.pos);
		firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        (grasp.grabber as Player)?.ObjectEaten(this);
        grasp.Release();
        Destroy();
    }

	public virtual void ThrowByPlayer() { }
}