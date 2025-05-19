using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dev.Akai.PCG
{
    public enum ProceduralGenerationTechnique
    {
        BinarySpacePartitioning,
        SimpleRandomRoomPlacement
    }

    public enum CorridorsGenerationTechnique
    {
        Method1,
        Method2,
        MinimumSpanningTree
    }

    public class ProceduralMapManager : MonoBehaviour
    {
        #region Serialized Properties

        [SerializeField] ProceduralGenerationTechnique mapGenerationAlgorithm = ProceduralGenerationTechnique.BinarySpacePartitioning;

        [SerializeField] private TilesetData tileset;
        [SerializeField] private Vector2Int startPoint;
        [SerializeField, Min(0)] private int mapWidth = 40;
        [SerializeField, Min(0)] private int mapHeight = 40;

        // ---- Tilemap References ----
        [SerializeField] private Tilemap backgroundTilemap, floorTilemap, wallsTilemap;
        [SerializeField] private Tilemap spawnRoomTilemap = null;

        // ---- Room Parameters ----
        [SerializeField, Min(0)] private int minRoomWidth = 8;
        [SerializeField, Min(0)] private int minRoomHeight = 8;
        [SerializeField, Range(0, 10)] private int offset = 1;
        [SerializeField] private bool startWithSpawnRoom;

        // ---- Corridors Parameters ----
        [SerializeField] private bool generateCorridors = true;
        [SerializeField] private CorridorsGenerationTechnique corridorsGenerationTechnique = CorridorsGenerationTechnique.MinimumSpanningTree;
        [SerializeField] private bool corridorsUsingDoors = true;
        [SerializeField] private int numberOfDefaultRoomDoors = 3;
        [SerializeField] private int numberOfInitialRoomDoors = 1;

        // ---- Gizmo Toogles ----
        [SerializeField] private bool showRoomAreas = false;
        [SerializeField] private bool showCorridorsPath = false;
        [SerializeField] private bool showDijkstraMap = false, showDijkstraTileLabels, showDijkstraTileColors;

        // ---- Game Content ----
        [SerializeField] private GameContentData gameContentData;

        #endregion

        private TilemapVisualizer tilemapVisualizer;

        private List<RoomData> roomDataList;
        private List<Color> roomGizmoColors;
        private HashSet<Vector2Int> corridorPositions;
        private Dictionary<Vector2Int, int> playerDijkstraMap;

        public void GenerateMap()
        {
            ClearMap();
            RunProceduralGeneration();
        }

        public void ClearMap()
        {
            tilemapVisualizer = new TilemapVisualizer(tileset, floorTilemap, wallsTilemap, backgroundTilemap, spawnRoomTilemap);
            tilemapVisualizer?.Clear();

            if (spawnRoomTilemap != null)
                spawnRoomTilemap.gameObject.SetActive(startWithSpawnRoom);

            roomDataList?.Clear();
            roomGizmoColors?.Clear();
            corridorPositions?.Clear();
            playerDijkstraMap?.Clear();

            ProceduralMapGenerator.ClearRoomData();
        }

        public void ClearSpawnRoom()
        {
            tilemapVisualizer?.ClearSpawnRoom();
        }

        private void RunProceduralGeneration()
        {
            var config = new MapGenerationParameters
            {
                MapGenerationAlgorithm = mapGenerationAlgorithm,
                StartPoint = startPoint,
                TilemapVisualizer = tilemapVisualizer,
                SpawnRoomTilemap = spawnRoomTilemap,
                MinRoomWidth = minRoomWidth,
                MinRoomHeight = minRoomHeight,
                MapWidth = mapWidth,
                MapHeight = mapHeight,
                Offset = offset,
                StartWithSpawnRoom = startWithSpawnRoom,
                GenerateCorridors = generateCorridors,
                CorridorsGenerationTechnique = corridorsGenerationTechnique,
                CorridorsUsingDoors = corridorsUsingDoors,
                NumberOfInitialRoomDoors = numberOfInitialRoomDoors,
                NumberOfDefaultRoomDoors = numberOfDefaultRoomDoors,
            };

            var map = ProceduralMapGenerator.GenerateRooms(config);
            roomDataList = map.RoomDataList;
            roomGizmoColors = map.RoomColors;
            corridorPositions = map.CorridorPositions;
            playerDijkstraMap = map.PlayerDijkstraMap;
        }

        #region Gizmos

        private void OnDrawGizmos()
        {
            // ---- Room Gizmos ----
            if (showRoomAreas && roomDataList != null)
            {
                int colorIndex = 0;
                foreach (var room in roomDataList)
                {
                    // Room Areas
                    Gizmos.color = roomGizmoColors[colorIndex % roomGizmoColors.Count];
                    foreach (var pos in room.FloorTiles)
                    {
                        Gizmos.DrawCube(new Vector3(pos.x + 0.5f, pos.y + 0.5f), Vector3.one);
                    }

                    // Room Centers
                    if (room == roomDataList[0])
                        Gizmos.color = Color.blue;
                    else
                        Gizmos.color = Color.red;

                    Gizmos.DrawSphere(new Vector3(room.Center.x + 0.5f, room.Center.y + 0.5f, 0), 0.5f);

                    // Room Doors
                    if (corridorsUsingDoors)
                    {
                        Gizmos.color = Color.green;
                        foreach (var pos in room.DoorPositions)
                        {
                            Gizmos.DrawSphere(new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0), 0.5f);
                        }
                    }

                    colorIndex++;
                }
            }

            // ---- Corridors Gizmos ----
            if (showCorridorsPath && corridorPositions != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var pos in corridorPositions)
                {
                    Gizmos.DrawSphere(new Vector3(pos.x + 0.5f, pos.y + 0.5f), 0.3f);
                }
            }

            // ---- Dijkstra Map Gizmos ----
            if (showDijkstraMap && playerDijkstraMap != null)
            {
                UnityEditor.Handles.color = Color.white;

                foreach (var kvp in playerDijkstraMap)
                {
                    Vector2Int pos = kvp.Key;
                    int distance = kvp.Value;
                    Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);

                    if (showDijkstraTileLabels)
                    {
                        UnityEditor.Handles.Label(worldPos, distance.ToString());
                    }
                    if (showDijkstraTileColors)
                    {
                        float t = Mathf.InverseLerp(0, playerDijkstraMap.Values.Max(), distance);
                        Gizmos.color = Color.Lerp(Color.green, Color.red, t);
                        Gizmos.DrawCube(new Vector3(pos.x + 0.5f, pos.y + 0.5f), Vector3.one * 0.75f);
                    }
                }
            }
        }

        #endregion
    }
}
