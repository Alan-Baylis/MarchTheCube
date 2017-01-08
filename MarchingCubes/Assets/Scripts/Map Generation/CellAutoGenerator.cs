﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CellAutoGenerator : MonoBehaviour {

    public int width;
    public int height;
    public int depth;
    public float cubeSize = 5f;
    public int smoothingIterations = 5;
    
    public CapsuleCollider capColl;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    public int[,,] map;

    void Start () {
        GenerateMap();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.M)) {
            GenerateMap();
        }
    }

    void GenerateMap() {
        map = new int[width, height, depth];
        RandomFillMap();
        RemoveCellsInColl();

        for (int i = 0; i < smoothingIterations; i++) {
            SmoothMap();
        }

        ProcessMap();

        MarchingCubes marchingCubes = GetComponent<MarchingCubes>();
        marchingCubes.GenerateMesh(map, cubeSize);
    }

    void RandomFillMap() {
        if (useRandomSeed)
            seed = Time.time.ToString();

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {

                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1 || z == 0 || z == depth - 1) {
                        map[x, y, z] = 1;
                    } else {
                        map[x, y, z] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                    }
                }
            }
        }
    }

    void RemoveCellsInColl() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {
                    if (capColl.bounds.Contains(new Vector3(x, y, z))) {
                        map[x, y, z] = 0;
                    }
                }
            }
        }
    }

    void SmoothMap() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {
                    //if (capColl.bounds.Contains(new Vector3(x, y, z))) {
                      //  map[x, y, z] = 0;
                    //}

                    int neighbourWallTiles = GetSurroundingWallCount(x, y, z);
                    //Debug.Log(neighbourWallTiles);
                    if (neighbourWallTiles >= 15) {
                        map[x, y, z] = 1;
                    } else if (neighbourWallTiles < 13) {
                        map[x, y, z] = 0;
                    }
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY, int gridZ) {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                for (int neighbourZ = gridZ - 1; neighbourZ <= gridZ + 1; neighbourZ++) {

                    if (IsInMapRange(neighbourX, neighbourY, neighbourZ)) {
                        if (neighbourX != gridX || neighbourY != gridY || neighbourZ != gridZ) {
                            wallCount += map[neighbourX, neighbourY, neighbourZ];
                        }
                    } else {
                        wallCount++;
                    }
                }
            }
        }

        return wallCount;
    }

    void ProcessMap () {
        List<List<Coord>> roomRegions = GetRegions(0);

        int roomThresholdSize = 1000;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions) {
            if (roomRegion.Count < roomThresholdSize) {
                foreach (Coord tile in roomRegion) {
                    map[tile.tileX, tile.tileY, tile.tileZ] = 1;
                }
            } else {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        survivingRooms.Sort();
        for (int i = 1; i < survivingRooms.Count; i++) {
            foreach(Coord tile in survivingRooms[i].tiles) {
                map[tile.tileX, tile.tileY, tile.tileZ] = 1;
            }
        }
    }

    // Returns a list of all the separate regions in the map
    List<List<Coord>> GetRegions (int tileType) {
        List<List<Coord>> regions = new List<List<Coord>>();

        int[,,] mapFlags = new int[width, height, depth];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {

                    if (mapFlags[x,y,z] == 0 && map[x,y,z] == tileType) {
                        List<Coord> newRegion = GetRegionTiles(x, y, z);
                        regions.Add(newRegion);

                        foreach (Coord tile in newRegion) {
                            mapFlags[tile.tileX, tile.tileY, tile.tileZ] = 1;
                        }
                    }

                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles (int startX, int startY, int startZ) {
        List<Coord> tiles = new List<Coord>();

        int[,,] mapFlags = new int[width, height, depth];
        int tileType = map[startX, startY, startZ];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY, startZ));
        mapFlags[startX, startY, startZ] = 1;

        while (queue.Count > 0) {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
                    for (int z = tile.tileZ - 1; z <= tile.tileZ + 1; z++) {
                        if (IsInMapRange(x, y, z) && (y == tile.tileY || x == tile.tileX || z == tile.tileZ)) {
                            if (mapFlags[x,y,z] == 0 && map[x,y,z] == tileType) {
                                mapFlags[x, y, z] = 1;
                                queue.Enqueue(new Coord(x, y, z));
                            }
                        }
                    }
                }
            }
        }

        return tiles;
    }
    
    bool IsInMapRange (int x, int y, int z) {
        return x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < depth;
    }

    struct Coord {
        public int tileX;
        public int tileY;
        public int tileZ;

        public Coord (int x, int y, int z) {
            tileX = x;
            tileY = y;
            tileZ = z;
        }
    }

    class Room : IComparable<Room>{
        public List<Coord> tiles;
        public int roomSize;

        public Room () {
        }

        public Room(List<Coord> roomTiles, int[,,] map) {
            tiles = roomTiles;
            roomSize = tiles.Count;
        }

        public int CompareTo (Room otherRoom) {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }
}