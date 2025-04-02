Port in progress.
My local changes (will need testing when fisobs is ported).
Will only be updated very very slowly when get some little free time.
# Creatures:
 - ChipChop -> not done yet but added interactions with new edible watcher creatures even if I've not played with them
 - RedHorrorShell -> V
 - ScutigeraShell -> V

# Hooks:
 - AbstractPhysicalObjectHooks -> Added global flag support in the hook + WatcherInitiateAI hook
 - ArenaHooks -> just need to fix the fisobs hooks when it's updated
 - BigEelHooks -> V
 - BigSpiderHooks -> V
 - CentipedeHooks -> V
 - CollectiblesTrackerHooks -> V
 - CreatureHooks -> V

# Items:
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

# Enums:
 - Added M4RUnlockList and M4RItemList

# REMINDER FOR ME: THE WATCHER PR FOR FISOBS IS REMOVING setCustomFlags FROM THE PARSE METHOD, ADD A NEW HOOK
