using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WombScript : MonoBehaviour {

    [SerializeField] string _weightFileName;
    [SerializeField] Texture DefaultTexture;
    [SerializeField] UnityEngine.UI.RawImage SourceUI;
    [SerializeField] UnityEngine.UI.RawImage ResultUI;

    RenderTexture InputTexture;
    RenderTexture ResultTexture;

    #region Pix2Pix implementation

    Dictionary<string, Pix2Pix.Tensor> _weightTable;
    Pix2Pix.Generator _generator;

    float _budget = 100;
    float _budgetAdjust = 10;

    readonly string[] _performanceLabels = {
        "N/A", "Poor", "Moderate", "Good", "Great", "Excellent"
    };

    void InitializePix2Pix()
    {
        var filePath = System.IO.Path.Combine(Application.streamingAssetsPath, _weightFileName);
        _weightTable = Pix2Pix.WeightReader.ReadFromFile(filePath);
        _generator = new Pix2Pix.Generator(_weightTable);
    }

    void FinalizePix2Pix()
    {
        _generator.Dispose();
        Pix2Pix.WeightReader.DisposeTable(_weightTable);
    }

    void UpdatePix2Pix()
    {
        // Advance the Pix2Pix inference until the current budget runs out.
        for (var cost = 0.0f; cost < _budget;)
        {
            if (!_generator.Running) _generator.Start(InputTexture);

            cost += _generator.Step();

            if (!_generator.Running) _generator.GetResult(ResultTexture);
        }

        Pix2Pix.GpuBackend.ExecuteAndClearCommandBuffer();

        // Review the budget depending on the current frame time.
        _budget -= (Time.deltaTime * 60 - 1.25f) * _budgetAdjust;
        _budget = Mathf.Clamp(_budget, 150, 1200);

        _budgetAdjust = Mathf.Max(_budgetAdjust - 0.05f, 0.5f);

        /*
        // Update the text display.
        var rate = 60 * _budget / 1000;

        var perf = (_budgetAdjust < 1) ?
            _performanceLabels[(int)Mathf.Min(5, _budget / 100)] :
            "Measuring GPU performance...";

        _textUI.text =
            string.Format("Pix2Pix refresh rate: {0:F1} Hz ({1})", rate, perf);
        */
    }

    #endregion

    #region MonoBehaviour implementation

    // Use this for initialization
    void Start () {
        ResultTexture = new RenderTexture(256, 256, 0);
        ResultTexture.enableRandomWrite = true;
        ResultTexture.Create();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    #endregion

}
