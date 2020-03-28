using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StoryManager : MonoBehaviour
{
	public UnityEngine.UI.Text storyTextBox;
	public TextAsset[] storyData;
	public int nextSceneIndex;

	private int storyIndex;

	// Use this for initialization
	void Start ()
	{
		storyTextBox.text = storyData [0].text;
		storyIndex = 0;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.anyKeyDown)
		{
			storyIndex++;
			if (storyIndex < storyData.Length)
			{
				storyTextBox.text = storyData [storyIndex].text;
			}
			else
			{
				SceneManager.LoadScene (nextSceneIndex);
			}
		}
	}
}
