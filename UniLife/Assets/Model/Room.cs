using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType { OFFICE, LECTURE, PRAC, FOOD, DORM }

public class Room { 

    public List<Tile> tiles
    {
        get;
        protected set;
    }

    public RoomType roomType;

    public List<Fixture> requiredFixtures;

    public bool IsComplete
    {
        get;
        protected set;
    }

    int roomIndex;
}
