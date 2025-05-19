using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dev.Akai.PCG
{
    public static class CorridorsGenerator
    {
        public static HashSet<Vector2Int> GenerateCorridors(MapGenerationParameters config, List<RoomData> roomsDataList)
        {
            HashSet<Vector2Int> corridorsPositions = new();

            switch (config.CorridorsGenerationTechnique)
            {
                case CorridorsGenerationTechnique.Method1:
                    corridorsPositions = GenerateCorridorsUsingMethod1(config, roomsDataList);
                    break;

                case CorridorsGenerationTechnique.Method2:
                    corridorsPositions = GenerateCorridorsUsingMethod2(config, roomsDataList);
                    break;

                case CorridorsGenerationTechnique.MinimumSpanningTree:
                    corridorsPositions = GenerateCorridorsUsingMST(config, roomsDataList);
                    break;
            }

            return corridorsPositions;
        }

        private static HashSet<Vector2Int> GenerateCorridorsUsingMethod1(MapGenerationParameters config, List<RoomData> roomsDataList)
        {
            List<Vector2Int> roomCenters = new();

            foreach (var room in roomsDataList)
            {
                roomCenters.Add(room.Center);
            }

            return ConnectRooms(roomCenters);
        }

        private static HashSet<Vector2Int> GenerateCorridorsUsingMethod2(MapGenerationParameters config, List<RoomData> roomsDataList)
        {
            List<Vector2Int> roomDoors = new();

            foreach (RoomData room in roomsDataList)
            {
                if (room == roomsDataList[0])
                    room.GenerateDoors(config.NumberOfInitialRoomDoors);
                else
                    room.GenerateDoors(config.NumberOfDefaultRoomDoors);

                roomDoors.AddRange(room.DoorPositions);
            }

            return ConnectRoomsByDoors2(roomDoors, roomsDataList);
        }

        private static HashSet<Vector2Int> GenerateCorridorsUsingMST(MapGenerationParameters config, List<RoomData> roomsDataList)
        {
            foreach (RoomData room in roomsDataList)
            {
                if (room == roomsDataList[0])
                    room.GenerateDoors(config.NumberOfInitialRoomDoors);
                else
                    room.GenerateDoors(config.NumberOfDefaultRoomDoors);
            }

            return ConnectRoomsByMST(roomsDataList, config.CorridorsUsingDoors);
        }

        // ---- Room Connection Methods ----

        private static HashSet<Vector2Int> ConnectRoomsByMST(List<RoomData> roomsDataList, bool connectByDoors)
        {
            HashSet<Vector2Int> corridors = new();
            List<RoomData> connectedRooms = new() { roomsDataList[0] };
            List<RoomData> unconnectedRooms = new(roomsDataList);
            unconnectedRooms.RemoveAt(0);

            while (unconnectedRooms.Count > 0)
            {
                float minDistance = float.MaxValue;
                Vector2Int bestFrom = Vector2Int.zero;
                Vector2Int bestTo = Vector2Int.zero;
                RoomData fromThisRoom = null;
                RoomData toThisRoom = null;

                foreach (var room1 in connectedRooms)
                {
                    foreach (var room2 in unconnectedRooms)
                    {
                        if (connectByDoors)
                        {
                            foreach (var door1 in room1.DoorPositions)
                            {
                                foreach (var door2 in room2.DoorPositions)
                                {
                                    float dist = Vector2.Distance(door1, door2);
                                    if (dist < minDistance)
                                    {
                                        minDistance = dist;
                                        bestFrom = door1;
                                        bestTo = door2;
                                        fromThisRoom = room1;
                                        toThisRoom = room2;
                                    }
                                }
                            }
                        }
                        else
                        {
                            float dist = Vector2.Distance(room1.Center, room2.Center);
                            if (dist < minDistance)
                            {
                                minDistance = dist;
                                bestFrom = room1.Center;
                                bestTo = room2.Center;
                                fromThisRoom = room1;
                                toThisRoom = room2;
                            }
                        }
                    }
                }

                var newCorridor = CreateCorridor(bestFrom, bestTo);
                corridors.UnionWith(newCorridor);

                connectedRooms.Add(toThisRoom);
                unconnectedRooms.Remove(toThisRoom);
            }

            return corridors;
        }

        private static HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
        {
            HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
            var currentRoomCenter = roomCenters[0];
            roomCenters.RemoveAt(0);

            while (roomCenters.Count > 0)
            {
                Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
                roomCenters.Remove(closest);

                HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
                currentRoomCenter = closest;
                corridors.UnionWith(newCorridor);
            }

            return corridors;
        }




        #region Handle

        // ---- Public Methods ----


        public static HashSet<Vector2Int> ConnectRoomsByDoors(List<Vector2Int> roomDoors, HashSet<Vector2Int> floorPositions, List<RoomData> roomsDataList)
        {
            HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
            var currentDoor = roomDoors[0];
            roomDoors.RemoveAt(0);

            while (roomDoors.Count > 0)
            {
                List<Vector2Int> validCandidates = new List<Vector2Int>();

                foreach (var candidateDoor in roomDoors)
                {
                    if (!AreDoorsInTheSameRoom(currentDoor, candidateDoor, roomsDataList))
                    {
                        validCandidates.Add(candidateDoor);
                    }
                }

                Vector2Int closestDoor = FindClosestPointTo(currentDoor, validCandidates);
                roomDoors.Remove(closestDoor);

                HashSet<Vector2Int> newCorridor = CreateCorridor2(currentDoor, closestDoor, floorPositions);
                currentDoor = closestDoor;
                corridors.UnionWith(newCorridor);
            }

            return corridors;
        }

        

        public static HashSet<Vector2Int> ConnectRoomsByDoors2(List<Vector2Int> roomDoors, List<RoomData> allRoomsList)
        {
            HashSet<Vector2Int> corridors = new();
            HashSet<RoomData> connectedRooms = new();
            List<Vector2Int> usedDoors = new();

            Vector2Int currentDoor = roomDoors[0];
            RoomData currentRoom = FindRoomForDoor(currentDoor, allRoomsList);
            connectedRooms.Add(currentRoom);
            usedDoors.Add(currentDoor);

            roomDoors.Remove(currentDoor);

            while (connectedRooms.Count < allRoomsList.Count)
            {
                float minDistance = float.MaxValue;
                Vector2Int bestFrom = Vector2Int.zero;
                Vector2Int bestTo = Vector2Int.zero;
                RoomData toRoom = null;

                foreach (var fromDoor in usedDoors)
                {
                    foreach (var candidateDoor in roomDoors)
                    {
                        RoomData candidateRoom = FindRoomForDoor(candidateDoor, allRoomsList);

                        if (connectedRooms.Contains(candidateRoom))
                            continue;

                        float dist = Vector2.Distance(fromDoor, candidateDoor);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            bestFrom = fromDoor;
                            bestTo = candidateDoor;
                            toRoom = candidateRoom;
                        }
                    }
                }

                var newCorridor = CreateCorridor(bestFrom, bestTo);
                corridors.UnionWith(newCorridor);
                connectedRooms.Add(toRoom);
                usedDoors.Add(bestTo);
                roomDoors.Remove(bestTo);
            }

            return corridors;
        }

        private static RoomData FindRoomForDoor(Vector2Int door, List<RoomData> rooms)
        {
            return rooms.FirstOrDefault(room => room.DoorPositions.Contains(door));
        }

        public static HashSet<Vector2Int> ConnectRoomsByFarthestDoors(List<Vector2Int> roomDoors, HashSet<Vector2Int> floorPositions, List<RoomData> allRoomsList)
        {
            HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();

            var currentDoor = roomDoors[0];
            roomDoors.RemoveAt(0);

            while (roomDoors.Count > 0)
            {
                Vector2Int closestDoor = Vector2Int.zero;

                foreach (var candidateDoor in roomDoors)
                {
                    if (AreDoorsInTheSameRoom(currentDoor, candidateDoor, allRoomsList)) continue;

                    closestDoor = candidateDoor;
                }

                roomDoors.Remove(closestDoor);

                HashSet<Vector2Int> newCorridor = CreateCorridor2(currentDoor, closestDoor, floorPositions);
                currentDoor = closestDoor;
                corridors.UnionWith(newCorridor);
            }
            return corridors;
        }

        private static Vector2Int FindClosestPointTo(Vector2Int currentPosition, List<Vector2Int> pointsList)
        {
            Vector2Int closestPoint = pointsList[0];
            float distance = float.MaxValue;

            foreach (var position in pointsList)
            {
                float minDistance = Vector2.Distance(position, currentPosition);
                if (minDistance < distance)
                {
                    distance = minDistance;
                    closestPoint = position;
                }
            }
            return closestPoint;
        }

        private static HashSet<Vector2Int> CreateCorridor(Vector2Int initialPosition, Vector2Int finalPosition)
        {
            HashSet<Vector2Int> corridorTiles = new();

            Vector2Int currentPos = initialPosition;

            // Vertical pathing
            while (currentPos.y != finalPosition.y)
            {
                corridorTiles.Add(currentPos);
                currentPos.y += currentPos.y < finalPosition.y ? 1 : -1;
            }

            // Horizontal pathing
            while (currentPos.x != finalPosition.x)
            {
                corridorTiles.Add(currentPos);
                currentPos.x += currentPos.x < finalPosition.x ? 1 : -1;
            }

            corridorTiles.Add(finalPosition);

            return corridorTiles;
        }

        private static bool AreDoorsInTheSameRoom(Vector2Int doorA, Vector2Int doorB, List<RoomData> allRoomsList)
        {
            foreach (var room in allRoomsList)
            {
                if (room.DoorPositions.Contains(doorA) && room.DoorPositions.Contains(doorB))
                {
                    return true;
                }
            }
            return false;
        }

        private static HashSet<Vector2Int> CreateCorridor2(Vector2Int from, Vector2Int to, HashSet<Vector2Int> floorPositions, int maxAttempts = 1000)
        {
            HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
            Vector2Int current = from;
            corridor.Add(current);

            bool moveXFirst = UnityEngine.Random.value < 0.5f;

            int attempts = 0;

            while (current != to && attempts < maxAttempts)
            {
                attempts++;

                Vector2Int step = Vector2Int.zero;

                // decide direction by random priority
                if (moveXFirst && current.x != to.x)
                {
                    step = (to.x > current.x) ? Vector2Int.right : Vector2Int.left;
                }
                else if (current.y != to.y)
                {
                    step = (to.y > current.y) ? Vector2Int.up : Vector2Int.down;
                }
                else if (current.x != to.x)
                {
                    step = (to.x > current.x) ? Vector2Int.right : Vector2Int.left;
                }

                Vector2Int nextPosition = current + step;

                // if the next initialPosition is in floorPositions, tries to go around
                if (floorPositions.Contains(nextPosition))
                {
                    // lateral options (change axis of movement)
                    List<Vector2Int> alternatives = new List<Vector2Int>();

                    if (step == Vector2Int.up || step == Vector2Int.down)
                    {
                        alternatives.Add(current + Vector2Int.left);
                        alternatives.Add(current + Vector2Int.right);
                    }
                    else
                    {
                        alternatives.Add(current + Vector2Int.up);
                        alternatives.Add(current + Vector2Int.down);
                    }

                    bool moved = false;
                    foreach (var alt in alternatives)
                    {
                        if (!floorPositions.Contains(alt))
                        {
                            current = alt;
                            corridor.Add(current);
                            moved = true;
                            break;
                        }
                    }

                    // if it's unable to go around, force step (to avoid infinite loop)
                    if (!moved)
                    {
                        current = nextPosition;
                        corridor.Add(current);
                    }
                }
                else
                {
                    current = nextPosition;
                    corridor.Add(current);
                }
            }

            return corridor;
        }

        private static HashSet<Vector2Int> CreateCorridor3(Vector2Int from, Vector2Int to, HashSet<Vector2Int> floorPositions, int thickness = 3)
        {
            HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
            Vector2Int position = from;
            corridor.Add(position);

            int maxAttempts = 1000;
            int attempts = 0;

            while (position != to && attempts < maxAttempts)
            {
                attempts++;

                Vector2Int step = Vector2Int.zero;

                // Prioridad: eje Y primero
                if (position.y != to.y)
                    step = (to.y > position.y) ? Vector2Int.up : Vector2Int.down;
                else if (position.x != to.x)
                    step = (to.x > position.x) ? Vector2Int.right : Vector2Int.left;

                Vector2Int nextPosition = position + step;

                if (floorPositions.Contains(nextPosition))
                {
                    List<Vector2Int> alternatives = new List<Vector2Int>();

                    if (step == Vector2Int.up || step == Vector2Int.down)
                    {
                        alternatives.Add(position + Vector2Int.left);
                        alternatives.Add(position + Vector2Int.right);
                    }
                    else
                    {
                        alternatives.Add(position + Vector2Int.up);
                        alternatives.Add(position + Vector2Int.down);
                    }

                    bool moved = false;
                    foreach (var alt in alternatives)
                    {
                        if (!floorPositions.Contains(alt))
                        {
                            position = alt;
                            corridor.UnionWith(ThickPosition(position, step, thickness));
                            moved = true;
                            break;
                        }
                    }

                    if (!moved)
                    {
                        position = nextPosition;
                        corridor.UnionWith(ThickPosition(position, step, thickness));
                    }
                }
                else
                {
                    position = nextPosition;
                    corridor.UnionWith(ThickPosition(position, step, thickness));
                }
            }

            return corridor;
        }

        private static IEnumerable<Vector2Int> ThickPosition(Vector2Int center, Vector2Int direction, int thickness)
        {
            List<Vector2Int> thickTiles = new List<Vector2Int>();
            thickTiles.Add(center);

            Vector2Int perpendicularA = new Vector2Int(-direction.y, direction.x); // rota 90°
            Vector2Int perpendicularB = new Vector2Int(direction.y, -direction.x); // rota -90°

            int half = (thickness - 1) / 2;

            for (int i = 1; i <= half; i++)
            {
                thickTiles.Add(center + perpendicularA * i);
                thickTiles.Add(center + perpendicularB * i);
            }

            return thickTiles;
        }

        #endregion
    }
}

