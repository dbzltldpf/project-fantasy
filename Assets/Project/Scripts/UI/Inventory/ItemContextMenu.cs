using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectFantasy.UI
{
    // 우클릭 메뉴 (장착/해제/사용/버리기), 바깥 클릭용 전체 화면 버튼으로 닫힘
    [DisallowMultipleComponent]
    public sealed class ItemContextMenu : MonoBehaviour
    {
        public enum MenuAction
        {
            Equip,
            Unequip,
            Use,
            Drop
        }

        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform panel;
        [Tooltip("메뉴 바깥을 덮는 투명 버튼 (누르면 닫힘)")]
        [SerializeField] private Button outsideBlocker;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button unequipButton;
        [SerializeField] private Button useButton;
        [SerializeField] private Button dropButton;

        public bool IsOpen => root.activeSelf;

        public event Action<MenuAction> ActionChosen;

        private void Awake()
        {
            outsideBlocker.onClick.AddListener(Hide);
            equipButton.onClick.AddListener(() => Choose(MenuAction.Equip));
            unequipButton.onClick.AddListener(() => Choose(MenuAction.Unequip));
            useButton.onClick.AddListener(() => Choose(MenuAction.Use));
            dropButton.onClick.AddListener(() => Choose(MenuAction.Drop));
            Hide();
        }

        public void Show(Vector2 screenPosition, bool canEquip, bool canUnequip, bool canUse)
        {
            equipButton.gameObject.SetActive(canEquip);
            unequipButton.gameObject.SetActive(canUnequip);
            useButton.gameObject.SetActive(canUse);

            root.SetActive(true);
            root.transform.SetAsLastSibling();
            panel.position = screenPosition;
        }

        public void Hide() => root.SetActive(false);

        private void Choose(MenuAction action)
        {
            Hide();
            ActionChosen?.Invoke(action);
        }
    }
}
