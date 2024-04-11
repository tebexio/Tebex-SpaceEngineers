using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using Tebex.API;
using Tebex.Shared.Components;
using Tebex.Triage;
using TebexSpaceEngineersPlugin;
using VRage.Collections;
using VRage.Game.ModAPI;

namespace Tebex.Adapters
{
    public class TebexSpaceEngineersAdapter : BaseTebexAdapter
    {
        private List<MyPlayer> lastPlayerList = new List<MyPlayer>();
        
        public static TebexSpaceEngineersPlugin.TebexPlugin Plugin { get; private set; }
        public TebexSpaceEngineersAdapter(TebexSpaceEngineersPlugin.TebexPlugin plugin)
        {
            Plugin = plugin;
        }
        
        public override void Init()
        {
            float commandQueueProcessTimeSeconds = PluginConfig.DebugMode ? 10.0f : 120.0f;
            float refreshTimeSeconds = PluginConfig.DebugMode ? 10.0f : 60.0f;
            bool skipWaitChecks = PluginConfig.DebugMode;
            
            // Initialize timers, hooks, etc. here
            Plugin.PluginTimers().Every(commandQueueProcessTimeSeconds, () =>
            {
                ProcessCommandQueue(skipWaitChecks);
            });
            Plugin.PluginTimers().Every(refreshTimeSeconds, () =>
            {
                DeleteExecutedCommands(skipWaitChecks);
            });
            Plugin.PluginTimers().Every(refreshTimeSeconds, () =>
            {
                ProcessJoinQueue(skipWaitChecks);
            });
            Plugin.PluginTimers().Every((refreshTimeSeconds * 15) + 1.0f, () =>  // Every 15 minutes for store info
            {
                RefreshStoreInformation(skipWaitChecks);
            });
            Plugin.PluginTimers().Every(0.5f, () =>
            {
                Task task = Plugin.WebRequests().ProcessNextRequestAsync();
                task.Wait();
            });

            Plugin.PluginTimers().Every(refreshTimeSeconds, () =>
            {
                var currentPlayers = MySession.Static.Players;
                var newPlayerList = new List<MyPlayer>();
                
                foreach (var player in currentPlayers.GetOnlinePlayers())
                {
                    if (!lastPlayerList.Contains(player))
                    {
                        var playerObj = GetPlayerRef(player.Id.SteamId.ToString());
                        if (playerObj is MyPlayer)
                        {
                            newPlayerList.Add(playerObj as MyPlayer);
                            Plugin.OnUserConnected(playerObj as MyPlayer);
                        }
                    }
                }
                lastPlayerList = newPlayerList;
            });
        }

        public override void LogWarning(string message)
        {
            VRage.Utils.MyLog.Default.WriteLineAndConsole("[WARNING] " + message);
        }

        public override void LogError(string message)
        {
            VRage.Utils.MyLog.Default.WriteLineAndConsole("[ERROR] " + message); 
        }

        public override void LogInfo(string message)
        {
            VRage.Utils.MyLog.Default.WriteLineAndConsole("[INFO] " + message);
        }

        public override void LogDebug(string message)
        {
            if (PluginConfig.DebugMode)
            {
                VRage.Utils.MyLog.Default.WriteLineAndConsole("[DEBUG] " + message);
            }
        }

        public override void ReplyPlayer(object player, string message)
        {
            if (player is MyPlayer)
            {
                MyPlayer spaceEngPlayer = (MyPlayer)player;
                //TODO send chat message
                LogInfo($"message for player '{spaceEngPlayer.DisplayName}': '{message}'");
            }
            else
            {
                LogError("Cannot send chat message to player of type: " + player.GetType());   
            }
        }

        public override void ExecuteOfflineCommand(TebexApi.Command command, object playerObj, string commandName, string[] args)
        {
            //TODO offline commands not currently supported in Space Engineers
            
            //playerObj is always null for offline commands
            var fullCommand = $"{commandName} {string.Join(" ", args)}";
            MyPlayer player = playerObj as MyPlayer;
            
            if (command.Conditions.Delay < 0)
            {
                command.Conditions.Delay = 0;
            }
            
            Plugin.PluginTimers().Once(command.Conditions.Delay, () =>
            {
                var success = false;
                if (success)
                {
                    ExecutedCommands.Add(command);
                }
                else
                {
                    LogWarning($"offline command did not succeed for player '{command.Player.Username}': {fullCommand}");
                }
            });
        }

        private bool ExecuteServerCommand(TebexApi.Command command, object playerObj, string commandName, string[] args)
        {
            var fullCommand = $"{commandName} {string.Join(" ", args)}";
            MyPlayer player = playerObj as MyPlayer;
            
            if (command.Conditions.Delay < 0)
            {
                command.Conditions.Delay = 0;
            }

            switch (commandName)
            {
                case "give_item":
                case "give-item":
                    if (args.Length < 3)
                    {
                        LogWarning($"not enough args in `give-item` command. usage: give-item [player] [itemId] [quantity]");
                        return false;    
                    }

                    var itemId = args[1];
                    var quantity = uint.Parse(args[2]);
                    return SpaceEngineersCommands.GiveItem(this, player, itemId, quantity);
                case "give_credits":
                case "give-credits":
                    if (args.Length < 2)
                    {
                        LogWarning($"not enough args in `give-credits` command. usage: give-credits [player] [amount]");
                        return false;  
                    }

                    var amount = uint.Parse(args[1]);
                    return SpaceEngineersCommands.GiveSpaceCredits(this,player, amount);
                default:
                    LogWarning($"unknown server command: {fullCommand}");
                    return false;
            }
        }
        
        public override bool ExecuteOnlineCommand(TebexApi.Command command, object playerObj, string commandName, string[] args)
        {
            return ExecuteServerCommand(command, playerObj, commandName, args);
        }

        public override bool IsPlayerOnline(string playerRefId)
        {
            object player = GetPlayerRef(playerRefId);
            if (player is MyPlayer)
            {
                return true;
            }
            if (player == null)
            {
                return false;
            }
            
            LogError("cannot get online status of player type: " + player.GetType());
            return false;
        }

        public static MyPlayer TryGetPlayerBySteamId(ulong steamId, int serialId = 0)
        {
            var collection = MySession.Static.Players;
        
            long identity = collection.TryGetIdentityId(steamId, serialId);
            if (identity == 0)
                return null;
            if (!collection.TryGetPlayerId(identity, out MyPlayer.PlayerId playerId))
                return null;
            return collection.TryGetPlayerById(playerId, out MyPlayer player) ? player : null;
        }
        
        // playerId must be a Steam ID 
        public override object GetPlayerRef(string playerId)
        {
            ulong steamId = 0;
            bool success = ulong.TryParse(playerId, out steamId);
            if (!success)
            {
                return null;
            }
            MyPlayer player = TryGetPlayerBySteamId(steamId);
            return player;
        }

        public override string ExpandUsernameVariables(string input, object playerObj)
        {
            MyPlayer spaceEngPlayer = (MyPlayer)playerObj;

            
            input = input.Replace("{id}", spaceEngPlayer.Id.SteamId.ToString());
            input = input.Replace("{name}", spaceEngPlayer.DisplayName);
            input = input.Replace("{username}", spaceEngPlayer.DisplayName);
            input = input.Replace("{steamname}", spaceEngPlayer.PlatformDisplayName);
            input = input.Replace("{username}", spaceEngPlayer.DisplayName);
            input = input.Replace("{displayname}", spaceEngPlayer.DisplayName);

            return input;
        }

        public override string ExpandOfflineVariables(string input, TebexApi.PlayerInfo info)
        {
            input = input.Replace("{id}", info.Id);
            input = input.Replace("{name}", info.Username);
            input = input.Replace("{username}", info.Username);
            input = input.Replace("{steamname}", info.Username);
            input = input.Replace("{uuid}", info.Uuid);

            return input;
        }

        public override void MakeWebRequest(string endpoint, string body, TebexApi.HttpVerb verb, TebexApi.ApiSuccessCallback onSuccess,
            TebexApi.ApiErrorCallback onApiError, TebexApi.ServerErrorCallback onServerError)
        {
            var headers = new Dictionary<string, string>();
            headers.Add("X-Tebex-Secret", PluginConfig.SecretKey);
            
            Plugin.WebRequests().Enqueue(endpoint, body, (code, response) =>
            {
                if (code == 200 || code == 201 || code == 202 || code == 204)
                {
                    onSuccess?.Invoke(code, response);
                }
                else if (code == 400)
                {
                    if (!endpoint.Contains(TebexApi.TebexTriageUrl))
                    {
                        ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent("Internal server error from Plugin API",
                            new Dictionary<string, string>
                            {
                                { "request", body },
                                { "response", response },
                            }));
                    }
                }
                else if (code == 403)
                {
                    LogError("Your server's secret key is either not set or incorrect.");
                    LogError("Use tebex:secret \"<key>\" to set your secret key to the one associated with your webstore.");
                    LogError("Set up your store and get your secret key at https://tebex.io/");
                }
                else if (code == 429) // rate limited
                {
                    LogWarning("We are being rate limited by Tebex API. If this issue continues, please report a problem.");
                    LogWarning("Requests will resume after 5 minutes.");
                    Plugin.PluginTimers().Once(60 * 5, () =>
                    {
                        LogWarning("Tebex rate limit timer has elapsed, processing will now continue");
                        IsRateLimited = false;
                    });
                }
                else if (code == 500)
                {
                    if (!endpoint.Contains(TebexApi.TebexTriageUrl)) // don't report errors that occur on the plugin logs instance
                    {
                        ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent("Internal server error from Plugin API",
                            new Dictionary<string, string>
                            {
                                { "request", body },
                                { "response", response },
                            }));
                        LogDebug(
                            "Internal Server Error from Tebex API. Please try again later. Error details follow below.");
                        LogDebug(response);    
                    }
                    
                    onServerError?.Invoke(code, response);
                }
                else if (code == 530) // cloudflare origin error
                {
                    LogDebug("CDN reported error code, web request not completed: " + code);
                    LogDebug(response);
                    onServerError?.Invoke(code, response);
                }
                else if (code == 0) // timeout or cancelled
                {
                    ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent("Request did not complete",
                        new Dictionary<string, string>
                        {
                            { "request", body },
                            { "response", response },
                        }));
                    LogDebug($"Request to Tebex did not complete. HTTP Code: {code}");
                    LogDebug(response);
                }
                else // response is a general failure error message in a json formatted response from the api
                {
                    try
                    {
                        var error = JsonConvert.DeserializeObject<TebexApi.TebexError>(response);
                        if (error != null)
                        {
                            ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent(
                                "Plugin API reported general failure", new Dictionary<string, string>
                                {
                                    { "request", body },
                                    { "error", error.ErrorMessage },
                                }));
                            onApiError?.Invoke(error);
                        }
                        else
                        {
                            ReportAutoTriageEvent(TebexTriage.CreateAutoTriageEvent(
                                "Plugin API error could not be interpreted!", new Dictionary<string, string>
                                {
                                    { "request", body },
                                    { "response", response },
                                }));
                            LogDebug($"Failed to unmarshal an expected error response from API.");
                            onServerError?.Invoke(code, response);
                        }

                        LogDebug($"Request to {endpoint} failed with code {code}.");
                        LogDebug(response);
                    }
                    catch (Exception e) // something really unexpected with our response, it's likely not JSON
                    {
                        // Try to allow server error callbacks to be processed, but they mmay assume the body contains
                        // parseable json when it doesn't.
                        try
                        {
                            LogError($"an unexpected server error occurred: {e.Message}");
                            onServerError?.Invoke(code, response);
                        }
                        catch (JsonReaderException ex)
                        {
                            LogError($"could not parse response from remote as JSON: {ex.Message}");
                            LogError(ex.ToString());
                        }
                    }
                }
            }, verb, headers, 10.0f);
        }

        public override TebexTriage.AutoTriageEvent FillAutoTriageParameters(TebexTriage.AutoTriageEvent partialEvent)
        {
            partialEvent.GameId = Plugin.GetGame();
            partialEvent.FrameworkId = "Vanilla";
            partialEvent.PluginVersion = TebexPlugin.GetPluginVersion();
            
            var serverIP = new IPAddress(Sandbox.Engine.Networking.MyGameService.GameServer.GetPublicIP());
            partialEvent.ServerIp = serverIP.ToString();
            
            return partialEvent;
        }

        public void SaveConfiguration()
        {
            Plugin.SaveConfiguration();
        }
    }
}