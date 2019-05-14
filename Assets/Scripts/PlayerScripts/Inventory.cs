using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>(8);
    public int maxItems = 8;
        
    public void PickUpItem(Item c)
    {
        // Remove it from the world and pick it up. Note, for server stuff this would issue a request to the server to grab it. The server will grant it to whoever was first.
        c.gameObject.SetActive(false);
        items.Add(c);
        Debug.Log("Picked up a " + c.ItemName + ".");
    }
    public void SilentDelete(Item c)
    {
        // the game object should still be inactive...
        items.Remove(c);
    }
    public bool HasItem(string itemName)
    {
        foreach(Item i in items)
        {
            if (i.ItemName.Equals(itemName))
            {
                return true;
            }
        }
        return false;
    }

}
