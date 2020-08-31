using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public NNModel modelAsset;
    private Model m_RuntimeModel;
    IWorker worker;
    public RawImage canvas;
    public Material material;
    int height;
    int width;
    // Start is called before the first frame update

    public RenderTexture basicView;
    void Start()
    {
        height = Screen.height;
        width = Screen.width;
        
        m_RuntimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, m_RuntimeModel);
    }

    // Update is called once per frame
    [Obsolete]
    void Update()
    {
        GetComponent<Transform>().position = new Vector3(GetComponent<Transform>().position.x, GetComponent<Transform>().position.y, 0);
        float vertical = Input.GetAxis("Horizontal");
        bool jump = Input.GetButtonDown("Jump");
        if(vertical != 0)
            GetComponent<Rigidbody>().velocity = new Vector3(vertical*10, GetComponent<Rigidbody>().velocity.y, 0);
        if (jump)
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(0, 10, 0));
        }
        if (!jump)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Tensor tensor = new Tensor(basicView, 3);
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            texture.Resize(256, 256);
            Color32[] color = texture.GetPixels32();
            width = height = 256;
            Tensor tensoro = TransformInput(color, width, height);
            worker.Execute(tensoro);
            var output = worker.PeekOutput();
            material.mainTexture = BarracudaTextureUtils.TensorToRenderTexture(output);
            canvas.texture = material.mainTexture;
            tensor.Dispose();
            output.Dispose();
            stopwatch.Stop();
            UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds);
        }
    }

        [SerializeField] private int _screenshotTextureW = 1280, _screenshotTextureH = 720;

        public Texture2D ScreenshotTexture { get; private set; }

        private void Awake()
        {
            ScreenshotTexture = new Texture2D(_screenshotTextureW, _screenshotTextureH, TextureFormat.RGB24, false);
        }
    


    public const int IMAGE_SIZE = 224;
    private const int IMAGE_MEAN = 127;
    private const float IMAGE_STD = 127.5f;
    private Tensor TransformInput(Color32[] pic, int v1, int v2)
    {
        float[] floatValues = new float[width * height * 3];

        for (int i = 0; i < pic.Length; ++i)
        {
            var color = pic[i];

            floatValues[i * 3 + 0] = (color.r - IMAGE_MEAN) / IMAGE_STD;
            floatValues[i * 3 + 1] = (color.g - IMAGE_MEAN) / IMAGE_STD;
            floatValues[i * 3 + 2] = (color.b - IMAGE_MEAN) / IMAGE_STD;
        }

        return new Tensor(1, height, width, 3, floatValues);
    
    }
}
