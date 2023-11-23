
using InfimaGames.LowPolyShooterPack;
using InfimaGames.LowPolyShooterPack.Interface;
using UnityEngine;

public class LocalGameManager : MonoBehaviour
{
    [SerializeField] private CharacterBehaviour infimaCharacter;
    [SerializeField] private GameObject[] disableAtStart;
    [SerializeField] private CanvasSpawner _canvasSpawner;

    private void Awake()
    {
        infimaCharacter.Initialize();
        
        infimaCharacter.Setup();

        _canvasSpawner.SpawnCanvas();
    }
}
