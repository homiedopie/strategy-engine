using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvanceFeatureFilter
{
    public class Validator
    {
        public int MAX_STRING = 64;
        public int MAX_INT = int.MaxValue;

        public bool ValidInput<T1>(T1 input)
        {
            if (input is string)
            {
                return ValidateString(input.ToString());
            }

            if (input is int)
            {
                return ValidateInt(Convert.ToInt32(input));
            }

            return false;
        }

        public bool ValidateString(string input)
        {
            if (input.Length > MAX_STRING)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return true;
        }

        public bool ValidateInt(int input)
        {
            if (input > MAX_INT)
            {
                return false;
            }

            return true;
        }

        public bool ValidMax(int input, int max)
        {
            return input <= max;
        }
    }
}
