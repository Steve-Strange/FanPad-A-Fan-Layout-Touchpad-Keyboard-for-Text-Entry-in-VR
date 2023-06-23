using UnityEngine;
using System.Collections;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ControllerInput : MonoBehaviour
{

    public SteamVR_Action_Vector2 moveAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("platformer", "Move");
    public SteamVR_Action_Boolean jumpAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("platformer", "Jump");

    private Interactable interactable;
    private bool clickDownLeft;
    private bool clickDownRight;
    Vector2 thumbLocationLeft;
    Vector2 thumbLocationRight;

    private void Start()
    {
        interactable = GetComponent<Interactable>();
    }

    private void Update()
    {
        thumbLocationLeft = moveAction[SteamVR_Input_Sources.LeftHand].axis;
        thumbLocationRight = moveAction[SteamVR_Input_Sources.RightHand].axis;

        clickDownLeft = jumpAction[SteamVR_Input_Sources.LeftHand].stateDown;
        clickDownRight = jumpAction[SteamVR_Input_Sources.LeftHand].stateDown;
        //print("thumbLocationLeft: " + thumbLocationLeft);
        //print("thumbLocationRight: " + thumbLocationRight);
    }
}