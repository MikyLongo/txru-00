/*
 * Static class that defines readonly constants and methods for accessing and managing layer or layer mask.
 */
using UnityEngine;

namespace EngineConf
{
    public static class Layer
    {
        public static readonly int DEFAULT = 0;
        public static readonly int TRANSPARENT_FX = 1;
        public static readonly int IGNORE_RAYCAST = 2;

        public static readonly int WATER = 4;
        public static readonly int UI = 5;
        public static readonly int BUILDING = 6;
        public static readonly int LEVEL = 7;
        public static readonly int PLAYER = 8;
        public static readonly int CAMERA = 9;
        public static readonly int ENEMY = 10;
        public static readonly int MINIMAP = 11;
        public static readonly int ITEM = 12;
        public static readonly int FOV = 13;
        public static readonly int BUILDING_NO_MINIMAP = 14;
        public static readonly int ENEMY_WITH_COLLISION = 15;

        public static bool IsInMask(int layer, LayerMask mask)
        {
            return (mask & (1 << layer)) != 0;
        }

        public static LayerMask GroundMask
        {
            get
            {
                LayerMask layerMask = (1 << BUILDING) | (1 << BUILDING_NO_MINIMAP);
                return layerMask;
            }
        }

        public static LayerMask BuildingMask
        {
            get
            {
                LayerMask layerMask = (1 << BUILDING) | (1 << BUILDING_NO_MINIMAP);
                return layerMask;
            }
        }

        //Entities that can fall
        public static LayerMask FallingMask
        {
            get
            {
                LayerMask layerMask = (1 << PLAYER) | (1 << ENEMY) | (1 << ENEMY_WITH_COLLISION);
                return layerMask;
            }
        }

        public static LayerMask AliveEntityMask
        {
            get
            {
                LayerMask layerMask = (1 << PLAYER) | (1 << ENEMY) | (1 << ENEMY_WITH_COLLISION);
                return layerMask;
            }
        }

        //Used by Player entity
        public static LayerMask EnemyMask
        {
            get
            {
                LayerMask layerMask = (1 << ENEMY) | (1 << ENEMY_WITH_COLLISION);
                return layerMask;
            }
        }

        //Used by Enemy entity
        public static LayerMask EnemySearchMask
        {
            get
            {
                LayerMask layerMask = (1 << PLAYER);
                return layerMask;
            }
        }

        public static LayerMask EnemyObstructionMask
        {
            get
            {
                LayerMask layerMask = (1 << BUILDING) | (1 << BUILDING_NO_MINIMAP);
                return layerMask;
            }
        }

        //Utilized by Guard to verify the presence of a guardable item.
        public static LayerMask GuardableObstructionMask
        {
            get
            {
                LayerMask layerMask = (1 << BUILDING) | (1 << BUILDING_NO_MINIMAP) | (1 << PLAYER);
                return layerMask;
            }
        }

        public static LayerMask LaserHitMask
        {
            get
            {
                LayerMask layerMask = (1 << PLAYER) | (1 << BUILDING) | (1 << BUILDING_NO_MINIMAP) | (1 << ENEMY) | (1 << ENEMY_WITH_COLLISION);
                return layerMask;
            }
        }
    }
}
