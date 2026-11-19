using System.Collections.Generic;
using UnityEngine;
namespace ProjectFantasy.Narration
{
    // 내레이션 문구 묶음 (한 줄씩 순서대로 표시)
    [CreateAssetMenu(fileName = "NarrationData", menuName = "ProjectFantasy/Narration/Narration Data")]
    public sealed class NarrationData : ScriptableObject
    {
        [Tooltip("한 줄씩 표시할 문구 (순서대로)")]
        [TextArea][SerializeField] private string[] lines;
        public IReadOnlyList<string> Lines => lines;
    }

}
