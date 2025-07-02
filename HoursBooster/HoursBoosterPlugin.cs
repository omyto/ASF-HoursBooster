using System;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Plugins.Interfaces;
using JetBrains.Annotations;

namespace HoursBooster;

[UsedImplicitly]
internal sealed class HoursBoosterPlugin : IGitHubPluginUpdates {
  public string Name => nameof(HoursBoosterPlugin);
  public string RepositoryName => "omyto/ASF-HoursBooster";
  public Version Version => typeof(HoursBoosterPlugin).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

  public Task OnLoaded() {
    ASF.ArchiLogger.LogGenericInfo($"HoursBooster - Let your games play themselves!");
    return Task.CompletedTask;
  }
}
