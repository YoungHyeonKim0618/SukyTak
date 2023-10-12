using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/*
 * 일단 커스텀 에디터로 구현하고, 커스텀 컨트롤로 나중에 치환하도록 하자.
 */
#if false

public class RoomPreview : BindableElement, INotifyValueChanged<List<VisualObject>>
{
    
    // ------------------------------------------------------------------------
    // UXML Factories
    // ------------------------------------------------------------------------
    public new class UxmlFactory : UxmlFactory<RoomPreview,UxmlTraits>{ }
    
    public new class UxmlTraits : BindableElement.UxmlTraits
    {
        private UxmlIntAttributeDescription m_MaxVisualObjects = new UxmlIntAttributeDescription
            { name = "max-visual-objects" };

            
        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            ((RoomPreview)ve).m_MaxVisualObjects = m_MaxVisualObjects.GetValueFromBag(bag, cc);
        }
    }
    
    

    // ------------------------------------------------------------------------
    // Members
    // ------------------------------------------------------------------------
    private int m_MaxVisualObjects;
    private List<VisualObject> m_VisualObjects;
        
    
    
    // ------------------------------------------------------------------------
    // Properties
    // ------------------------------------------------------------------------
    public int maxVisualObjects
    {
        get => m_MaxVisualObjects;
        set
        {
            m_MaxVisualObjects = value;
            // 필요하다면 수정 시 업데이트
        }
    }

    public List<VisualObject> visualObjects
    {
        get => m_VisualObjects;
        set
        {
            m_VisualObjects = value;
            UpdateVisualObjectGraphics();
        }
    }
    
    // ------------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------------
    public RoomPreview()
    {
        style.alignItems = Align.Center;
    }
    
    
    // ------------------------------------------------------------------------
    // 값 변경 감지 & 업데이트
    // ------------------------------------------------------------------------
    
    public void SetValueWithoutNotify(List<VisualObject> newValue)
    {
        throw new System.NotImplementedException();
    }
    
    // ------------------------------------------------------------------------
    // 그리기
    // ------------------------------------------------------------------------

    private void UpdateVisualObjectGraphics()
    {
        Debug.Log("Visual object graphics updated!");
    }


    public List<VisualObject> value { get; set; }
}

#endif