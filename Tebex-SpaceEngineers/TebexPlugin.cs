using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Tebex.Adapters;
using Tebex.API;
using Tebex.Shared.Components;
using VRage.Plugins;
using VRage.Utils;
using static TebexSpaceEngineersPlugin.PatchController;

namespace TebexSpaceEngineersPlugin {

    //Notation by Bishbash777#0465
    public class TebexPlugin : IConfigurablePlugin {
        #region Template
        //Global value for config which when implemented correctly, Can be read anywhere in the plugin assembly
        private PluginConfiguration m_configuration;


        //Init is called once the server has been deemed to be "Ready"
        public void Init(object gameInstance) {

            //GetConfiguration NEEDS to be called at this point in the process or else Developers will experience the
            //behaviour that is exhibited on the description of the GetConfiguration definition below...
            GetConfiguration(VRage.FileSystem.MyFileSystem.UserDataPath);

            //START PATCHING ALL methods marked for patch by controller - only needs to be called once.
            //PatchMethods(this); FIXME
        }

        //Called every gameupdate or 'Tick'
        public void Update() {
            //MyLog.Default.WriteLineAndConsole("tebex tick");
        }


        //Seems to either be non-functional or more likely called too late in the plugins initialisation stage meaning that
        //if you want to read any configuration values in Update() Or Init(), you will be met with a null ref crash...
        //Maybe consider a mandatory GLOBAL to be defined at the top of the main class which could be read by the DS
        //which will tell it the name of the cfg file therefore cutting out the need for GetConfiguration to be mandatory
        //in each seperate plugin that is ever developed.
        public IPluginConfiguration GetConfiguration(string userDataPath) {
            if (m_configuration == null) {
                string configFile = Path.Combine(userDataPath, "Tebex.cfg");
                if (File.Exists(configFile)) {
                    XmlSerializer serializer = new XmlSerializer(typeof(PluginConfiguration));
                    using (FileStream stream = File.OpenRead(configFile)) {
                        m_configuration = serializer.Deserialize(stream) as PluginConfiguration;
                    }
                }

                if (m_configuration == null) {
                    m_configuration = new PluginConfiguration();
                }
            }

            return m_configuration;
        }

        //Run when server is in unload/shutdown
        public void Dispose() {
        }

        //Returned to DS to display a friendly name of the plugin to the DS user...
        public string GetPluginTitle() {
            return "Tebex";
        }
        #endregion

        #region Tebex

        private static TebexSpaceEngineersAdapter _adapter;
         
        private static PluginTimers _timers;
        private static WebRequests _webrequest;
        
        public static string GetPluginVersion()
        {
            return "2.0.0";
        }

        protected void Load()
        {
            // Sync configuration to BaseTebexAdapter model
            BaseTebexAdapter.PluginConfig.SecretKey = Configuration.Instance.SecretKey;
            BaseTebexAdapter.PluginConfig.AutoReportingEnabled = Configuration.Instance.AutoReportingEnabled;
            BaseTebexAdapter.PluginConfig.DebugMode = Configuration.Instance.DebugMode;
            Init();
        }

        private void Init()
        {
            // Setup our API and adapter
            _adapter = new TebexSpaceEngineersAdapter(this);
            _adapter.LogInfo("Tebex is starting up...");
            
            // Init plugin components so they have access to our adapter
            _webrequest = new WebRequests(_adapter);
            _timers = new PluginTimers(_adapter);
            
            TebexApi.Instance.InitAdapter(_adapter);
 
            // Check if auto reporting is disabled and show a warning if so.
            if (!BaseTebexAdapter.PluginConfig.AutoReportingEnabled)
            {
                _adapter.LogWarning("Auto reporting issues to Tebex is disabled.");
                _adapter.LogWarning("To enable, please set 'AutoReportingEnabled' to 'true' in config/Tebex.json");
            }

            // Check if secret key has been set. If so, get store information and place in cache
            if (BaseTebexAdapter.PluginConfig.SecretKey != "your-secret-key-here")
            {
                // No-op, just to place info in the cache for any future triage events
                _adapter.FetchStoreInfo((info => { }));
                return;
            }

            _adapter.LogInfo("Tebex detected a new configuration file.");
            _adapter.LogInfo("Use tebex:secret <secret> to add your store's secret key.");
            _adapter.LogInfo("Alternatively, add the secret key to 'Tebex.json' and reload the plugin.");
        }

        public WebRequests WebRequests()
        {
            return _webrequest;
        }

        public PluginTimers PluginTimers()
        {
            return _timers;
        }

        public string GetGame()
        {
            return "Space Engineers";
        }

        public void Warn(string message)
        {
            Logger.LogWarning(message);
        }

        public void Error(string message)
        {
            Logger.LogError(message);
        }

        public void Info(string info)
        {
            Logger.Log(info);
        }

        public void OnUserConnected(MyPlayer player)
        {
            // Check for default config and inform the admin that configuration is waiting.
            if (MySession.Static.IsUserAdmin(player.Id.SteamId) && BaseTebexAdapter.PluginConfig.SecretKey == "your-secret-key-here"))
            {
                _adapter.ReplyPlayer(player, "Tebex is not configured. Use tebex:secret <secret> from the F1 menu to add your key."); 
                _adapter.ReplyPlayer(player, "Get your secret key by logging in at:");
                _adapter.ReplyPlayer(player, "https://tebex.io/");
            }

            //FIXME find player IP
            MyNetworkClient playerNetworkClient;
            Sync.Clients.TryGetClient(player.Id.SteamId, out playerNetworkClient);
            
            _adapter.LogDebug($"Player login event: {player.Id}");
            _adapter.OnUserConnected(player.Id.SteamId.ToString(), "0.0.0.0");
        }
        
        private void OnServerShutdown()
        {
            // Make sure join queue is always empties on shutdown
            _adapter.ProcessJoinQueue();
        }

        private BaseTebexAdapter.TebexConfig GetDefaultConfig()
        {
            return new BaseTebexAdapter.TebexConfig();
        }

        public static BaseTebexAdapter GetAdapter()
        {
            return _adapter;
        }

        public void SaveConfiguration()
        {
            Configuration.Save();
        }
        
        #endregion
    }
}
