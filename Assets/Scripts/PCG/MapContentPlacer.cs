using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.Akai.PCG
{
    public static class MapContentPlacer
    {
        public static void PlaceContent(Dictionary<Vector2Int, int> dijkstraMap, List<BoundsInt> roomsList, HashSet<Vector2Int> floorPositions)
        {
            List<(Vector2Int center, int distance)> orderedRooms = DijkstraMapGenerator.GetRoomsOrderedByDijkstraDistance(roomsList, dijkstraMap);

            //Vector2Int midRoom = orderedRooms[orderedRooms.Count * 2 / 3].center;

            //if (Diary.Instance == null)
            //{
            //    Vector2Int diaryRoomCenter = orderedRooms[1].center;
            //    Instantiate(diaryPrefab, new Vector3(diaryRoomCenter.x + 0.5f, diaryRoomCenter.y + 0.5f, 0f), Quaternion.identity);
            //}

            //Vector2Int farthestRoomCenter = orderedRooms.Last().center;
            //Instantiate(exitDoorPrefab, new Vector3(farthestRoomCenter.x + 0.5f, farthestRoomCenter.y + 0.5f, 0f), Quaternion.identity);
        }
    }
}
