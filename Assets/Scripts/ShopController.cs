using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    private int selected;
    private string mode = "buying"; 
    private PersistentData data;
    private GameObject root, itemArray, eventSystem, textFab;
    private Button buyButton, sellButton;
    TextMeshProUGUI title, description, button, stats, goldText;
    Image itemImage;
    PlayerController player;
    List<InventoryItem> activeList;

    void Start()
    {
        selected = 0;
        data = GameObject.FindWithTag("Data").GetComponent<PersistentData>();
        // Get inactive Root object stored in persistent data
        root = data.root;
        root.SetActive(false);
        player = root.GetComponentInChildren<PlayerController>();
        textFab = Resources.Load("Prefabs/DamageText") as GameObject;
        activeList = data.shopList;

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
                goldText = child.gameObject.GetComponent<TextMeshProUGUI>();
            } else if (child.name == "Buying") {
                buyButton = child.gameObject.GetComponent<Button>();
            } else if (child.name == "Selling") {
                sellButton = child.gameObject.GetComponent<Button>();
            }
        }
        eventSystem = GameObject.Find("EventSystem");

        buyButton.enabled = false;

        // Display Everything
        goldText.text = data.gold.ToString();
        DisplayItems();
        DisplayItem(0);
    }

    public void RefreshItems() {
        foreach (Transform child in itemArray.transform) {
            Destroy(child.gameObject);
        }
        if (mode == "selling") {
            data.inventory.Sort(InventoryController.CompareItems);
        }
        DisplayItems();
        DisplayItem(selected);
    }

    public void DisplayItems() {
        GameObject itemButton = Resources.Load("Prefabs/InventoryItemButton") as GameObject;
        if (itemButton == null) Debug.Log("ITEMBUTTON IS NULL");
        int i=0, j=0;
        float x, y;
        GameObject clone;
        RectTransform rt;
        // Add all shop items
        foreach (InventoryItem item in activeList) {
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
    }

    public void DisplayItem(int index) {
        selected = index;
        InventoryItem item = activeList[index];
        title.text = item.displayName;
        description.text = item.description;
        itemImage.overrideSprite = item.sprite;
        string stat = "";
        if (mode == "buying") {
            if (data.gold >= item.cost*5) {
                button.text = "Buy";
                stat = "cost: "+item.cost*5+"\n";
            } else {
                button.text = "-";
                stat = "cost: <color=#700000>"+item.cost*5+"<color=#000000>\n";
            }
        } else if (mode == "selling") {
            button.text = "Sell";
            stat = "Worth: <color=#007000>"+item.cost+"<color=#000000>\n";
        }

        // Display various types
        if (item.itemType == "Weapon") {
            Weapon wep = item as Weapon;
            stat += InventoryController.ColorStat("Dmg: ", wep.mindmg+wep.maxdmg,
                player.weapon.mindmg+player.weapon.maxdmg, wep);
            stat += InventoryController.ColorStat("attack: ", wep.atk, player.weapon.atk);
            stat += InventoryController.ColorStat("defense: ", wep.def, player.weapon.def);
            stat += InventoryController.ColorStat("atk speed: ", wep.attackSpeed, player.weapon.attackSpeed);
            stat += InventoryController.ColorStat("speed:  ", wep.speed, player.weapon.speed);
            stat += InventoryController.ColorStat("mana regen: ", 1+wep.manaRegen, 1+player.weapon.manaRegen);
        } else if (item.itemType == "Armor") {
            Armor arm = item as Armor;
            if (player.armor != null) {
                stat += InventoryController.ColorStat("Def: ", arm.def, player.armor.def);
                stat += InventoryController.ColorStat("armor ", arm.armor, player.armor.armor);
                stat += InventoryController.ColorStat("atk ", arm.atk, player.armor.atk);
                stat += InventoryController.ColorStat("dmg ", arm.dmg, player.armor.dmg);
                stat += InventoryController.ColorStat("spd ", arm.speed, player.armor.speed);
            } else {
                stat += InventoryController.ColorStat("Def: ", arm.def, 0);
                stat += InventoryController.ColorStat("armor ", arm.armor, 0);
                stat += InventoryController.ColorStat("atk ", arm.atk, 0);
                stat += InventoryController.ColorStat("dmg ", arm.dmg, 0);
                stat += InventoryController.ColorStat("spd ", arm.speed, 1f);
            }
        } else if (item.itemType == "Potion") {
            Potion pot = item as Potion;
            stat += "";
            if (pot.healing > 0) {
                stat += "+"+pot.healing+" hp";
            }
        } else if (item.itemType == "Food") {
            Food food = item as Food;
            stat += "Food: "+food.food/10+"%";
            if (food.damage > 0) {
                stat += "\n-"+food.damage+" hp";
            } else if (food.healing > 0) {
                stat += "\n+"+food.healing+" hp";
            }
        } else if (item.itemType == "Scroll") {
            
        }
        stats.text = stat;
    }

    public void ActivateButton() {
        InventoryItem item = activeList[selected];
        if (mode == "buying") {
            if (data.gold >= item.cost*5) {
                data.AddToInventory(item.Copy());
                data.gold -= item.cost*5;
                goldText.text = data.gold.ToString();
                // Float cost text
                GameObject text = Instantiate(textFab, new Vector3(0,0,0), Quaternion.identity, gameObject.transform);
                DmgTextController textCont = text.GetComponent<DmgTextController>();
                textCont.Init(goldText.transform.position, "cost", "-"+(item.cost*5).ToString());
            }
        } else if (mode == "selling") {
            data.gold += item.cost;
            goldText.text = data.gold.ToString();
            data.RemoveFromInventory(selected);
            // Float cost text
            GameObject text = Instantiate(textFab, new Vector3(0,0,0), Quaternion.identity, gameObject.transform);
            DmgTextController textCont = text.GetComponent<DmgTextController>();
            textCont.Init(goldText.transform.position, "gold", "+"+(item.cost).ToString());
            
            RefreshItems();
        }
    }

    public void ChangeToSelling() {
        buyButton.enabled = true;
        sellButton.enabled = false;
        activeList = data.inventory;
        mode = "selling";
        selected = 0;
        RefreshItems();
    }

    public void ChangeToBuying() {
        sellButton.enabled = true;
        buyButton.enabled = false;
        activeList = data.shopList;
        mode = "buying";
        selected = 0;
        RefreshItems();
    }

    public void BackToScene() {
        eventSystem.SetActive(false);
        SceneManager.UnloadSceneAsync("Shop");
        root.SetActive(true);
        player.enabled = true;
    }
}
