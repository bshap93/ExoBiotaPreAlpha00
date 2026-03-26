#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Yarn;
using Yarn.Unity;

namespace Manager.DialogueScene
{
    /// <summary>
    ///     Yarn variable storage that persists values using Easy Save 3.
    ///     Stores three dictionaries in ES3: floats, strings, bools.
    /// </summary>
    public class ES3VariableStorage : VariableStorageBehaviour, IEnumerable<KeyValuePair<string, object>>
    {
        [Tooltip("Key prefix used for the three typed dictionaries in the ES3 file")]
        public string keyPrefix = "Yarn.";

        [Tooltip("If true, writes to ES3 immediately on every SetValue()")]
        public bool autoSaveOnSet = true;

        // In-memory caches, merged for enumeration/Contains()
        private readonly Dictionary<string, object> _variables = new();
        private readonly Dictionary<string, Type> _variableTypes = new();
        private Dictionary<string, bool> _bools = new();

        private string _es3File;

        // Typed views (what actually goes to disk)
        private Dictionary<string, float> _floats = new();
        private Dictionary<string, string> _strings = new();


        private string FloatKey => $"{keyPrefix}Float";
        private string StringKey => $"{keyPrefix}String";
        private string BoolKey => $"{keyPrefix}Bool";


        private void OnEnable()
        {
            _es3File = GetSaveFilePath();
            LoadAllFromES3();
        }

        public void SaveAllVariables()
        {
            if (string.IsNullOrEmpty(_es3File))
                _es3File = GetSaveFilePath(); // ensure path is set
            SaveAllToES3(); // private helper already writes all 3 dicts
        }

        public void ReloadFromDisk()
        {
            if (string.IsNullOrEmpty(_es3File))
                _es3File = GetSaveFilePath();
            LoadAllFromES3(); // private helper already rebuilds caches
        }

        public string GetFilePath()
        {
            if (string.IsNullOrEmpty(_es3File))
                _es3File = GetSaveFilePath();
            return _es3File;
        }

        #region Helpers

        private void ValidateVariableName(string variableName)
        {
            if (variableName.StartsWith("$") == false)
                throw new ArgumentException(
                    $"{variableName} is not a valid variable name: Variable names must start with a '$'.");
        }

        private static bool TryGetAsType<T>(Dictionary<string, object> dict, string key,
            [NotNullWhen(true)] out T? result)
        {
            if (dict.TryGetValue(key, out var obj) && typeof(T).IsAssignableFrom(obj.GetType()))
            {
                result = (T)obj;
                return true;
            }

            result = default;
            return false;
        }

        private void RebuildMergedCache()
        {
            _variables.Clear();
            _variableTypes.Clear();

            foreach (var kvp in _floats)
            {
                _variables[kvp.Key] = kvp.Value;
                _variableTypes[kvp.Key] = typeof(float);
            }

            foreach (var kvp in _strings)
            {
                _variables[kvp.Key] = kvp.Value;
                _variableTypes[kvp.Key] = typeof(string);
            }

            foreach (var kvp in _bools)
            {
                _variables[kvp.Key] = kvp.Value;
                _variableTypes[kvp.Key] = typeof(bool);
            }
        }

        private string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.DialogueSave);
        }

        private void EnsureFilePath()
        {
            if (string.IsNullOrEmpty(_es3File))
                _es3File = GetSaveFilePath();
        }

        private void SaveAllToES3()
        {
            EnsureFilePath();
            ES3.Save(FloatKey, _floats, _es3File);
            ES3.Save(StringKey, _strings, _es3File);
            ES3.Save(BoolKey, _bools, _es3File);
        }

        private void LoadAllFromES3()
        {
            EnsureFilePath();
            _floats = ES3.Load(FloatKey, _es3File, new Dictionary<string, float>());
            _strings = ES3.Load(StringKey, _es3File, new Dictionary<string, string>());
            _bools = ES3.Load(BoolKey, _es3File, new Dictionary<string, bool>());
            RebuildMergedCache();
        }

        private void SaveTypedDictionary(string which)
        {
            EnsureFilePath();
            switch (which)
            {
                case nameof(_floats): ES3.Save(FloatKey, _floats, _es3File); break;
                case nameof(_strings): ES3.Save(StringKey, _strings, _es3File); break;
                case nameof(_bools): ES3.Save(BoolKey, _bools, _es3File); break;
            }
        }

        #endregion

        #region Setters

        public override bool TryGetValue<T>(string variableName, [NotNullWhen(true)] out T result)
        {
            ValidateVariableName(variableName);

            switch (GetVariableKind(variableName)) // uses Contains() or Program
            {
                case VariableKind.Stored:
                    if (TryGetAsType(_variables, variableName, out result)) return true;

                    if (Program is null)
                        throw new InvalidOperationException(
                            $"Can't get initial value for {variableName}, because {nameof(Program)} is not set");
                    return Program.TryGetInitialValue(variableName, out result);

                case VariableKind.Smart:
                    if (SmartVariableEvaluator is null)
                        throw new InvalidOperationException(
                            $"Can't get value for smart variable {variableName}, because {nameof(SmartVariableEvaluator)} is not set");
                    return SmartVariableEvaluator.TryGetSmartVariable(variableName, out result);

                case VariableKind.Unknown:
                default:
                    result = default;
                    return false;
            }
        }

        public override void SetValue(string variableName, string stringValue)
        {
            ValidateVariableName(variableName);

            _strings[variableName] = stringValue;
            _variables[variableName] = stringValue;
            _variableTypes[variableName] = typeof(string);

            if (autoSaveOnSet) SaveTypedDictionary(nameof(_strings));
            NotifyVariableChanged(variableName, stringValue); // fires listeners
        }

        public override void SetValue(string variableName, float floatValue)
        {
            ValidateVariableName(variableName);

            _floats[variableName] = floatValue;
            _variables[variableName] = floatValue;
            _variableTypes[variableName] = typeof(float);

            if (autoSaveOnSet) SaveTypedDictionary(nameof(_floats));
            NotifyVariableChanged(variableName, floatValue);
        }

        public override void SetValue(string variableName, bool boolValue)
        {
            ValidateVariableName(variableName);

            _bools[variableName] = boolValue;
            _variables[variableName] = boolValue;
            _variableTypes[variableName] = typeof(bool);

            if (autoSaveOnSet) SaveTypedDictionary(nameof(_bools));
            NotifyVariableChanged(variableName, boolValue);
        }

        #endregion


        #region Bulk / Utility

        public override void Clear()
        {
            _floats.Clear();
            _strings.Clear();
            _bools.Clear();
            _variables.Clear();
            _variableTypes.Clear();

            SaveAllToES3();
        }

        public override bool Contains(string variableName)
        {
            return _variables.ContainsKey(variableName);
        }

        // public override void SetAllVariables(Dictionary<string, float> floats, Dictionary<string, string> strings,
        //     Dictionary<string, bool> bools, bool clear = true)
        // {
        //     if (clear)
        //     {
        //         _floats.Clear();
        //         _strings.Clear();
        //         _bools.Clear();
        //         _variables.Clear();
        //         _variableTypes.Clear();
        //     }
        //
        //     // Use SetValue to ensure notifications & caches are correct
        //     foreach (var kv in floats) SetValue(kv.Key, kv.Value);
        //     foreach (var kv in strings) SetValue(kv.Key, kv.Value);
        //     foreach (var kv in bools) SetValue(kv.Key, kv.Value);
        //
        //     // Persist all in one batch
        //     SaveAllToES3();
        // }

        // Batching is used to avoid multiple writes to disk
        public override void SetAllVariables(
            Dictionary<string, float> floats,
            Dictionary<string, string> strings,
            Dictionary<string, bool> bools,
            bool clear = true)
        {
            var prev = autoSaveOnSet;
            autoSaveOnSet = false; // suppress per-variable writes

            if (clear)
            {
                _floats.Clear();
                _strings.Clear();
                _bools.Clear();
                _variables.Clear();
                _variableTypes.Clear();
            }

            foreach (var kv in floats) SetValue(kv.Key, kv.Value);
            foreach (var kv in strings) SetValue(kv.Key, kv.Value);
            foreach (var kv in bools) SetValue(kv.Key, kv.Value);

            SaveAllToES3(); // single write
            autoSaveOnSet = prev;
        }

        public override (Dictionary<string, float> FloatVariables, Dictionary<string, string> StringVariables,
            Dictionary<string, bool> BoolVariables) GetAllVariables()
        {
            // Return shallow copies so callers can’t mutate our dictionaries
            return (
                new Dictionary<string, float>(_floats),
                new Dictionary<string, string>(_strings),
                new Dictionary<string, bool>(_bools)
            );
        }

        #endregion

        #region Enumeration

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)_variables).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)_variables).GetEnumerator();
        }

        #endregion

        // private void OnDisable() {
        //     if (!autoSaveOnSet && !string.IsNullOrEmpty(_es3File))
        //         SaveAllToES3();
        // }
    }
}