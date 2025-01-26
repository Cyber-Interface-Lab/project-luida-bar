using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator
{
    public class MovingAveragerFloat
    {
        int head;
        int count;
        float sum;
        float[] values;

        public float average => sum / count;
        public bool hasAverage => count > 0;

        public MovingAveragerFloat(int buffer)
        {
            values = new float[buffer];
        }

        public void Push(float value)
        {
            var prev = values[head];
            values[head] = value;

            head++;
            if (head == values.Length) head = 0;

            sum += value;
            if (count == values.Length)
            {
                sum -= prev;
            }
            else
            {
                count++;
            }
        }
    }
}
