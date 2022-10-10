/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Writes logs to text file on device.
/// </summary>
namespace Whisperer
{
	public class Logger : MonoBehaviour
	{
		public static Logger Instance;

		[SerializeField] bool _writeLogs;
		[SerializeField] TMP_Text _pathField;
		[SerializeField] bool _toConsole;

		string _aggregate;
		List<string> _savedLogs = new List<string>();

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

		void WriteLogsToFile()
		{
			string path = Application.persistentDataPath + "/savedlogs.txt";
			if (_pathField) _pathField.text = path;
			StreamWriter writer = new StreamWriter(path, false);

			_aggregate = "";
			_savedLogs.ForEach(log =>
			{
				_aggregate += log + "\n";
			});

			writer.Write(_aggregate);
			writer.Close();
		}
	}
}
