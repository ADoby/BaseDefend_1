using UnityEngine;
using System.Collections;

public class MyInput : vp_FPInput 
{

    protected override void InputAttack()
    {
        if (!Screen.lockCursor)
            return;
        if (InputController.GetDown("ATTACK"))
            FPPlayer.Attack.TryStart();
        else
            FPPlayer.Attack.TryStop();
    }

    protected override void InputCamera()
    {
        if (InputController.GetDown("ZOOM"))
            FPPlayer.Zoom.TryStart();
        else
            FPPlayer.Zoom.TryStop();

        // toggle 3rd person mode
        if (InputController.GetClicked("TOGGLE3RDPERSON"))
            FPPlayer.CameraToggle3rdPerson.Send();
    }

    protected override void InputCrouch()
    {
        if (InputController.GetDown("CROUCH"))
            FPPlayer.Crouch.TryStart();
        else
            FPPlayer.Crouch.TryStop();
    }

    protected override void InputInteract()
    {
        if (InputController.GetClicked("INTERACT"))
            FPPlayer.Interact.TryStart();
        else
            FPPlayer.Interact.TryStop();
    }

    protected override void InputJump()
    {
        if (InputController.GetDown("JUMP"))
            FPPlayer.Jump.TryStart();
        else
            FPPlayer.Jump.Stop();
    }

    protected override void InputMove()
    {
        FPPlayer.InputMoveVector.Set(new Vector2(InputController.GetValue("RIGHT") - InputController.GetValue("LEFT"),
            InputController.GetValue("FORWARD") - InputController.GetValue("BACK")));
    }

    protected override void InputReload()
    {
        if (InputController.GetClicked("RELOAD"))
            FPPlayer.Reload.TryStart();
    }

    protected override void InputRun()
    {
        if (InputController.GetDown("RUN"))
            FPPlayer.Run.TryStart();
        else
            FPPlayer.Run.TryStop();
    }

    protected override void InputSetWeapon()
    {
        if (InputController.GetClicked("PREVWEAPON"))
            FPPlayer.SetPrevWeapon.Try();

        if (InputController.GetClicked("NEXTWEAPON"))
            FPPlayer.SetNextWeapon.Try();

        // --- switch to weapon 1-10 by direct button press ---

        //if (vp_Input.GetButton("SetWeapon1"))	// (etc.) suggested input axes
        if (InputController.GetClicked("1")) FPPlayer.SetWeapon.TryStart(1);
        if (InputController.GetClicked("2")) FPPlayer.SetWeapon.TryStart(2);
        if (InputController.GetClicked("3")) FPPlayer.SetWeapon.TryStart(3);
        if (InputController.GetClicked("4")) FPPlayer.SetWeapon.TryStart(4);
        if (InputController.GetClicked("5")) FPPlayer.SetWeapon.TryStart(5);
        if (InputController.GetClicked("6")) FPPlayer.SetWeapon.TryStart(6);
        /*
         * 
        if (Input.GetKeyDown(KeyCode.Alpha7)) FPPlayer.SetWeapon.TryStart(7);
        if (Input.GetKeyDown(KeyCode.Alpha8)) FPPlayer.SetWeapon.TryStart(8);
        if (Input.GetKeyDown(KeyCode.Alpha9)) FPPlayer.SetWeapon.TryStart(9);
        if (Input.GetKeyDown(KeyCode.Alpha0)) FPPlayer.SetWeapon.TryStart(10);*/

        // --- unwield current weapon by direct button press ---

        if (InputController.GetClicked("CLEARWEAPON")) FPPlayer.SetWeapon.TryStart(0);
    }

    public float MouseMult = 0.01f;

    protected override Vector2 GetMouseLook()
    {
        // don't allow mouselook if we are using the mouse cursor
        if (MouseCursorBlocksMouseLook && !Screen.lockCursor)
            return Vector2.zero;

        // only recalculate mouselook once per frame or smoothing will break
        if (m_LastMouseLookFrame == Time.frameCount)
            return m_CurrentMouseLook;

        m_LastMouseLookFrame = Time.frameCount;

        // --- fetch mouse input ---

        m_MouseLookSmoothMove.x = (InputController.GetValue("LOOKRIGHT") + InputController.GetValue("LOOKLEFT")) * MouseMult * Time.timeScale;
        m_MouseLookSmoothMove.y = (InputController.GetValue("LOOKUP") + InputController.GetValue("LOOKDOWN")) * MouseMult * Time.timeScale;

        // --- mouse smoothing ---

        // make sure the defined smoothing vars are within range
        MouseLookSmoothSteps = Mathf.Clamp(MouseLookSmoothSteps, 1, 20);
        MouseLookSmoothWeight = Mathf.Clamp01(MouseLookSmoothWeight);

        // keep mousebuffer at a maximum of (MouseSmoothSteps + 1) values
        while (m_MouseLookSmoothBuffer.Count > MouseLookSmoothSteps)
            m_MouseLookSmoothBuffer.RemoveAt(0);

        // add current input to mouse input buffer
        m_MouseLookSmoothBuffer.Add(m_MouseLookSmoothMove);

        // calculate mouse smoothing
        float weight = 1;
        Vector2 average = Vector2.zero;
        float averageTotal = 0.0f;
        for (int i = m_MouseLookSmoothBuffer.Count - 1; i > 0; i--)
        {
            average += m_MouseLookSmoothBuffer[i] * weight;
            averageTotal += (1.0f * weight);
            weight *= (MouseLookSmoothWeight / Delta);
        }

        // store the averaged input value
        averageTotal = Mathf.Max(1, averageTotal);
        m_CurrentMouseLook = vp_MathUtility.NaNSafeVector2(average / averageTotal);

        // --- mouse acceleration ---

        float mouseAcceleration = 0.0f;

        float accX = Mathf.Abs(m_CurrentMouseLook.x);
        float accY = Mathf.Abs(m_CurrentMouseLook.y);

        if (MouseLookAcceleration)
        {
            mouseAcceleration = Mathf.Sqrt((accX * accX) + (accY * accY)) / Delta;
            mouseAcceleration = (mouseAcceleration <= MouseLookAccelerationThreshold) ? 0.0f : mouseAcceleration;
        }

        m_CurrentMouseLook.x *= (MouseLookSensitivity.x + mouseAcceleration);
        m_CurrentMouseLook.y *= (MouseLookSensitivity.y + mouseAcceleration);

        m_CurrentMouseLook.y = (MouseLookInvert ? m_CurrentMouseLook.y : -m_CurrentMouseLook.y);

        //m_Player.AimMovement.Send(m_CurrentMouseLook);

        return m_CurrentMouseLook;
    }

    protected override void UpdateCursorLock()
    {
        // store the current mouse position as GUI coordinates
        m_MousePos.x = Input.mousePosition.x;
        m_MousePos.y = (Screen.height - Input.mousePosition.y);

        // uncomment this line to print the current mouse position
        //Debug.Log("X: " + (int)m_MousePos.x + ", Y:" + (int)m_MousePos.y);

        // if 'ForceCursor' is active, the cursor will always be visible
        // across the whole screen and firing will be disabled
        if (InputHandler.Instance.ForceMouseLock)
        {
            Screen.lockCursor = InputHandler.Instance.WantedLockStatus;
            return;
        }

        // see if any of the mouse buttons are being held down
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            // no zones prevent firing the current weapon. hide mouse cursor
            // and lock it at the center of the screen
            Screen.lockCursor = true;

        }


    }
}
