﻿using System;

namespace PulsarModLoader
{
    /// <summary>
    /// Distinctly organizes MPRequirements.
    /// </summary>
    [Obsolete]
    public enum MPFunction
    {
        /// <summary>
        /// No MP Requirements
        /// </summary>
        None,          //0 No mp requirements
        /// <summary>
        /// Only the host is required to have it installed
        /// </summary>
        HostOnly,      //1 Only the host is required to have it installed
        /// <summary>
        /// Host must have installed for clients to use
        /// </summary>
        HostRequired,  //2 Host must have installed for clients to use
        /// <summary>
        /// All clients must have installed
        /// </summary>
        All            //3 All clients must have installed
    }
}
namespace PulsarModLoader.MPModChecks
{
    /// <summary>
    /// Distinctly organizes MPRequirements
    /// </summary>
    public enum MPRequirement
    {
        /// <summary>
        /// No MP Requirements
        /// </summary>
        None,                   //0 No mp requirements/Clientside or Hostside
        /// <summary>
        /// No MP Requirements, is hidden from server listings.
        /// </summary>
        HideFromServerList,     //1 Hidden from server listings
        /// <summary>
        /// Host must have installed for clients to join
        /// </summary>
        Host,                   //2 Host must have installed for clients to join
        /// <summary>
        /// All clients must have installed
        /// </summary>
        All,                    //3 All clients must have installed
        /// <summary>
        /// No Requirements, Versions must match between clients
        /// </summary>
        MatchVersion            //4 No MP requirements, ensure versions match between clients
    }
}
