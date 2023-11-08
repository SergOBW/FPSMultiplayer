using InfimaGames.LowPolyShooterPack;
using InfimaGames.LowPolyShooterPack.Interface;
using UnityEngine;

public class LocalCharSetuper : MonoBehaviour
{
    [SerializeField] private CharacterBehaviour infimaCharacter;
    [SerializeField] private GameObject[] disableAtStart;
    [SerializeField] private CanvasSpawner _canvasSpawner;
    private void Awake()
    {
        foreach (var gameObject in disableAtStart)
        {
            gameObject.SetActive(false);
        }

        bl_EventHandler.onLocalPlayerSpawn += SetupLocalPlayer;
    }

    public void SetupLocalPlayer()
    {
        // Initialization
        bl_EventHandler.onLocalPlayerSpawn -= SetupLocalPlayer;
        infimaCharacter.Initialize();
        
        infimaCharacter.Setup();
        
        foreach (var gameObject in disableAtStart)
        {
            gameObject.SetActive(true);
        }

        _canvasSpawner.SpawnCanvas();
    }
}
