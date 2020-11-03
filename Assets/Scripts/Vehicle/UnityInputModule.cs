using UnityEngine;
using UnityEngine.InputSystem;
using RVP;

//Class for setting the input with the input manager

[RequireComponent(typeof(VehicleParent))]
[DisallowMultipleComponent]
public class UnityInputModule : MonoBehaviour
{

    private static UnityInputModule _inputModule;
    public static UnityInputModule instance
    {
        get
        {
            if (_inputModule == null)
            {
                _inputModule = FindObjectOfType<UnityInputModule>();
            }
            return _inputModule;
        }
    }

    [HideInInspector]
    public ControlActions controls;

    private VehicleParent vp;

    private void Awake()
    {
        controls = new ControlActions();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        vp = GetComponent<VehicleParent>();
    }

    void FixedUpdate()
    {
        //Get constant inputs
        vp.SetAccel(controls.Player.Accelerate.ReadValue<float>());
        vp.SetBrake(controls.Player.Break.ReadValue<float>());
        vp.SetSteer(controls.Player.Steer.ReadValue<float>());
        vp.SetEbrake(controls.Player.Ebreak.ReadValue<float>());
        vp.SetBoost(controls.Player.Boost.ReadValue<float>() > 0);
        vp.SetPitch(controls.Player.Pitch.ReadValue<float>());
        vp.SetYaw(controls.Player.Yaw.ReadValue<float>());
        vp.SetRoll(controls.Player.Roll.ReadValue<float>());
        vp.SetRoll(controls.Player.Roll.ReadValue<float>());
    }
}