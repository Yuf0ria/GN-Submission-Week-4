using Fusion;
using Network_Scripts;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTeamSpawnManager : MonoBehaviour
{
    #region Serialized Fields 
        [SerializeField] private GameObject panel; 
        [SerializeField] private Button team1, team2;
        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private Vector3 team1SpawnPosition = new Vector3(-5, 1, 0);
        [SerializeField] private Vector3 team2SpawnPosition = new Vector3(5, 1, 0);
    #endregion

    private bool hasSpawned = false;

    private void Start()
    {
        if (team1 != null && team2 != null)
        {
            team1.interactable = false;
            team1.onClick.AddListener(OnButtonClickTeam1);
            team2.interactable = false;
            team2.onClick.AddListener(OnButtonClickTeam2);
        } 
        else Debug.LogError("❌ Buttons are empty!");
        
        if (NetworkSessionManager.Instance != null)
        {
            if (NetworkSessionManager.Instance.IsSessionReady)
                OnSessionReady();
            else NetworkSessionManager.Instance.OnSessionStarted += OnSessionReady;
        } 
        else Debug.LogError("❌ No NetworkSessionManager in scene!");
    }
    
    private void OnSessionReady()
    {
        Debug.Log("✅ Session ready! Enabling spawn buttons");
        team1.interactable = true;
        team2.interactable = true;
    }
    
    private void OnButtonClickTeam1()
    {
        SpawnPlayer(team1SpawnPosition, "Team 1");
    }
    
    private void OnButtonClickTeam2()
    {
        SpawnPlayer(team2SpawnPosition, "Team 2");
    }
    
    private void SpawnPlayer(Vector3 spawnPosition, string teamName)
    {
        if (hasSpawned)
        {
            Debug.LogWarning("⚠️ Player already spawned!");
            return;
        }

        if (NetworkSessionManager.Instance == null)
        {
            Debug.LogError("❌ Session is NULL!");
            return;
        }
        
        NetworkRunner runner = NetworkSessionManager.Instance.Runner;
        if (runner == null)
        {
            Debug.LogError("❌ Runner is NULL!");
            return;
        }
        
        if (!runner.IsRunning)
        {
            Debug.LogError($"❌ Runner not running! State: IsServer={runner.IsServer}, IsClient={runner.IsClient}");
            return;
        }
        
        if (playerPrefab == null)
        {
            Debug.LogError("❌ No player prefab assigned!");
            return;
        }
        
        Debug.Log($"🎮 SPAWNING PLAYER for {teamName}");
        Debug.Log($"   Position: {spawnPosition}");
        Debug.Log($"   Local Player ID: {runner.LocalPlayer}");
        
        try
        {
            NetworkObject player = runner.Spawn(
                playerPrefab,
                spawnPosition,
                Quaternion.identity,
                runner.LocalPlayer
            );
            
            if (player == null)
            {
                Debug.LogError("❌ Spawn returned null!");
                return;
            }
            
            Debug.Log($"✅✅✅ PLAYER SPAWNED SUCCESSFULLY! ✅✅✅");
            Debug.Log($"   Team: {teamName}");
            Debug.Log($"   NetworkObject ID: {player.Id}");
            Debug.Log($"   Position: {player.transform.position}");
            Debug.Log($"   HasInputAuthority: {player.HasInputAuthority}");
            Debug.Log($"   HasStateAuthority: {player.HasStateAuthority}");
            
            hasSpawned = true;
            panel.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ SPAWN EXCEPTION: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }
    
    private void OnDestroy()
    {
        if (NetworkSessionManager.Instance != null)
        {
            NetworkSessionManager.Instance.OnSessionStarted -= OnSessionReady;
        }
        
        if (team1 != null && team2 != null)
        {
            team1.onClick.RemoveListener(OnButtonClickTeam1);
            team2.onClick.RemoveListener(OnButtonClickTeam2);
        }
    }
}