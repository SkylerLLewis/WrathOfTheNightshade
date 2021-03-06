using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Initializer : MonoBehaviour
{
    public Dictionary<string,Vector3Int> notableCells;
    Tile floor, leftWall, rightWall, leftRightWall, leftDoor, leftDoorOpen;
    Tile leftClearWall, rightClearWall, leftRightClearWall;
    Dictionary<string,Tile> tiles;
    private Tilemap floorMap, leftWallMap, rightWallMap, blockMap;
    Vector3Int cellLoc;
    private GameObject enemies, loot, lootFab;
    private PlayerController player;
    private DungeonController dungeonController;
    private Dictionary<string, GameObject> enemyFabs;
    private Dictionary<string, int> enemyWheel, lootWheel, goldLedger;
    private Dictionary<string, Sprite> goldSprites;
    public List<Room> rooms;
    private PersistentData data;
    
    void Awake() {
        data = GameObject.FindWithTag("Data").GetComponent<PersistentData>();
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        enemies = GameObject.FindWithTag("EntityList");
        dungeonController = gameObject.GetComponent<DungeonController>();
        loot = GameObject.FindWithTag("Loot");

        notableCells = new Dictionary<string, Vector3Int>();

        // Load tile Resources into dictionaries
        tiles = new Dictionary<string,Tile>();
        tiles.Add("floor", Resources.Load<Tile>("Tiles/DungeonMap/floor"));
        tiles.Add("floor1", Resources.Load<Tile>("Tiles/DungeonMap/floor1"));
        tiles.Add("floor2", Resources.Load<Tile>("Tiles/DungeonMap/floor2"));
        tiles.Add("floor3", Resources.Load<Tile>("Tiles/DungeonMap/floor3"));
        tiles.Add("leftWall", Resources.Load<Tile>("Tiles/DungeonMap/leftWall"));
        tiles.Add("rightWall", Resources.Load<Tile>("Tiles/DungeonMap/rightWall"));
        tiles.Add("leftDoor", Resources.Load<Tile>("Tiles/DungeonMap/leftDoor"));
        tiles.Add("leftDoorOpen", Resources.Load<Tile>("Tiles/DungeonMap/leftDoorOpen"));
        tiles.Add("rightDoor", Resources.Load<Tile>("Tiles/DungeonMap/rightDoor"));
        tiles.Add("rightDoorOpen", Resources.Load<Tile>("Tiles/DungeonMap/rightDoorOpen"));
        tiles.Add("stairsUp", Resources.Load<Tile>("Tiles/DungeonMap/stairsUp"));
        tiles.Add("stairsDown", Resources.Load<Tile>("Tiles/DungeonMap/stairsDown"));
        tiles.Add("chest", Resources.Load<Tile>("Tiles/DungeonMap/chest"));
        tiles.Add("column", Resources.Load<Tile>("Tiles/DungeonMap/column"));

        // Load Enemy prefabs and likelyhood
        int easy = 10 - 2*data.depth;
        int medium = 2;
        int hard = data.depth/2;
        int rare = data.depth/4;
        enemyFabs = new Dictionary<string, GameObject>();
        enemyWheel = new Dictionary<string, int>();
        enemyFabs.Add("Skeleton", Resources.Load("Prefabs/Skeleton") as GameObject);
        enemyFabs.Add("Skeleton Rat", Resources.Load("Prefabs/Skeleton Rat") as GameObject);
        enemyFabs.Add("Skeleton Archer", Resources.Load("Prefabs/Skeleton Archer") as GameObject);
        enemyFabs.Add("Skeleton Brute", Resources.Load("Prefabs/Skeleton Brute") as GameObject);
        enemyFabs.Add("Skeleton Stabber", Resources.Load("Prefabs/Skeleton Stabber") as GameObject);
        enemyFabs.Add("Skeleton Warrior", Resources.Load("Prefabs/Skeleton Warrior") as GameObject);
        enemyFabs.Add("Bonemass", Resources.Load("Prefabs/Bonemass") as GameObject);
        enemyWheel.Add("Skeleton", easy);
        enemyWheel.Add("Skeleton Rat", easy);
        enemyWheel.Add("Skeleton Archer", medium+1);
        enemyWheel.Add("Skeleton Brute", hard);
        enemyWheel.Add("Skeleton Stabber", hard);
        enemyWheel.Add("Skeleton Warrior", rare);

        // Loot drop chances!
        lootFab = Resources.Load("Prefabs/Loot Drop") as GameObject;
        lootWheel = new Dictionary<string, int>();
        goldLedger = new Dictionary<string, int>();
        goldSprites = new Dictionary<string,Sprite>();

        goldSprites.Add("small", Resources.Load<Sprite>("Gold"));
        goldSprites.Add("medium", Resources.Load<Sprite>("Gold Bar"));
        goldSprites.Add("large", Resources.Load<Sprite>("Gold Pile"));

        // Weapons
        lootWheel.Add("Sharp Twig", easy);
        lootWheel.Add("Plank with a Nail", easy);
        lootWheel.Add("Club", easy);
        lootWheel.Add("Long Stick", easy);
        lootWheel.Add("Log", easy);
        lootWheel.Add("Staff", easy);
        lootWheel.Add("Crude Bow", easy);

        lootWheel.Add("Rusty Shortsword", medium);
        lootWheel.Add("Half a Scissor", medium);
        lootWheel.Add("Copper Hatchet", medium);
        lootWheel.Add("Mallet", medium);
        lootWheel.Add("Flint Spear", medium);
        lootWheel.Add("Dog Chain", medium);
        lootWheel.Add("Cursed Dagger", medium);
        lootWheel.Add("Shortbow", medium);

        lootWheel.Add("Dueling Sword", rare);
        lootWheel.Add("Hunting Knife", rare);
        lootWheel.Add("Woodcutter's Axe", rare);
        lootWheel.Add("Hammer", rare);
        lootWheel.Add("Wooden Pike", rare);
        lootWheel.Add("Grain Scythe", rare);
        lootWheel.Add("Magewood Blade", rare);
        lootWheel.Add("Light Crossbow", rare);
        
        // Armors
        lootWheel.Add("Leather Tunic", 2*easy);
        lootWheel.Add("Bone Armor", 2*easy);

        lootWheel.Add("Padded Vest", medium);
        lootWheel.Add("Cast Iron Plates", medium);
        lootWheel.Add("Fang Bracers", medium);

        lootWheel.Add("Light Leather Armor", rare);
        lootWheel.Add("Gambeson", rare);
        lootWheel.Add("Patchy Brigandine", rare);

        // Potions & scrolls
        lootWheel.Add("Health Potion", 2*easy);
        lootWheel.Add("Mana Potion", easy);
        lootWheel.Add("Potion of Speed", easy);
        lootWheel.Add("Scroll of Return", easy);
        
        lootWheel.Add("Scroll of Descent", medium);

        lootWheel.Add("Potion of Regeneration", hard);

        foreach (Tilemap map in FindObjectsOfType<Tilemap>()) {
            if (map.name == "FloorMap") {
                floorMap = map;
            } else if (map.name == "LeftWallMap") {
                leftWallMap = map;
            } else if (map.name == "RightWallMap") {
                rightWallMap = map;
            } else if (map.name == "BlockMap") {
                blockMap = map;
            }
        }

        // Gen Dungeon
        GenerateDungeon();

        dungeonController.UpdateNotables(notableCells);
    }

    void Start() {
        rooms[0].Draw();
    }

    // Actions on collisions
    public void NotableActions(string key) {

        if (key == "stairsUp") {
            player.enabled = false;
            data.depth--;
            data.floorDirection = "up";
            if (data.depth == 0) {
                data.entrance = 1;
                data.followingEnemies.Clear();
                data.LoadingScreenLoad("GreenVillage", "ascending");
            } else {
                data.LoadingScreenLoad("BasicDungeon", "ascending");
                data.followingEnemies.Clear();
                if (enemies.transform.childCount > 0) {
                    foreach(Transform child in enemies.transform) {
                        data.followingEnemies.Add(child.name);
                    }
                }
            }
            return;

        } else if (key == "stairsDown") {
            player.enabled = false;
            data.depth++;
            data.floorDirection = "down";
            data.LoadingScreenLoad("BasicDungeon", "descending");
            data.followingEnemies.Clear();
            if (enemies.transform.childCount > 0) {
                foreach(Transform child in enemies.transform) {
                    data.followingEnemies.Add(child.name);
                }
            }
            return;

        } else if (key.IndexOf("Loot") >= 0) {
            Vector3 pos = floorMap.CellToWorld(notableCells[key]);
            if (pos.z != 0) { // Account for floor z diff
                pos.y += 0.75f;
                pos.z = 0;
            }
            Vector3 diff;
            Transform target = null;
            foreach (Transform child in loot.transform) {
                diff = pos - child.position;
                if (diff.magnitude < 0.5) {
                    target = child;
                    break;
                }
            }
            if (target != null) {
                if (Weapon.IsWeapon(target.name)) {
                    Weapon wep = new Weapon(target.name);
                    data.AddToInventory(wep);
                    player.saySomething = wep.displayName;
                } else if (Armor.IsArmor(target.name)) {
                    Armor arm = new Armor(target.name);
                    data.AddToInventory(arm);
                    player.saySomething = arm.displayName;
                } else if (Potion.IsPotion(target.name)) {
                    Potion pot = new Potion(target.name);
                    data.AddToInventory(pot);
                    player.saySomething = pot.displayName;
                } else if (Scroll.IsScroll(target.name)) {
                    Scroll scroll = new Scroll(target.name);
                    data.AddToInventory(scroll);
                    player.saySomething = scroll.displayName;
                }
                Destroy(target.gameObject);
                notableCells.Remove(key);
                dungeonController.UpdateNotables(notableCells);
            }
            return;

        } else if (key.IndexOf("Gold") >= 0) {
            Vector3 pos = floorMap.CellToWorld(notableCells[key]);
            if (pos.z != 0) { // Account for floor z diff
                pos.y += 0.75f;
                pos.z = 0;
            }
            Vector3 diff;
            Transform target = null;
            foreach (Transform child in loot.transform) {
                diff = pos - child.position;
                if (diff.magnitude < 0.5) {
                    target = child;
                    break;
                }
            }
            if (target != null) {
                data.gold += goldLedger[key];
                player.saySomething = "+"+goldLedger[key]+" gold";
                goldLedger.Remove(key);
                Destroy(target.gameObject);
                notableCells.Remove(key);
                dungeonController.UpdateNotables(notableCells);
            }
            return;

        } else if (key.IndexOf("Chest") >= 0) {
            OpenChest(notableCells[key]);
            notableCells.Remove(key);
            dungeonController.UpdateNotables(notableCells);
            return;
        }
    }

    // Creates a map of rooms 
    void GenerateDungeon() {
        // Create core room
        rooms = new List<Room>();
        Vector3Int head = new Vector3Int();
        Vector3Int tail = new Vector3Int();
        head.x = Random.Range(1, 4);
        head.y = Random.Range(1, 4);
        tail.x = Random.Range(-3, 0);
        tail.y = Random.Range(-3, 0);
        Room core = new Room(head, tail);
        core.enemies.Clear();
        foreach (string e in data.followingEnemies) {
            core.enemies.Add(e);
        }
        rooms.Add(core);
        int rand = Random.Range(0,2);
        int boss = 0;
        if (rand == 0) {// String shape Dungeon
            // Create 4 Neighbors
            for (int i=0; i<4; i++) {
                GenerateRoom(core, i);
            }
            // Create string of rooms
            int direction = Random.Range(0, 4);
            Room branch = core.neighbors[direction];
            boss = Random.Range(0, 4);
            for (int i=0; i<4; i++) {
                if (data.depth == 5 && i == boss) {
                    Debug.Log("Bonemass successfully created at room "+i);
                    GenerateRoom(branch, direction, "Bonemass");
                    branch = branch.neighbors[direction];
                    continue;
                }
                GenerateRoom(branch, direction);
                branch = branch.neighbors[direction];
            }
            // Add Random Rooms
            for (int i=0; i<16; i++) {
                branch = rooms[Random.Range(0, rooms.Count)];
                direction = Random.Range(0, 4);
                int j = 0;
                while (branch.neighbors[direction] != null) {
                    direction = (direction+1)%4;
                    j++;
                    if (j >= 4) { break; }
                }
                if (branch.neighbors[direction] == null) {
                    GenerateRoom(branch, direction);
                }
            }
        } else if (rand == 1) { // Loop shape Dungeon
            Debug.Log("Loop!");
            Room branch = core;
            int direction = Random.Range(0, 4);
            boss = Random.Range(0, 5);
            // Create loop of rooms
            for (int i=0; i<4; i++) {
                int dir = (direction+i)%4;
                for (int j=0; j<3; j++) {
                    if (data.depth == 5 && i*3+j == boss) {
                        Debug.Log("Bonemass successfully created at room "+(i+j));
                        GenerateRoom(branch, direction, "Bonemass");
                        branch = branch.neighbors[direction];
                        continue;
                    }
                    if (!GenerateRoom(branch, dir)) {
                        break; // Break if room creation fails
                    }
                    branch = branch.neighbors[dir];
                }
            }
            // Cap final room with forks
            for (int i=0; i<3; i++) {
                GenerateRoom(branch, (direction+2+i)%4);
            }
            // Add Random Rooms
            for (int i=0; i<12; i++) {
                branch = rooms[Random.Range(0, rooms.Count)];
                direction = Random.Range(0, 4);
                int j = 0;
                while (branch.neighbors[direction] != null) {
                    direction = (direction+1)%4;
                    j++;
                    if (j >= 4) { break; }
                }
                if (branch.neighbors[direction] == null) {
                    GenerateRoom(branch, direction);
                }
            }
        }
        GenExits();
        GenLoot();
        if (core.loot > 0) {
            rooms[1].loot += core.loot;
            core.loot = 0;
        }
    }

    private bool GenerateRoom(Room parent, int direction, string prefab="") {
        int width, height;
        Vector3Int head = new Vector3Int();
        Vector3Int tail = new Vector3Int();
        bool valid;
        Room newRoom;
        int tries = 0;
        do {
            width = Mathf.RoundToInt(Mathf.Pow(Random.Range(1.4f, 2.6f), 2));
            height = Mathf.RoundToInt(Mathf.Pow(Random.Range(1.4f, 2.6f), 2));
            if (prefab == "Bonemass") {
                width = 9;
                height = 5;
            }
            if (width*height <= 4) {
                if (Random.Range(0,2) == 0) {
                    width += 2;
                } else {
                    height += 2;
                }
            }
            tries++;
            // Generate room to test
            if (direction == 0) {
                tail.x = Random.Range(parent.tail.x-width+1, parent.head.x+1);
                tail.y = parent.head.y+1;
                head.x = tail.x + width-1;
                head.y = tail.y + height-1;
            } else if (direction == 1) {
                tail.x = parent.head.x+1;
                tail.y = Random.Range(parent.tail.y-height+1, parent.head.y+1);
                head.x = tail.x + width-1;
                head.y = tail.y + height-1;
            } else if (direction == 2) {
                head.x = Random.Range(parent.tail.x, parent.head.x+width);
                head.y = parent.tail.y-1;
                tail.x = head.x - width+1;
                tail.y = head.y - height+1;
            } else if (direction == 3) {
                head.x = parent.tail.x-1;
                head.y = Random.Range(parent.tail.y, parent.head.y+height);
                tail.x = head.x - width+1;
                tail.y = head.y - height+1;
            }
            Room room = new Room(head, tail);
            valid = true;
            foreach (Room r in rooms) {
                if (r.Collides(room)) {
                    valid = false;
                    break;
                }
            }
            if (tries > 20) {
                return false;
            }
        } while (!valid);
        newRoom = new Room(head, tail, parent, rooms, prefab);
        if (prefab == "Bonemass") {
            Vector3Int cell = new Vector3Int();
            for (int x=0; x<4; x++) {
                cell.x = newRoom.tail.x + 1 + 2*x;
                cell.y = newRoom.tail.y;
                GenChest(newRoom, cell);
                cell.y = newRoom.head.y;
                GenChest(newRoom, cell);
            }
        }
        rooms.Add(newRoom);
        parent.neighbors[direction] = rooms[rooms.Count-1];
        return true;
    }

    // Creates a floorspace, rooted at top corner
    public void DrawRoom(Room r) {
        int xLen = r.head.x - r.tail.x+1;
        int yLen = r.head.y - r.tail.y+1;
        Vector3Int placement = new Vector3Int(0,0,0);
        // Place floor
        for (int x=0; x>-xLen; x--) {
            for (int y=0; y>-yLen; y--) {
                placement.x = r.head.x + x;
                placement.y = r.head.y + y;
                Tile tile = null;
                tile = tiles["floor"];
                int rand = Random.Range(0, 20);
                if (rand < 10) {
                    tile = tiles["floor"];
                } else if (rand < 18) {
                    tile = tiles["floor1"];
                } else if (rand == 18) {
                    tile = tiles["floor2"];
                } else if (rand == 19) {
                    tile = tiles["floor3"];
                }
                Utilities.PlaceTile(floorMap, placement, tile);
            }
        }
        // Special Layout for prefabs
        if (r.prefab == "Bonemass") {
            placement = new Vector3Int(0,0,0);
            for (int x=0; x<4; x++) {
                placement.x = r.tail.x + 1 + 2*x;
                for (int y=0; y<2; y++) {
                    placement.y = r.tail.y + 1 + 2*y;
                    Utilities.PlaceTile(floorMap, placement, null);
                    Utilities.PlaceTile(blockMap, placement, tiles["column"]);
                }
            }
        }
        // Place Walls
        CreateWalls(r);
        // Create exits, if applicable
        foreach (KeyValuePair<string,Vector3Int> exit in notableCells) {
            if (r.Contains(exit.Value) && exit.Key.IndexOf("stairs") >= 0) {
                Utilities.PlaceTile(floorMap, exit.Value, null);
                Utilities.PlaceTile(blockMap, exit.Value, tiles[exit.Key]);
            }
        }
        // Place Doors
        foreach (Room n in r.neighbors) {
            if (n != null && !n.active) {
                PlaceDoor(r, n);
            }
        }
        r.active = true;
        PlaceChest(r);
        GenEnemies(r);
    }

    // A function for placing proper walls around a floor tile
    void CreateWalls(Room r) {
        Vector3Int cell;
        // Add four corners
        PlaceWall(r.head, "leftWall");
        PlaceWall(r.head, "rightWall");

        cell = new Vector3Int(r.head.x, r.tail.y, 0);
        PlaceWall(cell, "rightWall");
        cell = new Vector3Int(r.head.x, r.tail.y-1, 0);
        PlaceWall(cell, "leftWall", true);

        cell = new Vector3Int(r.tail.x, r.tail.y-1, 0);
        PlaceWall(cell, "leftWall", true);
        cell = new Vector3Int(r.tail.x-1, r.tail.y, 0);
        PlaceWall(cell, "rightWall", true);

        cell = new Vector3Int(r.tail.x, r.head.y, 0);
        PlaceWall(cell, "leftWall");
        cell = new Vector3Int(r.tail.x-1, r.head.y, 0);
        PlaceWall(cell, "rightWall", true);
        
        // Fill walls
        for (int x=r.head.x-1; x > r.tail.x; x--) {
            cell = new Vector3Int(x, r.head.y, 0);
            PlaceWall(cell, "leftWall");
            cell = new Vector3Int(x, r.tail.y-1, 0);
            PlaceWall(cell, "leftWall", true);
        }
        for (int y=r.head.y-1; y > r.tail.y; y--) {
            cell = new Vector3Int(r.head.x, y, 0);
            PlaceWall(cell, "rightWall");
            cell = new Vector3Int(r.tail.x-1, y, 0);
            PlaceWall(cell, "rightWall", true);
        }
    }

    void PlaceWall(Vector3Int cell, string name, bool clear=false) {
        // Determine Map
        Tilemap map;
        if (name.ToLower().IndexOf("left") >= 0) {
            map = leftWallMap;
        } else {
            map = rightWallMap;
        }
        TileBase currentWall = map.GetTile(cell);
        if (currentWall != null) { // Wall in-place, clarify it
            map.SetColor(cell, new Color(1,1,1,0.25f));
        } else { // Place wall normally
            Utilities.PlaceTile(map, cell, tiles[name]);
            if (clear) {
                map.SetColor(cell, new Color(1,1,1,0.25f));
            }
        }
    }

    void PlaceDoor(Room r1, Room r2) {
        Vector3Int cell = new Vector3Int();
        Vector3Int opposite = new Vector3Int();
        if (r1.head.y == r2.tail.y-1) { // New room is above
            // Cycle through possible door placments
            cell.y = r1.head.y;
            int x = Mathf.RoundToInt(Random.Range(0, r1.width));
            for (int i=0; i<r1.width; i++) {
                cell.x = r1.tail.x + x;
                opposite = new Vector3Int(cell.x,cell.y+1,cell.z);
                if (r1.Contains(cell) && r2.Contains(opposite)
                && !notableCells.ContainsValue(cell) && !notableCells.ContainsValue(opposite)) {
                    Utilities.PlaceTile(leftWallMap, cell, tiles["leftDoor"]);
                    r1.doors[0] = cell;
                    r2.doors[2] = new Vector3Int(cell.x, cell.y+1, 0);
                    break;
                }
                x = (x+1) % r1.width;
            }
        } else if (r1.head.x == r2.tail.x-1) { // New room is right
            // Cycle through possible door placments
            cell.x = r1.head.x;
            int y = Mathf.RoundToInt(Random.Range(0, r1.height));
            for (int i=0; i<r1.height; i++) {
                cell.y = r1.tail.y + y;
                opposite = new Vector3Int(cell.x+1,cell.y,cell.z);
                if (r1.Contains(cell) && r2.Contains(opposite)
                && !notableCells.ContainsValue(cell) && !notableCells.ContainsValue(opposite)) {
                    Utilities.PlaceTile(rightWallMap, cell, tiles["rightDoor"]);
                    r1.doors[1] = cell;
                    r2.doors[3] = new Vector3Int(cell.x+1, cell.y, 0);
                    break;
                }
                y = (y+1) % r1.height;
            }
        } else if (r1.tail.y == r2.head.y+1) { // New room is below
            // Cycle through possible door placments
            cell.y = r1.tail.y;
            int x = Mathf.RoundToInt(Random.Range(0, r1.width));
            for (int i=0; i<r1.width; i++) {
                cell.x = r1.tail.x + x;
                opposite = new Vector3Int(cell.x,cell.y-1,cell.z);
                if (r1.Contains(cell) && r2.Contains(opposite)
                && !notableCells.ContainsValue(cell) && !notableCells.ContainsValue(opposite)) {
                    Utilities.PlaceTile(leftWallMap, opposite, tiles["leftDoor"]);
                    leftWallMap.SetColor(opposite, new Color(1,1,1,0.5f));
                    r1.doors[2] = cell;
                    r2.doors[0] = new Vector3Int(cell.x, cell.y-1, 0);
                    break;
                }
                x = (x+1) % r1.width;
            }
        } else if (r1.tail.x == r2.head.x+1) { // New room is left
            // Cycle through possible door placments
            cell.x = r1.tail.x;
            int y = Mathf.RoundToInt(Random.Range(0, r1.height));
            for (int i=0; i<r1.height; i++) {
                cell.y = r1.tail.y + y;
                opposite = new Vector3Int(cell.x-1,cell.y,cell.z);
                if (r1.Contains(cell) && r2.Contains(opposite)
                && !notableCells.ContainsValue(cell) && !notableCells.ContainsValue(opposite)) {
                    Utilities.PlaceTile(rightWallMap, opposite, tiles["rightDoor"]);
                    rightWallMap.SetColor(opposite, new Color(1,1,1,0.5f));
                    r1.doors[3] = cell;
                    r2.doors[1] = new Vector3Int(cell.x-1, cell.y, 0);
                    break;
                }
                y = (y+1) % r1.height;
            }
        }
    }

    void GenExits() {
        string entrance = "stairsUp", exit = "stairsDown";
        if (data.floorDirection == "up") {
            entrance = "stairsDown";
            exit = "stairsUp";
        }
        notableCells.Add(entrance, new Vector3Int(0,1,0));
        if (data.depth < 5) {
            int rand = Random.Range(1, rooms.Count);
            int i = 0;
            foreach(Room r in rooms) {
                if (i == rand) {
                    Vector3Int cell = new Vector3Int(
                        r.tail.x+Random.Range(1,r.width-1),
                        r.tail.y+Random.Range(1,r.height-1),
                        0);
                    notableCells.Add(exit, cell);
                    break;
                }
                i++;
            }
        }
    }

    void GenLoot() {
        // 6-10 (8) loot per floor
        int loots = Random.Range(6,11);
        for (int i=0; i<loots; i++) {
            int rand = Random.Range(1, rooms.Count);
            if (rooms[rand].prefab != "Bonemass") {
                rooms[rand].loot++;
            }
        }
        foreach(Room r in rooms) {
            if (r.loot > 0) {
                GenChest(r);
            }
        }
    }

    public void OpenChest(Vector3Int cell) {
        Utilities.PlaceTile(floorMap, cell, null);
        Utilities.PlaceTile(floorMap, cell, tiles["floor"]);
        DropLoot(cell);
    }

    public void OpenDoor(Vector3Int cell, int dir) {
        if (dir == 0 || dir == 2) {
            Color color = leftWallMap.GetColor(cell);
            Utilities.PlaceTile(leftWallMap, cell, tiles["leftDoorOpen"]);
            leftWallMap.SetColor(cell, color);
            if (dir == 0) { cell.y++; }
        } else {
            Color color = rightWallMap.GetColor(cell);
            Utilities.PlaceTile(rightWallMap, cell, tiles["rightDoorOpen"]);
            rightWallMap.SetColor(cell, color);
            if (dir == 1) { cell.x++; }
        }
        foreach (Room r in rooms) {
            if (r.Contains(cell) && !r.active) {
                r.Draw();
            }
        }
    }

    // Give a room its list of enemies
    public List<string> RetrieveEnemies() {
        List<string> enemies = new List<string>();
        float mod = 0;
        if (data.depth == 4) {
            mod = 0.25f;
        } else if (data.depth == 5) {
            mod = 0.5f;
        }
        int numEnemies = Mathf.RoundToInt(Mathf.Pow(Random.Range(1f+mod,2f+mod), 2));
        float wheelTotal = 0f;
        foreach (KeyValuePair<string,int> e in enemyWheel) {
            wheelTotal += e.Value;
        }
        for (int _=0; _<numEnemies; _++) {
            int total = 0;
            float roll = Random.Range(0, wheelTotal+1);
            foreach (KeyValuePair<string,int> e in enemyWheel) {
                total += e.Value;
                if (roll <= total) {
                    enemies.Add(e.Key);
                    break;
                }
            }
        }
        return enemies;
    }

    public void GenEnemies(Room r) {
        // Generate spawn points
        List<Vector3Int> vectors = new List<Vector3Int>();
        // Add player and notables as occupied
        vectors.Add(player.tilePosition);
        foreach (KeyValuePair<string,Vector3Int> v in notableCells) {
            vectors.Add(v.Value);
        }
        for (int i=0; i<r.enemies.Count; i++) {
            Vector3Int v;
            bool valid;
            int sentinel = 0;
            do {
                valid = true;
                v = new Vector3Int(
                    Random.Range(r.tail.x, r.head.x+1),
                    Random.Range(r.tail.y, r.head.y+1),
                    0);
                foreach (Vector3Int v2 in vectors) {
                    if (v == v2) {
                        valid = false;
                        break;
                    }
                }
                sentinel++;
                if (sentinel > 100) {break;}
            } while (!valid);
            vectors.Add(v);
        }
        // Remove player and notables from spawn options
        vectors.RemoveRange(0, notableCells.Count+1);
        int ei = 0;
        foreach (string e in r.enemies) {
            Vector3 pos = floorMap.CellToWorld(vectors[ei]);
            pos.y += 0.25f;
            pos.z = 0;
            GameObject clone = Instantiate(
                enemyFabs[e],
                pos,
                Quaternion.identity,
                enemies.transform);
            clone.name = clone.name.Split('(')[0];
            clone.GetComponent<EnemyBehavior>().SetCoords(vectors[ei]);
            ei++;
        }
    }

    public void GenChest(Room r) {
        Vector3Int cell = new Vector3Int();
        int sentinel = 0;
        do {
            cell = new Vector3Int(
                    r.tail.x+Random.Range(0,r.width-1),
                    r.tail.y+Random.Range(0,r.height-1),
                    0);
            sentinel++;
            if (sentinel > 100) { return; }
        } while (notableCells.ContainsValue(cell) || cell == player.tilePosition);

        // Find new chest number to add
        int last = 0;
        foreach (KeyValuePair<string,Vector3Int> item in notableCells) {
            if (item.Key.IndexOf("Chest") >= 0) {
                int x = System.Convert.ToInt32(item.Key.Split('t')[1]);
                if (x > last) {
                    last = x; 
                }
            }
        }
        notableCells.Add("Chest"+(last+1), cell);
    }
    public void GenChest(Room r, Vector3Int cell) {
        // Find new chest number to add
        int last = 0;
        foreach (KeyValuePair<string,Vector3Int> item in notableCells) {
            if (item.Key.IndexOf("Chest") >= 0) {
                int x = System.Convert.ToInt32(item.Key.Split('t')[1]);
                if (x > last) {
                    last = x; 
                }
            }
        }
        notableCells.Add("Chest"+(last+1), cell);
    }
    public void PlaceChest(Room r) {
        foreach (KeyValuePair<string,Vector3Int> item in notableCells) {
            if (r.Contains(item.Value) && item.Key.IndexOf("Chest") >= 0) {
                Utilities.PlaceTile(floorMap, item.Value, tiles["chest"]);
            }
        }
    }

    private void DropLoot(Vector3Int cell) {
        Vector3 pos = floorMap.CellToWorld(cell);
            pos.y += 0.75f;
            pos.z = 0;
        
        if (Random.Range(0,2) == 1) { // 50% change for gold drop
            // 1 - 19  gold at lvl 1
            // 2 - 26  gold at lvl 3
            // 5 - 35 gold at lvl 5
            int mod = 0;
            if (data.depth >= 3) { mod = 1; }
            if (data.depth >= 5) { mod = 2; }
            int gold = Mathf.RoundToInt(Mathf.Pow(Random.Range(mod,4+mod), 2)) + Random.Range(1, 11);
            GameObject clone = Instantiate(
                lootFab,
                pos,
                Quaternion.identity,
                loot.transform);
            clone.name = "Gold";
            string sprite = "";
            if (gold < 25) {
                sprite = "small";
            } else if (gold < 35) {
                sprite = "medium";
            } else {
                sprite = "large";
            }
            clone.GetComponent<SpriteRenderer>().sprite = goldSprites[sprite];
            // Find new loot number to add
            int last = 0;
            foreach (KeyValuePair<string,Vector3Int> item in notableCells) {
                if (item.Key.IndexOf("Gold") >= 0) {
                    int x = System.Convert.ToInt32(item.Key.Split('d')[1]);
                    if (x > last) {
                        last = x; 
                    }
                }
            }
            notableCells.Add("Gold"+(last+1), cell);
            goldLedger.Add("Gold"+(last+1), gold);

        } else {
            // Spin the wheel!
            float wheelTotal = 0f;
            foreach (KeyValuePair<string,int> item in lootWheel) {
                wheelTotal += item.Value;
            }
            int total = 0;
            float roll = Random.Range(0f, wheelTotal);
            foreach (KeyValuePair<string,int> item in lootWheel) {
                total += item.Value;
                if (roll <= total) {
                    GameObject clone = Instantiate(
                        lootFab,
                        pos,
                        Quaternion.identity,
                        loot.transform);
                    clone.name = item.Key;
                    Sprite sprite = null;
                    if (Weapon.IsWeapon(item.Key)) {
                        sprite = Resources.Load<Sprite>("Weapons/"+item.Key);
                    } else if (Armor.IsArmor(item.Key)) {
                        sprite = Resources.Load<Sprite>("Armors/"+item.Key);
                    } else if (Potion.IsPotion(item.Key)) {
                        sprite = Resources.Load<Sprite>("Potions/"+item.Key);
                    } else if (Scroll.IsScroll(item.Key)) {
                        sprite = Resources.Load<Sprite>("Scrolls/"+item.Key);
                    }
                    clone.GetComponent<SpriteRenderer>().sprite = sprite;
                    break;
                }
            }
            // Find new loot number to add
            int last = 0;
            foreach (KeyValuePair<string,Vector3Int> item in notableCells) {
                if (item.Key.IndexOf("Loot") >= 0) {
                    int x = System.Convert.ToInt32(item.Key.Split('t')[1]);
                    if (x > last) {
                        last = x; 
                    }
                }
            }
            notableCells.Add("Loot"+(last+1), cell);
        }
    }

    public void HighlightTiles(List<Vector3Int> cells, Color color) {
        foreach (Vector3Int cell in cells) {
            floorMap.SetColor(cell, color);
        }
    }

    public List<Room> GetRooms() {
        return rooms;
    }
}
