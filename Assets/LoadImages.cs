using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class LoadImages : MonoBehaviour {
	public Text Response1;
    public Text Response2;
    public Text Response3;
    public Text Response4;

    
    Dictionary<string, string> urls = new Dictionary<string, string>();
    List<string> urls_arr;
    string[] actors = {"Brad Pitt", "Angelina Jolie", "Leonardo DiCaprio", "Tom Cruise"};
    string[] cars = {"Ferrari", "Lamborghini", "Bugatti", "Mustang"};
    string[] cartoons = {"Tom & Jerry", "Mister Bean", "Pokemon", "Batman"};
    string[] paintings = {"Monalisa", "Angelina Jolie", "Leonardo DiCaprio", "Tom Cruise"};

    private int currentImage = 0;
    private int correctResponse = -1;

	void Start () {
        // selectQuizDomain("actors");
        // setRandomResponses();
        // loadNextImage();
    }

    public bool checkCorrect(int guess) {
        if (guess == correctResponse) {
            if (currentImage < urls_arr.Count-1) {
                currentImage++;
                Debug.Log("====" +urls_arr.Count + "===="+currentImage);

                setRandomResponses();
                loadNextImage();
            } else {
                GameObject.Find("Room1_trigger").GetComponent<MakeTransitionToRoom1>().makeTransitionLeaveRoom();
                Response1.text = "";
                Response2.text = "";
                Response3.text = "";
                Response4.text = "";
                GetComponent<Renderer>().material.mainTexture = null;
            }
            return true;
        }
        return false;
    }

    public void selectQuizDomain(string domain) {
        switch(domain) {    
            case "actors":
                // make a copy of the selected domain in the "urls_arr" array
                urls_arr = new List<string>(actors);

                urls[actors[0]] = "https://specials-images.forbesimg.com/imageserve/5776a952a7ea436bd18c1204/416x416.jpg?background=000000&cropX1=150&cropX2=543&cropY1=24&cropY2=417";
                urls[actors[1]] = "https://starsunfolded.com/wp-content/uploads/2016/12/Angelina-Jolie.jpg";
                urls[actors[2]] = "http://d17zbv0kd7tyek.cloudfront.net/wp-content/uploads/2015/06/leonardo-dicaprio-fb.jpg";
                urls[actors[3]] = "https://media.wmagazine.com/photos/5a6a18f56c29fa0b4cf8e5a4/4:3/w_1536/tom-cruise-joins-instagram.jpg";
                break;
            case "cars":
                // make a copy of the selected domain in the "urls_arr" array
                urls_arr = new List<string>(actors);

                urls[actors[0]] = "https://specials-images.forbesimg.com/imageserve/5776a952a7ea436bd18c1204/416x416.jpg?background=000000&cropX1=150&cropX2=543&cropY1=24&cropY2=417";
                urls[actors[1]] = "https://starsunfolded.com/wp-content/uploads/2016/12/Angelina-Jolie.jpg";
                urls[actors[2]] = "http://d17zbv0kd7tyek.cloudfront.net/wp-content/uploads/2015/06/leonardo-dicaprio-fb.jpg";
                urls[actors[3]] = "https://media.wmagazine.com/photos/5a6a18f56c29fa0b4cf8e5a4/4:3/w_1536/tom-cruise-joins-instagram.jpg";
                break;
            case "cartoons":
                // make a copy of the selected domain in the "urls_arr" array
                urls_arr = new List<string>(actors);

                urls[actors[0]] = "https://specials-images.forbesimg.com/imageserve/5776a952a7ea436bd18c1204/416x416.jpg?background=000000&cropX1=150&cropX2=543&cropY1=24&cropY2=417";
                urls[actors[1]] = "https://starsunfolded.com/wp-content/uploads/2016/12/Angelina-Jolie.jpg";
                urls[actors[2]] = "http://d17zbv0kd7tyek.cloudfront.net/wp-content/uploads/2015/06/leonardo-dicaprio-fb.jpg";
                urls[actors[3]] = "https://media.wmagazine.com/photos/5a6a18f56c29fa0b4cf8e5a4/4:3/w_1536/tom-cruise-joins-instagram.jpg";
                break;
            case "paintings":
                // make a copy of the selected domain in the "urls_arr" array
                urls_arr = new List<string>(actors);

                urls[actors[0]] = "https://specials-images.forbesimg.com/imageserve/5776a952a7ea436bd18c1204/416x416.jpg?background=000000&cropX1=150&cropX2=543&cropY1=24&cropY2=417";
                urls[actors[1]] = "https://starsunfolded.com/wp-content/uploads/2016/12/Angelina-Jolie.jpg";
                urls[actors[2]] = "http://d17zbv0kd7tyek.cloudfront.net/wp-content/uploads/2015/06/leonardo-dicaprio-fb.jpg";
                urls[actors[3]] = "https://media.wmagazine.com/photos/5a6a18f56c29fa0b4cf8e5a4/4:3/w_1536/tom-cruise-joins-instagram.jpg";
                break;
            default:
                break;
        }
    }

    public string getRandomElem(List<string> orig, List<string> rest, int crtIndex) {
        int i = Random.Range(0, rest.Count);
        string img = rest[i];
        rest.RemoveAt(i);
        if (img.Equals(orig[currentImage]))
            correctResponse = crtIndex;

        return img;
    }

    public void setRandomResponses() {
        List<string> copy_urls_arr = new List<string>(urls_arr);

        if (currentImage >= urls_arr.Count)
            return;

        Response1.text = "1. " + getRandomElem(urls_arr, copy_urls_arr, 1);
        Response2.text = "2. " + getRandomElem(urls_arr, copy_urls_arr, 2);
        Response3.text = "3. " + getRandomElem(urls_arr, copy_urls_arr, 3);
        Response4.text = "4. " + getRandomElem(urls_arr, copy_urls_arr, 4);

        Debug.Log("correct: " + correctResponse + "   " + currentImage + "   " + actors[currentImage] + "  " + urls_arr.Count + "   " + copy_urls_arr.Count);
    }
	
    IEnumerator load(string url) {
        Texture2D tex;
        tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        using (WWW www = new WWW(url)) {
            yield return www;
            www.LoadImageIntoTexture(tex);
            GetComponent<Renderer>().material.mainTexture = tex;
        }
    }

    public void loadNextImage() {
        // Random.Range(0, urls.Count);
        if (urls_arr.Count >= currentImage)
            StartCoroutine(load(urls[urls_arr[currentImage]]));
        else
            GetComponent<Renderer>().material.mainTexture = null;
    }

}
