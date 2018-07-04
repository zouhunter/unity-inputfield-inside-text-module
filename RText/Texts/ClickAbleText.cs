using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RText
{
    public class ClickAbleText : RectEntryText, IPointerClickHandler
    {
        readonly Dictionary<string, Entry> _entryTable = new Dictionary<string, Entry>();
        private Dictionary<RectEntry, Entry> relatDic = new Dictionary<RectEntry, Entry>();

        struct Entry
        {
            public string RegexPattern;
            public Color Color;
            public Action<string> OnClick;

            public Entry(string regexPattern, Color color, Action<string> onClick)
            {
                RegexPattern = regexPattern;
                Color = color;
                OnClick = onClick;
            }
        }

        /// <summary>
        /// 正規表現にマッチした部分文字列にクリックイベントを登録します
        /// </summary>
        /// <param name="regexPattern">正規表現</param>
        /// <param name="onClick">クリック時のコールバック</param>
        public void SetClickableByRegex(string regexPattern, Action<string> onClick)
        {
            SetClickableByRegex(regexPattern, color, onClick);
        }

        /// <summary>
        /// 正規表現にマッチした部分文字列に色とクリックイベントを登録します
        /// </summary>
        /// <param name="regexPattern">正規表現</param>
        /// <param name="color">正規表現でマッチしたテキストの色</param>
        /// <param name="onClick">クリック時のコールバック</param>
        public void SetClickableByRegex(string regexPattern, Color color, Action<string> onClick)
        {
            if (string.IsNullOrEmpty(regexPattern) || onClick == null)
            {
                return;
            }

            _entryTable[regexPattern] = new Entry(regexPattern, color, onClick);
        }

        public override void RemoveRectEntrys()
        {
            base.RemoveRectEntrys();
            _entryTable.Clear();
        }

        /// <summary>
        /// テキストの変更などでクリックする文字位置の再計算が必要なときに呼び出されます
        /// 親の RegisterClickable メソッドを使ってクリック対象文字の情報を登録してください
        /// </summary>
        protected override void RegistEntrys()
        {
            foreach (var entry in _entryTable.Values)
            {
                foreach (Match match in Regex.Matches(text, entry.RegexPattern))
                {
                    var rEntry = RegistRectEntry(match.Index, match.Value.Length, entry.Color);
                    relatDic[rEntry] = entry;
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var localPosition = ToLocalPosition(eventData.position, eventData.pressEventCamera);

            for (int i = 0; i < entries.Count; i++)
            {
                for (int j = 0; j < entries[i].Rects.Count; j++)
                {
                    if (entries[i].Rects[j].Contains(localPosition))
                    {
                        Debug.Log(entries[i].Rects[j]);
                        var rEntry = entries[i];
                        if (relatDic.ContainsKey(rEntry))
                        {
                            var entry = relatDic[rEntry];
                            entry.OnClick.Invoke(entry.RegexPattern);
                        }
                        else
                        {
                            Debug.Log(rEntry);
                        }
                        break;
                    }
                }
            }
        }
    }
}