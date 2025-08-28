using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

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
    [HarmonyPatch(typeof(PLUIMainMenu), "Start")]
    class OptimizePMLRPCsPatch
    {
        static void Prefix()
        {
            PhotonNetwork.PhotonServerSettings.RpcList.Add("ReceiveMessage");
            PhotonNetwork.PhotonServerSettings.RpcList.Add("ClientRecieveModList");
            PhotonNetwork.PhotonServerSettings.RpcList.Add("ServerRecieveModList");
            PhotonNetwork.PhotonServerSettings.RpcList.Add("ClientRequestModList");
            PhotonNetwork.PhotonServerSettings.RpcList.Add("ClientRecieveIndexedModRPCs");
            PhotonNetwork.PhotonServerSettings.RpcList.Add("RecieveIndexedMessage");
            PhotonNetwork.networkingPeer.rpcShortcuts.Add("RecieveMessage", PhotonNetwork.networkingPeer.rpcShortcuts.Count);
            PhotonNetwork.networkingPeer.rpcShortcuts.Add("ClientRecieveModList", PhotonNetwork.networkingPeer.rpcShortcuts.Count);
            PhotonNetwork.networkingPeer.rpcShortcuts.Add("ServerRecieveModList", PhotonNetwork.networkingPeer.rpcShortcuts.Count);
            PhotonNetwork.networkingPeer.rpcShortcuts.Add("ClientRequestModList", PhotonNetwork.networkingPeer.rpcShortcuts.Count);
            PhotonNetwork.networkingPeer.rpcShortcuts.Add("ClientRecieveIndexedModRPCs", PhotonNetwork.networkingPeer.rpcShortcuts.Count);
            PhotonNetwork.networkingPeer.rpcShortcuts.Add("RecieveIndexedMessage", PhotonNetwork.networkingPeer.rpcShortcuts.Count);
        }
    }
}
