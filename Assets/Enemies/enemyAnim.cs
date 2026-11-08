using UnityEngine;

public class enemyAnim : MonoBehaviour
{
    private Animator Animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Animator != null)
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                Animator.SetTrigger("TrAntWalk");
                Animator.SetTrigger("TrBeeFly");
                Animator.SetTrigger("TrStickRun");
                Animator.SetTrigger("TrBrRun");
                Animator.SetTrigger("TrShRun");
                Animator.SetTrigger("TrSRun");
                Animator.SetTrigger("TrShRun");
                Animator.SetTrigger("TrPRun");
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                Animator.SetTrigger("TrAntAttk");
                Animator.SetTrigger("TrBeeAttk");
                Animator.SetTrigger("TrStickAttk");
                Animator.SetTrigger("TrBrAttk");
                Animator.SetTrigger("TrShCh");
                Animator.SetTrigger("TrSHeal");
                Animator.SetTrigger("TrShSh");
                Animator.SetTrigger("TrPCom");
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                Animator.SetTrigger("TrAntHurt");
                Animator.SetTrigger("TrBeeHurt");
                Animator.SetTrigger("TrStickHurt");
                Animator.SetTrigger("TrBrHurt");
                Animator.SetTrigger("TrShHurt");
                Animator.SetTrigger("TrSHurt");
                Animator.SetTrigger("TrShHurt");
                Animator.SetTrigger("TrPHurt");
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                Animator.SetTrigger("TrAntDie");
                Animator.SetTrigger("TrBeeDie");
                Animator.SetTrigger("TrStickDie");
                Animator.SetTrigger("TrBrDie");
                Animator.SetTrigger("TrShDie");
                Animator.SetTrigger("TrSDie");
                Animator.SetTrigger("TrShDie");
                Animator.SetTrigger("TrPDie");
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Animator.SetTrigger("TrBrIdle");
                Animator.SetTrigger("TrShIdle");
                Animator.SetTrigger("TrSIdle");
                Animator.SetTrigger("TrShIdle");
                Animator.SetTrigger("TrPIdle");
                Animator.SetTrigger("TrCivIdle");
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                Animator.SetTrigger("TrBrSm");
                Animator.SetTrigger("TrPHelp");
                Animator.SetTrigger("TrCivRev");
            }

        }
    }
}
