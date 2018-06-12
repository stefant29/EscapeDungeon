using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadImage : MonoBehaviour {

	void Start () {
		StartCoroutine(load("https://docs.unity3d.com/uploads/Main/ShadowIntro.png"));
	}
	
    IEnumerator load(string url)
    {
        Texture2D tex;
        tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
		Debug.Log("start");
        using (WWW www = new WWW(url))
        {
            yield return www;
            www.LoadImageIntoTexture(tex);
            GetComponent<Renderer>().material.mainTexture = tex;
			Debug.Log("end");
        }
    }
}
