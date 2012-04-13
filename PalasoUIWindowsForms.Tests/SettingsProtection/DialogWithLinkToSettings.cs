﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PalasoUIWindowsForms.Tests.SettingsProtection
{
	public partial class DialogWithLinkToSettings : Form
	{
		public DialogWithLinkToSettings()
		{
			InitializeComponent();
			//settingsLauncherButton2.LaunchSettingsCallback = () => new DialogWithSomeSettings().ShowDialog();

			//Let the helper manage our visibility & password challenge
			_settingsLauncherHelper.CustomSettingsControl = _customSettingsButton;
		}

		private void _customSettingsButton_Click(object sender, EventArgs e)
		{
			_settingsLauncherHelper.LaunchSettingsIfAppropriate(() =>
																	{
																		using (var dlg = new DialogWithSomeSettings())
																		{
																			return dlg.ShowDialog();
																		}
																	});
		}
	}
}