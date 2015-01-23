using System;
namespace AttributeSystem
{
    #region Classes
    [System.Serializable]
    public class AttributeValue
    {
        public float Value = 0f;

        public float StartValue = 0f;
        public float ValuePerPoint = 0f;
        public float ValuePerPointMultipliedByCurrentPoints = 0f;

        public virtual void Reset()
        {
            Value = StartValue;
        }
        public virtual void Recalculate(int points)
        {
            Reset();
            for (int i = 1; i <= points; i++)
            {
                IncreaseValue(i);
            }
        }
        public virtual void IncreaseValue(int points)
        {
            Value += ValuePerPoint + points * ValuePerPointMultipliedByCurrentPoints;
        }
        public virtual void DecreaseValue(int points)
        {
            Value -= ValuePerPoint + (points - 1) * ValuePerPointMultipliedByCurrentPoints;
        }
    }
    #endregion

    [System.Serializable]
    public class Attribute
    {
        public string Name = "";
        public bool Enabled = true;

        //Can't be changed from outside
        public bool Locked = false;

        public virtual void Reset()
        {
            if (!Enabled || Locked)
                return;
            ResetPoints();
        }

        #region POINTS
        public int Points = 0;
        public int StartPoints = 0;
        public int MaxPoints = 0;
    
        public bool ResetPoints()
        {
            if (!Enabled || Locked || Points == StartPoints)
                return false;
            Points = StartPoints;
            RecalculateValue();
            UpdatePoints();
            return true;
        }

        public bool AddPoint()
        {
            if (!Enabled || Locked || Points >= MaxPoints)
                return false;
            Points++;
            RecalculateValue();
            UpdatePoints();
            return true;
        }
        public bool RemovePoint()
        {
            if (!Enabled || Locked || Points <= StartPoints)
                return false;
            Points--;
            RecalculateValue();
            UpdatePoints();
            return true;
        }
        public bool MaximizePoints()
        {
            if (!Enabled || Locked)
                return false;
            Points = MaxPoints;
            RecalculateValue();
            UpdatePoints();
            return true;
        }
        protected void UpdatePoints()
        {
            TriggerAttributeChanged();
        }
        #endregion

        #region VALUE
        public AttributeValue ValueInfo;

        public float Value
        {
            get
            {
                return ValueInfo.Value;
            }
            set
            {
                ValueInfo.Value = value;
            }
        }

        private void IncreaseValue()
        {
            ValueInfo.IncreaseValue(Points);
        }
        private void DecreaseValue()
        {
            ValueInfo.DecreaseValue(Points);
        }
        private void ResetValue()
        {
            ValueInfo.Reset();
        }
        public void RecalculateValue()
        {
            ValueInfo.Recalculate(Points);
        }

        #endregion

        #region EVENTS
        public delegate void Event(Attribute info);
        public static Event AttributeChanged;
        public Event ThisAttributeChanged;

        public void TriggerAttributeChanged()
        {
            if (AttributeChanged != null)
                AttributeChanged(this);
            if (ThisAttributeChanged != null)
                ThisAttributeChanged(this);
        }
        #endregion

        #region EDITOR_PROPERTIES
        public bool FoldOut = false;
        public bool Point_FoldOut = false;
        public bool Value_FoldOut = false;
        #endregion

        public override string ToString()
        {
            return string.Format("AttribName:{0} Points:{1} Value:{2}", Name, Points, Value);
        }

    }
}

