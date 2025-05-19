using Dev.Akai.PCG;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Dev.Akai
{
    public class MapValidationResult
    {
        public bool HasErrors => Errors.Count > 0;
        public List<string> Errors = new();
    }

    [CustomEditor(typeof(ProceduralMapManager), true)]
    public class ProceduralMapManagerEditor : Editor
    {
        ProceduralMapManager manager;

        #region Serialized Properties

        SerializedProperty mapGenerationAlgorithm;
        SerializedProperty tileset;
        SerializedProperty startPoint;
        SerializedProperty mapWidth;
        SerializedProperty mapHeight;

        SerializedProperty backgroundTilemap;
        SerializedProperty floorTilemap;
        SerializedProperty wallsTilemap;
        SerializedProperty spawnRoomTilemap;

        SerializedProperty minRoomWidth;
        SerializedProperty minRoomHeight;
        SerializedProperty offset;
        SerializedProperty startWithSpawnRoom;

        SerializedProperty generateCorridors;
        SerializedProperty corridorsGenerationTechnique;
        SerializedProperty corridorsUsingDoors;
        SerializedProperty numberOfInitialRoomDoors;
        SerializedProperty numberOfDefaultRoomDoors;

        SerializedProperty showRoomAreas;
        SerializedProperty showCorridorsPath;
        SerializedProperty showDijkstraMap;
        SerializedProperty showTileColors;
        SerializedProperty showTileLabels;

        SerializedProperty gameContentData;

        #endregion

        private bool showTilemapSettings = true;
        private bool showRoomsSettings = true;
        private bool showCorridorsSettings = true;
        private bool showGizmosSettings = true;
        private bool showGameContentSettings = true;

        private void OnEnable()
        {
            manager = (ProceduralMapManager)target;

            mapGenerationAlgorithm = serializedObject.FindProperty("mapGenerationAlgorithm");
            startPoint = serializedObject.FindProperty("startPoint");
            tileset = serializedObject.FindProperty("tileset");
            minRoomWidth = serializedObject.FindProperty("minRoomWidth");
            minRoomHeight = serializedObject.FindProperty("minRoomHeight");

            backgroundTilemap = serializedObject.FindProperty("backgroundTilemap");
            floorTilemap = serializedObject.FindProperty("floorTilemap");
            wallsTilemap = serializedObject.FindProperty("wallsTilemap");
            spawnRoomTilemap = serializedObject.FindProperty("spawnRoomTilemap");

            mapWidth = serializedObject.FindProperty("mapWidth");
            mapHeight = serializedObject.FindProperty("mapHeight");
            offset = serializedObject.FindProperty("offset");
            startWithSpawnRoom = serializedObject.FindProperty("startWithSpawnRoom");

            generateCorridors = serializedObject.FindProperty("generateCorridors");
            corridorsGenerationTechnique = serializedObject.FindProperty("corridorsGenerationTechnique");
            corridorsUsingDoors = serializedObject.FindProperty("corridorsUsingDoors");
            numberOfInitialRoomDoors = serializedObject.FindProperty("numberOfInitialRoomDoors");
            numberOfDefaultRoomDoors = serializedObject.FindProperty("numberOfDefaultRoomDoors");

            showRoomAreas = serializedObject.FindProperty("showRoomAreas");
            showCorridorsPath = serializedObject.FindProperty("showCorridorsPath");
            showDijkstraMap = serializedObject.FindProperty("showDijkstraMap");
            showTileColors = serializedObject.FindProperty("showDijkstraTileColors");
            showTileLabels = serializedObject.FindProperty("showDijkstraTileLabels");

            gameContentData = serializedObject.FindProperty("gameContentData");
        }

        public override void OnInspectorGUI()
        {
            MapValidationResult validation = Validate(manager);

            serializedObject.Update();

            EditorGUILayout.PropertyField(mapGenerationAlgorithm);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Map Dimensions", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(startPoint, new GUIContent("Start Point", "Center point for the map generation."));
            
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Width", GUILayout.Width(80));
            mapWidth.intValue = EditorGUILayout.IntField(mapWidth.intValue, GUILayout.MinWidth(20));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Height", GUILayout.Width(80));
            mapHeight.intValue = EditorGUILayout.IntField(mapHeight.intValue, GUILayout.MinWidth(20));
            EditorGUI.indentLevel--;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Tilemap Settings
            showTilemapSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showTilemapSettings, "Tilemap Settings");
            if (showTilemapSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(tileset);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Tilemap References", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(backgroundTilemap);
                EditorGUILayout.PropertyField(floorTilemap);
                EditorGUILayout.PropertyField(wallsTilemap);
                EditorGUILayout.PropertyField(spawnRoomTilemap);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            EditorGUILayout.Space();

            // Room Parameters Layout
            showRoomsSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showRoomsSettings, "Room Settings");
            if (showRoomsSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(minRoomWidth);
                EditorGUILayout.PropertyField(minRoomHeight);
                EditorGUILayout.PropertyField(offset);
                EditorGUILayout.PropertyField(startWithSpawnRoom);

                EditorGUI.indentLevel--;

            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();

            // Corridors Parameters Layout
            showCorridorsSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCorridorsSettings, "Corridor Settings");
            if (showCorridorsSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(generateCorridors, new GUIContent("Generate Corridors", "Enable or disable corridor generation."));
                if (generateCorridors.boolValue)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(corridorsGenerationTechnique, new GUIContent("Corridor Generation Method"));
                    EditorGUILayout.PropertyField(corridorsUsingDoors, new GUIContent("Corridors Using Doors", "Whether corridors should be connected using doors."));
                    if (corridorsUsingDoors.boolValue)
                    {
                        EditorGUILayout.PropertyField(numberOfInitialRoomDoors);
                        EditorGUILayout.PropertyField(numberOfDefaultRoomDoors);
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();

            // Gizmos Layout
            showGizmosSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showGizmosSettings, "Gizmos");
            if (showGizmosSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(showRoomAreas);
                EditorGUILayout.PropertyField(showCorridorsPath);

                EditorGUILayout.PropertyField(showDijkstraMap);
                if (showDijkstraMap.boolValue)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(showTileColors);
                    EditorGUILayout.PropertyField(showTileLabels);

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;

            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();

            // Dijkstra Map Layout
            showGameContentSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showGameContentSettings, "Game Content");

            if (showGameContentSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(gameContentData);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Map Generation Tools", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            

            if (validation.HasErrors)
            {
                foreach (var error in validation.Errors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Click to generate a new map based on the current settings.", MessageType.Info);
            }

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(validation.HasErrors);

                if (GUILayout.Button("Generate Map"))
                {
                    manager.GenerateMap();
                }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            if (GUILayout.Button("Clear Map"))
            {
                manager.ClearMap();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Clear Spawn Room"))
            {
                manager.ClearSpawnRoom();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private MapValidationResult Validate(ProceduralMapManager manager)
        {
            MapValidationResult result = new();

            bool spawnRoom = startWithSpawnRoom.boolValue;
            int mapWidthValue = mapWidth.intValue;
            int mapHeightValue = mapHeight.intValue;
            int minRoomWidthValue = minRoomWidth.intValue;
            int minRoomHeightValue = minRoomHeight.intValue;
            Tilemap tilemap = spawnRoomTilemap.objectReferenceValue as Tilemap;

            if (spawnRoom && tilemap != null)
            {
                var bounds = tilemap.cellBounds;
                var tiles = tilemap.GetTilesBlock(bounds);

                bool hasTiles = false;
                foreach (var tile in tiles)
                {
                    if (tile != null)
                    {
                        hasTiles = true;
                        break;
                    }
                }

                if (!hasTiles)
                {
                    result.Errors.Add("El Tilemap de la sala inicial está vacío. Dibuja algunos tiles o desactiva 'Start With Spawn Room'.");
                }
            }

            if (mapWidthValue < minRoomWidthValue)
            {
                result.Errors.Add($"El ancho del mapa ({mapWidthValue}) no puede ser menor que el ancho mínimo de una sala ({minRoomWidthValue}).");
            }

            if (mapHeightValue < minRoomHeightValue)
            {
                result.Errors.Add($"El alto del mapa ({mapHeightValue}) no puede ser menor que el alto mínimo de una sala ({minRoomHeightValue}).");
            }

            if (minRoomWidthValue <= 0 || minRoomHeightValue <= 0)
            {
                result.Errors.Add("El tamaño mínimo de las habitaciones no puede ser inferior o igual a 0.");
            }

            return result;
        }
    }

}
