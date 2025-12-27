using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GravshipLaunchWindup
{
    public static class DebugUtility
    {
        public static void DebugLog(string msg, LogMessageType messageType = LogMessageType.Message)
        {
            if (messageType == LogMessageType.Message)
            {
                if (GLWSettings.printDebug)
                {
                    Log.Message("[Gravship Launch Windup] " + msg);
                }
            }
            else if (messageType == LogMessageType.Warning)
            {
                Log.Warning("[Gravship Launch Windup] " + msg);
            }
            else if (messageType == LogMessageType.Error)
            {
                Log.Error("[Gravship Launch Windup] " + msg);
            }
        }
    }
}
