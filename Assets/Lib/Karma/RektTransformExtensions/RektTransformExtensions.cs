/*
The MIT License (MIT)

Copyright (c) 2015 Christian 'ketura' McCarty

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/*
This file is intended to be used to alleviate the rather obtuse nature of the RectTransform.  RectTransform
is brilliant in the inspector, but leaves a bit to be desired when accessing via code.  This reproduces
all of the inspector's functionality (and then some) and makes some of the more AnnoyingToTypeAndHardToRemember
API calls into something more user-friendly.  

Speed was not the primary concern of this library, so I would not recommend using this in extremely 
frequent position adjustments.  However, now that you have this, you can see how to replicate everything
you might need, so it doubles as a reference resource as well.
*/

namespace RektTransform
{
    public static class Anchors
    {
        //All the default anchoring positions.  Note that unlike most grid-based systems, this
        // follows the form "Y, X", in accordance with how it is spoken aloud in English.
        //Thus, "StretchLeft" means stretching along the Y axis, and left-aligned on the X.
        public static MinMax TopLeft = new MinMax(0, 1, 0, 1);
        public static MinMax TopCenter = new MinMax(0.5f, 1, 0.5f, 1);
        public static MinMax TopRight = new MinMax(1, 1, 1, 1);
        public static MinMax TopStretch = new MinMax(0, 1, 1, 1);

        public static MinMax CenterLeft = new MinMax(0, 0.5f, 0, 0.5f);
        public static MinMax TrueCenter = new MinMax(0.5f, 0.5f, 0.5f, 0.5f);
        public static MinMax CenterRight = new MinMax(1, 0.5f, 1, 0.5f);
        public static MinMax CenterStretch = new MinMax(0, 0.5f, 1, 0.5f);

        public static MinMax BotLeft = new MinMax(0, 0, 0, 0);
        public static MinMax BotCenter = new MinMax(0.5f, 0, 0.5f, 0);
        public static MinMax BotRight = new MinMax(1, 0, 1, 0);
        public static MinMax BotStretch = new MinMax(0, 0, 1, 0);

        public static MinMax StretchLeft = new MinMax(0, 0, 0, 1);
        public static MinMax StretchCenter = new MinMax(0.5f, 0, 0.5f, 1);
        public static MinMax StretchRight = new MinMax(1, 0, 1, 1);
        public static MinMax TrueStretch = new MinMax(0, 0, 1, 1);
    }


    //Used to store the anchors of a RectTransform.  Could potentially be used for other things.
    public struct MinMax
    {
        public Vector2 min;
        public Vector2 max;

        public MinMax(Vector2 min, Vector2 max)
        {
            this.min = new Vector2(Mathf.Clamp01(min.x), Mathf.Clamp01(min.y));
            this.max = new Vector2(Mathf.Clamp01(max.x), Mathf.Clamp01(max.y));
        }

        public MinMax(float minx, float miny, float maxx, float maxy)
        {
            this.min = new Vector2(Mathf.Clamp01(minx), Mathf.Clamp01(miny));
            this.max = new Vector2(Mathf.Clamp01(maxx), Mathf.Clamp01(maxy));
        }
    }

    public static class RectTransformExtension
    {
        //Used for the creation of this library, but may come in handy for others.
        public static void DebugOutput(this UIBehaviour ui)
        {
            DebugOutput((RectTransform)ui.transform);
        }

        public static void DebugOutput(this RectTransform RT)
        {
            Debug.Log("Debug printing: " + RT);
            Debug.Log("Pos: " + RT.localPosition);
            Debug.Log("Rect: " + RT.rect);
            Debug.Log("Size Delta: " + RT.sizeDelta);
            Debug.Log("Pivot: " + RT.pivot);
            Debug.Log("Offset Min: " + RT.offsetMin + ", Offset Max: " + RT.offsetMax);
            Debug.Log("Anchored Pos: " + RT.anchoredPosition);
            Debug.Log("Anchor min: " + RT.anchorMin + ", Anchor Max: " + RT.anchorMax);
            Debug.Log("\n");
        }

        //Helper function for saving the anchors as one, instead of playing with both corners.

        public static MinMax GetAnchors(this UIBehaviour ui)
        {
            return GetAnchors(ui.transform as RectTransform);
        }

        public static MinMax GetAnchors(this RectTransform RT)
        {
            return new MinMax(RT.anchorMin, RT.anchorMax);
        }

        //Helper function to restore the anchors as above

        public static void SetAnchors(this UIBehaviour ui, MinMax anchors)
        {
            RectTransform RT = ui.transform as RectTransform;
            RT.anchorMin = anchors.min;
            RT.anchorMax = anchors.max;
        }

        public static void SetAnchors(this RectTransform RT, MinMax anchors)
        {
            RT.anchorMin = anchors.min;
            RT.anchorMax = anchors.max;
        }

        //Why the hell is UIBehavior.rectTransform not a thing?  This is dumb.

        public static RectTransform GetParent(this UIBehaviour ui)
        {
            return ui.transform.parent as RectTransform;
        }

        public static RectTransform GetParent(this RectTransform RT)
        {
            return RT.parent as RectTransform;
        }

        //If I have this many dots in a line, it had better be an ellipsis.

        public static float GetWidth(this UIBehaviour ui)
        {
            return GetWidth(ui.transform as RectTransform);
        }

        public static float GetWidth(this RectTransform RT)
        {
            return RT.rect.width;
        }

        public static float GetHeight(this UIBehaviour ui)
        {
            return GetHeight(ui.transform as RectTransform);
        }

        public static float GetHeight(this RectTransform RT)
        {
            return RT.rect.height;
        }

        //RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 10)
        //vs
        //RT.SetWidth(10)

        public static void SetWidth(this UIBehaviour ui, float width)
        {
            SetWidth(ui.transform as RectTransform, width);
        }

        public static void SetWidth(this RectTransform RT, float width)
        {
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        public static void SetHeight(this UIBehaviour ui, float height)
        {
            SetHeight(ui.transform as RectTransform, height);
        }

        public static void SetHeight(this RectTransform RT, float height)
        {
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public static void SetSize(this UIBehaviour ui, float width, float height)
        {
            SetSize(ui.transform as RectTransform, width, height);
        }

        public static void SetSize(this RectTransform RT, float width, float height)
        {
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            RT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        //Sets the position of the RT as if the anchor was set to center.

        public static void SetPos(this UIBehaviour ui, float x, float y)
        {
            SetPos(ui.transform as RectTransform, x, y);
        }

        public static void SetPos(this UIBehaviour ui, Vector2 pos)
        {
            SetPos(ui.transform as RectTransform, pos);
        }

        public static void SetPos(this RectTransform RT, float x, float y)
        {
            RT.SetPosX(x);
            RT.SetPosY(y);
        }

        public static void SetPos(this RectTransform RT, Vector2 pos)
        {
            RT.SetPosX(pos.x);
            RT.SetPosY(pos.y);
        }

        public static void SetPosX(this UIBehaviour ui, float x)
        {
            SetPosX(ui.transform as RectTransform, x);
        }

        public static void SetPosY(this UIBehaviour ui, float y)
        {
            SetPosY(ui.transform as RectTransform, y);
        }

        public static void SetPosX(this RectTransform RT, float x)
        {
            float xmax = RT.GetParent().rect.xMax;
            float center = RT.anchorMax.x - RT.anchorMin.x;
            float anchorFactor = RT.anchorMax.x * 2 - 1;

            RT.anchoredPosition = new Vector2(0 - (xmax * anchorFactor) + x + (center * xmax), RT.anchoredPosition.y);
        }

        public static void SetPosY(this RectTransform RT, float y)
        {
            float ymax = RT.GetParent().rect.yMax;
            float center = RT.anchorMax.y - RT.anchorMin.y;
            float anchorFactor = RT.anchorMax.y * 2 - 1;

            RT.anchoredPosition = new Vector2(RT.anchoredPosition.x, 0 - (ymax * anchorFactor) + y + (center * ymax));
        }

        //These four functions actually return the center of the edge mentioned, so
        // GetLeft gives you the center-left point, etc.  

        public static Vector2 GetLeft(this UIBehaviour ui)
        {
            return GetLeft(ui.transform as RectTransform);
        }

        public static Vector2 GetRight(this UIBehaviour ui)
        {
            return GetRight(ui.transform as RectTransform);
        }

        public static Vector2 GetTop(this UIBehaviour ui)
        {
            return GetTop(ui.transform as RectTransform);
        }

        public static Vector2 GetBottom(this UIBehaviour ui)
        {
            return GetBottom(ui.transform as RectTransform);
        }

        public static Vector2 GetLeft(this RectTransform RT)
        {
            return new Vector2(RT.offsetMin.x, RT.anchoredPosition.y);
        }

        public static Vector2 GetRight(this RectTransform RT)
        {
            return new Vector2(RT.offsetMax.x, RT.anchoredPosition.y);
        }

        public static Vector2 GetTop(this RectTransform RT)
        {
            return new Vector2(RT.anchoredPosition.x, RT.offsetMax.y);
        }

        public static Vector2 GetBottom(this RectTransform RT)
        {
            return new Vector2(RT.anchoredPosition.x, RT.offsetMin.y);
        }

        //Same as setting the "Left" etc variables in the inspector.  Unlike the inspector, these
        // can be used regardless of anchor position.  Be warned, there's a reason the functionality
        // is hidden in the editor, as the behavior is unintuitive when adjusting the parent's rect.
        // If you're calling these every frame or otherwise updating frequently, shouldn't be a problem, though.

        public static void SetLeftEdge(this UIBehaviour ui, float left)
        {
            SetLeftEdge(ui.transform as RectTransform, left);
        }

        public static void SetRightEdge(this UIBehaviour ui, float right)
        {
            SetRightEdge(ui.transform as RectTransform, right);
        }

        public static void SetTopEdge(this UIBehaviour ui, float top)
        {
            SetTopEdge(ui.transform as RectTransform, top);
        }

        public static void SetBottomEdge(this UIBehaviour ui, float bottom)
        {
            SetBottomEdge(ui.transform as RectTransform, bottom);
        }

        public static void SetLeftEdge(this RectTransform RT, float left)
        {
            float xmin = RT.GetParent().rect.xMin;
            float anchorFactor = RT.anchorMin.x * 2 - 1;

            RT.offsetMin = new Vector2(xmin + (xmin * anchorFactor) + left, RT.offsetMin.y);
        }

        public static void SetRightEdge(this RectTransform RT, float right)
        {
            float xmax = RT.GetParent().rect.xMax;
            float anchorFactor = RT.anchorMax.x * 2 - 1;

            RT.offsetMax = new Vector2(xmax - (xmax * anchorFactor) - right, RT.offsetMax.y);
        }

        public static void SetTopEdge(this RectTransform RT, float top)
        {
            float ymax = RT.GetParent().rect.yMax;
            float anchorFactor = RT.anchorMax.y * 2 - 1;

            RT.offsetMax = new Vector2(RT.offsetMax.x, ymax - (ymax * anchorFactor) - top);
        }

        public static void SetBottomEdge(this RectTransform RT, float bottom)
        {
            float ymin = RT.GetParent().rect.yMin;
            float anchorFactor = RT.anchorMin.y * 2 - 1;

            RT.offsetMin = new Vector2(RT.offsetMin.x, ymin + (ymin * anchorFactor) + bottom);
        }

        //Moves the edge to the requested position relative to the current position.  
        // NOTE:  using these functions repeatedly will result in unintuitive
        // behavior, since the anchored position is getting changed with each call.  
        public static void SetRelativeLeft(this UIBehaviour ui, float left)
        {
            SetRelativeLeft(ui.transform as RectTransform, left);
        }

        public static void SetRelativeRight(this UIBehaviour ui, float right)
        {
            SetRelativeRight(ui.transform as RectTransform, right);
        }

        public static void SetRelativeTop(this UIBehaviour ui, float top)
        {
            SetRelativeTop(ui.transform as RectTransform, top);
        }

        public static void SetRelativeBottom(this UIBehaviour ui, float bottom)
        {
            SetRelativeBottom(ui.transform as RectTransform, bottom);
        }

        public static void SetRelativeLeft(this RectTransform RT, float left)
        {
            RT.offsetMin = new Vector2(RT.anchoredPosition.x + left, RT.offsetMin.y);
        }

        public static void SetRelativeRight(this RectTransform RT, float right)
        {
            RT.offsetMax = new Vector2(RT.anchoredPosition.x + right, RT.offsetMax.y);
        }

        public static void SetRelativeTop(this RectTransform RT, float top)
        {
            RT.offsetMax = new Vector2(RT.offsetMax.x, RT.anchoredPosition.y + top);
        }

        public static void SetRelativeBottom(this RectTransform RT, float bottom)
        {
            RT.offsetMin = new Vector2(RT.offsetMin.x, RT.anchoredPosition.y + bottom);
        }

        //Sets the position of the RectTransform relative to the parent's Left etc side,
        // regardless of anchor setting.  

        public static void MoveLeftFromParent(this UIBehaviour ui, float left = 0)
        {
            MoveLeftFromParent(ui.transform as RectTransform, left);
        }

        public static void MoveRightFromParent(this UIBehaviour ui, float right = 0)
        {
            MoveRightFromParent(ui.transform as RectTransform, right);
        }

        public static void MoveTopFromParent(this UIBehaviour ui, float top = 0)
        {
            MoveTopFromParent(ui.transform as RectTransform, top);
        }

        public static void MoveBottomFromParent(this UIBehaviour ui, float bottom = 0)
        {
            MoveBottomFromParent(ui.transform as RectTransform, bottom);
        }

        public static void MoveLeftFromParent(this RectTransform RT, float left = 0)
        {
            float xmin = RT.GetParent().rect.xMin;
            float center = RT.anchorMax.x - RT.anchorMin.x;
            float anchorFactor = RT.anchorMax.x * 2 - 1;

            RT.anchoredPosition = new Vector2(xmin + (xmin * anchorFactor) + left - (center * xmin), RT.anchoredPosition.y);
        }

        public static void MoveRightFromParent(this RectTransform RT, float right = 0)
        {
            float xmax = RT.GetParent().rect.xMax;
            float center = RT.anchorMax.x - RT.anchorMin.x;
            float anchorFactor = RT.anchorMax.x * 2 - 1;

            RT.anchoredPosition = new Vector2(xmax - (xmax * anchorFactor) - right + (center * xmax), RT.anchoredPosition.y);
        }

        public static void MoveTopFromParent(this RectTransform RT, float top = 0)
        {
            float ymax = RT.GetParent().rect.yMax;
            float center = RT.anchorMax.y - RT.anchorMin.y;
            float anchorFactor = RT.anchorMax.y * 2 - 1;

            RT.anchoredPosition = new Vector2(RT.anchoredPosition.x, ymax - (ymax * anchorFactor) - top + (center * ymax));
        }

        public static void MoveBottomFromParent(this RectTransform RT, float bottom = 0)
        {
            float ymin = RT.GetParent().rect.yMin;
            float center = RT.anchorMax.y - RT.anchorMin.y;
            float anchorFactor = RT.anchorMax.y * 2 - 1;

            RT.anchoredPosition = new Vector2(RT.anchoredPosition.x, ymin + (ymin * anchorFactor) + bottom - (center * ymin));
        }

        //Moves the RectTransform to align the child left edge with the parent left edge, etc.  

        public static void MoveLeftEdgeFromParent(this UIBehaviour ui, float left = 0)
        {
            MoveLeftEdgeFromParent(ui.transform as RectTransform, left);
        }

        public static void MoveRightEdgeFromParent(this UIBehaviour ui, float right = 0)
        {
            MoveRightEdgeFromParent(ui.transform as RectTransform, right);
        }

        public static void MoveTopEdgeFromParent(this UIBehaviour ui, float top = 0)
        {
            MoveTopEdgeFromParent(ui.transform as RectTransform, top);
        }

        public static void MoveBottomEdgeFromParent(this UIBehaviour ui, float bottom = 0)
        {
            MoveBottomEdgeFromParent(ui.transform as RectTransform, bottom);
        }

        public static void MoveLeftEdgeFromParent(this RectTransform RT, float left = 0)
        {
            RT.MoveLeftFromParent(left + RT.GetWidth() / 2);
        }

        public static void MoveRightEdgeFromParent(this RectTransform RT, float right = 0)
        {
            RT.MoveRightFromParent(right + RT.GetWidth() / 2);
        }

        public static void MoveTopEdgeFromParent(this RectTransform RT, float top = 0)
        {
            RT.MoveTopFromParent(top + RT.GetHeight() / 2);
        }

        public static void MoveBottomEdgeFromParent(this RectTransform RT, float bottom = 0)
        {
            RT.MoveBottomFromParent(bottom + RT.GetHeight() / 2);
        }
    }
}