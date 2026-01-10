using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLaneController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float laneLimit = 2.5f; // レーン制限を追加
    private CourseProgressor progressor;

    void Awake() {
        progressor = GetComponent<CourseProgressor>();
    }

    void Update()
    {
        if (progressor == null || Keyboard.current == null) return;

        float input = 0f;
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) input = -1f;
        else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) input = 1f;

        if (input != 0f)
        {
            progressor.targetHorizontalOffset += input * moveSpeed * Time.deltaTime;
            // レーンをはみ出さないように制限
            progressor.targetHorizontalOffset = Mathf.Clamp(progressor.targetHorizontalOffset, -laneLimit, laneLimit);
        }
    }
}