using UnityEngine;
using UnityEngine.UI;

public class SkyboxController : MonoBehaviour
{
    public Material originalCubemap;
    public Material blurredCubemap;
    public ReflectionProbe reflectionProbe;
    public Text switchButtonText;

    private bool blurred = false;

    public void Switch()
    {
        blurred = !blurred;
        if (blurred)
        {
            RenderSettings.skybox = blurredCubemap;
            switchButtonText.text = "Blurred cubemap";
        }
        else
        {
            RenderSettings.skybox = originalCubemap;
            switchButtonText.text = "Original cubemap";
        }

        reflectionProbe.RenderProbe();
    }
}