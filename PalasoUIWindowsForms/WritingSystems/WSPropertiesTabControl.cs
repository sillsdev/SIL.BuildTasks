using System;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.WritingSystems
{
	public partial class WSPropertiesTabControl : UserControl
	{
		private WritingSystemSetupModel _model;

		public WSPropertiesTabControl()
		{
			InitializeComponent();
		}

		public void BindToModel(WritingSystemSetupModel model)
		{
			 if (_model != null)
			{
				_model.SelectionChanged -= ModelChanged;
				_model.CurrentItemUpdated -= ModelChanged;
			}

			_model = model;
			_identifiersControl.BindToModel(_model);
			_fontControl.BindToModel(_model);
			_keyboardControl.BindToModel(_model);
			_sortControl.BindToModel(_model);
			_spellingControl.BindToModel(_model);


			if (_model != null)
			{
				_model.SelectionChanged+= ModelChanged;
				_model.CurrentItemUpdated += ModelChanged;
			}
			this.Disposed += OnDisposed;
		}

		private void ModelChanged(object sender, EventArgs e)
		{
		   if( !_model.CurrentIsVoice &&
				_tabControl.Controls.Contains(_spellingPage))
		   {
			   return;// don't mess if we really don't need a change
		   }

			_tabControl.Controls.Clear();
			this._tabControl.Controls.Add(this._identifiersPage);

			if( !_model.CurrentIsVoice)
			{
				this._tabControl.Controls.Add(this._spellingPage);
				this._tabControl.Controls.Add(this._fontsPage);
				this._tabControl.Controls.Add(this._keyboardsPage);
				this._tabControl.Controls.Add(this._sortingPage);
			}
		}

		void OnDisposed(object sender, EventArgs e)
		{
			if (_model != null)
				_model.SelectionChanged -= ModelChanged;
		}

		public void MoveDataFromViewToModel()
		{
			_identifiersControl.MoveDataFromViewToModel();
		}

		public void UnwireBeforeClosing()
		{
			_identifiersControl.UnwireBeforeClosing();
		}
	}
}