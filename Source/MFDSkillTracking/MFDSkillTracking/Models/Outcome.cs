using MFDSkillTracking.Common;

namespace MFDSkillTracking.Models
{
    public class Outcome : PropertyChangedBase
    {
        private double _victoryA;
        private double _victoryB;
        private double _victoryC;
        private double _defeatA;
        private double _defeatB;
        private double _defeatC;
        private double _scalingFactor;

        public double VictoryA
        {
            get { return _victoryA; }
            private set
            {
                _victoryA = value;
                OnPropertyChanged();
            }
        }

        public double VictoryB
        {
            get { return _victoryB; }
            private set
            {
                _victoryB = value;
                OnPropertyChanged();
            }
        }

        public double VictoryC
        {
            get { return _victoryC; }
            private set
            {
                _victoryC = value;
                OnPropertyChanged();
            }
        }

        public double DefeatA
        {
            get { return _defeatA; }
            private set
            {
                _defeatA = value;
                OnPropertyChanged();
            }
        }

        public double DefeatB
        {
            get { return _defeatB; }
            private set
            {
                _defeatB = value;
                OnPropertyChanged();
            }
        }

        public double DefeatC
        {
            get { return _defeatC; }
            private set
            {
                _defeatC = value;
                OnPropertyChanged();
            }
        }

        public double ScalingFactor
        {
            get { return _scalingFactor; }
            private set
            {
                _scalingFactor = value;
                OnPropertyChanged();
            }
        }

        public void Update(double outcome, double scale)
        {
            if (outcome <= -50) DefeatC += scale;
            else if (outcome <= -20) DefeatB += scale;
            else if (outcome < 0) DefeatA += scale;
            else if (outcome < 20) VictoryA += scale;
            else if (outcome < 50) VictoryB += scale;
            else VictoryC += scale;

            ScalingFactor += scale;
        }
    }
}
