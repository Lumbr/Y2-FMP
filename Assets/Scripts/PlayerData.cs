using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class PlayerData : MonoBehaviour
{
    #region Singleton Shit
    public static PlayerData instance;

    public static SaveManager saveManager;
    private void Awake()
    {
        
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            saveManager = new SaveManager();
            saveManager.LookForSaves();
            instance = this;
        }
        DontDestroyOnLoad(instance);
    }
    #endregion
    public List<(ItemObject item, int amount)> inventory = new List<(ItemObject, int)> { };
    public int money;
    public int matchesDone;
    public float playTime;
    public Controls controls = new Controls(KeyCode.C);

    public List<InventoryItem> uiItems;
    public InventoryItem uiItemPrefab;
    public Transform inventoryPlace;
    public GameObject forge, fight, pause;
    public bool fighting;
    public bool paused;
    public bool menu;
    public DialogueManager dMan;
    
    void Update()
    {
        /*if (paused)
        {
            Time.timeScale = 0;
            pause.SetActive(true);
        }*/
        playTime += Time.deltaTime;
        if (menu)
        {
            forge.SetActive(false);
            fight.SetActive(false);
            return;
        }
        
        if (fighting)
        {
            forge.SetActive(false);
            fight.SetActive(true);
        }
        else
        {
            forge.SetActive(true);
            fight.SetActive(false);
        }
    }
    void UpdateInventory()
    {

        foreach (InventoryItem uiItem in uiItems) if(uiItem) Destroy(uiItem.gameObject);
        uiItems.Clear();
        int count = 0;
        foreach (var item in inventory)
        {
            count++;
            var something = Instantiate(uiItemPrefab, inventoryPlace);
            uiItems.Add(something);
            something.Instantiate(item.item, item.amount);
            if (count % 2 == 1)
            {
                something.transform.localPosition = new Vector3(-60, (-(count - 1) * 80), 0);
            }
            else
            {
                something.transform.localPosition = new Vector3(60, (-(count / 2 - 1) * 80), 0);
            }

        }
    }
    
    public void AddItem(ItemObject item, int amount)
    {
        int index = inventory.FindIndex(x => x.item == item);
        if(index != -1) inventory[index] = (item, inventory[index].amount + amount);
        else inventory.Add((item, amount));
        UpdateInventory();
    }
    public bool RemoveItem(ItemObject item, int amount)
    {
        int index = inventory.FindIndex(x => x.item == item);
        if (index != -1)
        {
            if (inventory[index].amount >= amount)
            {
                inventory[index] = (item, inventory[index].amount - amount);
                if (inventory[index].amount <= 0) inventory.Remove(inventory[index]);
                UpdateInventory();
                return true;
            }
            else return false;
        }
        else return false;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Item item))
        {
            if (!item.clickedOn)
            {
                AddItem(item.item, 1);
                Destroy(item.gameObject);
            }
        }
    }
}
public class SaveManager
{
    
    public List<SaveData> saves = new List<SaveData> { };
    // save on index 0 is always AutoSave
    // any further indexes are manual if implemented
    public void AutoSave()
    {
        if (saves.Count >= 1) 
            saves[0] = new SaveData(
                PlayerData.instance.inventory, 
                PlayerData.instance.money, 
                PlayerData.instance.matchesDone, 
                PlayerData.instance.playTime, 
                PlayerData.instance.controls);
        else 
            saves.Add(new SaveData(
                PlayerData.instance.inventory, 
                PlayerData.instance.money, 
                PlayerData.instance.matchesDone, 
                PlayerData.instance.playTime, 
                PlayerData.instance.controls));
        saves[0].Save("AutoSave");
    }
    public void LookForSaves()
    {
        Debug.Log(Application.persistentDataPath);
        saves.Clear();
        if (LocateSave("AutoSave", out SaveData data))
        {
            saves.Add(data);
            Debug.Log(data.money);
        }
        else Debug.Log("No autosave present");
        for ( int i = 1; ; i++ )
            if(LocateSave($"SaveSlot{i}", out data)) 
                saves.Add(data);
            else return;
    }
    public void ManualSave()
    {
        var currentSave = new SaveData(PlayerData.instance.inventory, PlayerData.instance.money, PlayerData.instance.matchesDone, PlayerData.instance.playTime, PlayerData.instance.controls);
        saves.Add(currentSave);
        currentSave.Save($"SaveSlot{saves.IndexOf(currentSave)}");
    }
    public void LoadSave(int index)
    {
        if(saves.Count > index)
        {
            PlayerData.instance.inventory = saves[index].inventory;
            PlayerData.instance.money = saves[index].money;
            PlayerData.instance.matchesDone = saves[index].matchesDone;
            PlayerData.instance.controls = saves[index].controls;
        }
    }
    bool LocateSave(string name, out SaveData data)
    {
        if (File.Exists(Application.persistentDataPath + $"/{name}.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + $"/{name}.dat", FileMode.Open);
            data = (SaveData)bf.Deserialize(file);
            Debug.Log("Save Loaded");
            return true;
        }
        else
        {
            Debug.Log("There is no save data!");
            data = null;
            return false;
        }
    }
    [Serializable]
    public class SaveData
    {
        public List<(ItemObject item, int amount)> inventory = new List<(ItemObject, int)> { };
        public int money;
        public int matchesDone; 
        public float playTime;
        public Controls controls = new Controls(KeyCode.C);
        
        public SaveData(List<(ItemObject item, int amount)> inventory, int money, int matchesDone, float timePlayed, Controls controls)
        {
            this.inventory = inventory;
            this.money = money;
            this.matchesDone = matchesDone;
            this.playTime = playTime;
            this.controls = controls;
        }
        public void LoadSave()
        {
            PlayerData.instance.inventory = inventory;
            PlayerData.instance.money = money;
            PlayerData.instance.matchesDone = matchesDone;
            PlayerData.instance.playTime = playTime;
            PlayerData.instance.controls = controls;
        }
        public void Save(string name)
        {
            DeleteSave(name);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + $"/{name}.dat");
            bf.Serialize(file, this);
            file.Close();
            Debug.Log("Game data saved!");
        }

        public void DeleteSave(string name)
        {
            if (File.Exists(Application.persistentDataPath + $"/{name}.dat"))
                File.Delete(Application.persistentDataPath + $"/{name}.dat");
        }
    }
}
[Serializable]
public class Controls
{
    public KeyCode punch;

    public Controls(KeyCode punch)
    {
        this.punch = punch;
    }
}