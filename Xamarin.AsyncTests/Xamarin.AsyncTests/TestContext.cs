﻿//
// TestContext.cs
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
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace Xamarin.AsyncTests
{
	using Constraints;

	public sealed class TestContext
	{
		readonly TestContext parent;
		readonly int logLevel;
		readonly TestStatistics statistics;
		readonly TestResult result;
		readonly TestLogger logger;
		readonly SynchronizationContext syncContext;

		public TestName Name {
			get;
			private set;
		}

		public TestResult Result {
			get { return result ?? parent.Result; }
		}

		internal TestContext (TestConfiguration config, SettingsBag settings, TestStatistics statistics,
			int logLevel, TestLogger logger, TestName name, TestResult result)
		{
			Name = name;
			this.statistics = statistics;
			this.logger = logger;
			this.result = result;
			this.logLevel = logLevel;
			this.syncContext = SynchronizationContext.Current;

			CreateConfigSnapshot (config, settings);
		}

		TestContext (TestContext parent, TestName name, TestResult result)
		{
			Name = name;
			this.parent = parent;
			this.result = result;
			this.logLevel = parent.logLevel;
			this.logger = parent.logger;
			this.statistics = parent.statistics;
			this.syncContext = SynchronizationContext.Current;
		}

		void Invoke (Action action)
		{
			if (syncContext == null)
				action ();
			else
				syncContext.Post (d => action (), null);
		}

		internal TestContext CreateChild (TestName name)
		{
			return new TestContext (this, name, result);
		}

		internal TestContext CreateChild (TestName name, TestResult result)
		{
			return new TestContext (this, name, result);
		}

		#region Statistics

		public void OnTestRunning ()
		{
			Invoke (() => statistics.OnTestRunning (Name));
		}

		public void OnTestFinished (TestStatus status)
		{
			Result.Status = status;
			Invoke (() => statistics.OnTestFinished (Name, status));
		}

		public void OnTestCanceled ()
		{
			OnTestFinished (TestStatus.Canceled);
		}

		#endregion

		#region Logging

		public void OnError (Exception error)
		{
			Result.AddError (error);
			Invoke (() => {
				statistics.OnException (Name, error);
				logger.LogError (error);
			});
		}

		public void LogDebug (int level, string message)
		{
			if (level > logLevel)
				return;
			Invoke (() => logger.LogDebug (level, message));
		}

		public void LogDebug (int level, string format, params object[] args)
		{
			LogDebug (level, string.Format (format, args));
		}

		public void LogMessage (string message)
		{
			Invoke (() => logger.LogMessage (message));
		}

		public void LogMessage (string message, params object[] args)
		{
			LogMessage (string.Format (message, args));
		}

		public void LogError (Exception error)
		{
			Invoke (() => logger.LogError (error));
		}

		#endregion

		#region Assertions

		public bool Expect (object actual, Constraint constraint)
		{
			return Expect (actual, constraint, false, null);
		}

		public bool Expect (object actual, Constraint constraint, string format = null, params object[] args)
		{
			return Expect (actual, constraint, false, format, args);
		}

		public bool Expect (bool value, bool fatal = false, string format = null, params object[] args)
		{
			return Expect (value, Is.True, fatal, format, args);
		}

		public bool Expect (object actual, Constraint constraint, bool fatal = false, string format = null, params object[] args)
		{
			var sb = new StringBuilder ();

			string error;
			if (constraint.Evaluate (actual, out error))
				return true;
			sb.AppendFormat ("AssertionFailed ({0})", constraint.Print ());
			if (format != null) {
				sb.Append (": ");
				if (args != null)
					sb.AppendFormat (format, args);
				else
					sb.Append (format);
			}
			if (error != null) {
				sb.AppendLine ();
				sb.Append (error);
			} else {
				sb.AppendLine ();
				sb.AppendFormat ("Actual value: {0}", Assert.Print (actual));
			}

			var exception = new AssertionException (sb.ToString ());
			Invoke (() => Result.AddError (exception));
			if (fatal)
				throw exception;
			return false;
		}

		#endregion

		#region Config Snapshot

		Dictionary<TestFeature, bool> features;
		List<TestCategory> categories;
		TestCategory currentCategory;

		void CreateConfigSnapshot (TestConfiguration config, SettingsBag settings)
		{
			features = new Dictionary<TestFeature, bool> ();
			categories = new List<TestCategory> ();

			foreach (var feature in config.Features) {
				bool enabled;
				if (feature.Constant != null)
					enabled = feature.Constant.Value;
				else
					enabled = settings.IsFeatureEnabled (feature.Name) ?? feature.DefaultValue ?? false;
				features.Add (feature, enabled);
			}

			categories.AddRange (config.Categories);

			TestCategory category = TestCategory.All;
			var key = settings.CurrentCategory;
			if (key != null && config.TryGetCategory (key, out category))
				currentCategory = category;
		}

		public TestCategory CurrentCategory {
			get { return parent != null ? parent.CurrentCategory : currentCategory; }
		}

		public bool IsEnabled (TestFeature feature)
		{
			return parent != null ? parent.IsEnabled (feature) : features [feature];
		}

		#endregion
	}
}

