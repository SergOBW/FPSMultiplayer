//Copyright 2022, Infima Games. All Rights Reserved.

using InfimaGames.LowPolyShooterPack.Legacy;
using JetBrains.Annotations;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Weapon. This class handles most of the things that weapons need.
    /// </summary>
    public class Weapon : WeaponBehaviour
    {
        #region FIELDS SERIALIZED
        
        [Title(label: "Settings")]
        
        [Tooltip("Weapon Name. Currently not used for anything, but in the future, we will use this for pickups!")]
        [SerializeField] 
        private string weaponName;

        [SerializeField] private float damage;
        [SerializeField] private GunType WeaponType;
        
        [SerializeField] int weaponID;

        [Tooltip("How much the character's movement speed is multiplied by when wielding this weapon.")]
        [SerializeField]
        private float multiplierMovementSpeed = 1.0f;
        
        [Title(label: "Firing")]

        [Tooltip("Is this weapon automatic? If yes, then holding down the firing button will continuously fire.")]
        [SerializeField] 
        private bool automatic;
        
        [Tooltip("Is this weapon bolt-action? If yes, then a bolt-action animation will play after every shot.")]
        [SerializeField]
        private bool boltAction;

        [Tooltip("Amount of shots fired at once. Helpful for things like shotguns, where there are multiple projectiles fired at once.")]
        [SerializeField]
        private int shotCount = 1;
        
        [Tooltip("How far the weapon can fire from the center of the screen.")]
        [SerializeField]
        private float spread = 0.25f;
        
        [Tooltip("How fast the projectiles are.")]
        [SerializeField]
        private float projectileImpulse = 400.0f;

        [Tooltip("Amount of shots this weapon can shoot in a minute. It determines how fast the weapon shoots.")]
        [SerializeField] 
        private int roundsPerMinutes = 200;

        [Title(label: "Reloading")]
        
        [Tooltip("Determines if this weapon reloads in cycles, meaning that it inserts one bullet at a time, or not.")]
        [SerializeField]
        private bool cycledReload;
        
        [Tooltip("Determines if the player can reload this weapon when it is full of ammunition.")]
        [SerializeField]
        private bool canReloadWhenFull = true;

        [Tooltip("Should this weapon be reloaded automatically after firing its last shot?")]
        [SerializeField]
        private bool automaticReloadOnEmpty;

        [Tooltip("Time after the last shot at which a reload will automatically start.")]
        [SerializeField]
        private float automaticReloadOnEmptyDelay = 0.25f;

        [Title(label: "Animation")]

        [Tooltip("Transform that represents the weapon's ejection port, meaning the part of the weapon that casings shoot from.")]
        [SerializeField]
        private Transform socketEjection;

        [Tooltip("Settings this to false will stop the weapon from being reloaded while the character is aiming it.")]
        [SerializeField]
        private bool canReloadAimed = true;

        [Title(label: "Resources")]

        [Tooltip("Casing Prefab.")]
        [SerializeField]
        private GameObject prefabCasing;
        
        [Tooltip("Projectile Prefab. This is the prefab spawned when the weapon shoots.")]
        [SerializeField]
        private GameObject prefabProjectile;
        
        [Tooltip("The AnimatorController a player character needs to use while wielding this weapon.")]
        [SerializeField] 
        public RuntimeAnimatorController controller;

        [Tooltip("Weapon Body Texture.")]
        [SerializeField]
        private Sprite spriteBody;
        
        [Title(label: "Audio Clips Holster")]

        [Tooltip("Holster Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipHolster;

        [Tooltip("Unholster Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipUnholster;
        
        [Title(label: "Audio Clips Reloads")]

        [Tooltip("Reload Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReload;
        
        [Tooltip("Reload Empty Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReloadEmpty;
        
        [Title(label: "Audio Clips Reloads Cycled")]
        
        [Tooltip("Reload Open Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReloadOpen;
        
        [Tooltip("Reload Insert Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReloadInsert;
        
        [Tooltip("Reload Close Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReloadClose;
        
        [Title(label: "Audio Clips Other")]

        [Tooltip("AudioClip played when this weapon is fired without any ammunition.")]
        [SerializeField]
        private AudioClip audioClipFireEmpty;
        
        [Tooltip("")]
        [SerializeField]
        private AudioClip audioClipBoltAction;

        #endregion

        #region FIELDS

        /// <summary>
        /// Weapon Animator.
        /// </summary>
        private Animator animator;
        /// <summary>
        /// Attachment Manager.
        /// </summary>
        private WeaponAttachmentManagerBehaviour attachmentManager;

        /// <summary>
        /// Amount of ammunition left.
        /// </summary>
        private int ammunitionCurrent;

        #region Attachment Behaviours
        
        /// <summary>
        /// Equipped scope Reference.
        /// </summary>
        private ScopeBehaviour scopeBehaviour;
        
        /// <summary>
        /// Equipped Magazine Reference.
        /// </summary>
        private MagazineBehaviour magazineBehaviour;
        /// <summary>
        /// Equipped Muzzle Reference.
        /// </summary>
        private MuzzleBehaviour muzzleBehaviour;

        /// <summary>
        /// Equipped Laser Reference.
        /// </summary>
        private LaserBehaviour laserBehaviour;
        /// <summary>
        /// Equipped Grip Reference.
        /// </summary>
        private GripBehaviour gripBehaviour;

        #endregion

        /// <summary>
        /// The GameModeService used in this game!
        /// </summary>
        private IGameModeService gameModeService;
        /// <summary>
        /// The main player character behaviour component.
        /// </summary>
        [SerializeField]private CharacterBehaviour characterBehaviour;

        /// <summary>
        /// The player character's camera.
        /// </summary>
        private Transform playerCamera;
        
        #endregion

        #region Equip

        public override void Equip()
        {
            animator = GetComponent<Animator>();
            //Get Attachment Manager.
            attachmentManager = GetComponent<WeaponAttachmentManagerBehaviour>();
            
            attachmentManager.SetupAttachments();
            
            characterBehaviour = bl_GameManager.Instance.LocalPlayerReferences.playerCharacter;
            //Cache the world camera. We use this in line traces.
            playerCamera = characterBehaviour.GetCameraWorld().transform;

            //Get Scope.
            scopeBehaviour = attachmentManager.GetEquippedScope();
            
            //Get Magazine.
            magazineBehaviour = attachmentManager.GetEquippedMagazine();
            //Get Muzzle.
            muzzleBehaviour = attachmentManager.GetEquippedMuzzle();

            //Get Laser.
            laserBehaviour = attachmentManager.GetEquippedLaser();
            //Get Grip.
            gripBehaviour = attachmentManager.GetEquippedGrip();

            //Max Out Ammo.
            ammunitionCurrent = magazineBehaviour.GetAmmunitionTotal();
            
            PlayerNetwork = bl_PlayerReferences.LocalPlayer.playerNetwork;
            PlayerCamera = bl_PlayerReferences.LocalPlayer.playerCamera;
        }
        #endregion
        
        #region GETTERS

        /// <summary>
        /// GetFieldOfViewMultiplierAim.
        /// </summary>
        public override float GetFieldOfViewMultiplierAim()
        {
            //Make sure we don't have any issues even with a broken setup!
            if (scopeBehaviour != null) 
                return scopeBehaviour.GetFieldOfViewMultiplierAim();
            
            //Error.
            Debug.LogError("Weapon has no scope equipped!");
  
            //Return.
            return 1.0f;
        }
        /// <summary>
        /// GetFieldOfViewMultiplierAimWeapon.
        /// </summary>
        public override float GetFieldOfViewMultiplierAimWeapon()
        {
            //Make sure we don't have any issues even with a broken setup!
            if (scopeBehaviour != null) 
                return scopeBehaviour.GetFieldOfViewMultiplierAimWeapon();
            
            //Error.
            Debug.LogError("Weapon has no scope equipped!");
  
            //Return.
            return 1.0f;
        }
        
        /// <summary>
        /// GetAnimator.
        /// </summary>
        public override Animator GetAnimator() => animator;
        /// <summary>
        /// CanReloadAimed.
        /// </summary>
        public override bool CanReloadAimed() => canReloadAimed;

        /// <summary>
        /// GetSpriteBody.
        /// </summary>
        public override Sprite GetSpriteBody() => spriteBody;
        /// <summary>
        /// GetMultiplierMovementSpeed.
        /// </summary>
        public override float GetMultiplierMovementSpeed() => multiplierMovementSpeed;

        /// <summary>
        /// GetAudioClipHolster.
        /// </summary>
        public override AudioClip GetAudioClipHolster() => audioClipHolster;
        /// <summary>
        /// GetAudioClipUnholster.
        /// </summary>
        public override AudioClip GetAudioClipUnholster() => audioClipUnholster;

        /// <summary>
        /// GetAudioClipReload.
        /// </summary>
        public override AudioClip GetAudioClipReload() => audioClipReload;
        /// <summary>
        /// GetAudioClipReloadEmpty.
        /// </summary>
        public override AudioClip GetAudioClipReloadEmpty() => audioClipReloadEmpty;
        
        /// <summary>
        /// GetAudioClipReloadOpen.
        /// </summary>
        public override AudioClip GetAudioClipReloadOpen() => audioClipReloadOpen;
        /// <summary>
        /// GetAudioClipReloadInsert.
        /// </summary>
        public override AudioClip GetAudioClipReloadInsert() => audioClipReloadInsert;
        /// <summary>
        /// GetAudioClipReloadClose.
        /// </summary>
        public override AudioClip GetAudioClipReloadClose() => audioClipReloadClose;

        /// <summary>
        /// GetAudioClipFireEmpty.
        /// </summary>
        public override AudioClip GetAudioClipFireEmpty() => audioClipFireEmpty;
        /// <summary>
        /// GetAudioClipBoltAction.
        /// </summary>
        public override AudioClip GetAudioClipBoltAction() => audioClipBoltAction;

        /// <summary>
        /// GetAudioClipFire.
        /// </summary>
        [CanBeNull]
        public override AudioClip GetAudioClipFire()
        {
            if (muzzleBehaviour != null)
            {
                return muzzleBehaviour.GetAudioClipFire();
            }
            return null;
        }
        /// <summary>
        /// GetAmmunitionCurrent.
        /// </summary>
        public override int GetAmmunitionCurrent() => ammunitionCurrent;

        /// <summary>
        /// GetAmmunitionTotal.
        /// </summary>
        public override int GetAmmunitionTotal() => magazineBehaviour.GetAmmunitionTotal();
        /// <summary>
        /// HasCycledReload.
        /// </summary>
        public override bool HasCycledReload() => cycledReload;

        /// <summary>
        /// IsAutomatic.
        /// </summary>
        public override bool IsAutomatic() => automatic;
        /// <summary>
        /// IsBoltAction.
        /// </summary>
        public override bool IsBoltAction() => boltAction;

        /// <summary>
        /// GetAutomaticallyReloadOnEmpty.
        /// </summary>
        public override bool GetAutomaticallyReloadOnEmpty() => automaticReloadOnEmpty;
        /// <summary>
        /// GetAutomaticallyReloadOnEmptyDelay.
        /// </summary>
        public override float GetAutomaticallyReloadOnEmptyDelay() => automaticReloadOnEmptyDelay;

        /// <summary>
        /// CanReloadWhenFull.
        /// </summary>
        public override bool CanReloadWhenFull() => canReloadWhenFull;
        /// <summary>
        /// GetRateOfFire.
        /// </summary>
        public override float GetRateOfFire() => roundsPerMinutes;
        
        /// <summary>
        /// IsFull.
        /// </summary>
        public override bool IsFull() => ammunitionCurrent == magazineBehaviour.GetAmmunitionTotal();
        /// <summary>
        /// HasAmmunition.
        /// </summary>
        public override bool HasAmmunition() => ammunitionCurrent > 0;

        /// <summary>
        /// GetAnimatorController.
        /// </summary>
        public override RuntimeAnimatorController GetAnimatorController() => controller;
        /// <summary>
        /// GetAttachmentManager.
        /// </summary>
        public override WeaponAttachmentManagerBehaviour GetAttachmentManager() => attachmentManager;

        #endregion

        #region METHODS

        /// <summary>
        /// Reload.
        /// </summary>
        public override void Reload()
        {
            //Set Reloading Bool. This helps cycled reloads know when they need to stop cycling.
            const string boolName = "Reloading";
            animator.SetBool(boolName, true);
            
            //Try Play Reload Sound.
            ServiceLocator.Current.Get<IAudioManagerService>().PlayOneShot(HasAmmunition() ? audioClipReload : audioClipReloadEmpty, new AudioSettings(1.0f, 0.0f, false));
            
            //Play Reload Animation.
            animator.Play(cycledReload ? "Reload Open" : (HasAmmunition() ? "Reload" : "Reload Empty"), 0, 0.0f);
        }
        /// <summary>
        /// Fire.
        /// </summary>
        public override void Fire(float spreadMultiplier = 1.0f)
        {
            //We need a muzzle in order to fire this weapon!
            if (muzzleBehaviour == null)
                return;
            
            //Make sure that we have a camera cached, otherwise we don't really have the ability to perform traces.
            if (playerCamera == null)
                return;

            //Play the firing animation.
            const string stateName = "Fire";
            animator.Play(stateName, 0, 0.0f);
            //Reduce ammunition! We just shot, so we need to get rid of one!
            ammunitionCurrent = Mathf.Clamp(ammunitionCurrent - 1, 0, magazineBehaviour.GetAmmunitionTotal());

            //Set the slide back if we just ran out of ammunition.
            if (ammunitionCurrent == 0)
                SetSlideBack(1);
            
            //Play all muzzle effects.
            muzzleBehaviour.Effect();

            //Spawn as many projectiles as we need.
            for (var i = 0; i < shotCount; i++)
            {
                //Determine a random spread value using all of our multipliers.
                Vector3 spreadValue = Random.insideUnitSphere * (spread * spreadMultiplier);
                //Remove the forward spread component, since locally this would go inside the object we're shooting!
                spreadValue.z = 0;
                //Convert to world space.
                spreadValue = playerCamera.TransformDirection(spreadValue);

                //Spawn projectile from the projectile spawn point.
                GameObject projectile = Instantiate(prefabProjectile, playerCamera.position, Quaternion.Euler(playerCamera.eulerAngles + spreadValue));
                //Add velocity to the projectile.
                BuildBulletData();
                projectile.GetComponent<Projectile>().SetupProjectile(BulletSettings);
                var instanceData = GetBulletPosition();
                PlayerNetwork.ReplicateFire(GunType.Machinegun, instanceData.ProjectedHitPoint, BulletSettings.Inaccuracity);
                projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileImpulse;
            }
        }

        /// <summary>
        /// FillAmmunition.
        /// </summary>
        public override void FillAmmunition(int amount)
        {
            //Update the value by a certain amount.
            ammunitionCurrent = amount != 0 ? Mathf.Clamp(ammunitionCurrent + amount, 
                0, GetAmmunitionTotal()) : magazineBehaviour.GetAmmunitionTotal();
        }
        /// <summary>
        /// SetSlideBack.
        /// </summary>
        public override void SetSlideBack(int back)
        {
            //Set the slide back bool.
            const string boolName = "Slide Back";
            animator.SetBool(boolName, back != 0);
        }

        /// <summary>
        /// EjectCasing.
        /// </summary>
        public override void EjectCasing()
        {
            //Spawn casing prefab at spawn point.
            if(prefabCasing != null && socketEjection != null)
                Instantiate(prefabCasing, socketEjection.position, socketEjection.rotation);
        }

        public override int GetGunId()
        {
            return weaponID;
        }

        #endregion

        #region BL_GUN
        public BulletData BulletSettings = new BulletData();
        public bl_PlayerNetwork PlayerNetwork;
        public Camera PlayerCamera;
        private static bl_Gun.BulletInstanceData bulletInstanceData;
        RaycastHit hit;
        public void BuildBulletData()
        {
            BulletSettings.Damage = damage;
            BulletSettings.ImpactForce = projectileImpulse;
            BulletSettings.SetInaccuracity(spread, spread);
            BulletSettings.Speed = roundsPerMinutes;
            BulletSettings.WeaponName = weaponName;
            BulletSettings.Position = bl_PlayerReferences.LocalPlayer.Position;
            BulletSettings.WeaponID = bl_PlayerReferences.LocalPlayer.gunManager.GetCurrentGunID;
            BulletSettings.isNetwork = false;
            BulletSettings.DropFactor = shotCount;
            BulletSettings.Range = 1000;
            BulletSettings.ActorViewID = bl_GameManager.LocalPlayerViewID;
            BulletSettings.MFPSActor = bl_GameManager.Instance.LocalActor;
            BulletSettings.IsLocalPlayer = true;
        }
        /*
        
        public override FireType GetFireType()
        {
            return boltAction ? FireType.Single : FireType.Auto;
        }

        protected override bool IsFiring()
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsName("Fire");
        }

        protected override bool IsAiming()
        {
            return characterBehaviour.IsAiming();
        }

        protected override int BulletsLeft()
        {
            bl_EventHandler.DispatchLocalPlayerAmmoUpdate(ammunitionCurrent);
            return ammunitionCurrent;
        }
        */
        
        public bl_Gun.BulletInstanceData GetBulletPosition() 
        {
        bulletInstanceData.Rotation = transform.parent.rotation;
        Vector3 cp = PlayerCamera.transform.position;
        Vector3 mp = attachmentManager.GetEquippedMuzzle().transform.position;
        bulletInstanceData.ProjectedHitPoint = cp + (PlayerCamera.transform.forward * 100);

        // if there's a collider between the player camera and the weapon fire point
        // that means the fire point is going through the collider
        if (Physics.Linecast(cp, mp, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers))
        {
            // since we can't shoot from the fire point (since the bullets will go through the collider)
            // the bullet will be instanced from the center of the player camera
            bulletInstanceData.Position = cp;
        }
        else
        {
            // we can shoot from the fire point
            bulletInstanceData.Position = mp;
        }

        // Now there is a situation where the bullet wont get to hit the desired destination 
        // this happens when the bullet trajectory is hampered by a collider that is near to the bullet instantiation point.
        // in these case the player may not be aiming to that collider but the bullet will hit that collider instead of where the player is aiming to
        // so to fix this we do the following:

        // detect colliders between the fire point and the direction (5 meters long) where the player is aiming to
        if (Physics.Raycast(bulletInstanceData.Position, PlayerCamera.transform.forward, out hit, 5, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers, QueryTriggerInteraction.Ignore))
        {
            // if indeed there is a collider hampering the trajectory
            // instance the bullet from the camera to make the bullet avoid the obstacle and hit the desired destination.

            bulletInstanceData.ProjectedHitPoint = hit.point;
            bulletInstanceData.Position = cp;
        }
        else // All clear, not obstacles detected
        {
            if (characterBehaviour.IsAiming())
            {
                if (WeaponType != GunType.Sniper)
                {
                    // this result in a more realist bullet shoot effect when aiming but add a sightly inaccuracy on close targets
                    bulletInstanceData.Rotation = Quaternion.LookRotation(bulletInstanceData.ProjectedHitPoint - mp);
                    if (Physics.Raycast(cp, PlayerCamera.transform.forward, out hit, 500, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers, QueryTriggerInteraction.Ignore))
                    {
                        bulletInstanceData.ProjectedHitPoint = hit.point;
                        bulletInstanceData.Rotation = Quaternion.LookRotation(bulletInstanceData.ProjectedHitPoint - mp);

#if MFPSTPV
                        if (bl_CameraViewSettings.IsThirdPerson())
                        {
                            // In third person view, we have to make sure the projected hit point is not behind the weapon fire point
                            if (bl_MathUtility.Distance(hit.point, cp) <= bl_MathUtility.Distance(mp, cp))
                            {
                                // if the projected hit point is behind the fire point, then the bullet raycast has to be fired from the 
                                // actual fire point
                                bulletInstanceData.ProjectedHitPoint = cp + (PlayerCamera.transform.forward * 100);
                                bulletInstanceData.Rotation = m_Transform.parent.rotation;
                            }
                        }
#endif
                    }

                    // this in the other hand make the bullet travel exactly where the player is aiming but the bullets are instanced from the center of the screen
                    // which if you are using some sort of custom effects in the bullets will be noticed (like shooting bullets from the eyes)
                    // but if this is what you need and you are not using custom bullet trails/effects, comment the above lines and uncomment the following one:

                    // bulletInstanceData.Position = cp;
                }
                else
                {
                    bulletInstanceData.Position = cp;
                }
            }
            else
            {
#if MFPSTPV
                // in third person we have to always get the direction from the camera
                if (bl_CameraViewSettings.IsThirdPerson())
                {
                    if (Physics.Raycast(cp, PlayerCamera.transform.forward, out hit, 500, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers, QueryTriggerInteraction.Ignore))
                    {

                        // In third person view, we have to make sure the projected hit point is not behind the weapon fire point
                        if (bl_MathUtility.Distance(hit.point, cp) > bl_MathUtility.Distance(mp, cp))
                        {
                            bulletInstanceData.ProjectedHitPoint = hit.point;
                            bulletInstanceData.Rotation = Quaternion.LookRotation(bulletInstanceData.ProjectedHitPoint - mp);
                        }
                        else
                        {
                            // if the projected hit point is behind the fire point, then the bullet raycast has to be fired from the 
                            // actual fire point
                        }
                    }
                }
#endif
            }
        }

        return bulletInstanceData;
    }
        

        #endregion
        
    }
}