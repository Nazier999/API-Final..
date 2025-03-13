using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class Edit_Delete : MonoBehaviour
{
    public string serverUrl = "http://localhost:3000/player";

    public GameObject playerItemPrefab;
    public Transform contentPanel;

    private List<PlayerData> players = new List<PlayerData>();

    public GameObject editPanel;

    public TMP_InputField screenNameInput;
    public TMP_InputField firstNameInput;
    public TMP_InputField lastNameInput;
    public TMP_InputField levelInput;
    public TMP_InputField scoreInput;

    public Button saveButton;
    public Button cancelButton;

    // Stores player currently being edited
    private PlayerData selectedPlayer;

    void Start()
    {
        StartCoroutine(GetPlayers());
    }

    public void FetchAndDisplayPlayers()
    {
        StartCoroutine(GetPlayers());
    }

    private IEnumerator GetPlayers()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(serverUrl))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"Received Players JSON: {json}");

                players = JsonConvert.DeserializeObject<List<PlayerData>>(json);
                players = players.OrderBy(p => p.screenName).ToList();

                DisplayPlayers();
            }
            else
            {
                Debug.LogError("Error fetching players: " + request.error);
            }
        }
    }

    private void DisplayPlayers()
    {
        // Clear old entries
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (PlayerData player in players)
        {
            GameObject playerItem = Instantiate(playerItemPrefab, contentPanel);

            TMP_Text[] textFields = playerItem.GetComponentsInChildren<TMP_Text>();

            textFields[0].text = player.screenName;
            textFields[1].text = player.firstName;
            textFields[2].text = player.lastName;
            textFields[3].text = player.level.ToString();
            textFields[4].text = player.score.ToString();

            Button[] buttons = playerItem.GetComponentsInChildren<Button>();

            //which player is being selected
            PlayerData capturedPlayer = player;
            buttons[0].onClick.RemoveAllListeners();
            buttons[0].onClick.AddListener(() => OpenEditPanel(capturedPlayer));

            buttons[1].onClick.RemoveAllListeners();
            buttons[1].onClick.AddListener(() => DeletePlayer(capturedPlayer));
        }
    }

    private void OpenEditPanel(PlayerData player)
    {
        selectedPlayer = player;

        // Populate input fields with current data
        screenNameInput.text = player.screenName;  // Ensure screenName is set
        firstNameInput.text = player.firstName;
        lastNameInput.text = player.lastName;
        levelInput.text = player.level.ToString();
        scoreInput.text = player.score.ToString();

        editPanel.SetActive(true);
    }


    public void SavePlayerEdits()
    {
        if (selectedPlayer == null) return;

        // Update selected player object with new values
        selectedPlayer.screenName = screenNameInput.text;  // Add this line to update screen name
        selectedPlayer.firstName = firstNameInput.text;
        selectedPlayer.lastName = lastNameInput.text;
        selectedPlayer.level = int.Parse(levelInput.text);
        selectedPlayer.score = int.Parse(scoreInput.text);

        // Convert updated player to JSON
        string json = JsonConvert.SerializeObject(selectedPlayer);

        // Update player on the server
        StartCoroutine(UpdatePlayerRequest(json, selectedPlayer.screenName));
    }


    private IEnumerator UpdatePlayerRequest(string json, string playerName)
    {
        string url = $"http://localhost:3000/update/{playerName}";


        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Player updated: {playerName}");
                editPanel.SetActive(false);
                FetchAndDisplayPlayers(); // Refresh list
            }
            else
            {
                Debug.LogError($"Error updating player: {request.error}");
            }
        }
    }


    private void DeletePlayer(PlayerData player)
    {
        Debug.Log($"Deleting {player.screenName}");
        StartCoroutine(DeletePlayerRequest(player.screenName));
    }

    private IEnumerator DeletePlayerRequest(string playerName)
    {
        string url = $"http://localhost:3000/delete/{playerName}";
        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Player {playerName} deleted successfully.");
                FetchAndDisplayPlayers();
            }
            else
            {
                Debug.LogError($"Error deleting player: {request.error}");
            }
        }
    }

    public void CancelEdit()
    {
        editPanel.SetActive(false);
    }
}


