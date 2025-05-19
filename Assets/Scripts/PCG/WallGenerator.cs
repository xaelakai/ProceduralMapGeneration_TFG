using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.Akai.PCG
{
    public static class WallGenerator
    {
        // ---- Public Methods ----

        public static HashSet<Vector2Int> GenerateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
        {
            HashSet<Vector2Int> basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.cardinalDirectionsList);
            HashSet<Vector2Int> cornerWallPositions = FindWallsInDirections(floorPositions, Direction2D.diagonalDirectionsList);

            HashSet<Vector2Int> wallPositions = new (basicWallPositions);
            wallPositions.UnionWith(cornerWallPositions);

            CreateBasicWalls(tilemapVisualizer, basicWallPositions, floorPositions);
            CreateCornerWalls(tilemapVisualizer, cornerWallPositions, floorPositions);

            return wallPositions;
        }

        public static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
        {
            HashSet<Vector2Int> wallPositions = new();
            foreach (var position in floorPositions)
            {
                foreach (var direction in directionList)
                {
                    var neighbourPosition = position + direction;
                    if (floorPositions.Contains(neighbourPosition) == false)
                        wallPositions.Add(neighbourPosition);
                }
            }
            return wallPositions;
        }

        // ---- Private Methods ----

        private static void CreateBasicWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPositions, HashSet<Vector2Int> floorPositions)
        {
            foreach (var position in basicWallPositions)
            {
                string neighboursBinaryType = "";
                foreach (var direction in Direction2D.cardinalDirectionsList)
                {
                    var neighbourPosition = position + direction;
                    if (floorPositions.Contains(neighbourPosition))
                    {
                        neighboursBinaryType += "1";
                    }
                    else
                    {
                        neighboursBinaryType += "0";
                    }
                }
                tilemapVisualizer.PaintSingleBasicWall(position, neighboursBinaryType);
            }
        }

        private static void CreateCornerWalls(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> cornerWallPositions, HashSet<Vector2Int> floorPositions)
        {
            foreach (var position in cornerWallPositions)
            {
                string neighboursBinaryType = "";
                foreach (var direction in Direction2D.eightDirectionsList)
                {
                    var neighbourPosition = position + direction;
                    if (floorPositions.Contains(neighbourPosition))
                    {
                        neighboursBinaryType += "1";
                    }
                    else
                    {
                        neighboursBinaryType += "0";
                    }
                }
                tilemapVisualizer.PaintSingleCornerWall(position, neighboursBinaryType);
            }
        }
    }
}

