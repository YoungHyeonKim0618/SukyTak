
using System;
using UnityEngine;

public class RoomEnterDetector : MonoBehaviour
{
    public Room Host;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            player.Detected(this);
        }
    }

    [SerializeField] private BoxCollider2D _collider;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if(_collider != null)
            Gizmos.DrawWireCube(transform.position,_collider.size);
    }
}