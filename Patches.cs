using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace QuickTapDodge;

[HarmonyPatch(typeof(PlayerController), nameof(PlayerController.FixedUpdate))]
static class PlayerControllerFixedUpdatePatch
{
    // Fields to keep track of the dodge key state
    public static float DodgePressTimer = 0;
    public static float DodgeReleaseTimer = 0;
    public static bool DodgeTapPressed = false;
    public static bool CanDodge = true;


    private static float _forwardPressTimer,
        _forwardReleaseTimer,
        _backwardPressTimer,
        _backwardReleaseTimer,
        _rightPressTimer,
        _rightReleaseTimer,
        _leftPressTimer,
        _leftReleaseTimer;

    private static bool _forwardTapPressed, _backwardTapPressed, _rightTapPressed, _leftTapPressed;

    public static void Postfix(PlayerController __instance)
    {
        bool dodgePressed = ZInput.GetButton("Forward") || ZInput.GetButton("JoyLStickUp");
        bool forwardDoubleTap = PlayerController.DetectTap(dodgePressed, Time.fixedDeltaTime, QuickTapDodgePlugin.MinPressTime.Value, false, ref _forwardPressTimer, ref _forwardReleaseTimer, ref _forwardTapPressed);
        dodgePressed = ZInput.GetButton("Backward") || ZInput.GetButton("JoyLStickDown");
        bool backwardDoubleTap = PlayerController.DetectTap(dodgePressed, Time.fixedDeltaTime, QuickTapDodgePlugin.MinPressTime.Value, false, ref _backwardPressTimer, ref _backwardReleaseTimer, ref _backwardTapPressed);
        dodgePressed = ZInput.GetButton("Right") || ZInput.GetButton("JoyLStickRight");
        bool rightDoubleTap = PlayerController.DetectTap(dodgePressed, Time.fixedDeltaTime, QuickTapDodgePlugin.MinPressTime.Value, false, ref _rightPressTimer, ref _rightReleaseTimer, ref _rightTapPressed);
        dodgePressed = ZInput.GetButton("Left") || ZInput.GetButton("JoyLStickLeft");
        bool leftDoubleTap = PlayerController.DetectTap(dodgePressed, Time.fixedDeltaTime, QuickTapDodgePlugin.MinPressTime.Value, false, ref _leftPressTimer, ref _leftReleaseTimer, ref _leftTapPressed);

        // We need to manually replicate this check from the original FixedUpdate
        bool flag1 = __instance.InInventoryEtc();
        bool flag2 = Hud.IsPieceSelectionVisible();

        // If player is crouched and hits jump button, cancel dodge and jump if configured
        if (__instance.m_character.IsCrouching() && ZInput.GetButton("Jump") && !flag2 && !flag1 && QuickTapDodgePlugin.PreventDodgeWhileCrouched.Value == QuickTapDodgePlugin.Toggle.On)
        {
            JumpNextFrame.Instance.CancelAndJump(__instance.m_character, QuickTapDodgePlugin.PreventJumpWhileCrouched.Value == QuickTapDodgePlugin.Toggle.On);
        }

        // Check if the dodge button has been pressed and we are allowed to dodge
        if ((forwardDoubleTap || backwardDoubleTap || rightDoubleTap || leftDoubleTap) && CanDodge)
        {
            if (!__instance.m_character.InAttack() && !__instance.m_character.IsBlocking())
            {
                CanDodge = false;
                Vector3 dodgeDir = JumpNextFrame.Instance.GetDodgeDir(__instance);
                __instance.m_character.Dodge(dodgeDir);
            }
        }

        // Reset the dodge availability flag when dodge button is not pressed
        if (!dodgePressed)
        {
            CanDodge = true;
        }
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.InDodge))]
static class PlayerInDodgePatch
{
    static void Prefix(Player __instance, ref bool __result)
    {
        if (!__instance.m_inDodge) return;
        // If the jump button is pressed, cancel the dodge
        if (!ZInput.GetButton("Jump") && !ZInput.GetButton("JoyJump")) return;

        if (QuickTapDodgePlugin.PreventDodgeWhileCrouched.Value != QuickTapDodgePlugin.Toggle.On) return;
        JumpNextFrame.Instance.CancelAndJump(__instance, QuickTapDodgePlugin.PreventJumpWhileCrouched.Value == QuickTapDodgePlugin.Toggle.On);
        __result = false;
    }
}

[HarmonyPatch(typeof(Hud), nameof(Hud.Awake))]
static class HudAwakePatch
{
    static void Postfix(Hud __instance)
    {
        /* Attach this to something that already exists and is called once
           so we can use the method but not create our own object */
        __instance.m_rootObject.AddComponent<JumpNextFrame>();
    }
}