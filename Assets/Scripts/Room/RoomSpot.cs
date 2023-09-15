
using System;
using UnityEngine;

public class RoomSpot : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // 부모 Room 정보
    // ------------------------------------------------------------------------
    [SerializeField, DisableInInspector]
    private Room _host;

    public Room Host
    {
        get { return _host; }
        set
        {
            _host = value;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // host가 null이라면 (초기화되지 않았다면) 쓰이지 않으므로 아무 일도 일어나지 않음
        if (_host != null && other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            player.TouchRoomSpot(this);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position,0.25f);
    }
}