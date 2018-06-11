using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Globalization;
using System.Threading;
using System.IO;

// using UnityEditor;
using Newtonsoft.Json.Linq;
 
[RequireComponent (typeof (AudioSource))]

public class SingleMicrophoneCapture : MonoBehaviour 
{
	// A handle to the attached AudioSource
	public AudioSource audioSource;

	float[] clipSampleData = new float[1024];
	// minimum noise level
	float minimumLevel = 0.001f;
	// duration of noise before the current speech is stopped
	long pauseInSpeaking = 0;
	// moment of time when the speaker starst talking
	long startSpeaking = 0;
	// moment of time when the speaker stops talking
	long stopSpeaking = 0;
	// check if the user has spoken
	bool spoken = false;
	// temporary folder name for recordings
	string recordingsFolderName;
	// interval to start the recording early
	int delta = 25000;


	/* function called before exiting the application */
 	void OnApplicationQuit() {
		// delete the folder containing microphone recordings
		Debug.Log("Removing recordings folder: " + recordingsFolderName);
		// FileUtil.DeleteFileOrDirectory(recordingsFolderName);
    }

	/* function called at the initialization */
	void Start() {
		recordingsFolderName = Application.persistentDataPath + "/MicRec/";
		initMic();
	}

	/* initialize the microphone recording */
	private void initMic() {
		// get the AudioSource into variable audioSource
		audioSource = this.GetComponent<AudioSource>();
		// start recording using the microphone
		audioSource.clip = Microphone.Start(null, true, 60, 44100);
		// play the recording in realtime
		while (!(Microphone.GetPosition(null) > 0)) { }
		audioSource.Play();
	}

	/* get the average of a float list */
	public float Average(params float[] customerssalary) {
		// sum all elements from the array
		float sum = 0;
		for(int i = 0; i < customerssalary.Length; i++)
			sum += customerssalary[i];
		// get the average by dividing the sum with the length
		return sum / customerssalary.Length;
	}

	/* function called at every frame */
	void Update() {
		audioSource.GetSpectrumData(clipSampleData, 0, FFTWindow.Rectangular);
		float currentAverageVolume = Average(clipSampleData);
		// Debug.Log(currentAverageVolume);

		if (currentAverageVolume > minimumLevel) {
			Debug.Log("Speaking");
			pauseInSpeaking = 0;
			if (!spoken) {			
				startSpeaking = Microphone.GetPosition(null);
				spoken = true;
			}
		} else {
			pauseInSpeaking ++;
		}

		if (spoken && pauseInSpeaking == 80) {
			// reset flag
			spoken = false;

			// get time when speaker stops his speech
			stopSpeaking = Microphone.GetPosition(null);

			// compute the number of samples recorded with delta time before the start
			long noSamples = stopSpeaking - startSpeaking + delta;

			// get the samples
			float[] data = new float[noSamples];
			if ((int)startSpeaking-delta < 0)
				audioSource.clip.GetData(data, 0);
			else
				audioSource.clip.GetData(data, (int)startSpeaking-delta);

			// compute clip length: number of samples / frequency_of_sample
			int clipLength = (int)Math.Ceiling(noSamples * 1.0f / audioSource.clip.frequency);
			
			// reinitialize audioClip with the new clipLength computed
			audioSource.clip = Microphone.Start(null, true, clipLength, 44100);
			// set the selected data as samples in the new audioClip
			audioSource.clip.SetData(data, 0);

			// save audioClip to file
			string fileName = recordingsFolderName + "Audio" + DateTime.Now.ToString("HHmmss");
			SavWav.Save(fileName, audioSource.clip);

			// send WAV file to WIT AI
			StartCoroutine(callWitAI_Wav(fileName + ".wav"));

			// restart microphone
			initMic();
			// pauseInSpeaking = 0;
		}
	}

	/* coroutine to send read and send a wav file to WIT AI */
	IEnumerator callWitAI_Wav (string fileName) {
		// construct the headers needed to authenticate on wit.ai
		Dictionary<string, string> headers = new Dictionary<string,string>();
		headers.Add("Authorization", "Bearer QLLF3GMFKHF3L5ZHYPNO6KJU2D2ZP4EE");
		headers.Add("Content-Type", "audio/wav");

		// open the WAV file int a WWW object
		WWW wav_file = new WWW("file:///" + fileName);
		// wait for the file to be imported
        while (!wav_file.isDone) {
            Debug.Log("uploading");
        }
		
		// send WWW request to URL
		WWW www = new WWW("https://api.wit.ai/speech", wav_file.bytes, headers);
		
		// so it won't stall the main thread
		yield return www;
		
		Debug.Log("Response: " + www.text);

		// parse and call methods returned from wit.ai
		JObject jObject = JObject.Parse(www.text);
		GetComponent<Actions>().m_MyText.text = jObject["_text"].ToString();
		GetComponent<Actions>().parse_WIT_response(www.text);
	}
}
