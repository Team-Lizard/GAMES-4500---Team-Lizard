using UnityEngine;

public enum Ability
{
    RUN,
    JUMP,
    DOUBLE_JUMP,
    WALL_JUMP,
    WALL_RUN,
    SLIDE
}

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

    public void ChangeAbilityStamina(Ability ability, float value)
    {
        switch (ability)
        {
            case Ability.RUN:
                StaminaRun.SetStaminaCost(value);
                break;
            case Ability.JUMP:
                StaminaJump.SetStaminaCost(value);
                break;
            case Ability.DOUBLE_JUMP:
                StaminaDoubleJump.SetStaminaCost(value);
                break;
            case Ability.WALL_JUMP:
                StaminaWallJump.SetStaminaCost(value);
                break;
            case Ability.WALL_RUN:
                StaminaWallRun.SetStaminaCost(value);
                break;
            case Ability.SLIDE:
                StaminaSlide.SetStaminaCost(value);
                break;
        }
    }

    public bool CanUseAbility(Ability ability)
    {
        switch (ability)
        {
            case Ability.RUN:
                return Stamina.Instance().TrySpendStamina(StaminaRun.GetCost());
            case Ability.JUMP:
                return Stamina.Instance().TrySpendStamina(StaminaJump.GetCost());
            case Ability.DOUBLE_JUMP:
                return Stamina.Instance().TrySpendStamina(StaminaDoubleJump.GetCost());
            case Ability.WALL_JUMP:
                return Stamina.Instance().TrySpendStamina(StaminaWallJump.GetCost());
            case Ability.WALL_RUN:
                return Stamina.Instance().TrySpendStamina(StaminaWallRun.GetCost());
            case Ability.SLIDE:
                return Stamina.Instance().TrySpendStamina(StaminaSlide.GetCost());

            default:
                return false;
        }
    }
}
