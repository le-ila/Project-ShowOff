using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HookGrabable : MonoBehaviour {
    //When adding a hook to the level the forward direction of the object must be set appropriatly(facing towards where the player
    //will dismount when going up)

    public System.Action OnMastReached;
    public System.Action OnShipReached;

    [Header("Translation")]
    [SerializeField] [Tooltip("Target for moving up must be placed first and then the target for moving down")] Transform[] targetPosition;
    [SerializeField] [Tooltip("Target for moving up must be placed first and then the target for moving down")] Transform[] teleportPosition;
    [SerializeField] Vector3 distanceToPlatform;
    [SerializeField] bool isMovingUp;
    [SerializeField] [Tooltip("Speed in meters / second")] float speed;
    [SerializeField] float coroutineUpdateTime;

    Vector3[] forwardDirection = new Vector3[2];
    XRSimpleInteractable interactable;
    Transform playerTransform = null;
    ContinuousMoveProviderBase moveScript;
    bool isCoroutineRunning = false;
    bool wasReleased = false;

    ContinuousMoveProviderBase playerMovement;
    ContinuousTurnProviderBase playerTurning;

    int currentTargetIndex { get { return (isMovingUp) ? 0 : 1; } }

    #region Unity Events
    private void Awake() {
        interactable = GetComponent<XRSimpleInteractable>();

        interactable.selectEntered.AddListener(OnGrab);
        interactable.selectExited.AddListener(OnRelease);

        interactable.hoverExited.AddListener(OnHoverExit);

        forwardDirection[0] = transform.forward;
        forwardDirection[1] = -transform.forward;
    }

    private void Start() {
        playerMovement = SceneManager.instance.playerGameObject.GetComponent<ContinuousMoveProviderBase>();
        playerTurning = SceneManager.instance.playerGameObject.GetComponent<ContinuousTurnProviderBase>();
    }
    #endregion

    #region Grab
    void OnGrab(SelectEnterEventArgs pArgs) {
        playerTransform = SceneManager.instance.playerGameObject.transform;
        moveScript = playerTransform.GetComponent<ContinuousMoveProviderBase>();
        moveScript.useGravity = false;
        wasReleased = false;

        playerMovement.enabled = false;
        playerTurning.enabled = false;

        StartCoroutine(TranslateCoroutine());
    }

    void OnRelease(SelectExitEventArgs pArgs) {
        if (wasReleased)
            return;

        if (moveScript != null)
            moveScript.useGravity = true;

        playerTransform = null;
        wasReleased = true;

        playerMovement.enabled = true;
        playerTurning.enabled = true;

        if (!isCoroutineRunning)
            return;

        if (isMovingUp)
            isMovingUp = !isMovingUp;
    }

    void OnHoverExit(HoverExitEventArgs pArgs) {
        //OnRelease(null);
    }

    IEnumerator TranslateCoroutine(float startDelay = 0) {
        float epsilon = 0.025f;

        if (startDelay != 0)
            yield return new WaitForSeconds(0);

        isCoroutineRunning = true;

        float distanceToTarget = (targetPosition[currentTargetIndex].position - transform.position).magnitude;

        while (distanceToTarget >= epsilon) {

            Vector3 directionToTarget = (targetPosition[currentTargetIndex].position - transform.position).normalized;
            distanceToTarget = (targetPosition[currentTargetIndex].position - transform.position).magnitude;
            float distanceToTravel = speed * Time.deltaTime;
            distanceToTravel = Mathf.Clamp(distanceToTravel, 0, distanceToTarget);

            transform.Translate(distanceToTravel * directionToTarget, Space.World);

            if (playerTransform != null)
                playerTransform.Translate(distanceToTravel * directionToTarget, Space.World);

            yield return new WaitForSeconds(coroutineUpdateTime);
        }

        if (playerTransform != null) {
            playerTransform.transform.position = teleportPosition[currentTargetIndex].position;
        }

        isCoroutineRunning = false;

        if (isMovingUp)
            OnMastReached?.Invoke();
        else
            OnShipReached?.Invoke();

        isMovingUp = !isMovingUp;

        if (playerTransform != null && isMovingUp)
            OnRelease(null);
    }
    #endregion
}
