using UnityEngine;

// Повесь на SamuraiSprite
public class PlayerAnimationBridge : MonoBehaviour
{
    PlayerController2D controller;

    void Awake()
    {
        controller = GetComponentInParent<PlayerController2D>();
    }

    public void OnAttack1Enter()  => controller?.OnAttack1Enter();
    public void OnAttack2Enter()  => controller?.OnAttack2Enter();
    public void OnAttack3Enter()  => controller?.OnAttack3Enter();
    public void EnableHitbox1()   => controller?.EnableHitbox1();
    public void EnableHitbox2()   => controller?.EnableHitbox2();
    public void EnableHitbox3()   => controller?.EnableHitbox3();
    public void OnHitboxOff()     => controller?.OnHitboxOff();
   public void OnAttackEnd()
{
    Debug.Log("Bridge: OnAttackEnd вызван");
    controller?.OnAttackEnd();
}
}
