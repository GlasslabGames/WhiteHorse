////////////////////////////////////////////////////////////////////////////////
//  
// @module Affter Effect Importer
// @author Osipov Stanislav lacost.st@gmail.com
//
////////////////////////////////////////////////////////////////////////////////
#define SUPPORT_HALF_SIZE
using System.IO;
using System;
using System.Xml;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public class AEDataParcer  {

	private static float frameDuration;

	public static AEAnimationData ParceAnimationData(string data) {

		XmlDocument doc =  new XmlDocument();
		doc.LoadXml(data);

		AEAnimationData animation = new AEAnimationData ();

		XmlNode anim = doc.SelectSingleNode("after_affect_animation_doc");


		XmlNode meta = anim.SelectSingleNode("meta");
		animation.frameDuration = GetFloat (meta, "frameDuration");
		animation.totalFrames = GetInt (meta, "totalFrames");
		animation.duration =  GetFloat (meta, "duration");

		frameDuration = animation.frameDuration;


		XmlNode composition = anim.SelectSingleNode("composition");
		animation.composition = ParseComposition (composition);




		XmlNode sub_items = anim.SelectSingleNode("sub_items");
		XmlNodeList usedCompositions = sub_items.SelectNodes("composition");

		foreach (XmlNode c in usedCompositions) {
			animation.addComposition(ParseComposition (c));
		}



		return animation;
	}
  
  public static IEnumerator asyncParceAnimationData(string data, Action<AEAnimationData> callback) {
    float startTime = Time.realtimeSinceStartup;
    XmlDocument doc =  new XmlDocument();
    doc.LoadXml(data);

    if (Time.realtimeSinceStartup - startTime > .01f) { yield return null; startTime = Time.realtimeSinceStartup; }

    AEAnimationData animation = new AEAnimationData ();
    
    XmlNode anim = doc.SelectSingleNode("after_affect_animation_doc");
    
    
    XmlNode meta = anim.SelectSingleNode("meta");
    animation.frameDuration = GetFloat (meta, "frameDuration");
    animation.totalFrames = GetInt (meta, "totalFrames");
    animation.duration =  GetFloat (meta, "duration");
    
    frameDuration = animation.frameDuration;
    
    if (Time.realtimeSinceStartup - startTime > .01f) { yield return null; startTime = Time.realtimeSinceStartup; }
    
    XmlNode composition = anim.SelectSingleNode("composition");
    animation.composition = ParseComposition (composition);
    
    if (Time.realtimeSinceStartup - startTime > .01f) { yield return null; startTime = Time.realtimeSinceStartup; }
    
    
    
    XmlNode sub_items = anim.SelectSingleNode("sub_items");
    XmlNodeList usedCompositions = sub_items.SelectNodes("composition");
    
    if (Time.realtimeSinceStartup - startTime > .01f) { yield return null; startTime = Time.realtimeSinceStartup; }

    foreach (XmlNode c in usedCompositions) {
      animation.addComposition(ParseComposition (c));
      if (Time.realtimeSinceStartup - startTime > .01f) { yield return null; startTime = Time.realtimeSinceStartup; }
    }
    
    
    
    callback(animation);
  }

	public static AECompositionTemplate ParseComposition(XmlNode composition) {

		AECompositionTemplate comp = new AECompositionTemplate ();

		comp.id = System.Convert.ToInt32(composition.Attributes.GetNamedItem("id").Value);
    comp.width = System.Convert.ToSingle(composition.Attributes.GetNamedItem("w").Value);
		comp.heigh = System.Convert.ToSingle(composition.Attributes.GetNamedItem("h").Value);


		XmlNode meta = composition.SelectSingleNode ("meta");
		comp.duration = GetFloat (meta, "duration");
		comp.totalFrames = GetInt (meta, "totalFrames");
		comp.frameDuration = frameDuration;


		XmlNodeList layersNodes = composition.SelectNodes("layer");

		foreach (XmlNode layerNode in layersNodes) {
			AELayerTemplate layer = new AELayerTemplate ();

			string layerType = layerNode.Attributes.GetNamedItem("type").Value;

			if(layerType.Equals("Composition")) {
				layer.type = AELayerType.COMPOSITION;
				layer.id  = System.Convert.ToInt32(layerNode.Attributes.GetNamedItem("id").Value);
			}

			if(layerType.Equals("Footage")) {
				layer.type = AELayerType.FOOTAGE;
        string sourceString = layerNode.Attributes.GetNamedItem("source").Value;
#if !SUPPORT_HALF_SIZE
        layer.source = sourceString;
#else
        if (GLResourceManager.ScreenHalfSize && GLResourceManager.InstanceOrCreate.AssetExists(Path.GetFileNameWithoutExtension(sourceString) + "_halfSize"))
        {
          layer.source = "_halfSize/"+ Path.GetFileNameWithoutExtension(sourceString) + "_halfSize"+ Path.GetExtension(sourceString);
        }
        else
        {
          layer.source = sourceString;
        }
#endif
			}

			layer.index  = Int32.Parse (layerNode.Attributes.GetNamedItem("index").Value);

			if(layerNode.Attributes.GetNamedItem("parent").Value != "none") {
				layer.parent = Int32.Parse (layerNode.Attributes.GetNamedItem("parent").Value);
			} else {
				layer.parent = 0;
			}


			layer.width  = System.Convert.ToInt32(layerNode.Attributes.GetNamedItem("w").Value);
			layer.height = System.Convert.ToInt32(layerNode.Attributes.GetNamedItem("h").Value);

			layer.name = layerNode.Attributes.GetNamedItem("name").Value;

      string blendMode = layerNode.Attributes.GetNamedItem("blending").Value;
			layer.blending = (AELayerBlendingType) System.Enum.Parse (typeof(AELayerBlendingType), blendMode);

			float inTime  = GetFloat (layerNode, "inPoint");
			float outTime = GetFloat (layerNode, "outPoint");
			layer.setInOutTime (inTime, outTime, comp);


			XmlNodeList frameNodes = layerNode.SelectNodes("keyframe");
      AEFrameTemplate prevFrame = null;
			foreach (XmlNode frameNode in frameNodes) {
        AEFrameTemplate frame;
        if (prevFrame != null)
          frame = new AEFrameTemplate (prevFrame); // start with previous frame's values
        else
          frame = new AEFrameTemplate();

				frame.index = Int32.Parse (frameNode.Attributes.GetNamedItem("frame").Value);
				//frame.time = System.Convert.ToSingle (frameNode.Attributes.GetNamedItem("time").Value);

				XmlNode source = frameNode.SelectSingleNode ("source");
				XmlNodeList propertyNodes = source.SelectNodes ("property");
				foreach(XmlNode propertyNode in propertyNodes) {
					string propType = propertyNode.Attributes.GetNamedItem ("name").Value;

					switch(propType) {
						case AEPropertyType.ANCHOR_POINT:
						  frame.pivot = new Vector3 (GetFloat(propertyNode, "x"), GetFloat (propertyNode, "y"), GetFloat (propertyNode, "z"));
              frame.IsPivotChanged = true;
						  break;
						case AEPropertyType.POSITION:
						  frame.SetPosition( new Vector3 (GetFloat(propertyNode, "x"), GetFloat (propertyNode, "y"), GetFloat (propertyNode, "z")) );
              frame.IsPositionChanged = true;
						  break;
						case AEPropertyType.SCALE:
						  frame.scale = new Vector3 (GetFloat(propertyNode, "x"), GetFloat (propertyNode, "y"), GetFloat (propertyNode, "z")) * 0.01f;
              frame.IsScaleChanged = true;
						  break;
						case AEPropertyType.ROTATION:
						  frame.rotation = GetFloat (propertyNode, "val");
              frame.IsRotationChanged = true;
						  break;
						case AEPropertyType.OPACITY:
              frame.opacity = GetFloat (propertyNode, "val");
              frame.IsOpacityChanged = true;
						  break;
					}
				}

				layer.addFrame (frame);
        prevFrame = frame;
			}

			comp.addLayer (layer);
		}

    comp.layers.TrimExcess();

		return comp;
	}


  public static float GetFloat(XmlNode node, string name) {
    return System.Convert.ToSingle(node.Attributes.GetNamedItem(name).Value, CultureInfo.InvariantCulture);
	}

	public static int GetInt(XmlNode node, string name) {
		return System.Convert.ToInt32(node.Attributes.GetNamedItem(name).Value);
	}
}
