using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Helpers.Json;
using ArchiSteamFarm.Localization;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Interaction;
using JetBrains.Annotations;
using SteamKit2;

namespace HoursBooster;

#pragma warning disable CA1812 // ASF uses this class during runtime
[UsedImplicitly]
internal sealed class HoursBoosterPlugin : IASF, IBot, IBotCommand2, IBotSteamClient, IGitHubPluginUpdates {
  public const string PluginName = "HoursBooster";
  public static GlobalConfig GlobalConfig { get; private set; } = new();
  public string Name => PluginName;
  public string RepositoryName => "omyto/ASF-HoursBooster";
  public Version Version => typeof(HoursBoosterPlugin).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

  private static readonly ConcurrentDictionary<Bot, BoosterEngine> BoosterEngines = new();

  public Task OnLoaded() {
    ASF.ArchiLogger.LogGenericInfo("HoursBooster - Let your games play themselves!");
    return Task.CompletedTask;
  }

  public Task OnASFInit(IReadOnlyDictionary<string, JsonElement>? additionalConfigProperties) {
    if (additionalConfigProperties != null && additionalConfigProperties.Count > 0) {
      if (additionalConfigProperties.TryGetValue(PluginName, out JsonElement configValue)) {
        GlobalConfig? config = configValue.ToJsonObject<GlobalConfig>();
        if (config != null) {
          GlobalConfig = config;
        }
      }
    }
    return Task.CompletedTask;
  }

  public Task OnBotInit(Bot bot) => Task.CompletedTask;

  public Task OnBotDestroy(Bot bot) {
    ArgumentNullException.ThrowIfNull(bot);
    if (BoosterEngines.TryRemove(bot, out BoosterEngine? engine)) {
      engine.Dispose();
    }
    return Task.CompletedTask;
  }

  public Task<IReadOnlyCollection<ClientMsgHandler>?> OnBotSteamHandlersInit(Bot bot) => Task.FromResult<IReadOnlyCollection<ClientMsgHandler>?>(null);

  public Task OnBotSteamCallbacksInit(Bot bot, CallbackManager callbackManager) {
    ArgumentNullException.ThrowIfNull(bot);

    if (BoosterEngines.TryRemove(bot, out BoosterEngine? engine)) {
      engine.Dispose();
    }

    engine = new BoosterEngine(bot);
    _ = BoosterEngines.TryAdd(bot, engine);
    return engine.OnSteamCallbacksInit(callbackManager);
  }

  public async Task<string?> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamID = 0) {
    ArgumentNullException.ThrowIfNull(bot);

    if (!Enum.IsDefined(access)) {
      throw new InvalidEnumArgumentException(nameof(access), (int) access, typeof(EAccess));
    }

    if (args != null && args.Length > 0) {
      switch (args[0].ToUpperInvariant()) {
        case "FARMHOURS":
        case "BOOSTHOURS":
          return args.Length == 1 ? ResponsePlay(access, bot) : await ResponsePlay(access, Utilities.GetArgsAsText(args, 1, ","), steamID).ConfigureAwait(false);
        default:
          break;
      }
    }

    return null;
  }

  private static string? ResponsePlay(EAccess access, Bot bot) {
    if (!Enum.IsDefined(access)) {
      throw new InvalidEnumArgumentException(nameof(access), (int) access, typeof(EAccess));
    }

    if (access < EAccess.Master) {
      return null;
    }

    if (BoosterEngines.TryGetValue(bot, out BoosterEngine? engine)) {
      string message = engine.Play();
      return bot.Commands.FormatBotResponse(message);
    }

    return bot.Commands.FormatBotResponse($"Unable to locate any booster for bot: {bot.BotName}!");
  }

  private static async Task<string?> ResponsePlay(EAccess access, string botNames, ulong steamID = 0) {
    ArgumentException.ThrowIfNullOrEmpty(botNames);

    HashSet<Bot>? bots = Bot.GetBots(botNames);

    if ((bots == null) || (bots.Count == 0)) {
      return access >= EAccess.Owner ? Commands.FormatStaticResponse(string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNames)) : null;
    }

    IList<string?> results = await Utilities.InParallel(bots.Select(bot => Task.Run(() => ResponsePlay(Commands.GetProxyAccess(bot, access, steamID), bot)))).ConfigureAwait(false);

    List<string> responses = [.. results.Where(static result => !string.IsNullOrEmpty(result))!];

    return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
  }
}
#pragma warning restore CA1812 // ASF uses this class during runtime
