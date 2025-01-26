using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.KaomoLab.CSEmulator
{
    //頻繁なAddとClearで負荷がかかりにくいかもしれない実装
    public class RemainedList<T> where T : class
    {
        int head = 0;
        readonly List<T> list;

        public RemainedList(int capacity)
        {
            list = new List<T>(capacity);
        }

        public bool TryRemainAdd(out T item)
        {
            if (list.Count > head)
            {
                var ret = list[head];
                head++;
                item = ret;
                return true;
            }
            item = null;
            return false;
        }

        public void Add(T value)
        {
            list.Add(value);
            head++;
        }

        public void Clear()
        {
            head = 0;
        }

        public IEnumerable<T> Each()
        {
            for(var i = 0; i < head; i++)
            {
                yield return list[i];
            }
        }
    }

    //https://docs.unity3d.com/ScriptReference/GL.html
    public class OpenGLDrawer
    {

        static Material lineMaterial;
        static void CreateMaterial()
        {
            if (!lineMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        public OpenGLDrawer()
        {
        }

        class Line
        {
            public Vector3 start;
            public Vector3 end;
            public Color color;
            public Line(Vector3 start, Vector3 end, Color color)
            {
                Set(start, end, color);
            }
            public void Set(Vector3 start, Vector3 end, Color color)
            {
                this.start = start;
                this.end = end;
                this.color = color;
            }
        }
        readonly RemainedList<Line> lines = new(10);

        int nowFrame = 0;

        public void AddLine(Vector3 start, Vector3 end, Color color, int frame)
        {
            RefleshFrame(frame);
            if (lines.TryRemainAdd(out var line))
                line.Set(start, end, color);
            else
                lines.Add(new Line(start, end, color));
        }
        void RefleshFrame(int frame)
        {
            //OnRenderObjectは複数回呼ばれる可能性があるのでこのような処理になっている
            if (nowFrame == frame) return;

            lines.Clear();
            nowFrame = frame;
        }

        public void DoRender(int frame)
        {
            RefleshFrame(frame);
            CreateMaterial();
            GL.PushMatrix(); //基本的にworld座標なので不要だけど一応

            DrawLines();

            GL.PopMatrix();
        }

        void DrawLines()
        {
            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);

            foreach (var line in lines.Each())
            {
                GL.Color(line.color);
                GL.Vertex3(line.start.x, line.start.y, line.start.z);
                GL.Vertex3(line.end.x, line.end.y, line.end.z);
            }

            GL.End();
        }
    }
}
