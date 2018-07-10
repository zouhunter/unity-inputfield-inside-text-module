using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace RText
{
    public class InputAbleTextModifer : RectEntryModifyer/*, IPointerClickHandler*/
    {
        private InputField inputFieldPrefab;
        readonly Dictionary<string, Entry> _entryTable = new Dictionary<string, Entry>();
        private Dictionary<RectEntry, InputFieldEntry> relatDic = new Dictionary<RectEntry, InputFieldEntry>();
        private List<InputFieldEntry> inputFieldEntrys = new List<InputFieldEntry>();
        private List<int> disabled = new List<int>();

        private bool modifyed;
        public InputFieldEntry[] CreatedItems { get {return inputFieldEntrys.ToArray(); } }
        public class InputFieldEntry
        {
            public bool active = false;
            public int index;
            public InputField InputPrefab;
            public InputField inputField;
            public Text parent;
            public Action<int, string> onEndEdit;

            public void Init(int index, InputField InputPrefab, Text parent, Action<int, string> onEndEdit)
            {
                //Debug.Log("Init:" + index);
                this.index = index;
                this.onEndEdit = onEndEdit;
                this.InputPrefab = InputPrefab;
                this.parent = parent;
                active = true;
            }
            public void SetRect(Rect rect,bool interactable)
            {
                if (inputField == null)
                {
                    inputField = Instantiate(InputPrefab);
                    inputField.transform.SetParent(parent.transform, false);
                    inputField.onEndEdit.RemoveAllListeners();
                    inputField.onEndEdit.AddListener((x) => onEndEdit.Invoke(index, x));
                    inputField.gameObject.SetActive(true);
                }
                inputField.interactable = interactable;
                var rectTransform = inputField.transform as RectTransform;
                rectTransform.localPosition = rect.position;
                rectTransform.sizeDelta = new Vector2(rect.size.x, parent.fontSize);
            }
        }

        struct Entry
        {
            public string RegexPattern;
            public Action<int, string> onEndEdit;

            public Entry(string regexPattern, Action<int, string> onEndEdit)
            {
                RegexPattern = regexPattern;
                this.onEndEdit = onEndEdit;
            }
        }

        public void InitEnviroment(InputField prefab)
        {
            this.inputFieldPrefab = prefab;
            prefab.gameObject.SetActive(false);
        }

        public void SetInputField(string regexPattern, Action<int, string> onEndEdit)
        {
            if (string.IsNullOrEmpty(regexPattern) || onEndEdit == null)
            {
                return;
            }

            _entryTable[regexPattern] = new Entry(regexPattern, onEndEdit);
        }

        public override void RemoveRectEntrys()
        {
            base.RemoveRectEntrys();
            _entryTable.Clear();
        }
        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            base.ModifyMesh(vertexHelper);
            modifyed = true;
        }
        protected override void RegistEntrys()
        {
            int index = 0;
            foreach (var entry in _entryTable.Values)
            {
                foreach (Match match in Regex.Matches(text, entry.RegexPattern))
                {
                    if (!string.IsNullOrEmpty(match.Value))
                    {
                        var rEntry = RegistRectEntry(match.Index, match.Value.Length, color);
                        if (rEntry.Rects != null)
                        {
                            relatDic[(RectEntry)rEntry] = GetInputFiendEntry(index++, entry.onEndEdit);
                        }
                    }

                }
            }
        }

        private InputFieldEntry GetInputFiendEntry(int index, Action<int, string> onEndEdit)
        {
            var inputFieldEntry = inputFieldEntrys.Count > index ? inputFieldEntrys[index] : null;

            if (inputFieldEntry == null)
            {
                inputFieldEntry = new InputFieldEntry();
                inputFieldEntrys.Add(inputFieldEntry);
            }

            inputFieldEntry.Init(index, inputFieldPrefab, textComponent, onEndEdit);
            return inputFieldEntry;
        }

        private void Update()
        {
            if (modifyed)
            {
                modifyed = false;
                UpdateInputFields();
            }
        }

        public void UpdateInputFields()
        {
            //Debug.Log("UpdateInputFields:" + entries.Count);
            var index = 0;
            for (index = 0; index < entries.Count; index++)
            {
                var rEntry = entries[index];
                Debug.Assert(rEntry.Rects.Count != 0);
                var rect = entries[index].Rects[0];
                if (relatDic.ContainsKey(rEntry))
                {
                    var inputEntry = relatDic[rEntry];
                    inputEntry.SetRect(rect, !disabled.Contains(index));
                }
                else
                {
                    Debug.Log(rect);
                }
            }

            for (int i = index; i < inputFieldEntrys.Count; i++)
            {
                var item = inputFieldEntrys[i];
                if (item != null && item.inputField != null)
                    item.inputField.gameObject.SetActive(false);
            }
        }
        public void ClearInputFields()
        {
            if(inputFieldEntrys != null)
            foreach (var item in inputFieldEntrys)
            {
                if (item != null && item.inputField != null)
                    item.inputField.text = "";
            }
        }
        public void DisableInputField(int id)
        {
            disabled.Add(id);
        }
        public void ClearDisabled()
        {
            disabled.Clear();
        }
    }
}