namespace MFDSkillTracking.Common
{
    public static class Constants
    {
        public const int MIN_DICE_LEVEL = 1;
        public const int MAX_DICE_LEVEL = 100;

        private static int _computationAccuracy = 10000;
        public static int ComputationAccuracy
        {
            get { return _computationAccuracy; }
            set
            {
                _computationAccuracy = value == 1 ? 500
                    : value == 2 ? 1000
                    : value == 3 ? 2500
                    : value == 4 ? 5000
                    : value == 5 ? 10000
                    : 500;
            }
        }
    }
}
