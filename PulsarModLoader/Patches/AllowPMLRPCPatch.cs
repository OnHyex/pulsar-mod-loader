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
    internal class ControlModRPCCache
    {
        internal ControlModRPCCache()
        {
            Instance = this;
            MethodInfo[] methods = typeof(ModMessageHelper).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo method in methods)
            {
                if (method.IsDefined(typeof(PunRPC), false))
                {
                    _PMLRPCNames.Add(method.Name);
                }
            }
            _InitialListRPCSize = PhotonNetwork.PhotonServerSettings.RpcList.Count;
        }
        internal static ControlModRPCCache Instance;
        private static List<string> _PMLRPCNames = new List<string>();
        private static int _InitialListRPCSize;
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
            PhotonNetwork.PhotonServerSettings.RpcList.RemoveRange(_InitialListRPCSize - 1, PhotonNetwork.PhotonServerSettings.RpcList.Count - _InitialListRPCSize);
            foreach (string rpcName in _PMLRPCNames)
            {
                PhotonNetwork.networkingPeer.rpcShortcuts.Remove(rpcName);
            }

        }
        [HarmonyPatch(typeof(PLUIMainMenu), "Start")]
        class CreateModRPCCacheManager
        {
            static void Postfix()
            {
                new ControlModRPCCache();
                Events.Instance.ServerStartEvent += (PLServer server) =>
                {
                    ControlModRPCCache.RegisterRPCs();
                };
                Events.Instance.OnLeaveGameEvent += () =>
                {
                    ControlModRPCCache.UnRegisterRPCs();
                };
            }
        }
    }
}
