using UnityEngine;
using Unity.Barracuda;

public class RunML : MonoBehaviour
{
    public NNModel model;
    private Model mRuntimeModel;
    private IWorker mWorker;

    void Start()
    {
        mRuntimeModel = ModelLoader.Load(model);
        mWorker = WorkerFactory.CreateWorker(mRuntimeModel);
    }

    void Update()
    {
        // Tensor input = new Tensor([], height, width, channels);
        // m_Worker.Execute(input);
        // Tensor O = m_Worker.PeekOutput("output_layer_name");
        // input.Dispose();
    }



}