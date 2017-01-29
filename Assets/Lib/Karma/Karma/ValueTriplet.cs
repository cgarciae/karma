using System;

namespace Karma {
    public class ValueTriplet<T1, T2, T3> {
        public readonly T1 First;
        public readonly T2 Second;
        public readonly T3 Third;

        public ValueTriplet() {
            First = default(T1);
            Second = default(T2);
            Third = default(T3);
        }

        public ValueTriplet(T1 first, T2 second, T3 third) {
            First = first;
            Second = second;
            Third = third;
        }

        public override bool Equals(Object obj) {
            var that = obj as Karma.ValueTriplet<T1, T2, T3>;

            if (that == null) {
                return false;
            }

            return Equals(that);
        }

        public bool Equals(Karma.ValueTriplet<T1, T2, T3> that) {
            if (that == null) {
                return false;
            }

            return object.Equals(First, that.First) && object.Equals(Second, that.Second) && object.Equals(Third, that.Third);
        }

        public override int GetHashCode() {
            unchecked
            {
                return 17 * First.GetHashCode() + 31 * Second.GetHashCode() + 47 * Third.GetHashCode();
            }
        }
    }

    public static class ValueTriplet {
        public static Karma.ValueTriplet<T1, T2, T3> New<T1, T2, T3>(T1 first, T2 second, T3 third) {
            return new Karma.ValueTriplet<T1, T2, T3>(first, second, third);
        }
    }
}
