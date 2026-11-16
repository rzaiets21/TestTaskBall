using System.Collections;

namespace TestTaskBall.utils {
    public static class Extensions {
        public static bool IsNullOrEmpty(this IList list) {
            return list == null || list.Count == 0;
        }
    }
}