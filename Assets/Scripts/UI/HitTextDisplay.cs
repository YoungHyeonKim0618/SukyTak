
using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

/*
 * 적이나 플레이어가 피해를 입을 때 그 수치를 표기하는 클래스.
 * 수치가 표기될 캔버스 자신이 이 스크립트를 가진다.
 */
public class HitTextDisplay : MonoBehaviour
{
    [SerializeField] private FloatVector3ChannelSO _channel;
    [SerializeField] private TextMeshProUGUI p_DamageTmp;

    private void Start()
    {
        _channel.OnEventRaised += DisplayHitText;
    }

    public void DisplayHitText(float dmg, Vector3 vec3)
    {
        Vector3 worldPos = new Vector3(vec3.x, vec3.y, 0);
        bool crit = vec3.z > 0;
        
        var tmp = Instantiate(p_DamageTmp, worldPos, Quaternion.identity, transform);
        if (dmg >= 0)
        {
            tmp.text = crit ? $"<color=white>크리티컬!</color>\n" : "";
            tmp.text += $"- {dmg:0.#}";
        } 
        else tmp.text = $"빗나감!";
        
        tmp.rectTransform.DOMoveY(worldPos.y + 1, 0.5f);
        Destroy(tmp.gameObject,1f);
    }
}