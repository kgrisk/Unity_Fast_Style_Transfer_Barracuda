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
    public List <NNModel> modelAsset;
    private List <Model> m_RuntimeModel;
    private int styleNumber;
    List<IWorker> worker;
    public RawImage rawImage;
    int height;
    int width;
    // Start is called before the first frame update

    public RenderTexture basicView;
    private RenderTexture outputTexture;
    void Start()
    {
        worker = new List<IWorker>();
        m_RuntimeModel = new List<Model>();
        height = Screen.height/4;
        width = Screen.width/4;
        outputTexture = new RenderTexture(width, height, basicView.depth);
        basicView.height = height/2;
        basicView.width = width/2;
        styleNumber = -1;

        foreach(NNModel model in modelAsset)
        {
            Model temp = ModelLoader.Load(model);
            m_RuntimeModel.Add(temp);
            worker.Add(WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, temp));
        }
    }
    int i = -1;
    Coroutine cor;
    // Update is called once per frame
    void Update()
    {
        bool switchStyle = Input.GetKeyDown(KeyCode.LeftShift);
        if (switchStyle)
        {
            if(cor != null)
            {
                StopCoroutine(cor);
            }
            styleNumber++;

            if (styleNumber >= modelAsset.Count)
            {
                rawImage.enabled = false;
                styleNumber = -1;
            }
            else
            {
                if (styleNumber == 0)
                {
                    rawImage.enabled = true;
                    
                }
                cor = StartCoroutine(Classify());
            }
        }


        if (styleNumber > -1)
        {

            //Tensor tensor = new Tensor(basicView, 3);
            //worker[styleNumber].Execute(tensor);
            
            
            //var output = worker[styleNumber].PeekOutput();
            //output.ToReadOnlyArray();
            //output.ToRenderTexture(outputTexture, 0, 0, 1 / 255f, 0, null);
            //rawImage.texture = outputTexture;
            //output.Dispose();
        }
    }

    private IEnumerator Classify()
    {
        while (true)
        {
            Tensor tensor = new Tensor(basicView, 3);
            var enumerator = this.worker[styleNumber].StartManualSchedule(tensor);
            int skip_layers = 0;
            while (enumerator.MoveNext())
            {
                skip_layers++;
                if (skip_layers >= 20)
                {
                    skip_layers = 0;
                    yield return null;
                }
            };

            // this.worker.Execute(inputs);
            // Execute() scheduled async job on GPU, waiting till completion
            // yield return new WaitForSeconds(0.5f);

            var output = worker[styleNumber].PeekOutput();
            output.ToReadOnlyArray();
            output.ToRenderTexture(outputTexture, 0, 0, 1 / 255f, 0, null);
            rawImage.texture = outputTexture;
            output.Dispose();
            yield return null;
        }

    }
}
