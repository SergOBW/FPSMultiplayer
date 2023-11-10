//Copyright 2022, Infima Games. All Rights Reserved.

using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Weapon Attachment Manager. Handles equipping and storing a Weapon's Attachments.
    /// </summary>
    public class WeaponAttachmentManager : WeaponAttachmentManagerBehaviour
    {
        #region FIELDS SERIALIZED

        [Title(label: "Scope")]

        [Tooltip("Determines if the ironsights should be shown on the weapon model.")]
        [SerializeField]
        private bool scopeDefaultShow = true;
        
        [Tooltip("Default Scope!")]
        [SerializeField]
        private ScopeBehaviour scopeDefaultBehaviour;

        [Tooltip("Selected Scope Index. If you set this to a negative number, ironsights will be selected as the enabled scope.")]
        [SerializeField]
        private int scopeIndex = -1;

        [Tooltip("First scope index when using random scopes.")]
        [SerializeField]
        private int scopeIndexFirst = -1;
        
        [Tooltip("Should we pick a random index when starting the game?")]
        [SerializeField]
        private bool scopeIndexRandom;

        [Tooltip("All possible Scope Attachments that this Weapon can use!")]
        [SerializeField]
        private ScopeBehaviour[] scopeArray;
        
        [Title(label: "Muzzle")]

        [Tooltip("Selected Muzzle Index.")]
        [SerializeField]
        private int muzzleIndex;
        
        [Tooltip("Should we pick a random index when starting the game?")]
        [SerializeField]
        private bool muzzleIndexRandom = true;

        [Tooltip("All possible Muzzle Attachments that this Weapon can use!")]
        [SerializeField]
        private MuzzleBehaviour[] muzzleArray;
        
        [Title(label: "Laser")]

        [Tooltip("Selected Laser Index.")]
        [SerializeField]
        private int laserIndex = -1;
        
        [Tooltip("Should we pick a random index when starting the game?")]
        [SerializeField]
        private bool laserIndexRandom = true;

        [Tooltip("All possible Laser Attachments that this Weapon can use!")]
        [SerializeField]
        private LaserBehaviour[] laserArray;
        
        [Title(label: "Grip")]

        [Tooltip("Selected Grip Index.")]
        [SerializeField]
        private int gripIndex = -1;
        
        [Tooltip("Should we pick a random index when starting the game?")]
        [SerializeField]
        private bool gripIndexRandom = true;

        [Tooltip("All possible Grip Attachments that this Weapon can use!")]
        [SerializeField]
        private GripBehaviour[] gripArray;
        
        [Title(label: "Magazine")]

        [Tooltip("Selected Magazine Index.")]
        [SerializeField]
        private int magazineIndex;
        
        [Tooltip("Should we pick a random index when starting the game?")]
        [SerializeField]
        private bool magazineIndexRandom = true;

        [Tooltip("All possible Magazine Attachments that this Weapon can use!")]
        [SerializeField]
        private Magazine[] magazineArray;
        
        public bool isFPWeapon = true;

        #endregion

        #region FIELDS

        /// <summary>
        /// Equipped Scope.
        /// </summary>
        private ScopeBehaviour scopeBehaviour;
        /// <summary>
        /// Equipped Muzzle.
        /// </summary>
        private MuzzleBehaviour muzzleBehaviour;
        /// <summary>
        /// Equipped Laser.
        /// </summary>
        private LaserBehaviour laserBehaviour; 
        /// <summary>
        /// Equipped Grip.
        /// </summary>
        private GripBehaviour gripBehaviour;
        /// <summary>
        /// Equipped Magazine.
        /// </summary>
        private MagazineBehaviour magazineBehaviour;
        
        private PhotonView photonView;
        private bl_Gun Gun;
        private bool isSync;
        private int[] AttachmentsIds = new int[6] { 0, 0, 0, 0, 0, 0};
        public string WeaponName;
        
        

        #endregion

        #region UNITY FUNCTIONS

        /// <summary>
        /// Awake.
        /// </summary>
        /*
        protected override void Awake()
        {
            //Randomize. This allows us to spice things up a little!
            if (scopeIndexRandom)
                scopeIndex = Random.Range(scopeIndexFirst, scopeArray.Length);
            //Select Scope!
            scopeBehaviour = scopeArray.SelectAndSetActive(scopeIndex);
            //Check if we have no scope. This could happen if we have an incorrect index.
            if (scopeBehaviour == null || scopeBehaviour.isActiveAndEnabled)
            {
                //Select Default Scope.
                scopeBehaviour = scopeDefaultBehaviour;
                //Set Active.
                scopeBehaviour.gameObject.SetActive(scopeDefaultShow);
            }
            
            //Randomize. This allows us to spice things up a little!
            if (muzzleIndexRandom)
                muzzleIndex = Random.Range(0, muzzleArray.Length);
            //Select Muzzle!
            muzzleBehaviour = muzzleArray.SelectAndSetActive(muzzleIndex);

            //Randomize. This allows us to spice things up a little!
            if (laserIndexRandom)
                laserIndex = Random.Range(0, laserArray.Length);
            //Select Laser!
            laserBehaviour = laserArray.SelectAndSetActive(laserIndex);
            
            //Randomize. This allows us to spice things up a little!
            if (gripIndexRandom)
                gripIndex = Random.Range(0, gripArray.Length);
            //Select Grip!
            gripBehaviour = gripArray.SelectAndSetActive(gripIndex);
            
            //Randomize. This allows us to spice things up a little!
            if (magazineIndexRandom)
                magazineIndex = Random.Range(0, magazineArray.Length);
            //Select Magazine!
            magazineBehaviour = magazineArray.SelectAndSetActive(magazineIndex);
        }
        */
        

        #endregion

        #region GETTERS

        public override ScopeBehaviour GetEquippedScope() => scopeBehaviour;
        public override ScopeBehaviour GetEquippedScopeDefault() => scopeDefaultBehaviour;

        public override MagazineBehaviour GetEquippedMagazine() => magazineBehaviour;
        public override MuzzleBehaviour GetEquippedMuzzle() => muzzleBehaviour;

        public override LaserBehaviour GetEquippedLaser() => laserBehaviour;
        public override GripBehaviour GetEquippedGrip() => gripBehaviour;

        #endregion
        
        public override void SetupAttachments(string json = "0,0,0,0,0,0")
        {
            if(photonView == null) { photonView = bl_PlayerReferences.LocalPlayer.photonView; }
            if (isFPWeapon && !isSync)
            {
                Gun = GetComponent<bl_Gun>();
                WeaponName = bl_CustomizerData.Instance.Weapons[4].WeaponName;
                Debug.Log(WeaponName);
                AttachmentsIds = bl_CustomizerData.Instance.LoadAttachmentsForWeapon(WeaponName);
                ApplyAttachments();
                string line = bl_CustomizerData.Instance.CompileArray(AttachmentsIds);
                photonView.RPC("SyncCustomizer", RpcTarget.Others, Gun.GunID, line);
                isSync = true;
                bl_PhotonCallbacks.PlayerEnteredRoom += OnNewPlayerEnter;
            }
            else
            {
                Debug .Log(json);
                AttachmentsIds = bl_CustomizerData.Instance.DecompileLine(json);
                ApplyAttachments();
            }
        }

        private void ApplyAttachments()
        {
            if (AttachmentsIds == new []{0,0,0,0,0,0})
            {
                //Randomize. This allows us to spice things up a little!
                scopeBehaviour = scopeDefaultBehaviour;
                //Set Active.
                scopeBehaviour.gameObject.SetActive(true);
            
                //Randomize. This allows us to spice things up a little!
                muzzleIndex = 0;
                //Select Muzzle!
                muzzleBehaviour = muzzleArray.SelectAndSetActive(muzzleIndex);

                //Randomize. This allows us to spice things up a little!
                if (laserArray.Length > 0)
                {                laserIndex = 0;
                    //Select Laser!
                    laserBehaviour = laserArray.SelectAndSetActive(laserIndex);
                    
                }

                if (gripArray.Length > 0 )
                {
                    gripIndex = 0;
                    //Select Grip!
                    gripBehaviour = gripArray.SelectAndSetActive(gripIndex);
                }
            
                //Randomize. This allows us to spice things up a little!
                magazineIndex = 0;
                //Select Magazine!
                magazineBehaviour = magazineArray.SelectAndSetActive(magazineIndex);
            }

            muzzleIndex = AttachmentsIds[0];
            scopeIndex = AttachmentsIds[1];
            gripIndex = AttachmentsIds[2];
            magazineIndex = AttachmentsIds[3];
            laserIndex = AttachmentsIds[4];
            
            scopeBehaviour = scopeArray[scopeIndex];
            
            scopeBehaviour.gameObject.SetActive(true);
            
            muzzleBehaviour = muzzleArray.SelectAndSetActive(muzzleIndex);
            
            laserBehaviour = laserArray.SelectAndSetActive(laserIndex);

            gripBehaviour = gripArray.SelectAndSetActive(gripIndex);
            
            magazineBehaviour = magazineArray.SelectAndSetActive(magazineIndex);
        }

        private void OnNewPlayerEnter(Player obj)
        {
            if (isFPWeapon)
            {
                string line = bl_CustomizerData.Instance.CompileArray(AttachmentsIds);
                photonView.RPC("SyncCustomizer", RpcTarget.Others, Gun.GunID, line);
            }
        }
        
        private void OnDestroy()
        {
            bl_PhotonCallbacks.PlayerEnteredRoom -= OnNewPlayerEnter;
        }
    }
}