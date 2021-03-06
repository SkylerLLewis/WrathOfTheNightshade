using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class InventoryController : MonoBehaviour
{
    int selected;
    bool refreshNeeded;
    private PersistentData data;
    private GameObject root, itemArray, eventSystem;
    TextMeshProUGUI title, description, button, stats, gold;
    RectTransform hpBar, foodBar;
    Image itemImage;
    PlayerController player;
    UIController uiController;
    public static Dictionary<string, int> ItemTypeOrder = new Dictionary<string, int>{
        {"Armor", 1},
        {"Weapon", 2},
        {"Scroll", 3},
        {"Potion", 4},
        {"Food", 5}};

    void Start() {
        selected = -1;
        data = GameObject.FindWithTag("Data").GetComponent<PersistentData>();
        // Get inactive Root object stored in persistent data
        root = data.root;
        root.SetActive(false);
        player = root.GetComponentInChildren<PlayerController>();
        uiController = root.GetComponentInChildren<UIController>();

        foreach (Transform child in gameObject.transform) {
            if (child.name == "ItemArray") {
                itemArray = child.gameObject;
            } else if (child.name == "Title") {
                title = child.gameObject.GetComponent<TextMeshProUGUI>();
            } else if (child.name == "Description") {
                description = child.gameObject.GetComponent<TextMeshProUGUI>();
            } else if (child.name == "Activate") {
                button = child.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            } else if (child.name == "Stats") {
                stats = child.gameObject.GetComponent<TextMeshProUGUI>();
            } else if (child.name == "ItemImage") {
                itemImage = child.gameObject.GetComponent<Image>();
            } else if (child.name == "Gold") {
                gold = child.gameObject.GetComponent<TextMeshProUGUI>();
            } else if (child.name == "HP Bar") {
                hpBar = child.GetComponent<RectTransform>();
            } else if (child.name == "Food Bar") {
                foodBar = child.GetComponent<RectTransform>();
            }
        }
        eventSystem = GameObject.Find("EventSystem");

        // Display Everything
        UpdateBars();
        gold.text = data.gold.ToString();
        DisplayItems();
        DisplayItem(-1);
    }

    public void RefreshItems() {
        foreach (Transform child in itemArray.transform) {
            Destroy(child.gameObject);
        }
        DisplayItems();
        if (refreshNeeded) {
            DisplayItem(-1);
        }
    }

    public void DisplayItems() {
        data.inventory.Sort(CompareItems);
        GameObject itemButton = Resources.Load("Prefabs/InventoryItemButton") as GameObject;
        int i=0, j=0;
        float x, y;
        GameObject clone;
        RectTransform rt;
        // Add all inventory items
        foreach (InventoryItem item in data.inventory) {
            x = -7.5f + (i%11)*1.25f;
            y = 4f - j*1.25f;
            clone = Instantiate(
                itemButton,
                itemArray.transform);
            rt = clone.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector3(x, y, 0);
            clone.GetComponent<Image>().overrideSprite =  item.sprite;
            clone.GetComponent<InventoryItemController>().SetItemIndex(i);
            i++;
            if (i%11 == 0) { j++; }
            // Display item count
            if (item.count != 1) {
                clone.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = item.count.ToString();
            }
        }
        // Display Equipped Weapon
        clone = Instantiate(
            itemButton,
            itemArray.transform);
        rt = clone.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector3(7.5f, 2f, 0);
        clone.GetComponent<Image>().overrideSprite =  data.weapon.sprite;
        clone.GetComponent<InventoryItemController>().SetItemIndex(-1);
        // Display Equipped Armor
        if (data.armor != null) {
            clone = Instantiate(
                itemButton,
                itemArray.transform);
            rt = clone.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector3(6.25f, 2f, 0);
            clone.GetComponent<Image>().overrideSprite =  data.armor.sprite;
            clone.GetComponent<InventoryItemController>().SetItemIndex(-2);
        }
    }

    public void DisplayItem(int index) {
        selected = index;
        InventoryItem item;
        if (index == -1) {
            item = data.weapon;
        } else if (index == -2) {
            item = data.armor;
        } else {
            item = data.inventory[index];
        }
        title.text = item.displayName;
        description.text = item.description;
        itemImage.overrideSprite = item.sprite;
        // Display various types
        if (item.itemType == "Weapon") {
            Weapon wep = item as Weapon;
            if (index >= 0) {
                button.text = "Equip";
            } else {
                button.text = "-";
            }
            string stat = ColorStat("Dmg: ", wep.mindmg+wep.maxdmg,
                player.weapon.mindmg+player.weapon.maxdmg, wep);
            stat += ColorStat("attack: ", wep.atk, player.weapon.atk);
            stat += ColorStat("defense: ", wep.def, player.weapon.def);
            stat += ColorStat("atk speed: ", wep.attackSpeed, player.weapon.attackSpeed);
            stat += ColorStat("speed: ", wep.speed, player.weapon.speed);
            stat += ColorStat("mana regen: ", 1+wep.manaRegen, 1+player.weapon.manaRegen);
            stats.text = stat;
        } else if (item.itemType == "Armor") {
            if (index >= 0) {
                button.text = "Equip";
            } else {
                button.text = "-";
            }
            Armor arm = item as Armor;
            string stat = "";
            if (player.armor != null) {
                stat = ColorStat("Def: ", arm.def, player.armor.def);
                stat += ColorStat("armor ", arm.armor, player.armor.armor);
                stat += ColorStat("atk ", arm.atk, player.armor.atk);
                stat += ColorStat("dmg ", arm.dmg, player.armor.dmg);
                stat += ColorStat("spd ", arm.speed, player.armor.speed);
            } else {
                stat = ColorStat("Def: ", arm.def, 0);
                stat += ColorStat("armor ", arm.armor, 0);
                stat += ColorStat("atk ", arm.atk, 0);
                stat += ColorStat("dmg ", arm.dmg, 0);
                stat += ColorStat("spd ", arm.speed, 1f);
            }

            stats.text = stat;
        } else if (item.itemType == "Potion") {
            button.text = "Drink";
            Potion pot = item as Potion;
            string stat = "";
            if (pot.healing > 0) {
                stat += "+"+pot.healing+" hp";
            }
            stats.text = stat;
        } else if (item.itemType == "Food") {
            Food food = item as Food;
            if (player.hp > food.damage) {
                button.text = "Eat";
            } else {
                button.text = "No HP";
            }
            string stat = "Food: "+food.food/10+"%";
            if (food.damage > 0) {
                stat += "\n-"+food.damage+" hp";
            } else if (food.healing > 0) {
                stat += "\n+"+food.healing+" hp";
            }
            stats.text = stat;
        } else if (item.itemType == "Scroll") {
            if (data.mapType == "Dungeon") {
                button.text = "Cast";
            } else {
                button.text = "-";
            }
        }
    }

    public static string ColorStat(string prefix, int stat, int current, Weapon w=null) {
        string s = "";
        if (stat != 0 || stat != current) {
            if (stat > current) {
                s += "<color=#006000>";
            } else if (stat < current) {
                s += "<color=#700000>";
            } else {
                s += "<color=black>";
            }
            s += prefix;
            if (w == null) {
                s += stat;
                if (stat != current && current != 0) {
                    s += " (";
                    if (stat-current > 0) s += "+";
                    s += (stat-current)+")";
                }
                s += "\n";
            } else { // This is a weapon's damage
                float fStat = stat/2f, fCurrent = current/2f;
                s += w.mindmg+"-"+w.maxdmg;
                if (stat != current) {
                    s += " (";
                    if (stat-current > 0) s += "+";
                    s += (fStat-fCurrent)+")";
                }
                s += "\n";
            }
        }
        return s;
    }
    // float overload
    public static string ColorStat(string prefix, float stat, float current) { 
        string s = "";
        if (stat != 1f || stat != current) {
            if (stat > current) {
                s += "<color=#006000>";
            } else if (stat < current) {
                s += "<color=#700000>";
            } else {
                s += "<color=black>";
            }
            s += prefix;
            if (stat > 1f) { s += "+"; }
            s += Mathf.RoundToInt((stat - 1)*100)+"%";
            if (stat != current && current != 1f) {
                s += " (";
                if (stat-current > 0) s += "+";
                s += Mathf.RoundToInt((stat-current)*100)+"%)";
            }
            s += "\n";
        }
        return s;
    }

    public void ActivateButton() {
        if (selected < 0) return;
        bool needToReturn = false;
        refreshNeeded = false;
        InventoryItem item = data.inventory[selected];
        if (item.itemType == "Weapon") {
            EquipWeapon();
        } else if (item.itemType == "Armor") {
            EquipArmor();
        } else if (item.itemType == "Potion") {
            item.Activate(player);
            if (item.count == 0) {
                data.inventory.RemoveAt(selected);
                refreshNeeded = true;
            }
            UpdateBars();
            uiController.UpdateMana();
            uiController.UpdateHp();
        } else if (item.itemType == "Scroll") {
            if (data.mapType != "Dungeon") return;
            item.Activate(player);
            if (item.count == 0) {
                data.inventory.RemoveAt(selected);
                refreshNeeded = true;
            }
            needToReturn = true;
        } else if (item.itemType == "Food") {
            if ((item as Food).damage >= player.hp) {// Not enough hp!
                button.text = "No HP";
                return;
            }
            item.Activate(player);
            if (item.count == 0) {
                data.inventory.RemoveAt(selected);
                refreshNeeded = true;
            }
            UpdateBars();
            uiController.UpdateFood();
        }
        // Return if actions are needed
        if (needToReturn) {
            BackToScene();
        // End the player's turn if in combat
        } else if (player.inCombat) {
            BackToScene();
            player.EndTurn();
        } else {
            RefreshItems();
        }
    }

    public void EquipWeapon() {
        player.EquipWeapon(data.inventory[selected] as Weapon);
        Weapon old = data.weapon;
        data.weapon = data.inventory[selected] as Weapon;
        data.inventory.RemoveAt(selected);
        data.inventory.Add(old);
        DisplayItem(-1);
        player.FloatText("msg", "Equipped "+data.weapon.displayName);
    }

    public void EquipArmor() {
        player.EquipArmor(data.inventory[selected] as Armor);
        Armor old = data.armor;
        data.armor = data.inventory[selected] as Armor;
        data.inventory.RemoveAt(selected);
        if (old != null) {
            data.inventory.Add(old);
        }
        DisplayItem(-2);
        player.FloatText("msg", "Equipped "+data.armor.displayName);
    }

    public void UpdateBars() {
        hpBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 380f*((player.hp*1.0f)/player.maxhp));
        foodBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 380f*(player.food/1000));
    }

    public static int CompareItems(InventoryItem left, InventoryItem right) {
        if (left.displayName == right.displayName) {
            return 0;
        }
        if (ItemTypeOrder[left.itemType] < ItemTypeOrder[right.itemType]) {
            return -1;
        } else if (ItemTypeOrder[left.itemType] > ItemTypeOrder[right.itemType]) {
            return 1;
        } else {
            if (left.tier > right.tier) {
                return -1;
            } else if (left.tier < right.tier) {
                return 1;
            } else {
                if (string.Compare(left.name, right.name) < 0) {
                    return -1;
                } else if (string.Compare(left.name, right.name) > 0) {
                    return 1;
                } else {
                    if (left.quality > right.quality) {
                        return -1;
                    } else if (left.quality < right.quality) {
                        return 1;
                    } else {
                        return 0;
                    }
                }
            }
        }
    }

    public void BackToScene() {
        eventSystem.SetActive(false);
        SceneManager.UnloadSceneAsync("Inventory");
        root.SetActive(true);
    }

}
