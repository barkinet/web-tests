﻿//
// TestLogger.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;

namespace Xamarin.AsyncTests
{
	public sealed class TestLogger : ITestLogger
	{
		readonly TestLoggerBackend backend;

		internal TestLoggerBackend Backend {
			get { return backend; }
		}

		public TestLogger (TestLoggerBackend backend)
		{
			this.backend = backend;
		}

		public int LogLevel {
			get; set;
		}

		void OnLogEvent (TestLoggerBackend.LogEntry entry)
		{
			if (entry.LogLevel <= LogLevel)
				backend.OnLogEvent (entry);
		}

		public void LogDebug (int level, string message)
		{
			OnLogEvent (new TestLoggerBackend.LogEntry (TestLoggerBackend.EntryKind.Debug, level, message));
		}

		public void LogDebug (int level, string format, params object[] args)
		{
			LogDebug (level, string.Format (format, args));
		}

		public void LogMessage (string message)
		{
			OnLogEvent (new TestLoggerBackend.LogEntry (TestLoggerBackend.EntryKind.Message, 0, message));
		}

		public void LogMessage (string format, params object[] args)
		{
			LogMessage (string.Format (format, args));
		}

		public void LogError (Exception error)
		{
			OnLogEvent (new TestLoggerBackend.LogEntry (TestLoggerBackend.EntryKind.Error, 0, error.Message, error));
		}

		public static string Print (object obj)
		{
			return obj != null ? obj.ToString () : "<null>";
		}

		public void ResetStatistics ()
		{
			backend.OnStatisticsEvent (new TestLoggerBackend.StatisticsEventArgs {
				Type = TestLoggerBackend.StatisticsEventType.Reset });
		}

		public void OnTestRunning (TestName name)
		{
			backend.OnStatisticsEvent (new TestLoggerBackend.StatisticsEventArgs {
				Type = TestLoggerBackend.StatisticsEventType.Running, Name = name
			});
		}

		public void OnTestFinished (TestName name, TestStatus status)
		{
			backend.OnStatisticsEvent (new TestLoggerBackend.StatisticsEventArgs {
				Type = TestLoggerBackend.StatisticsEventType.Finished, Name = name, Status = status
			});
		}

		public void OnException (TestName name, Exception ex)
		{
			OnTestFinished (name, TestStatus.Error);
		}
	}
}

