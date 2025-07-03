using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Steam;
using SteamKit2;

namespace HoursBooster;

internal sealed class BoosterEngine(Bot bot) {
  private Bot Bot { get; } = bot;
  private Timer? Timer { get; set; }
  private List<uint> BoostedGames { get; set; } = [];
  private List<uint> PlayingGames { get; set; } = [];
  private Dictionary<uint, string>? OwnedGames { get; set; }

  /// Disposes the BoosterEngine, stopping any ongoing heartbeating process and releasing resources.
  internal void Dispose() {
    Timer?.Dispose();
    Timer = null;
  }

  /// Initializes the BoosterEngine and subscribes to necessary callbacks.
  internal Task OnSteamCallbacksInit(CallbackManager callbackManager) {
    ArgumentNullException.ThrowIfNull(callbackManager);
    LogTrace("Subscribing to PlayingSessionStateCallback");
    _ = callbackManager.Subscribe<SteamUser.PlayingSessionStateCallback>(OnPlayingSessionState);
    return Task.CompletedTask;
  }

  /// Handles the PlayingSessionStateCallback to check if playing is blocked.
  private void OnPlayingSessionState(SteamUser.PlayingSessionStateCallback callback) {
    ArgumentNullException.ThrowIfNull(callback);
    LogTrace($"Received PlayingSessionStateCallback: PlayingBlocked={callback.PlayingBlocked}, PlayingAppID={callback.PlayingAppID}");

    if (callback.PlayingBlocked || callback.PlayingAppID == 0 || !PlayingGames.Contains(callback.PlayingAppID)) {
      Dispose();
      PlayingGames.Clear();
    }
  }

  /// Starts the heartbeating process to boost games for the specified duration.
  internal string Play() {
    if (!Bot.IsConnectedAndLoggedOn) {
      return Strings.BotNotConnected;
    }

    if (Timer != null) {
      return "Already running";
    }

    Timer = new Timer(Heartbeating, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);
    return Strings.Done;
  }

  /// Starts the heartbeating process to boost games.
  private async void Heartbeating(object? state) {
    if (Bot.IsConnectedAndLoggedOn && Bot.IsPlayingPossible) {
      if (PlayingGames.Count > 0) {
        LogInfo("Switching to booosting another games");
      }

      PlayingGames = await GetGamesToBoost().ConfigureAwait(false);
      if (PlayingGames.Count == 0) {
        LogInfo("No games to boost");
        _ = Bot.Actions.Resume();
        return;
      }

      (bool success, string message) = await Bot.Actions.Play(PlayingGames).ConfigureAwait(false);
      if (!success) {
        PlayingGames.Clear();
        LogInfo($"Failed to boost games: {message}");
        return;
      }

      int duration = HoursBoosterPlugin.GlobalConfig.Duration > 0 ? HoursBoosterPlugin.GlobalConfig.Duration : GlobalConfig.DefaultDuration;

      foreach (uint appID in PlayingGames) {
        BoostedGames.Add(appID);
        if (OwnedGames?.TryGetValue(appID, out string? name) == true) {
          LogInfo($"Boosting game {appID} ({name}) for {duration} hours");
        }
        else {
          LogInfo($"Boosting game {appID} for {duration} hours");
        }
      }

      _ = Timer?.Change(TimeSpan.FromHours(duration), Timeout.InfiniteTimeSpan);
    }
    else {
      LogInfo("Bot is not connected or playing is not possible");
    }
  }

  /// Retrieves a list of games to boost.
  [SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "<Pending>")]
  [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "<Pending>")]
  private async Task<List<uint>> GetGamesToBoost() {
    OwnedGames = await Bot.ArchiHandler.GetOwnedGames(Bot.SteamID).ConfigureAwait(false);
    if (OwnedGames == null || OwnedGames.Count == 0) {
      return [];
    }

    HashSet<uint> validGames = OwnedGames.Keys.ToHashSet();

    if (ASF.GlobalConfig != null && ASF.GlobalConfig.Blacklist.Count > 0) {
      validGames.ExceptWith(ASF.GlobalConfig.Blacklist);
    }

    if (HoursBoosterPlugin.GlobalConfig.Blacklist.Count > 0) {
      validGames.ExceptWith(HoursBoosterPlugin.GlobalConfig.Blacklist);
    }

    List<uint> waitingGames = validGames.Except(BoostedGames).ToList();

    int maxGames = HoursBoosterPlugin.GlobalConfig.MaxGames is > 0 and <= 32 ? HoursBoosterPlugin.GlobalConfig.MaxGames : GlobalConfig.DefaultMaxGames;

    if (waitingGames.Count > 0) {
      int skip = waitingGames.Count > maxGames ? Random.Shared.Next(0, waitingGames.Count - maxGames + 1) : 0;
      return waitingGames.Skip(skip).Take(maxGames).ToList();
    }

    BoostedGames.Clear();
    return validGames.Take(maxGames).ToList();
  }

  /// 
  private void LogInfo(string message, [CallerMemberName] string? callerMethodName = null) {
    ArgumentException.ThrowIfNullOrEmpty(callerMethodName);
    Bot.ArchiLogger.LogGenericInfo(message, $"{HoursBoosterPlugin.PluginName}|{callerMethodName}");
  }

  private void LogTrace(string message, [CallerMemberName] string? callerMethodName = null) {
    ArgumentException.ThrowIfNullOrEmpty(callerMethodName);
    Bot.ArchiLogger.LogGenericTrace(message, $"{HoursBoosterPlugin.PluginName}|{callerMethodName}");
  }
}
