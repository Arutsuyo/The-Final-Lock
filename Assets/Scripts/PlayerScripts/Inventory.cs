using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	public CameraController cc;
	public List<Item> items = new List<Item>(8);
	public int maxItems = 8;
	public GameObject[] icons;

	public void PickUpItem(Item c)
	{
        // Remove it from the world and pick it up. Note, for server stuff this would issue a request to the server to grab it. The server will grant it to whoever was first.
        cc.IVP.AddNewObject(c);
        //c.gameObject.SetActive(false);
		items.Add(c);

        

		Debug.Log("Picked up a " + c.ItemName + ".");
	}

	public void SilentDelete(Item c)
	{
        // the game object should still be inactive...

        if (items.Contains(c))
            items.Remove(c);
        c.gameObject.SetActive(false); // ASSUMES SOME THINGS
	}

	public bool HasItem(string itemName)
	{
		foreach (Item i in items)
		{
			if (i.ItemName.Equals(itemName))
				return true;
		}

		return false;
	}

	public Item GetItem(string itemName)
	{
		foreach (Item i in items)
		{
			if (i.ItemName.Equals(itemName))
				return i;
		}

		return null;
	}
}
