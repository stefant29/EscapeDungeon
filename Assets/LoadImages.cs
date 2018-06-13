using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

/*
 * Load images to the frame, get random responses and check user input correctness
 */
public class LoadImages : MonoBehaviour {
    // GUI helping answer options, displayed along the frame
	public Text Response1;
    public Text Response2;
    public Text Response3;
    public Text Response4;
    
    // dictionary with keys - strings and values - urls
    private Dictionary<string, string> urls = new Dictionary<string, string>();
    // current selected array of quiz domains: actors, cars, cartoons or paintings
    private List<string> urls_arr;
    // lists of quiz responses
    private string[] actors = {"Brad Pitt", "Angelina Jolie", "Leonardo DiCaprio", "Tom Cruise"};
    private string[] cars = {"Ferrari", "Lamborghini", "Bugatti", "Mustang"};
    private string[] cartoons = {"Tom & Jerry", "Mister Bean", "Pokemon", "Batman"};
    private string[] paintings = {"Monalisa", "Van Gogh, self-portrait", "Birth of Venus", "Last Supper"};

    // index of current loaded image
    private int currentImage = 0;
    // the index of the correct image response
    private int correctResponse = -1;

    /* check if the guess is correct */
    public bool checkCorrect(int guess) {
        if (guess == correctResponse) {
            if (currentImage < urls_arr.Count-1) {
                // increment number of current image
                currentImage++;
                // load the next image
                loadNextImage();
            } else {
                // if there are no more images to load, reset the frame and the options
                // and start the transition for exiting the room1
                GameObject.Find("Room1_enter").GetComponent<MakeTransitionToRooms>().exitRoom1();
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

    // return random image from a list
    public string getRandomElem(List<string> orig, List<string> rest, int crtIndex) {
        int i = Random.Range(0, rest.Count);
        string img = rest[i];
        rest.RemoveAt(i);
        if (img.Equals(orig[currentImage]))
            correctResponse = crtIndex;

        return img;
    }

    /* Set random answer options */
    public void setRandomResponses() {
        // create a copy of the current list
        List<string> copy_urls_arr = new List<string>(urls_arr);

        if (currentImage >= urls_arr.Count)
            return;

        // get random answer options
        Response1.text = "1. " + getRandomElem(urls_arr, copy_urls_arr, 1);
        Response2.text = "2. " + getRandomElem(urls_arr, copy_urls_arr, 2);
        Response3.text = "3. " + getRandomElem(urls_arr, copy_urls_arr, 3);
        Response4.text = "4. " + getRandomElem(urls_arr, copy_urls_arr, 4);
    }
	
    /* load an image from the given url and set it as texture to the object */
    IEnumerator load(string url) {
        Texture2D tex;
        tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        using (WWW www = new WWW(url)) {
            yield return www;
            www.LoadImageIntoTexture(tex);
            GetComponent<Renderer>().material.mainTexture = tex;

            // after loading the image-texture, set the random answer options
            setRandomResponses();
        }
    }

    /* load the next image from the list */
    public void loadNextImage() {
        // if there is an image to load, load it
        if (urls_arr.Count >= currentImage)
            StartCoroutine(load(urls[urls_arr[currentImage]]));
        // else, remove the texture
        else
            GetComponent<Renderer>().material.mainTexture = null;
    }
    
    /* depending on the domain, set the urls to images */
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
                urls_arr = new List<string>(cars);

                urls[cars[0]] = "https://barcelonagttours.com/wp-content/uploads/2017/04/conducir-ferrari-barcelona-gt-tours.jpg";
                urls[cars[1]] = "https://media.wired.com/photos/59b1a379762606611bf55655/1:1/w_2400,c_limit/LamborghiniRoadsterHP.jpg";
                urls[cars[2]] = "https://cdn.vox-cdn.com/thumbor/lMSbaGt5SUSqRCYVZaYuMZkriyY=/1400x1400/filters:format(jpeg)/cdn.vox-cdn.com/uploads/chorus_asset/file/10355461/02_BUGATTI_Chiron_Sport_34_front_WEB.jpg";
                urls[cars[3]] = "https://www.cjponyparts.com/media/catalog/product/cache/1/image/9df78eab33525d08d6e5fb8d27136e95/9/8/98021_1.1246.jpg";
                break;
            case "cartoons":
                // make a copy of the selected domain in the "urls_arr" array
                urls_arr = new List<string>(cartoons);

                urls[cartoons[0]] = "https://lh3.googleusercontent.com/XE7nMAE8LrEzwBddKBV-yoZ4lttsVbjKPFf62A3eddVU06PnK_rIF0UNez_vedAsDsM59qaMHwZkyszR3g";
                urls[cartoons[1]] = "https://pbs.twimg.com/profile_images/748442111293464576/ZvaI1B6__400x400.jpg";
                urls[cartoons[2]] = "https://cdn.vox-cdn.com/thumbor/jdubq0_u5Pu_F3IP4ufO6Tf0Zps=/100x0:1180x720/1400x1400/filters:focal(100x0:1180x720):format(jpeg)/cdn.vox-cdn.com/uploads/chorus_image/image/33832359/pokemon-black-and-white-anime-screenshot_1280.0.jpg";
                urls[cartoons[3]] = "https://fsmedia.imgix.net/80/c5/7c/0d/bd4d/49d0/b9b2/cc8c46cf2ebf/batman-the-animated-series.png?fm=jpg&w=800&h=800&crop=entropy&fit=crop";
                break;
            case "paintings":
                // make a copy of the selected domain in the "urls_arr" array
                urls_arr = new List<string>(paintings);

                urls[paintings[0]] = "https://cdn.shopify.com/s/files/1/1307/5697/products/product-image-413387377_1024x1024.jpg?v=1511217017";
                urls[paintings[1]] = "https://muurmeesters.nl/wp-content/uploads/2017/05/MUURMEESTERS-Van-Gogh-Selfportrait-verkleind-600x600.jpg";
                urls[paintings[2]] = "https://ih0.redbubble.net/image.466230612.2113/flat,800x800,075,t.u6.jpg";
                urls[paintings[3]] = "https://static.dezeen.com/uploads/2008/02/lastsuppersq.jpg";
                break;
            default:
                break;
        }
    }

}
