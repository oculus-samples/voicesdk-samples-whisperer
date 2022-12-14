/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

/// <summary>
/// Writes logs to text file on device.
/// </summary>
namespace Whisperer
{
    public class Logger : MonoBehaviour
    {
        public static Logger Instance;

        [SerializeField] private bool _writeLogs;
        [SerializeField] private TMP_Text _pathField;
        [SerializeField] private bool _toConsole;

        private string _aggregate;
        private readonly List<string> _savedLogs = new();

        private void Awake()
        {
            Instance = this;
        }

        public void AddLog(string log)
        {
            if (_toConsole) Debug.Log(log);

            if (_writeLogs)
            {
                _savedLogs.Add(log);
                WriteLogsToFile();
            }
        }

        private void WriteLogsToFile()
        {
            var path = Application.persistentDataPath + "/savedlogs.txt";
            if (_pathField) _pathField.text = path;
            var writer = new StreamWriter(path, false);

            _aggregate = "";
            _savedLogs.ForEach(log => { _aggregate += log + "\n"; });

            writer.Write(_aggregate);
            writer.Close();
        }
    }
}
