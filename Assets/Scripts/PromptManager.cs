
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PromptManager : MonoBehaviour
{

	public RectTransform mainTrans;
	public InputField input;
	public bool reset = false;
	public float timeToPlay = 2f;
	public AnimationCurve position;
	public bool IsActive { get; private set; }
	public System.Action<System.Object> callback;
	public int upperBound;
	public int lowerBound;
	public string defaultText;
	public bool bad = false;
	public float badTime = 0;
	public Text UITitle;
	public Text SubmitTitle;
	public Vector3 pos;
	public Text SampleText;
	public void Submitted()
	{
		if (!IsActive) { return; }
		if (input.text.Length == 0)
		{
			input.text = defaultText;
		}
		switch (input.contentType)
		{
			case InputField.ContentType.IntegerNumber:
				int iop;
				Debug.Log(input.text);
				if (int.TryParse(input.text, out iop))
				{
					if (iop < lowerBound || iop > upperBound)
					{
						bad = true;
						badTime = Time.time;
						return;
					}
				}
				else
				{
					bad = true;
					badTime = Time.time;
					return;
				}
				break;
			default:
				if (input.text.Length < lowerBound)
				{
					bad = true;
					badTime = Time.time;
					return;
				}
				break;
		}
		directionUp = Random.Range(0, 2) == 1;
		IsActive = false;
		callback(input.text);
		input.text = "";

		callback = null;
		StartCoroutine("MoveAway");
	}

	public void Cancelled()
	{
		if (!IsActive) { return; }
		IsActive = false;
		directionUp = Random.Range(0, 2) == 1;
		input.text = "";
		callback(null);
		callback = null;
		StartCoroutine("MoveAway");
	}
	private bool directionUp = false;
	private float speed = 0;
	private float offset = 0;
	public IEnumerator MoveAway()
	{
		offset = Time.time;
		while (Time.time - offset <= timeToPlay)
		{

			mainTrans.position = new Vector3(
				Screen.width / 2f,
				Screen.height / 2f
				+ (directionUp ? -1 : 1) * Screen.height *
				position.Evaluate((Time.time - offset) / timeToPlay),
				0);
			yield return null;
		}
	}

	public IEnumerator MoveTowards()
	{
		offset = Time.time + timeToPlay;
		while (offset - Time.time >= 0)
		{

			mainTrans.position = new Vector3(
				Screen.width / 2f,
				Screen.height / 2f +
				(directionUp ? -1 : 1) * Screen.height *
				position.Evaluate((offset - Time.time) / timeToPlay),
				0);
			yield return null;
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		pos = input.transform.localPosition;
	}
	public bool GetAcknowledge(System.Action<System.Object> a, string title = "Please enter a number:", string submitText = "")
	{
		if (IsActive) { return false; }
		input.gameObject.SetActive(false);
		// Assumes user input the hint in here.
		UITitle.text = title;
		input.characterLimit = 1;//Mathf.CeilToInt(Mathf.Log10(Mathf.Max(Mathf.Abs(upperBound), Mathf.Abs(lowerBound))));
		IsActive = true;
		callback = a;
		lowerBound = 0;
		upperBound = 1;
		input.text = "";
		SubmitTitle.text = submitText;
		input.contentType = InputField.ContentType.Standard;
		StartCoroutine("MoveTowards");
		return true;
	}
	public bool GetNumber(System.Action<System.Object> a, int lowerBound = int.MinValue, int upperBound = int.MaxValue, string title = "Please enter a number:", string sampleText = "", string submitText = "")
	{
		if (IsActive) { return false; };
		UITitle.text = title; // Assumes user inputted the hint in here.
		this.upperBound = upperBound;
		this.lowerBound = lowerBound;
		input.characterLimit = Mathf.CeilToInt(Mathf.Log10(Mathf.Max(Mathf.Abs(upperBound), Mathf.Abs(lowerBound))));
		IsActive = true;
		SampleText.text = sampleText;
		callback = a;
		input.text = "";
		SubmitTitle.text = submitText;
		defaultText = sampleText;
		input.contentType = InputField.ContentType.IntegerNumber;
		input.gameObject.SetActive(true);
		StartCoroutine("MoveTowards");
		return true;
	}

	public bool GetString(System.Action<System.Object> a, int lowerBound = 0, int upperBound = int.MaxValue, string title = "Please enter a string:", string sampleText = "", string submitText = "")
	{
		if (IsActive) { return false; };
		UITitle.text = title; // Assumes user inputted the hint in here.
		this.upperBound = upperBound;
		this.lowerBound = lowerBound;
		input.characterLimit = upperBound;
		SampleText.text = sampleText;
		IsActive = true;
		SubmitTitle.text = submitText;
		callback = a;
		input.text = "";
		defaultText = sampleText;
		input.gameObject.SetActive(true);
		input.contentType = InputField.ContentType.Standard;
		StartCoroutine("MoveTowards");
		return true;
	}
	// Update is called once per frame
	void Update()
	{
		if (bad)
		{
			if (Time.time - badTime > 1f)
			{
				bad = false;
			}
			else
			{
				input.transform.localPosition = pos + new Vector3(
					Mathf.Sin(Mathf.PingPong(12 * (Time.time - badTime), 1f)) * 5f,
					0,
					0);
			}
		}
	}
}
