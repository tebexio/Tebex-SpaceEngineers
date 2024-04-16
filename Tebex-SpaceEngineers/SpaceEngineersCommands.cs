using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Sandbox.Definitions;
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
        // Cache for available item definitions, for lookup by name
        private static Dictionary<String, MyPhysicalItemDefinition> _itemDefinitions;
        public static void InitItemDefinitions(BaseTebexAdapter adapter)
        {
            // Get all public item definitions and store in a map for easy lookup in GiveItem()
            var publicItems = MyDefinitionManager.Static.GetAllDefinitions()
                .Where(e => e is MyPhysicalItemDefinition && e.Public)
                .Cast<MyPhysicalItemDefinition>()
                .OrderBy(e => e.DisplayNameText);
            _itemDefinitions = new Dictionary<string, MyPhysicalItemDefinition>();
            foreach (var definition in publicItems)
            {
                var defIdentifier = definition.ToString().Replace("MyObjectBuilder_", ""); // definition.Id.ToString() = "MyObjectBuilder_Ore/Gold"
                adapter.LogDebug($"caching item definition '{defIdentifier}'");
                _itemDefinitions.Add(defIdentifier, definition);
            }
        }
        
        public static bool GiveItem(BaseTebexAdapter adapter, MyPlayer player, string itemName, uint quantity)
        {
            if (player == null)
            {
                adapter.LogError($"Failed to get player for give item command");
                return false;
            }

            if (!_itemDefinitions.ContainsKey(itemName))
            {
                adapter.LogError($"Item ID not found: '{itemName}'");
                return false;
            }
            
            var itemDef = _itemDefinitions[itemName];
            var itemOb = MyObjectBuilderSerializer.CreateNewObject(itemDef.Id.TypeId, itemDef.Id.SubtypeName);
            if (itemOb == null)
            {
                adapter.LogError($"Failed to create object builder");
                return false;
            }

            var item = itemOb as MyObjectBuilder_PhysicalObject;
            if (item == null)
            {
                adapter.LogError($"Failed to create physical object for {itemName}");
                return false;
            }

            item.SubtypeName = itemName;
            var inventory = player.Character.GetInventoryBase() as MyInventory;
            if (inventory == null)
            {
                adapter.LogError("No inventory obj for player " + player.Character.Name);
                return false;
            }

            var maxVolume = inventory.MaxVolume;
            var qtyFixedPoint = new MyFixedPoint();
            qtyFixedPoint.RawValue = quantity;
            
            IMyInventoryItem invItem = new MyPhysicalInventoryItem(qtyFixedPoint, item);
            inventory.ResetVolume();
            bool inventoryAddSuccess = inventory.Add(invItem, qtyFixedPoint);
            if (!inventoryAddSuccess)
            {
                adapter.LogError($"Failed to add {qtyFixedPoint}x of item {itemName} to player {player.DisplayName}");
                return false;
            }
            
            inventory.FixInventoryVolume((float)maxVolume);
            return true;
        }

        public static bool GiveSpaceCredits(BaseTebexAdapter adapter, MyPlayer player, uint amount)
        {
            long balanceBefore = MyBankingSystem.GetBalance(player.Identity.IdentityId);
            bool success = MyBankingSystem.ChangeBalance(player.Identity.IdentityId, amount);
           
            return success;
        }
    }
}