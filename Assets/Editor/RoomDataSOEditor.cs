
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(RoomDataSO))]
public class RoomDataSOEditor : Editor
{
    // ------------------------------------------------------------------------
    // Members
    // ------------------------------------------------------------------------
    private RoomDataSO _data;

    private float _width = 350;
    private float _height = 280;
    
    // 바로 위의 Layout과의 거리
    private float yOffset = 20;

    private SerializedProperty _idProperty;
    private SerializedProperty _spriteProperty;
    private SerializedProperty _visualObjectsProperty;
    private void OnEnable()
    {
        _data = target as RoomDataSO;

        _idProperty = serializedObject.FindProperty("ID");
        _spriteProperty = serializedObject.FindProperty("Background");
        _visualObjectsProperty = serializedObject.FindProperty("VisualObjects");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw 순서를 원하는 대로 설정한다.
        EditorGUILayout.PropertyField(_idProperty);
        EditorGUILayout.PropertyField(_spriteProperty);
        
        DrawVisualObjects();

        EditorGUILayout.PropertyField(_visualObjectsProperty);

        // 수정사항 적용
        serializedObject.ApplyModifiedProperties();
    }

    /*
     * 인스펙터 상에서 가로 250, 세로 200 크기의 Preview 생성.
     */
    private void DrawVisualObjects()
    {
        if (_data != null)
        {
            // 가운데 정렬
            float inspectorWidth = EditorGUIUtility.currentViewWidth;
            
            Rect lastRect = GUILayoutUtility.GetLastRect();
            float currentHeight = lastRect.y + lastRect.height + yOffset;
            float previewLeftX = 0.5f * (inspectorWidth - _width);
            Vector2 previewCenter = new Vector2(previewLeftX + 0.5f * _width, currentHeight + 0.5f * _height);
            
            Rect previewerRect = new Rect(previewLeftX, currentHeight, _width, _height);
            if (_data.Background != null)
            {
                GUI.DrawTexture(previewerRect,_data.Background.texture);
            }
            
            foreach (var vo in _data.VisualObjects)
            {
                if(vo.interactableSo != null)
                {
                    /*
                     * 위치를 비율에 맞게 조절해야 한다.
                     * 현재 방의 캔버스 월드 사이즈가 (5,4)라고 하면, 가로는 width/5, 세로는 height/4 를 곱해야 함.
                     */
                    Vector2 roomSize = new Vector2(GameConstantsSO.Instance.RoomWidth,
                        GameConstantsSO.Instance.RoomHeight);
                    
                    Vector2 pos = previewCenter + new Vector2(vo.position.x * _width / roomSize.x,
                        vo.position.y * _height / roomSize.y) ;
                    Sprite sprite = vo.interactableSo.Sprite;

                    /*
                     * 크기도 역시 비율에 맞게 조절해야 함.
                     * 여기서의 텍스쳐 크기 : 텍스쳐 크기 / ppu * 방 크기 / preview 크기
                     * pos가 그려지는 VO의 좌상단 기준이므로 offset을 빼준다!
                     */
                    float voWidth = sprite.texture.width / sprite.pixelsPerUnit * _width / roomSize.x * vo.scale.x;
                    float voHeight = sprite.texture.height / sprite.pixelsPerUnit * _height / roomSize.y * vo.scale.y;
                    GUI.DrawTexture(new Rect(pos.x - voWidth * 0.5f, pos.y - voHeight * 0.5f, voWidth, voHeight),
                        sprite.texture);
                }
            }
        }
        GUILayout.Space(_height + yOffset);
    }

}