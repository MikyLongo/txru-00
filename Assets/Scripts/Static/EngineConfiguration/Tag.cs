/*
 * Static class that defines readonly constants and methods for accessing and managing tags or groups of tags.
 */
using System;

namespace EngineConf
{
    public static class Tag
    {
        public static readonly string UNTAGGED = "Untagged";
        public static readonly string FINISH = "Finish";
        public static readonly string EDITOR_ONLY = "EditorOnly";
        public static readonly string GAME_CONTROLLER = "GameController";

        //Entity
        public static readonly string PLAYER = "Player";
        public static readonly string PLAYER_BODY_PART = "PlayerBodyPart";
        public static readonly string ENEMY = "Enemy";
        public static readonly string ENEMY_BODY_PART = "EnemyBodyPart";
        public static readonly string ITEM = "Item";

        //Level
        public static readonly string RESPAWN = "Respawn";
        public static readonly string DESTINATION_POINT = "DestinationPoint";
        public static readonly string CHECK_POINT = "CheckPoint";
        public static readonly string PATROL_POINT = "PatrolPoint";

        //Camera
        public static readonly string CAMERA_INTERACTION = "CameraInteraction";
        public static readonly string MAIN_CAMERA = "MainCamera";
        public static readonly string MINIMAP = "Minimap";
        public static readonly string CAMERA_ORB = "CameraOrb";

        //Building
        public static readonly string FLOOR = "Floor";
        public static readonly string WALL = "Wall";
        public static readonly string STAIRS = "Stairs";
        public static readonly string SLOPE = "Slope";
        public static readonly string ROOF = "Roof";
        public static readonly string DOOR = "Door";
        public static readonly string SMALL_PASSAGE = "SmallPassage";

        //Invisible Building/Environment
        public static readonly string INVISIBLE_BORDER = "InvisibleBorder";
        public static readonly string OUT_OF_BOUND = "OutOfBound";

        //Interaction
        public static readonly string INTERACTION = "Interaction";
        public static readonly string SOUNDNOISE = "SoundNoise";


        public static readonly string[] BUILDING_TAGS = 
        {
            FLOOR,
            WALL,
            STAIRS,
            SLOPE,
            ROOF,
            DOOR,
            INVISIBLE_BORDER
        };

        public static readonly string[] INVISIBLE_BUILDING_TAGS =
        {
            INVISIBLE_BORDER,
            OUT_OF_BOUND,
            //SMALL_PASSAGE //NO = Ensure the PlayerBox becomes invisible when fully inside the tunnel
        };

        public static readonly string[] ENTITIES_BODY_PART =
        {
            PLAYER_BODY_PART,
            ENEMY_BODY_PART
        };

        public static readonly string[] PLAYER_INTERACTION_TO_IGNORE =
        {
            SOUNDNOISE
        };

        public static bool IsBuilding(string tag)
        {
            return Array.Exists(BUILDING_TAGS, str => str.Equals(tag));
        }

        public static bool IsInvisibleBuilding(string tag)
        {
            return Array.Exists(INVISIBLE_BUILDING_TAGS, str => str.Equals(tag));
        }

        public static bool IsEntitiesBody(string tag)
        {
            return Array.Exists(ENTITIES_BODY_PART, str => str.Equals(tag));
        }

        public static bool IsPlayerInteractionToIgnore(string tag)
        {
            return Array.Exists(PLAYER_INTERACTION_TO_IGNORE, str => str.Equals(tag));
        }
    }
}