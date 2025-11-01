namespace EngineeringTargets.Services
{
    public static class LevelTemplates
    {
        public static class StandardSpheres
        {
            public const string Humanity = "Человечество";
            public const string State = "Государство";
            public const string Industry = "Отрасль";
            public const string Customer = "Предприятие-заказчик";
            public const string Developer = "Проектная организация (разработчик)";
            public const string Department = "Отдел (подразделение)";
            public const string Personal = "Личные";
        }

        public static string[] GetStandardLevelNames()
        {
            return new[]
            {
                StandardSpheres.Humanity,
                StandardSpheres.State,
                StandardSpheres.Industry,
                StandardSpheres.Customer,
                StandardSpheres.Developer,
                StandardSpheres.Department,
                StandardSpheres.Personal
            };
        }

        public static string[] GetRecommendedLevelNames(int startFrom = 0)
        {
            var all = GetStandardLevelNames();
            if (startFrom >= all.Length)
                return new string[0];

            var result = new string[all.Length - startFrom];
            for (int i = startFrom; i < all.Length; i++)
            {
                result[i - startFrom] = all[i];
            }
            return result;
        }
    }
}

