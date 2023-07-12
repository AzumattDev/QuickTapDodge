using System.Collections;
using UnityEngine;

namespace QuickTapDodge;

public class JumpNextFrame : MonoBehaviour
{
    // This entire class is to prevent the jump from happening on the same frame as the dodge.
    // Since jumping detects if you're in a dodge, we need to wait until the next frame to jump.
    // Preventing an infinite loop.
    public static JumpNextFrame Instance = null!;

    void Awake()
    {
        Instance = this;
    }

    public void Jump(Player player)
    {
        StartCoroutine(JumpCoroutine(player));
    }

    private IEnumerator JumpCoroutine(Player player)
    {
        yield return new WaitForEndOfFrame();
        player.Jump(true);
    }

    public void CancelAndJump(Player __instance, bool preventJump = false)
    {
        // Interrupt the dodge animation
        __instance.m_zanim.SetTrigger("stop");
        if (!preventJump)
            Instance.Jump(__instance);

        // Reset the dodge variables
        __instance.m_queuedDodgeTimer = 0.0f;
        __instance.m_inDodge = false;
    }

    public Vector3 GetDodgeDir(PlayerController __instance)
    {
        Vector3 dodgeDir = __instance.m_character.m_moveDir;
        if (!((double)dodgeDir.magnitude < 0.10000000149011612)) return dodgeDir;
        dodgeDir = __instance.m_character.IsCrouching() || __instance.m_character.m_crouchToggled
            ? __instance.m_character.m_lookDir with { y = 0.0f }
            : -__instance.m_character.m_lookDir with { y = 0.0f };
        dodgeDir.Normalize();

        return dodgeDir;
    }
}