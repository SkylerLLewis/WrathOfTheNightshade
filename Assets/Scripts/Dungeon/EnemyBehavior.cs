using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyBehavior : MonoBehaviour
{
    public float moveSpeed, attackSpeed, visualSpeed;
    public float timer;
    public bool moving=false, attacking=false, dying=false, waiting=false, disabled=false;
    public Vector3Int tilePosition, targetDoorCell;
    public string[] facing;
    private Tilemap floorMap, leftWallMap, rightWallMap, blockMap;
    private DungeonController dungeonController;
    private Dictionary<string,Tilemap> maps;
    private GameObject arrow;
    private PlayerController player;
    private EntityController entityController;
    private PathFinder pathFinder;
    private GameObject[] entities;
    private GameObject canvas;
    private Vector3 targetPosition, startPosition, highPoint;
    private Quaternion startAngle, targetAngle;
    private float count = 1.0f;
    public Room currentRoom;
    
    // Combat Stats
    public int maxhp, hp, attack, defense, mindmg, maxdmg, moveRatio, actionCounter, xp;
    public string enemyType;

    public void SetCoords(Vector3Int cell) {
        tilePosition = cell;
        currentRoom = Room.FindByCell(tilePosition, dungeonController.GetRooms());
    }
    void Awake() {
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
        maps = new Dictionary<string, Tilemap>();
        maps.Add("left", leftWallMap);
        maps.Add("right", rightWallMap);
        dungeonController = floorMap.GetComponent<DungeonController>();
        targetDoorCell = Vector3Int.zero;
        pathFinder = floorMap.GetComponent<PathFinder>();
        canvas = GameObject.FindWithTag("WorldCanvas");
        //tilePosition = floorMap.WorldToCell(this.transform.position);
        //tilePosition.z = 0;
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        entityController = GameObject.FindObjectOfType<EntityController>();
        arrow = Resources.Load<GameObject>("Prefabs/Arrow");

        timer = Random.Range(0, moveSpeed);
        facing = new string[2];
        visualSpeed = 8-2*moveSpeed;
        actionCounter = Random.Range(0, moveRatio+1);
    }

    public void MyTurn() {
        targetPosition = this.transform.position;
        startPosition = this.transform.position;
        // A numerical direction that can rotate 0-3
        int direction = 0;
        int [] directions = new int[3];
        Vector3Int targetCell;

        Vector3Int delta = new Vector3Int();
        delta.x = player.tilePosition.x - tilePosition.x;
        delta.y = player.tilePosition.y - tilePosition.y;
        // Manhattan distance
        int distance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);

        // Get naiive direction
        if (delta.y > 0) {
            direction = 0;
        } else if (delta.x > 0) {
            direction = 1;
        } else if (delta.y < 0) {
            direction = 2;
        } else if (delta.x < 0) {
            direction = 3;
        }

        // In another room? Make for the door!
        if (currentRoom != player.currentRoom) {
            if (targetDoorCell == Vector3Int.zero) {
                targetDoorCell = currentRoom.GetDoorCell(player.currentRoom);
            }
            delta.x = targetDoorCell.x - tilePosition.x;
            delta.y = targetDoorCell.y - tilePosition.y;
        }

        // Ranged? Run away!
        if (enemyType == "Ranged" && distance < 4) {
            delta.x *= -1;
            delta.y *= -1;
        }
        
        if (enemyType == "Melee") {
            if (distance == 1 &&
            pathFinder.DirectionWalkable(tilePosition, direction)) {
                // Player in range, ATTACK!
                
                targetCell = pathFinder.DirectionToCell(direction, tilePosition);attacking = true;
                targetPosition = startPosition;
                highPoint = player.transform.position;
                count = 0.0f;
                Attack(player);
                // Face enemy in new direction
                if (direction < 2) {
                    facing[0] = "up";
                } else {
                    facing[0] = "down";
                }
                if (direction == 0 || direction == 3) {
                    facing [1] = "left";
                } else {
                    facing[1] = "right";
                }
                EndTurn(attackSpeed);
                return; // Attack successful, turn over.
            }
        } else if (enemyType == "Ranged") {
            if (pathFinder.LineOfSight(tilePosition, player.tilePosition)) {
                if (actionCounter == moveRatio) {
                    actionCounter = 0;
                    // Player in line of sight, ATTACK!

                    waiting = true;
                    count = 1.0f;
                    RangedAttack(player);

                    // Face enemy in new direction
                    if (direction < 2) {
                        facing[0] = "up";
                    } else {
                        facing[0] = "down";
                    }
                    if (direction == 0 || direction == 3) {
                        facing [1] = "left";
                    } else {
                        facing[1] = "right";
                    }
                    EndTurn(attackSpeed);
                    return; // Attack successful, turn over.
                } else {
                    // Continue with move!
                    actionCounter++;
                }
            }
        }

        // Try to move
        // Decide best direction for moving
        if (Mathf.Abs(delta.y) >= Mathf.Abs(delta.x)) {
            if (delta.y > 0) {
                direction = 0;
            } else if (delta.y < 0) {
                direction = 2;
            } else {
                direction = Utilities.Choice(0, 2);
            }
            if (!pathFinder.DirectionWalkable(tilePosition, direction, "enemy")) {
                if (delta.x > 0) {
                    direction = 1;
                } else if (delta.x < 0) {
                    direction = 3;
                } else {
                    direction = Utilities.Choice(1, 3);
                }
                if (!pathFinder.DirectionWalkable(tilePosition, direction, "enemy")) {
                    direction = (direction + 2) % 4;
                    if (!pathFinder.DirectionWalkable(tilePosition, direction, "enemy")) {
                        direction = -1;
                    }
                }
            }
        } else if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)) {
            if (delta.x > 0) {
                direction = 1;
            } else if (delta.x < 0) {
                direction = 3;
            } else {
                direction = Utilities.Choice(1, 3);
            }
            if (!pathFinder.DirectionWalkable(tilePosition, direction, "enemy")) {
                if (delta.y > 0) {
                    direction = 0;
                } else if (delta.y < 0) {
                    direction = 2;
                } else {
                    direction = Utilities.Choice(0, 2);
                }
                if (!pathFinder.DirectionWalkable(tilePosition, direction, "enemy")) {
                    direction = (direction + 2) % 4;
                    if (!pathFinder.DirectionWalkable(tilePosition, direction, "enemy")) {
                        direction = -1;
                    }
                }
            }
        }
        
        if (direction != -1) {
            // Move!
            targetCell = pathFinder.DirectionToCell(direction, tilePosition);
            targetPosition = floorMap.CellToWorld(targetCell);
            targetPosition.y += 0.25f;
            targetPosition.z = 0;
            
            moving = true;
            highPoint = startPosition +(targetPosition -startPosition)/2 +Vector3.up *0.5f;
            count = 0.0f;
            tilePosition = targetCell;
            // Face enemy in new direction
            if (direction < 2) {
                facing[0] = "up";
            } else {
                facing[0] = "down";
            }
            if (direction == 0 || direction == 3) {
                facing [1] = "left";
            } else {
                facing[1] = "right";
            }
            EndTurn(moveSpeed);
        } else {
            // Not doing jack, report back
            waiting = true;
            count = 0f;
            EndTurn(moveSpeed);
        }
        if (!currentRoom.Contains(tilePosition)) {
            currentRoom = Room.FindByCell(tilePosition, dungeonController.GetRooms());
            targetDoorCell = Vector3Int.zero;
        }
    }

    void Update() {
        if (moving) {
            if (count < 1.0f) {
                count += 1.0f * visualSpeed * Time.deltaTime;

                Vector3 m1 = Vector3.Lerp(startPosition, highPoint, count);
                Vector3 m2 = Vector3.Lerp(highPoint, targetPosition, count);
                this.transform.position = Vector3.Lerp(m1, m2, count);
            } else {
                moving = false;
                entityController.Report();
            }
        } else if (attacking) {
            if (count < 1.0f) {
                count += 1.0f * visualSpeed * Time.deltaTime;

                Vector3 m1 = Vector3.Lerp(startPosition, highPoint, count);
                Vector3 m2 = Vector3.Lerp(highPoint, targetPosition, count);
                this.transform.position = Vector3.Lerp(m1, m2, count);
            } else {
                attacking = false;
                entityController.Report();
            }
        } else if (dying) {
            if (count < 1.0f) {
                count += 1.0f * visualSpeed * Time.deltaTime;
                float t = Mathf.Sin(count * Mathf.PI * 0.5f);
                this.transform.rotation = Quaternion.Lerp(startAngle, targetAngle, t);
            } else {
                Destroy(this.gameObject, 0.5f);
            }
        } else if (waiting) {
            if (count < 1.0f) {
                count += 1.0f * visualSpeed * Time.deltaTime;
            } else {
                waiting = false;
                entityController.Report();
            }
        }
    }
    void Attack(PlayerController target) {
        int roll = Mathf.RoundToInt(Random.Range(1,20+1));
        roll += attack - target.defense;
        if (roll >= 8) {
            int dmg = Random.Range(mindmg,maxdmg+1);
            int crit = 5 + attack-target.defense;
            if (Random.Range(0, 100) < crit) {
                dmg += Random.Range(mindmg,maxdmg+1);
                target.Damage(dmg, "crit");
            } else {
                target.Damage(dmg, "dmg");
            }
        } else {
            target.Damage(0, "miss");
        }
    }

    void RangedAttack(PlayerController target) {
        int damage;
        int roll = Mathf.RoundToInt(Random.Range(1,20+1));
        string style = "dmg";
        roll += attack - target.defense;
        if (roll >= 8) {
            damage = Random.Range(mindmg,maxdmg+1);
        } else {
            damage = 0;
            style = "miss";
        }
        GameObject clone = Instantiate(
            arrow,
            transform.position,
            Quaternion.identity);
        clone.name = clone.name.Split('(')[0];
        clone.GetComponent<EnemyProjectileController>().Shoot(player, damage, style);
    }

    public void Damage(int dmg, string style) {
        GameObject dmgTextFab = Resources.Load("Prefabs/DamageText") as GameObject;
        GameObject text = Instantiate(dmgTextFab, new Vector3(0,0,0), Quaternion.identity, canvas.transform);
        DmgTextController textCont = text.GetComponent<DmgTextController>();
        textCont.Init(this.transform.position, style, dmg.ToString());
        if (dmg > 0) {
            hp -= dmg;
            if (hp <= 0) {
                Die();
            }
        }
    }

    public void FutureDamage(int dmg) {
        if (dmg >= hp) {
            disabled = true;
        }
        return;
    }

    private void EndTurn(float time) {
        timer += time;
    }

    private void Die() {
        dying = true;
        player.GetXP(xp);
        count = 0f;
        startAngle = this.transform.rotation;
        targetAngle = startAngle;
        if (facing[1] == "left") {
            targetAngle.z -= 1;
        } else {
            targetAngle.z += 1;
        }
    }
}
