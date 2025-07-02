# Steam Games Hours Booster

[ASF-HoursBooster](https://github.com/omyto/ASF-HoursBooster) is a plugin for [ArchiSteamFarm](https://github.com/JustArchiNET/ArchiSteamFarm) that helps boost playtime of Steam games.

## Usage

1. Download the `HoursBooster.zip` archive file from the [latest release](https://github.com/omyto/ASF-HoursBooster/releases/latest).
2. Extract the archive into the `plugins` folder inside your `ArchiSteamFarm` directory.
3. Configure the plugin properties in the `ASF.json` file _(optional)_.
4. Restart `ArchiSteamFarm` _(or start it if it's not running)_.
5. Enter `farmhours` or `boosthours` command to play games.

### Configuration

The `HoursBooster` plugin configuration has the following structure, which is located within `ASF.json`.

```json
{
  "HoursBooster": {
    "MaxGames": 1,
    "Duration": 2,
    "Blacklist": []
  }
}
```
<details>
<summary><i>Example: ASF.json</i></summary>

```json
{
  "Blacklist": [ 300, 440, 550, 570, 730 ],
  "FarmingDelay": 20,
  "GiftsLimiterDelay": 2,
  "IdleFarmingPeriod": 12,
  "InventoryLimiterDelay": 5,
  "WebLimiterDelay": 500,
  "HoursBooster": {
    "MaxGames": 2,
    "Duration": 3,
    "Blacklist": [ 221380, 813780, 933110, 1017900, 1466860 ]
  }
}
```
</details>

#### Explanation

| Configuration | Type        | Default | Range   | Description                                                 |
|---------------|-------------|:-------:|:-------:|-------------------------------------------------------------|
| MaxGames      | Number      | 1       | 1-32    | The maximum number of games to be played at the same time.  |
| Duration      | Number      | 2       | 1-255   | The duration (in hours) each batch of games will be played. |
| Blacklist     | List Number |         |         | A list of Steam game IDs that are excluded from farming.    |

### Commands

| Command             | Access  | Description                                            |
| ------------------- | ------- | ------------------------------------------------------ |
| `farmhours [bots]`  | Master+ | Starts boosting the specified bot instances.           |
| `boosthours [bots]` | Master+ | Same with `farmhours`. :D                              |

Note: _You can use ASF's `!resume` command to stop farming hours._
