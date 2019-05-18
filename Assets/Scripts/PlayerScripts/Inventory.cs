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
		c.gameObject.SetActive(false);
		items.Add(c);

		if (c.tag == "Lockpick")
			cc.iconPick.SetActive(true);
		if (c.tag == "Tension")
			cc.iconTension.SetActive(true);
		if (c.tag == "Key")
			cc.iconKey.SetActive(true);

		Debug.Log("Picked up a " + c.ItemName + ".");
	}

	public void SilentDelete(Item c)
	{
		// the game object should still be inactive...
		items.Remove(c);
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
