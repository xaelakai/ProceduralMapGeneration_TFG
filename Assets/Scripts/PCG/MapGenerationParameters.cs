using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dev.Akai.PCG
{
    public class MapGenerationParameters
    {
        public ProceduralGenerationTechnique MapGenerationAlgorithm;

        public Vector2Int StartPoint;
        public TilemapVisualizer TilemapVisualizer;
        public Tilemap SpawnRoomTilemap;

        public int MinRoomWidth;
        public int MinRoomHeight;
        public int MapWidth;
        public int MapHeight;
        public int Offset;
        public bool StartWithSpawnRoom;

        public bool GenerateCorridors;
        public CorridorsGenerationTechnique CorridorsGenerationTechnique;
        public bool CorridorsUsingDoors;
        public int NumberOfInitialRoomDoors;
        public int NumberOfDefaultRoomDoors;
    }

    public class MapGeneratedData
    {
        public List<RoomData> RoomDataList;
        public List<Color> RoomColors;
        public HashSet<Vector2Int> CorridorPositions;
        public HashSet<Vector2Int> FloorPositions;
        public Dictionary<Vector2Int, int> PlayerDijkstraMap;
    }
}
