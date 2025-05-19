using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.Akai.PCG
{
    public static class DijkstraMapGenerator
    {
        public static Dictionary<Vector2Int, int> GenerateDijkstraMap(Vector2Int startPosition, HashSet<Vector2Int> floorPositions)
        {
            Dictionary<Vector2Int, int> distanceMap = new Dictionary<Vector2Int, int>();
            Queue<Vector2Int> frontier = new Queue<Vector2Int>();

            distanceMap[startPosition] = 0;
            frontier.Enqueue(startPosition);

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();
                int currentDistance = distanceMap[current];

                foreach (var direction in Direction2D.cardinalDirectionsList)
                {
                    Vector2Int neighbor = current + direction;

                    if (!floorPositions.Contains(neighbor)) continue;
                    if (distanceMap.ContainsKey(neighbor)) continue;

                    distanceMap[neighbor] = currentDistance + 1;
                    frontier.Enqueue(neighbor);
                }
            }

            return distanceMap;
        }

        public static List<(Vector2Int center, int distance)> GetRoomsOrderedByDijkstraDistance(List<BoundsInt> roomsList, Dictionary<Vector2Int, int> dijkstraMap)
        {
            List<(Vector2Int center, int distance)> orderedRooms = new List<(Vector2Int, int)>();
            foreach (var room in roomsList)
            {
                Vector2Int center = (Vector2Int)Vector3Int.RoundToInt(room.center);
                if (dijkstraMap.TryGetValue(center, out int distance))
                {
                    orderedRooms.Add((center, distance));
                }
            }
            orderedRooms.Sort((a, b) => a.distance.CompareTo(b.distance));

            return orderedRooms;
        }
    }
}
