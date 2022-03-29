using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ItemType
{
    Metal,
    Blade,
    Weapon,
    Hilt
}
[CreateAssetMenu(fileName = "Item", menuName = "Item", order = 1)]
public class ItemObject : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public Sprite sprite;
    public Color color;
    public int basePrice;
    public int[] statBoosts;
    public int amount;
    public float materialAmount;
}