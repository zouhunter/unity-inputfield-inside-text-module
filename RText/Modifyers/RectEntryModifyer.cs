using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

namespace RText
{
    [RequireComponent(typeof(Text))]
    public abstract class RectEntryModifyer : BaseMeshEffect
    {
        public class RectEntry
        {
            public Color color;
            public int startIndex;
            public int length;
            public List<Rect> Rects;
            internal void Init(int startIndex, int length, Color color)
            {
                this.startIndex = startIndex;
                this.color = color;
                this.length = length;
                Rects = new List<Rect>();
            }
        };
        protected Canvas _rootCanvas;
        protected virtual Canvas RootCanvas { get { return _rootCanvas ?? (_rootCanvas = GetComponentInParent<Canvas>()); } }
        protected const int CharVertsNum = 6;
        protected readonly List<RectEntry> entries = new List<RectEntry>();
        protected static readonly ObjectPool<List<UIVertex>> _verticesPool = new ObjectPool<List<UIVertex>>(() => new List<UIVertex>(), null, l => l.Clear());
        protected ObjectPool<RectEntry> rectPool = new ObjectPool<RectEntry>(() => new RectEntry(), null, null);
        protected Text textComponent;
        protected Color color { get { return textComponent.color; } }
        protected string text { get { return textComponent.text;} }
        protected RectTransform rectTransform { get { return textComponent.rectTransform; } }
        protected override void Awake()
        {
            base.Awake();
            textComponent = graphic as Text;
        }

        protected RectEntry RegistRectEntry(int startIndex, int wordLength, Color color)
        {
            if (startIndex < 0 || wordLength < 0 || startIndex + wordLength > text.Length)
            {
                return default(RectEntry);
            }
            var entry = rectPool.Get();
            entry.Init(startIndex, wordLength, color);
            entries.Add(entry);
            return entry;
        }

        protected abstract void RegistEntrys();

        public virtual void RemoveRectEntrys()
        {
            entries.Clear();
        }
        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            entries.Clear();
            RegistEntrys();

            var vertices = _verticesPool.Get();
            vertexHelper.GetUIVertexStream(vertices);

            Modify(ref vertices);

            vertexHelper.Clear();
            vertexHelper.AddUIVertexTriangleStream(vertices);
            _verticesPool.Release(vertices);
        }

        void Modify(ref List<UIVertex> vertices)
        {
            var verticesCount = vertices.Count;

            for (int i = 0, len = entries.Count; i < len; i++)
            {
                var entry = entries[i];

                for (int textIndex = entry.startIndex, wordEndIndex = entry.startIndex + entry.length; textIndex < wordEndIndex; textIndex++)
                {
                    var vertexStartIndex = textIndex * CharVertsNum;
                    if (vertexStartIndex + CharVertsNum > verticesCount)
                    {
                        break;
                    }

                    var min = Vector2.one * float.MaxValue;
                    var max = Vector2.one * float.MinValue;

                    for (int vertexIndex = 0; vertexIndex < CharVertsNum; vertexIndex++)
                    {
                        var vertex = vertices[vertexStartIndex + vertexIndex];
                        vertex.color = entry.color;
                        vertices[vertexStartIndex + vertexIndex] = vertex;

                        var pos = vertices[vertexStartIndex + vertexIndex].position;

                        if (pos.y < min.y)
                        {
                            min.y = pos.y;
                        }

                        if (pos.x < min.x)
                        {
                            min.x = pos.x;
                        }

                        if (pos.y > max.y)
                        {
                            max.y = pos.y;
                        }

                        if (pos.x > max.x)
                        {
                            max.x = pos.x;
                        }
                    }

                    entry.Rects.Add(new Rect { min = min, max = max });
                }

                // 同じ行で隣り合った矩形をまとめる
                var mergedRects = new List<Rect>();
                foreach (var charRects in SplitRectsByRow(entry.Rects))
                {
                    mergedRects.Add(CalculateAABB(charRects));
                }

                entry.Rects = mergedRects;
                entries[i] = entry;
            }
        }

        List<List<Rect>> SplitRectsByRow(List<Rect> rects)
        {
            var rectsList = new List<List<Rect>>();
            var rowStartIndex = 0;

            for (int i = 1, len = rects.Count; i < len; i++)
            {
                if (rects[i].xMin < rects[i - 1].xMin)//换行判断
                {
                    rectsList.Add(rects.GetRange(rowStartIndex, i - rowStartIndex));
                    rowStartIndex = i;
                }
            }

            if (rowStartIndex < rects.Count)
            {
                rectsList.Add(rects.GetRange(rowStartIndex, rects.Count - rowStartIndex));
            }

            return rectsList;
        }

        Rect CalculateAABB(List<Rect> rects)
        {
            var min = Vector2.one * float.MaxValue;
            var max = Vector2.one * float.MinValue;

            for (int i = 0, len = rects.Count; i < len; i++)
            {
                if (rects[i].xMin < min.x)
                {
                    min.x = rects[i].xMin;
                }

                if (rects[i].yMin < min.y)
                {
                    min.y = rects[i].yMin;
                }

                if (rects[i].xMax > max.x)
                {
                    max.x = rects[i].xMax;
                }

                if (rects[i].yMax > max.y)
                {
                    max.y = rects[i].yMax;
                }
            }

            return new Rect { min = min, max = max };
        }

        protected Vector3 ToLocalPosition(Vector3 position, Camera camera)
        {
            if (!RootCanvas)
            {
                return Vector3.zero;
            }

            if (RootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return transform.InverseTransformPoint(position);
            }

            var localPosition = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position, camera, out localPosition);
            return localPosition;
        }

    }
}
