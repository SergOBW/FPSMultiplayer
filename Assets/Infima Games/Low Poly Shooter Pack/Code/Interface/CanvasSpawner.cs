//Copyright 2022, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack.Interface
{
    /// <summary>
    /// Player Interface.
    /// </summary>
    public class CanvasSpawner : bl_PhotonHelper
    {
        #region FIELDS SERIALIZED

        [Title(label: "Settings")]
        
        [Tooltip("Canvas prefab spawned at start. Displays the player's user interface.")]
        [SerializeField]
        private GameObject canvasPrefab;
        
        [Tooltip("Quality settings menu prefab spawned at start. Used for switching between different quality settings in-game.")]
        [SerializeField]
        private GameObject qualitySettingsPrefab;

        private GameObject _canvasGo;
        private GameObject _qualitySettingsGo;

        #endregion

        #region UNITY

        public void SpawnCanvas()
        {
            //Spawn Interface.
            if (bl_GameManager.Instance != null)
            {
                if (isMine)
                {
                    _canvasGo = Instantiate(canvasPrefab);
                    //Spawn Quality Settings Menu.
                    _qualitySettingsGo = Instantiate(qualitySettingsPrefab);
                
                    bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDeath;

                }
            }
            else
            {
                _canvasGo = Instantiate(canvasPrefab);
                //Spawn Quality Settings Menu.
                _qualitySettingsGo = Instantiate(qualitySettingsPrefab);
            }
        }
        private void OnLocalPlayerDeath()
        {
            bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDeath;
            Destroy(_canvasGo);
            Destroy(_qualitySettingsGo);
        }

        #endregion
        
    }
}