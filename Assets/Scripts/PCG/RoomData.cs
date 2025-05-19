using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.Akai.PCG
{
    public class RoomData
    {
        public Vector2Int Center { get; private set; }
        public HashSet<Vector2Int> FloorTiles { get; private set; } = new();
        public List<Vector2Int> DoorPositions { get; private set; } = new();
        public int RoomID { get; private set; }

        public RoomData(Vector2Int centerPosition, HashSet<Vector2Int> floorTiles)
        {
            this.Center = centerPosition;
            this.FloorTiles = floorTiles;
        }

        public void GenerateDoors(int numberOfDoors)
        {
            DoorPositions.Clear();

            var basicWallPositions = WallGenerator.FindWallsInDirections(FloorTiles, Direction2D.cardinalDirectionsList);

            List<Vector2Int> wallList = new(basicWallPositions);
            int totalWallTiles = wallList.Count;
            int maxNumberOfDoors = Mathf.Min(numberOfDoors, wallList.Count);

            int attempts = 0;

            while (DoorPositions.Count < maxNumberOfDoors && attempts < 100)
            {
                int index = Random.Range(0, totalWallTiles);
                Vector2Int doorTile = wallList[index];
                if (!DoorPositions.Contains(doorTile))
                {
                    DoorPositions.Add(doorTile);
                }
                attempts++;
            }
        }
    }
}
