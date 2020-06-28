using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.Barracuda;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EmotionDetect : MonoBehaviour
{
    public NNModel Model;
    public RenderTexture CamTexture;
    private IWorker m_Worker;
    public float Angry = 0;
    public float Disgusted = 0;
    public float Fearful = 0;
    public float Happy = 0;
    public float Neutral = 0;
    public float Sad = 0;
    public float Surprised = 0;
    public float Contempt = 0;
    public Texture2D TestImage;
    private Texture2D CamTextureRead;

    private bool m_IsWorking = false;


    public void Start()
    {
        // yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        // if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        // {
        //     webCamTexture = new WebCamTexture(64, 64);
        //     webCamTexture.Play();
        // }
        var model = ModelLoader.Load(Model);
        m_Worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputeRef, model);

        CamTextureRead = new Texture2D(64, 64, TextureFormat.RGBAFloat, false);
    }

    public void Update()
    {
        Detect();
    }

    public void Detect()
    {
        m_IsWorking = true;
        RenderTexture.active = CamTexture;
        Rect rect = new Rect(0, 0, 64, 64);
        CamTextureRead.ReadPixels(rect, 0, 0, false);
        CamTextureRead.Apply();
        var pixels = CamTextureRead.GetPixels();
        var pixels2 = new float[64 * 64];
        // float sum = 0;
        // float min = 90000;
        // float max = 0;
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels2[pixels.Length - i - 1] = Mathf.Clamp(4f * pixels[i].grayscale + 0.5f, 0f, 1f) * 256f;
        }
        // for (int i = 0; i < pixels.Length; i++)
        // {
        //     float val = pixels2[i];
        //     sum += val;
        //     min = val < min ? val : min;
        //     max = val > max ? val : max;
        // }
        // float avg = sum / (64 * 64);
        // Debug.Log(avg);
        // Debug.Log(max - min);

        // Debug.Log(pixels2.Length);
        var input = new Tensor(1, 64, 64, 1, pixels2);
        // TextureAsTensorData tatd = new TextureAsTensorData(new RenderTexture[] { CamTexture }, 1, TextureAsTensorData.Flip.None);
        // var input = new Tensor(tatd.shape, tatd);
        // var input = tatd.Download();
        // Debug.Log("input");
        // for (int i = 0; i < 64 * 64; i++)
        // {
        //     Debug.Log(input[i]);
        // }
        m_Worker.Execute(input);

        //  emotion_table = {'neutral':0, 'happiness':1, 'surprise':2, 'sadness':3, 'anger':4, 'disgust':5, 'fear':6, 'contempt':7}
        Tensor outputTensor = m_Worker.PeekOutput();
        float[] tensorValues = outputTensor.ToReadOnlyArray();
        float[] softmaxed = Softmax(tensorValues);
        Neutral = softmaxed[0];
        Happy = softmaxed[1];
        Surprised = softmaxed[2];
        Sad = softmaxed[3];
        Angry = softmaxed[4];
        Disgusted = softmaxed[5];
        Fearful = softmaxed[6];
        Contempt = softmaxed[7];
        input.Dispose();
        m_IsWorking = false;
    }


    private float[] Softmax(float[] values)
    {
        var maxVal = values.Max();
        var exp = values.Select(v => Mathf.Exp(v - maxVal));
        var sumExp = exp.Sum();

        return exp.Select(v => (float)(v / sumExp)).ToArray();
    }

}