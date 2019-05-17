using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PosterClue : MonoBehaviour
{
	public CombinationLock safe;
	public GameObject canv;
	private GameObject[] sprays;
	private List<int> usedSprays = new List<int>();

	// Start is called before the first frame update
	void Start()
	{
		int sprayCount = canv.transform.childCount;
		
		for(int i = 0; i < 3; i++)
		{
			for(int j = 0; j < safe.combo[i]; j++)
			{
				int ran;
				do {
					ran = Random.Range(0, sprayCount);
				} while (usedSprays.Contains(ran));

				GameObject spray = canv.transform.GetChild(ran).gameObject;
				Color setColor;
				switch(i)
				{
					case 0:
						setColor = new Color(255, 0, 0);
						break;
					case 1:
						setColor = new Color(0, 255, 0);
						break;
					case 2:
						setColor = new Color(0, 0, 255);
						break;
					default:
						setColor = new Color(0, 0, 0);
						break;
				}

				spray.GetComponent<RawImage>().color = setColor;
				spray.SetActive(true);
				usedSprays.Add(ran);
			}
		}
	}

}
