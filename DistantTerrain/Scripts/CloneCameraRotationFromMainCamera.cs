//Distant Terrain Mod for Daggerfall-Unity
//http://www.reddit.com/r/dftfu
//http://www.dfworkshop.net/
//Author: Michael Rauter (a.k.a. Nystul)
//License: MIT License (http://www.opensource.org/licenses/mit-license.php)

using UnityEngine;
using DaggerfallWorkshop.Game;

namespace DistantTerrain
{
    public class CloneCameraRotationFromMainCamera : MonoBehaviour
    {
        void LateUpdate()
        {
            this.transform.rotation = Camera.main.transform.rotation;
        }

    }
}