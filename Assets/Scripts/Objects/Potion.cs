using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : InventoryItem {
    public int healing;
    public float speed;
    public static List<List<string>> PotionTiers = new List<List<string>>() {
        new List<string>() {"Health Potion"}
    };
    public Potion(string n) {
        itemType = "Potion";
        name = n;
        displayName = n;
        count = 1;
        sprite = Resources.Load<Sprite>("Potions/"+name);
        if (sprite == null) {
            Debug.Log("Potion name \""+name+"\" does not exist.");
        }
        Classify();
    }

    private void Classify() {
        healing = 0;
        speed = 0f;
        if (name == "Health Potion") {
            description = "A pungent, herbal smelling healing potion.";
            healing = 20;
            cost = 20;
        }
    }

    public override void Activate(PlayerController player) {
        count--;
        if (healing > 0) {
            player.Heal(healing);
        }
    }

    public override InventoryItem Copy()
    {
        return new Potion(this.name);
    }

    public static bool IsPotion(string s) {
        foreach (List<string> tier in PotionTiers) {
            if (tier.Contains(s)) {
                return true;
            }
        }
        return false;
    }
}