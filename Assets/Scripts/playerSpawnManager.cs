using Fusion;
using Network_Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawnManager : MonoBehaviour
{
    #region Serialized Fields 
        //privates
        //canvas
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] public TMP_Dropdown materialsDropdown; 
        [SerializeField] private GameObject panel; 
        [SerializeField] private Button button;
        //player
        [SerializeField] private NetworkObject playerPrefab;
        //public
        [SerializeField] public Material[] materials; //mats for dropdown
    #endregion

    private void Start()
    {
        //load dropdown options @ the start
        DropdownOptions();
        //set button to false unless session is ready
        if (button != null)
        {
            //disabled to make the spawn of the player appear above the plane
            button.interactable = false;
            button.onClick.AddListener(OnButtonClick);
        } 
        else Debug.LogError("button is empty!");
        //and if session manager is not null
        if (NetworkSessionManager.Instance != null)
        {
            if (NetworkSessionManager.Instance.IsSessionReady)
                OnSessionReady();
            else NetworkSessionManager.Instance.OnSessionStarted += OnSessionReady;
        } else Debug.LogError("No session manager at the scene");
    }
    
    private void OnSessionReady()
    {
        Debug.Log("Session is ready!, setting btn to true");
        button.interactable = true;
    }
    
    private void DropdownOptions()
    {
        Debug.Log("Total no. of Mats: " + materials.Length);
        materialsDropdown.ClearOptions();
        foreach (var mats in materials)
            materialsDropdown.options.Add(new TMP_Dropdown.OptionData(mats.name));
        materialsDropdown.RefreshShownValue();
    }
    
    private void OnButtonClick()
    {
        //adding a debug because spawning keeps on failing
        if (NetworkSessionManager.Instance == null)
            Debug.LogError("Session is NULL!");
        // Check Runner
        NetworkRunner runner = NetworkSessionManager.Instance.Runner;
        if (runner == null)
            Debug.LogError("Runner is NULL!");
        else Debug.Log("✓ Runner exists");
        
        // Check if running
        if (!runner.IsRunning)
        {
            Debug.LogError("napagod na runner natin at ayaw na mag run, halaaa");
            //if all the errors are false it means that the network session manager is deleted or deactivated
            Debug.LogError($"State: IsServer={runner.IsServer}, IsClient={runner.IsClient}, IsRunning={runner.IsRunning}");
            return;
        }
        //returns all true
        Debug.Log($"✓ Runner is running (IsServer={runner.IsServer}, IsClient={runner.IsClient})");
        
        //Check prefab
        if (playerPrefab == null)
            Debug.LogError("no player prefab on the inspector");
        else Debug.Log($"✓ Prefab assigned: {playerPrefab.name}");
        
        //text to string
        string playerName = inputField.text;
        int materialDropdownIndex = materialsDropdown.value;
        
        Debug.Log($"Vector3 Position: {playerPrefab.transform.position}");
        
        try
        {
            NetworkObject player = runner.Spawn(
                playerPrefab,
                new Vector3(0, 1, 0),
                Quaternion.identity,
                runner.LocalPlayer
            );
            
            if (player == null)
                Debug.LogError("spawn returned null");
            else Debug.Log($"Player is spawned: {player.name} at {player.transform.position}");
            
            PlayerCustomization customization = player.GetComponent<PlayerCustomization>();
            
            if (customization != null)
            {
                customization.InsPlayerInfo(playerName, materialDropdownIndex);
                Debug.Log($"Customization applied: {playerName}, Material Index: {materialDropdownIndex}");
            }
            else
            {
                Debug.LogWarning("⚠️ PlayerCustomization component not found on player!");
            }
                
            panel.SetActive(false);
            Debug.Log("✅ Panel hidden, spawn complete!");
        }
        catch (System.Exception e) //Makes the system send a message
        {
            Debug.LogError($"❌ SPAWN EXCEPTION: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from event
        if (NetworkSessionManager.Instance != null)
        {
            NetworkSessionManager.Instance.OnSessionStarted -= OnSessionReady;
        }
        
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}