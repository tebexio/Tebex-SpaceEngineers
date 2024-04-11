using System.Runtime.Remoting.Contexts;
using Sandbox.Game;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Sandbox.Game.World;
using Tebex.Adapters;
using VRage;
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
        public static bool GiveItem(BaseTebexAdapter adapter, MyPlayer player, uint itemId, uint quantity)
        {
            /**
            if (player == null)
            {
                adapter.LogError($"Failed to get player for give item command");
                return false;
            }

            var itemOb = MyObjectBuilderSerializer.CreateNewObject(RewardType.RewardTypeMap[type]);
            if (itemOb == null)
            {
                adapter.LogError($"Failed to create object builder");
                return false;
            }

            var item = itemOb as MyObjectBuilder_PhysicalObject;
            if (item == null)
            {
                adapter.LogError($"Failed to create physical object for {type}");
                return false;
            }

            item.SubtypeName = itemName;
            var inventory = player.Character.GetInventoryBase() as MyInventory;
            if (inventory == null)
            {
                adapter.LogError("No inventory obj for player " + player.Character.Name);
            }

            var maxVolume = inventory.MaxVolume;
            var qtyFixedPoint = new MyFixedPoint();
            qtyFixedPoint.RawValue = quantity;
            IMyInventoryItem invItem = new MyPhysicalInventoryItem(qtyFixedPoint, item);

            inventory.ResetVolume();
            inventory.Add(invItem, qtyFixedPoint);
            inventory.FixInventoryVolume((float)maxVolume);
            return true;*/
            return false;
        }

        public static bool GiveSpaceCredits(MyPlayer player, uint amount)
        {
            return MyBankingSystem.ChangeBalance(player.Identity.IdentityId, amount);
        }
    }
}