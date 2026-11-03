using UnityEngine;

// Turns an Animator off once its current clip has finished. An enabled Animator re-writes its properties
// every frame even when nothing changes, which makes UI canvases rebuild every frame. Anything that plays
// or changes a sleeping animator from code needs to call AnimatorSleeper.WakeUp(animator) first.
[RequireComponent(typeof(Animator))]
public class AnimatorSleeper : MonoBehaviour {

    // these are serialized so that copies made with Instantiate (e.g. the results screen's copy of the
    // board) know the animator they were copied from was asleep
    [SerializeField, HideInInspector] private bool isAsleep;
    [SerializeField, HideInInspector] private bool originalKeepStateOnDisable;
    [SerializeField, HideInInspector] private bool originalWriteDefaultsOnDisable;

    private Animator animator;
    private int finishedStateHash;
    private int finishedFrameCount;

    // how many frames a finished state has to sit still before we sleep - gives exit-time transitions
    // a chance to fire first
    private const int framesBeforeSleep = 2;

    //--------------------------------------------------------------------------------------------------------
    public static void Attach(Animator animator) {
        if (animator.GetComponent<AnimatorSleeper>() == null) {
            animator.gameObject.AddComponent<AnimatorSleeper>();
        }
    }

    //--------------------------------------------------------------------------------------------------------
    public static void WakeUp(Animator animator) {
        AnimatorSleeper sleeper = animator.GetComponent<AnimatorSleeper>();
        if (sleeper != null) {
            sleeper.WakeUp(keepState: true);
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void Awake() {
        animator = GetComponent<Animator>();
    }

    //--------------------------------------------------------------------------------------------------------
    private void OnEnable() {
        // the object was just reactivated - restart the animator the same way Unity normally would
        if (isAsleep) {
            WakeUp(keepState: false);
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void LateUpdate() {
        if (isAsleep) {
            return;
        }

        if (!animator.enabled
            || animator.runtimeAnimatorController == null
            || animator.layerCount != 1
            || animator.IsInTransition(0)) {

            finishedFrameCount = 0;
            return;
        }

        // a state with no clip (like the blocks' "Empty" state) has nothing left to play
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        bool hasClip = animator.GetCurrentAnimatorClipInfoCount(0) > 0;
        if (hasClip && (state.loop || state.normalizedTime < 1f)) {
            finishedFrameCount = 0;
            return;
        }

        if (state.fullPathHash != finishedStateHash) {
            finishedStateHash = state.fullPathHash;
            finishedFrameCount = 0;
        }

        finishedFrameCount++;
        if (finishedFrameCount >= framesBeforeSleep) {
            Sleep();
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void Sleep() {
        originalKeepStateOnDisable = animator.keepAnimatorStateOnDisable;
        originalWriteDefaultsOnDisable = animator.writeDefaultValuesOnDisable;

        // keep the current state and pose so waking up carries on from exactly where we left off
        animator.keepAnimatorStateOnDisable = true;
        animator.writeDefaultValuesOnDisable = false;
        animator.enabled = false;
        isAsleep = true;
    }

    //--------------------------------------------------------------------------------------------------------
    private void WakeUp(bool keepState) {
        finishedFrameCount = 0;

        if (!isAsleep) {
            return;
        }

        isAsleep = false;
        animator.enabled = true;
        animator.keepAnimatorStateOnDisable = originalKeepStateOnDisable;
        animator.writeDefaultValuesOnDisable = originalWriteDefaultsOnDisable;

        if (!keepState) {
            // start over from the default state, like a freshly activated animator
            animator.Rebind();
        }
    }
}
