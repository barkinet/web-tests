﻿//
// ReflectionTestSuiteBuilder.cs
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
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.AsyncTests.Framework.Reflection
{
	class ReflectionTestSuiteBuilder : TestCollectionBuilder
	{
		public Assembly Assembly {
			get;
			private set;
		}

		public TestCase Test {
			get;
			private set;
		}

		public ReflectionTestSuiteBuilder (ReflectionTestSuite suite)
			: base (suite, TestName.Empty, null)
		{
			Assembly = suite.Assembly;

			Resolve ();

			Test = new TestCaseCollection (this, suite.Name);
		}

		protected override IEnumerable<TestBuilder> ResolveChildren ()
		{
			foreach (var type in Assembly.ExportedTypes) {
				var tinfo = type.GetTypeInfo ();
				var attr = tinfo.GetCustomAttribute<AsyncTestFixtureAttribute> (true);
				if (attr == null)
					continue;

				yield return new ReflectionTestFixtureBuilder (this, attr, tinfo);
			}
		}

		protected override IEnumerable<TestHost> CreateParameterHosts ()
		{
			yield break;
		}

		class TestCaseCollection : TestCase
		{
			public TestCollectionBuilder Builder {
				get;
				private set;
			}

			public TestCaseCollection (TestCollectionBuilder builder, TestName name)
				: base (builder.Suite, name, builder)
			{
				Builder = builder;
			}

			internal override Task<bool> Run (TestContext ctx, CancellationToken cancellationToken)
			{
				return Builder.Invoker.Invoke (ctx, null, cancellationToken);
			}
		}
	}
}
