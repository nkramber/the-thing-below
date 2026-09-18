namespace ScreenScaleProbe;

/// <summary>How the probe puts the frame of 1280 by 720 on the screen.</summary>
public enum FitMode
{
    /// <summary>
    /// The largest whole-number fit, centered, with black bars around it (D-568). A screen whose
    /// fit is fractional shows wide bars, because the frame takes the whole number below the fit.
    /// </summary>
    Whole,

    /// <summary>
    /// The fit of D-573. The probe scales the frame up past the screen by a whole number with the
    /// Nearest filter, and then it scales that picture down to the screen with a linear filter.
    /// </summary>
    Fill,
}
