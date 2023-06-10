using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTAssets.UltimateLODSystem
{
    /*
     This class is responsible for global management of all ULOD components
    */
    /*
     * The Ultimate LOD System was developed by Marcos Tomaz in 2020.
     * Need help? Contact me (mtassets@windsoft.xyz)
    */

    [AddComponentMenu("")] //Hide this script in component menu.
    public class UltimateLevelOfDetailGlobal : MonoBehaviour
    {
        //Private static variables
        private static bool enableGlobalUlodSystem = true;
        private static float lodDistanceMultiplier = 1.0f;

        //Public static variables
        public static Camera currentCameraThatIsOnTopOfScreenInThisScene = null;

        //Public and static methods

        public static bool isGlobalULodSystemEnabled()
        {
            //Return if the lod auto management is enabled
            return enableGlobalUlodSystem;
        }

        public static void EnableGlobalULodSystem(bool enable)
        {
            //Enable or disable the lod auto management by ULOD
            enableGlobalUlodSystem = enable;
        }

        public static void SetGlobalLodDistanceMultiplier(float multiplier)
        {
            //Set a new LOD distance multiplier
            lodDistanceMultiplier = multiplier;
        }

        public static float GetGlobalLodDistanceMultiplier()
        {
            //Return the global lod multiplier
            return lodDistanceMultiplier;
        }
    }
}