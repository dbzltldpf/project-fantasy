using UnityEngine;

namespace ProjectFantasy.Items
{
    // 등급 목록과 가중치 추첨 (장비 데이터가 참조, 상자·보스 전용 표로 교체 가능)
    [CreateAssetMenu(fileName = "ItemGradeTable", menuName = "ProjectFantasy/Items/Item Grade Table")]
    public sealed class ItemGradeTable : ScriptableObject
    {
        [Tooltip("등급 목록 (확률 = 가중치 / 가중치 합)")]
        [SerializeField] private ItemGrade[] grades =
        {
            new ItemGrade("일반", new Color(0.9f, 0.9f, 0.9f), 70, 1f),
            new ItemGrade("희귀", new Color(0.35f, 0.6f, 1f), 22, 1.15f),
            new ItemGrade("영웅", new Color(0.7f, 0.4f, 1f), 7, 1.3f),
            new ItemGrade("전설", new Color(1f, 0.6f, 0.15f), 1, 1.5f),
        };

        // 가중치 비례 추첨 (가중치 합이 0이면 첫 등급)
        public ItemGrade Roll()
        {
            if (grades.Length == 0) return null;

            int totalWeight = 0;
            foreach (ItemGrade grade in grades)
            {
                totalWeight += grade.Weight;
            }
            if (totalWeight <= 0) return grades[0];

            int pick = Random.Range(0, totalWeight);
            foreach (ItemGrade grade in grades)
            {
                if (pick < grade.Weight) return grade;
                pick -= grade.Weight;
            }
            return grades[grades.Length - 1];
        }
    }
}
