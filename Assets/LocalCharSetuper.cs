using InfimaGames.LowPolyShooterPack;
using InfimaGames.LowPolyShooterPack.Interface;
using UnityEngine;

public class LocalCharSetuper : bl_PhotonHelper
{
    [SerializeField] private CharacterBehaviour infimaCharacter;
    [SerializeField] private GameObject[] disableAtStart;
    [SerializeField] private CanvasSpawner _canvasSpawner;
    private void Awake()
    {
        if (isMine)
        {
            bl_EventHandler.onLocalPlayerSpawn += SetupLocalPlayer;
        }
    }

    public void SetupLocalPlayer()
    {
        // Initialization
        bl_EventHandler.onLocalPlayerSpawn -= SetupLocalPlayer;
        infimaCharacter.Initialize();
        
        infimaCharacter.Setup();

        _canvasSpawner.SpawnCanvas();
    }
}
