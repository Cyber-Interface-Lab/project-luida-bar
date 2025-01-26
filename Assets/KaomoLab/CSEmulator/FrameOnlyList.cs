using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.KaomoLab.CSEmulator
{
    public class FrameOnlyList<T>
    {
        public int Count => newList.Count;

        readonly List<T> oldList = new();
        readonly List<T> newList = new();

        public FrameOnlyList() { }

        int frameCount = -1;

        public void Add(T item)
        {
            if(frameCount != UnityEngine.Time.frameCount)
            {
                oldList.Clear();
                oldList.AddRange(newList);
                newList.Clear();
                frameCount = UnityEngine.Time.frameCount;
            }
            newList.Add(item);
        }

        public bool Contains(T item)
        {
            return newList.Contains(item);
        }

        public IEnumerable<T> GetItems()
        {
            return newList;
        }

        public IEnumerable<T> GetAddedItems()
        {
            return newList.Where(i => !oldList.Contains(i));
        }

        public IEnumerable<T> GetRemovedItems()
        {
            return oldList.Where(i => !newList.Contains(i));
        }

    }
}
