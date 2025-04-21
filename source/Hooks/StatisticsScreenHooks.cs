global using static LBMergedMods.Hooks.StatisticsScreenHooks;
using Menu;

namespace LBMergedMods.Hooks;
//CHK
public static class StatisticsScreenHooks
{
    internal static void On_StoryGameStatisticsScreen_GetDataFromGame(On.Menu.StoryGameStatisticsScreen.orig_GetDataFromGame orig, StoryGameStatisticsScreen self, KarmaLadderScreen.SleepDeathScreenDataPackage package)
    {
        orig(self, package);
        if (package.saveState.deathPersistentSaveData is not DeathPersistentSaveData dt || !ScoreData.TryGetValue(dt, out var data))
            return;
        var score = data.Score;
        if (score > 0)
        {
            var tickers = self.allTickers;
            var ticker = new StoryGameStatisticsScreen.LabelTicker(self, self.pages[0], new(self.ContinueAndExitButtonsXPos - 300f, 535f - 30f * (-3 + tickers.Count)), score, NewTickerID.ScoreTokens, self.Translate("Bonus score from collecting tokens : "));
            ticker.numberLabel.pos.x += 160f;
            tickers.Add(ticker);
            self.pages[0].subObjects.Add(ticker);
        }
    }

    internal static void On_StoryGameStatisticsScreen_TickerIsDone(On.Menu.StoryGameStatisticsScreen.orig_TickerIsDone orig, StoryGameStatisticsScreen self, StoryGameStatisticsScreen.Ticker ticker)
    {
        orig(self, ticker);
        if (ticker.ID == NewTickerID.ScoreTokens)
            self.scoreKeeper.AddScoreAdder(ticker.getToValue, 1);
    }
}