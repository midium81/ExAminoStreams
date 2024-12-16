namespace ExAminoStreams
{
    public class FloatTrackBar : TrackBar
    {
        private float precision = 0.01f;

        public float Precision
        {
            get { return precision; }
            set
            {
                precision = value;
            }
        }

        public new float LargeChange
        { 
            get { return base.LargeChange * precision; } 
            set { base.LargeChange = (int)(value / precision); } 
        }

        public new float Maximum
        { 
            get { return base.Maximum * precision; } 
            set { base.Maximum = (int)(value / precision); } 
        }

        public new float Minimum
        { 
            get { return base.Minimum * precision; } 
            set { base.Minimum = (int)(value / precision); } 
        }

        public new float SmallChange
        { 
            get { return base.SmallChange * precision; } 
            set { base.SmallChange = (int)(value / precision); } 
        }

        public new float Value
        { 
            get { return base.Value * precision; } 
            set 
            {
                var newValue = (int)(value / precision);
                if (newValue > base.Maximum)
                    newValue = base.Maximum;
                else if (newValue < base.Minimum)
                    newValue = base.Minimum;

                base.Value = newValue;
            } 
        }
    }
}