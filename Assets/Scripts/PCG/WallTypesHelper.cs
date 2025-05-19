using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.Akai.PCG
{
    public static class WallTypesHelper
    {
        // Basic Walls

        public static HashSet<int> WallTop = new()
        {
            0b0010,
            0b0110,
            0b0011,
            0b0111,
            0b1010,
            0b1110,
            0b1010,
            0b1111,
            0b1011,
        };

        public static HashSet<int> WallLeft = new()
        {
            0b0100
        };

        public static HashSet<int> WallRight = new()
        {
            0b0001
        };

        public static HashSet<int> WallBottom = new()
        {
            0b1000
        };

        public static HashSet<int> WallLeftRight = new()
        {
            0b0101
        };

        public static HashSet<int> WallTopLeftRight = new()
        {
            0b1101
        };

        // Corner Walls

        public static HashSet<int> WallInnerCornerDownLeft = new()
        {
            0b11110001,
            0b11110011,
            0b11100000,
            0b11110000,
            0b11100001,
            0b10100000,
            0b01010001,
            0b11010001,
            0b11010000,
            0b10110001,
            0b10100001,
            0b10010000,
            //0b00110001,
            0b10110000,
            0b00100001,
            0b10010001
        };

        public static HashSet<int> WallInnerCornerDownRight = new()
        {
            0b11000111,
            0b11000011,
            0b10000011,
            0b10000111,
            0b10000010,
            0b01000101,
            0b11000101,
            //0b01000011,
            0b10000101,
            0b11000110,
            0b11000010,
            0b10000100,
            0b10000110,
            0b11000100,
            0b01000010
        };

        public static HashSet<int> WallDiagonalCornerUpLeft = new()
        {
            0b00010000,
            0b00010111,
            0b00010011
        };

        public static HashSet<int> WallDiagonalCornerUpRight = new()
        {
            0b00000100,
            0b00000101,
        };
    }
}
