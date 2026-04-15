using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.Example
{
	// Generate Id:7f72954c-f968-4aec-9c47-2d625e583344
	public partial class UIMainMenuPanel
	{
		public const string Name = "UIMainMenuPanel";
		
		[SerializeField]
		public UnityEngine.UI.Image Showtime;
		[SerializeField]
		public UnityEngine.UI.Image Title;
		[SerializeField]
		public UnityEngine.UI.Button Btn_Start;
		[SerializeField]
		public UnityEngine.UI.Button Btn_Exit;
		
		private UIMainMenuPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Showtime = null;
			Title = null;
			Btn_Start = null;
			Btn_Exit = null;
			
			mData = null;
		}
		
		public UIMainMenuPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UIMainMenuPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIMainMenuPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
