using HarmonyLib;
using PulsarModLoader.MPModChecks;
using PulsarModLoader.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PulsarModLoader
{
    [HarmonyPatch(typeof(PLServer), "Awake")]
    static class MMHInstantiate
    {
        static void Prefix(PLServer __instance)
        {
            __instance.gameObject.AddComponent(typeof(ModMessageHelper));
        }
    }

    /// <summary>
    /// Manages ModMessages as well as PML RPCs.
    /// </summary>
    public class ModMessageHelper : Photon.MonoBehaviour
    {
        /// <summary>
        /// Static instance var
        /// </summary>
        public static ModMessageHelper Instance;

        /// <summary>
        /// Obsolete, no longer functions.
        /// </summary>
        [Obsolete]
        public Dictionary<PhotonPlayer, string> PlayersWithMods = new Dictionary<PhotonPlayer, string>();

        private static Dictionary<string, ModMessage> modMessageHandlers = new Dictionary<string, ModMessage>();

        internal static List<string> indexableModMessageHandlers;
        /// <summary>
        /// Obsolete. Returns "NoPlayer"
        /// </summary>
        /// <param name="inPlayer"></param>
        /// <returns></returns>
        [Obsolete]
        public string GetPlayerMods(PhotonPlayer inPlayer) //if the player exists, return the modlist, otherwise return the string 'NoPlayer'
        {
            return "NoPlayer";
        }


        ModMessageHelper()
        {
            modMessageHandlers = new Dictionary<string, ModMessage>();
            IEnumerable<PulsarMod> modList = ModManager.Instance.GetAllMods();
            foreach (PulsarMod mod in modList)
            {
                Assembly asm = mod.GetType().Assembly;
                Type modMessage = typeof(ModMessage);
                foreach (Type t in asm.GetTypes())
                {
                    if (modMessage.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    {
                        ModMessage modMessageHandler = (ModMessage)Activator.CreateInstance(t);
                        modMessageHandlers.Add(mod.HarmonyIdentifier() + "#" + modMessageHandler.GetIdentifier(), modMessageHandler);
                    }
                }
            }
            ModMessage publicCommands = new Chat.Extensions.HandlePublicCommands();
            modMessageHandlers.Add("#" + publicCommands.GetIdentifier(), publicCommands);
            if (PhotonNetwork.isMasterClient)
            {
                indexableModMessageHandlers = new List<string>(modMessageHandlers.Keys);
            }
            Instance = this;
        }

        /// <summary>
        /// Obsolete. Exists for legacy purposes.
        /// </summary>
        /// <param name="modName"></param>
        /// <returns>Mod info in string format</returns>
        [Obsolete]
        public string GetModName(string modName)
        {
            PulsarMod mod = ModManager.Instance.GetMod(modName);
            return $"{mod.Name} {mod.Version} MPF{mod.MPRequirements}";
        }
        
        /// <summary>
        /// RPC for ModMessage reciept.
        /// </summary>
        /// <param name="modID"></param>
        /// <param name="arguments"></param>
        /// <param name="pmi"></param>
        [PunRPC]
        public void ReceiveMessage(string modID, object[] arguments, PhotonMessageInfo pmi)
        {
            //Utilities.Logger.Info($"ModMessage received message for {modID}");
            if (modMessageHandlers.TryGetValue(modID, out ModMessage modMessage))
            {
                modMessage.HandleRPC(arguments, pmi);
            }
            else
            {
                Utilities.Logger.Info($"ModMessage for {modID} doesn't exist");
            }
        }

        /// <summary>
        /// RPC activated message popup, used for client failures to join.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="pmi"></param>
        [PunRPC]
        public void RecieveErrorMessage(string message, PhotonMessageInfo pmi)
        {
            if (pmi.sender.IsMasterClient)
            {
                PLNetworkManager.Instance.MainMenu.AddActiveMenu(new PLErrorMessageMenu(message));
            }
        }

        /// <summary>
        /// Host recieves mod list from connecting client
        /// </summary>
        /// <param name="recievedData"></param>
        /// <param name="pmi"></param>
        [PunRPC]
        public void ServerRecieveModList(byte[] recievedData, PhotonMessageInfo pmi)
        {
            MPUserDataBlock userDataBlock = MPModCheckManager.DeserializeHashfullMPUserData(recievedData);
            Logger.Info($"recieved modlist from user with the following info:\nPMLVersion: {userDataBlock.PMLVersion}\nModlist:{MPModCheckManager.GetModListAsString(userDataBlock.ModData)}");
            MPModCheckManager.Instance.AddNetworkedPeerMods(pmi.sender, userDataBlock);
            this.photonView.RPC("ClientRecieveIndexedModRPCs", pmi.sender, new object[]
            {
                indexableModMessageHandlers.ToArray()
            });
        }

        /// <summary>
        /// Recieves mod list from connecting client.
        /// </summary>
        /// <param name="recievedData"></param>
        /// <param name="pmi"></param>
        [PunRPC]
        public void ClientRecieveModList(byte[] recievedData, PhotonMessageInfo pmi)
        {
            //Send local modlist to client other client
            if (!pmi.sender.IsMasterClient && !MPModCheckManager.Instance.SentModLists.Contains(pmi.sender))
            {
                MPModCheckManager.Instance.SendModlistToClient(pmi.sender);
                MPModCheckManager.Instance.SentModLists.Add(pmi.sender);
            }
            MPUserDataBlock userDataBlock = MPModCheckManager.DeserializeHashlessMPUserData(recievedData);
            Logger.Info($"recieved modlist from user with the following info:\nPMLVersion: {userDataBlock.PMLVersion}\nModlist:{MPModCheckManager.GetModListAsString(userDataBlock.ModData)}");


            MPModCheckManager.Instance.AddNetworkedPeerMods(pmi.sender, userDataBlock);

            Events.Instance.CallClientModlistRecievedEvent(pmi.sender);
        }

        /// <summary>
        /// Recieves the order of mod rpcs so client and host are synced for sending/recieving indexed versions of RecieveMessage
        /// </summary>
        /// <param name="modRPCNames"></param>
        /// <param name="pmi"></param>
        [PunRPC]
        public void ClientRecieveIndexedModRPCs(string[] modRPCNames, PhotonMessageInfo pmi)
        {
            if (!pmi.sender.IsMasterClient)
            {
                return;
            }
            indexableModMessageHandlers = modRPCNames.ToList<string>();
        }
        /// <summary>
        /// Recieves the index of mod a message instead of the whole string modifier to reduce network load
        /// </summary>
        /// <param name="index"></param>
        /// <param name="arguments"></param>
        /// <param name="pmi"></param>
        [PunRPC]
        public void RecieveIndexedMessage(int index, object[] arguments, PhotonMessageInfo pmi)
        {
            if (index >= 0 && index < indexableModMessageHandlers.Count)
            {
                this.ReceiveMessage(indexableModMessageHandlers[index], arguments, pmi);
            }
            else
            {
                Utilities.Logger.Info($"Index is not valid for indexed mod message");
            }
        }

        /// <summary>
        /// Client sends request to the host for modlist of a client. Deprecated
        /// </summary>
        /// <param name="pmi"></param>
        [PunRPC]
        public void ClientRequestModList(PhotonMessageInfo pmi)
        {
            Logger.Info("ModMessageHelper recieved modlist request. This is a deprecated RPC and is only called by older modloader versions");
        }
    }
}
