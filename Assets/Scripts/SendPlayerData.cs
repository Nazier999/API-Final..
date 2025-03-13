using TMPro;
using UnityEngine;

public class SendPlayerData : MonoBehaviour
{
    public TMP_InputField screenName;
    public TMP_InputField fName;
    public TMP_InputField lName;
    public TMP_InputField level;
    public TMP_InputField score;

    public PostData post;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SendData()
    {
        if (screenName.text != "" && fName.text != "" && score.text != "" && lName.text != "" && level.text != "")
        {
            int scoreData = int.Parse(score.text);
            int levelData = int.Parse(level.text);
            post.SetupPlayerData(screenName.text, fName.text, lName.text, levelData, scoreData);
        }
    }
}