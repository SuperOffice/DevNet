using System;

namespace SuperOffice.DevNet.CDD
{
    public class StepInfo : MarshalByRefObject
    {
        public string Name { get; set; }
        public int StepNumber { get; set; }
        public string State { get; set; }

        public override string ToString()
        {
            return string.Format("{0}-{1}{2}", Name, StepNumber, State == "Released" ? "R" : "D");
        }
    }
}
