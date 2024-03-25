using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.Plugins;

namespace TebexSpaceEngineersPlugin
{
    public class PluginConfiguration : IPluginConfiguration
    {
        [Display(Name = "Debug Mode", Description = "Set to true for in-depth logging information")]
        [Category("General")]
        public bool DebugMode = false;

        [Display(Name = "Secret Key", Description = "Your Game Server key from https://creator.tebex.io/game-servers")]
        [Category("General")]
        public string SecretKey = "Your Tebex Secret Key";

        public void Save(string userDataPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PluginConfiguration));
            string configFile = Path.Combine(userDataPath, "Tebex.cfg");
            using(StreamWriter stream = new StreamWriter(configFile, false, Encoding.UTF8))
            {
                serializer.Serialize(stream, this);
            }
        }
    }
}
