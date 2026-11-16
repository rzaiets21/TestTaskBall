using System;
using System.Collections;
using System.Collections.Generic;

namespace TestTaskBall.utils {
    public static class Extensions {
        public static bool IsNullOrEmpty(this IList list) {
            return list == null || list.Count == 0;
        }

        public static T FirstOrDefault<T>(this IList<T> list, Predicate<T> predicate = null) {
            if (list == null || list.Count == 0) {
                return default;
            }

            if (predicate == null) {
                return list[0];
            }
            
            for (int i = 0; i < list.Count; i++) {
                if (predicate.Invoke(list[i])) {
                    return list[i];
                }
            }
            
            return default;
        }
    }
}