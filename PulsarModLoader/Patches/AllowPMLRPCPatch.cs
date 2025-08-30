using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace PulsarModLoader.Patches
{
    [HarmonyPatch(typeof(NetworkingPeer), "ExecuteRpc")]
    class AllowPMLRPCPatch
    {
        static bool PatchMethod(bool ShouldContinue, string MethodName)
        {
            if (MethodName == "ReceiveMessage" || MethodName == "ClientRecieveModList" || MethodName == "ServerRecieveModList" || MethodName == "ClientRequestModList" || MethodName == "ClientRecieveIndexedModRPCs" || MethodName == "RecieveIndexedMessage")
            {
                return true;
            }
            else
            {
                return ShouldContinue;
            }
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> targetSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Brfalse_S),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Stloc_S),
                new CodeInstruction(OpCodes.Ldloc_S),
            };

            List<CodeInstruction> injectedSequence = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AllowPMLRPCPatch), "PatchMethod")),
            };
            return HarmonyHelpers.PatchBySequence(instructions, targetSequence, injectedSequence, checkMode: HarmonyHelpers.CheckMode.NEVER);
        }
    }
    //Pushes the PML rpcs into the shortcut cache for both the outgoing and ingoing RPC hashing so that instead of the strings for each rpc being sent just the index of them in these lists are sent (fixed 4 bytes instead of unkown number of bytes likely greater than 4)
    //PhotonNetwork.PhotonServerSettings.RpcList is the cache for recieving RPCs
    //PhotonNetwork.networkingpeer.rpcShortcuts is the cache for sending RPCs
    internal class ControlModRPCCache
    {
        internal ControlModRPCCache()
        {
            Instance = this;
            MethodInfo[] methods = typeof(ModMessageHelper).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo method in methods)
            {
                if (method.IsDefined(typeof(PunRPC), false) && !excludedRPCs.Contains(method.Name))
                {
                    _PMLRPCNames.Add(method.Name);
                }
            }
            Events.Instance.ServerStartEvent += (PLServer server) =>
            {
                ControlModRPCCache.RegisterRPCs();
            };
            Events.Instance.OnLeaveGameEvent += () =>
            {
                ControlModRPCCache.UnRegisterRPCs();
            };
        }
        //Excluding these methods from cache achieves compatibility with older PML as mod list syncing will still properly occur and the host / client will be able to fully turn off the optimizations properly after recieving data from the outdated client
        private static readonly List<string> excludedRPCs = new List<string>() { "ClientRecieveModList", "ServerRecieveModList" };
        internal static ControlModRPCCache Instance;
        private static List<string> _PMLRPCNames = new List<string>();
        internal static void RegisterRPCs()
        {
            PhotonNetwork.PhotonServerSettings.RpcList.AddRange(_PMLRPCNames);
            foreach (string rpcName in _PMLRPCNames)
            {
                PhotonNetwork.networkingPeer.rpcShortcuts.Add(rpcName, PhotonNetwork.networkingPeer.rpcShortcuts.Count);
            }
        }
        internal static void UnRegisterRPCs()
        {
            foreach (string rpcName in _PMLRPCNames)
            {
                PhotonNetwork.PhotonServerSettings.RpcList.Remove(rpcName);
                PhotonNetwork.networkingPeer.rpcShortcuts.Remove(rpcName);
            }

        }
        [HarmonyPatch(typeof(PLUIMainMenu), "Start")]
        class CreateModRPCCacheManager
        {
            static void Postfix()
            {
                new ControlModRPCCache();
            }
        }
    }
}
