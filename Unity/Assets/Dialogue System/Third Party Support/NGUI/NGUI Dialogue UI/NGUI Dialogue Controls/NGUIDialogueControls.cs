using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.NGUI {
	
	/// <summary>
	/// Contains all dialogue (conversation) controls for an NGUI Dialogue UI.
	/// </summary>
	[System.Serializable]
	public class NGUIDialogueControls : AbstractDialogueUIControls {
		
		/// <summary>
		/// The panel containing the dialogue controls. A panel is optional, but you may want one
		/// so you can include a background image, panel-wide effects, etc.
		/// </summary>
		public UIPanel panel;
		
		/// <summary>
		/// The NPC subtitle controls.
		/// </summary>
		public NGUISubtitleControls npcSubtitle;
		
		/// <summary>
		/// The PC subtitle controls.
		/// </summary>
		public NGUISubtitleControls pcSubtitle;
		
		/// <summary>
		/// The response menu controls.
		/// </summary>
		public NGUIResponseMenuControls responseMenu;
		
		public override AbstractUISubtitleControls NPCSubtitle { 
			get { return npcSubtitle; }
		}
		
		public override AbstractUISubtitleControls PCSubtitle {
			get { return pcSubtitle; }
		}
		
		public override AbstractUIResponseMenuControls ResponseMenu {
			get { return responseMenu; }
		}
		
		public override void ShowPanel() {
			if (panel != null) NGUIDialogueUIControls.SetControlActive(panel.gameObject, true);
		}
		
		public override void SetActive(bool value) {
			base.SetActive(value);
			if (panel != null) NGUIDialogueUIControls.SetControlActive(panel.gameObject, value);
		}

	}
		
}
