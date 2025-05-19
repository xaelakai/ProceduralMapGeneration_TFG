using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dev.Akai.PCG
{
    [CreateAssetMenu(menuName = "PCG/Tileset")]
    public class TilesetData : ScriptableObject
    {
        [Header("Basic Tiles")]
        public TileBase backgroundTile;
        public TileBase floorTile;

        [Header("Wall Tiles")]
        public TileBase topWallTile;
        public TileBase bottomWallTile;
        public TileBase leftWallTile;
        public TileBase rightWallTile;

        [Header("Corner Tiles")]
        public TileBase wallLeftRightTile;
        public TileBase wallTopLeftRightCorner;
        public TileBase wallInnerCornerDownLeft, wallInnerCornerDownRight;
        public TileBase wallDiagonalCornerUpLeft, wallDiagonalCornerUpRight;
    }
}
