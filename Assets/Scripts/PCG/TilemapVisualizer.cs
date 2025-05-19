using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dev.Akai.PCG
{
    public class TilemapVisualizer
    {
        private TilesetData tileSet;

        private Tilemap backgroundTilemap;
        private Tilemap floorTilemap;
        private Tilemap wallTilemap;
        private Tilemap spawnRoomTilemap;

        private Dictionary<int, TileBase> wallTileMapping;

        // ---- Public Methods ----

        public TilemapVisualizer(TilesetData tileSet, Tilemap floorMap, Tilemap wallMap, Tilemap backgroundMap, Tilemap spawnRoomMap)
        {
            this.tileSet = tileSet;
            this.floorTilemap = floorMap;
            this.wallTilemap = wallMap;
            this.backgroundTilemap = backgroundMap;
            this.spawnRoomTilemap = spawnRoomMap;

            InitializeWallTileMapping();
        }

        public void Clear()
        {
            backgroundTilemap.ClearAllTiles();
            floorTilemap.ClearAllTiles();
            wallTilemap.ClearAllTiles();
        }

        public void ClearSpawnRoom()
        {
            spawnRoomTilemap.ClearAllTiles();
        }

        public void PaintBackground(int width, int height, Vector2Int startPosition)
        {
            int backgroundWidth = width + 20;
            int backgroundHeight = height + 20;
            Vector2Int backgroundStartPos = new(startPosition.x - 10, startPosition.y - 10);

            for (int x = 0; x < backgroundWidth; x++)
            {
                for (int y = 0; y < backgroundHeight; y++)
                {
                    Vector2Int tilePosition = new Vector2Int(backgroundStartPos.x + x, backgroundStartPos.y + y);
                    PaintSingleTile(backgroundTilemap, tileSet.backgroundTile, tilePosition);
                }
            }
        }

        public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
        {
            foreach (var pos in floorPositions)
            {
                PaintSingleTile(floorTilemap, tileSet.floorTile, pos);
            }
        }

        public void PaintSingleBasicWall(Vector2Int position, string binaryType)
        {
            int typeAsInt = Convert.ToInt32(binaryType, 2);
            TileBase tile = GetBasicWallTile(typeAsInt);

            if (tile != null)
                PaintSingleTile(wallTilemap, tile, position);
        }

        internal void PaintSingleCornerWall(Vector2Int position, string binaryType)
        {
            int typeAsInt = Convert.ToInt32(binaryType, 2);
            TileBase tile = GetCornerWallTile(typeAsInt);

            if (tile != null)
                PaintSingleTile(wallTilemap, tile, position);
        }

        // ---- Private Methods ----

        private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
        {
            var tilePosition = tilemap.WorldToCell((Vector3Int)position);
            tilemap.SetTile(tilePosition, tile);
        }

        private void InitializeWallTileMapping()
        {
            wallTileMapping = new();

            foreach (int type in WallTypesHelper.WallTop)
                wallTileMapping[type] = tileSet.topWallTile;

            foreach (int type in WallTypesHelper.WallRight)
                wallTileMapping[type] = tileSet.rightWallTile;

            foreach (int type in WallTypesHelper.WallLeft)
                wallTileMapping[type] = tileSet.leftWallTile;

            foreach (int type in WallTypesHelper.WallBottom)
                wallTileMapping[type] = tileSet.bottomWallTile;

            foreach (int type in WallTypesHelper.WallLeftRight)
                wallTileMapping[type] = tileSet.wallLeftRightTile;

            foreach (int type in WallTypesHelper.WallTopLeftRight)
                wallTileMapping[type] = tileSet.wallTopLeftRightCorner;
        }

        private TileBase GetBasicWallTile(int type)
        {
            return wallTileMapping.TryGetValue(type, out TileBase tile) ? tile : null;
        }

        private TileBase GetCornerWallTile(int type)
        {
            // Inner corners
            if (WallTypesHelper.WallInnerCornerDownLeft.Contains(type))
                return tileSet.wallInnerCornerDownLeft;

            if (WallTypesHelper.WallInnerCornerDownRight.Contains(type))
                return tileSet.wallInnerCornerDownRight;

            //Diagonal corners
            if (WallTypesHelper.WallDiagonalCornerUpLeft.Contains(type))
                //PaintDiagonalUpCorner(position, Vector2Int.right, WallDiagonalCornerUpLeft);
                return null;

            if (WallTypesHelper.WallDiagonalCornerUpRight.Contains(type))
                //PaintDiagonalUpCorner(position, Vector2Int.left, WallDiagonalCornerUpRight);
                return null;

            return null;
        }

        //private void PaintDiagonalUpCorner(Vector2Int position, Vector2Int offsetDirection, TileBase cornerTile)
        //{
        //    Vector2Int newPosition = position + Vector2Int.down + offsetDirection;
        //    Vector2Int newSideTile = newPosition + offsetDirection;

        //    if (topWallsTilemap.GetTile((Vector3Int)newSideTile) == null && floorTilemap.GetTile((Vector3Int)newSideTile) != null)
        //    {
        //        PaintSingleTile(wallTilemap, cornerTile, newPosition);

        //        Vector2Int newUpTile = newPosition + Vector2Int.up;
        //        Vector2Int newTwoTilesUp = newUpTile + Vector2Int.up;
        //        wallTilemap.SetTile((Vector3Int)newUpTile, null);
        //        topWallsTilemap.SetTile((Vector3Int)newTwoTilesUp, null);

        //        if (topWallsTilemap.GetTile((Vector3Int)position) == null)
        //        {
        //            Vector2Int sideWallTile = newPosition - offsetDirection;
        //            wallTilemap.SetTile((Vector3Int)sideWallTile, null);
        //        }
        //    }
        //    else
        //    {
        //        PaintSingleTile(wallTilemap, wallTopTile, newPosition + Vector2Int.up);
        //        PaintSingleTile(topWallsTilemap, upperWallTopTile, newPosition + Vector2Int.up * 2);
        //        topWallsTilemap.SetTile((Vector3Int)newPosition, null);
        //    }
        //}
    }
}
