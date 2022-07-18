using SerpentCore.Core.Helpers;
using UnityEngine;
using UnityEngine.XR;

namespace SerpentCore.Core.Managers
{
    internal static class InputManager
    {
        private static Vector2 _mouseAxis;
        public static Vector2 LeftInput
        {
            get
            {
                if (XRDevice.isPresent) 
                    return new Vector2(Input.GetAxis(Constants.LEFT_HORIZONTAL), Input.GetAxis(Constants.LEFT_VERTICAL)) * 16;
                _mouseAxis.x = Mathf.Clamp(_mouseAxis.x+Input.GetAxis("Mouse X"), -16f, 16f);
                _mouseAxis.y = Mathf.Clamp(_mouseAxis.y+Input.GetAxis("Mouse Y"), -16f, 16f);
                var translatedHit = Utilities.GetIntersection(_mouseAxis.x, _mouseAxis.y, Mathf.Max(Mathf.Abs(_mouseAxis.x),Mathf.Abs(_mouseAxis.y)));
                if (translatedHit.x1 > 0 && _mouseAxis.x > 0)
                    return new Vector2((float)translatedHit.x1, (float)translatedHit.y1);
                return new Vector2((float)translatedHit.x2, (float)translatedHit.y2);;
            }
        }
        public static Vector2 RightInput
        {
            get
            {
                if (XRDevice.isPresent) 
                    return new Vector2(Input.GetAxis(Constants.RIGHT_HORIZONTAL), Input.GetAxis(Constants.RIGHT_VERTICAL)) * 16;
                _mouseAxis.x = Mathf.Clamp(_mouseAxis.x+Input.GetAxis("Mouse X"), -16f, 16f);
                _mouseAxis.y = Mathf.Clamp(_mouseAxis.y+Input.GetAxis("Mouse Y"), -16f, 16f);
                var translatedHit = Utilities.GetIntersection(_mouseAxis.x, _mouseAxis.y, Mathf.Max(Mathf.Abs(_mouseAxis.x),Mathf.Abs(_mouseAxis.y)));
                if (translatedHit.x1 > 0 && _mouseAxis.x > 0)
                    return new Vector2((float)translatedHit.x1, (float)translatedHit.y1);
                return new Vector2((float)translatedHit.x2, (float)translatedHit.y2);;
            }
        }

        public static void ResetMousePos()
        {
            _mouseAxis.x = 0f;
            _mouseAxis.y = 0f;
        }
        
    }
}