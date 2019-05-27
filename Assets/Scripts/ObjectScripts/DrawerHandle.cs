﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerHandle : MonoBehaviour
{
	public Interactable drawer;
	public bool opened = false;
	public Vector3 posClosed;
	public Vector3 posOpened;
	public Vector3 basePos;
	public float TD = 0;
	public void Start()
	{
		drawer.interactEvent += OpenDrawer;
		drawer.gameInteractComplete += ToggleDrawer;
		basePos = gameObject.transform.localPosition;
		drawer.NeedCheckOwner = false;
	}
	public void ToggleDrawer()
	{
		StopAllCoroutines();
		StartCoroutine(ToggleState());
	}

	public IEnumerator ToggleState()
	{
		opened = !opened;
		if (opened)
		{
			while (TD < 2)
			{
				TD += Time.deltaTime;
				gameObject.transform.localPosition = basePos + gameObject.transform.localRotation*Vector3.Lerp(posClosed, posOpened, TD / 2.0f);
				yield return null;

			}
		}
		else
		{
			while (TD >= 0)
			{
				TD -= Time.deltaTime;
				gameObject.transform.localPosition = basePos + gameObject.transform.localRotation * Vector3.Lerp(posClosed, posOpened, TD / 2.0f);
				yield return null;
			}
		}

	}



	public bool OpenDrawer(CameraController cc)
	{
		drawer.SendSF();

		return false;
	}

}
