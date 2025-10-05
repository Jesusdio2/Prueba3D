using UnityEngine;

public class HotbarSelector : MonoBehaviour {
    public RectTransform selector;
    public Vector3[] slotPositions;

    void Update() {
        for (int i = 0; i < slotPositions.Length; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                MoveSelector(i);
            }
        }
    }

    void MoveSelector(int index) {
        selector.localPosition = slotPositions[index];
        Debug.Log("Ranura " + (index + 1) + " activada");
    }
}
