using System.Collections.Generic;

namespace Risk.Models
{
    public class ContinentEnum
    {
        public static IReadOnlyDictionary<int, ContinentEnum> ContinentsById => continentsById;
        public static IReadOnlyList<ContinentEnum> Continents => ContinentsById.Values.ToList();
        public int Id { get; }
        public int Bonus { get; }
        private static Dictionary<int, ContinentEnum> continentsById = new Dictionary<int, ContinentEnum>();
        private ContinentEnum(int id, int bonus)
        {
            Id = id;
            Bonus = bonus;
            continentsById[id] = this;
        }
        public static readonly ContinentEnum Asia = new ContinentEnum(0, 7);
        public static readonly ContinentEnum Africa = new ContinentEnum(1, 3);
        public static readonly ContinentEnum Europe = new ContinentEnum(2, 5);
        public static readonly ContinentEnum Australia = new ContinentEnum(3, 2);
        public static readonly ContinentEnum NorthAmerica = new ContinentEnum(4, 5);
        public static readonly ContinentEnum SouthAmerica = new ContinentEnum(5, 2);

    }
}
