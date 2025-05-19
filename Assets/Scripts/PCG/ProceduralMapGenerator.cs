using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dev.Akai.PCG
{
    public static class ProceduralMapGenerator
    {
        private static List<RoomData> roomDataList = new();
        private static HashSet<Vector2Int> floorPositions;
        private static HashSet<Vector2Int> corridorPositions;
        private static HashSet<Vector2Int> wallPositions;
        private static List<Color> roomGizmoColors = new();
        private static RoomData spawnRoom;

        // ---- Public Methods ----

        public static void ClearRoomData()
        {
            roomDataList?.Clear();
            roomGizmoColors?.Clear();
        }

        public static MapGeneratedData GenerateRooms(MapGenerationParameters config)
        {
            // centers the Start Point and creates the map area around it
            config.StartPoint += new Vector2Int(-config.MapWidth / 2, -config.MapHeight / 2);
            config.TilemapVisualizer.PaintBackground(config.MapWidth, config.MapHeight, config.StartPoint);
            BoundsInt mapArea = new BoundsInt((Vector3Int)config.StartPoint, new Vector3Int(config.MapWidth, config.MapHeight, 0));
            
            List<BoundsInt> roomBoundsList = ProceduralGenerationAlgorithms.SelectGenerationTechnique(config, mapArea);
            
            // Rooms
            floorPositions = new HashSet<Vector2Int>();
            spawnRoom = CreateSpawnRoomData(config.SpawnRoomTilemap, config.StartWithSpawnRoom, roomBoundsList);
            GenerateRoomFloors(roomBoundsList, config.Offset);

            // Corridors
            corridorPositions = new HashSet<Vector2Int>();
            if (config.GenerateCorridors)
            {
                corridorPositions = CorridorsGenerator.GenerateCorridors(config, roomDataList);
                floorPositions.UnionWith(corridorPositions);
            }

            // Visualization
            config.TilemapVisualizer.PaintFloorTiles(floorPositions);

            wallPositions = new (WallGenerator.GenerateWalls(floorPositions, config.TilemapVisualizer));

            var playerDijkstraMap = DijkstraMapGenerator.GenerateDijkstraMap(spawnRoom.Center, floorPositions);

            MapContentPlacer.PlaceContent(playerDijkstraMap, roomBoundsList, floorPositions);

            return new MapGeneratedData
            {
                RoomDataList = roomDataList,
                RoomColors = roomGizmoColors,
                CorridorPositions = corridorPositions,
                FloorPositions = floorPositions,
                PlayerDijkstraMap = playerDijkstraMap
            };
        }

        // ---- Private Methods ----

        #region Spawn Room Creation

        private static RoomData CreateSpawnRoomData(Tilemap spawnRoomTilemap, bool startWithSpawnRoom, List<BoundsInt> roomBoundsList)
        {
            Vector2Int roomCenter;
            HashSet<Vector2Int> roomFloor;

            if (spawnRoomTilemap != null && startWithSpawnRoom)
            {
                roomFloor = GetSpawnRoomLayout(spawnRoomTilemap);

                int sumX = 0, sumY = 0;
                foreach (var pos in roomFloor)
                {
                    sumX += pos.x;
                    sumY += pos.y;
                }

                if (roomFloor.Count == 0)
                    return null;

                roomCenter = new Vector2Int(sumX / roomFloor.Count, sumY / roomFloor.Count);

            }
            else if (roomBoundsList.Count > 0 || spawnRoomTilemap == null)
            {
                BoundsInt closestRoom = roomBoundsList
                    .OrderBy(room => Vector2Int.Distance((Vector2Int)Vector3Int.RoundToInt(room.center), Vector2Int.zero)).First();

                roomFloor = new();

                for (int col = 0; col < closestRoom.size.x; col++)
                {
                    for (int row = 0; row < closestRoom.size.y; row++)
                    {
                        Vector2Int position = (Vector2Int)closestRoom.min + new Vector2Int(col, row);
                        roomFloor.Add(position);
                    }
                }

                roomCenter = (Vector2Int)Vector3Int.RoundToInt(closestRoom.center);
            }
            else
            {
                return null;
            }
            
            spawnRoom = new RoomData(roomCenter, roomFloor);
            SaveRoomData(roomCenter, roomFloor);
            return spawnRoom;
        }

        private static HashSet<Vector2Int> GetSpawnRoomLayout(Tilemap tilemap)
        {
            HashSet<Vector2Int> roomFloor = new();
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] tiles = tilemap.GetTilesBlock(bounds);

            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    TileBase tile = tiles[x + y * bounds.size.x];
                    if (tile != null)
                    {
                        Vector3Int tilePos = new Vector3Int(x + bounds.xMin, y + bounds.yMin, 0);
                        roomFloor.Add((Vector2Int)tilePos);
                    }
                }
            }

            return roomFloor;
        }

        #endregion

        private static HashSet<Vector2Int> GenerateRoomFloors(List<BoundsInt> roomBoundsList, int offset)
        {
            HashSet<Vector2Int> floorPositions = new();

            foreach (var room in roomBoundsList)
            {
                Vector2Int roomCenter = (Vector2Int)Vector3Int.RoundToInt(room.center);

                if (spawnRoom != null && roomCenter == spawnRoom.Center)
                    continue;

                HashSet<Vector2Int> roomFloor = new();

                for (int col = offset; col < room.size.x - offset; col++)
                {
                    for (int row = offset; row < room.size.y - offset; row++)
                    {
                        Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                        roomFloor.Add(position);
                    }
                }

                if (!roomFloor.Overlaps(spawnRoom.FloorTiles))
                    SaveRoomData(roomCenter, roomFloor);
            }

            return floorPositions;
        }

        private static void SaveRoomData(Vector2Int roomCenter, HashSet<Vector2Int> roomFloor)
        {
            RoomData newRoom = new RoomData(roomCenter, roomFloor);
            roomDataList.Add(newRoom);
            floorPositions.UnionWith(roomFloor);
            roomGizmoColors.Add(UnityEngine.Random.ColorHSV());
        }
    }
}
