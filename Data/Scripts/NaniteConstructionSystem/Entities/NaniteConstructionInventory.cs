using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Entity;
using VRage.ObjectBuilders;
using VRage.Game.ModAPI;
using Sandbox.Definitions;
using NaniteConstructionSystem.Extensions;

namespace NaniteConstructionSystem.Entities
{
    public class NaniteConstructionInventory
    {
        public List<IMyInventory> connectedInventory = new List<IMyInventory>();

        private Dictionary<string, int> m_componentsRequired;
        public Dictionary<string, int> ComponentsRequired
        {
            get { return m_componentsRequired; }
        }

        private MyEntity m_constructionBlock;
        public NaniteConstructionInventory(MyEntity constructionBlock)
        {
            m_constructionBlock = constructionBlock;
            m_componentsRequired = new Dictionary<string, int>();
        }

        internal void TakeRequiredComponents()
        {
            if (MyAPIGateway.Session.CreativeMode || ComponentsRequired.Count < 1)
                return;

            List<IMyInventory> removalList = new List<IMyInventory>();
            try
            {
                // there seems to be an issue when nanites are taking stuff from multiple inventories, they tend to
                // overstuff themselves over their limit, which leads to the loss of shit. I have made this flag
                // for testing purposes
                // TODO: remove once better solution is found
                //var itemsMoved = false;

                // go through inventories connected with the nanite control facility
                foreach (IMyInventory inventory in connectedInventory)
                {
                    IMyInventory inv = null;

                    // this is the inventory of nanite facility
                    IMyInventory constructionInventory = GetConstructionInventory();

                    // inventory does not exist or is empty, skip
                    if (inventory == null || inventory.CurrentVolume == inventory.MaxVolume)
                        continue;

                    // if there is no valid inventory with the nanite facility, remove from the list and skip
                    if (!GridHelper.IsValidInventoryConnection(constructionInventory, inventory, out inv))
                    {
                        removalList.Add(inventory);
                        continue;
                    }

                    // go through each inventory item in the connected inventory
                    foreach (var inventoryItem in inventory.GetItems().ToList())
                    {
                        // go through each required item by the facility
                        foreach (var componentNeeded in ComponentsRequired.ToList())
                        {
                            //if (itemsMoved)
                            //    break;

                            // component in the inventory is not a component, we have 0 of them or is different than we need, skip
                            if (inventoryItem.Content.TypeId != typeof(MyObjectBuilder_Component) || componentNeeded.Value <= 0
                            || (int)inventoryItem.Amount <= 0f || inventoryItem.Content.SubtypeName != componentNeeded.Key)
                                continue;

                            // get maximum ammount of components we want to move
                            var validAmount = GetMaxComponentAmount(componentNeeded.Key, (float)constructionInventory.MaxVolume - (float)constructionInventory.CurrentVolume);

                            float amount;

                            // if we have more, get some, if we have less, get all
                            if (inventoryItem.Amount >= componentNeeded.Value)
                                amount = Math.Min(componentNeeded.Value, validAmount);
                            else
                                amount = Math.Min((float)inventoryItem.Amount, validAmount);

                            // if items can't be added, skip
                            var NcfInventory = (MyInventory)constructionInventory;
                            var ObjectBuilder = new MyObjectBuilder_PhysicalObject();
                            ObjectBuilder = new MyObjectBuilder_Component() { SubtypeName = inventoryItem.Content.SubtypeName };
                            var space = NcfInventory.ComputeAmountThatFits(ObjectBuilder.GetId());

                            if ((int)amount >= space)
                                amount = (float)space;

                            if (!constructionInventory.CanItemsBeAdded((int)amount, new SerializableDefinitionId(typeof(MyObjectBuilder_Component), componentNeeded.Key)))
                                continue;

                            // itemsMoved = true;

                            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                            {
                                try
                                {
                                    inventory.RemoveItemsOfType((int)amount, (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(typeof(MyObjectBuilder_Component), componentNeeded.Key));
                                    constructionInventory.AddItems((int)amount, (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(typeof(MyObjectBuilder_Component), componentNeeded.Key));

                                    if (ComponentsRequired.ContainsKey(componentNeeded.Key))
                                        ComponentsRequired[componentNeeded.Key] -= (int)amount;
                                }
                                catch (Exception ex)
                                {
                                    Logging.Instance.WriteLine($"Nanite Control Factory: Exception in NaniteConstructionInventory.TakeRequiredComponents:\n{ex.ToString()}");
                                }
                            });
                        }
                    }
                }
            }
            catch (InvalidOperationException)
            {
                Logging.Instance.WriteLine("NaniteConstructionSystem.Extensions.GridHelper.TryMoveToFreeCargo: A list was modified. Aborting.", 1);
            }
            catch (Exception ex) when (ex.ToString().Contains("IndexOutOfRangeException")) //because Keen thinks we shouldn't have access to this exception ...
            {
                Logging.Instance.WriteLine("NaniteConstructionSystem.Extensions.GridHelper.TryMoveToFreeCargo: A list was modified. Aborting.", 1);
            }

            foreach (IMyInventory inv in removalList)
                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                    {connectedInventory.Remove(inv);});
        }

        private float GetMaxComponentAmount(string componentName, float remainingVolume)
        {
            var componentDef = MyDefinitionManager.Static.GetComponentDefinition(new MyDefinitionId(typeof(MyObjectBuilder_Component), componentName));
            return remainingVolume / componentDef.Volume;
        }

        internal void SetupRequiredComponents(List<IMySlimBlock> targetList, List<IMySlimBlock> possibleTargetList, int maxTargets, ref Dictionary<string, int> available, bool isProjection, bool shouldUseAssemblers)
        {
            if (MyAPIGateway.Session.CreativeMode)
                return;

            DebugSession.Instance.WriteLine($"NaniteConstructionInventory.SetupRequiredComponents(): {targetList.Count} target(s), {possibleTargetList.Count} possible target(s)...");

            Dictionary<string, int> missing = new Dictionary<string, int>();
            if (targetList.Count < maxTargets)
            {
                if (!isProjection)
                {
                    foreach(var item in targetList.ToList())
                    {
                        missing.Clear();
                        item.GetMissingComponents(missing);
                        CalculateComponentsRequired(ref missing, ref available);

                        foreach (var missingItem in missing)
                        {
                            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                            {
                                if (ComponentsRequired.ContainsKey(missingItem.Key))
                                    ComponentsRequired[missingItem.Key] += missingItem.Value;
                                else
                                    ComponentsRequired.Add(missingItem.Key, missingItem.Value);
                            });
                        }
                    }
                }

                // If we're not supposed to use assemblers, there's no point in checking what components are required for
                // blocks we're not actively targeting. Otherwise, we end up in a "MissingParts" loop.
                if (!shouldUseAssemblers)
                {
                    DebugSession.Instance.WriteLine($"NaniteConstructionInventory.SetupRequiredComponents(): Skipping checking possibleTargetList because shouldUseAssemblers is false.");
                    return;
                }

                foreach (var item in possibleTargetList.ToList())
                {
                    if (targetList.Contains(item))
                        continue;

                    missing.Clear();
                    if (!isProjection)
                    {
                        item.GetMissingComponents(missing);
                        CalculateComponentsRequired(ref missing, ref available);
                    }
                    else
                    {
                        var missingName = GetProjectionComponents(item).First().Key;
                        missing.Add(missingName, 1);
                    }

                    foreach (var missingItem in missing)
                    {
                        MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                        {
                            if (ComponentsRequired.ContainsKey(missingItem.Key))
                                ComponentsRequired[missingItem.Key] += missingItem.Value;
                            else
                                ComponentsRequired.Add(missingItem.Key, missingItem.Value);
                        });
                    }
                }
            } else {
                DebugSession.Instance.WriteLine($"NaniteConstructionInventory.SetupRequiredComponents(): {targetList.Count} target(s) exceeds max targets of {maxTargets}!");
            }
        }

        internal bool CheckComponentsAvailable(ref Dictionary<string, int> missing, ref Dictionary<string, int> available)
        {
            if (MyAPIGateway.Session.CreativeMode)
                return true;

            var checkMissing = new Dictionary<string, int>(missing);
            var checkAvailable = new Dictionary<string, int>(available);

            bool result = false;
            foreach (var item in checkMissing)
            {
                if (checkAvailable.ContainsKey(item.Key))
                {
                    result = true;
                    break;
                }
            }

            foreach (var item in missing)
            {
                if (available.ContainsKey(item.Key))
                {
                    available[item.Key] -= item.Value;
                    if (available[item.Key] <= 0)
                        available.Remove(item.Key);
                }
            }

            return result;
        }

        internal void CalculateComponentsRequired(ref Dictionary<string, int> missing, ref Dictionary<string, int> available)
        {
            if (MyAPIGateway.Session.CreativeMode)
                return;

            var checkMissing = new Dictionary<string, int>(missing);
            var checkAvailable = new Dictionary<string, int>(available);
            var missingResult = new Dictionary<string, int>(missing);

            foreach (var item in checkMissing)
            {
                if (!checkAvailable.ContainsKey(item.Key))
                    continue;

                if (checkAvailable[item.Key] < item.Value)
                    missingResult[item.Key] -= checkAvailable[item.Key];

                else
                    missingResult[item.Key] -= item.Value;
            }

            missing.Clear();
            foreach (var item in missingResult)
                missing.Add(item.Key, item.Value);
        }

        internal Dictionary<string, int> GetProjectionComponents(IMySlimBlock block, bool firstOnly = true)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            if (MyAPIGateway.Session.CreativeMode)
                return result;

            MyCubeBlockDefinition blockDefinition = (MyCubeBlockDefinition)block.BlockDefinition;
            if (firstOnly)
                result.Add(blockDefinition.Components[0].Definition.Id.SubtypeName, 1);

            else
                foreach (var item in blockDefinition.Components)
                    result.Add(item.Definition.Id.SubtypeName, item.Count);

            return result;
        }

        internal void GetAvailableComponents(ref Dictionary<string, int> availableComponents)
        {
            if (MyAPIGateway.Session.CreativeMode)
                return;

            IMyInventory inventory = GetConstructionInventory();
            if (inventory != null)
            {
                foreach (var item in inventory.GetItems())
                {
                    if ((int)item.Amount < 1)
                        continue;

                    if (!availableComponents.ContainsKey(item.Content.SubtypeName))
                        availableComponents.Add(item.Content.SubtypeName, (int)item.Amount);
                    else
                        availableComponents[item.Content.SubtypeName] += (int)item.Amount;
                }
            }
        }

        internal void SubtractAvailableComponents(List<object> targetList, ref Dictionary<string, int> availableComponents, ref List<object> blocksExceedingAvailable, bool isProjection)
        {
            if (MyAPIGateway.Session.CreativeMode)
                return;

            Dictionary<string, int> missing = new Dictionary<string, int>();
            foreach (var target in targetList)
            {
                IMySlimBlock item = (IMySlimBlock)target;
                missing.Clear();

                if (isProjection)
                {
                    var missingName = GetProjectionComponents(item).First().Key;
                    missing.Add(missingName, 1);
                } else
                    item.GetMissingComponents(missing);

                var lAvailableComponents = availableComponents;
                // Check to see if we have all of the components we need before removing them from availableComponents
                var hasComponentsAvailable = missing.All(mc => lAvailableComponents.ContainsKey(mc.Key) && lAvailableComponents[mc.Key] >= mc.Value);

                if (!hasComponentsAvailable)
                {
                    // If we don't have all the components we need, remove the block from the targetList by adding it to blocksExceedingAvailable
                    blocksExceedingAvailable.Add(target);
                    continue;
                }

                foreach (var component in missing)
                    availableComponents[component.Key] -= component.Value;
            }
        }

        internal bool ProcessMissingComponents(IMySlimBlock target)
        {
            if (MyAPIGateway.Session.CreativeMode)
                return true;

            try
            {
                IMyInventory inventory = GetConstructionInventory();
                if (inventory == null)
                {
                    Logging.Instance.WriteLine($"NaniteConstructionInventory.ProcessMissingComponents(): Inventory is null = {(inventory == null)}.");
                    return false;
                }

                Dictionary<string, int> missingComponents = new Dictionary<string, int>();
                // target block is projection
                if (target.CubeGrid.Physics == null)
                {
                    try {
                        MyCubeBlockDefinition blockDefinition = (MyCubeBlockDefinition)target.BlockDefinition;
                        missingComponents.Add(blockDefinition.Components[0].Definition.Id.SubtypeName, 1);
                    } catch (Exception ex) {
                        Logging.Instance.WriteLine($"NaniteConstructionInventory.ProcessMissingComponents():\n{ex.ToString()}");
                        return false;
                    }
                }

                var firstPass = false;
                foreach (var item in inventory.GetItems().ToList()) {
                    if (missingComponents.ContainsKey(item.Content.SubtypeName) && !firstPass) {
                        var amount = (float)missingComponents[item.Content.SubtypeName];
                        if (amount >= (float)item.Amount)
                            amount = (float)item.Amount;

                        missingComponents[item.Content.SubtypeName] -= (int)item.Amount;
                        if (missingComponents[item.Content.SubtypeName] <= 0)
                            missingComponents.Remove(item.Content.SubtypeName);

                        inventory.RemoveItemsOfType((int)amount, (MyObjectBuilder_PhysicalObject)item.Content);
                        firstPass = true;
                    }
                }

                if (missingComponents.Count == 0)
                    return true;

                return false;
            }
            catch(Exception ex)
            {
                Logging.Instance.WriteLine(string.Format("Exception: {0}", ex.ToString()));
                return false;
            }
        }

        private IMyInventory GetConstructionInventory()
        {
            return (IMyInventory)m_constructionBlock.GetInventoryBase(0);
        }
    }
}
