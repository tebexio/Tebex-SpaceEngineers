using System.Runtime.Remoting.Contexts;
using Sandbox.Game;
using Sandbox.Game.World;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

namespace TebexSpaceEngineersPlugin
{
    // Commands in vanilla Space Engineers are explicitly defined (give money, give item) instead of implicitly by the user.
    //  These are still sent as normal commands and must be interpreted, but these are the implementations of those supported commands.
    public static class SpaceEngineersCommands
    {
        public static bool GiveItem(MyPlayer player, uint itemId)
        {
            return false;
        }

        public static bool GiveSpaceCredits(MyPlayer player, uint amount)
        {
            return false;
        }

        /*
        private bool Give(ulong steamId, ItemType type, string itemName, int quantity)
        {
            /*
            //get IMyPlayer from steamId
            var player = TryGetPlayerBySteamId(steamId);

            if (player == null)
            {
                Context.Respond("Player not found");
                Log.Error($"Failed to get player from steamId {steamId}");
                return false;
            }

            var itemOb = MyObjectBuilderSerializer.CreateNewObject(RewardType.RewardTypeMap[type]);

            if (itemOb == null)
            {
                Log.Error($"Failed to create object builder for {type}");
                return false;
            }

            var item = itemOb as MyObjectBuilder_PhysicalObject;
            if (item == null)
            {
                Log.Error($"Failed to create physical object for {type}");
                return false;
            }

            item.SubtypeName = itemName;
            var inventory = player.Character.GetInventory() as MyInventory;

            var maxVolume = inventory.MaxVolume;

            IMyInventoryItem invItem = new MyPhysicalInventoryItem(quantity, item);

            inventory.ResetVolume();
            inventory.Add(invItem, quantity);
            inventory.FixInventoryVolume((float)maxVolume);
            return true;
        }
            return false;
    }*/
    }
}