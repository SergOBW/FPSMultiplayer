using UnityEngine;
using System;

namespace MFPS.Addon.Customizer
{
    [Serializable]
    public class bl_AttachmentsPositions
    {

        [Header("Root Attachments")]
        public GameObject BarrelRoot;
        public GameObject OpticsRoot;
        public GameObject FeederRoot;
        public GameObject CylinderRoot;
        public GameObject LaserRoot;
        [Header("Positions References")]
        public Transform BarrelPosition;
        public Transform OpticPosition;
        public Transform FeederPosition;
        public Transform CylinderPosition;
        public Transform LaserPosition;
        public Transform ModelParent;

        [HideInInspector] public Vector3[] defaultPositions = new Vector3[5] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};

        public void Init()
        {
            defaultPositions = new Vector3[5] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero};
            if (BarrelRoot != null)
            {
                defaultPositions[0] = BarrelRoot.transform.localPosition;
            }
            if (OpticsRoot != null)
            {
                defaultPositions[1] = OpticsRoot.transform.localPosition;
            }
            if (FeederRoot != null)
            {
                defaultPositions[2] = FeederRoot.transform.localPosition;
            }
            if (CylinderRoot != null)
            {
                defaultPositions[3] = CylinderRoot.transform.localPosition;
            }
            if (LaserRoot != null)
            {
                defaultPositions[4] = LaserPosition.transform.localPosition;
            }
        }
    }
}