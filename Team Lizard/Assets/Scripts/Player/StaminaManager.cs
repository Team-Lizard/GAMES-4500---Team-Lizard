using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    [Header("Stamina")]
        [SerializeField]
        [Tooltip("How much stamina should be spent to run.")]
        public StaminaSetting StaminaRun;

        [Tooltip("How much stamina should be spent to jump regularly.")]
        public StaminaSetting StaminaJump;

        [SerializeField]
        [Tooltip("How much stamina should be spent to double jump.")]
        public StaminaSetting StaminaDoubleJump;

        [SerializeField]
        [Tooltip("How much stamina should be spent to jump off of a wall.")]
        public StaminaSetting StaminaWallJump;

        [SerializeField]
        [Tooltip("How much stamina should be spent to wall run.")]
        public StaminaSetting StaminaWallRun;

        [SerializeField]
        [Tooltip("How much stamina should be spent to slide.")]
        public StaminaSetting StaminaSlide;

    public void ActivateAbility(Ability ability)
    {
        switch (ability)
        {
            case Ability.RUN:
                StaminaRun.Enable();
                break;
            case Ability.JUMP:
                StaminaJump.Enable();
                break;
            case Ability.DOUBLE_JUMP:
                StaminaDoubleJump.Enable();
                break;
            case Ability.WALL_JUMP:
                StaminaWallJump.Enable();
                break;
            case Ability.WALL_RUN:
                StaminaWallRun.Enable();
                break;
            case Ability.SLIDE:
                StaminaSlide.Enable();
                break;
        }
    }
}
