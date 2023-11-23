//Copyright 2022, Infima Games. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    public class Inventory : InventoryBehaviour
    {
        #region FIELDS
        
        /// <summary>
        /// Array of all weapons. These are gotten in the order that they are parented to this object.
        /// </summary>
        private WeaponBehaviour[] weapons;
        
        /// <summary>
        /// Currently equipped WeaponBehaviour.
        /// </summary>
        private WeaponBehaviour equipped;
        /// <summary>
        /// Currently equipped index.
        /// </summary>
        private int equippedIndex = -1;

        #endregion
        
        #region METHODS
        
        public override void Init(int equippedAtStart = 0)
        {
            //Cache all weapons. Beware that weapons need to be parented to the object this component is on!
            weapons = GetComponentsInChildren<WeaponBehaviour>(true);
            
            //Disable all weapons. This makes it easier for us to only activate the one we need.
            foreach (WeaponBehaviour weapon in weapons)
                weapon.gameObject.SetActive(false);

            if (bl_GameManager.Instance != null)
            {
                Setup(bl_PlayerReferences.LocalPlayer.gunManager.GetPlayerEquip());
            }
            

            //Equip.
            Equip(equippedAtStart);
        }
        
        public override WeaponBehaviour Equip(int index)
        {
            //If we have no weapons, we can't really equip anything.
            if (weapons == null)
                return equipped;
            
            //The index needs to be within the array's bounds.
            if (index > weapons.Length - 1)
                return equipped;

            //No point in allowing equipping the already-equipped weapon.
            if (equippedIndex == index)
                return equipped;
            
            //Disable the currently equipped weapon, if we have one.
            if (equipped != null)
            {
                equipped.GetAttachmentManager().DeEquip();
                equipped.gameObject.SetActive(false);
            }

            //Update index.
            equippedIndex = index;
            //Update equipped.
            equipped = weapons[equippedIndex];
            //Activate the newly-equipped weapon.
            equipped.gameObject.SetActive(true);
            equipped.Equip();
            if (bl_GameManager.Instance != null)
            {
                bl_PlayerReferences.LocalPlayer.gunManager.SwitchByIndex(index);
            }
            return equipped;
        }
        

        #endregion

        #region Getters

        public override int GetLastIndex()
        {
            //Get last index with wrap around.
            int newIndex = equippedIndex - 1;
            if (newIndex < 0)
                newIndex = weapons.Length - 1;

            //Return.
            return newIndex;
        }

        public override int GetNextIndex()
        {
            //Get next index with wrap around.
            int newIndex = equippedIndex + 1;
            if (newIndex > weapons.Length - 1)
                newIndex = 0;

            //Return.
            return newIndex;
        }
        

        public override WeaponBehaviour GetEquipped() => equipped;
        public override int GetEquippedIndex() => equippedIndex;

        #endregion

        public void Setup(List<bl_Gun> playerEquip)
        {
            WeaponBehaviour[] weaponBehaviour = weapons;
            WeaponBehaviour[] newWeaponBehaviours = new WeaponBehaviour[4];
            for (int i = 0; i < playerEquip.Count; i++)
            {
                foreach (var weaponBehaviourToFind in weaponBehaviour)
                {
                    if (playerEquip[i].GunID == weaponBehaviourToFind.GetGunId())
                    {
                        newWeaponBehaviours[i] = weaponBehaviourToFind;
                    }
                }
            }

            weapons = newWeaponBehaviours;
        }
    }
}