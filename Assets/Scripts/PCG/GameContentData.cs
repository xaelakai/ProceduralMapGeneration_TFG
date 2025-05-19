using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.Akai.PCG
{
    [CreateAssetMenu(menuName = "PCG/Game Content")]
    public class GameContentData : ScriptableObject
    {
        [Header("Prefabs References")]
        public GameObject[] Collectibles;
        public GameObject[] Enemies;
        public GameObject ExitDoor;
    }
}
