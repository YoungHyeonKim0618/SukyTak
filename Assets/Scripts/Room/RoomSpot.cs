
using System;
using UnityEngine;

public class RoomSpot : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // 부모 Room 정보
    // ------------------------------------------------------------------------
    [SerializeField, DisableInInspector]
    private Room _host;

    [SerializeField] private bool _isStair;
    public bool IsStair => _isStair;

    public Room Host
    {
        get { return _host; }
        set
        {
            _host = value;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position,0.25f);
    }
}