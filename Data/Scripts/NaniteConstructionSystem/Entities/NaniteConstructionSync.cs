using System;
using System.Text;
using Sandbox.ModAPI;
using VRage.Utils;
using VRage.ModAPI;

using NaniteConstructionSystem.Extensions;
using NaniteConstructionSystem.Settings;

namespace NaniteConstructionSystem.Entities
{
    public class NaniteConstructionManagerSync
    {
        private bool m_init;
        public NaniteConstructionManagerSync()
        {
            m_init = false;
        }

        public void Initialize()
        {
            if (Sync.IsClient)
            {
                Logging.Instance.WriteLine($"Nanite: Running RegisterSecureMessageHandler for Client...");
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8950, HandleUpdateState);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8951, HandleAddTarget);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8952, HandleCompleteTarget);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8953, HandleCancelTarget);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8954, HandleDetails);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8960, HandleTerminalSettings);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8962, HandleAssemblerSettings);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8971, HandleBeaconTerminalSettings);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8974, HandleFactoryGroup);
            }

            if (Sync.IsServer)
            {
                Logging.Instance.WriteLine($"Nanite: Running RegisterSecureMessageHandler for Server...");
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8961, HandleNeedTerminalSettings);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8963, HandleNeedAssemblerSettings);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8964, HandleTerminalSettings);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8965, HandleAssemblerSettings);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8972, HandleBeaconTerminalSettings);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(8973, HandleNeedBeaconTerminalSettings);
            }

            m_init = true;
        }

        public void Unload()
        {
            if (m_init)
            {
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8950, HandleUpdateState);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8951, HandleAddTarget);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8952, HandleCompleteTarget);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8953, HandleCancelTarget);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8954, HandleDetails);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8960, HandleTerminalSettings);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8962, HandleAssemblerSettings);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8961, HandleNeedTerminalSettings);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8963, HandleNeedAssemblerSettings);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8964, HandleTerminalSettings);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8965, HandleAssemblerSettings);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8972, HandleBeaconTerminalSettings);
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(8973, HandleNeedBeaconTerminalSettings);
            }
        }

        /// <summary>
        /// Multiplayer packet handlers - Direct to the proper block handler
        /// </summary>
        /// <param name="data"></param>
        private void HandleUpdateState(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (MyAPIGateway.Session == null)
                    return;

                StateData state = MyAPIGateway.Utilities.SerializeFromXML<StateData>(ASCIIEncoding.ASCII.GetString(data));
                Logging.Instance.WriteLine(string.Format("UpdateState: {0}", state.State.ToString()), 1);
                if (NaniteConstructionManager.NaniteBlocks == null)
                    return;

                NaniteConstructionManager.VerifyBlocks();
                foreach (var item in NaniteConstructionManager.NaniteBlocks)
                {
                    if (item.Key == state.EntityId)
                        item.Value.SyncUpdateState(state);
                }
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(string.Format("HandleUpdateState Error: {0}", ex.ToString()));
            }
        }

        // HandleAddTarget
        private void HandleAddTarget(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (MyAPIGateway.Session == null)
                    return;

                if (NaniteConstructionManager.NaniteBlocks == null)
                    return;

                TargetData target = MyAPIGateway.Utilities.SerializeFromXML<TargetData>(ASCIIEncoding.ASCII.GetString(data));
                Logging.Instance.WriteLine(string.Format("HandleAddTarget: {0}", target.ToString()), 1);
                NaniteConstructionManager.VerifyBlocks();
                foreach (var item in NaniteConstructionManager.NaniteBlocks)
                {
                    if (item.Key == target.EntityId)
                        item.Value.SyncAddTarget(target);
                }
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(string.Format("HandleAddTarget Error: {0}", ex.ToString()));
            }
        }

        // HandleCompleteTarget
        private void HandleCompleteTarget(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (MyAPIGateway.Session == null)
                    return;

                if (NaniteConstructionManager.NaniteBlocks == null)
                    return;

                TargetData target = MyAPIGateway.Utilities.SerializeFromXML<TargetData>(ASCIIEncoding.ASCII.GetString(data));
                NaniteConstructionManager.VerifyBlocks();
                foreach (var item in NaniteConstructionManager.NaniteBlocks)
                {
                    if (item.Key == target.EntityId)
                        item.Value.SyncCompleteTarget(target);
                }
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(string.Format("HandleCompleteTarget Error: {0}", ex.ToString()));
            }
        }

        // HandleCancelTarget
        private void HandleCancelTarget(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (MyAPIGateway.Session == null)
                    return;

                if (NaniteConstructionManager.NaniteBlocks == null)
                    return;

                TargetData target = MyAPIGateway.Utilities.SerializeFromXML<TargetData>(ASCIIEncoding.ASCII.GetString(data));
                NaniteConstructionManager.VerifyBlocks();
                foreach (var item in NaniteConstructionManager.NaniteBlocks)
                {
                    if (item.Key == target.EntityId)
                        item.Value.SyncCancelTarget(target);
                }
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(string.Format("HandleCancelTarget Error: {0}", ex.ToString()));
            }
        }

        private void HandleDetails(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            MyAPIGateway.Parallel.Start(() =>
            {
                try
                {
                    if (MyAPIGateway.Session == null)
                        return;

                    if (NaniteConstructionManager.NaniteBlocks == null)
                        return;

                    DetailData details = MyAPIGateway.Utilities.SerializeFromXML<DetailData>(ASCIIEncoding.ASCII.GetString(data));

                    if (details == null)
                        return;
                    //Logging.Instance.WriteLine(string.Format("HandleDetails: {0}", details.EntityId));
                    NaniteConstructionManager.VerifyBlocks();

                    foreach (var item in NaniteConstructionManager.NaniteBlocks)
                        if (item.Key == details.EntityId && item.Value.Initialized)
                        {
                            //Logging.Instance.WriteLine(string.Format("Details for Factory: {0}", details.EntityId));
                            item.Value.SyncDetails(details);
                            break;
                        }
                }
                catch (Exception ex)
                {
                    Logging.Instance.WriteLine(string.Format("HandleDetails() Error: {0}", ex.ToString()));
                }
            });
            
        }

        private void HandleFactoryGroup(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            MyAPIGateway.Parallel.Start(() =>
            {
                try
                {
                    if (MyAPIGateway.Session == null)
                        return;

                    if (NaniteConstructionManager.NaniteBlocks == null)
                        return;

                    FactoryGroupData details = MyAPIGateway.Utilities.SerializeFromXML<FactoryGroupData>(ASCIIEncoding.ASCII.GetString(data));
                    NaniteConstructionManager.VerifyBlocks();

                    foreach (var item in NaniteConstructionManager.NaniteBlocks)
                        if (item.Key == details.EntityId && item.Value.Initialized)
                        {
                            item.Value.SyncFactoryGroup(details);
                            break;
                        }
                }
                catch (Exception ex)
                {
                    Logging.Instance.WriteLine(string.Format("HandleFactoryGroup() Error: {0}", ex.ToString()));
                }
            });
        }

        public void SendTerminalSettings(IMyTerminalBlock block)
        {
            SendTerminalSettings(block.EntityId);
        }

        public void SendTerminalSettings(long blockId)
        {
            if (NaniteConstructionManager.NaniteBlocks == null)
                return;

            if (!NaniteConstructionManager.NaniteBlocks.ContainsKey(blockId))
                return;

            if (!NaniteConstructionManager.TerminalSettings.ContainsKey(blockId))
                NaniteConstructionManager.TerminalSettings.Add(blockId, new NaniteTerminalSettings());

            SerializableKeyValuePair<long, NaniteTerminalSettings> settings = new SerializableKeyValuePair<long, NaniteTerminalSettings>(blockId, NaniteConstructionManager.TerminalSettings[blockId]);

            if (Sync.IsServer)
            {
                Logging.Instance.WriteLine("SendTerminalSettings -> Others", 0);
                MyAPIGateway.Multiplayer.SendMessageToOthers(8960, ASCIIEncoding.ASCII.GetBytes(MyAPIGateway.Utilities.SerializeToXML(settings)));
            }
            else if (Sync.IsClient)
            {
                Logging.Instance.WriteLine("SendTerminalSettings -> Server", 0);
                MyAPIGateway.Multiplayer.SendMessageToServer(8964, ASCIIEncoding.ASCII.GetBytes(MyAPIGateway.Utilities.SerializeToXML(settings)));
            }
        }

        private void HandleTerminalSettings(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            Logging.Instance.WriteLine("HandleTerminalSettings", 2);

            try
            {
                if (MyAPIGateway.Session == null)
                    return;

                var settings = MyAPIGateway.Utilities.SerializeFromXML<SerializableKeyValuePair<long, NaniteTerminalSettings>>(ASCIIEncoding.ASCII.GetString(data));
                if (!NaniteConstructionManager.TerminalSettings.ContainsKey(settings.Key))
                    NaniteConstructionManager.TerminalSettings.Add(settings.Key, settings.Value);
                else
                    NaniteConstructionManager.TerminalSettings[settings.Key] = settings.Value;

                if (Sync.IsServer)
                    SendTerminalSettings(settings.Key);
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(string.Format("HandleTerminalSettings() Error: {0}", ex.ToString()));
            }
        }

        public void SendAssemblerSettings(IMyTerminalBlock block)
        {
            SendAssemblerSettings(block.EntityId);
        }

        public void SendAssemblerSettings(long blockId)
        {
            if (!NaniteConstructionManager.AssemblerSettings.ContainsKey(blockId))
            {
                Logging.Instance.WriteLine($"Nanite[SendAssemblerSettings({blockId})]: AssemblerSettings doesn't contain {blockId}, adding defaults...");
                NaniteConstructionManager.AssemblerSettings.Add(blockId, new NaniteAssemblerSettings());
            }

            Logging.Instance.WriteLine($"Nanite[SendAssemblerSettings({blockId})]: Serializing before sending to client");
            SerializableKeyValuePair<long, NaniteAssemblerSettings> settings = new SerializableKeyValuePair<long, NaniteAssemblerSettings>(blockId, NaniteConstructionManager.AssemblerSettings[blockId]);

            Logging.Instance.WriteLine($"Nanite[SendAssemblerSettings({blockId})]: Serialized successfully!");

            if (Sync.IsServer)
            {
                Logging.Instance.WriteLine($"Nanite[SendAssemblerSettings({blockId})]: Sending AssemblerSettings to all clients");
                MyAPIGateway.Multiplayer.SendMessageToOthers(8962, ASCIIEncoding.ASCII.GetBytes(MyAPIGateway.Utilities.SerializeToXML(settings)));
            }
            else if (Sync.IsClient)
            {
                Logging.Instance.WriteLine($"Nanite[SendAssemblerSettings({blockId})]: Sending updated AssemblerSettings to the server");
                MyAPIGateway.Multiplayer.SendMessageToServer(8965, ASCIIEncoding.ASCII.GetBytes(MyAPIGateway.Utilities.SerializeToXML(settings)));
            }
        }

        private void HandleAssemblerSettings(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (MyAPIGateway.Session == null)
                    return;

                var settings = MyAPIGateway.Utilities.SerializeFromXML<SerializableKeyValuePair<long, NaniteAssemblerSettings>>(ASCIIEncoding.ASCII.GetString(data));
                if (!NaniteConstructionManager.AssemblerSettings.ContainsKey(settings.Key))
                    NaniteConstructionManager.AssemblerSettings.Add(settings.Key, settings.Value);

                NaniteConstructionManager.AssemblerSettings[settings.Key] = settings.Value;
                
                if (Sync.IsServer)
                    SendAssemblerSettings(settings.Key);
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(string.Format("HandleAssemblerSettings() Error: {0}", ex.ToString()));
            }
        }

        private void HandleNeedTerminalSettings(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                Logging.Instance.WriteLine($"Nanite[TerminalSettings]: Received TerminalSettings request from SteamId64#{steamId}...");
                if (MyAPIGateway.Session == null)
                    return;

                Logging.Instance.WriteLine($"Nanite[TerminalSettings]: Unserializing...");

                var blockId = MyAPIGateway.Utilities.SerializeFromXML<long>(ASCIIEncoding.ASCII.GetString(data));

                Logging.Instance.WriteLine($"Nanite[TerminalSettings]: Received TerminalSettings request from SteamId64#{steamId} for BlockId#{blockId}...");

                NaniteConstructionManager.VerifyBlocks();
                foreach (var item in NaniteConstructionManager.NaniteBlocks)
                {
                    if (item.Key == blockId)
                    {
                        Logging.Instance.WriteLine($"Nanite[TerminalSettings]: Found block!");
                        SendTerminalSettings(blockId);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(string.Format("HandleNeedTerminalSettings() Error: {0}", ex.ToString()));
            }
        }

        public void SendNeedTerminalSettings(long blockId)
        {
            if (MyAPIGateway.Multiplayer == null)
            {
                Logging.Instance.WriteLine($"Nanite: ERR Requesting TerminalSettings for EntityId#{blockId} as we're not in multiplayer...");
                return;
            }

            if (Sync.IsServer)
            {
                Logging.Instance.WriteLine($"Nanite: ERR Requesting AssemblerSettings for EntityId#{blockId} as we're the server!");
                return;
            }

            Logging.Instance.WriteLine($"Nanite: Requesting TerminalSettings for EntityId#{blockId}...");
            MyAPIGateway.Multiplayer.SendMessageToServer(8961, ASCIIEncoding.ASCII.GetBytes(MyAPIGateway.Utilities.SerializeToXML<long>(blockId)));
        }

        private void HandleNeedAssemblerSettings(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                Logging.Instance.WriteLine($"Nanite[AssemblerSettings]: Received AssemblerSettings request from SteamId64#{steamId}...");
                if (MyAPIGateway.Session == null)
                    return;

                Logging.Instance.WriteLine($"Nanite[AssemblerSettings]: Unserializing...");

                var blockId = MyAPIGateway.Utilities.SerializeFromXML<long>(ASCIIEncoding.ASCII.GetString(data));
                Logging.Instance.WriteLine($"Nanite[AssemblerSettings]: Received AssemblerSettings request from SteamId64#{steamId} for BlockId#{blockId}...");
                foreach (var item in NaniteConstructionManager.AssemblerBlocks)
                {
                    if (item.Key == blockId)
                    {
                        Logging.Instance.WriteLine($"Nanite[TerminalSettings]: Found block!");
                        SendAssemblerSettings(blockId);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(string.Format("HandleNeedAssemblerSettings() Error: {0}", ex.ToString()));
            }

        }

        public void SendNeedAssemblerSettings(long blockId)
        {
            if (MyAPIGateway.Multiplayer == null)
            {
                Logging.Instance.WriteLine($"Nanite: ERR Requesting AssemblerSettings for EntityId#{blockId} as we're not in multiplayer...");
                return;
            }

            if (Sync.IsServer)
            {
                Logging.Instance.WriteLine($"Nanite: ERR Requesting AssemblerSettings for EntityId#{blockId} as we're the server!");
                return;
            }

            Logging.Instance.WriteLine($"Requesting Assembler Settings -> {blockId}", 2);
            MyAPIGateway.Multiplayer.SendMessageToServer(8963, ASCIIEncoding.ASCII.GetBytes(MyAPIGateway.Utilities.SerializeToXML(blockId)));
        }

        public void SendBeaconTerminalSettings(long blockId)
        {
            if (!NaniteConstructionManager.BeaconTerminalSettings.ContainsKey(blockId))
                NaniteConstructionManager.BeaconTerminalSettings.Add(blockId, new NaniteBeaconTerminalSettings());

            SerializableKeyValuePair<long, NaniteBeaconTerminalSettings> settings = new SerializableKeyValuePair<long, NaniteBeaconTerminalSettings>(blockId, NaniteConstructionManager.BeaconTerminalSettings[blockId]);

            if (Sync.IsServer)
            {
                Logging.Instance.WriteLine("SendAssemblerSettings -> Others", 2);
                Logging.Instance.WriteLine($"SendBeaconTerminalSettings -> Client: {blockId}", 1);
                MyAPIGateway.Multiplayer.SendMessageToOthers(8971, ASCIIEncoding.ASCII.GetBytes(MyAPIGateway.Utilities.SerializeToXML(settings)));
            } else if (Sync.IsClient)
            {
                Logging.Instance.WriteLine("SendAssemblerSettings -> Server", 2);
                Logging.Instance.WriteLine($"SendBeaconTerminalSettings -> Server: {blockId}", 1);
                MyAPIGateway.Multiplayer.SendMessageToServer(8972, ASCIIEncoding.ASCII.GetBytes(MyAPIGateway.Utilities.SerializeToXML(settings)));
            }
        }

        private void HandleBeaconTerminalSettings(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (MyAPIGateway.Session == null)
                    return;

                var settings = MyAPIGateway.Utilities.SerializeFromXML<SerializableKeyValuePair<long, NaniteBeaconTerminalSettings>>(ASCIIEncoding.ASCII.GetString(data));
                if (!NaniteConstructionManager.BeaconTerminalSettings.ContainsKey(settings.Key))
                    NaniteConstructionManager.BeaconTerminalSettings.Add(settings.Key, settings.Value);

                NaniteConstructionManager.BeaconTerminalSettings[settings.Key] = settings.Value;
                Logging.Instance.WriteLine("Receieved Beacon Terminal Settings Update: {settings.Key}");

                if (Sync.IsServer)
                    SendBeaconTerminalSettings(settings.Key);
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(string.Format("HandleBeaconTerminalSettings() Error: {0}", ex.ToString()));
            }
        }

        public void SendNeedBeaconTerminalSettings(long blockId)
        {
            if (MyAPIGateway.Multiplayer == null)
            {
                Logging.Instance.WriteLine($"Nanite: ERR Requesting BeaconTerminalSettings for EntityId#{blockId} as we're not in multiplayer...");
                return;
            }

            if (Sync.IsServer)
            {
                Logging.Instance.WriteLine($"Nanite: ERR Requesting AssemblerSettings for EntityId#{blockId} as we're the server!");
                return;
            }

            Logging.Instance.WriteLine($"SendNeedBeaconTerminalSettings: {blockId}", 1);
            MyAPIGateway.Multiplayer.SendMessageToServer(8973, ASCIIEncoding.ASCII.GetBytes(MyAPIGateway.Utilities.SerializeToXML(blockId)));
        }

        private void HandleNeedBeaconTerminalSettings(ushort channelId, byte[] data, ulong steamId, bool fromServer)
        {
            try
            {
                if (MyAPIGateway.Session == null)
                    return;

                var settings = MyAPIGateway.Utilities.SerializeFromXML<long>(ASCIIEncoding.ASCII.GetString(data));
                Logging.Instance.WriteLine($"HandleNeedBeaconTerminalSettings: {settings}", 2);

                foreach (var item in NaniteConstructionManager.BeaconTerminalSettings)
                {
                    if (item.Key == settings)
                    {
                        Logging.Instance.WriteLine($"SendBeaconTerminalSettings: {settings}", 2);
                        SendBeaconTerminalSettings(item.Key);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(string.Format("HandleNeedBeaconTerminalSettings() Error: {0}", ex.ToString()));
            }
        }
    }
}
