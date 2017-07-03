using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimation : MonoBehaviour {

	public static UIAnimation UIA;

	void Awake()
	{
		if (UIA != null) 
		{
			GameObject.Destroy (UIA);
		} 
		else 
		{
			UIA = this;
		}
		DontDestroyOnLoad(this);
	}

	public void activate_attack_node(GameObject atkNode, bool on)
	{
		Debug.Log("Activating attack node!");
		if (on)
		{
			// Play an animation that lights up the attack node
			StartCoroutine(activate_attack_node_anim(atkNode));
		}
		else
		{
			// Reset the state of the node without animations
			atkNode.transform.GetChild(0).GetComponent<Image>().fillAmount = 0;
		}
	}

	IEnumerator activate_attack_node_anim(GameObject atkNode)
	{
		int frames = 10;
		Image filler = atkNode.transform.GetChild(0).GetComponent<Image>();
		for (int i = 0; i < frames; i++)
		{
			filler.fillAmount = (i + 1f) / frames;
			// WaitForEndOfFrame is not affected by timescale change
			yield return new WaitForEndOfFrame();
		}
	}

	public void activate_movement_bar(GameObject movementBar, bool on)
	{
//		Debug.Log("Activating attack node!");
		if (on)
		{
			// Play an animation that lights up the attack node
			StartCoroutine(activate_movement_bar_anim(movementBar));
		}
		else
		{
			// Reset the state of the node without animations
			movementBar.GetComponent<Scrollbar>().size = 0;
		}
	}

	IEnumerator activate_movement_bar_anim(GameObject movementBar)
	{
		int frames = 25;
		Scrollbar filler = movementBar.GetComponent<Scrollbar>();
		for (int i = 0; i < frames; i++)
		{
			filler.size = (i + 1f) / frames;
			yield return new WaitForEndOfFrame();
		}
	}
}
