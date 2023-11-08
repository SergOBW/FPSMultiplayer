using UnityEngine;
using Photon.Pun;

public class bl_PlayerSideLeaning : bl_MonoBehaviour
{
    public enum LeaningState
    {
        Idle,
        LeaningLeft,
        LeaningRight
    }

    private LeaningState _state = LeaningState.Idle;
    [SerializeField] private float leaningAngle = 15f;
    [SerializeField] private float lateralDisplacement = 0.3f;
    [SerializeField] private float pitchOffset = 15f;
    [SerializeField] private float smoothness = 3;
    [SerializeField] private AnimationCurve easingTransition = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public LeaningState State
    {
        get { return _state; }
        set
        {
            if (_state != value)
            {
                OnStateChange(value);
            }
            _state = value;
        }
    }

    private Transform spinePivotTransform;
    private Transform fpSpineTransform;
    private Vector3 desiredRotation, desiredFPRotation;
    private Vector3 desiredFpPosition;
    private Vector3 defaultRotation, originFpPosition;
    private Quaternion originSpineRotation, originFpRotation;
    private Quaternion lerpSpineRotation;
    private bool isLocal = false;
    private float elapsedTime = 0.0f;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        isLocal = photonView.IsMine;
        if (PlayerReferences.playerIK.GetComponent<bl_LeaningAnimator>() == null)
        {
            var script = PlayerReferences.playerIK.gameObject.AddComponent<bl_LeaningAnimator>();
            script.onAnimatorIK += OnAnimatorIK;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        SetupPlayer();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();

        if (isMine)
        {
#if MFPSM
            bl_TouchHelper.onMobileButton += OnMobileButton;
#endif
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (isMine)
        {
#if MFPSM
            bl_TouchHelper.onMobileButton -= OnMobileButton;
#endif
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetupPlayer()
    {
        // set up the third person player
        if (PlayerReferences.PlayerAnimator != null && PlayerReferences.PlayerAnimator.gameObject.activeInHierarchy)
        {
            var spine = PlayerReferences.PlayerAnimator.GetBoneTransform(HumanBodyBones.Spine);
            if (spine == null)
            {
                Debug.LogWarning("Couldn't find the spine in this player model.");
                return;
            }

            spinePivotTransform = spine.transform;
            defaultRotation = spinePivotTransform.localEulerAngles;
            originSpineRotation = spinePivotTransform.localRotation;
        }

        // set up the first person player
        if (isLocal)
        {
            var spinePivot = new GameObject("Spine Pivot");
            fpSpineTransform = spinePivot.transform;
            spinePivot.transform.SetParent(PlayerReferences.PlayerCameraTransform.parent, false);
#if UNITY_2021_3_OR_NEWER
            spinePivot.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
#else
            spinePivot.transform.localPosition = Vector3.zero;
            spinePivot.transform.localRotation = Quaternion.identity;
#endif
            //PlayerReferences.PlayerCameraTransform.parent = spinePivot.transform;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        InputControl();
        MovementControl();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layerIndex"></param>
    private void OnAnimatorIK(int layerIndex)
    {
        if (_state == LeaningState.Idle && elapsedTime >= 1) return;

        PlayerReferences.PlayerAnimator.SetBoneLocalRotation(HumanBodyBones.Spine, lerpSpineRotation);
    }

    /// <summary>
    /// 
    /// </summary>
    void InputControl()
    {
        if (!isLocal) return;
        if (!bl_RoomMenu.Instance.isCursorLocked || bl_GameInput.InputFocus != MFPSInputFocus.Player || bl_GameData.Instance.isChating) return;
        if (bl_UtilityHelper.isMobile) return;

        if (bl_Input.isButton("Leaning Left"))
        {
            State = LeaningState.LeaningLeft;
        }
        else if (bl_Input.isButton("Leaning Right"))
        {
            State = LeaningState.LeaningRight;
        }
        else
        {
            State = LeaningState.Idle;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void MovementControl()
    {
        if (elapsedTime > 1) return;

        float t = easingTransition.Evaluate(elapsedTime);
        lerpSpineRotation = Quaternion.Slerp(originSpineRotation, Quaternion.Euler(desiredRotation), t);

        if (isLocal && fpSpineTransform != null)
        {
            fpSpineTransform.localRotation = Quaternion.Slerp(originFpRotation, Quaternion.Euler(desiredFPRotation), t);
            fpSpineTransform.localPosition = Vector3.Lerp(originFpPosition, desiredFpPosition, t);
        }

        elapsedTime += Time.deltaTime * smoothness;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newState"></param>
    void OnStateChange(LeaningState newState)
    {
        elapsedTime = 0;
        CalculatePositions(newState);

        if (!isLocal) return;

        photonView.RPC(nameof(RpcPlayerLeaning), RpcTarget.Others, newState);
    }

    /// <summary>
    /// 
    /// </summary>
    private void CalculatePositions(LeaningState currentState)
    {
        originSpineRotation = lerpSpineRotation;
        if (fpSpineTransform != null)
        {
            originFpRotation = fpSpineTransform.localRotation;
            originFpPosition = fpSpineTransform.localPosition;
        }

        if (currentState == LeaningState.LeaningRight)
        {
            desiredRotation = defaultRotation;
            desiredRotation.y -= leaningAngle;
            desiredRotation.z -= pitchOffset;
            desiredFPRotation.z = -leaningAngle;
            desiredFpPosition.x = lateralDisplacement;
        }
        else if (currentState == LeaningState.LeaningLeft)
        {
            desiredRotation = defaultRotation;
            desiredRotation.y += leaningAngle;
            desiredRotation.z += pitchOffset;
            desiredFPRotation.z = leaningAngle;
            desiredFpPosition.x = -lateralDisplacement;
        }
        else
        {
            desiredRotation = defaultRotation;
            desiredFPRotation.z = 0;
            desiredFpPosition.x = 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newState"></param>
    [PunRPC]
    void RpcPlayerLeaning(LeaningState newState)
    {
        _state = newState;
        elapsedTime = 0;
        CalculatePositions(newState);
    }

#if MFPSM
    void OnMobileButton(MFPS.Mobile.FPSMobileButton button)
    {
        if (button == MFPS.Mobile.FPSMobileButton.LeaningLeft)
        {
            if (_state == LeaningState.LeaningLeft) State = LeaningState.Idle;
            else State = LeaningState.LeaningLeft;
        }
        else if (button == MFPS.Mobile.FPSMobileButton.LeaningRight)
        {
            if (_state == LeaningState.LeaningRight) State = LeaningState.Idle;
            else State = LeaningState.LeaningRight;
        }
    }
#endif

    private bl_PlayerReferences _playerReferences = null;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null)
            {
                _playerReferences = GetComponent<bl_PlayerReferences>();
            }
            return _playerReferences;
        }
    }

}