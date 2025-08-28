﻿namespace PulsarModLoader
{
    /// <summary>
    /// Abstract class for ModMessages.
    /// </summary>
    public abstract class ModMessage
    {
        /// <summary>
        /// Gets the unique identifier for this mod
        /// </summary>
        /// <returns>namespace.name</returns>
        public string GetIdentifier()
        {
            return GetType().Namespace + "." + GetType().Name;
        }

        /// <summary>
        /// Send data to a PhotonPlayer's mod specified by harmonyIdentifier and handlerIdentifier
        /// </summary>
        /// <param name="harmonyIdentifier">PulsarModLoader.PulsarMod.HarmonyIdentifier()</param>
        /// <param name="handlerIdentifier">PulsarModLoader.ModMessage.GetIdentifier()</param>
        /// <param name="player"></param>
        /// <param name="arguments"></param>
        public static void SendRPC(string harmonyIdentifier, string handlerIdentifier, PhotonPlayer player, object[] arguments)
        {
            string fullName = harmonyIdentifier + "#" + handlerIdentifier;
            int index = ModMessageHelper.indexableModMessageHandlers.IndexOf(fullName);
            if (index != -1)
            {
                ModMessageHelper.Instance.photonView.RPC("RecieveIndexedMessage", player, new object[]
                { 
                    index,
                    arguments
                });
                return;
            }
            ModMessageHelper.Instance.photonView.RPC("ReceiveMessage", player, new object[]
            {
                fullName,
                arguments
            });
        }

        /// <summary>
        /// Send data to a PhotonTarget's mod specified by harmonyIdentifier and handlerIdentifier
        /// </summary>
        /// <param name="harmonyIdentifier">PulsarModLoader.PulsarMod.HarmonyIdentifier()</param>
        /// <param name="handlerIdentifier">PulsarModLoader.ModMessage.GetIdentifier()</param>
        /// <param name="targets"></param>
        /// <param name="arguments"></param>
        public static void SendRPC(string harmonyIdentifier, string handlerIdentifier, PhotonTargets targets, object[] arguments)
        {
            string fullName = harmonyIdentifier + "#" + handlerIdentifier;
            int index = ModMessageHelper.indexableModMessageHandlers.IndexOf(fullName);
            if (index != -1)
            {
                ModMessageHelper.Instance.photonView.RPC("RecieveIndexedMessage", targets, new object[]
                {
                    index,
                    arguments
                });
                return;
            }
            ModMessageHelper.Instance.photonView.RPC("ReceiveMessage", targets, new object[]
            {
                fullName,
                arguments
            });
        }

        /// <summary>
        /// Recieve data from other players
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="sender"></param>
        public abstract void HandleRPC(object[] arguments, PhotonMessageInfo sender);
    }
}
