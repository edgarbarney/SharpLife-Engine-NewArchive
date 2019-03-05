/***
*
*	Copyright (c) 1996-2001, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology").  Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
*   This source code contains proprietary and confidential information of
*   Valve LLC and its suppliers.  Access to this code is restricted to
*   persons who have executed a written SDK license with Valve.  Any access,
*   use or distribution of this code by or to any unlicensed person is illegal.
*
****/

using SharpLife.Engine.Shared.UI;
using System;

namespace SharpLife.Engine.Host
{
    /// <summary>
    /// Handles engine hosting, startup
    /// </summary>
    public static class EngineHost
    {
        public static void Start(string[] args, HostType type)
        {
            var launcher = new Launcher();

            try
            {
                var engine = launcher.Launch(args, type);

                engine.Run();
            }
            catch (Exception e)
            {
                //Log first, in case user terminates program while messagebox is open
                //The logger can be null here if logger creation throws
                if (launcher.Logger != null)
                {
                    launcher.Logger.Error(e, "A fatal error occurred");
                }
                else
                {
                    launcher.FallbackErrorLog(e.Message + "\n");
                }

                //Display an error message for clients only (dedicated server doesn't have a local UI)
                if (type == HostType.Client)
                {
                    MessageBox.Error("SharpLife error", e.Message);
                }

                throw;
            }
        }
    }
}
