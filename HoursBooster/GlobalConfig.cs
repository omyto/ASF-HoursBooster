using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ArchiSteamFarm.Helpers.Json;

namespace HoursBooster;

public sealed class GlobalConfig {
  public const byte DefaultMaxGames = 5;
  public const byte DefaultDuration = 5;

  /// Maximum number of games to boost concurrently
  [JsonInclude]
  [Range(byte.MinValue, byte.MaxValue)]
  public byte MaxGames { get; private set; } = DefaultMaxGames;


  /// Duration in hours for each game to be boosted
  [JsonInclude]
  [Range(byte.MinValue, byte.MaxValue)]
  public byte Duration { get; private set; } = DefaultDuration;

  /// List of app IDs to ignore when boosting
  [JsonDisallowNull]
  [JsonInclude]
  public ImmutableHashSet<uint> Blacklist { get; private init; } = [];

  [JsonConstructor]
  internal GlobalConfig() { }
}
