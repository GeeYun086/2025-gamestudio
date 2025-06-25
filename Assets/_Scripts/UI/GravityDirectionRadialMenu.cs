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

        public float DeadZoneRadius => _deadZoneRadius * scaledPixelsPerPoint;
        public float InnerRadius => _innerRadius * scaledPixelsPerPoint;
        public float HorizontalAngle => _horizontalAngle;

        public GravityDirectionRadialMenu()
        {
            generateVisualContent += DrawRadialMenu;
        }

        void DrawRadialMenu(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;
            painter.lineWidth = _outlineWidth;

            painter.strokeColor = _outlineColor;

            var alpha = _horizontalAngle * 0.5f;
            var angles = new[] { 180f + alpha, 360f - alpha, alpha, 180f - alpha};
            
            // Upper Arc
            painter.BeginPath();
            painter.Arc(Vector2.zero, _innerRadius, angles[0], angles[1]);
            painter.Stroke();

            // Lower Arc
            painter.BeginPath();
            painter.Arc(Vector2.zero, _innerRadius, angles[2], angles[3]);
            painter.Stroke();
            
            // Draw major lines
            foreach (var angle in angles) {
                const float outerRadius = 2000f;
                painter.BeginPath();
                var direction = AngleToVector(angle);
                painter.MoveTo(direction*_deadZoneRadius);
                painter.LineTo(direction*outerRadius);
                painter.Stroke();
            }
            
            // Dead zone
            painter.BeginPath();
            painter.Arc(Vector2.zero, _deadZoneRadius, 0, 360);
            painter.Stroke();
            
        }
        
        static Vector2 AngleToVector(float angleDegrees)
        {
            float rad = Mathf.Deg2Rad * angleDegrees;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}