using UnityEngine;
using UnityEngine.UIElements;

namespace GravityGame.UI
{
    /// <summary>
    ///     UI Element that draws the radial menu with an inner up-down circle and outer up-down-left-right circle
    ///     Used for 6-direction gravity selection
    /// </summary>
    [UxmlElement]
    public partial class GravityDirectionRadialMenu : VisualElement
    {
        [UxmlAttribute] float _deadZoneRadius = 45f;
        [UxmlAttribute] float _innerRadius = 300;
        [UxmlAttribute] float _horizontalAngle = 58.82f;
        [UxmlAttribute] Color _outlineColor = Color.white;
        [UxmlAttribute] float _outlineWidth = 4f;

        [UxmlAttribute] float _upWidth = 100f;
        [UxmlAttribute] float _upLength = 350f;


        public GravityDirectionRadialMenu()
        {
            generateVisualContent += DrawRadialMenu;
        }

        public enum Zone { None, Left, Right, Up, Down, OuterUp, OuterDown }

        public Zone GetDirection(Vector2 mouse)
        {
            mouse /= scaledPixelsPerPoint;
            float distance = mouse.magnitude;
            if (distance < _deadZoneRadius) return Zone.None;

            float angle = Vector3.Angle(mouse, Vector3.up);
            float alpha = _horizontalAngle * 0.5f;
            if (angle > 90 - alpha && angle < 90 + alpha) return mouse.x > 0 ? Zone.Right : Zone.Left;

            var right = LowerUpSection(alpha);
            var left = LowerUpSection(alpha+180f);
            var up = new Vector2(0, _upLength);
            var down = new Vector2(0, -_upLength);
            if (PointInTriangle(mouse, right, left, up)) return Zone.Up;
            if (PointInTriangle(mouse, right, left, down)) return Zone.Down;
            
            if (mouse.y > 0) return Zone.OuterUp;
            return Zone.OuterDown;
            
            
            
            static bool PointInTriangle(Vector2 pt, Vector2 a, Vector2 b, Vector2 c)
            {
                float d1 = Sign(pt, a, b);
                float d2 = Sign(pt, b, c);
                float d3 = Sign(pt, c, a);
                return (d1 >= 0 && d2 >= 0 && d3 >= 0) || (d1 <= 0 && d2 <= 0 && d3 <= 0);
                
                static float Sign(Vector2 p1, Vector2 p2, Vector2 p3) => 
                    (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
            }
        }

        void DrawRadialMenu(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;
            painter.lineWidth = _outlineWidth;

            painter.strokeColor = _outlineColor;

            float alpha = _horizontalAngle * 0.5f;
            float[] angles = { 180f + alpha, 360f - alpha, alpha, 180f - alpha };

            var upLeft = AngleToVector(angles[0]);
            var upRight = AngleToVector(angles[1]);
            var downLeft = AngleToVector(angles[2]);
            var downRight = AngleToVector(angles[3]);
            painter.BeginPath();
            painter.MoveTo(LowerUpSection(angles[0]));
            painter.LineTo(new Vector2(0, -_upLength));
            painter.LineTo(LowerUpSection(angles[1]));
            painter.Stroke();


            painter.BeginPath();
            painter.MoveTo(LowerUpSection(angles[2]));
            painter.LineTo(new Vector2(0, _upLength));
            painter.LineTo(LowerUpSection(angles[3]));
            painter.Stroke();

            // // Upper Arc
            // painter.BeginPath();
            // painter.Arc(Vector2.zero, _innerRadius, angles[0], angles[1]);
            // painter.Stroke();
            //
            // // Lower Arc
            // painter.BeginPath();
            // painter.Arc(Vector2.zero, _innerRadius, angles[2], angles[3]);
            // painter.Stroke();

            // Draw major lines
            foreach (float angle in angles) {
                const float outerRadius = 2000f;
                painter.BeginPath();
                var direction = AngleToVector(angle);
                painter.MoveTo(direction * _deadZoneRadius);
                painter.LineTo(direction * outerRadius);
                painter.Stroke();
            }

            // Dead zone
            painter.BeginPath();
            painter.Arc(Vector2.zero, _deadZoneRadius, 0, 360);
            painter.Stroke();
        }
        
        Vector2 LowerUpSection(float angle)
        {
            var v = AngleToVector(angle);
            return v / Mathf.Abs(v.x) * _upWidth;
        }

        static Vector2 AngleToVector(float angleDegrees)
        {
            float rad = Mathf.Deg2Rad * angleDegrees;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}