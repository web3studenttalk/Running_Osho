using UnityEngine;
using System.Collections;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private float respawnTime = 5.0f;
    private Collider itemCollider;
    private Renderer[] allRenderers;

    void Awake() { itemCollider = GetComponent<Collider>(); allRenderers = GetComponentsInChildren<Renderer>(); }

    private void OnTriggerEnter(Collider other) {
        bool isRacer = other.CompareTag("Racer") || (other.transform.parent && other.transform.parent.CompareTag("Racer"));
        if (!isRacer) return;

        ItemType type = (ItemType)Random.Range(1, 5);
        var player = other.GetComponentInParent<PlayerLaneController>();
        var prog = other.GetComponentInParent<CourseProgressor>();

        if (player) ItemManager.Instance.GiveItemToPlayer(type);
        else if (prog) ItemManager.Instance.StartCoroutine(ItemManager.Instance.ApplyEffect(prog, type));

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine() {
        if (itemCollider) itemCollider.enabled = false;
        foreach (var r in allRenderers) if (r) r.enabled = false;
        yield return new WaitForSeconds(respawnTime);
        if (itemCollider) itemCollider.enabled = true;
        foreach (var r in allRenderers) if (r) r.enabled = true;
    }
}