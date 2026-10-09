using System;

namespace Bootleg.Ball
{
    /// <summary>
    /// Abilities publish the interception risk of the action under the cursor here; UI such as
    /// <c>InterceptionRiskLabel</c> listens. Keeps abilities free of UI references.
    /// </summary>
    public static class InterceptionRiskPreview
    {
        public static event Action<InterceptionRisk> Shown;
        public static event Action Hidden;

        /// <summary>Shows the risk, or hides the preview if no defender can intercept.</summary>
        public static void Show(InterceptionRisk risk)
        {
            if (risk.Defenders == 0)
            {
                Hide();
                return;
            }
            Shown?.Invoke(risk);
        }

        public static void Hide()
        {
            Hidden?.Invoke();
        }
    }
}
