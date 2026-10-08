using UnityEngine;

namespace ProjectFantasy.Utils
{
    public static class AnimatorExtensions
    {
        // 초기화 시 1회만 호출 (parameters는 배열 할당)
        public static bool HasFloatParameter(this Animator animator, int parameterHash)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.nameHash == parameterHash && parameter.type == AnimatorControllerParameterType.Float) return true;
            }
            return false;
        }
    }
}
