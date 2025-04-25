Port DONNNNNNNNNNNEEEEEEE!
My local changes (will need testing when fisobs is ported).
Will only be updated very very slowly when get some little free time.

# Creatures: V
 - BigrubProperties -> V
 - Blizzor (Graphics, Main) update (special AbstractAI class removed, replaced by a hook to a new miros method)
 - BouncingBall (Graphics, Main, AI) updated
 - Caterpillar (Graphics, Main, AI) updated
 - ChipChop (Graphics, Main) updated
 - CommonEel (Graphics, AI, Main) updated
 - Denture (Graphics, Main) updated
 - RedHorrorShell -> V
 - ScutigeraShell -> V
 - Sporantula (Main, AI, Graphics) updated
 - Surface Swimmer (Main, AI, Graphics) updated
 - ThornBug (Graphics, Main, AI) updated
 - TintedBeetle (Graphics, Main, AI) updated
 - WaterBlob (Graphics, Main, AI) updated
 - WaterSpitter (Graphics, Main, AI, WaterSpit) updated
 - the rest -> updated

do critobs with static world changes (main ones)

# Hooks: V
 - AbstractPhysicalObjectHooks -> Added global flag support in the hook + WatcherInitiateAI hook
 - ArenaHooks -> just need to fix the fisobs hooks when it's updated
 - BigEelHooks -> V
 - BigSpiderHooks -> V
 - CentipedeHooks -> V
 - CollectiblesTrackerHooks -> V
 - CreatureHooks -> V
 - From DaddyHooks to OverseerHooks -> updated
 - the rest -> updated

will need in-game testing ofc, tho I cannot test watcher stuff like rotten lizards

# Items: V
the rest: added ripple checks, PlaySound chunk arg, IHaveAStalkImpl, ID+RippleLayer string + fixed gummy anther sprites
 - LittleBalloon -> PlaySound chunk arg, IHaveAStalk impl, ripple check
 - ScoreToken -> V
 - ScoreTokenData -> V
 - ScoreTokenRepresentation -> V
 - SmallPuffBall -> ripple checks + PlaySound chunk arg
 - StarLemon -> IHaveAStalk impl
 - IHaveAStalk -> IHaveAStalkState rename
 - StalkUtils -> SkyDandelion + Pomegranate
 - ThornyStrawberry -> PlaySound chunk arg, IHaveAStalk impl, ripple check
 - ThornyStrawberryData -> V

# Enums: V
 - Added M4RUnlockList and M4RItemList
 - New sounds

Don't forget to recompile shaders and to redraw some sprites because the shader acts differently now.

Shrouded and seer will be the next ones.

## Shrouded
# Utils -> GONE
# Items V

CW CODE and CENTI MAKER UPDATED YAY!
