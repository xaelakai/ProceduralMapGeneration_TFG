using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.Akai.PCG
{
    public static class ProceduralGenerationAlgorithms
    {
        public static List<BoundsInt> SelectGenerationTechnique(MapGenerationParameters config, BoundsInt initialArea)
        {
            List<BoundsInt> roomBoundsList = null;

            switch (config.MapGenerationAlgorithm)
            {
                case ProceduralGenerationTechnique.BinarySpacePartitioning:
                    roomBoundsList = BinarySpacePartitioning(initialArea, config.MinRoomWidth, config.MinRoomHeight, config.Offset);
                    break;
                case ProceduralGenerationTechnique.SimpleRandomRoomPlacement:
                    roomBoundsList = SimpleRandomRoomPlacement(initialArea, config.MinRoomWidth, config.MinRoomHeight);
                    break;
            }
            return roomBoundsList;
        }

        #region Binary Space Partitioning

        private static List<BoundsInt> BinarySpacePartitioning(BoundsInt initialArea, int minWidth, int minHeight, int offset)
        {
            List<BoundsInt> roomList = new List<BoundsInt>();
            Queue<BoundsInt> roomQueue = new Queue<BoundsInt>();
            roomQueue.Enqueue(initialArea);

            int partitionMinWidth = minWidth + offset * 2;
            int partitionMinHeight = minHeight + offset * 2;

            while (roomQueue.Count > 0)
            {
                BoundsInt currentArea = roomQueue.Dequeue();

                if (currentArea.size.x < minWidth || currentArea.size.y < minHeight)
                    continue;

                // Check if the current area can be split horizontally or vertically
                bool canSplitHorizontally = currentArea.size.y >= partitionMinHeight * 2;
                bool canSplitVertically = currentArea.size.x >= partitionMinWidth * 2;

                if (ShouldSplitHorizontally(canSplitHorizontally, canSplitVertically))
                {
                    SplitHorizontally(currentArea, partitionMinHeight, roomQueue, roomList);
                }
                else if (canSplitVertically)
                {
                    SplitVertically(currentArea, partitionMinWidth, roomQueue, roomList);
                }
                else
                {
                    // If the current area can't be split, add it to the final list
                    roomList.Add(currentArea);
                }
            }

            // Return the list of rooms after partitioning
            return roomList;
        }

        private static bool ShouldSplitHorizontally(bool canSplitHorizontally, bool canSplitVertically)
        {
            // If any type of split is possible, return false
            if (!canSplitHorizontally && !canSplitVertically)
                return false;

            // If vertical split is not possible, try horizontal
            if (canSplitHorizontally && !canSplitVertically)
                return true;

            // If horizontal split is not possible, try vertical
            if (!canSplitHorizontally && canSplitVertically)
                return false;

            // If both are possible, randomly choose a split direction
            return Random.value < 0.5f;
        }

        private static void SplitVertically(BoundsInt area, int partitionMinWidth, Queue<BoundsInt> roomQueue, List<BoundsInt> roomList)
        {
            int max = area.size.x - partitionMinWidth;

            if (partitionMinWidth >= max)
            {
                roomList.Add(area);
                return;
            }

            int xSplit = Random.Range(partitionMinWidth, max);

            BoundsInt left = new BoundsInt(area.min, new Vector3Int(xSplit, area.size.y, area.size.z));
            BoundsInt right = new BoundsInt(new Vector3Int(area.min.x + xSplit, area.min.y, area.min.z),
                                            new Vector3Int(area.size.x - xSplit, area.size.y, area.size.z));
            roomQueue.Enqueue(left);
            roomQueue.Enqueue(right);
        }

        private static void SplitHorizontally(BoundsInt area, int partitionMinHeight, Queue<BoundsInt> roomQueue, List<BoundsInt> roomList)
        {
            int max = area.size.y - partitionMinHeight;

            if (partitionMinHeight >= max)
            {
                roomList.Add(area);
                return;
            }

            int ySplit = Random.Range(partitionMinHeight, max);

            BoundsInt bottom = new BoundsInt(area.min, new Vector3Int(area.size.x, ySplit, area.size.z));
            BoundsInt top = new BoundsInt(new Vector3Int(area.min.x, area.min.y + ySplit, area.min.z),
                                            new Vector3Int(area.size.x, area.size.y - ySplit, area.size.z));
            roomQueue.Enqueue(bottom);
            roomQueue.Enqueue(top);
        }

        #endregion

        #region Simple Random Room Placement

        private static List<BoundsInt> SimpleRandomRoomPlacement(BoundsInt initialArea, int minWidth, int minHeight, int maxAttempts = 500, int maxRooms = 20)
        {
            List<BoundsInt> roomList = new List<BoundsInt>();

            int attempts = 0;

            while (attempts < maxAttempts && roomList.Count < maxRooms)
            {
                int roomWidth = Random.Range(minWidth, initialArea.size.x / 2);
                int roomHeight = Random.Range(minHeight, initialArea.size.y / 2);

                int xPos = Random.Range(initialArea.xMin, initialArea.xMax - roomWidth);
                int yPos = Random.Range(initialArea.yMin, initialArea.yMax - roomHeight);

                BoundsInt newRoom = new BoundsInt(new Vector3Int(xPos, yPos, 0), new Vector3Int(roomWidth, roomHeight, 1));

                bool overlaps = false;

                foreach (var room in roomList)
                {
                    if (RoomsOverlap(newRoom, room))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                    roomList.Add(newRoom);

                attempts++;
            }

            return roomList;
        }

        private static bool RoomsOverlap(BoundsInt a, BoundsInt b)
        {
            return a.xMin < b.xMax && a.xMax > b.xMin &&
                   a.yMin < b.yMax && a.yMax > b.yMin;
        }

        #endregion

        #region Simple Random Walk Rooms

        // to add

        #endregion
    }

    public static class Direction2D
    {
        public static List<Vector2Int> cardinalDirectionsList = new()
        {
            new Vector2Int(0, 1), // UP
            new Vector2Int(1, 0), // RIGHT
            new Vector2Int(0, -1), // DOWN
            new Vector2Int(-1, 0), // LEFT
        };

        public static List<Vector2Int> diagonalDirectionsList = new()
        {
            new Vector2Int(1, 1), // UP - RIGHT
            new Vector2Int(1, -1), // RIGHT - DOWN
            new Vector2Int(-1, -1), // DOWN - LEFT
            new Vector2Int(-1, 1), // LEFT - UP
        };

        public static List<Vector2Int> eightDirectionsList = new()
        {
            new Vector2Int(0, 1), // UP
            new Vector2Int(1, 1), // UP - RIGHT
            new Vector2Int(1, 0), // RIGHT
            new Vector2Int(1, -1), // RIGHT - DOWN
            new Vector2Int(0, -1), // DOWN
            new Vector2Int(-1, -1), // DOWN - LEFT
            new Vector2Int(-1, 0), // LEFT
            new Vector2Int(-1, 1) // LEFT - UP
        };

        public static Vector2Int GetRandomCardinalDirection()
        {
            return cardinalDirectionsList[Random.Range(0, cardinalDirectionsList.Count)];
        }
    }
}
