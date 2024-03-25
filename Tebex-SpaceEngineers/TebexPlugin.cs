using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.Plugins;
using VRage.Utils;
using static TebexSpaceEngineersPlugin.PatchController;

namespace TebexSpaceEngineersPlugin {

    //Notation by Bishbash777#0465
    public class TebexPlugin : IConfigurablePlugin {
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
    }
}
