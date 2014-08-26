using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AELayerTemplate  {

	public int id;

	public int index;
	public int parent;
	
	public int width  = 1;
	public int height = 1;

	public string name;

	public AELayerType type;
	public AELayerBlendingType blending;
	public AELayerBlendingType forcedBlending = AELayerBlendingType.NORMAL;



	public int inFrame;
	public int outFrame;

	public float inTime;
	public float outTime;


	public string source;
	
	public List<AEFrameTemplate> frames =  new List<AEFrameTemplate>();

	
	public void addFrame(AEFrameTemplate frame) {
		frames.Add (frame);
	}

	public void setInOutTime(float timeIn, float timeOut, AECompositionTemplate tpl) {
		inTime = timeIn;
		outTime = timeOut;

		inFrame = Mathf.RoundToInt (inTime / tpl.frameDuration);
		outFrame = Mathf.RoundToInt (outTime / tpl.frameDuration);
	}

  // Difference with GetKeyframe is this searches backwards for what the frame SHOULD be
  public AEFrameTemplate GetFrame(int index) {
    for (int i=frames.Count-1; i>=0; i--)
    {
      if (frames[i].index <= index) return frames[i];
    }

    Debug.LogError("Could not find frame for index "+index);
    return null;
  }

  // Difference with above is this grabs the exact keyframe.
  public AEFrameTemplate GetKeyframe(int index)
  {
    // Binary search
    int min = 0;
    int max = frames.Count-1;
    int searchIndex = (max + min)/2;
    while (min <= max)
    {
      AEFrameTemplate frame = frames[searchIndex];
      if (frame.index == index)
      {
        return frame;
      }
      else if (frame.index < index) // it's above
      {
        min = searchIndex+1;
      }
      else // it's below
      {
        max = searchIndex-1;
      }
      
      searchIndex = (max+min)/2;
    }

    return null;
  }

  private int getFrameIndexInArray(AEFrameTemplate frame)
  {
    for (int i=frames.Count-1; i>=0; i--)// in frames)
    {
      if (frames[i] == frame)
      {
        return i;
      }
    }

    return -1;
  }

  public AEFrameTemplate GetLastFrameWithProperty(AEFrameTemplate fromFrame, string propType, bool LookBackward, int stopAtFrame = -1)
  {
    int prevFrameIndexInArray = getFrameIndexInArray(fromFrame);
    while (prevFrameIndexInArray > 0 && prevFrameIndexInArray < lastFrameIndex)
    {
      prevFrameIndexInArray += (LookBackward ? -1 : 1);
      AEFrameTemplate frame = frames[prevFrameIndexInArray];
      if (stopAtFrame != -1)
      {
        if ((LookBackward && frame.index <= stopAtFrame) || (!LookBackward && frame.index >= stopAtFrame))
        {
          //return null;
        }
      }

      switch(propType) {
        case AEPropertyType.ANCHOR_POINT:
          if (frame.IsPivotChanged)
          {
            return frame;
          }
          break;
        case AEPropertyType.POSITION:
          if (frame.IsPositionChanged)
          {
            return frame;
          }
          break;
        case AEPropertyType.SCALE:
          if (frame.IsScaleChanged)
          {
            return frame;
          }
          break;
        case AEPropertyType.ROTATION:
          if (frame.IsRotationChanged)
          {
            return frame;
          }
          break;
        case AEPropertyType.OPACITY:
          if (frame.IsOpacityChanged)
          {
            return frame;
          }
          break;
        default:
          return frame;
          break;
      }
    }

    return null;
  }

	public int totalFrames {
		get {
			return frames.Count;
		}
	}

	public int lastFrameIndex  {
		get {
			return totalFrames - 1;
		}
	}


	public string sourceNoExt {
		get {
			return source.Substring (0, source.Length - 4);
		}
	}
}
