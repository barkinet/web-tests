﻿//
// OptionsPage.xaml.cs
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
﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Xamarin.AsyncTests.UI
{	
	public partial class OptionsPage : ContentPage
	{
		public OptionsModel Model {
			get;
			private set;
		}

		void OnRepeatCountChanged (object sender, EventArgs args)
		{
			int repeatCount;
			if (!int.TryParse (Model.RepeatCountEntry, out repeatCount)) {
				Model.RepeatCountEntry = Model.RepeatCount.ToString ();
				return;
			}

			Model.RepeatCount = repeatCount;
		}

		public OptionsPage (OptionsModel model)
		{
			Model = model;

			InitializeComponent ();

			BindingContext = model;

			Picker.SelectedIndexChanged += (sender, e) => {
				Model.Categories.SelectedIndex = Picker.SelectedIndex;
			};
		}

		void Load (object sender, PropertyChangedEventArgs args)
		{
			BatchBegin ();
			switch (args.PropertyName) {
			case "SelectedItem":
				Picker.SelectedIndex = Model.Categories.SelectedIndex;
				break;
			case "Configuration":
				LoadConfiguration ();
				break;
			}
			BatchCommit ();
		}

		void LoadConfiguration ()
		{
			// FIXME: can we also do this with data binding somehow?
			Picker.BatchBegin ();
			Picker.SelectedIndex = -1;
			Picker.Items.Clear ();
			foreach (var category in Model.Categories.Categories)
				Picker.Items.Add (category.Name);
			Picker.SelectedIndex = Model.Categories.SelectedIndex;
			Picker.BatchCommit ();
		}

		protected override void OnAppearing ()
		{
			Picker.BatchBegin ();
			Model.Categories.PropertyChanged += Load;
			LoadConfiguration ();
			Picker.BatchCommit ();
			base.OnAppearing ();
		}

		protected override void OnDisappearing ()
		{
			Picker.BatchBegin ();
			Model.Categories.PropertyChanged -= Load;
			Picker.SelectedIndex = -1;
			Picker.Items.Clear ();
			Picker.BatchCommit ();
			base.OnDisappearing ();
		}
	}
}

